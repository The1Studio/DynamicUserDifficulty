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
Calculates new difficulty based on current session data using stateless modifiers.
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

### QuitType

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Models`

Enum describing how a session ended.

| Value | Description |
|-------|-------------|
| Normal | Normal session completion |
| RageQuit | Detected rage quit behavior |
| Timeout | Session timed out |
| Crash | Application crashed |

---

## Modifiers

### IDifficultyModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Interface for all stateless difficulty modifiers.

| Property | Type | Description |
|----------|------|-------------|
| ModifierName | string | Unique name of modifier |
| Priority | int | Execution order (lower = earlier) |
| IsEnabled | bool | Whether modifier is active |

| Method | Returns | Description |
|--------|---------|-------------|
| **Calculate()** | **ModifierResult** | **🆕 STATELESS calculation - NO parameters! Gets all data from injected providers** |
| OnApplied(DifficultyResult) | void | Called after difficulty applied |

### BaseDifficultyModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Abstract base class for stateless modifiers with provider injection.

#### Protected Methods

##### NoChange(string)
Creates a "no change" modifier result.
```csharp
protected ModifierResult NoChange(string reason = "No adjustment needed")
```

### Built-in Modifiers (7 Total) ✅ COMPLETE

#### WinStreakModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Increases difficulty based on consecutive wins using **stateless provider data**.

**Provider Dependencies:**
- `IWinStreakProvider.GetWinStreak()` - Current consecutive wins

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| WinThreshold | 3 | Wins needed to trigger |
| StepSize | 0.5 | Difficulty increase per win |
| MaxBonus | 2.0 | Maximum difficulty increase |

**Method Signature:**
```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var winStreak = this.winStreakProvider.GetWinStreak();
    // Calculate bonus using provider data...
}
```

#### LossStreakModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Decreases difficulty based on consecutive losses using **stateless provider data**.

**Provider Dependencies:**
- `IWinStreakProvider.GetLossStreak()` - Current consecutive losses

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| LossThreshold | 2 | Losses needed to trigger |
| StepSize | 0.3 | Difficulty decrease per loss |
| MaxReduction | 1.5 | Maximum difficulty decrease |

**Method Signature:**
```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var lossStreak = this.winStreakProvider.GetLossStreak();
    // Calculate reduction using provider data...
}
```

#### TimeDecayModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Reduces difficulty based on time since last play using **stateless provider data**.

**Provider Dependencies:**
- `ITimeDecayProvider.GetTimeSinceLastPlay()` - Time since last session
- `ITimeDecayProvider.GetDaysAwayFromGame()` - Days away from game

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| DecayPerDay | 0.5 | Difficulty reduction per day |
| MaxDecay | 2.0 | Maximum total reduction |
| GraceHours | 6 | Hours before decay starts |

**Method Signature:**
```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var timeSinceLastPlay = this.timeDecayProvider.GetTimeSinceLastPlay();
    var daysAway = this.timeDecayProvider.GetDaysAwayFromGame();
    // Calculate decay using provider data...
}
```

#### RageQuitModifier

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Reduces difficulty when rage quit is detected using **stateless provider data**.

**Provider Dependencies:**
- `IRageQuitProvider.GetLastQuitType()` - How last session ended
- `IRageQuitProvider.GetRecentRageQuitCount()` - Recent rage quit count

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| RageQuitThreshold | 30 | Seconds to detect rage quit |
| RageQuitReduction | 1.0 | Reduction for rage quit |
| QuitReduction | 0.5 | Reduction for normal quit |

**Method Signature:**
```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var lastQuitType = this.rageQuitProvider.GetLastQuitType();
    var recentRageQuits = this.rageQuitProvider.GetRecentRageQuitCount();
    // Calculate adjustment using provider data...
}
```

#### CompletionRateModifier ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Adjusts difficulty based on overall player success rate using **multiple stateless providers**.

**Provider Dependencies:**
- `IWinStreakProvider.GetTotalWins()` - Total career wins
- `IWinStreakProvider.GetTotalLosses()` - Total career losses
- `ILevelProgressProvider.GetCompletionRate()` - Overall completion rate

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| LowCompletionThreshold | 0.4f | Below this rate = easier difficulty |
| HighCompletionThreshold | 0.7f | Above this rate = harder difficulty |
| LowCompletionAdjustment | -0.5f | Reduction for low completion rate |
| HighCompletionAdjustment | 0.5f | Increase for high completion rate |

**Method Signature:**
```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var totalWins = this.winStreakProvider.GetTotalWins();
    var totalLosses = this.winStreakProvider.GetTotalLosses();
    var completionRate = this.levelProgressProvider.GetCompletionRate();
    // Calculate adjustment using multiple providers...
}
```

#### LevelProgressModifier ✅ **ENHANCED WITH NEW FEATURES**

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Analyzes level progression patterns with enhanced time-based calculations using **stateless provider data**.

**Provider Dependencies:**
- `ILevelProgressProvider.GetCurrentLevel()` - Current level number
- `ILevelProgressProvider.GetAverageCompletionTime()` - Average completion time
- `ILevelProgressProvider.GetAttemptsOnCurrentLevel()` - Attempts on current level
- `ILevelProgressProvider.GetCurrentLevelDifficulty()` - Current level's difficulty
- `ILevelProgressProvider.GetCurrentLevelTimePercentage()` **🆕 NEW** - Performance percentage

**Enhanced Parameters (11 total):**
| Parameter | Default | Range | Description |
|-----------|---------|-------|-------------|
| HighAttemptsThreshold | 5 | 3-10 | Max attempts before reduction |
| DifficultyDecreasePerAttempt | 0.2f | 0.1f-0.5f | Reduction per excess attempt |
| FastCompletionRatio | 0.7f | 0.3f-0.9f | Fast completion threshold ratio |
| SlowCompletionRatio | 1.5f | 1.1f-2.0f | Slow completion threshold ratio |
| FastCompletionBonus | 0.3f | 0.1f-1f | Bonus for fast completion |
| SlowCompletionPenalty | 0.3f | 0.1f-1f | Reduction for slow completion |
| **MaxPenaltyMultiplier** | **1.0f** | **0.5f-1.5f** | **🆕 Maximum penalty multiplier** |
| **FastCompletionMultiplier** | **1.0f** | **0.1f-0.9f** | **🆕 Multiplier for fast completion bonus** |
| **HardLevelThreshold** | **3f** | **2f-5f** | **🆕 Minimum level difficulty for mastery bonus** |
| **MasteryCompletionRate** | **0.7f** | **0.5f-1f** | **🆕 Completion rate threshold for mastery** |
| **MasteryBonus** | **0.3f** | **0.1f-0.5f** | **🆕 Difficulty increase for mastering hard levels** |

**Method Signature:**
```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var currentLevel = this.levelProgressProvider.GetCurrentLevel();
    var attempts = this.levelProgressProvider.GetAttemptsOnCurrentLevel();
    var timePercentage = this.levelProgressProvider.GetCurrentLevelTimePercentage(); // 🆕 NEW
    var levelDifficulty = this.levelProgressProvider.GetCurrentLevelDifficulty();

    // Enhanced time-based calculation using provider data...
    if (timePercentage > 0)
    {
        if (timePercentage < this.config.FastCompletionRatio)
        {
            var bonus = this.config.FastCompletionBonus * (1.0f - timePercentage) * this.config.FastCompletionMultiplier;
            // Apply configurable multiplier...
        }
        // Additional logic...
    }
}
```

#### SessionPatternModifier ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Modifiers`

Detects session patterns and player frustration using **stateless provider data**.

**Provider Dependencies:**
- `IRageQuitProvider.GetAverageSessionDuration()` - Average session duration
- `IRageQuitProvider.GetRecentRageQuitCount()` - Recent rage quit count
- `IRageQuitProvider.GetCurrentSessionDuration()` - Current session duration

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| ShortSessionThreshold | 60f | Short session threshold (seconds) |
| LongSessionThreshold | 600f | Long session threshold (seconds) |
| RageQuitCountThreshold | 3 | Recent rage quits threshold |
| ShortSessionReduction | -0.2f | Reduction for short sessions |
| RageQuitReduction | -0.5f | Reduction for rage quit pattern |
| LongSessionBonus | 0.1f | Bonus for long engagement |

**Method Signature:**
```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var avgSessionDuration = this.rageQuitProvider.GetAverageSessionDuration();
    var recentRageQuits = this.rageQuitProvider.GetRecentRageQuitCount();
    var currentSessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();
    // Calculate adjustment using provider data...
}
```

---

## Calculators

### IDifficultyCalculator

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Calculators`

Interface for stateless difficulty calculation logic.

```csharp
DifficultyResult Calculate(IEnumerable<IDifficultyModifier> modifiers)
```

**🆕 STATELESS:** Takes only modifiers (no session data parameter). Modifiers get data from providers.

### DifficultyCalculator

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Calculators`

Default implementation that:
1. Runs all enabled modifiers using their stateless Calculate() methods
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

### IDifficultyDataProvider (Required)

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Base interface for difficulty storage. **This is the ONLY data the module stores.**

| Method | Returns | Description |
|--------|---------|-------------|
| GetCurrentDifficulty() | float | Gets current difficulty (only stored value) |
| SetCurrentDifficulty(float) | void | Sets current difficulty (only stored value) |

### IWinStreakProvider (Optional) - Using 4/4 methods ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Win/loss streak tracking interface for external game data.

| Method | Returns | Used By | Description |
|--------|---------|---------|-------------|
| GetWinStreak() | int | WinStreakModifier ✅ | Current consecutive wins |
| GetLossStreak() | int | LossStreakModifier ✅ | Current consecutive losses |
| GetTotalWins() | int | CompletionRateModifier ✅ | Total wins across all time |
| GetTotalLosses() | int | CompletionRateModifier ✅ | Total losses across all time |

### ITimeDecayProvider (Optional) - Using 3/3 methods ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Time-based tracking interface for external game data.

| Method | Returns | Used By | Description |
|--------|---------|---------|-------------|
| GetTimeSinceLastPlay() | TimeSpan | TimeDecayModifier ✅ | Time since last play session |
| GetLastPlayTime() | DateTime | TimeDecayModifier ✅ | Last play timestamp |
| GetDaysAwayFromGame() | int | TimeDecayModifier ✅ | Days away from game |

### IRageQuitProvider (Optional) - Using 4/4 methods ✅

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Rage quit detection interface for external game data.

| Method | Returns | Used By | Description |
|--------|---------|---------|-------------|
| GetLastQuitType() | QuitType | RageQuitModifier ✅ | How last session ended |
| GetCurrentSessionDuration() | float | SessionPatternModifier ✅ | Current session duration |
| GetRecentRageQuitCount() | int | RageQuitModifier ✅, SessionPatternModifier ✅ | Recent rage quit count |
| GetAverageSessionDuration() | float | SessionPatternModifier ✅ | Average session duration |

### ILevelProgressProvider (Optional) - Using 6/6 methods ✅ **ENHANCED**

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Providers`

Level progress tracking interface for external game data with enhanced time-based method.

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
| **IDifficultyDataProvider** | 2 | 2 | 100% | Core system (ONLY stored data) |
| **IWinStreakProvider** | 4 | 4 | 100% | WinStreakModifier, LossStreakModifier, CompletionRateModifier |
| **ITimeDecayProvider** | 3 | 3 | 100% | TimeDecayModifier |
| **IRageQuitProvider** | 4 | 4 | 100% | RageQuitModifier, SessionPatternModifier |
| **ILevelProgressProvider** | **6** | **6** | **100%** | **CompletionRateModifier, LevelProgressModifier** |

---

## Configuration

### DifficultyConfig

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Configuration`

**SINGLE ScriptableObject** for all difficulty settings. Contains ALL 7 modifier configurations.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| MinDifficulty | float | 1.0 | Minimum difficulty |
| MaxDifficulty | float | 10.0 | Maximum difficulty |
| DefaultDifficulty | float | 3.0 | Starting difficulty |
| MaxChangePerSession | float | 2.0 | Max change in one session |
| ModifierConfigs | ModifierConfigContainer | Container | ALL 7 modifier configurations |
| EnableDebugLogs | bool | false | Enable debug logging |

#### Methods

##### CreateDefault()
```csharp
public static DifficultyConfig CreateDefault()
```
Creates a new configuration with default settings for all 7 modifiers.

##### GetModifierConfig<T>(string)
```csharp
public T GetModifierConfig<T>(string modifierType) where T : class, IModifierConfig
```
Gets strongly-typed configuration for a specific modifier type.

### ModifierConfigContainer

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Configuration`

Container for all modifier configurations with polymorphic serialization using `[SerializeReference]`.

#### Methods

##### GetConfig<T>(string)
```csharp
public T GetConfig<T>(string modifierType) where T : class, IModifierConfig
```
Gets a strongly-typed configuration for a specific modifier type.

##### SetConfig(IModifierConfig)
```csharp
public void SetConfig(IModifierConfig config)
```
Adds or updates a modifier configuration.

##### InitializeDefaults()
```csharp
public void InitializeDefaults()
```
Initializes with default configurations for all 7 modifiers.

### BaseModifierConfig

**Namespace:** `TheOneStudio.DynamicUserDifficulty.Configuration`

Abstract base class for modifier configurations. Uses `[Serializable]` not `[CreateAssetMenu]`.

| Field | Type | Description |
|-------|------|-------------|
| enabled | bool | Whether enabled |
| priority | int | Execution order |
| ModifierType | string | Type identifier (abstract) |

#### Methods

##### CreateDefault()
```csharp
public abstract BaseModifierConfig CreateDefault()
```
Creates a default configuration instance for the specific modifier type.

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

---

## Usage Examples

### Basic Integration with Stateless Architecture
```csharp
public class GameController
{
    private readonly IDynamicDifficultyService difficultyService;

    public void StartLevel()
    {
        // STATELESS calculation - modifiers get data from providers automatically
        var result = difficultyService.CalculateDifficulty();
        difficultyService.ApplyDifficulty(result);

        // Use result.NewDifficulty to configure level
        ConfigureLevel(result.NewDifficulty);
    }

    public void OnLevelComplete(bool won, float time)
    {
        // Service handles everything via stateless providers
        difficultyService.OnLevelComplete(won, time);
    }
}
```

### Enhanced LevelProgressModifier Provider Implementation ✅
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

### Custom Stateless Modifier with Provider Methods ✅
```csharp
public class AdvancedPlayerModifier : BaseDifficultyModifier<AdvancedPlayerConfig>
{
    private readonly ILevelProgressProvider levelProvider;
    private readonly IRageQuitProvider rageProvider;

    public override string ModifierName => "AdvancedPlayer";

    public AdvancedPlayerModifier(AdvancedPlayerConfig config,
        ILevelProgressProvider levelProvider,
        IRageQuitProvider rageProvider) : base(config)
    {
        this.levelProvider = levelProvider;
        this.rageProvider = rageProvider;
    }

    /// <summary>
    /// STATELESS calculation - NO parameters!
    /// Gets ALL data from injected provider interfaces.
    /// </summary>
    public override ModifierResult Calculate()
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

        // Complex analysis using all available provider data
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

### Provider Implementation Example
```csharp
public class GameDataProvider :
    IWinStreakProvider,
    ITimeDecayProvider,
    IRageQuitProvider,
    ILevelProgressProvider
{
    private readonly UITemplateLevelDataController levelController;
    private readonly UITemplateGameSessionDataController sessionController;

    // IWinStreakProvider implementation
    public int GetWinStreak() => levelController.GetWinStreak();
    public int GetLossStreak() => levelController.GetLossStreak();
    public int GetTotalWins() => levelController.GetTotalWins();
    public int GetTotalLosses() => levelController.GetTotalLosses();

    // ITimeDecayProvider implementation
    public TimeSpan GetTimeSinceLastPlay() => sessionController.GetTimeSinceLastPlay();
    public DateTime GetLastPlayTime() => sessionController.GetLastPlayTime();
    public int GetDaysAwayFromGame() => sessionController.GetDaysAwayFromGame();

    // IRageQuitProvider implementation
    public QuitType GetLastQuitType() => sessionController.GetLastQuitType();
    public float GetCurrentSessionDuration() => sessionController.GetCurrentSessionDuration();
    public int GetRecentRageQuitCount() => sessionController.GetRecentRageQuitCount();
    public float GetAverageSessionDuration() => sessionController.GetAverageSessionDuration();

    // ILevelProgressProvider implementation
    public int GetCurrentLevel() => levelController.CurrentLevel;
    public float GetAverageCompletionTime() => levelController.GetAverageCompletionTime();
    public int GetAttemptsOnCurrentLevel() => levelController.GetAttemptsOnCurrentLevel();
    public float GetCompletionRate() => levelController.GetCompletionRate();
    public float GetCurrentLevelDifficulty() => levelController.GetCurrentLevelDifficulty();
    public float GetCurrentLevelTimePercentage() => levelController.GetCurrentLevelTimePercentage(); // 🆕 NEW
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
| NullReferenceException | Config not loaded or Provider not injected | Ensure DifficultyConfig in Resources and providers registered |
| ArgumentOutOfRangeException | Invalid difficulty | Check min/max bounds |
| InvalidOperationException | Service not initialized | Call Initialize() first |
| ArgumentNullException | Null modifier or data | Validate inputs before calls |

### Debug Mode

Enable debug logs in DifficultyConfig to see:
- Modifier calculations (stateless)
- Difficulty changes
- Provider method calls ✅
- Time-based calculations ✅
- Configuration loading
- Performance metrics

---

## Key Architectural Changes

### 🆕 **Stateless Architecture Benefits**

1. **Pure Calculation Engine**
   - Module only stores current difficulty (single float)
   - All other data comes from external providers
   - No data synchronization issues

2. **Provider Pattern**
   - Clean separation of concerns
   - Game data stays in game systems
   - Easy to test with mock providers

3. **Method Signature Change**
   ```csharp
   // OLD (with session data parameter)
   ModifierResult Calculate(PlayerSessionData sessionData)

   // NEW (stateless - no parameters)
   ModifierResult Calculate() // Gets data from injected providers
   ```

4. **Enhanced Provider Utilization**
   - 22/22 provider methods used (100% utilization)
   - Comprehensive player behavior analysis
   - All 7 modifiers fully implemented

5. **Single Configuration Asset**
   - Only ONE DifficultyConfig ScriptableObject needed
   - All 7 modifier configs embedded as [Serializable] classes
   - Simplified asset management

---

*API Version: 3.0.0*
*Last Updated: 2025-01-26*
*Architecture: STATELESS with Provider Pattern*
*Method Signature: Calculate() - NO PARAMETERS*
*Provider Utilization: 22/22 methods (100%)*
*Configuration: Single ScriptableObject with Embedded [Serializable] Configs*