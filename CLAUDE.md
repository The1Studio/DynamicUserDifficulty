# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 📚 Complete Documentation Index

### Master Index
- **[Documentation/INDEX.md](Documentation/INDEX.md)** - Complete documentation map and navigation guide

### Project Overview
- **[README.md](README.md)** - Project overview, quick start, basic usage
- **[CLAUDE.md](CLAUDE.md)** - This file - AI assistant guidance

### Design & Architecture
- **[DynamicUserDifficulty.md](DynamicUserDifficulty.md)** - Business requirements, formulas, testing strategies
- **[TechnicalDesign.md](TechnicalDesign.md)** - Complete technical architecture, patterns, test framework

### Implementation Guides
- **[Documentation/ImplementationGuide.md](Documentation/ImplementationGuide.md)** - Step-by-step implementation with code templates
- **[Documentation/APIReference.md](Documentation/APIReference.md)** - Complete API documentation for all interfaces
- **[Documentation/ModifierGuide.md](Documentation/ModifierGuide.md)** - Creating custom difficulty modifiers
- **[Documentation/IntegrationGuide.md](Documentation/IntegrationGuide.md)** - Integration with UITemplate and Screw3D

### Testing Documentation
- **[Documentation/TestFrameworkDesign.md](Documentation/TestFrameworkDesign.md)** - Test infrastructure and patterns
- **[Documentation/TestStrategy.md](Documentation/TestStrategy.md)** - Testing approach and guidelines
- **[Documentation/TestImplementation.md](Documentation/TestImplementation.md)** ✅ Complete test implementation with 71+ tests (~92% coverage)

### Configuration
- **[package.json](package.json)** - Unity package manifest
- **[UITemplate.Services.DynamicUserDifficulty.asmdef](UITemplate.Services.DynamicUserDifficulty.asmdef)** - Assembly definition
- **[Tests/DynamicUserDifficulty.Tests.asmdef](Tests/DynamicUserDifficulty.Tests.asmdef)** - Test assembly definition
- **[Tests/DynamicUserDifficulty.Tests.Runtime.asmdef](Tests/DynamicUserDifficulty.Tests.Runtime.asmdef)** - Runtime test assembly

### Documentation Management
- **[Documentation/README.md](Documentation/README.md)** - Documentation structure overview

## 🚨 CRITICAL Unity Development Rules

### ⚠️ **NEVER CREATE .meta FILES MANUALLY**
**Unity generates .meta files automatically. Creating them manually causes compilation failures and GUID conflicts.**

**❌ NEVER DO:**
- Create .meta files yourself
- Copy .meta files between projects
- Edit .meta file GUIDs manually

**✅ ALWAYS DO:**
- Let Unity generate .meta files automatically
- Delete corrupt .meta files and let Unity regenerate them
- Use `Assets → Reimport All` to fix .meta file issues
- When troubleshooting compilation: Delete all .meta files in package and let Unity regenerate

**🔧 Emergency Fix for Compilation Issues:**
```bash
# Delete all .meta files in the module
find Packages/com.theone.dynamicuserdifficulty -name "*.meta" -delete

# Let Unity regenerate them
Unity Editor → Assets → Reimport All
```

### 📦 New Modifier Implementation Requirements

**When creating new difficulty modifiers, you MUST:**

1. **Add Configuration Parameters**
   - Add new parameter constants to `DifficultyConstants.cs`
   - Update `DifficultyConfig.CreateDefault()` method
   - Add parameter keys to `PARAM_*` constants section

2. **Example for New Modifier:**
```csharp
// In DifficultyConstants.cs
public const string PARAM_NEW_MODIFIER_THRESHOLD = "NewModifierThreshold";
public const float NEW_MODIFIER_DEFAULT_VALUE = 1.0f;

// In DifficultyConfig.cs CreateDefault() method
config.SetParameter(DifficultyConstants.PARAM_NEW_MODIFIER_THRESHOLD,
                   DifficultyConstants.NEW_MODIFIER_DEFAULT_VALUE);

// In your modifier Calculate() method
var threshold = GetParameter(DifficultyConstants.PARAM_NEW_MODIFIER_THRESHOLD,
                           DifficultyConstants.NEW_MODIFIER_DEFAULT_VALUE);
```

3. **Register in DI Module**
   - Add case in `DynamicDifficultyModule.RegisterModifierByType()`
   - Use consistent modifier type string naming

## Module Overview

The DynamicUserDifficulty service is a Unity module within the UITemplate framework for implementing adaptive difficulty based on player performance. It integrates with the existing Screw3D gameplay system and UITemplate's data controllers.

## Architecture Integration Points

### Parent Architecture Context
- **Framework**: UITemplate with VContainer dependency injection (NO ZENJECT)
- **Project Type**: Unity 6 mobile puzzle game (Unscrew Factory)
- **Signal System**: MessagePipe + SignalBus for event-driven communication
- **Data Access**: MUST use controllers from `/Scripts/Models/Controllers/`

### Related Systems
- **ScrewDistributionHelper**: `/Assets/Scripts/Services/Difficulty/ScrewDistributionHelper.cs`
  - Handles weighted screw color distribution
  - Creates clustering patterns for puzzle solvability
- **DifficultyObjectsView**: `/Assets/TheOneFeature/Core/Features/Gameplay/Scripts/Views/Difficulty/`
  - UI representation of difficulty levels
- **Level Validation**: `/Assets/Screw3D/Scripts/EditorTools/`
  - Ensures level solvability across difficulty settings

## Key Development Commands

### Testing Dynamic Difficulty
```bash
# Open Unity and test in Play Mode
Unity Editor → Open Scene: Assets/Scenes/1.MainScene.unity → Play

# Run level validation to ensure difficulty changes don't break levels
Unity Editor → Screw3D → Batch Operations → Validate All Levels

# Test with specific difficulty settings
Unity Editor → TheOne → Configuration And Tools → Difficulty Settings
```

### Build Integration
```bash
# Ensure difficulty service is included in builds
Unity Editor → File → Build Settings → Player Settings → Scripting Define Symbols
# Add: THEONE_DYNAMIC_DIFFICULTY

# Command line validation
Unity -batchmode -executeMethod Screw3D.Gameplay.EditorTools.Services.BatchOperationService.ValidateAllLevelsFromCommandLine
```

## Implementation Architecture

### Required Components

#### 1. Data Models
```csharp
// PlayerPerformanceData.cs - Track player metrics
public class PlayerPerformanceData
{
    public int ConsecutiveWins { get; set; }
    public int ConsecutiveLosses { get; set; }
    public float AverageCompletionTime { get; set; }
    public int RetryCount { get; set; }
}

// DifficultySettings.cs - Configuration
public class DifficultySettings : ScriptableObject
{
    public int WinThresholdForIncrease = 3;
    public int LossThresholdForDecrease = 2;
    public float TimeThresholdMultiplier = 1.5f;
}
```

#### 2. Service Implementation Pattern
```csharp
// IDynamicUserDifficultyService.cs
public interface IDynamicUserDifficultyService
{
    DifficultyLevel CurrentDifficulty { get; }
    void UpdatePerformance(LevelResult result);
    DifficultyAdjustment CalculateAdjustment();
}

// DynamicUserDifficultyService.cs
public class DynamicUserDifficultyService : IDynamicUserDifficultyService
{
    private readonly UITemplateLevelDataController levelController;
    private readonly UITemplateGameSessionDataController sessionController;

    // Constructor injection via VContainer
    public DynamicUserDifficultyService(
        UITemplateLevelDataController levelController,
        UITemplateGameSessionDataController sessionController)
    {
        this.levelController = levelController;
        this.sessionController = sessionController;
    }
}
```

#### 3. VContainer Registration
```csharp
// In UITemplateVContainer.cs or DynamicUserDifficultyModule.cs
builder.Register<IDynamicUserDifficultyService, DynamicUserDifficultyService>(Lifetime.Singleton);
```

### Integration with Existing Systems

#### Level System Integration
- Subscribe to `WonSignal` and `LostSignal` from Screw3D
- Update difficulty before `LevelConfigService` loads next level
- Modify screw distribution using `ScrewDistributionHelper`

#### Data Controller Usage
```csharp
// CORRECT - Using controllers
var currentLevel = levelController.CurrentLevel;
var winStreak = levelController.GetWinStreak();

// WRONG - Direct data access (NEVER DO THIS)
var data = levelData.CurrentLevel; // ❌
```

#### Signal Integration
```csharp
// Subscribe to gameplay signals
signalBus.Subscribe<WonSignal>(OnLevelWon);
signalBus.Subscribe<LostSignal>(OnLevelLost);
signalBus.Subscribe<ProgressChangedSignal>(OnProgressChanged);
```

## Directory Structure Pattern

```
DynamicUserDifficulty/
├── Models/
│   ├── PlayerPerformanceData.cs
│   ├── DifficultyAdjustment.cs
│   └── DifficultySettings.cs
├── Services/
│   ├── IDynamicUserDifficultyService.cs
│   └── DynamicUserDifficultyService.cs
├── Calculators/
│   ├── PerformanceCalculator.cs
│   └── DifficultyAdjustmentCalculator.cs
├── Providers/
│   └── DifficultyDataProvider.cs
├── DynamicUserDifficulty.asmdef
└── CLAUDE.md
```

## Assembly Definition Requirements

Create `DynamicUserDifficulty.asmdef`:
```json
{
    "name": "UITemplate.Services.DynamicUserDifficulty",
    "references": [
        "UITemplate.Scripts",
        "UITemplate.Signal",
        "UITemplate.LocalData",
        "GameFoundation.DI",
        "GameFoundation.Signals",
        "Screw3D",
        "TheOne.Extensions",
        "VContainer",
        "UniTask"
    ]
}
```

## Testing Workflow

1. **Unit Testing Difficulty Calculations**
   - Test threshold triggers
   - Verify adjustment algorithms
   - Validate boundary conditions

2. **Integration Testing**
   - Test with level validation tool
   - Verify signal subscriptions
   - Ensure data persistence

3. **Manual Testing Checklist**
   - [ ] Win 3+ levels consecutively → Difficulty increases
   - [ ] Lose 2+ levels consecutively → Difficulty decreases
   - [ ] Check screw distribution changes
   - [ ] Verify UI difficulty indicators update
   - [ ] Test data persistence across sessions

## Common Pitfalls to Avoid

1. **DO NOT access data models directly** - Always use controllers
2. **DO NOT modify level data during gameplay** - Apply changes before level load
3. **DO NOT ignore level validation** - Run after difficulty adjustments
4. **DO NOT create tight coupling** - Use interfaces and dependency injection
5. **DO NOT forget signal cleanup** - Unsubscribe in Dispose()

## Analytics Integration

Track difficulty events:
```csharp
analyticService.Track("difficulty_adjusted", new Dictionary<string, object>
{
    ["old_difficulty"] = oldDifficulty.ToString(),
    ["new_difficulty"] = newDifficulty.ToString(),
    ["trigger_reason"] = reason,
    ["level"] = currentLevel
});
```

## Build and Deployment

### Pre-commit Checklist
- [ ] Run level validation with new difficulty settings
- [ ] Test on device with profiler
- [ ] Verify assembly references
- [ ] Check feature flag: `THEONE_DYNAMIC_DIFFICULTY`

### CI/CD Integration
The service will be validated through Jenkins pipeline:
- Android: `Jenkinsfile/Android`
- iOS: `Jenkinsfile/IOS`
- Validation: Automatic level testing with difficulty variations

## Performance Considerations

- Cache difficulty calculations (don't recalculate every frame)
- Use object pooling for difficulty-adjusted elements
- Profile on minimum spec devices (2GB RAM Android)
- Keep difficulty adjustments under 50ms computation time

## Debug Support

Enable debug logging:
```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    Debug.Log($"[DynamicDifficulty] Adjusted from {oldLevel} to {newLevel}");
#endif
```

Access via Unity Logs Viewer in-game console when enabled.