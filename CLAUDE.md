# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Meta Quest 3 VR/AR game where players use a hammer to hit moles and score points (Whac-A-Mole style).

## Technology Stack

- **Unity 6.0** (6000.1.12f1)
- **Meta XR SDK** v78.0.0 for Quest 3 optimization
- **XR Interaction Toolkit** v3.1.2 for VR controller interactions
- **OpenXR** for cross-platform VR support
- **URP** (Universal Render Pipeline) for performance optimization

## Architecture Overview

### Core Game Systems

**Mole System** (`Assets/Scripts/Game/`):
- `Mole.cs`: Individual mole behavior with state machine (Hidden, Rising, Idle, Falling, Hit)
- `MoleManager.cs`: Spawns and manages mole groups (1-3 moles simultaneously) with progressive difficulty

**VR Interaction**:
- Hammer detection via "Hammer" tag on game objects
- Physics-based collision detection using Unity's Rigidbody system
- XR Controller input through Unity Input System

**Key Patterns**:
- Object pooling for mole instances to optimize memory allocation
- State-based design for mole behavior transitions
- Event-driven callbacks for game state changes

### Project Structure

```
Assets/
├── Scripts/
│   ├── Game/                    # Core game logic
│   │   ├── Mole.cs             # Individual mole behavior
│   │   └── MoleManager.cs      # Spawning and difficulty system
│   ├── Metaverse/Interactions/ # Additional VR interactions
│   └── TutorialInfo/           # Tutorial system
├── Scenes/
│   ├── Metaverse.unity        # Main scene
│   └── Game.unity             # Game-specific scene
├── XR/                        # XR configuration
├── MetaXR/                    # Meta Quest SDK
└── Rubber Play Hammer/         # VR hammer asset
```

## Development Workflow

### Building for Meta Quest 3
1. **Platform**: Switch to Android in Build Settings
2. **XR Setup**: Configure OpenXR with Meta OpenXR provider
3. **Build**: Create APK for device testing

### Key Development Settings
- **Input System**: Unity Input System (not legacy)
- **Scripting Backend**: IL2CPP for performance
- **Target Architecture**: ARM64
- **Graphics API**: Vulkan (recommended for Quest 3)

### Performance Considerations
- Maintain 90fps target for smooth VR experience
- Use object pooling to avoid garbage collection spikes
- Optimize draw calls with URP batching
- Test on actual Quest 3 device for validation

## Common Development Tasks

### Adding New Mole Types
1. Create new prefabs in `Assets/` based on existing mole structure
2. Update `Mole.cs` enum with new states if needed
3. Modify `MoleManager.cs` spawning logic
4. Configure collision layers and physics materials

### Modifying Game Difficulty
Edit difficulty progression in `MoleManager.cs`:
- Adjust spawn intervals
- Modify mole group sizes
- Change timing curves for mole movement

### Testing VR Interactions
1. Use Unity's XR Simulator for basic testing
2. Deploy to Quest 3 for proper controller testing
3. Verify hammer collision detection with debug logging

## Important Configuration

### XR Settings (Edit > Project Settings > XR Plug-in Management)
- **OpenXR**: Enabled with Meta OpenXR provider
- **Interaction Toolkit**: Configured for Quest controllers
- **Stereo Rendering**: Enabled for VR

### Input Actions
- Hammer trigger mapped to primary button
- Grip interactions for holding hammer
- Physics-based movement for natural feeling

## Code Style Guidelines

- Follow C# naming conventions (PascalCase for methods, camelCase for variables)
- Use `[SerializeField]` for inspector-exposed private fields
- Implement `IDisposable` for objects using Unity resources
- Use Coroutines for timed operations instead of Update loops
- Add `[Header]` attributes for organized Inspector layout

## Testing Strategy

- **Unit Tests**: Use Unity Test Framework for game logic
- **VR Testing**: Test on actual Quest 3 device (simulator insufficient)
- **Performance Profiling**: Monitor frame rate and memory usage
- **Collision Testing**: Verify hammer interactions work properly