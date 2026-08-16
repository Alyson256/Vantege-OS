/**
 * Hook com Gerenciamento de Estado Otimizado para React 19
 * Utiliza o padrão Command para garantir Rollback (undo_all.bat(coisa assim)) de modificações no Windows.
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { ipcClient, TelemetryPayload } from '../lib/ipc-client';

export interface CommandMemento {
  tweakId: string;
  previousValue: any;
  appliedValue: any;
  timestamp: Date;
}

export function useSystemOptimizer() {
  const [telemetry, setTelemetry] = useState<TelemetryPayload>({
    timestamp: 0,
    cpuPct: 0,
    usedRamGb: 0,
    totalRamGb: 0,
    dpcLatencyUs: 0,
    isrLatencyUs: 0,
  });
  const [isBackendConnected, setIsBackendConnected] = useState(false);
  const [isExecuting, setIsExecuting] = useState(false);
  const rollbackStack = useRef<CommandMemento[]>([]);

  useEffect(() => {
    ipcClient.connect();

    const unsubConn = ipcClient.on('connection_change', ({ connected }) => {
      setIsBackendConnected(connected);
    });

    // Atualização de telemetria sem engasgar a thread de UI
    const unsubTelemetry = ipcClient.on('telemetry_tick', (data: TelemetryPayload) => {
      setTelemetry(data);
    });

    return () => {
      unsubConn();
      unsubTelemetry();
    };
  }, []);

  const applyTweak = useCallback(async (tweakId: string, params: any) => {
    setIsExecuting(true);
    try {
      const response = await ipcClient.sendCommand<{ originalState: any }>(
        'APPLY_TWEAK',
        { tweakId, ...params }
      );

      // Empilha o memento para rollback preciso
      rollbackStack.current.push({
        tweakId,
        previousValue: response.originalState,
        appliedValue: params,
        timestamp: new Date(),
      });

      return { success: true };
    } catch (err: any) {
      console.error(`Falha ao aplicar tweak ${tweakId}:`, err);
      return { success: false, error: err.message };
    } finally {
      setIsExecuting(false);
    }
  }, []);

  const rollbackLastAction = useCallback(async () => {
    const lastCommand = rollbackStack.current.pop();
    if (!lastCommand) return { success: false, message: 'Nenhuma ação para desfazer.' };

    setIsExecuting(true);
    try {
      await ipcClient.sendCommand('ROLLBACK_TWEAK', {
        tweakId: lastCommand.tweakId,
        restoreValue: lastCommand.previousValue,
      });
      return { success: true };
    } catch (err: any) {
      // Re-empilha em caso de falha para não perder histórico
      rollbackStack.current.push(lastCommand);
      return { success: false, error: err.message };
    } finally {
      setIsExecuting(false);
    }
  }, []);

  return {
    telemetry,
    isBackendConnected,
    isExecuting,
    applyTweak,
    rollbackLastAction,
    canUndo: rollbackStack.current.length > 0,
  };
}
