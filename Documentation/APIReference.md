# Dynamic User Difficulty - API Reference

## Table of Contents
- [Core Interfaces](#core-interfaces)
- [Provider Interfaces](#provider-interfaces)
- [Data Models](#data-models)
- [Modifiers](#modifiers)
- [Calculators](#calculators)
- [Configuration](#configuration)
- [Events & Signals](#events--signals)

---

## Core Interfaces

### IDynamicDifficultyService

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Core`

The main service interface for managing dynamic difficulty with stateless architecture.

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| CurrentDifficulty | float | Gets the current difficulty level (1-10) |

#### Methods

##### CalculateDifficulty()
Calculates new difficulty based on external data via providers using stateless modifiers.
```csharp
DifficultyResult CalculateDifficulty()
```
**Returns:** `DifficultyResult` containing new difficulty and applied modifiers
**Note:** Takes NO parameters - uses provider interfaces to get external data

##### GetDefaultDifficulty()
Returns the default difficulty value.
```csharp
float GetDefaultDifficulty()
```

##### IsValidDifficulty(float)
Validates if a difficulty value is within acceptable range.
```csharp
bool IsValidDifficulty(float difficulty)
```

##### ClampDifficulty(float)
Clamps difficulty value to configured min/max range.
```csharp
float ClampDifficulty(float difficulty)
```

---

## Provider Interfaces

All provider interfaces are read-only and provide data from external game systems.

### IDifficultyDataProvider (Required)

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Base interface for difficulty data storage.

#### Methods
```csharp
float GetCurrentDifficulty()
void SetCurrentDifficulty(float difficulty)
```

### IWinStreakProvider (Optional)

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Provides win/loss streak data from external game systems.

#### Methods - Using 4/4 methods (100% utilization) ✅
```csharp
int GetWinStreak()        // ✅ Used by WinStreakModifier
int GetLossStreak()       // ✅ Used by LossStreakModifier
int GetTotalWins()        // ✅ Used by CompletionRateModifier
int GetTotalLosses()      // ✅ Used by CompletionRateModifier
```

### ITimeDecayProvider (Optional)

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Provides time-based data for difficulty decay calculations.

#### Methods - Using 3/3 methods (100% utilization) ✅
```csharp
TimeSpan GetTimeSinceLastPlay()  // ✅ Used by TimeDecayModifier
DateTime GetLastPlayTime()       // ✅ Used by TimeDecayModifier
int GetDaysAwayFromGame()        // ✅ Used by TimeDecayModifier
```

### IRageQuitProvider (Optional)

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Provides quit behavior data for rage quit detection.

#### Methods - Using 4/4 methods (100% utilization) ✅
```csharp
QuitType GetLastQuitType()         // ✅ Used by RageQuitModifier (ANY quit behavior)
float GetCurrentSessionDuration()  // ✅ Used by SessionPatternModifier
int GetRecentRageQuitCount()       // ✅ Used by RageQuitModifier, SessionPatternModifier
float GetAverageSessionDuration()  // ✅ Used by SessionPatternModifier
```

### ILevelProgressProvider (Optional)

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Provides level progression and completion data.

#### Methods - Using 6/6 methods (100% utilization) ✅
```csharp
int GetCurrentLevel()              // ✅ Used by LevelProgressModifier
float GetAverageCompletionTime()   // ✅ Used by LevelProgressModifier
int GetAttemptsOnCurrentLevel()    // ✅ Used by LevelProgressModifier (struggling players)
float GetCompletionRate()          // ✅ Used by CompletionRateModifier
float GetCurrentLevelDifficulty()  // ✅ Used by LevelProgressModifier
float GetCurrentLevelTimePercentage() // ✅ Enhanced timing analysis
```

### 🆕 ISessionPatternProvider (Optional) - NEW

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Provides advanced session pattern analysis data for enhanced behavioral tracking.

#### Methods - Using 5/5 methods (100% utilization) ✅

##### GetRecentSessionDurations(int count)
Gets the recent session durations for pattern analysis.
```csharp
List<float> GetRecentSessionDurations(int count)
```
| Parameter | Type | Description |
|-----------|------|-------------|
| count | int | Number of recent sessions to retrieve |

**Returns:** List of session durations in seconds, most recent first
**Usage:** ✅ Used by SessionPatternModifier for session history analysis

##### GetTotalRecentQuits()
Gets the total number of quits in recent history.
```csharp
int GetTotalRecentQuits()
```
**Usage:** ✅ Used by SessionPatternModifier for quit ratio calculations

##### GetRecentMidLevelQuits()
Gets the number of mid-level quits in recent history.
```csharp
int GetRecentMidLevelQuits()
```
**Usage:** ✅ Used by SessionPatternModifier for mid-level quit pattern detection

##### GetPreviousDifficulty()
Gets the difficulty value from before the last adjustment.
```csharp
float GetPreviousDifficulty()
```
**Usage:** ✅ Used by SessionPatternModifier to track difficulty improvement effectiveness

##### GetSessionDurationBeforeLastAdjustment()
Gets the session duration from before the last difficulty adjustment.
```csharp
float GetSessionDurationBeforeLastAdjustment()
```
**Usage:** ✅ Used by SessionPatternModifier to track if difficulty changes improve session length

### **🎯 Provider Usage Summary**

**Total Provider Methods: 27**
**Methods Used: 27/27 (100% utilization)** ✅

- **IWinStreakProvider**: 4/4 methods used (100%)
- **ITimeDecayProvider**: 3/3 methods used (100%)
- **IRageQuitProvider**: 4/4 methods used (100%)
- **ILevelProgressProvider**: 6/6 methods used (100%)
- **🆕 ISessionPatternProvider**: 5/5 methods used (100%)
- **IDifficultyDataProvider**: 2/2 methods used (100%)

---

## Data Models

### DifficultyResult

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Complete result of difficulty calculation.

| Field | Type | Description |
|-------|------|-------------|
| PreviousDifficulty | float | Difficulty before calculation |
| NewDifficulty | float | Calculated new difficulty |
| AppliedModifiers | List<ModifierResult> | All applied modifiers |
| CalculatedAt | DateTime | When calculation occurred |
| PrimaryReason | string | Main reason for change |

### ModifierResult

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Result from a single modifier calculation.

| Field | Type | Description |
|-------|------|-------------|
| ModifierName | string | Name of the modifier |
| Value | float | Difficulty adjustment value |
| Reason | string | Human-readable reason |
| Metadata | Dictionary<string, object> | Additional data |

#### Static Factory Methods
```csharp
static ModifierResult NoChange()               // Returns zero adjustment
static ModifierResult Create(string name, float value, string reason)
```

### QuitType

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Enum describing how a session ended.

| Value | Description |
|-------|-------------|
| Normal | Normal session completion |
| RageQuit | Quit due to frustration (applies penalty) |
| MidPlay | Quit during level play (applies penalty) |
| Unknown | Unknown quit type |

**Note:** RageQuitModifier applies penalties for ANY quit behavior, not just rage quits.

### PlayerSessionData

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Contains comprehensive session tracking data.

| Field | Type | Description |
|-------|------|-------------|
| WinStreak | int | Current consecutive wins |
| LossStreak | int | Current consecutive losses |
| TotalWins | int | Total wins across all sessions |
| TotalLosses | int | Total losses across all sessions |
| LastPlayTime | DateTime | When last session occurred |
| CurrentSessionStartTime | DateTime | When current session started |
| Sessions | List<SessionInfo> | Historical session data |

### SessionInfo

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Individual session information.

| Field | Type | Description |
|-------|------|-------------|
| StartTime | DateTime | Session start time |
| EndTime | DateTime | Session end time |
| Duration | float | Session duration in seconds |
| LevelsPlayed | int | Number of levels played |
| LevelsWon | int | Number of levels won |
| QuitType | QuitType | How session ended |

### DetailedSessionInfo

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Enhanced session information with comprehensive tracking.

| Field | Type | Description |
|-------|------|-------------|
| SessionId | string | Unique session identifier |
| StartTime | DateTime | Session start time |
| EndTime | DateTime | Session end time |
| Duration | float | Session duration in seconds |
| LevelsPlayed | int | Number of levels attempted |
| LevelsCompleted | int | Number of levels completed |
| CompletionRate | float | Session completion rate (0-1) |
| AverageCompletionTime | float | Average time per completed level |
| DifficultyAtStart | float | Difficulty when session started |
| DifficultyAtEnd | float | Difficulty when session ended |
| QuitType | QuitType | How session ended |
| QuitReason | string | Detailed quit reason |
| MidLevelQuits | int | Number of mid-level quits |
| TotalAttempts | int | Total level attempts |

---

## Modifiers

All modifiers implement `IDifficultyModifier` and use stateless `Calculate()` methods.

### Base Modifier Interface

```csharp
public interface IDifficultyModifier
{
    string ModifierName { get; }
    int Priority { get; }
    bool IsEnabled { get; set; }
    ModifierResult Calculate(); // NO PARAMETERS - stateless!
}
```

### Available Modifiers

#### WinStreakModifier
**Purpose:** Increases difficulty after consecutive wins
**Provider:** IWinStreakProvider
**Configuration:** WinStreakConfig
**Key Settings:** WinThreshold, StepSize, MaxBonus

#### LossStreakModifier
**Purpose:** Decreases difficulty after consecutive losses
**Provider:** IWinStreakProvider
**Configuration:** LossStreakConfig
**Key Settings:** LossThreshold, StepSize, MaxReduction

#### TimeDecayModifier
**Purpose:** Reduces difficulty for returning players
**Provider:** ITimeDecayProvider
**Configuration:** TimeDecayConfig
**Key Settings:** DecayPerDay, MaxDecay, GraceHours

#### RageQuitModifier
**Purpose:** Applies penalties for ANY quit behavior
**Provider:** IRageQuitProvider
**Configuration:** RageQuitConfig
**Key Settings:** RageQuitThreshold, RageQuitReduction, QuitReduction, MidPlayReduction
**Behavior:** Detects and applies penalties for all quit types (Normal, RageQuit, MidPlay)

#### CompletionRateModifier
**Purpose:** Adjusts based on overall completion rate
**Providers:** IWinStreakProvider, ILevelProgressProvider
**Configuration:** CompletionRateConfig
**Key Settings:** LowThreshold, HighThreshold, LowRateAdjustment, HighRateAdjustment

#### LevelProgressModifier
**Purpose:** Analyzes level progression patterns and detects struggling players
**Provider:** ILevelProgressProvider
**Configuration:** LevelProgressConfig
**Key Settings:** HighAttemptsThreshold, FastCompletionRatio, SlowCompletionRatio
**Enhanced Features:**
- ✅ PercentUsingTimeToComplete integration
- ✅ Struggling player detection (0% completion on easy levels)
- ✅ Enhanced time-based analysis

#### 🆕 SessionPatternModifier (Enhanced)
**Purpose:** Advanced session behavior analysis with comprehensive pattern detection
**Providers:** IRageQuitProvider, ISessionPatternProvider
**Configuration:** SessionPatternConfig
**Enhanced Features:**
- ✅ 100% configuration field utilization (12/12 fields)
- ✅ Advanced session history analysis
- ✅ Mid-level quit pattern detection
- ✅ Difficulty improvement tracking
- ✅ Session duration effectiveness analysis

**Key Settings:**
- MinNormalSessionDuration
- VeryShortSessionThreshold
- SessionHistorySize
- ShortSessionRatio
- MidLevelQuitRatio
- RageQuitCountThreshold
- DifficultyImprovementThreshold

---

## Configuration

### DifficultyConfig (ScriptableObject)

**Path:** `Assets/Resources/GameConfigs/DifficultyConfig.asset`

Single ScriptableObject containing all configuration.

| Field | Type | Description |
|-------|------|-------------|
| MinDifficulty | float | Minimum difficulty (default: 1.0) |
| MaxDifficulty | float | Maximum difficulty (default: 10.0) |
| DefaultDifficulty | float | Starting difficulty (default: 3.0) |
| MaxChangePerSession | float | Maximum change per session (default: 2.0) |
| ModifierConfigs | ModifierConfigContainer | Embedded modifier configurations |

### Modifier Configuration Classes

All modifier configs inherit from `BaseModifierConfig` and are `[Serializable]` classes (NOT ScriptableObjects).

#### Base Configuration
```csharp
public abstract class BaseModifierConfig : IModifierConfig
{
    public abstract string ModifierType { get; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public abstract IModifierConfig CreateDefault();
}
```

### 🆕 Enhanced SessionPatternConfig

**All 12 configuration fields utilized (100%)**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| MinNormalSessionDuration | float | 180s | Minimum session considered normal |
| VeryShortSessionThreshold | float | 60s | Sessions below this are very short |
| VeryShortSessionDecrease | float | 0.5 | Penalty for very short sessions |
| SessionHistorySize | int | 5 | Recent sessions to analyze |
| ShortSessionRatio | float | 0.5 | Ratio triggering short session penalty |
| ConsistentShortSessionsDecrease | float | 0.8 | Penalty for pattern of short sessions |
| RageQuitPatternDecrease | float | 1.0 | Penalty for rage quit patterns |
| MidLevelQuitDecrease | float | 0.4 | Penalty for mid-level quits |
| MidLevelQuitRatio | float | 0.3 | Ratio triggering mid-quit penalty |
| RageQuitCountThreshold | int | 2 | Minimum rage quits for pattern |
| RageQuitPenaltyMultiplier | float | 0.5 | Multiplier for rage quit penalties |
| DifficultyImprovementThreshold | float | 1.2 | Improvement ratio for effectiveness |

---

## Dependency Injection

### Registration

```csharp
// Single line registration in DI container
builder.RegisterDynamicDifficulty();
```

### Service Registrations

The `RegisterDynamicDifficulty()` extension method registers:

- `IDynamicDifficultyService` → `DynamicDifficultyService`
- All 7 modifiers with direct ILogger injection
- Configuration loading and validation
- Provider pattern support

### 🆕 ILogger Integration

All modifiers now use direct ILogger injection:

```csharp
public class ExampleModifier : BaseDifficultyModifier<ExampleConfig>
{
    public ExampleModifier(ExampleConfig config, IProvider provider, ILogger logger)
        : base(config, logger) // Direct injection, no null defaults
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
}
```

**Benefits:**
- No null defaults - robust error handling
- Constructor validation ensures all dependencies are provided
- Consistent logging across all modifiers
- Better debugging and error tracking

---

## Events & Signals

### Integration with UITemplate

The system integrates with UITemplate's signal system:

```csharp
// Automatic signal handling
signalBus.Subscribe<WonSignal>(OnLevelWon);
signalBus.Subscribe<LostSignal>(OnLevelLost);
```

### Manual Event Recording

```csharp
// Manual event recording through adapter
difficultyAdapter.RecordSessionEnd(QuitType.RageQuit);
difficultyAdapter.RecordLevelStart(levelId);
```

---

## Migration Guide

### From ILoggerManager to ILogger

**Old Pattern (Deprecated):**
```csharp
public ExampleModifier(ExampleConfig config, IProvider provider, ILoggerManager loggerManager = null)
    : base(config, loggerManager?.CreateLogger<ExampleModifier>())
```

**New Pattern (Required):**
```csharp
public ExampleModifier(ExampleConfig config, IProvider provider, ILogger logger)
    : base(config, logger) // Direct injection, no null defaults
{
    this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
}
```

### Provider Interface Implementation

When implementing providers, ensure all methods are implemented:

```csharp
public class GameDifficultyProvider :
    IDifficultyDataProvider,
    IWinStreakProvider,
    ITimeDecayProvider,
    IRageQuitProvider,
    ILevelProgressProvider,
    ISessionPatternProvider // New interface
{
    // Implement all 27 methods for 100% utilization
}
```

---

## Error Handling

### Common Exceptions

| Exception | Cause | Solution |
|-----------|-------|----------|
| ArgumentNullException | Required provider not injected | Ensure all required providers are registered |
| InvalidOperationException | Service not initialized | Call RegisterDynamicDifficulty() |
| ConfigurationException | Invalid configuration | Validate DifficultyConfig asset |

### Null Safety

All modifiers include null safety checks:

```csharp
public override ModifierResult Calculate()
{
    try
    {
        if (this.provider == null)
        {
            return ModifierResult.NoChange();
        }

        // Calculation logic
    }
    catch (Exception e)
    {
        this.logger?.Error($"[{ModifierName}] Error calculating: {e.Message}");
        return ModifierResult.NoChange();
    }
}
```

---

## Performance Considerations

### Calculation Performance
- **Target**: < 10ms per calculation
- **Memory**: < 1KB per session
- **Provider calls**: Optimized for frequent access

### Optimization Tips
1. Cache expensive provider calculations
2. Use efficient data structures in providers
3. Disable debug logging in production
4. Implement provider methods for O(1) access when possible

---

This API reference covers all interfaces, classes, and methods in the Dynamic User Difficulty system. For implementation examples, see the [Implementation Guide](ImplementationGuide.md).