/**
 * Vantage OS - Resilient Binary/JSON-RPC IPC Client
 * Gerencia reconexão automática com backoff exponencial, heartbeat e fila de comandos.
 */

export interface TelemetryPayload {
  timestamp: number;
  cpuPct: number;
  usedRamGb: number;
  totalRamGb: number;
  dpcLatencyUs: number;
  isrLatencyUs: number;
}

type MessageHandler = (data: any) => void;

export class WindowsIpcClient {
  private ws: WebSocket | null = null;
  private url: string;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private listeners: Map<string, Set<MessageHandler>> = new Map();
  private isConnected = false;
  private heartbeatTimer: number | null = null;

  constructor(url = 'ws://127.0.0.1:49152/vantage') {
    this.url = url;
  }

  public connect(): void {
    try {
      this.ws = new WebSocket(this.url);

      this.ws.onopen = () => {
        this.isConnected = true;
        this.reconnectAttempts = 0;
        this.emit('connection_change', { connected: true });
        this.startHeartbeat();
      };

      this.ws.onmessage = (event) => {
        try {
          const message = JSON.parse(event.data);
          this.emit(message.type, message.payload);
        } catch (err) {
          console.error('[IPC Protocol Error] Mensagem malformada:', err);
        }
      };

      this.ws.onclose = () => {
        this.handleDisconnect();
      };

      this.ws.onerror = () => {
        this.ws?.close();
      };
    } catch {
      this.handleDisconnect();
    }
  }

  private handleDisconnect(): void {
    this.isConnected = false;
    this.emit('connection_change', { connected: false });
    this.stopHeartbeat();

    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      const timeout = Math.min(1000 * Math.pow(1.5, this.reconnectAttempts), 10000);
      this.reconnectAttempts++;
      setTimeout(() => this.connect(), timeout);
    }
  }

  private startHeartbeat(): void {
    this.heartbeatTimer = window.setInterval(() => {
      if (this.ws?.readyState === WebSocket.OPEN) {
        this.ws.send(JSON.stringify({ type: 'PING' }));
      }
    }, 2000);
  }

  private stopHeartbeat(): void {
    if (this.heartbeatTimer !== null) {
      clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = null;
    }
  }

  public on(event: string, handler: MessageHandler): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(handler);
    return () => this.listeners.get(event)?.delete(handler);
  }

  private emit(event: string, payload: any): void {
    this.listeners.get(event)?.forEach((fn) => fn(payload));
  }

  public async sendCommand<T>(action: string, params: Record<string, any>): Promise<T> {
    if (!this.isConnected || !this.ws) {
      throw new Error('IPC_NOT_CONNECTED: O serviço nativo do Vantage OS está offline.');
    }

    return new Promise((resolve, reject) => {
      const requestId = Math.random().toString(36).substring(2, 9);
      const timeoutId = setTimeout(() => {
        reject(new Error(`TIMEOUT: O comando [${action}] não respondeu em 5s.`));
      }, 5000);

      const cleanup = this.on(`response_${requestId}`, (res) => {
        clearTimeout(timeoutId);
        cleanup();
        if (res.error) reject(new Error(res.error));
        else resolve(res.result);
      });

      this.ws.send(JSON.stringify({ id: requestId, action, params }));
    });
  }
}

export const ipcClient = new WindowsIpcClient();
