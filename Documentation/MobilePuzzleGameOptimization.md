# Mobile Puzzle Game Optimization Guide

## Overview
This guide provides recommended configurations and best practices for using the Dynamic User Difficulty module in mobile puzzle games like Unscrew Factory.

## Mobile Puzzle Game Characteristics
- **Session Length**: 3-5 minutes average
- **Interruptions**: Frequent (calls, notifications, transit)
- **Retry Behavior**: High retry rates are normal
- **Skill-Based**: Success depends on strategy, not reflexes
- **Thinking Time**: Players need time to analyze puzzles

## Recommended Configuration Values

### 🎯 Optimized for Mobile Puzzle Games

```csharp
// Create this configuration for mobile puzzle games
public class MobilePuzzleConfig
{
    // Win/Loss Streaks - Balanced for puzzle progression
    public const float WIN_THRESHOLD = 4f;           // Was 3f - accounts for easy level replays
    public const float WIN_STEP_SIZE = 0.4f;         // Was 0.5f - gentler increases
    public const float WIN_MAX_BONUS = 1.8f;         // Was 2f - prevent over-adjustment

    public const float LOSS_THRESHOLD = 2f;          // Keep at 2 - quick frustration response
    public const float LOSS_STEP_SIZE = 0.3f;        // Keep - conservative adjustment
    public const float LOSS_MAX_REDUCTION = 2.0f;    // Was 1.5f - bigger safety net

    // Time Decay - Mobile lifestyle friendly
    public const float GRACE_HOURS = 18f;            // Was 6f - accounts for daily routine
    public const float DECAY_PER_DAY = 0.3f;         // Was 0.5f - gentler decay
    public const float MAX_DECAY = 2.5f;             // Was 2f - help returning players more

    // Rage Quit - Puzzle thinking time aware
    public const float RAGE_QUIT_THRESHOLD = 90f;    // Was 30f - allow experimentation
    public const float RAGE_QUIT_PENALTY = 0.8f;     // Was 1f - might be interruption
    public const float NORMAL_QUIT_PENALTY = 0.3f;   // Was 0.5f - lighter penalty
    public const float MID_PLAY_PENALTY = 0.2f;      // Was 0.3f - very light

    // Completion Rate - Puzzle retry patterns
    public const int MIN_ATTEMPTS = 6;               // Was 10 - faster activation
    public const float LOW_COMPLETION = 0.35f;       // Was 0.4f - more forgiving
    public const float HIGH_COMPLETION = 0.75f;      // Was 0.7f - higher bar

    // Level Progress - Realistic expectations
    public const int LEVELS_PER_HOUR = 10;           // Was 15 - 6 min per level
    public const int HIGH_ATTEMPTS = 7;              // Was 5 - allow experimentation
    public const float FAST_RATIO = 0.6f;            // Was 0.7f - reward genuine speed
    public const float SLOW_RATIO = 2.0f;            // Was 1.5f - thinking time tolerance

    // Session Patterns - Mobile reality
    public const float VERY_SHORT_SESSION = 120f;    // Was 60f - normal interruptions
    public const float NORMAL_SESSION = 240f;        // Was 180f - 4 min puzzles
    public const float SHORT_PENALTY = 0.3f;         // Was 0.5f - lighter penalty
}
```

## Modifier-Specific Documentation

### 1. WinStreakModifier
**Purpose**: Increases difficulty when player shows mastery through consecutive wins.

**Mobile Puzzle Considerations**:
- Players often replay easier levels for rewards/practice
- Win streaks don't always indicate mastery
- Should account for level difficulty variance

**Recommended Settings**:
```csharp
winThreshold = 4f;        // Trigger after 4 wins (was 3)
stepSize = 0.4f;          // Gentler increase (was 0.5f)
maxBonus = 1.8f;          // Lower cap (was 2f)
```

**Why**: Prevents premature difficulty spikes from replaying easy levels.

### 2. LossStreakModifier
**Purpose**: Decreases difficulty to prevent player frustration and churn.

**Mobile Puzzle Considerations**:
- Mobile players quit faster when frustrated
- Quick response needed to retain players
- Balance between help and maintaining challenge

**Recommended Settings**:
```csharp
lossThreshold = 2f;       // Keep at 2 - quick response
stepSize = 0.3f;          // Keep - conservative
maxReduction = 2.0f;      // Increase from 1.5f
```

**Why**: Current settings work well, just increase safety net.

### 3. TimeDecayModifier
**Purpose**: Helps returning players by reducing difficulty after absence.

**Mobile Puzzle Considerations**:
- Mobile play patterns are sporadic
- Daily commute/break patterns common
- Weekend vs weekday differences

**Recommended Settings**:
```csharp
graceHours = 18f;         // Up from 6f - daily cycle
decayPerDay = 0.3f;       // Down from 0.5f - gentler
maxDecay = 2.5f;          // Up from 2f - more help
```

**Why**: Accounts for daily mobile gaming patterns without over-penalizing.

### 4. RageQuitModifier
**Purpose**: Detects frustration through quit patterns and adjusts accordingly.

**⚠️ CRITICAL FOR MOBILE**: Most important modifier to configure correctly.

**Mobile Puzzle Considerations**:
- Interruptions are common and normal
- Puzzle games require thinking time
- Physics puzzles need experimentation

**Recommended Settings**:
```csharp
rageQuitThreshold = 90f;  // Up from 30f - thinking time
rageQuitPenalty = 0.8f;   // Down from 1f - might be interruption
normalQuitPenalty = 0.3f; // Down from 0.5f
midPlayPenalty = 0.2f;    // Down from 0.3f
```

**Why**: 30 seconds is thinking time in puzzles, not rage quitting.

### 5. CompletionRateModifier
**Purpose**: Adjusts based on overall success rate across multiple attempts.

**Mobile Puzzle Considerations**:
- High retry rates are normal and healthy
- Recent performance more important than lifetime
- Puzzle difficulty can spike suddenly

**Recommended Settings**:
```csharp
minAttemptsRequired = 6;          // Down from 10
lowCompletionThreshold = 0.35f;   // Down from 0.4f
highCompletionThreshold = 0.75f;  // Up from 0.7f
totalStatsWeight = 0.2f;          // Focus on recent
```

**Why**: Faster response with more forgiving thresholds for puzzle games.

### 6. LevelProgressModifier
**Purpose**: Complex analysis of progression speed and performance patterns.

**Mobile Puzzle Considerations**:
- Physics puzzles take 3-6 minutes each
- Complexity increases non-linearly
- Interruptions affect completion time

**Recommended Settings**:
```csharp
expectedLevelsPerHour = 10;       // Down from 15
highAttemptsThreshold = 7;        // Up from 5
fastCompletionRatio = 0.6f;       // Down from 0.7f
slowCompletionRatio = 2.0f;       // Up from 1.5f
difficultyDecreasePerAttempt = 0.15f; // Gentler
```

**Why**: 15 levels/hour (4 min each) unrealistic for physics puzzles.

### 7. SessionPatternModifier
**Purpose**: Analyzes session behavior to detect engagement patterns.

**Mobile Puzzle Considerations**:
- Short sessions are normal (waiting in line, etc.)
- Interruptions don't indicate disengagement
- Multiple short sessions can be positive

**Recommended Settings**:
```csharp
veryShortSessionThreshold = 120f; // Up from 60f
minNormalSessionDuration = 240f;  // Up from 180f
veryShortSessionDecrease = 0.3f;  // Down from 0.5f
rageQuitCountThreshold = 3;       // Up from 2
```

**Why**: Mobile reality - 60 second sessions are normal check-ins.

## Implementation Example

```csharp
// In your DI configuration
public static class MobilePuzzleConfiguration
{
    public static void ConfigureForMobilePuzzle(this IContainerBuilder builder)
    {
        // Load and modify default configs
        var config = Resources.Load<DifficultyConfig>("GameConfigs/DifficultyConfig");

        // Apply mobile puzzle optimizations
        ApplyMobilePuzzleSettings(config);

        // Register the module
        builder.RegisterDynamicDifficulty();
    }

    private static void ApplyMobilePuzzleSettings(DifficultyConfig config)
    {
        // Win Streak
        var winConfig = config.ModifierConfigs.GetConfig<WinStreakConfig>();
        winConfig.SetThreshold(4f);
        winConfig.SetStepSize(0.4f);

        // Rage Quit - CRITICAL
        var rageConfig = config.ModifierConfigs.GetConfig<RageQuitConfig>();
        rageConfig.SetThreshold(90f); // Most important change

        // Level Progress
        var levelConfig = config.ModifierConfigs.GetConfig<LevelProgressConfig>();
        levelConfig.SetExpectedLevelsPerHour(10);

        // ... apply other settings
    }
}
```

## Testing Recommendations

### Key Metrics to Monitor
1. **Session Length Distribution**: Should peak at 3-5 minutes
2. **Rage Quit False Positives**: Track < 90 second quits
3. **Difficulty Oscillation**: Avoid ping-ponging
4. **Retention by Difficulty**: Track 1-day, 7-day retention

### A/B Testing Suggestions
- Test rage quit thresholds: 60s vs 90s vs 120s
- Test levels per hour: 8 vs 10 vs 12
- Test grace periods: 12h vs 18h vs 24h

## Common Pitfalls to Avoid

1. **Don't Penalize Thinking Time**: Puzzles require contemplation
2. **Don't Assume Continuous Play**: Mobile sessions are fragmented
3. **Don't Over-Adjust**: Small changes have big impacts
4. **Don't Ignore Interruptions**: They're part of mobile life
5. **Don't Use Action Game Settings**: Puzzle games are different

## Platform-Specific Notes

### iOS Considerations
- Background app refresh affects session tracking
- Game Center integration for progression
- Higher interruption rate from notifications

### Android Considerations
- More device variety affects performance
- Background restrictions vary by manufacturer
- Consider battery optimization impacts

## Conclusion

Mobile puzzle games require significantly different difficulty tuning than console or PC games. The key adjustments are:

1. **Longer time thresholds** for rage quit detection (90s+)
2. **Realistic level completion** expectations (10/hour not 15)
3. **Forgiving interruption** handling (120s+ for short sessions)
4. **Daily cycle awareness** (18h+ grace periods)
5. **Gentler adjustments** overall to account for mobile variance

These settings create a more enjoyable and fair experience for mobile puzzle game players while maintaining appropriate challenge progression.