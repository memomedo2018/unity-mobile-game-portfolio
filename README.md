# Moha Saker — Unity Mobile Game Development

I build focused 3D mobile games and gameplay prototypes in Unity and C#. My work covers the full playable loop: controls, camera, physics, AI, UI, optimization, SDK integration, testing, and release support for iOS and Android.

> Commercial game source and production assets remain private. This repository contains project presentation material and small, standalone C# examples created specifically for this portfolio with AI assistance. The examples are not extracted from the shipped games and do not represent historical commits to those projects.

## Review the code

[![C# sample checks](https://github.com/memomedo2018/unity-mobile-game-portfolio/actions/workflows/samples.yml/badge.svg)](https://github.com/memomedo2018/unity-mobile-game-portfolio/actions/workflows/samples.yml)

Three focused examples demonstrate testable gameplay logic and clear integration boundaries:

| Example | What to inspect | Code |
| --- | --- | --- |
| Mobile joystick | Single-pointer ownership, radial dead zone, diagonal clamping, cancellation on focus loss | [MobileStick.cs](samples/Core/MobileStick.cs) · [Unity UI adapter](samples/Unity/TouchStickView.cs) |
| Enemy state machine | Idle/chase/attack transitions, separate enter/exit ranges, cooldowns without burst damage | [EnemyBrain.cs](samples/Core/EnemyBrain.cs) |
| Progress saving | Versioned format, validation, corruption detection, atomic replacement, last-good backup | [ProgressSave.cs](samples/Core/ProgressSave.cs) |

**Start here:** [integration guide and design notes](samples/README.md) · [23 executable behavior tests](tests/Program.cs) · [CI results](https://github.com/memomedo2018/unity-mobile-game-portfolio/actions/workflows/samples.yml)

Run the core tests with the .NET 8 SDK; no Unity editor or third-party test packages are required:

```sh
dotnet run --project tests/PortfolioSamples.Tests.csproj --configuration Release
```

CI runs the core tests on Windows and Linux. The Unity UI adapter is outside that .NET project and has its own integration notes; CI is not a device-playtest claim.

## Featured work

### ROADFORGE — shipped iOS game

![ROADFORGE gameplay](media/roadforge.png)

A mobile tank-combat game with vehicle controls, enemy encounters, progression, and mobile release workflows.

- Unity / C# 3D gameplay
- Mobile input and camera tuning
- UI, level flow, QA, and performance passes
- iOS release; Android release work in progress
- [Project page](https://uploadforsoftware.com/projects/roadforge/)

### Neon Moto Drift — racing prototype

![Neon Moto Drift gameplay](media/neon-moto.png)

An arcade motorcycle prototype exploring touch steering, traffic behavior, drift feedback, and checkpoints.

- Unity 6 prototype
- Touch-first control loop
- Vehicle camera and AI traffic experiments
- Fast iteration on feel and readability

### TNT Crash — physics prototype

![TNT Crash visual concept](media/tnt-crash.png)

*AI-assisted concept visualization for an in-progress project; not a gameplay recording.*

A compact destruction puzzle prototype centered on aiming, explosive chain reactions, and readable physics feedback.

- Unity 6 / C#
- Physics-driven interactions
- Level objectives and mobile HUD
- Modular prototype architecture

### Furniture Escape — room puzzle prototype

![Furniture Escape visual concept](media/furniture-escape.png)

*AI-assisted concept visualization for an in-progress project; not a gameplay recording.*

A top-down room puzzle prototype focused on movement planning, environmental interaction, and clear visual guidance.

- Unity 6 / C#
- Puzzle-state logic
- Touch-friendly interaction design
- Isometric camera and level readability

### Number Rush — hyper-casual prototype

![Number Rush visual concept](media/number-rush.png)

*AI-assisted concept visualization for an in-progress project; not a gameplay recording.*

A quick-session runner prototype that combines number gates, risk/reward choices, collectibles, and simple progression feedback.

- Unity 6 / C#
- Hyper-casual gameplay loop
- Gate and score systems
- Mobile UI and rapid iteration

## Capabilities

- **Gameplay:** C# systems, controls, cameras, physics, AI, level flow, rapid prototyping
- **Mobile:** iOS and Android builds, touch UI, optimization, device testing, release support
- **Production:** Git workflows, SDK integrations, debugging, QA, issue reproduction, documentation
- **3D content:** Blender asset preparation, lighting, scene assembly, and performance-aware art integration

## Source-code policy

Full game source and production assets are not published. The standalone examples above were written separately for review and contain no game assets, private project code, service credentials, or signing material. Everything in this public repository can be viewed and downloaded; private project directories have not been uploaded. I can discuss the examples and project architecture with prospective clients through Upwork.

## Contact

- Portfolio: [uploadforsoftware.com](https://uploadforsoftware.com)
- Upwork: [Moha Saker](https://www.upwork.com/freelancers/~0132997374ccadf132)

© 2026 Moha Saker. All rights reserved.
