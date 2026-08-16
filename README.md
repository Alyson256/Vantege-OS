# Vantage OS - Core Engine

> Introducing the brand new Vantage OS, featuring an upgraded interface and advanced tools. The next major update is just around the corner!

**Author:** **Alyson** · [github.com/Alyson256](https://github.com/Alyson256) 
**License:** MIT License | Licença MIT  
**Language:**  [PT-BR](./assets/docs/pt-br.md)  

---

![Vantage OS Dashboard](./assets/dashboard.png)

## The Vision
Vantage OS is not just a cleaning script. It is a low-level telemetry and optimization dashboard featuring a next-generation interface, designed to monitor hardware in real-time (CPU, GPU, DPC Latency) and apply surgical optimizations without system overhead.

*Note: The UI/UX foundation and component architecture were accelerated and structured utilizing AI tools. This approach allowed for rapid prototyping, ensuring a premium visual experience while maintaining absolute focus on the core engine's performance and low-level integrations.*

## Key Features
- **Real-Time Telemetry:** Monitor usage, temperature, and power consumption.
- **Latency Analysis:** Track DPC and ISR to ensure zero FPS drops in real-time tasks.
- **One-Click Optimization:** Safely clean RAM cache and system junk.
- **Modern UI/UX:** Clean, performance-focused desktop design with native Dark Mode support.

## Project Status: Active Development
**Current Phase:** UI/UX Refactoring & Backend Integration

The visual foundation is deployed, and I am currently running a sprint focused on stability and WebSocket integration to bind the React frontend with the C/Python hardware telemetry backend.

- **Hotfixes:** Resolving React component lifecycle issues, reorganizing component architecture, and isolating data states.
- **Code Cleanup:** Stripping redundant mobile-responsive CSS classes to ensure a strictly lightweight desktop experience.
- **Up Next:** Building the local Python/C core engine to feed the `ws://localhost:3000` telemetry stream.