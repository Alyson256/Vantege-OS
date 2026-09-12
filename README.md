# Vantage OS - Core Engine

> Introducing the brand new Vantage OS, featuring an upgraded interface and advanced tools. The next major update is just around the corner!

**Author:** **Alyson** · [github.com/Alyson256](https://github.com/Alyson256) 
**License:** MIT License | Licença MIT  
**Language:**  [PT-BR](./src/assets/docs/pt-br.md)  

---

![Vantage OS Dashboard](./src/assets/dashboard.png)

# General

Vantage OS is not just a cleaning script. It is a low-level telemetry and optimization dashboard, designed to monitor hardware in real-time (CPU, GPU, DPC/ISR Latency) and apply surgical optimizations without adding system overhead of its own.

> Note: The UI/UX foundation was rapidly prototyped in React with the help of AI-assisted tools, which allowed fast iteration on the visual design and component layout. That prototype is now being ported to a native WPF/WinUI 3 (C#) implementation — a system optimizer has no business competing with the very resources it exists to free up, so the shipped app runs with zero browser engine and a minimal memory footprint.

Key Features
Real-Time Telemetry — Monitor CPU/GPU usage, temperature, and power consumption live.
Latency Analysis — Track DPC and ISR execution time to catch what's causing dropped frames or audio glitches during real-time tasks.
One-Click Optimization — Safely free cached RAM and clean system junk.
Modern, Native UI — Clean, performance-focused desktop design (WPF/WinUI 3) with native Dark Mode — no web rendering engine involved.
Tech Stack
UI: WPF / WinUI 3 (C#, .NET) — native, no browser engine
Core Engine: C# — WMI, Registry and ETW for real-time telemetry and system tweaks
Storage: SQLite — embedded, zero external dependencies
Prototyping: React — used only for early UI/UX exploration, not part of the shipped app
Project Status: Active Development

# Current Phase: Architecture Migration — React Prototype → Native WPF/WinUI 3

The visual design is validated, and I'm currently porting the interface from the original React prototype into a native XAML implementation, while building the core telemetry engine directly in C#. This removes the local server / WebSocket layer entirely — telemetry is read in-process, with no network hop between "frontend" and "backend."

In Progress: Porting UI components from the React prototype into native XAML views.
In Progress: Implementing direct WMI / Registry / ETW access for real-time telemetry, replacing the old ws://localhost:3000 dependency.
Up Next: Wiring a local SQLite database for usage profiles, custom tweaks, and feedback history.
Up Next: Automatic System Restore point creation before any batch optimization, as a safety net.
