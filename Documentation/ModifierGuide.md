# Dynamic User Difficulty - Modifier Development Guide

## Table of Contents
1. [Overview](#overview)
2. [Built-in Modifiers (7 Total)](#built-in-modifiers-7-total)
3. [Creating Custom Modifiers](#creating-custom-modifiers)
4. [Modifier Lifecycle](#modifier-lifecycle)
5. [Configuration System](#configuration-system)
6. [Best Practices](#best-practices)
7. [Example Modifiers](#example-modifiers)
8. [Testing Modifiers](#testing-modifiers)

## Overview

Modifiers are the core extensibility mechanism of the Dynamic User Difficulty system. Each modifier calculates a difficulty adjustment based on specific player behavior or game state using a **stateless architecture**. The system includes **7 comprehensive modifiers** covering all aspects of player behavior analysis.

### Key Concepts
- **Stateless**: Modifiers are pure calculation engines with no internal state
- **Provider-Based**: Uses external data through clean provider interfaces
- **Type-Safe Configurable**: Parameters via strongly-typed [Serializable] classes embedded in ONE ScriptableObject
- **Prioritized**: Execution order controlled by priority
- **Toggleable**: Can be enabled/disabled at runtime
- **Pure Functions**: Calculate() method takes NO parameters and gets all data from providers

### ⚠️ **CRITICAL: Stateless Architecture**
**All 7 modifier implementations use a completely stateless approach where the Calculate() method takes NO parameters and all data comes from external providers.**

## Built-in Modifiers (7 Total)

### 1. WinStreakModifier ✅ STATELESS
**Purpose**: Increases difficulty based on consecutive wins
**Data Source**: `IWinStreakProvider.GetWinStreak()`
**Config**: `WinStreakConfig` ([Serializable] class, NOT ScriptableObject)

```csharp
public class WinStreakModifier : BaseDifficultyModifier<WinStreakConfig>
{
    private readonly IWinStreakProvider winStreakProvider;

    public override ModifierResult Calculate()
    {
        // Get data from provider - stateless approach
        var winStreak = this.winStreakProvider.GetWinStreak();

        // Use strongly-typed properties instead of string parameters
        var winThreshold = this.config.WinThreshold;
        var stepSize = this.config.StepSize;
        var maxBonus = this.config.MaxBonus;

        if (winStreak < winThreshold)
            return ModifierResult.NoChange();

        var bonus = Math.Min(
            (winStreak - winThreshold + 1) * stepSize,
            maxBonus
        );

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = bonus,
            Reason = $"Win streak bonus ({winStreak} consecutive wins)",
            Metadata =
            {
                ["streak"] = winStreak,
                ["threshold"] = winThreshold,
                ["applied"] = bonus > 0f
            }
        };
    }
}
```

### 2. LossStreakModifier ✅ STATELESS
**Purpose**: Decreases difficulty based on consecutive losses
**Data Source**: `IWinStreakProvider.GetLossStreak()`
**Config**: `LossStreakConfig` ([Serializable] class)

```csharp
public class LossStreakModifier : BaseDifficultyModifier<LossStreakConfig>
{
    private readonly IWinStreakProvider winStreakProvider;

    public override ModifierResult Calculate()
    {
        // Get data from provider - stateless approach
        var lossStreak = this.winStreakProvider.GetLossStreak();

        // Use strongly-typed properties
        var lossThreshold = this.config.LossThreshold;
        var stepSize = this.config.StepSize;
        var maxReduction = this.config.MaxReduction;

        if (lossStreak < lossThreshold)
            return ModifierResult.NoChange();

        var reduction = -Math.Min(
            (lossStreak - lossThreshold + 1) * stepSize,
            maxReduction
        );

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = reduction,
            Reason = $"Loss streak compensation ({lossStreak} consecutive losses)",
            Metadata =
            {
                ["streak"] = lossStreak,
                ["threshold"] = lossThreshold,
                ["applied"] = reduction < 0f
            }
        };
    }
}
```

### 3. TimeDecayModifier ✅ STATELESS
**Purpose**: Reduces difficulty for returning players
**Data Sources**:
- `ITimeDecayProvider.GetTimeSinceLastPlay()`
- `ITimeDecayProvider.GetLastPlayTime()`
- `ITimeDecayProvider.GetDaysAwayFromGame()`
**Config**: `TimeDecayConfig` ([Serializable] class)

```csharp
public class TimeDecayModifier : BaseDifficultyModifier<TimeDecayConfig>
{
    private readonly ITimeDecayProvider timeDecayProvider;

    public override ModifierResult Calculate()
    {
        // Get data from providers - stateless approach
        // Use all three provider methods for comprehensive tracking
        var lastPlayTime = this.timeDecayProvider.GetLastPlayTime();
        var timeSincePlay = this.timeDecayProvider.GetTimeSinceLastPlay();
        var daysAway = this.timeDecayProvider.GetDaysAwayFromGame();

        var hoursSincePlay = timeSincePlay.TotalHours;

        // Use strongly-typed properties
        var decayPerDay = this.config.DecayPerDay;
        var maxDecay = this.config.MaxDecay;
        var graceHours = this.config.GraceHours;

        if (hoursSincePlay <= graceHours)
            return ModifierResult.NoChange("Recently played");

        // Use provider's daysAway value for more accurate calculation
        float effectiveDays = daysAway;

        // If grace period hasn't passed for a full day, adjust
        if (daysAway == 0 && hoursSincePlay > graceHours)
        {
            var effectiveHours = hoursSincePlay - graceHours;
            effectiveDays = effectiveHours / 24f;
        }

        var decay = -Math.Min(effectiveDays * decayPerDay, maxDecay);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = decay,
            Reason = $"Time decay ({daysAway} days since last play)",
            Metadata =
            {
                ["last_play_time"] = lastPlayTime.ToString("O"),
                ["time_since_play"] = timeSincePlay.ToString(),
                ["days_away_provider"] = daysAway,
                ["hours_away"] = hoursSincePlay,
                ["grace_hours"] = graceHours,
                ["applied"] = decay < 0f
            }
        };
    }
}
```

### 4. RageQuitModifier ✅ STATELESS
**Purpose**: Detects and compensates for rage quits
**Data Sources**:
- `IRageQuitProvider.GetLastQuitType()`
- `IRageQuitProvider.GetCurrentSessionDuration()`
- `IRageQuitProvider.GetRecentRageQuitCount()`
**Config**: `RageQuitConfig` ([Serializable] class)

```csharp
public class RageQuitModifier : BaseDifficultyModifier<RageQuitConfig>
{
    private readonly IRageQuitProvider rageQuitProvider;

    public override ModifierResult Calculate()
    {
        // Get data from providers - stateless approach
        var lastQuitType = this.rageQuitProvider.GetLastQuitType();
        var sessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();
        var recentRageQuits = this.rageQuitProvider.GetRecentRageQuitCount();

        // Use strongly-typed properties
        var rageQuitThreshold = this.config.RageQuitThreshold;
        var rageQuitReduction = this.config.RageQuitReduction;
        var quitReduction = this.config.QuitReduction;
        var midPlayReduction = this.config.MidPlayReduction;

        var value = 0f;
        var reason = "No quit penalty";

        // Apply different penalties based on quit type
        switch (lastQuitType)
        {
            case QuitType.RageQuit:
                value = -rageQuitReduction;
                reason = $"Rage quit detected (duration: {sessionDuration:F0}s, recent quits: {recentRageQuits})";
                break;

            case QuitType.Normal:
                value = -quitReduction;
                reason = $"Normal quit detected (duration: {sessionDuration:F0}s)";
                break;

            case QuitType.MidPlay:
                value = -midPlayReduction;
                reason = $"Mid-play quit detected (duration: {sessionDuration:F0}s)";
                break;
        }

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = value,
            Reason = reason,
            Metadata =
            {
                ["last_quit_type"] = lastQuitType.ToString(),
                ["session_duration"] = sessionDuration,
                ["recent_rage_quits"] = recentRageQuits,
                ["rage_quit_detected"] = lastQuitType == QuitType.RageQuit
            }
        };
    }
}
```

### 5. CompletionRateModifier ✅ STATELESS
**Purpose**: Adjusts difficulty based on overall player success rate
**Data Sources**:
- `IWinStreakProvider.GetTotalWins()`
- `IWinStreakProvider.GetTotalLosses()`
- `ILevelProgressProvider.GetCompletionRate()`
**Config**: `CompletionRateConfig` ([Serializable] class)

```csharp
public class CompletionRateModifier : BaseDifficultyModifier<CompletionRateConfig>
{
    private readonly IWinStreakProvider winStreakProvider;
    private readonly ILevelProgressProvider levelProgressProvider;

    public override ModifierResult Calculate()
    {
        // Get data from providers - stateless approach
        var totalWins = this.winStreakProvider.GetTotalWins();
        var totalLosses = this.winStreakProvider.GetTotalLosses();
        var totalAttempts = totalWins + totalLosses;

        // Check minimum attempts requirement
        if (totalAttempts < this.config.MinAttemptsRequired)
        {
            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = 0f,
                Reason = $"Not enough attempts ({totalAttempts}/{this.config.MinAttemptsRequired})",
                Metadata =
                {
                    ["totalAttempts"] = totalAttempts,
                    ["required"] = this.config.MinAttemptsRequired
                }
            };
        }

        // Calculate completion rate
        var completionRate = totalAttempts > 0 ? (float)totalWins / totalAttempts : 0.5f;

        // Also get level-specific completion rate for more accurate assessment
        var levelCompletionRate = this.levelProgressProvider.GetCompletionRate();

        // Weighted average of overall and level-specific rates
        var weightedRate = completionRate * (1f - this.config.TotalStatsWeight) +
                          levelCompletionRate * this.config.TotalStatsWeight;

        var value = 0f;
        var reason = "Completion rate normal";

        if (weightedRate < this.config.LowCompletionThreshold)
        {
            value = -this.config.LowCompletionDecrease;
            reason = $"Low completion rate ({weightedRate:P0}) - decreasing difficulty";
        }
        else if (weightedRate > this.config.HighCompletionThreshold)
        {
            value = this.config.HighCompletionIncrease;
            reason = $"High completion rate ({weightedRate:P0}) - increasing difficulty";
        }

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = value,
            Reason = reason,
            Metadata =
            {
                ["completionRate"] = completionRate,
                ["levelCompletionRate"] = levelCompletionRate,
                ["weightedRate"] = weightedRate,
                ["totalWins"] = totalWins,
                ["totalLosses"] = totalLosses,
                ["applied"] = Math.Abs(value) > 0f
            }
        };
    }
}
```

### 6. LevelProgressModifier ✅ STATELESS **ENHANCED WITH TIME-BASED ANALYSIS**

**Purpose**: Analyzes level progression patterns with enhanced time-based calculations and configurable performance thresholds
**Data Sources**:
- `ILevelProgressProvider.GetCurrentLevel()`
- `ILevelProgressProvider.GetAverageCompletionTime()`
- `ILevelProgressProvider.GetAttemptsOnCurrentLevel()`
- `ILevelProgressProvider.GetCurrentLevelDifficulty()`
- `ILevelProgressProvider.GetCompletionRate()`
- **`ILevelProgressProvider.GetCurrentLevelTimePercentage()` 🆕 NEW**

**Config**: `LevelProgressConfig` ([Serializable] class) **with enhanced parameters**

#### **Enhanced Configuration Parameters**

**Standard Parameters**:
- `HighAttemptsThreshold` (default: 5) - Maximum attempts before reduction
- `DifficultyDecreasePerAttempt` (default: 0.2f) - Reduction per excess attempt
- `FastCompletionRatio` (default: 0.7f) - Fast completion threshold ratio
- `SlowCompletionRatio` (default: 1.5f) - Slow completion threshold ratio
- `FastCompletionBonus` (default: 0.3f) - Bonus for fast completion
- `SlowCompletionPenalty` (default: 0.3f) - Reduction for slow completion

**🆕 Enhanced Parameters**:
- `MaxPenaltyMultiplier` (default: 1.0f) - Maximum penalty multiplier for slow completion
- `FastCompletionMultiplier` (default: 1.0f) - Multiplier for fast completion bonus
- `HardLevelThreshold` (default: 3f) - Minimum level difficulty for mastery bonus
- `MasteryCompletionRate` (default: 0.7f) - Completion rate threshold for mastery
- `MasteryBonus` (default: 0.3f) - Difficulty increase for mastering hard levels
- `EasyLevelThreshold` (default: 2f) - Maximum level difficulty for struggle detection
- `StruggleCompletionRate` (default: 0.3f) - Completion rate threshold for struggle
- `StrugglePenalty` (default: 0.3f) - Difficulty decrease for struggling on easy levels

#### **Enhanced Implementation with PercentUsingTimeToComplete Integration**

```csharp
public class LevelProgressModifier : BaseDifficultyModifier<LevelProgressConfig>
{
    private readonly ILevelProgressProvider levelProgressProvider;

    public override ModifierResult Calculate()
    {
        // Get data from providers - stateless approach
        var value = 0f;
        var reasons = new List<string>();

        // 1. Check attempts on current level
        var attempts = this.levelProgressProvider.GetAttemptsOnCurrentLevel();
        if (attempts > this.config.HighAttemptsThreshold)
        {
            var attemptsAdjustment = -(attempts - this.config.HighAttemptsThreshold) *
                                   this.config.DifficultyDecreasePerAttempt;
            value += attemptsAdjustment;
            reasons.Add($"High attempts ({attempts})");
        }

        // 2. 🆕 PRIMARY: Use PercentUsingTimeToComplete from UITemplate
        var timePercentage = this.levelProgressProvider.GetCurrentLevelTimePercentage();
        if (timePercentage > 0)
        {
            if (timePercentage < this.config.FastCompletionRatio)
            {
                // Completing levels faster than expected (< 100% of time limit/average)
                var bonus = this.config.FastCompletionBonus * (1.0f - timePercentage) *
                           this.config.FastCompletionMultiplier; // 🆕 Configurable multiplier
                value += bonus;
                reasons.Add($"Fast completion ({timePercentage:P0} of expected time)");
            }
            else if (timePercentage > this.config.SlowCompletionRatio)
            {
                // Taking longer than expected (> 100% of time limit/average)
                var penalty = this.config.SlowCompletionPenalty *
                             Math.Min(timePercentage - 1.0f, this.config.MaxPenaltyMultiplier); // 🆕 Configurable cap
                value -= penalty;
                reasons.Add($"Slow completion ({timePercentage:P0} of expected time)");
            }
        }

        // 3. Check overall level progression speed
        var currentLevel = this.levelProgressProvider.GetCurrentLevel();
        var avgCompletionTime = this.levelProgressProvider.GetAverageCompletionTime();

        if (avgCompletionTime > 0 && currentLevel > 0)
        {
            // Use average completion time to estimate progression rate
            var estimatedHoursPlayed = (currentLevel * avgCompletionTime) / 3600f;
            var expectedLevel = (int)(estimatedHoursPlayed * this.config.ExpectedLevelsPerHour);

            if (expectedLevel > 0)
            {
                var levelDifference = currentLevel - expectedLevel;
                var progressionAdjustment = levelDifference * this.config.LevelProgressionFactor;

                if (Math.Abs(progressionAdjustment) > 0.01f)
                {
                    value += progressionAdjustment;
                    if (levelDifference > 0)
                        reasons.Add($"Fast progression (L{currentLevel} vs expected L{expectedLevel})");
                    else
                        reasons.Add($"Slow progression (L{currentLevel} vs expected L{expectedLevel})");
                }
            }
        }

        // 4. 🆕 NEW: Check level difficulty vs player performance using configurable thresholds
        var levelDifficulty = this.levelProgressProvider.GetCurrentLevelDifficulty();
        var completionRate = this.levelProgressProvider.GetCompletionRate();

        // If player is succeeding on hard levels, increase difficulty further
        if (levelDifficulty >= this.config.HardLevelThreshold &&
            completionRate > this.config.MasteryCompletionRate)
        {
            value += this.config.MasteryBonus;
            reasons.Add("Mastering hard levels");
        }
        // If player is struggling on easy levels, decrease difficulty more
        else if (levelDifficulty <= this.config.EasyLevelThreshold &&
                 completionRate < this.config.StruggleCompletionRate)
        {
            value -= this.config.StrugglePenalty;
            reasons.Add("Struggling on easy levels");
        }

        var finalReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Normal level progression";

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = value,
            Reason = finalReason,
            Metadata = {
                ["attempts"] = attempts,
                ["currentLevel"] = currentLevel,
                ["levelDifficulty"] = levelDifficulty,
                ["completionRate"] = completionRate,
                ["timePercentage"] = timePercentage, // 🆕 NEW
                ["avgCompletionTime"] = avgCompletionTime,
                ["applied"] = Math.Abs(value) > 0f,
            }
        };
    }
}
```

### 7. SessionPatternModifier ✅ STATELESS
**Purpose**: Detects session patterns and player frustration through duration and behavior analysis
**Data Sources**:
- `IRageQuitProvider.GetAverageSessionDuration()`
- `IRageQuitProvider.GetRecentRageQuitCount()`
- `IRageQuitProvider.GetCurrentSessionDuration()`
- `IRageQuitProvider.GetLastQuitType()`
**Config**: `SessionPatternConfig` ([Serializable] class)

```csharp
public class SessionPatternModifier : BaseDifficultyModifier<SessionPatternConfig>
{
    private readonly IRageQuitProvider rageQuitProvider;

    public override ModifierResult Calculate()
    {
        // Get data from providers - stateless approach
        var value = 0f;
        var reasons = new List<string>();

        // 1. Check current session duration
        var currentSessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();
        if (currentSessionDuration < this.config.VeryShortSessionThreshold && currentSessionDuration > 0)
        {
            value -= this.config.VeryShortSessionDecrease;
            reasons.Add($"Very short session ({currentSessionDuration:F0}s)");
        }

        // 2. Check average session duration pattern
        var avgSessionDuration = this.rageQuitProvider.GetAverageSessionDuration();
        if (avgSessionDuration > 0 && avgSessionDuration < this.config.MinNormalSessionDuration)
        {
            var durationRatio = avgSessionDuration / this.config.MinNormalSessionDuration;
            var durationAdjustment = -(1f - durationRatio) * this.config.ConsistentShortSessionsDecrease;
            value += durationAdjustment;
            reasons.Add($"Short avg sessions ({avgSessionDuration:F0}s)");
        }

        // 3. Use rage quit data from provider
        var lastQuitType = this.rageQuitProvider.GetLastQuitType();
        var recentRageQuitCount = this.rageQuitProvider.GetRecentRageQuitCount();

        // Check for rage quit patterns
        if (recentRageQuitCount >= this.config.RageQuitCountThreshold)
        {
            var rageQuitPenalty = this.config.RageQuitPatternDecrease * this.config.RageQuitPenaltyMultiplier;
            value -= rageQuitPenalty;
            reasons.Add($"Recent rage quits ({recentRageQuitCount})");
        }

        // 4. Check last quit type for mid-level quit pattern
        if (lastQuitType == QuitType.MidPlay)
        {
            value -= this.config.MidLevelQuitDecrease;
            reasons.Add("Mid-level quit detected");
        }

        var finalReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Normal session patterns";

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = value,
            Reason = finalReason,
            Metadata =
            {
                ["currentSessionDuration"] = currentSessionDuration,
                ["avgSessionDuration"] = avgSessionDuration,
                ["rageQuitCount"] = recentRageQuitCount,
                ["lastQuitType"] = lastQuitType.ToString(),
                ["applied"] = Math.Abs(value) > 0f
            }
        };
    }
}
```

## Creating Custom Modifiers

### ⚠️ **CRITICAL: Stateless Configuration Pattern**

**The configuration system uses a single ScriptableObject with embedded [Serializable] config classes.**

### Step 1: Create the Configuration Class ([Serializable], NOT [CreateAssetMenu])

```csharp
using System;
using UnityEngine;
using TheOneStudio.DynamicUserDifficulty.Configuration;

namespace YourNamespace
{
    /// <summary>
    /// Configuration for your custom modifier.
    /// [Serializable] class embedded in DifficultyConfig, NOT a separate ScriptableObject.
    /// </summary>
    [Serializable]
    public class SpeedBonusConfig : BaseModifierConfig
    {
        [SerializeField] private float timeThreshold = 60f;
        [SerializeField] private float bonusAmount = 0.5f;
        [SerializeField] private bool enableSpecialMode = false;

        // Type-safe property access
        public float TimeThreshold => this.timeThreshold;
        public float BonusAmount => this.bonusAmount;
        public bool EnableSpecialMode => this.enableSpecialMode;

        public override string ModifierType => "SpeedBonus";

        public override BaseModifierConfig CreateDefault()
        {
            var config = new SpeedBonusConfig();
            config.timeThreshold = 60f;
            config.bonusAmount = 0.5f;
            config.enableSpecialMode = false;
            return config;
        }
    }
}
```

### Step 2: Create the Modifier Class (Stateless)

```csharp
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace YourNamespace
{
    public class SpeedBonusModifier : BaseDifficultyModifier<SpeedBonusConfig>
    {
        private readonly ILevelProgressProvider levelProvider;

        public override string ModifierName => "SpeedBonus";

        public SpeedBonusModifier(SpeedBonusConfig config, ILevelProgressProvider provider)
            : base(config)
        {
            this.levelProvider = provider;
        }

        public override ModifierResult Calculate()
        {
            // Get data from provider - stateless approach
            var avgTime = this.levelProvider.GetAverageCompletionTime();
            var timePercentage = this.levelProvider.GetCurrentLevelTimePercentage(); // 🆕 NEW

            // Check condition using type-safe config properties
            if (avgTime <= 0 || avgTime >= this.config.TimeThreshold)
                return ModifierResult.NoChange("No speed bonus applicable");

            var adjustment = this.config.BonusAmount;

            // 🆕 Use new time percentage for more accurate calculation
            if (timePercentage > 0 && timePercentage < 0.8f) // Faster than 80% of expected time
            {
                adjustment *= (1.0f - timePercentage); // Scale bonus based on speed
            }

            // Apply special mode if enabled
            if (this.config.EnableSpecialMode)
                adjustment *= 1.5f;

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = adjustment,
                Reason = $"Fast completion bonus (avg: {avgTime:F0}s, {timePercentage:P0} of expected)",
                Metadata =
                {
                    ["average_time"] = avgTime,
                    ["time_percentage"] = timePercentage, // 🆕 NEW
                    ["threshold"] = this.config.TimeThreshold,
                    ["special_mode"] = this.config.EnableSpecialMode
                }
            };
        }
    }
}
```

### Step 3: Update ModifierConfigContainer

```csharp
// In ModifierConfigContainer.cs InitializeDefaults() method
public void InitializeDefaults()
{
    this.configs = new()
    {
        // Existing 7 modifiers...
        (WinStreakConfig)new WinStreakConfig().CreateDefault(),
        (LossStreakConfig)new LossStreakConfig().CreateDefault(),
        (TimeDecayConfig)new TimeDecayConfig().CreateDefault(),
        (RageQuitConfig)new RageQuitConfig().CreateDefault(),
        (CompletionRateConfig)new CompletionRateConfig().CreateDefault(),
        (LevelProgressConfig)new LevelProgressConfig().CreateDefault(),
        (SessionPatternConfig)new SessionPatternConfig().CreateDefault(),

        // Add your custom modifier
        (SpeedBonusConfig)new SpeedBonusConfig().CreateDefault()
    };
}
```

### Step 4: Register in DI Module

```csharp
// In DynamicDifficultyModule.cs RegisterModifiers() method
private void RegisterModifiers(IContainerBuilder builder)
{
    // Existing modifiers...

    // Add your custom modifier
    var speedBonusConfig = this.configContainer.GetConfig<SpeedBonusConfig>("SpeedBonus")
        ?? new SpeedBonusConfig().CreateDefault() as SpeedBonusConfig;

    builder.Register<SpeedBonusModifier>(Lifetime.Singleton)
           .WithParameter(speedBonusConfig)
           .As<IDifficultyModifier>();
}
```

### Step 5: Add Constants

```csharp
// In DifficultyConstants.cs
public const string MODIFIER_TYPE_SPEED_BONUS = "SpeedBonus";
```

## Modifier Lifecycle

### 1. Initialization
```csharp
public SpeedBonusModifier(SpeedBonusConfig config, IYourProvider provider) : base(config)
{
    // One-time setup
    // Config and provider are injected and stored
    this.provider = provider;
}
```

### 2. Calculation Phase (Stateless)
```csharp
public override ModifierResult Calculate()
{
    // Called each time difficulty is calculated
    // Should be stateless and pure - NO parameters taken
    // Use provider to get external data
    // Use this.config.PropertyName for type-safe access
    // Return adjustment value
}
```

### 3. Application Phase
```csharp
public override void OnApplied(DifficultyResult result)
{
    // Optional: Called after difficulty is applied
    // Can trigger side effects
    // Update analytics, etc.
}
```

## Configuration System

### ⚠️ **CRITICAL: Single ScriptableObject Architecture**

**Configuration Structure:**

1. **DifficultyConfig** (ScriptableObject) - Main configuration container
   - **This is the ONLY ScriptableObject** created as an asset
   - Contains all settings including embedded modifier configurations

2. **ModifierConfigContainer** - Container holding all modifier configs
   - Embedded within DifficultyConfig using `[SerializeReference]`
   - Enables polymorphic serialization of different config types

3. **Individual Config Classes** - All modifier configurations
   - **These are [Serializable] classes, NOT separate ScriptableObjects**
   - Embedded within the single DifficultyConfig asset

### Type-Safe Configuration Access

```csharp
// ✅ CORRECT: Type-safe property access
float threshold = this.config.TimeThreshold;
float adjustment = this.config.BonusAmount;
bool enableMode = this.config.EnableSpecialMode;

// ✅ CORRECT: Multiple properties
var settings = new
{
    MinValue = this.config.MinValue,
    MaxValue = this.config.MaxValue,
    StepSize = this.config.StepSize
};

// ❌ INCORRECT: Old string-based approach (DO NOT USE)
// float threshold = config.GetParameter("TimeThreshold", 60f);
```

### Unity Inspector Integration

**When you select the single DifficultyConfig asset in Unity:**
- All 7+ modifier configurations appear in one Inspector
- Each config section is collapsible and clearly labeled
- Type-safe serialization ensures proper validation
- No need to manage multiple separate ScriptableObject assets

### Dynamic Configuration

```csharp
public override ModifierResult Calculate()
{
    // Check if should run
    if (!ShouldRun())
        return ModifierResult.NoChange("Conditions not met");

    // Get dynamic threshold based on provider data
    var dynamicThreshold = GetDynamicThreshold();

    // Calculate with dynamic values
    return CalculateWithThreshold(dynamicThreshold);
}

private float GetDynamicThreshold()
{
    // Example: Scale threshold with provider data
    var baseThreshold = this.config.BaseThreshold;
    var scaleFactor = this.config.ScaleFactor;

    // Use provider to get current context
    int playerLevel = this.levelProvider.GetCurrentLevel();

    return baseThreshold + (playerLevel * scaleFactor);
}
```

## Best Practices

### 1. Keep Calculations Pure and Stateless

✅ **Good: Pure function with provider data**
```csharp
public override ModifierResult Calculate()
{
    // Read from providers (external data)
    var winRate = this.levelProvider.GetCompletionRate();
    var avgTime = this.levelProvider.GetAverageCompletionTime();
    var timePercentage = this.levelProvider.GetCurrentLevelTimePercentage(); // 🆕 NEW

    // Pure calculation
    var adjustment = CalculateAdjustment(winRate, avgTime, timePercentage);

    // Deterministic result
    return new ModifierResult { Value = adjustment };
}
```

❌ **Bad: Side effects and non-deterministic behavior**
```csharp
public override ModifierResult Calculate()
{
    // Don't do this!
    SaveToDatabase(sessionData);  // Side effect
    var random = Random.Range(0, 1);  // Non-deterministic
    sessionData.WinStreak++;      // Modifying external state
}
```

### 2. Use Meaningful Names

```csharp
// Good: Descriptive names
public override string ModifierName => "CompletionRateAnalysis";
public override string ModifierName => "SessionPatternDetection";

// Bad: Generic names
public override string ModifierName => "Modifier1";
public override string ModifierName => "CustomMod";
```

### 3. Provide Clear Reasons

```csharp
// Good: Specific reason with data
reason = $"Low completion rate ({completionRate:P0}) - reducing difficulty by {adjustment:F2}";
reason = $"Fast completion pattern ({avgTime:F0}s avg) - increasing challenge";

// Bad: Generic reason
reason = "Difficulty changed";
reason = "Adjustment applied";
```

### 4. Handle Edge Cases

```csharp
public override ModifierResult Calculate()
{
    // Check for sufficient data
    var totalGames = this.winStreakProvider.GetTotalWins() + this.winStreakProvider.GetTotalLosses();
    if (totalGames < 5)
    {
        return ModifierResult.NoChange("Insufficient data for analysis");
    }

    // Validate provider data
    var completionRate = this.levelProvider.GetCompletionRate();
    if (completionRate < 0 || completionRate > 1)
    {
        Debug.LogWarning($"Invalid completion rate: {completionRate}");
        return ModifierResult.NoChange("Invalid completion rate data");
    }

    // 🆕 Validate new time percentage data
    var timePercentage = this.levelProvider.GetCurrentLevelTimePercentage();
    if (timePercentage < 0)
    {
        Debug.LogWarning($"Invalid time percentage: {timePercentage}");
        return ModifierResult.NoChange("Invalid time percentage data");
    }

    // Normal calculation...
}
```

### 5. Use Metadata for Debugging

```csharp
return new ModifierResult
{
    ModifierName = ModifierName,
    Value = adjustment,
    Reason = reason,
    Metadata =
    {
        ["completion_rate"] = completionRate,
        ["total_games"] = totalGames,
        ["avg_time"] = avgTime,
        ["time_percentage"] = timePercentage, // 🆕 NEW
        ["threshold_used"] = this.config.Threshold,
        ["calculation_time"] = DateTime.Now
    }
};
```

### 6. Type-Safe Configuration Pattern

```csharp
// ✅ CORRECT: Create [Serializable] config class
[Serializable]
public class MyModifierConfig : BaseModifierConfig
{
    [SerializeField] private float threshold = 5f;

    public float Threshold => this.threshold; // Type-safe property
    public override string ModifierType => "MyModifier";

    public override BaseModifierConfig CreateDefault()
    {
        return new MyModifierConfig { threshold = 5f };
    }
}

// ❌ INCORRECT: DO NOT create separate ScriptableObject
// [CreateAssetMenu(...)] // DO NOT USE - configs are embedded, not separate assets
```

## Example Modifiers

### 1. Enhanced Skill Progression Modifier with PercentUsingTimeToComplete

```csharp
/// <summary>
/// Configuration for Enhanced Skill Progression modifier - [Serializable] class embedded in DifficultyConfig
/// </summary>
[Serializable]
public class EnhancedSkillProgressionConfig : BaseModifierConfig
{
    [SerializeField] private float timeEfficiencyWeight = 0.4f;
    [SerializeField] private float consistencyWeight = 0.35f;
    [SerializeField] private float progressionWeight = 0.25f;
    [SerializeField] private float maxReduction = 2f;
    [SerializeField] private float maxIncrease = 1.5f;

    public float TimeEfficiencyWeight => this.timeEfficiencyWeight;
    public float ConsistencyWeight => this.consistencyWeight;
    public float ProgressionWeight => this.progressionWeight;
    public float MaxReduction => this.maxReduction;
    public float MaxIncrease => this.maxIncrease;

    public override string ModifierType => "EnhancedSkillProgression";

    public override BaseModifierConfig CreateDefault()
    {
        var config = new EnhancedSkillProgressionConfig();
        config.timeEfficiencyWeight = 0.4f;
        config.consistencyWeight = 0.35f;
        config.progressionWeight = 0.25f;
        config.maxReduction = 2f;
        config.maxIncrease = 1.5f;
        return config;
    }
}

public class EnhancedSkillProgressionModifier : BaseDifficultyModifier<EnhancedSkillProgressionConfig>
{
    private readonly ILevelProgressProvider levelProvider;
    private readonly IWinStreakProvider winProvider;

    public override string ModifierName => "EnhancedSkillProgression";

    public EnhancedSkillProgressionModifier(EnhancedSkillProgressionConfig config,
        ILevelProgressProvider levelProvider,
        IWinStreakProvider winProvider) : base(config)
    {
        this.levelProvider = levelProvider;
        this.winProvider = winProvider;
    }

    public override ModifierResult Calculate()
    {
        // Comprehensive skill analysis using multiple providers (stateless)
        var completionRate = this.levelProvider.GetCompletionRate();
        var avgTime = this.levelProvider.GetAverageCompletionTime();
        var timePercentage = this.levelProvider.GetCurrentLevelTimePercentage(); // 🆕 NEW
        var currentLevel = this.levelProvider.GetCurrentLevel();
        var totalWins = this.winProvider.GetTotalWins();

        if (totalWins < 10) return ModifierResult.NoChange("Insufficient game history");

        // 🆕 Enhanced time efficiency calculation using PercentUsingTimeToComplete
        var timeEfficiencyScore = CalculateTimeEfficiencyScore(timePercentage, avgTime);
        var consistencyScore = CalculateConsistencyScore(completionRate);
        var progressionScore = CalculateProgressionScore(currentLevel, totalWins);

        // Weighted skill calculation
        var overallSkill = (timeEfficiencyScore * this.config.TimeEfficiencyWeight) +
                          (consistencyScore * this.config.ConsistencyWeight) +
                          (progressionScore * this.config.ProgressionWeight);

        // Map skill to difficulty adjustment
        var adjustment = MapSkillToAdjustment(overallSkill);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = GetSkillBasedReason(overallSkill),
            Metadata =
            {
                ["skill_score"] = overallSkill,
                ["time_efficiency_score"] = timeEfficiencyScore,
                ["consistency_score"] = consistencyScore,
                ["progression_score"] = progressionScore,
                ["completion_rate"] = completionRate,
                ["avg_time"] = avgTime,
                ["time_percentage"] = timePercentage, // 🆕 NEW
                ["current_level"] = currentLevel
            }
        };
    }

    private float CalculateTimeEfficiencyScore(float timePercentage, float avgTime)
    {
        // 🆕 Primary: Use PercentUsingTimeToComplete if available
        if (timePercentage > 0)
        {
            // Convert time percentage to efficiency score
            // Values < 1.0 = faster than expected (higher efficiency)
            // Values > 1.0 = slower than expected (lower efficiency)
            if (timePercentage <= 0.5f) return 1f; // Extremely fast
            if (timePercentage <= 0.8f) return 0.8f; // Fast
            if (timePercentage <= 1.0f) return 0.6f; // Normal
            if (timePercentage <= 1.5f) return 0.4f; // Slow
            return 0.2f; // Very slow
        }

        // Fallback: Use average completion time
        if (avgTime <= 0) return 0.5f; // No data, assume average

        // Simple time-based scoring (adjust thresholds based on your game)
        if (avgTime <= 30f) return 1f;   // Very fast
        if (avgTime <= 60f) return 0.8f; // Fast
        if (avgTime <= 120f) return 0.6f; // Normal
        if (avgTime <= 300f) return 0.4f; // Slow
        return 0.2f; // Very slow
    }

    private float CalculateConsistencyScore(float completionRate)
    {
        // High completion rate = higher skill
        return Math.Max(0f, Math.Min(1f, completionRate));
    }

    private float CalculateProgressionScore(int currentLevel, int totalWins)
    {
        if (totalWins == 0) return 0f;

        // Good progression = reaching higher levels with fewer total wins
        var progressionRatio = (float)currentLevel / totalWins;
        return Math.Max(0f, Math.Min(1f, progressionRatio * 10f)); // Scale appropriately
    }

    private float MapSkillToAdjustment(float skillScore)
    {
        // Low skill (0-0.3): Reduce difficulty
        if (skillScore < 0.3f)
            return -this.config.MaxReduction * (0.3f - skillScore) / 0.3f;

        // High skill (0.7-1.0): Increase difficulty
        if (skillScore > 0.7f)
            return this.config.MaxIncrease * (skillScore - 0.7f) / 0.3f;

        // Average skill (0.3-0.7): No change
        return 0f;
    }

    private string GetSkillBasedReason(float skillScore)
    {
        if (skillScore < 0.3f) return $"Low skill detected ({skillScore:P0}) - reducing difficulty";
        if (skillScore > 0.7f) return $"High skill detected ({skillScore:P0}) - increasing difficulty";
        return $"Average skill level ({skillScore:P0}) - maintaining difficulty";
    }
}
```

### 2. Time-Based Engagement Pattern Modifier

```csharp
[Serializable]
public class TimeBasedEngagementConfig : BaseModifierConfig
{
    [SerializeField] private float optimalTimePercentage = 0.8f;
    [SerializeField] private float strugglingTimeThreshold = 1.5f;
    [SerializeField] private float masteringTimeThreshold = 0.6f;
    [SerializeField] private float engagementBonus = 0.4f;
    [SerializeField] private float strugglingPenalty = 0.6f;

    public float OptimalTimePercentage => this.optimalTimePercentage;
    public float StrugglingTimeThreshold => this.strugglingTimeThreshold;
    public float MasteringTimeThreshold => this.masteringTimeThreshold;
    public float EngagementBonus => this.engagementBonus;
    public float StrugglingPenalty => this.strugglingPenalty;

    public override string ModifierType => "TimeBasedEngagement";

    public override BaseModifierConfig CreateDefault()
    {
        var config = new TimeBasedEngagementConfig();
        config.optimalTimePercentage = 0.8f;
        config.strugglingTimeThreshold = 1.5f;
        config.masteringTimeThreshold = 0.6f;
        config.engagementBonus = 0.4f;
        config.strugglingPenalty = 0.6f;
        return config;
    }
}

public class TimeBasedEngagementModifier : BaseDifficultyModifier<TimeBasedEngagementConfig>
{
    private readonly ILevelProgressProvider levelProvider;
    private readonly IRageQuitProvider rageProvider;

    public override string ModifierName => "TimeBasedEngagement";

    public TimeBasedEngagementModifier(TimeBasedEngagementConfig config,
        ILevelProgressProvider levelProvider,
        IRageQuitProvider rageProvider) : base(config)
    {
        this.levelProvider = levelProvider;
        this.rageProvider = rageProvider;
    }

    public override ModifierResult Calculate()
    {
        // 🆕 Use PercentUsingTimeToComplete for more accurate engagement analysis (stateless)
        var timePercentage = this.levelProvider.GetCurrentLevelTimePercentage();
        var avgSessionDuration = this.rageProvider.GetAverageSessionDuration();
        var recentRageQuits = this.rageProvider.GetRecentRageQuitCount();

        var engagementScore = CalculateEngagementScore(timePercentage, avgSessionDuration, recentRageQuits);
        var adjustment = MapEngagementToAdjustment(engagementScore, timePercentage);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = GetEngagementReason(engagementScore, timePercentage),
            Metadata =
            {
                ["engagement_score"] = engagementScore,
                ["time_percentage"] = timePercentage, // 🆕 NEW
                ["avg_session_duration"] = avgSessionDuration,
                ["recent_rage_quits"] = recentRageQuits,
                ["time_based_analysis"] = timePercentage > 0 // 🆕 NEW
            }
        };
    }

    private float CalculateEngagementScore(float timePercentage, float avgDuration, int rageQuits)
    {
        // 🆕 Enhanced engagement calculation using time percentage
        var timeEngagementScore = 0.5f; // Default neutral score

        if (timePercentage > 0)
        {
            // Players taking optimal time show good engagement
            if (timePercentage <= this.config.OptimalTimePercentage)
                timeEngagementScore = 0.8f;
            // Players struggling might be frustrated
            else if (timePercentage >= this.config.StrugglingTimeThreshold)
                timeEngagementScore = 0.2f;
            // Players mastering quickly might want more challenge
            else if (timePercentage <= this.config.MasteringTimeThreshold)
                timeEngagementScore = 1.0f;
        }

        // Session duration engagement (0-1)
        var durationScore = Math.Min(1f, avgDuration / 300f); // 5 minutes optimal

        // Rage quit penalty (0-1)
        var rageQuitScore = Math.Max(0f, 1f - (rageQuits * 0.25f));

        // Weighted combination with emphasis on time-based engagement
        return (timeEngagementScore * 0.5f + durationScore * 0.3f + rageQuitScore * 0.2f);
    }

    private float MapEngagementToAdjustment(float engagementScore, float timePercentage)
    {
        // 🆕 Time-aware adjustment mapping
        if (timePercentage > 0)
        {
            // Player mastering levels quickly - increase challenge
            if (timePercentage <= this.config.MasteringTimeThreshold && engagementScore > 0.7f)
                return this.config.EngagementBonus;

            // Player struggling significantly - reduce difficulty
            if (timePercentage >= this.config.StrugglingTimeThreshold && engagementScore < 0.4f)
                return -this.config.StrugglingPenalty;
        }

        // Standard engagement-based adjustments
        if (engagementScore < 0.3f) return -0.4f;
        if (engagementScore > 0.8f) return 0.3f;

        return 0f;
    }

    private string GetEngagementReason(float engagementScore, float timePercentage)
    {
        if (timePercentage > 0)
        {
            if (timePercentage <= this.config.MasteringTimeThreshold)
                return $"Mastering levels quickly ({timePercentage:P0} of expected time) - increasing challenge";
            if (timePercentage >= this.config.StrugglingTimeThreshold)
                return $"Struggling with timing ({timePercentage:P0} of expected time) - providing assistance";
        }

        if (engagementScore < 0.3f) return $"Low engagement ({engagementScore:P0}) - reducing difficulty";
        if (engagementScore > 0.8f) return $"High engagement ({engagementScore:P0}) - increasing challenge";
        return $"Moderate engagement ({engagementScore:P0}) - maintaining current difficulty";
    }
}
```

## Testing Modifiers

### Unit Test Template

```csharp
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using Moq;

[TestFixture]
public class EnhancedModifierTests
{
    private LevelProgressModifier modifier;
    private LevelProgressConfig config;
    private Mock<ILevelProgressProvider> mockProvider;

    [SetUp]
    public void Setup()
    {
        config = new LevelProgressConfig();
        config = (LevelProgressConfig)config.CreateDefault();

        mockProvider = new Mock<ILevelProgressProvider>();
        modifier = new LevelProgressModifier(config, mockProvider.Object);
    }

    [Test]
    public void Calculate_WithFastTimePercentage_ReturnsBonus()
    {
        // Arrange
        mockProvider.Setup(p => p.GetCurrentLevelTimePercentage()).Returns(0.6f); // Fast completion
        mockProvider.Setup(p => p.GetAttemptsOnCurrentLevel()).Returns(3);

        // Act
        var result = modifier.Calculate(); // No parameters - stateless!

        // Assert
        Assert.Greater(result.Value, 0);
        Assert.Contains("Fast completion", result.Reason);
        Assert.AreEqual("LevelProgress", result.ModifierName);
        Assert.IsTrue(result.Metadata.ContainsKey("time_percentage")); // 🆕 NEW
    }

    [Test]
    public void Calculate_WithSlowTimePercentage_AppliesPenalty()
    {
        // Arrange
        mockProvider.Setup(p => p.GetCurrentLevelTimePercentage()).Returns(1.8f); // Slow completion
        mockProvider.Setup(p => p.GetAttemptsOnCurrentLevel()).Returns(3);

        // Act
        var result = modifier.Calculate(); // No parameters - stateless!

        // Assert
        Assert.Less(result.Value, 0);
        Assert.Contains("Slow completion", result.Reason);
        Assert.IsTrue(result.Metadata.ContainsKey("time_percentage")); // 🆕 NEW
    }

    [TestCase(0.5f, 0.3f)] // Very fast - significant bonus
    [TestCase(0.8f, 0.1f)] // Moderately fast - small bonus
    [TestCase(1.2f, 0f)]   // Normal speed - no change
    [TestCase(1.8f, -0.2f)] // Slow - penalty applied
    public void Calculate_WithVariousTimePercentages_ReturnsExpectedValues(
        float timePercentage, float expectedSign)
    {
        // Arrange
        mockProvider.Setup(p => p.GetCurrentLevelTimePercentage()).Returns(timePercentage);
        mockProvider.Setup(p => p.GetAttemptsOnCurrentLevel()).Returns(3);

        // Act
        var result = modifier.Calculate(); // No parameters - stateless!

        // Assert
        if (expectedSign > 0)
            Assert.Greater(result.Value, 0, $"Expected positive adjustment for time percentage {timePercentage}");
        else if (expectedSign < 0)
            Assert.Less(result.Value, 0, $"Expected negative adjustment for time percentage {timePercentage}");
        else
            Assert.AreEqual(0, result.Value, 0.01f, $"Expected no adjustment for time percentage {timePercentage}");
    }
}
```

### Integration Test with All Provider Methods

```csharp
[Test]
public void EnhancedModifierUsesAllProviderMethods_Correctly()
{
    // Arrange
    var levelProvider = new Mock<ILevelProgressProvider>();

    levelProvider.Setup(p => p.GetCurrentLevel()).Returns(25);
    levelProvider.Setup(p => p.GetAverageCompletionTime()).Returns(120f);
    levelProvider.Setup(p => p.GetAttemptsOnCurrentLevel()).Returns(3);
    levelProvider.Setup(p => p.GetCompletionRate()).Returns(0.7f);
    levelProvider.Setup(p => p.GetCurrentLevelDifficulty()).Returns(4.0f);
    levelProvider.Setup(p => p.GetCurrentLevelTimePercentage()).Returns(0.8f); // 🆕 NEW

    var modifier = new LevelProgressModifier(config, levelProvider.Object);

    // Act
    var result = modifier.Calculate(); // No parameters - stateless!

    // Assert - Verify all provider methods were called
    levelProvider.Verify(p => p.GetCurrentLevel(), Times.Once);
    levelProvider.Verify(p => p.GetAverageCompletionTime(), Times.AtLeastOnce);
    levelProvider.Verify(p => p.GetAttemptsOnCurrentLevel(), Times.Once);
    levelProvider.Verify(p => p.GetCompletionRate(), Times.Once);
    levelProvider.Verify(p => p.GetCurrentLevelDifficulty(), Times.Once);
    levelProvider.Verify(p => p.GetCurrentLevelTimePercentage(), Times.Once); // 🆕 NEW

    // Verify result uses data from all provider methods
    Assert.IsTrue(result.Metadata.ContainsKey("attempts"));
    Assert.IsTrue(result.Metadata.ContainsKey("currentLevel"));
    Assert.IsTrue(result.Metadata.ContainsKey("levelDifficulty"));
    Assert.IsTrue(result.Metadata.ContainsKey("completionRate"));
    Assert.IsTrue(result.Metadata.ContainsKey("avgCompletionTime"));
    Assert.IsTrue(result.Metadata.ContainsKey("time_percentage")); // 🆕 NEW
}
```

### Configuration Testing

```csharp
[Test]
public void EnhancedConfiguration_EmbeddedInDifficultyConfig_WorksCorrectly()
{
    // Arrange
    var mainConfig = DifficultyConfig.CreateDefault();
    var levelProgressConfig = mainConfig.GetModifierConfig<LevelProgressConfig>("LevelProgress");

    // Act & Assert
    Assert.IsNotNull(levelProgressConfig);
    Assert.AreEqual("LevelProgress", levelProgressConfig.ModifierType);

    // Test new configuration parameters
    Assert.AreEqual(1.0f, levelProgressConfig.MaxPenaltyMultiplier);
    Assert.AreEqual(1.0f, levelProgressConfig.FastCompletionMultiplier);
    Assert.AreEqual(3f, levelProgressConfig.HardLevelThreshold);
    Assert.AreEqual(0.7f, levelProgressConfig.MasteryCompletionRate);
    Assert.AreEqual(0.3f, levelProgressConfig.MasteryBonus);
    Assert.AreEqual(2f, levelProgressConfig.EasyLevelThreshold);
    Assert.AreEqual(0.3f, levelProgressConfig.StruggleCompletionRate);
    Assert.AreEqual(0.3f, levelProgressConfig.StrugglePenalty);
}
```

### Manual Testing Checklist

- [ ] Enhanced modifier registers correctly in DI
- [ ] All configuration parameters load properly from single DifficultyConfig
- [ ] `GetCurrentLevelTimePercentage()` provider method is called correctly
- [ ] Time-based calculations use PercentUsingTimeToComplete as primary metric
- [ ] New configuration parameters (maxPenaltyMultiplier, fastCompletionMultiplier, etc.) affect calculations
- [ ] Mastery and struggle detection works with configurable thresholds
- [ ] Edge cases handled gracefully (zero/negative values)
- [ ] Performance impact minimal (< 1ms)
- [ ] Debug logs include time percentage information
- [ ] Metadata populated with all new fields for debugging
- [ ] Integration with all 7 modifiers still works
- [ ] Unity Inspector shows all enhanced config parameters properly
- [ ] Stateless Calculate() method works without parameters

## Troubleshooting

### Common Issues

1. **Modifier not running**
   - Check if registered in DI
   - Verify IsEnabled = true in the single config
   - Check priority order
   - Ensure provider dependencies are satisfied

2. **Enhanced Configuration Issues** ⚠️ **UPDATED**
   - Only ONE DifficultyConfig ScriptableObject should exist
   - All modifier configs use [Serializable], not [CreateAssetMenu]
   - All modifier configs are embedded in the single asset
   - Use type-safe property access: `this.config.MaxPenaltyMultiplier`

3. **Time Percentage Issues** 🆕 **NEW**
   - Verify `GetCurrentLevelTimePercentage()` implementation returns valid values (0-positive)
   - Check UITemplate integration provides `PercentUsingTimeToComplete` data
   - Values < 1.0 should indicate faster than expected completion

4. **Stateless Issues** ⚠️ **CRITICAL**
   - Calculate() method should take NO parameters
   - All data must come from provider interfaces
   - No internal state should be stored in modifiers
   - Providers must be injected via constructor

5. **Wrong values**
   - Verify provider data is correct, especially new time percentage method
   - Check all configuration values in Unity Inspector
   - Test with mock providers to isolate calculation logic
   - Validate time-based calculation logic

6. **Performance issues**
   - Cache expensive calculations
   - Avoid LINQ in hot paths
   - Profile with Unity Profiler
   - Check provider method performance, especially new time percentage calculation

7. **Provider-related exceptions**
   - Check provider implementations include new `GetCurrentLevelTimePercentage()` method
   - Verify provider registration in DI
   - Handle null provider data gracefully
   - Validate time percentage values are non-negative

---

*Last Updated: 2025-01-29*
*Stateless Architecture Implementation • Calculate() Method Takes NO Parameters • 7 Modifiers Complete • Enhanced LevelProgressModifier with PercentUsingTimeToComplete Integration • Production Ready*