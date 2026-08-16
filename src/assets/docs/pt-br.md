# Vantage OS - Core Engine

> Dashboard de telemetria de baixo nível e otimização para Windows 11 — seguro, documentado e totalmente reversível.

**Autor:** **Alyson** · [github.com/Alyson256](https://github.com/Alyson256)  
**Licença:** MIT License | Licença MIT  
**Documentação:** [EN](./assets/docs/en.md) | [PT-BR](./assets/docs/pt-br.md)  

---

![Vantage OS Dashboard](./assets/dashboard.png)

## A Visão
O Vantage OS transforma a clássica experiência de otimização via terminal (CLI) em uma aplicação desktop profissional. Não é apenas um script de limpeza, mas um motor de telemetria e otimização projetado para monitorar o hardware em tempo real (CPU, GPU, Latência DPC) e aplicar otimizações cirúrgicas sem causar impacto no desempenho do sistema.

*Nota: A fundação de UI/UX e a arquitetura de componentes foram aceleradas e estruturadas utilizando ferramentas de IA. Essa abordagem permitiu a prototipagem rápida, garantindo uma experiência visual premium enquanto mantém o foco absoluto na performance do motor central e nas integrações de baixo nível.*

## Princípios de Design
Cada otimização aplicada pelo Vantage OS segue regras rigorosas de segurança:
- **Seguro:** Modifica apenas serviços não essenciais e chaves de registro (*policies*).
- **Reversível:** Sistema integrado de Desfazer Global (Global Undo) e criação automática de Pontos de Restauração.
- **Transparente:** Toda alteração no sistema é registrada com data e hora no Histórico de Ações.
- **Documentado:** Detalhamento completo de riscos disponível na documentação.

## Recursos Principais
- **Telemetria em Tempo Real:** Monitoramento de hardware (Uso, Temperatura, Energia) alimentado por um fluxo local via WebSocket (`localhost:3000`).
- **Análise de Latência:** Rastreamento de rotinas DPC e ISR para garantir zero quedas de FPS em tarefas de tempo real.
- **Perfis Inteligentes:** Aplicação de configurações em múltiplas camadas com um único clique (Modo Gamer, Check-up & Limpeza).
- **Customização Granular:** Opções individuais para ajustes de Privacidade, Rede, Sistema e Interface.
- **Instalador de Apps:** Instalação silenciosa de pacotes e softwares essenciais.

## Arquitetura do Sistema

Em vez de um script monolítico, o Vantage OS separa a interface da camada de execução:

```text
vantage-os/
├── frontend/          ← React, Vite, TailwindCSS (Dashboard de UI/UX)
├── backend/           ← Motor Core em Python/C (Telemetria, WebSockets Locais)
├── scripts/           ← Scripts atômicos de otimização acionados pelo backend
└── assets/docs/       ← Documentação multilíngue e detalhamento de riscos