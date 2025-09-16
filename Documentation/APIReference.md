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

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Core`

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

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models`

Stores player session information for difficulty calculation.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| CurrentDifficulty | float | 3.0f | Current difficulty level |
| WinStreak | int | 0 | Consecutive wins |
| LossStreak | int | 0 | Consecutive losses |
| LastPlayTime | DateTime | Now | Last time player played |
| LastSession | SessionInfo | null | Information about last session |
| RecentSessions | Queue<SessionInfo> | Empty(10) | Queue of last 10 sessions |

### SessionInfo

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models`

Information about a single game session.

| Field | Type | Description |
|-------|------|-------------|
| StartTime | DateTime | When session started |
| EndTime | DateTime | When session ended |
| EndType | SessionEndType | How session ended |
| LevelId | int | Level played |
| PlayDuration | float | Duration in seconds |
| Won | bool | Whether level was won |

### SessionEndType

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models`

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

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models`

Result from a single modifier calculation.

| Field | Type | Description |
|-------|------|-------------|
| ModifierName | string | Name of the modifier |
| Value | float | Difficulty adjustment value |
| Reason | string | Human-readable reason |
| Metadata | Dictionary<string, object> | Additional data |

### DifficultyResult

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models`

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

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers`

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

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers`

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

### Built-in Modifiers

#### WinStreakModifier

Increases difficulty based on consecutive wins.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| WinThreshold | 3 | Wins needed to trigger |
| StepSize | 0.5 | Difficulty increase per win |
| MaxBonus | 2.0 | Maximum difficulty increase |

#### LossStreakModifier

Decreases difficulty based on consecutive losses.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| LossThreshold | 2 | Losses needed to trigger |
| StepSize | 0.3 | Difficulty decrease per loss |
| MaxReduction | 1.5 | Maximum difficulty decrease |

#### TimeDecayModifier

Reduces difficulty based on time since last play.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| DecayPerDay | 0.5 | Difficulty reduction per day |
| MaxDecay | 2.0 | Maximum total reduction |
| GraceHours | 6 | Hours before decay starts |

#### RageQuitModifier

Reduces difficulty when rage quit is detected.

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| RageQuitThreshold | 30 | Seconds to detect rage quit |
| RageQuitReduction | 1.0 | Reduction for rage quit |
| QuitReduction | 0.5 | Reduction for normal quit |

---

## Calculators

### IDifficultyCalculator

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Calculators`

Interface for difficulty calculation logic.

```csharp
DifficultyResult Calculate(
    PlayerSessionData sessionData,
    IEnumerable<IDifficultyModifier> modifiers)
```

### DifficultyCalculator

Default implementation that:
1. Runs all enabled modifiers
2. Aggregates results
3. Clamps to valid range
4. Applies max change limit

### ModifierAggregator

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Calculators`

Aggregates multiple modifier results.

```csharp
float Aggregate(List<ModifierResult> results)
```

Default: Sums all modifier values

---

## Providers

### ISessionDataProvider

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Providers`

Interface for session data persistence.

| Method | Returns | Description |
|--------|---------|-------------|
| GetCurrentSession() | PlayerSessionData | Gets current session data |
| SaveSession(PlayerSessionData) | void | Saves session data |
| UpdateWinStreak(int) | void | Updates win streak |
| UpdateLossStreak(int) | void | Updates loss streak |
| RecordSessionEnd(SessionEndType) | void | Records how session ended |

### SessionDataProvider

Default implementation using Unity PlayerPrefs.

**PlayerPrefs Keys:**
- `DUD_CurrentDifficulty`
- `DUD_WinStreak`
- `DUD_LossStreak`
- `DUD_LastPlayTime`
- `DUD_SessionData`

---

## Configuration

### DifficultyConfig

**Namespace:** `TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration`

ScriptableObject for difficulty settings.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| MinDifficulty | float | 1.0 | Minimum difficulty |
| MaxDifficulty | float | 10.0 | Maximum difficulty |
| DefaultDifficulty | float | 3.0 | Starting difficulty |
| MaxChangePerSession | float | 2.0 | Max change in one session |
| ModifierConfigs | List<ModifierConfig> | Empty | Modifier configurations |
| EnableDebugLogs | bool | false | Enable debug logging |

### ModifierConfig

Configuration for individual modifiers.

| Field | Type | Description |
|-------|------|-------------|
| ModifierType | string | Type identifier |
| Enabled | bool | Whether enabled |
| Priority | int | Execution order |
| ResponseCurve | AnimationCurve | Value transformation |
| Parameters | List<ModifierParameter> | Custom parameters |

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

### Custom Modifier
```csharp
public class SpeedModifier : BaseDifficultyModifier
{
    public override string ModifierName => "Speed";

    public override ModifierResult Calculate(PlayerSessionData data)
    {
        // Custom logic
        var avgTime = CalculateAverageTime(data.RecentSessions);
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

### Debug Mode

Enable debug logs in DifficultyConfig to see:
- Modifier calculations
- Difficulty changes
- Session tracking
- Performance metrics

---

*API Version: 1.0.0*
*Last Updated: 2025-09-16*