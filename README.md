# Vantage OS - Core Engine

> Introducing the brand new Vantage OS, featuring an upgraded native interface and advanced telemetry tools. The next major update is just around the corner!

**Author:** **Alyson** · [github.com/Alyson256](https://github.com/Alyson256) 
**License:** MIT License | Licença MIT  
**Language:**  [PT-BR](./src/assets/docs/pt-br.md)  



## General

Vantage OS is not just a cleaning script. It is a low-level telemetry and optimization dashboard featuring a next-generation native interface, designed to monitor hardware in real-time (CPU, GPU, DPC Latency) and apply surgical optimizations without system overhead.

*Note: The UI/UX foundation was initially accelerated utilizing AI tools for rapid prototyping. The project has since transitioned to a fully native C#/.NET architecture, ensuring a premium visual experience with absolute focus on the core engine's performance and low-level system integrations.*

## Key Features
- **Real-Time Telemetry:** Monitor usage, temperature, and power consumption.
- **Latency Analysis:** Track DPC and ISR to ensure zero FPS drops in real-time tasks.
- **One-Click Optimization:** Safely clean RAM cache and system junk.
- **Native Modern UI/UX:** Clean, performance-focused desktop design built with XAML, featuring native Dark Mode support.

## Project Status: Active Development
**Current Phase:** Native UI/UX Implementation & Low-Level API Integration

The visual foundation and core engine have successfully migrated to a C#/.NET WPF architecture. Current focus is on expanding native system controls and refining the MVVM structure.

### Recent Updates (Native-WPF Branch):
- **Safe Native Optimizations:** Implemented RAM cleaning (via `EmptyWorkingSet` P/Invoke) and Junk Files removal directly through safe Windows APIs, avoiding third-party driver dependencies or kernel risks.
- **Real-Time Localization (i18n):** Added a dynamic language switching system (PT/EN) directly linked to the MVVM layer, updating the UI instantly without reloads.
- **Low-Overhead Telemetry:** Integrated `WMI` for static hardware polling (run-once) and safe `P/Invoke` (e.g., `GlobalMemoryStatusEx`) for high-frequency metrics, guaranteeing near-zero DPC Latency generation by the app itself.
- **Native MVVM Styling:** Successfully ported web-based Tailwind design tokens into native XAML ResourceDictionaries, keeping the premium visual aesthetic while maximizing performance.

- **Up Next:** Building the interactive real-time area charts for `HardwareMonitor` and migrating the Registry engine for the `Custom Tweaks` section.
