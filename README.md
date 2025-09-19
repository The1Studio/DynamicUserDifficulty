# 🎮 Dynamic User Difficulty Service

An intelligent, modular difficulty adjustment system for Unity games that adapts to player performance in real-time.

[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)](package.json)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-143%20passing-brightgreen.svg)](#testing)
[![Coverage](https://img.shields.io/badge/coverage-~92%25-brightgreen.svg)](#testing)

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Quick Start](#quick-start)
- [Documentation](#documentation)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Configuration](#configuration)
- [Extending the System](#extending-the-system)
- [API Reference](#api-reference)
- [Testing](#testing)
- [Contributing](#contributing)
- [Support](#support)

## Overview

The Dynamic User Difficulty (DUD) service automatically adjusts game difficulty based on:
- **Win/Loss Streaks** - Consecutive wins increase difficulty, losses decrease it
- **Time Since Last Play** - Reduces difficulty for returning players
- **Session Behavior** - Detects rage quits and adjusts accordingly
- **Custom Modifiers** - Easily extend with your own difficulty factors

### Why Use This System?

- 🎯 **Increases Retention**: Keeps players in the optimal challenge zone
- 🔧 **Highly Modular**: Add new modifiers without changing existing code
- 📊 **Data-Driven**: Configure everything through ScriptableObjects
- 🧪 **Testable**: Clean architecture with dependency injection
- 📈 **Analytics Ready**: Built-in tracking for all difficulty changes

## Features

### Core Features ✅ COMPLETE
- ✅ Automatic difficulty adjustment based on player performance
- ✅ Modular modifier system for easy extension
- ✅ Time-based decay for returning players
- ✅ Rage quit detection and compensation
- ✅ Configurable difficulty ranges and thresholds
- ✅ Built-in analytics integration
- ✅ Debug tools and visualization

### Technical Features ✅ PRODUCTION-READY
- ✅ Clean architecture with SOLID principles
- ✅ VContainer dependency injection
- ✅ Unity assembly definitions
- ✅ **Complete test suite (143 tests, ~92% coverage)**
- ✅ Performance optimized (<10ms calculations)
- ✅ Full API documentation
- ✅ **Production-ready with complete implementation**

### ✅ Implementation Status

| Component | Status | Tests | Coverage |
|-----------|--------|-------|----------|
| **Core Service** | ✅ Complete | 10 tests | ~95% |
| **4 Modifiers** | ✅ Complete | 45 tests | ~95% |
| **Models & Data** | ✅ Complete | 20 tests | ~90% |
| **Calculators** | ✅ Complete | 18 tests | ~90% |
| **Configuration** | ✅ Complete | 25 tests | ~88% |
| **Services** | ✅ Complete | 14 tests | ~85% |
| **Integration** | ✅ Complete | 11 tests | ~90% |
| **TOTAL** | **✅ PRODUCTION-READY** | **143 tests** | **~92%** |

## Quick Start

### 1️⃣ Installation

```bash
# This package is already installed as a Git submodule in:
Packages/com.theone.dynamicuserdifficulty/

# No feature flag required - the module is registered directly in VContainer
```

### 2️⃣ Create Configuration

```bash
Right-click in Project → Create → DynamicDifficulty → Config
Save as: Assets/Resources/Configs/DifficultyConfig.asset
```

### 3️⃣ Register in VContainer

```csharp
// In UITemplateVContainer.cs
var difficultyConfig = Resources.Load<TheOneStudio.DynamicUserDifficulty.Configuration.DifficultyConfig>("Configs/DifficultyConfig");
if (difficultyConfig != null)
{
    builder.RegisterModule(new TheOneStudio.DynamicUserDifficulty.DI.DynamicDifficultyModule(difficultyConfig));
}
```

### 4️⃣ Use in Game

```csharp
public class GameController
{
    private readonly IDynamicDifficultyService difficultyService;

    public void StartLevel()
    {
        var result = difficultyService.CalculateDifficulty();
        difficultyService.ApplyDifficulty(result);

        // Use result.NewDifficulty to configure your level
        ConfigureLevel(result.NewDifficulty);
    }
}
```

## Documentation

### 📚 Complete Documentation Index

**[📁 Documentation/INDEX.md](Documentation/INDEX.md)** - Master documentation index with complete navigation

### Core Documentation

| Document | Purpose | Read When |
|----------|---------|-----------|
| **[README.md](README.md)** | Overview and quick start | First time setup |
| **[CLAUDE.md](CLAUDE.md)** | Complete document index for AI | Using with Claude Code |

### Design & Architecture

| Document | Purpose | Read When |
|----------|---------|-----------|
| **[DynamicUserDifficulty.md](DynamicUserDifficulty.md)** | Business logic and formulas | Understanding requirements |
| **[TechnicalDesign.md](TechnicalDesign.md)** | Architecture and patterns | Learning the system |

### Implementation Guides

| Document | Purpose | Read When |
|----------|---------|-----------|
| **[Documentation/ImplementationGuide.md](Documentation/ImplementationGuide.md)** | Step-by-step implementation | Building from scratch |
| **[Documentation/APIReference.md](Documentation/APIReference.md)** | Complete API documentation | During development |
| **[Documentation/ModifierGuide.md](Documentation/ModifierGuide.md)** | Creating custom modifiers | Extending the system |
| **[Documentation/IntegrationGuide.md](Documentation/IntegrationGuide.md)** | Integration with game systems | Connecting to your game |

### Testing Documentation ✅ COMPLETE

| Document | Purpose | Read When |
|----------|---------|-----------|
| **[Documentation/TestFrameworkDesign.md](Documentation/TestFrameworkDesign.md)** | Test infrastructure design | Setting up tests |
| **[Documentation/TestStrategy.md](Documentation/TestStrategy.md)** | Testing approach & guidelines | Planning test coverage |
| **[Documentation/TestImplementation.md](Documentation/TestImplementation.md)** ✅ | **Complete test suite (143 tests)** | Test implementation details |

### 🎯 Learning Path

```mermaid
graph LR
    A[Read README] --> B[Review Design]
    B --> C[Follow Implementation]
    C --> D[Integrate with Game]
    D --> E[Add Custom Modifiers]
```

## Project Structure

```
DynamicUserDifficulty/
├── 📁 Documentation/           # All documentation
│   ├── 📄 INDEX.md            # Master index
│   ├── 📄 README.md           # Documentation overview
│   ├── 📄 ImplementationGuide.md
│   ├── 📄 APIReference.md
│   ├── 📄 ModifierGuide.md
│   ├── 📄 IntegrationGuide.md
│   ├── 📄 TestFrameworkDesign.md
│   ├── 📄 TestStrategy.md
│   └── 📄 TestImplementation.md ✅ 143 tests
│
├── 📁 Runtime/                # Source code ✅ COMPLETE
│   ├── 📁 Core/              # Main service
│   │   ├── IDynamicDifficultyService.cs
│   │   └── DynamicDifficultyService.cs
│   │
│   ├── 📁 Modifiers/         # Difficulty modifiers ✅ 4/4 COMPLETE
│   │   ├── 📁 Base/
│   │   │   └── BaseDifficultyModifier.cs
│   │   └── 📁 Implementations/
│   │       ├── WinStreakModifier.cs ✅
│   │       ├── LossStreakModifier.cs ✅
│   │       ├── TimeDecayModifier.cs ✅
│   │       └── RageQuitModifier.cs ✅
│   │
│   ├── 📁 Models/            # Data structures
│   │   ├── PlayerSessionData.cs
│   │   └── DifficultyResult.cs
│   │
│   ├── 📁 Calculators/       # Calculation logic
│   ├── 📁 Providers/         # Data providers
│   ├── 📁 Configuration/     # ScriptableObjects
│   └── 📁 DI/               # Dependency injection
│
├── 📁 Editor/                # Editor tools
├── 📁 Tests/                 # ✅ 143 tests across 11 files
├── 📄 README.md              # This file
├── 📄 CLAUDE.md              # AI guidance
├── 📄 package.json           # Package manifest
└── 📄 *.asmdef              # Assembly definition
```

## Installation

### Option 1: Unity Package Manager (Recommended)

1. Open Unity Package Manager
2. Click "+" → "Add package from git URL"
3. Enter: `https://github.com/The1Studio/DynamicUserDifficulty.git`

### Option 2: Git Submodule (Already Configured)

```bash
# Already added as submodule at:
git submodule add git@github.com:The1Studio/DynamicUserDifficulty.git Packages/com.theone.dynamicuserdifficulty
```

### Dependencies

- Unity 2021.3 or higher
- VContainer 1.16.0+
- UniTask 2.3.0+
- UITemplate Framework

## Basic Usage

### Calculate Difficulty

```csharp
// Get the service
var difficultyService = container.Resolve<IDynamicDifficultyService>();

// Calculate new difficulty
var result = difficultyService.CalculateDifficulty();

// Apply the difficulty
difficultyService.ApplyDifficulty(result);

// Access the difficulty value
float difficulty = result.NewDifficulty; // 1-10 scale
```

### Track Player Performance

```csharp
// When player wins
difficultyService.OnLevelComplete(won: true, completionTime: 120f);

// When player loses
difficultyService.OnLevelComplete(won: false, completionTime: 45f);

// When player quits
difficultyService.OnSessionEnd(SessionEndType.QuitAfterLoss);
```

### Map Difficulty to Game Parameters

```csharp
public void ConfigureLevel(float difficulty)
{
    // Example mappings (1-10 difficulty scale)

    // Screw colors: 3-7 based on difficulty
    int colorCount = Mathf.FloorToInt(2 + difficulty * 0.5f);

    // Piece complexity: 0.1-1.0
    float complexity = difficulty / 10f;

    // Time limit: Only at high difficulty
    bool hasTimeLimit = difficulty > 7;

    // Hints: More at lower difficulty
    int hintCount = Mathf.Max(0, 5 - Mathf.FloorToInt(difficulty / 2));
}
```

## Configuration

### Difficulty Settings

Configure in `DifficultyConfig` ScriptableObject:

```yaml
Difficulty Range:
  Min: 1.0
  Max: 10.0
  Default: 3.0
  Max Change Per Session: 2.0

Modifiers:
  - Win Streak:
      Threshold: 3 wins
      Step Size: 0.5
      Max Bonus: 2.0

  - Loss Streak:
      Threshold: 2 losses
      Step Size: 0.3
      Max Reduction: 1.5

  - Time Decay:
      Decay Per Day: 0.5
      Max Decay: 2.0
      Grace Hours: 6

  - Rage Quit:
      Detection Time: 30 seconds
      Reduction: 1.0
```

### Difficulty Presets

```csharp
// Easy Mode
MinDifficulty: 1, MaxDifficulty: 5, WinThreshold: 5

// Normal Mode
MinDifficulty: 1, MaxDifficulty: 10, WinThreshold: 3

// Hard Mode
MinDifficulty: 3, MaxDifficulty: 10, WinThreshold: 2
```

## Extending the System

### Creating a Custom Modifier

```csharp
public class SpeedBonusModifier : BaseDifficultyModifier
{
    public override string ModifierName => "SpeedBonus";

    public SpeedBonusModifier(ModifierConfig config) : base(config) { }

    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        // Fast completion = Higher difficulty
        var avgTime = GetAverageCompletionTime(sessionData);
        var speedBonus = avgTime < 60 ? 0.5f : 0f;

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = speedBonus,
            Reason = "Fast completion bonus"
        };
    }
}
```

### Registering the Modifier

```csharp
// In DynamicDifficultyModule.cs
builder.Register<SpeedBonusModifier>(Lifetime.Singleton)
       .As<IDifficultyModifier>();
```

### Configuration

Add to DifficultyConfig:
```yaml
Speed Bonus:
  Enabled: true
  Priority: 5
  Parameters:
    TimeThreshold: 60
    BonusAmount: 0.5
```

## API Reference

### Core Interfaces

#### IDynamicDifficultyService
```csharp
float CurrentDifficulty { get; }
DifficultyResult CalculateDifficulty();
void ApplyDifficulty(DifficultyResult result);
void OnLevelComplete(bool won, float time);
```

#### IDifficultyModifier
```csharp
string ModifierName { get; }
int Priority { get; }
ModifierResult Calculate(PlayerSessionData data);
```

### Data Models

#### DifficultyResult
```csharp
float PreviousDifficulty;
float NewDifficulty;
List<ModifierResult> AppliedModifiers;
string PrimaryReason;
```

#### PlayerSessionData
```csharp
float CurrentDifficulty;
int WinStreak;
int LossStreak;
DateTime LastPlayTime;
SessionInfo LastSession;
```

📖 [Full API Documentation](Documentation/APIReference.md)

## Testing

### Run Unit Tests

```bash
Window → General → Test Runner → Run All
```

### Test Implementation Status ✅ COMPLETE

**Complete test suite with 143 tests and ~92% code coverage!**

| Component | Tests | Coverage | Status |
|-----------|-------|----------|--------|
| **Modifiers** | 45 tests | ~95% | ✅ Complete |
| **Models** | 20 tests | ~90% | ✅ Complete |
| **Calculators** | 18 tests | ~90% | ✅ Complete |
| **Services** | 14 tests | ~85% | ✅ Complete |
| **Configuration** | 25 tests | ~88% | ✅ Complete |
| **Core** | 10 tests | ~90% | ✅ Complete |
| **Integration** | 11 tests | ~90% | ✅ Complete |
| **Total** | **143 tests** | **~92%** | ✅ **PRODUCTION-READY** |

### Test Categories

- ✅ **Unit Tests** - All modifiers, calculators, and models
- ✅ **Integration Tests** - Service integration and player journeys
- ✅ **Test Framework** - Mocks, builders, and utilities
- ✅ **Error Handling** - Graceful failure recovery

### Important Testing Notes

- **Unity Test Runner Setup**: Requires proper assembly definitions
- **Cache Clearing**: Sometimes needed (`Assets → Reimport All`)
- **TestResults Location**: `/home/tuha/.config/unity3d/TheOneStudio/Unscrew Factory/TestResults.xml`
- **Constructor Injection Pattern**: All tests use constructor injection (not Initialize methods)

See [Documentation/TestImplementation.md](Documentation/TestImplementation.md) for complete test details.

### Manual Testing

1. Enable debug mode in DifficultyConfig
2. Use debug window: `TheOne → Debug → Difficulty Monitor`
3. Test scenarios:
   - Win 3+ times → Difficulty increases
   - Lose 2+ times → Difficulty decreases
   - Quit after loss → Difficulty decreases more
   - Return after days → Difficulty decreases

## Performance

- **Calculation Time**: < 10ms
- **Memory Usage**: < 1KB per session
- **Cache Duration**: 5 minutes
- **Update Frequency**: Once per level

### Optimization Tips

1. Cache calculations for level duration
2. Limit session history to 10 entries
3. Use object pooling for results
4. Disable debug logs in production

## Troubleshooting

### Common Issues

| Problem | Solution |
|---------|----------|
| Service not initialized | Call `Initialize()` in startup |
| Config not loading | Check Resources/Configs/ path |
| Modifiers not running | Check if config is loaded in VContainer |
| Difficulty not changing | Check modifier thresholds |
| Tests not running | Try `Assets → Reimport All` to clear cache |

### Debug Commands

```csharp
// Force difficulty
difficultyService.SetDifficulty(5.0f);

// Reset streaks
difficultyService.ResetStreaks();

// Clear session data
difficultyService.ClearSessionData();
```

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

1. Fork the repository
2. Create feature branch
3. Make changes with tests
4. Update documentation
5. Submit pull request

### Code Style

- Follow C# conventions
- Use meaningful names
- Add XML documentation
- Keep methods under 20 lines
- Write unit tests

## Roadmap

- [ ] Version 1.1: Machine learning predictions
- [ ] Version 1.2: Multi-factor analysis
- [ ] Version 1.3: A/B testing framework
- [ ] Version 2.0: Cloud synchronization

## Support

- 📧 Email: support@theonestudio.com
- 💬 Discord: [Join our server](https://discord.gg/theonestudio)
- 🐛 Issues: [GitHub Issues](https://github.com/The1Studio/DynamicUserDifficulty/issues)
- 📖 Docs: [Full Documentation](Documentation/README.md)

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

<div align="center">

**[Quick Start](#quick-start)** • **[Documentation](#documentation)** • **[API Reference](#api-reference)** • **[Support](#support)**

✅ **PRODUCTION-READY** • 143 Tests • ~92% Coverage

Made with ❤️ by TheOne Studio

</div>