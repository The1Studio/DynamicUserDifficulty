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

Modifiers are the core extensibility mechanism of the Dynamic User Difficulty system. Each modifier calculates a difficulty adjustment based on specific player behavior or game state. The system now includes **7 comprehensive modifiers** covering all aspects of player behavior analysis.

### Key Concepts
- **Modular**: Each modifier is independent
- **Type-Safe Configurable**: Parameters via strongly-typed [Serializable] classes embedded in ONE ScriptableObject
- **Prioritized**: Execution order controlled by priority
- **Toggleable**: Can be enabled/disabled at runtime
- **Provider-Based**: Uses external data through clean interfaces

### ⚠️ **CRITICAL: Configuration Architecture**
**All 7 modifier configurations are embedded within a SINGLE DifficultyConfig ScriptableObject using polymorphic serialization.**

## Built-in Modifiers (7 Total)

### 1. WinStreakModifier ✅ ORIGINAL
**Purpose**: Increases difficulty based on consecutive wins
**Data Source**: `IWinStreakProvider.GetWinStreak()`
**Config**: `WinStreakConfig` ([Serializable] class, NOT ScriptableObject)

```csharp
public class WinStreakModifier : BaseDifficultyModifier<WinStreakConfig>
{
    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        var winStreak = this.winStreakProvider.GetWinStreak();
        if (winStreak < this.config.WinThreshold) return ModifierResult.NoChange();

        var bonus = Math.Min(
            (winStreak - this.config.WinThreshold) * this.config.StepSize,
            this.config.MaxBonus
        );

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = bonus,
            Reason = $"Win streak bonus ({winStreak} consecutive wins)"
        };
    }
}
```

### 2. LossStreakModifier ✅ ORIGINAL
**Purpose**: Decreases difficulty based on consecutive losses
**Data Source**: `IWinStreakProvider.GetLossStreak()`
**Config**: `LossStreakConfig` ([Serializable] class)

```csharp
public class LossStreakModifier : BaseDifficultyModifier<LossStreakConfig>
{
    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        var lossStreak = this.lossStreakProvider.GetLossStreak();
        if (lossStreak < this.config.LossThreshold) return ModifierResult.NoChange();

        var reduction = -Math.Min(
            (lossStreak - this.config.LossThreshold) * this.config.StepSize,
            this.config.MaxReduction
        );

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = reduction,
            Reason = $"Loss streak compensation ({lossStreak} consecutive losses)"
        };
    }
}
```

### 3. TimeDecayModifier ✅ ORIGINAL
**Purpose**: Reduces difficulty for returning players
**Data Source**: `ITimeDecayProvider.GetTimeSinceLastPlay()`
**Config**: `TimeDecayConfig` ([Serializable] class)

```csharp
public class TimeDecayModifier : BaseDifficultyModifier<TimeDecayConfig>
{
    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        var timeSinceLastPlay = this.timeDecayProvider.GetTimeSinceLastPlay();
        var hoursSince = (float)timeSinceLastPlay.TotalHours;

        if (hoursSince < this.config.GraceHours) return ModifierResult.NoChange();

        var daysSince = hoursSince / 24f;
        var decay = -Math.Min(daysSince * this.config.DecayPerDay, this.config.MaxDecay);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = decay,
            Reason = $"Time decay ({daysSince:F1} days since last play)"
        };
    }
}
```

### 4. RageQuitModifier ✅ ORIGINAL
**Purpose**: Detects and compensates for rage quits
**Data Sources**: `IRageQuitProvider.GetLastQuitType()`, `GetCurrentSessionDuration()`, `GetRecentRageQuitCount()`
**Config**: `RageQuitConfig` ([Serializable] class)

```csharp
public class RageQuitModifier : BaseDifficultyModifier<RageQuitConfig>
{
    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        var quitType = this.rageQuitProvider.GetLastQuitType();
        var sessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();
        var recentRageQuits = this.rageQuitProvider.GetRecentRageQuitCount();

        // Detect rage quit patterns
        bool isRageQuit = quitType == QuitType.RageQuit ||
                         (sessionDuration < this.config.RageQuitThreshold && quitType == QuitType.QuitAfterLoss);

        if (!isRageQuit && recentRageQuits == 0) return ModifierResult.NoChange();

        var reduction = isRageQuit ? -this.config.RageQuitReduction : -this.config.QuitReduction;

        // Additional reduction for multiple recent rage quits
        if (recentRageQuits > 1) reduction *= (1 + recentRageQuits * 0.2f);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = reduction,
            Reason = $"Rage quit compensation (recent quits: {recentRageQuits})"
        };
    }
}
```

### 5. CompletionRateModifier ✅ NEW
**Purpose**: Adjusts difficulty based on overall player success rate
**Data Sources**: `IWinStreakProvider.GetTotalWins()`, `GetTotalLosses()`, `ILevelProgressProvider.GetCompletionRate()`
**Config**: `CompletionRateConfig` ([Serializable] class)

```csharp
public class CompletionRateModifier : BaseDifficultyModifier<CompletionRateConfig>
{
    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        var totalWins = this.winStreakProvider.GetTotalWins();
        var totalLosses = this.winStreakProvider.GetTotalLosses();
        var completionRate = this.levelProgressProvider.GetCompletionRate();

        if (totalWins + totalLosses < 10) return ModifierResult.NoChange(); // Need sufficient data

        float adjustment = 0f;
        string reason = "No completion rate adjustment";

        if (completionRate < this.config.LowCompletionThreshold)
        {
            adjustment = this.config.LowCompletionAdjustment;
            reason = $"Low completion rate ({completionRate:P0}) - reducing difficulty";
        }
        else if (completionRate > this.config.HighCompletionThreshold)
        {
            adjustment = this.config.HighCompletionAdjustment;
            reason = $"High completion rate ({completionRate:P0}) - increasing difficulty";
        }

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = reason,
            Metadata = new Dictionary<string, object>
            {
                ["completion_rate"] = completionRate,
                ["total_wins"] = totalWins,
                ["total_losses"] = totalLosses
            }
        };
    }
}
```

### 6. LevelProgressModifier ✅ NEW
**Purpose**: Analyzes level progression patterns (attempts, completion time, progression speed)
**Data Sources**: `ILevelProgressProvider.GetCurrentLevel()`, `GetAverageCompletionTime()`, `GetAttemptsOnCurrentLevel()`, `GetCurrentLevelDifficulty()`
**Config**: `LevelProgressConfig` ([Serializable] class)

```csharp
public class LevelProgressModifier : BaseDifficultyModifier<LevelProgressConfig>
{
    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        var currentLevel = this.levelProgressProvider.GetCurrentLevel();
        var averageTime = this.levelProgressProvider.GetAverageCompletionTime();
        var attempts = this.levelProgressProvider.GetAttemptsOnCurrentLevel();
        var levelDifficulty = this.levelProgressProvider.GetCurrentLevelDifficulty();

        float adjustment = 0f;
        var reasons = new List<string>();

        // Analyze attempts
        if (attempts > this.config.AttemptsThreshold)
        {
            var excessAttempts = attempts - this.config.AttemptsThreshold;
            var attemptsReduction = excessAttempts * this.config.AttemptsReduction;
            adjustment += attemptsReduction;
            reasons.Add($"Too many attempts ({attempts})");
        }

        // Analyze completion time
        if (averageTime > 0)
        {
            if (averageTime < this.config.FastCompletionTime)
            {
                adjustment += this.config.FastCompletionBonus;
                reasons.Add($"Fast completion ({averageTime:F0}s)");
            }
            else if (averageTime > this.config.SlowCompletionTime)
            {
                adjustment += this.config.SlowCompletionReduction;
                reasons.Add($"Slow completion ({averageTime:F0}s)");
            }
        }

        // Progressive difficulty scaling
        var levelFactor = Math.Min(currentLevel / 100f, 1f); // Cap at level 100
        adjustment *= (1f + levelFactor * 0.1f); // Slightly scale with level

        var reason = reasons.Any() ? string.Join(", ", reasons) : "No progress adjustment";

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = reason,
            Metadata = new Dictionary<string, object>
            {
                ["current_level"] = currentLevel,
                ["average_time"] = averageTime,
                ["attempts"] = attempts,
                ["level_difficulty"] = levelDifficulty,
                ["level_factor"] = levelFactor
            }
        };
    }
}
```

### 7. SessionPatternModifier ✅ NEW
**Purpose**: Detects session patterns and player frustration through duration and behavior analysis
**Data Sources**: `IRageQuitProvider.GetAverageSessionDuration()`, `GetRecentRageQuitCount()`
**Config**: `SessionPatternConfig` ([Serializable] class)

```csharp
public class SessionPatternModifier : BaseDifficultyModifier<SessionPatternConfig>
{
    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        var avgSessionDuration = this.rageQuitProvider.GetAverageSessionDuration();
        var recentRageQuits = this.rageQuitProvider.GetRecentRageQuitCount();

        float adjustment = 0f;
        var reasons = new List<string>();

        // Analyze session duration patterns
        if (avgSessionDuration < this.config.ShortSessionThreshold)
        {
            adjustment += this.config.ShortSessionReduction;
            reasons.Add($"Short sessions ({avgSessionDuration:F0}s avg)");
        }
        else if (avgSessionDuration > this.config.LongSessionThreshold)
        {
            adjustment += this.config.LongSessionBonus;
            reasons.Add($"Long engagement ({avgSessionDuration:F0}s avg)");
        }

        // Analyze rage quit patterns
        if (recentRageQuits >= this.config.RageQuitCountThreshold)
        {
            adjustment += this.config.RageQuitReduction;
            reasons.Add($"Rage quit pattern ({recentRageQuits} recent quits)");
        }

        var reason = reasons.Any() ? string.Join(", ", reasons) : "No session pattern adjustment";

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = reason,
            Metadata = new Dictionary<string, object>
            {
                ["avg_session_duration"] = avgSessionDuration,
                ["recent_rage_quits"] = recentRageQuits,
                ["short_threshold"] = this.config.ShortSessionThreshold,
                ["long_threshold"] = this.config.LongSessionThreshold
            }
        };
    }
}
```

## Creating Custom Modifiers

### ⚠️ **CRITICAL: Corrected Configuration Pattern**

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

### Step 2: Create the Modifier Class

```csharp
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;

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

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            // Get data from provider
            var avgTime = this.levelProvider.GetAverageCompletionTime();

            // Check condition using type-safe config properties
            if (avgTime <= 0 || avgTime >= this.config.TimeThreshold)
                return NoChange("No speed bonus applicable");

            var adjustment = this.config.BonusAmount;

            // Apply special mode if enabled
            if (this.config.EnableSpecialMode)
                adjustment *= 1.5f;

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = adjustment,
                Reason = $"Fast completion bonus (avg: {avgTime:F0}s)",
                Metadata = new Dictionary<string, object>
                {
                    ["average_time"] = avgTime,
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

### 2. Calculation Phase
```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Called each time difficulty is calculated
    // Should be stateless and pure
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
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Check if should run
    if (!ShouldRun(sessionData))
        return NoChange("Conditions not met");

    // Get dynamic threshold based on player level
    var dynamicThreshold = GetDynamicThreshold(sessionData);

    // Calculate with dynamic values
    return CalculateWithThreshold(sessionData, dynamicThreshold);
}

private float GetDynamicThreshold(PlayerSessionData data)
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

### 1. Keep Calculations Pure

✅ **Good: Pure function with provider data**
```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Read from providers (external data)
    var winRate = this.levelProvider.GetCompletionRate();
    var avgTime = this.levelProvider.GetAverageCompletionTime();

    // Pure calculation
    var adjustment = CalculateAdjustment(winRate, avgTime);

    // Deterministic result
    return new ModifierResult { Value = adjustment };
}
```

❌ **Bad: Side effects and non-deterministic behavior**
```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Don't do this!
    SaveToDatabase(sessionData);  // Side effect
    var random = Random.Range(0, 1);  // Non-deterministic
    sessionData.WinStreak++;      // Modifying input
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
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Check for sufficient data
    var totalGames = this.winStreakProvider.GetTotalWins() + this.winStreakProvider.GetTotalLosses();
    if (totalGames < 5)
    {
        return NoChange("Insufficient data for analysis");
    }

    // Validate provider data
    var completionRate = this.levelProvider.GetCompletionRate();
    if (completionRate < 0 || completionRate > 1)
    {
        Debug.LogWarning($"Invalid completion rate: {completionRate}");
        return NoChange("Invalid completion rate data");
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
    Metadata = new Dictionary<string, object>
    {
        ["completion_rate"] = completionRate,
        ["total_games"] = totalGames,
        ["avg_time"] = avgTime,
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

### 1. Skill Progression Modifier

```csharp
/// <summary>
/// Configuration for Skill Progression modifier - [Serializable] class embedded in DifficultyConfig
/// </summary>
[Serializable]
public class SkillProgressionConfig : BaseModifierConfig
{
    [SerializeField] private float fastCompletionTime = 30f;
    [SerializeField] private float slowCompletionTime = 180f;
    [SerializeField] private float maxReduction = 2f;
    [SerializeField] private float maxIncrease = 1.5f;

    public float FastCompletionTime => this.fastCompletionTime;
    public float SlowCompletionTime => this.slowCompletionTime;
    public float MaxReduction => this.maxReduction;
    public float MaxIncrease => this.maxIncrease;

    public override string ModifierType => "SkillProgression";

    public override BaseModifierConfig CreateDefault()
    {
        var config = new SkillProgressionConfig();
        config.fastCompletionTime = 30f;
        config.slowCompletionTime = 180f;
        config.maxReduction = 2f;
        config.maxIncrease = 1.5f;
        return config;
    }
}

public class SkillProgressionModifier : BaseDifficultyModifier<SkillProgressionConfig>
{
    private readonly ILevelProgressProvider levelProvider;
    private readonly IWinStreakProvider winProvider;

    public override string ModifierName => "SkillProgression";

    public SkillProgressionModifier(SkillProgressionConfig config,
        ILevelProgressProvider levelProvider,
        IWinStreakProvider winProvider) : base(config)
    {
        this.levelProvider = levelProvider;
        this.winProvider = winProvider;
    }

    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        // Comprehensive skill analysis using multiple providers
        var completionRate = this.levelProvider.GetCompletionRate();
        var avgTime = this.levelProvider.GetAverageCompletionTime();
        var currentLevel = this.levelProvider.GetCurrentLevel();
        var totalWins = this.winProvider.GetTotalWins();

        if (totalWins < 10) return NoChange("Insufficient game history");

        // Calculate skill score (0-1)
        var timeScore = CalculateTimeScore(avgTime);
        var consistencyScore = CalculateConsistencyScore(completionRate);
        var progressionScore = CalculateProgressionScore(currentLevel, totalWins);

        var overallSkill = (timeScore + consistencyScore + progressionScore) / 3f;

        // Map skill to difficulty adjustment
        var adjustment = MapSkillToAdjustment(overallSkill);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = GetSkillBasedReason(overallSkill),
            Metadata = new Dictionary<string, object>
            {
                ["skill_score"] = overallSkill,
                ["time_score"] = timeScore,
                ["consistency_score"] = consistencyScore,
                ["progression_score"] = progressionScore,
                ["completion_rate"] = completionRate,
                ["avg_time"] = avgTime,
                ["current_level"] = currentLevel
            }
        };
    }

    private float CalculateTimeScore(float avgTime)
    {
        if (avgTime <= 0) return 0.5f; // No data, assume average

        // Fast completion = higher skill
        var fastTime = this.config.FastCompletionTime;
        var slowTime = this.config.SlowCompletionTime;

        if (avgTime <= fastTime) return 1f;
        if (avgTime >= slowTime) return 0f;

        // Linear interpolation between fast and slow
        return 1f - ((avgTime - fastTime) / (slowTime - fastTime));
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

### 2. Engagement Pattern Modifier

```csharp
[Serializable]
public class EngagementPatternConfig : BaseModifierConfig
{
    [SerializeField] private float optimalSessionDuration = 300f;
    [SerializeField] private float maxHoursForFullScore = 24f;
    [SerializeField] private float disengagementReduction = 1f;
    [SerializeField] private float highEngagementBonus = 0.5f;

    public float OptimalSessionDuration => this.optimalSessionDuration;
    public float MaxHoursForFullScore => this.maxHoursForFullScore;
    public float DisengagementReduction => this.disengagementReduction;
    public float HighEngagementBonus => this.highEngagementBonus;

    public override string ModifierType => "EngagementPattern";

    public override BaseModifierConfig CreateDefault()
    {
        var config = new EngagementPatternConfig();
        config.optimalSessionDuration = 300f;
        config.maxHoursForFullScore = 24f;
        config.disengagementReduction = 1f;
        config.highEngagementBonus = 0.5f;
        return config;
    }
}

public class EngagementPatternModifier : BaseDifficultyModifier<EngagementPatternConfig>
{
    private readonly IRageQuitProvider rageProvider;
    private readonly ITimeDecayProvider timeProvider;

    public override string ModifierName => "EngagementPattern";

    public EngagementPatternModifier(EngagementPatternConfig config,
        IRageQuitProvider rageProvider,
        ITimeDecayProvider timeProvider) : base(config)
    {
        this.rageProvider = rageProvider;
        this.timeProvider = timeProvider;
    }

    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        // Analyze engagement patterns
        var avgSessionDuration = this.rageProvider.GetAverageSessionDuration();
        var recentRageQuits = this.rageProvider.GetRecentRageQuitCount();
        var timeSinceLastPlay = this.timeProvider.GetTimeSinceLastPlay();

        var engagementScore = CalculateEngagementScore(avgSessionDuration, recentRageQuits, timeSinceLastPlay);
        var adjustment = MapEngagementToAdjustment(engagementScore);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = GetEngagementReason(engagementScore),
            Metadata = new Dictionary<string, object>
            {
                ["engagement_score"] = engagementScore,
                ["avg_session_duration"] = avgSessionDuration,
                ["recent_rage_quits"] = recentRageQuits,
                ["hours_since_last_play"] = timeSinceLastPlay.TotalHours
            }
        };
    }

    private float CalculateEngagementScore(float avgDuration, int rageQuits, TimeSpan timeSince)
    {
        // Duration score (0-1): Longer sessions = better engagement
        var durationScore = Math.Min(1f, avgDuration / this.config.OptimalSessionDuration);

        // Rage quit penalty (0-1): More rage quits = worse engagement
        var rageQuitScore = Math.Max(0f, 1f - (rageQuits * 0.2f));

        // Recency score (0-1): More recent play = better engagement
        var hoursSince = (float)timeSince.TotalHours;
        var recencyScore = Math.Max(0f, 1f - (hoursSince / this.config.MaxHoursForFullScore));

        // Weighted average
        return (durationScore * 0.4f + rageQuitScore * 0.4f + recencyScore * 0.2f);
    }

    private float MapEngagementToAdjustment(float engagementScore)
    {
        // Low engagement: Make easier to re-engage
        if (engagementScore < 0.3f)
            return -this.config.DisengagementReduction;

        // High engagement: Can handle more challenge
        if (engagementScore > 0.8f)
            return this.config.HighEngagementBonus;

        return 0f;
    }

    private string GetEngagementReason(float engagementScore)
    {
        if (engagementScore < 0.3f) return $"Low engagement ({engagementScore:P0}) - reducing difficulty to re-engage";
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

[TestFixture]
public class SpeedBonusModifierTests
{
    private SpeedBonusModifier modifier;
    private SpeedBonusConfig config;
    private Mock<ILevelProgressProvider> mockProvider;

    [SetUp]
    public void Setup()
    {
        config = new SpeedBonusConfig();
        // Set test values using the CreateDefault pattern
        config = (SpeedBonusConfig)config.CreateDefault();

        mockProvider = new Mock<ILevelProgressProvider>();
        modifier = new SpeedBonusModifier(config, mockProvider.Object);
    }

    [Test]
    public void Calculate_WithFastCompletion_ReturnsBonus()
    {
        // Arrange
        mockProvider.Setup(p => p.GetAverageCompletionTime()).Returns(30f); // Fast time
        var sessionData = new PlayerSessionData();

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.Greater(result.Value, 0);
        Assert.Contains("Fast completion bonus", result.Reason);
        Assert.AreEqual("SpeedBonus", result.ModifierName);
        Assert.IsTrue(result.Metadata.ContainsKey("average_time"));
    }

    [Test]
    public void Calculate_WithSlowCompletion_ReturnsNoChange()
    {
        // Arrange
        mockProvider.Setup(p => p.GetAverageCompletionTime()).Returns(120f); // Slow time
        var sessionData = new PlayerSessionData();

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.AreEqual(0, result.Value);
        Assert.Contains("No speed bonus applicable", result.Reason);
    }

    [TestCase(15f, 0.5f)] // Very fast
    [TestCase(45f, 0.5f)] // Medium fast
    [TestCase(75f, 0f)]   // Too slow
    public void Calculate_WithVariousCompletionTimes_ReturnsExpectedValues(
        float completionTime, float expectedBonus)
    {
        // Arrange
        mockProvider.Setup(p => p.GetAverageCompletionTime()).Returns(completionTime);
        var sessionData = new PlayerSessionData();

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.AreEqual(expectedBonus, result.Value, 0.01f);
    }
}
```

### Integration Test with Multiple Providers

```csharp
[Test]
public void ModifierUsesAllProviderMethods_Correctly()
{
    // Arrange
    var winProvider = new Mock<IWinStreakProvider>();
    var levelProvider = new Mock<ILevelProgressProvider>();
    var rageProvider = new Mock<IRageQuitProvider>();

    winProvider.Setup(p => p.GetTotalWins()).Returns(50);
    winProvider.Setup(p => p.GetTotalLosses()).Returns(20);
    levelProvider.Setup(p => p.GetCompletionRate()).Returns(0.7f);
    levelProvider.Setup(p => p.GetCurrentLevel()).Returns(25);
    levelProvider.Setup(p => p.GetAverageCompletionTime()).Returns(120f);
    rageProvider.Setup(p => p.GetAverageSessionDuration()).Returns(300f);

    var modifier = new ComprehensiveModifier(config, winProvider.Object,
        levelProvider.Object, rageProvider.Object);

    // Act
    var result = modifier.Calculate(sessionData);

    // Assert - Verify all provider methods were called
    winProvider.Verify(p => p.GetTotalWins(), Times.Once);
    winProvider.Verify(p => p.GetTotalLosses(), Times.Once);
    levelProvider.Verify(p => p.GetCompletionRate(), Times.Once);
    levelProvider.Verify(p => p.GetCurrentLevel(), Times.Once);
    levelProvider.Verify(p => p.GetAverageCompletionTime(), Times.Once);
    rageProvider.Verify(p => p.GetAverageSessionDuration(), Times.Once);

    // Verify result uses data from all providers
    Assert.IsTrue(result.Metadata.ContainsKey("total_wins"));
    Assert.IsTrue(result.Metadata.ContainsKey("completion_rate"));
    Assert.IsTrue(result.Metadata.ContainsKey("avg_session_duration"));
}
```

### Configuration Testing

```csharp
[Test]
public void Configuration_EmbeddedInDifficultyConfig_WorksCorrectly()
{
    // Arrange
    var mainConfig = DifficultyConfig.CreateDefault();
    var speedBonusConfig = mainConfig.GetModifierConfig<SpeedBonusConfig>("SpeedBonus");

    // Act & Assert
    Assert.IsNotNull(speedBonusConfig);
    Assert.AreEqual("SpeedBonus", speedBonusConfig.ModifierType);
    Assert.AreEqual(60f, speedBonusConfig.TimeThreshold);
    Assert.AreEqual(0.5f, speedBonusConfig.BonusAmount);
}
```

### Manual Testing Checklist

- [ ] Modifier registers correctly in DI
- [ ] Configuration loads properly from single DifficultyConfig
- [ ] Provider methods are called correctly
- [ ] Calculation returns expected values
- [ ] Edge cases handled gracefully
- [ ] Performance impact minimal (< 1ms)
- [ ] Debug logs work correctly
- [ ] Metadata populated for debugging
- [ ] OnApplied hook fires correctly
- [ ] Integration with all 7 modifiers works
- [ ] Unity Inspector shows embedded config properly

## Troubleshooting

### Common Issues

1. **Modifier not running**
   - Check if registered in DI
   - Verify IsEnabled = true in the single config
   - Check priority order
   - Ensure provider dependencies are satisfied

2. **Configuration Issues** ⚠️ **UPDATED**
   - Only ONE DifficultyConfig ScriptableObject should exist
   - Config classes use [Serializable], not [CreateAssetMenu]
   - All modifier configs are embedded in the single asset
   - Use type-safe property access: `this.config.PropertyName`

3. **Wrong values**
   - Verify provider data is correct
   - Check configuration values in Unity Inspector
   - Test with mock providers

4. **Performance issues**
   - Cache expensive calculations
   - Avoid LINQ in hot paths
   - Profile with Unity Profiler
   - Check provider method performance

5. **Provider-related exceptions**
   - Check provider implementations
   - Verify provider registration in DI
   - Handle null provider data gracefully

---

*Last Updated: 2025-01-22*
*Configuration Structure Corrected - Single ScriptableObject with Embedded [Serializable] Configs*