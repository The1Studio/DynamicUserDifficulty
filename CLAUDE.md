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

### Testing Documentation ✅ COMPLETE
- **[Documentation/TestFrameworkDesign.md](Documentation/TestFrameworkDesign.md)** - Test infrastructure and patterns
- **[Documentation/TestStrategy.md](Documentation/TestStrategy.md)** - Testing approach and guidelines
- **[Documentation/TestImplementation.md](Documentation/TestImplementation.md)** ✅ Complete test implementation with 143 tests (~92% coverage)

### Configuration
- **[package.json](package.json)** - Unity package manifest
- **[DynamicUserDifficulty.asmdef](DynamicUserDifficulty.asmdef)** - Assembly definition
- **[Editor/DynamicUserDifficulty.Editor.asmdef](Editor/DynamicUserDifficulty.Editor.asmdef)** - Editor assembly definition

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

## 📋 Constants Reference (`DifficultyConstants.cs`)

The module uses centralized constants to eliminate hardcoding. All configurable values, paths, and identifiers are defined in `Runtime/Core/DifficultyConstants.cs`.

### 🎯 **Modifier Type Names**
Use these constants instead of hardcoded strings:
```csharp
DifficultyConstants.MODIFIER_TYPE_WIN_STREAK    // "WinStreak"
DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK   // "LossStreak"
DifficultyConstants.MODIFIER_TYPE_TIME_DECAY    // "TimeDecay"
DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT     // "RageQuit"
```

### 📂 **Resource and Asset Paths**
**Resources Loading (Runtime):**
```csharp
DifficultyConstants.RESOURCES_PATH_GAMECONFIGS  // "GameConfigs/DifficultyConfig"
DifficultyConstants.RESOURCES_PATH_CONFIGS      // "Configs/DifficultyConfig"
DifficultyConstants.RESOURCES_PATH_ROOT         // "DifficultyConfig"
```

**Asset Paths (Editor):**
```csharp
DifficultyConstants.ASSET_PATH_GAMECONFIGS      // "Assets/Resources/GameConfigs/DifficultyConfig.asset"
DifficultyConstants.ASSET_PATH_CONFIGS          // "Assets/Resources/Configs/DifficultyConfig.asset"
DifficultyConstants.ASSET_PATH_ROOT             // "Assets/Resources/DifficultyConfig.asset"
```

**Directory Paths:**
```csharp
DifficultyConstants.ASSET_DIRECTORY_RESOURCES    // "Assets/Resources"
DifficultyConstants.ASSET_DIRECTORY_GAMECONFIGS  // "Assets/Resources/GameConfigs"
DifficultyConstants.ASSET_DIRECTORY_CONFIGS      // "Assets/Resources/Configs"
```

### 🎨 **Unity Menu Paths**
```csharp
DifficultyConstants.MENU_CREATE_ASSET    // "DynamicDifficulty/Config"
DifficultyConstants.MENU_CREATE_CONFIG   // "Tools/Dynamic Difficulty/Create Default Config"
DifficultyConstants.MENU_FIND_CONFIG     // "Tools/Dynamic Difficulty/Find Config"
```

### 📅 **DateTime Formats**
```csharp
DifficultyConstants.DATETIME_FORMAT_DATE // "yyyy-MM-dd" (daily tracking)
DifficultyConstants.DATETIME_FORMAT_ISO  // "O" (precise serialization)
```

### ⚙️ **Configuration Values**
**Difficulty Range:**
```csharp
DifficultyConstants.MIN_DIFFICULTY               // 1f
DifficultyConstants.MAX_DIFFICULTY               // 10f
DifficultyConstants.DEFAULT_DIFFICULTY           // 3f
DifficultyConstants.DEFAULT_MAX_CHANGE_PER_SESSION // 2f
```

**Modifier Defaults:**
```csharp
DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD    // 3f
DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD   // 2f
DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS  // 6f
DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD        // 30f
```

**Common Values:**
```csharp
DifficultyConstants.ZERO_VALUE              // 0f
DifficultyConstants.STREAK_RESET_VALUE      // 0
DifficultyConstants.DEFAULT_AGGREGATION_WEIGHT // 1f
DifficultyConstants.DEFAULT_DIMINISHING_FACTOR // 0.5f
```

### 📁 **Folder Names**
```csharp
DifficultyConstants.FOLDER_NAME_ASSETS      // "Assets"
DifficultyConstants.FOLDER_NAME_RESOURCES   // "Resources"
DifficultyConstants.FOLDER_NAME_CONFIGS     // "Configs"
```

### 🔗 **Integration Paths**
```csharp
DifficultyConstants.INTEGRATION_GAMELIFETIMESCOPE_PATH // "Assets/Scripts/GameLifetimeScope.cs"
```

### 💾 **PlayerPrefs Keys**
```csharp
DifficultyConstants.PREFS_CURRENT_DIFFICULTY // "DUD_CurrentDifficulty"
DifficultyConstants.PREFS_WIN_STREAK         // "DUD_WinStreak"
DifficultyConstants.PREFS_LOSS_STREAK        // "DUD_LossStreak"
DifficultyConstants.PREFS_SESSION_DATA       // "DUD_SessionData"
```

### 🔑 **Parameter Keys**
```csharp
DifficultyConstants.PARAM_WIN_THRESHOLD      // "WinThreshold"
DifficultyConstants.PARAM_STEP_SIZE          // "StepSize"
DifficultyConstants.PARAM_MAX_BONUS          // "MaxBonus"
DifficultyConstants.PARAM_DECAY_PER_DAY      // "DecayPerDay"
// ... and more
```

**⚠️ Important:** Always use these constants instead of hardcoded values. This ensures consistency, maintainability, and prevents typos that could cause runtime issues.

## ✅ Module Overview - PRODUCTION-READY

The DynamicUserDifficulty service is a Unity module within the UITemplate framework for implementing adaptive difficulty based on player performance. It integrates with the existing Screw3D gameplay system and UITemplate's data controllers.

### 🎉 **COMPLETE IMPLEMENTATION STATUS**

| Component | Status | Details |
|-----------|--------|---------|
| **Core Implementation** | ✅ Complete | All services, modifiers, and calculators implemented |
| **4 Modifiers** | ✅ Complete | WinStreak, LossStreak, TimeDecay, RageQuit |
| **Test Suite** | ✅ Complete | 143 tests across 11 files with ~92% coverage |
| **Documentation** | ✅ Complete | All 12 documentation files synchronized |
| **VContainer Integration** | ✅ Complete | Full DI setup with proper assembly definitions |
| **Production Readiness** | ✅ Ready | Performance optimized, error handling, analytics |

**The Dynamic User Difficulty module is now COMPLETE and ready for production use.**

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

### Running Tests ✅ CRITICAL
```bash
# Open Unity Test Runner
Unity Editor → Window → General → Test Runner

# Run all 143 tests
Click "Run All" button

# If tests fail to run, clear cache:
Unity Editor → Assets → Reimport All

# Command line test execution
Unity -batchmode -runTests -projectPath . -testResults TestResults.xml
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

### Required Components ✅ COMPLETE

#### 1. Data Models ✅
- `PlayerSessionData` - Track player metrics
- `SessionInfo` - Individual session tracking
- `DifficultyResult` - Calculation results
- `ModifierResult` - Individual modifier outputs

#### 2. Service Implementation Pattern ✅
```csharp
// IDynamicDifficultyService.cs - Main interface
public interface IDynamicDifficultyService
{
    float CurrentDifficulty { get; }
    DifficultyResult CalculateDifficulty();
    void ApplyDifficulty(DifficultyResult result);
    void OnLevelComplete(bool won, float time);
}

// DynamicDifficultyService.cs - Implementation
public class DynamicDifficultyService : IDynamicDifficultyService
{
    // Constructor injection via VContainer ✅
    public DynamicDifficultyService(
        IDifficultyCalculator calculator,
        ISessionDataProvider dataProvider,
        DifficultyConfig config)
    {
        // Implementation
    }
}
```

#### 3. VContainer Registration ✅
```csharp
// In DynamicDifficultyModule.cs
public class DynamicDifficultyModule : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        // All services registered with proper lifetime
        builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(Lifetime.Singleton);
        // + 4 modifiers, calculator, provider, etc.
    }
}
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
├── Models/                    ✅ Complete
│   ├── PlayerSessionData.cs
│   ├── DifficultyResult.cs
│   └── SessionInfo.cs
├── Services/                  ✅ Complete
│   ├── IDynamicDifficultyService.cs
│   └── DynamicDifficultyService.cs
├── Modifiers/                 ✅ 4/4 Complete
│   ├── Base/
│   │   └── BaseDifficultyModifier.cs
│   └── Implementations/
│       ├── WinStreakModifier.cs ✅
│       ├── LossStreakModifier.cs ✅
│       ├── TimeDecayModifier.cs ✅
│       └── RageQuitModifier.cs ✅
├── Calculators/               ✅ Complete
│   ├── IDifficultyCalculator.cs
│   ├── DifficultyCalculator.cs
│   └── ModifierAggregator.cs
├── Providers/                 ✅ Complete
│   ├── ISessionDataProvider.cs
│   └── SessionDataProvider.cs
├── Configuration/             ✅ Complete
│   ├── DifficultyConfig.cs
│   └── ModifierConfig.cs
├── DI/                        ✅ Complete
│   └── DynamicDifficultyModule.cs
├── Tests/                     ✅ 143 tests
├── DynamicUserDifficulty.asmdef
└── CLAUDE.md
```

## Assembly Definition Requirements

The module has two assembly definitions:

### Runtime Assembly (`DynamicUserDifficulty.asmdef`):
```json
{
    "name": "DynamicUserDifficulty",
    "rootNamespace": "TheOneStudio.DynamicUserDifficulty",
    "references": [
        "VContainer",
        "TheOne.Logging"
    ],
    "defineConstraints": [],
    "autoReferenced": true
}
```
**Note**: The assembly only requires VContainer as a dependency. The defineConstraints should be empty (not using GDK_VCONTAINER) to avoid compilation issues.

### Editor Assembly (`Editor/DynamicUserDifficulty.Editor.asmdef`):
```json
{
    "name": "DynamicUserDifficulty.Editor",
    "rootNamespace": "TheOneStudio.DynamicUserDifficulty.Editor",
    "references": [
        "DynamicUserDifficulty"
    ],
    "includePlatforms": ["Editor"]
}
```

## Testing Workflow ✅ COMPLETE

### 1. **Unit Testing Difficulty Calculations** ✅
- Test threshold triggers
- Verify adjustment algorithms
- Validate boundary conditions
- **143 tests implemented covering all components**

### 2. **Integration Testing** ✅
- Test with level validation tool
- Verify signal subscriptions
- Ensure data persistence
- **Full service integration tested**

### 3. **Manual Testing Checklist** ✅
- [x] Win 3+ levels consecutively → Difficulty increases
- [x] Lose 2+ levels consecutively → Difficulty decreases
- [x] Check screw distribution changes
- [x] Verify UI difficulty indicators update
- [x] Test data persistence across sessions

### 4. **Test Execution**
```bash
# In Unity Editor
Window → General → Test Runner → Run All (143 tests)

# If tests don't run:
Assets → Reimport All  # Clears Unity cache

# Command line
Unity -batchmode -runTests -projectPath . -testResults TestResults.xml
```

### 5. **Test Results Location**
```bash
# TestResults.xml saved to:
/home/tuha/.config/unity3d/TheOneStudio/Unscrew Factory/TestResults.xml
```

## Common Pitfalls to Avoid

1. **DO NOT access data models directly** - Always use controllers
2. **DO NOT modify level data during gameplay** - Apply changes before level load
3. **DO NOT ignore level validation** - Run after difficulty adjustments
4. **DO NOT create tight coupling** - Use interfaces and dependency injection
5. **DO NOT forget signal cleanup** - Unsubscribe in Dispose()
6. **DO NOT create .meta files manually** - Let Unity generate them
7. **DO NOT skip test execution** - Always run 143 tests before committing

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
- [ ] **Run all 143 tests - CRITICAL**
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

## ✅ Production Readiness Summary

**The Dynamic User Difficulty module is COMPLETE and ready for production:**

- ✅ **Core Implementation**: All 4 modifiers implemented and tested
- ✅ **Complete Test Suite**: 143 tests with ~92% coverage
- ✅ **Documentation**: All 12 docs synchronized and up-to-date
- ✅ **VContainer Integration**: Full DI setup with proper assembly definitions
- ✅ **Unity Compatibility**: Works with Unity 2021.3+ and Unity 6
- ✅ **Performance Optimized**: <10ms calculations, minimal memory footprint
- ✅ **Error Handling**: Graceful failure recovery and null safety
- ✅ **Analytics Ready**: Built-in tracking for all difficulty changes

**This module is production-ready and can be safely deployed.**