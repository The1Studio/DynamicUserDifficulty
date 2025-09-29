# Dynamic User Difficulty - API Reference

## Table of Contents
- [Core Interfaces](#core-interfaces)
- [Data Models](#data-models)
- [Modifiers](#modifiers)
- [Calculators](#calculators)
- [Providers](#providers)
- [Configuration](#configuration)
- [Events & Signals](#events--signals)

---

## Core Interfaces

### IDynamicDifficultyService

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Core`

The main service interface for managing dynamic difficulty.

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| CurrentDifficulty | float | Gets the current difficulty level (1-10) |

#### Methods

##### Initialize()
Initializes the service with saved data.
```csharp
void Initialize()
```

##### CalculateDifficulty()
Calculates new difficulty based on current session data.
```csharp
DifficultyResult CalculateDifficulty()
```
**Returns:** `DifficultyResult` containing new difficulty and applied modifiers

##### ApplyDifficulty(DifficultyResult)
Applies the calculated difficulty and saves to persistence.
```csharp
void ApplyDifficulty(DifficultyResult result)
```
| Parameter | Type | Description |
|-----------|------|-------------|
| result | DifficultyResult | The calculation result to apply |

##### RegisterModifier(IDifficultyModifier)
Registers a new difficulty modifier.
```csharp
void RegisterModifier(IDifficultyModifier modifier)
```

##### OnLevelComplete(bool, float)
Called when a level is completed.
```csharp
void OnLevelComplete(bool won, float completionTime)
```
| Parameter | Type | Description |
|-----------|------|-------------|
| won | bool | Whether the level was won |
| completionTime | float | Time taken to complete in seconds |

---

## Data Models

### PlayerSessionData

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Stores player session information for difficulty calculation.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| CurrentDifficulty | float | 3.0f | Current difficulty level |
| WinStreak | int | 0 | Consecutive wins |
| LossStreak | int | 0 | Consecutive losses |
| LastPlayTime | DateTime | Now | Last time player played |
| LastSession | SessionInfo | null | Information about last session |
| RecentSessions | Queue<SessionInfo> | Empty(10) | Queue of last 10 sessions |
| DetailedSessions | List<DetailedSessionInfo> | Empty(20) | Detailed session tracking |

### SessionInfo

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Information about a single game session.

| Field | Type | Description |
|-------|------|-------------|
| StartTime | DateTime | When session started |
| EndTime | DateTime | When session ended |
| EndType | SessionEndType | How session ended |
| LevelId | int | Level played |
| PlayDuration | float | Duration in seconds |
| Won | bool | Whether level was won |

### DetailedSessionInfo

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Enhanced session tracking with comprehensive metrics.

| Field | Type | Description |
|-------|------|-------------|
| StartTime | DateTime | When session started |
| EndTime | DateTime | When session ended |
| Duration | TimeSpan | Total session duration |
| EndReason | SessionEndReason | How session ended (enum) |
| LevelsCompleted | int | Number of levels completed successfully |
| LevelsFailed | int | Number of levels failed |
| StartDifficulty | float | Difficulty at session start |
| EndDifficulty | float | Difficulty at session end |

### SessionEndReason

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Detailed enum for session end tracking.

| Value | Description |
|-------|-------------|
| Normal | Normal session completion |
| CompletedLevel | Session ended after completing a level |
| FailedLevel | Session ended after failing a level |
| QuitMidLevel | Player quit during level play |
| RageQuit | Detected rage quit behavior |
| Timeout | Session timed out |
| Crash | Application crashed |
| Unknown | Unknown end reason |

### SessionEndType

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Enum describing how a session ended.

| Value | Description |
|-------|-------------|
| CompletedWin | Level completed successfully |
| CompletedLoss | Level failed |
| QuitDuringPlay | Player quit mid-level |
| QuitAfterWin | Player quit after winning |
| QuitAfterLoss | Player quit after losing (potential rage quit) |
| Timeout | Session timed out |

### ModifierResult

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Result from a single modifier calculation.

| Field | Type | Description |
|-------|------|-------------|
| ModifierName | string | Name of the modifier |
| Value | float | Difficulty adjustment value |
| Reason | string | Human-readable reason |
| Metadata | Dictionary<string, object> | Additional data |

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

---

## Modifiers

### IDifficultyModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Interface for all difficulty modifiers.

| Property | Type | Description |
|----------|------|-------------|
| ModifierName | string | Unique name of modifier |
| Priority | int | Execution order (lower = earlier) |
| IsEnabled | bool | Whether modifier is active |

| Method | Returns | Description |
|--------|---------|-------------|
| Calculate(PlayerSessionData) | ModifierResult | Calculates modifier value |
| OnApplied(DifficultyResult) | void | Called after difficulty applied |

### BaseDifficultyModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Abstract base class for modifiers.

#### Protected Methods

##### GetParameter(string, float)
Gets a configuration parameter value.
```csharp
protected float GetParameter(string key, float defaultValue = 0f)
```

##### ApplyCurve(float)
Applies response curve to a value.
```csharp
protected float ApplyCurve(float input)
```

### Built-in Modifiers (7 Total) ✅ COMPLETE

#### WinStreakModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Increases difficulty based on consecutive wins.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| WinThreshold | 3 | Wins needed to trigger |
| StepSize | 0.5 | Difficulty increase per win |
| MaxBonus | 2.0 | Maximum difficulty increase |

#### LossStreakModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Decreases difficulty based on consecutive losses.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| LossThreshold | 2 | Losses needed to trigger |
| StepSize | 0.3 | Difficulty decrease per loss |
| MaxReduction | 1.5 | Maximum difficulty decrease |

#### TimeDecayModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Reduces difficulty based on time since last play.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| DecayPerDay | 0.5 | Difficulty reduction per day |
| MaxDecay | 2.0 | Maximum total reduction |
| GraceHours | 6 | Hours before decay starts |

#### RageQuitModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Reduces difficulty when rage quit is detected.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| RageQuitThreshold | 30 | Seconds to detect rage quit |
| RageQuitReduction | 1.0 | Reduction for rage quit |
| QuitReduction | 0.5 | Reduction for normal quit |

#### CompletionRateModifier ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Adjusts difficulty based on overall player success rate.

**Data Sources:**
- `IWinStreakProvider.GetTotalWins()`
- `IWinStreakProvider.GetTotalLosses()`
- `ILevelProgressProvider.GetCompletionRate()`

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| LowCompletionThreshold | 0.4f | Below this rate = easier difficulty |
| HighCompletionThreshold | 0.7f | Above this rate = harder difficulty |
| LowCompletionAdjustment | -0.5f | Reduction for low completion rate |
| HighCompletionAdjustment | 0.5f | Increase for high completion rate |

#### LevelProgressModifier ✅ **ENHANCED WITH NEW FEATURES**

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Analyzes level progression patterns with enhanced time-based calculations and configurable performance thresholds.

**Data Sources:**
- `ILevelProgressProvider.GetCurrentLevel()`
- `ILevelProgressProvider.GetAverageCompletionTime()`
- `ILevelProgressProvider.GetAttemptsOnCurrentLevel()`
- `ILevelProgressProvider.GetCurrentLevelDifficulty()`
- `ILevelProgressProvider.GetCurrentLevelTimePercentage()` **🆕 NEW**

**Enhanced Parameters (11 total):**
| Parameter | Default | Range | Description |
|-----------|---------|-------|-------------|
| HighAttemptsThreshold | 5 | 3-10 | Max attempts before reduction |
| DifficultyDecreasePerAttempt | 0.2f | 0.1f-0.5f | Reduction per excess attempt |
| FastCompletionRatio | 0.7f | 0.3f-0.9f | Fast completion threshold ratio |
| SlowCompletionRatio | 1.5f | 1.1f-2.0f | Slow completion threshold ratio |
| FastCompletionBonus | 0.3f | 0.1f-1f | Bonus for fast completion |
| SlowCompletionPenalty | 0.3f | 0.1f-1f | Reduction for slow completion |
| **MaxPenaltyMultiplier** | **1.0f** | **0.5f-1.5f** | **🆕 Maximum penalty multiplier for slow completion** |
| **FastCompletionMultiplier** | **1.0f** | **0.1f-0.9f** | **🆕 Multiplier for fast completion bonus** |
| **HardLevelThreshold** | **3f** | **2f-5f** | **🆕 Minimum level difficulty for mastery bonus** |
| **MasteryCompletionRate** | **0.7f** | **0.5f-1f** | **🆕 Completion rate threshold for mastery** |
| **MasteryBonus** | **0.3f** | **0.1f-0.5f** | **🆕 Difficulty increase for mastering hard levels** |
| **EasyLevelThreshold** | **2f** | **1f-3f** | **🆕 Maximum level difficulty for struggle detection** |
| **StruggleCompletionRate** | **0.3f** | **0.1f-0.5f** | **🆕 Completion rate threshold for struggle** |
| **StrugglePenalty** | **0.3f** | **0.1f-0.5f** | **🆕 Difficulty decrease for struggling on easy levels** |
| **EstimatedHoursPerSession** | **0.33f** | **0.1f-1f** | **🆕 Estimated hours per session for progression calculation** |

**Enhanced Time-Based Calculation:**
The modifier now uses `GetCurrentLevelTimePercentage()` as the primary time metric, with fallback to session-based completion time:

```csharp
// Primary: Use PercentUsingTimeToComplete from UITemplate
var timePercentage = levelProgressProvider.GetCurrentLevelTimePercentage();
if (timePercentage > 0)
{
    // Values < 1.0 = faster than expected, values > 1.0 = slower than expected
    if (timePercentage < config.FastCompletionRatio)
    {
        var bonus = config.FastCompletionBonus * (1.0f - timePercentage) * config.FastCompletionMultiplier;
        // Apply bonus with configurable multiplier
    }
    else if (timePercentage > config.SlowCompletionRatio)
    {
        var penalty = config.SlowCompletionPenalty * Math.Min(timePercentage - 1.0f, config.MaxPenaltyMultiplier);
        // Apply penalty with configurable cap
    }
}
else
{
    // Fallback: Use session-based completion time
    var avgCompletionTime = levelProgressProvider.GetAverageCompletionTime();
    // Calculate time ratio and apply adjustments
}
```

#### SessionPatternModifier ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Detects session patterns and player frustration.

**Data Sources:**
- `IRageQuitProvider.GetAverageSessionDuration()`
- `IRageQuitProvider.GetRecentRageQuitCount()`

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| ShortSessionThreshold | 60f | Short session threshold (seconds) |
| LongSessionThreshold | 600f | Long session threshold (seconds) |
| RageQuitCountThreshold | 3 | Recent rage quits threshold |
| ShortSessionReduction | -0.2f | Reduction for short sessions |
| RageQuitReduction | -0.5f | Reduction for rage quit pattern |
| LongSessionBonus | 0.1f | Bonus for long engagement |

---

## Calculators

### IDifficultyCalculator

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Calculators`

Interface for difficulty calculation logic.

```csharp
DifficultyResult Calculate(
    PlayerSessionData sessionData,
    IEnumerable<IDifficultyModifier> modifiers)
```

### DifficultyCalculator

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Calculators`

Default implementation that:
1. Runs all enabled modifiers
2. Aggregates results
3. Clamps to valid range
4. Applies max change limit

### ModifierAggregator

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Calculators`

Aggregates multiple modifier results with various strategies.

#### Methods

##### Aggregate(List<ModifierResult>)
```csharp
float Aggregate(List<ModifierResult> results)
```
Default strategy: Sums all modifier values

##### AggregateWeighted(List<ModifierResult>, float[])
```csharp
float AggregateWeighted(List<ModifierResult> results, float[] weights)
```
Calculates weighted average of modifier values

##### AggregateMax(List<ModifierResult>)
```csharp
float AggregateMax(List<ModifierResult> results)
```
Returns the modifier with maximum absolute value

##### AggregateDiminishing(List<ModifierResult>, float)
```csharp
float AggregateDiminishing(List<ModifierResult> results, float diminishingFactor = 0.5f)
```
Applies diminishing returns to prevent extreme values

---

## Providers

### ISessionDataProvider

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Interface for session data persistence.

| Method | Returns | Description |
|--------|---------|-------------|
| GetCurrentSession() | PlayerSessionData | Gets current session data |
| SaveSession(PlayerSessionData) | void | Saves session data |
| UpdateWinStreak(int) | void | Updates win streak |
| UpdateLossStreak(int) | void | Updates loss streak |
| RecordSessionEnd(SessionEndType) | void | Records how session ended |

### IDifficultyDataProvider (Required)

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Base interface for difficulty storage.

| Method | Returns | Description |
|--------|---------|-------------|
| GetCurrentDifficulty() | float | Gets current difficulty |
| SetCurrentDifficulty(float) | void | Sets current difficulty |

### IWinStreakProvider (Optional) - Using 4/4 methods ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Win/loss streak tracking interface.

| Method | Returns | Used By | Description |
|--------|---------|---------|-------------|
| GetWinStreak() | int | WinStreakModifier ✅ | Current consecutive wins |
| GetLossStreak() | int | LossStreakModifier ✅ | Current consecutive losses |
| GetTotalWins() | int | CompletionRateModifier ✅ | Total wins across all time |
| GetTotalLosses() | int | CompletionRateModifier ✅ | Total losses across all time |

### ITimeDecayProvider (Optional) - Using 3/3 methods ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Time-based tracking interface.

| Method | Returns | Used By | Description |
|--------|---------|---------|-------------|
| GetTimeSinceLastPlay() | TimeSpan | TimeDecayModifier ✅ | Time since last play session |
| GetLastPlayTime() | DateTime | TimeDecayModifier ✅ | Last play timestamp |
| GetDaysAwayFromGame() | int | TimeDecayModifier ✅ | Days away from game |

### IRageQuitProvider (Optional) - Using 4/4 methods ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Rage quit detection interface.

| Method | Returns | Used By | Description |
|--------|---------|---------|-------------|
| GetLastQuitType() | QuitType | RageQuitModifier ✅ | How last session ended |
| GetCurrentSessionDuration() | float | SessionPatternModifier ✅ | Current session duration |
| GetRecentRageQuitCount() | int | RageQuitModifier ✅, SessionPatternModifier ✅ | Recent rage quit count |
| GetAverageSessionDuration() | float | SessionPatternModifier ✅ | Average session duration |

### ILevelProgressProvider (Optional) - Using 6/6 methods ✅ **ENHANCED**

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Level progress tracking interface with new time-based method.

| Method | Returns | Used By | Description |
|--------|---------|---------|-------------|
| GetCurrentLevel() | int | LevelProgressModifier ✅ | Current level number |
| GetAverageCompletionTime() | float | LevelProgressModifier ✅ | Average completion time |
| GetAttemptsOnCurrentLevel() | int | LevelProgressModifier ✅ | Attempts on current level |
| GetCompletionRate() | float | CompletionRateModifier ✅ | Overall completion rate |
| GetCurrentLevelDifficulty() | float | LevelProgressModifier ✅ | Current level's difficulty |
| **GetCurrentLevelTimePercentage()** | **float** | **LevelProgressModifier** ✅ | **🆕 Performance percentage for current level completion time (< 1.0 = faster than expected, > 1.0 = slower than expected)** |

### **🎯 Provider Utilization Summary**

**Total Provider Methods: 22 (was 21)**
**Methods Used by Modifiers: 22/22 (100% utilization)** ✅

| Provider Interface | Methods | Used | Utilization | Modifiers Using |
|-------------------|---------|------|-------------|-----------------|
| **IDifficultyDataProvider** | 2 | 2 | 100% | Core system |
| **IWinStreakProvider** | 4 | 4 | 100% | WinStreakModifier, LossStreakModifier, CompletionRateModifier |
| **ITimeDecayProvider** | 3 | 3 | 100% | TimeDecayModifier |
| **IRageQuitProvider** | 4 | 4 | 100% | RageQuitModifier, SessionPatternModifier |
| **ILevelProgressProvider** | **6** | **6** | **100%** | **CompletionRateModifier, LevelProgressModifier** |

### PlayerPrefsDataProvider

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Default implementation using Unity PlayerPrefs.

**PlayerPrefs Keys:**
- `DUD_CurrentDifficulty`
- `DUD_WinStreak`
- `DUD_LossStreak`
- `DUD_LastPlayTime`
- `DUD_SessionData`
- `DUD_DetailedSessions`

---

## Configuration

### DifficultyConfig

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Configuration`

ScriptableObject for difficulty settings.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| MinDifficulty | float | 1.0 | Minimum difficulty |
| MaxDifficulty | float | 10.0 | Maximum difficulty |
| DefaultDifficulty | float | 3.0 | Starting difficulty |
| MaxChangePerSession | float | 2.0 | Max change in one session |
| ModifierConfigs | List<ModifierConfig> | Empty | Modifier configurations |
| EnableDebugLogs | bool | false | Enable debug logging |

#### Methods

##### CreateDefault()
```csharp
public static DifficultyConfig CreateDefault()
```
Creates a new configuration with default settings.

##### GetModifierConfig(string)
```csharp
public ModifierConfig GetModifierConfig(string modifierType)
```
Gets configuration for a specific modifier type.

##### SetParameter(string, float)
```csharp
public void SetParameter(string key, float value)
```
Sets a global parameter value.

### ModifierConfig

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Configuration`

Configuration for individual modifiers.

| Field | Type | Description |
|-------|------|-------------|
| ModifierType | string | Type identifier |
| Enabled | bool | Whether enabled |
| Priority | int | Execution order |
| ResponseCurve | AnimationCurve | Value transformation |
| Parameters | List<ModifierParameter> | Custom parameters |

#### Methods

##### SetModifierType(string)
```csharp
public void SetModifierType(string type)
```
Sets the modifier type using DifficultyConstants.

##### SetParameter(string, float)
```csharp
public void SetParameter(string key, float value)
```
Sets a parameter for this modifier.

##### GetParameter(string, float)
```csharp
public float GetParameter(string key, float defaultValue = 0f)
```
Gets a parameter value with fallback.

---

## Core Constants

### DifficultyConstants

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Core`

Centralized constants for the entire system.

#### Modifier Type Names
```csharp
public const string MODIFIER_TYPE_WIN_STREAK = "WinStreak";
public const string MODIFIER_TYPE_LOSS_STREAK = "LossStreak";
public const string MODIFIER_TYPE_TIME_DECAY = "TimeDecay";
public const string MODIFIER_TYPE_RAGE_QUIT = "RageQuit";
public const string MODIFIER_TYPE_COMPLETION_RATE = "CompletionRate"; // ✅
public const string MODIFIER_TYPE_LEVEL_PROGRESS = "LevelProgress"; // ✅
public const string MODIFIER_TYPE_SESSION_PATTERN = "SessionPattern"; // ✅
```

#### Parameter Keys
```csharp
public const string PARAM_WIN_THRESHOLD = "WinThreshold";
public const string PARAM_STEP_SIZE = "StepSize";
public const string PARAM_MAX_BONUS = "MaxBonus";
public const string PARAM_DECAY_PER_DAY = "DecayPerDay";
// ... and more
```

#### Default Values
```csharp
public const float MIN_DIFFICULTY = 1f;
public const float MAX_DIFFICULTY = 10f;
public const float DEFAULT_DIFFICULTY = 3f;
public const float WIN_STREAK_DEFAULT_THRESHOLD = 3f;
// ... and more
```

---

## Events & Signals

### Integration with Screw3D Signals

Subscribe to these signals for automatic difficulty adjustment:

| Signal | When Fired | Action |
|--------|------------|--------|
| WonSignal | Level won | Update win streak |
| LostSignal | Level lost | Update loss streak |
| GamePausedSignal | Game paused | Track potential quit |
| GameResumedSignal | Game resumed | Calculate time away |
| LevelStartSignal | Level starts | Apply difficulty |

### Custom Events

The service fires these events:

| Event | Data | When |
|-------|------|------|
| OnDifficultyChanged | DifficultyResult | After difficulty changes |
| OnModifierApplied | ModifierResult | After each modifier |
| OnSessionRecorded | SessionInfo | When session ends |

---

## Usage Examples

### Basic Integration
```csharp
public class GameController
{
    private readonly IDynamicDifficultyService difficultyService;

    public void StartLevel()
    {
        var result = difficultyService.CalculateDifficulty();
        difficultyService.ApplyDifficulty(result);

        // Use result.NewDifficulty to configure level
        ConfigureLevel(result.NewDifficulty);
    }

    public void OnLevelComplete(bool won, float time)
    {
        difficultyService.OnLevelComplete(won, time);
    }
}
```

### Enhanced LevelProgressModifier Usage ✅
```csharp
public class EnhancedLevelProgressProvider : ILevelProgressProvider
{
    private readonly UITemplateLevelDataController levelController;

    public float GetCurrentLevelTimePercentage()
    {
        // 🆕 NEW: Return UITemplate's PercentUsingTimeToComplete
        var levelData = levelController.GetCurrentLevelData();
        return levelData?.PercentUsingTimeToComplete ?? 0f;
    }

    public int GetCurrentLevel() => levelController.CurrentLevel;
    public float GetAverageCompletionTime() => levelController.GetAverageTime();
    public int GetAttemptsOnCurrentLevel() => levelController.GetAttempts();
    public float GetCompletionRate() => levelController.GetCompletionRate();
    public float GetCurrentLevelDifficulty() => levelController.GetLevelDifficulty();
}
```

### Custom Modifier with Provider Methods ✅
```csharp
public class AdvancedPlayerModifier : BaseDifficultyModifier
{
    private readonly ILevelProgressProvider levelProvider;
    private readonly IRageQuitProvider rageProvider;

    public override string ModifierName => "AdvancedPlayer";

    public AdvancedPlayerModifier(ModifierConfig config,
        ILevelProgressProvider levelProvider,
        IRageQuitProvider rageProvider) : base(config)
    {
        this.levelProvider = levelProvider;
        this.rageProvider = rageProvider;
    }

    public override ModifierResult Calculate(PlayerSessionData data)
    {
        // Use all provider methods for comprehensive analysis
        var completionRate = levelProvider.GetCompletionRate();
        var currentLevel = levelProvider.GetCurrentLevel();
        var avgTime = levelProvider.GetAverageCompletionTime();
        var attempts = levelProvider.GetAttemptsOnCurrentLevel();
        var levelDifficulty = levelProvider.GetCurrentLevelDifficulty();
        var timePercentage = levelProvider.GetCurrentLevelTimePercentage(); // 🆕 NEW

        var avgSessionDuration = rageProvider.GetAverageSessionDuration();
        var recentRageQuits = rageProvider.GetRecentRageQuitCount();

        // Complex analysis using all available data
        var skillScore = CalculateSkillScore(completionRate, avgTime, attempts);
        var engagementScore = CalculateEngagementScore(avgSessionDuration, recentRageQuits);
        var timeEfficiencyScore = CalculateTimeEfficiencyScore(timePercentage); // 🆕 NEW

        var adjustment = (skillScore + engagementScore + timeEfficiencyScore) / 3f;

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = $"Advanced analysis: skill={skillScore:F2}, engagement={engagementScore:F2}, time={timeEfficiencyScore:F2}",
            Metadata = new Dictionary<string, object>
            {
                ["completion_rate"] = completionRate,
                ["current_level"] = currentLevel,
                ["avg_time"] = avgTime,
                ["attempts"] = attempts,
                ["level_difficulty"] = levelDifficulty,
                ["time_percentage"] = timePercentage, // 🆕 NEW
                ["avg_session_duration"] = avgSessionDuration,
                ["recent_rage_quits"] = recentRageQuits
            }
        };
    }
}
```

### Accessing Current Difficulty
```csharp
float currentDifficulty = difficultyService.CurrentDifficulty;

// Map to game parameters
int screwColors = Mathf.FloorToInt(currentDifficulty * 0.7f + 2);
float pieceComplexity = currentDifficulty / 10f;
```

---

## Error Handling

### Common Exceptions

| Exception | Cause | Solution |
|-----------|-------|----------|
| NullReferenceException | Config not loaded | Ensure DifficultyConfig in Resources |
| ArgumentOutOfRangeException | Invalid difficulty | Check min/max bounds |
| InvalidOperationException | Service not initialized | Call Initialize() first |
| ArgumentNullException | Null modifier or data | Validate inputs before calls |

### Debug Mode

Enable debug logs in DifficultyConfig to see:
- Modifier calculations
- Difficulty changes
- Session tracking
- Performance metrics
- Provider method calls ✅
- Time-based calculations ✅

---

*API Version: 2.2.0*
*Last Updated: 2025-01-26*
*Namespace: TheOneStudio.DynamicUserDifficulty.*
*7 Modifiers • 22/22 Provider Methods Used (100% Utilization) • Enhanced LevelProgressModifier • Production Ready*