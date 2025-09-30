# Modifier Reference - Dynamic User Difficulty

Comprehensive documentation for all 7 difficulty modifiers in the Dynamic User Difficulty system.

## Table of Contents

- [Overview](#overview)
- [Quick Reference](#quick-reference)
- [Modifier Details](#modifier-details)
  - [1. WinStreakModifier](#1-winstreakmodifier)
  - [2. LossStreakModifier](#2-lossstreakmodifier)
  - [3. TimeDecayModifier](#3-timedecaymodifier)
  - [4. RageQuitModifier](#4-ragequitmodifier)
  - [5. CompletionRateModifier](#5-completionratemodifier)
  - [6. LevelProgressModifier](#6-levelprogressmodifier)
  - [7. SessionPatternModifier](#7-sessionpatternmodifier)
- [Configuration Examples](#configuration-examples)
- [Mobile Optimization](#mobile-optimization)
- [Troubleshooting](#troubleshooting)

## Overview

The Dynamic User Difficulty system uses 7 comprehensive modifiers to analyze player behavior and adjust difficulty in real-time. Each modifier is stateless, deriving all data from external game services through provider interfaces.

### Modifier Architecture

- **Stateless Design**: All modifiers use `Calculate()` methods with NO parameters
- **Provider-Based**: Data comes from external services via interfaces
- **Type-Safe Configuration**: Each modifier has a strongly-typed config class
- **Mobile Optimized**: Designed for mobile puzzle games like Unscrew Factory

## Quick Reference

| Modifier | Purpose | Provider Required | Difficulty Impact | Priority |
|----------|---------|-------------------|-------------------|----------|
| **WinStreakModifier** | Increases difficulty on consecutive wins | `IWinStreakProvider` | +0.5 to +2.0 | 1 |
| **LossStreakModifier** | Decreases difficulty on consecutive losses | `IWinStreakProvider` | -0.3 to -1.5 | 2 |
| **TimeDecayModifier** | Reduces difficulty for returning players | `ITimeDecayProvider` | -0.5 to -2.0 | 3 |
| **CompletionRateModifier** | Adjusts based on overall success rate | `IWinStreakProvider`, `ILevelProgressProvider` | ±0.5 | 4 |
| **RageQuitModifier** | Compensates for player frustration | `IRageQuitProvider` | -0.3 to -1.0 | 5 |
| **LevelProgressModifier** | Analyzes progression patterns | `ILevelProgressProvider` | ±0.8 | 6 |
| **SessionPatternModifier** | Detects session behavior patterns | `IRageQuitProvider` | -0.4 to -1.0 | 7 |

## Modifier Details

### 1. WinStreakModifier

**Purpose**: Increases difficulty when players win consecutively to maintain challenge.

**Algorithm**: Linear increase based on consecutive wins above threshold.
```csharp
if (winStreak >= threshold)
{
    adjustment = (winStreak - threshold + 1) * stepSize;
    adjustment = min(adjustment, maxBonus);
}
```

#### Configuration Fields (WinStreakConfig)

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| `WinThreshold` | float | 3.0 | 1-10 | Number of consecutive wins to trigger increase |
| `StepSize` | float | 0.5 | 0.1-2.0 | Difficulty increase per win above threshold |
| `MaxBonus` | float | 2.0 | 0.5-5.0 | Maximum difficulty increase from win streaks |

#### Provider Dependencies

- **IWinStreakProvider**: `GetWinStreak()`

#### Usage Examples

```csharp
// Casual Game: Less aggressive
winThreshold: 5, stepSize: 0.3, maxBonus: 1.5

// Competitive Game: More aggressive
winThreshold: 2, stepSize: 0.7, maxBonus: 3.0

// Educational Game: Very gentle
winThreshold: 7, stepSize: 0.2, maxBonus: 1.0
```

#### Mobile Optimization Notes

- Keep `WinThreshold` at 3+ for mobile users who play in short bursts
- Use moderate `StepSize` (0.3-0.5) to avoid frustrating casual players
- Cap `MaxBonus` at 2.0 to prevent difficulty spikes

---

### 2. LossStreakModifier

**Purpose**: Reduces difficulty when players lose consecutively to prevent frustration.

**Algorithm**: Linear decrease based on consecutive losses above threshold.
```csharp
if (lossStreak >= threshold)
{
    adjustment = -(lossStreak - threshold + 1) * stepSize;
    adjustment = max(adjustment, -maxReduction);
}
```

#### Configuration Fields (LossStreakConfig)

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| `LossThreshold` | float | 2.0 | 1-10 | Number of consecutive losses to trigger decrease |
| `StepSize` | float | 0.3 | 0.1-2.0 | Difficulty decrease per loss above threshold |
| `MaxReduction` | float | 1.5 | 0.5-5.0 | Maximum difficulty reduction from loss streaks |

#### Provider Dependencies

- **IWinStreakProvider**: `GetLossStreak()`

#### Usage Examples

```csharp
// Quick Response: Immediate help
lossThreshold: 1, stepSize: 0.5, maxReduction: 2.0

// Standard Mobile: Balanced approach
lossThreshold: 2, stepSize: 0.3, maxReduction: 1.5

// Challenge Mode: Less forgiving
lossThreshold: 4, stepSize: 0.2, maxReduction: 1.0
```

#### Mobile Optimization Notes

- Use low `LossThreshold` (2-3) since mobile sessions are short
- Moderate `StepSize` provides quick relief without making game too easy
- Higher `MaxReduction` helps recover from bad streaks

---

### 3. TimeDecayModifier

**Purpose**: Reduces difficulty for players returning after time away to ease them back into the game.

**Algorithm**: Exponential decay based on time elapsed since last play.
```csharp
if (timeSinceLastPlay > graceHours)
{
    daysAway = (timeSinceLastPlay - graceHours) / 24;
    adjustment = -min(daysAway * decayPerDay, maxDecay);
}
```

#### Configuration Fields (TimeDecayConfig)

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| `DecayPerDay` | float | 0.5 | 0.1-2.0 | Difficulty reduction per day of inactivity |
| `MaxDecay` | float | 2.0 | 0.5-5.0 | Maximum total difficulty reduction from time decay |
| `GraceHours` | float | 6.0 | 0-48 | Hours before decay starts (grace period) |

#### Provider Dependencies

- **ITimeDecayProvider**: `GetTimeSinceLastPlay()`, `GetLastPlayTime()`, `GetDaysAwayFromGame()`

#### Usage Examples

```csharp
// Aggressive Retention: Quick to help returning players
decayPerDay: 0.8, maxDecay: 3.0, graceHours: 2

// Standard Mobile: Balanced decay
decayPerDay: 0.5, maxDecay: 2.0, graceHours: 6

// Hardcore Game: Minimal help
decayPerDay: 0.2, maxDecay: 1.0, graceHours: 24
```

#### Mobile Optimization Notes

- Set `GraceHours` to 6-12 for mobile patterns (daily play)
- Use moderate `DecayPerDay` (0.3-0.7) for gradual re-engagement
- Cap `MaxDecay` to prevent making game too easy on return

---

### 4. RageQuitModifier

**Purpose**: Detects and compensates for rage quit behavior by reducing difficulty.

**Algorithm**: Different penalties based on quit type and timing.
```csharp
if (quitType == RageQuit && sessionTime < threshold)
{
    adjustment = -rageQuitReduction;
}
else if (quitType == MidPlay)
{
    adjustment = -midPlayReduction;
}
else if (quitType == Normal)
{
    adjustment = -quitReduction;
}
```

#### Configuration Fields (RageQuitConfig)

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| `RageQuitThreshold` | float | 30.0 | 5-120 | Time threshold (seconds) to consider rage quit |
| `RageQuitReduction` | float | 1.0 | 0.5-3.0 | Difficulty reduction for rage quit |
| `QuitReduction` | float | 0.5 | 0.1-2.0 | Difficulty reduction for normal quit |
| `MidPlayReduction` | float | 0.3 | 0.1-1.0 | Difficulty reduction for mid-play quit |

#### Provider Dependencies

- **IRageQuitProvider**: `GetLastQuitType()`, `GetCurrentSessionDuration()`, `GetRecentRageQuitCount()`

#### Usage Examples

```csharp
// High Sensitivity: Quick to detect frustration
rageQuitThreshold: 15, rageQuitReduction: 1.5, quitReduction: 0.7

// Mobile Standard: Balanced detection
rageQuitThreshold: 30, rageQuitReduction: 1.0, quitReduction: 0.5

// Less Aggressive: Minimal intervention
rageQuitThreshold: 60, rageQuitReduction: 0.5, quitReduction: 0.2
```

#### Mobile Optimization Notes

- Lower `RageQuitThreshold` (15-30s) for mobile attention spans
- Higher penalties help retention after frustration events
- Track interruptions (calls, notifications) vs actual rage quits

---

### 5. CompletionRateModifier

**Purpose**: Analyzes overall player success rate and adjusts difficulty accordingly.

**Algorithm**: Compares completion rate against thresholds with weighted total stats.
```csharp
recentRate = recentWins / (recentWins + recentLosses);
totalRate = totalWins / (totalWins + totalLosses);
combinedRate = (recentRate * (1 - weight)) + (totalRate * weight);

if (combinedRate < lowThreshold)
{
    adjustment = -lowCompletionDecrease;
}
else if (combinedRate > highThreshold)
{
    adjustment = highCompletionIncrease;
}
```

#### Configuration Fields (CompletionRateConfig)

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| `LowCompletionThreshold` | float | 0.4 | 0-1 | Minimum completion rate before difficulty decreases |
| `HighCompletionThreshold` | float | 0.7 | 0-1 | Maximum completion rate before difficulty increases |
| `LowCompletionDecrease` | float | 0.5 | 0.1-2.0 | Difficulty decrease when below low threshold |
| `HighCompletionIncrease` | float | 0.5 | 0.1-2.0 | Difficulty increase when above high threshold |
| `MinAttemptsRequired` | int | 10 | 5-50 | Minimum attempts before modifier activates |
| `TotalStatsWeight` | float | 0.3 | 0-1 | Weight factor for total wins/losses consideration |

#### Provider Dependencies

- **IWinStreakProvider**: `GetTotalWins()`, `GetTotalLosses()`
- **ILevelProgressProvider**: `GetCompletionRate()`

#### Usage Examples

```csharp
// Tight Control: Narrow success band
lowThreshold: 0.5, highThreshold: 0.6, adjustments: ±0.3

// Standard Mobile: Wide tolerance band
lowThreshold: 0.4, highThreshold: 0.7, adjustments: ±0.5

// Loose Control: Very wide band
lowThreshold: 0.3, highThreshold: 0.8, adjustments: ±0.7
```

#### Mobile Optimization Notes

- Use wide threshold bands (0.4-0.7) for mobile's variable play quality
- Low `MinAttemptsRequired` (5-10) for quick activation
- Moderate `TotalStatsWeight` (0.2-0.4) balances recent vs historical performance

---

### 6. LevelProgressModifier

**Purpose**: Analyzes detailed level progression including attempts, completion time, and progression speed.

**Algorithm**: Multi-factor analysis combining attempts, time performance, progression rate, and mastery detection.
```csharp
// 1. Attempts Analysis
if (attempts > threshold) adjustment -= (attempts - threshold) * decreasePerAttempt;

// 2. Time Performance (using PercentUsingTimeToComplete)
if (timePercentage < fastRatio) adjustment += bonus * (1 - timePercentage);
else if (timePercentage > slowRatio) adjustment -= penalty * (timePercentage - 1);

// 3. Progression Analysis
expectedLevel = estimatedHours * levelsPerHour;
adjustment += (currentLevel - expectedLevel) * progressionFactor;

// 4. Mastery/Struggle Detection
if (hardLevel && highCompletion) adjustment += masteryBonus;
else if (easyLevel && lowCompletion) adjustment -= strugglePenalty;
```

#### Configuration Fields (LevelProgressConfig)

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| **Attempts Settings** | | | | |
| `HighAttemptsThreshold` | int | 5 | 3-10 | Number of attempts indicating struggle |
| `DifficultyDecreasePerAttempt` | float | 0.2 | 0.1-0.5 | Decrease per attempt over threshold |
| **Completion Time Settings** | | | | |
| `FastCompletionRatio` | float | 0.7 | 0.3-0.9 | Percentage of avg time considered fast |
| `SlowCompletionRatio` | float | 1.5 | 1.1-2.0 | Percentage of avg time considered slow |
| `FastCompletionBonus` | float | 0.3 | 0.1-1.0 | Difficulty increase for fast completion |
| `SlowCompletionPenalty` | float | 0.3 | 0.1-1.0 | Difficulty decrease for slow completion |
| **Level Progression Settings** | | | | |
| `ExpectedLevelsPerHour` | int | 15 | 5-30 | Expected progression rate |
| `LevelProgressionFactor` | float | 0.1 | 0.05-0.2 | Adjustment based on progression difference |
| **Performance Settings** | | | | |
| `MaxPenaltyMultiplier` | float | 1.0 | 0.5-1.5 | Cap for slow completion penalty |
| `FastCompletionMultiplier` | float | 1.0 | 0.5-2.0 | Multiplier for fast completion bonus |
| **Mastery/Struggle Detection** | | | | |
| `HardLevelThreshold` | float | 3.0 | 2-5 | Minimum difficulty for mastery detection |
| `MasteryCompletionRate` | float | 0.7 | 0.5-1.0 | Completion rate threshold for mastery |
| `MasteryBonus` | float | 0.3 | 0.1-0.5 | Difficulty increase for mastering hard levels |
| `EasyLevelThreshold` | float | 2.0 | 1-3 | Maximum difficulty for struggle detection |
| `StruggleCompletionRate` | float | 0.3 | 0.1-0.5 | Completion rate threshold for struggle |
| `StrugglePenalty` | float | 0.3 | 0.1-0.5 | Difficulty decrease for struggling on easy |

#### Provider Dependencies

- **ILevelProgressProvider**: `GetCurrentLevel()`, `GetAverageCompletionTime()`, `GetAttemptsOnCurrentLevel()`, `GetCompletionRate()`, `GetCurrentLevelDifficulty()`, `GetCurrentLevelTimePercentage()`

#### Usage Examples

```csharp
// Casual Mobile: Forgiving progression
attemptsThreshold: 7, fastRatio: 0.8, expectedLevelsPerHour: 10

// Standard Mobile: Balanced progression
attemptsThreshold: 5, fastRatio: 0.7, expectedLevelsPerHour: 15

// Competitive: Tight progression control
attemptsThreshold: 3, fastRatio: 0.6, expectedLevelsPerHour: 20
```

#### Mobile Optimization Notes

- **Enhanced Time Analysis**: Uses `PercentUsingTimeToComplete` for accurate mobile timing
- Moderate attempt thresholds (4-6) account for mobile interruptions
- Lower expected progression (8-15 levels/hour) for mobile play patterns
- Mastery detection helps identify skilled players on mobile

---

### 7. SessionPatternModifier

**Purpose**: Analyzes session duration patterns and behavior to detect frustration or disengagement.

**Algorithm**: Pattern analysis across recent sessions with rage quit integration.
```csharp
// 1. Very Short Session Detection
if (currentSessionDuration < veryShortThreshold)
{
    adjustment = -veryShortSessionDecrease;
}

// 2. Pattern Analysis
shortSessions = countShortSessions(recentSessions);
if (shortSessions / totalSessions > shortSessionRatio)
{
    adjustment -= consistentShortSessionsDecrease;
}

// 3. Rage Quit Pattern
if (rageQuitCount >= threshold)
{
    adjustment -= rageQuitPatternDecrease * penaltyMultiplier;
}
```

#### Configuration Fields (SessionPatternConfig)

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| **Session Duration Settings** | | | | |
| `MinNormalSessionDuration` | float | 180.0 | 60-600 | Minimum duration for normal session (seconds) |
| `VeryShortSessionThreshold` | float | 60.0 | 30-120 | Duration considered very short (seconds) |
| `VeryShortSessionDecrease` | float | 0.5 | 0.2-1.0 | Difficulty decrease for very short sessions |
| **Pattern Detection** | | | | |
| `SessionHistorySize` | int | 5 | 3-10 | Number of recent sessions to analyze |
| `ShortSessionRatio` | float | 0.5 | 0.3-0.8 | Percentage of sessions that must be short |
| `ConsistentShortSessionsDecrease` | float | 0.8 | 0.3-1.5 | Adjustment for consistent short sessions |
| **Rage Quit Analysis** | | | | |
| `RageQuitTimeThreshold` | float | 30.0 | 10-60 | Time threshold for rage quit detection |
| `RageQuitCountThreshold` | int | 2 | 1-5 | Minimum rage quits to trigger pattern |
| `RageQuitPatternDecrease` | float | 1.0 | 0.5-2.0 | Difficulty decrease for rage quit patterns |
| `RageQuitPenaltyMultiplier` | float | 0.5 | 0.1-1.0 | Multiplier for recent rage quit penalty |
| **Quit Behavior** | | | | |
| `MidLevelQuitDecrease` | float | 0.4 | 0.2-1.0 | Difficulty decrease for mid-level quits |
| `MidLevelQuitRatio` | float | 0.3 | 0.2-0.6 | Ratio of mid-level quits to trigger adjustment |

#### Provider Dependencies

- **IRageQuitProvider**: `GetCurrentSessionDuration()`, `GetAverageSessionDuration()`, `GetRecentRageQuitCount()`, `GetLastQuitType()`

#### Usage Examples

```csharp
// High Sensitivity: Quick pattern detection
sessionHistorySize: 3, shortSessionRatio: 0.4, veryShortThreshold: 45

// Standard Mobile: Balanced pattern detection
sessionHistorySize: 5, shortSessionRatio: 0.5, veryShortThreshold: 60

// Conservative: Slower to react
sessionHistorySize: 8, shortSessionRatio: 0.6, veryShortThreshold: 90
```

#### Mobile Optimization Notes

- Lower thresholds (60-90s) accommodate mobile interruptions vs true frustration
- Small history size (3-5 sessions) for quick adaptation
- Distinguish between interruptions and intentional short sessions

---

## Configuration Examples

### Casual Mobile Puzzle Game
```yaml
WinStreak:
  threshold: 4, stepSize: 0.3, maxBonus: 1.5

LossStreak:
  threshold: 2, stepSize: 0.4, maxReduction: 2.0

TimeDecay:
  decayPerDay: 0.6, maxDecay: 2.5, graceHours: 8

CompletionRate:
  lowThreshold: 0.35, highThreshold: 0.75, adjustments: ±0.4

LevelProgress:
  attemptsThreshold: 6, expectedLevelsPerHour: 12, fastRatio: 0.75
```

### Competitive Puzzle Game
```yaml
WinStreak:
  threshold: 2, stepSize: 0.6, maxBonus: 2.5

LossStreak:
  threshold: 3, stepSize: 0.3, maxReduction: 1.5

TimeDecay:
  decayPerDay: 0.3, maxDecay: 1.5, graceHours: 12

CompletionRate:
  lowThreshold: 0.45, highThreshold: 0.65, adjustments: ±0.6

LevelProgress:
  attemptsThreshold: 4, expectedLevelsPerHour: 18, fastRatio: 0.65
```

### Educational Puzzle Game
```yaml
WinStreak:
  threshold: 6, stepSize: 0.2, maxBonus: 1.0

LossStreak:
  threshold: 2, stepSize: 0.5, maxReduction: 2.5

TimeDecay:
  decayPerDay: 0.8, maxDecay: 3.0, graceHours: 4

CompletionRate:
  lowThreshold: 0.3, highThreshold: 0.8, adjustments: ±0.3

LevelProgress:
  attemptsThreshold: 8, expectedLevelsPerHour: 8, fastRatio: 0.8
```

## Mobile Optimization

### Key Mobile Considerations

1. **Short Session Tolerance**: Mobile players often have interrupted sessions
2. **Variable Attention**: Performance varies more on mobile due to distractions
3. **Retention Focus**: More aggressive help for struggling players
4. **Battery Awareness**: Efficient calculations to preserve battery life

### Recommended Mobile Settings

- **Win Thresholds**: 3-5 wins (higher than PC due to shorter sessions)
- **Loss Response**: Quick (1-2 losses) but gentle decreases
- **Time Decay**: 6-12 hour grace periods for daily play patterns
- **Session Analysis**: Distinguish interruptions from frustration
- **Progress Expectations**: Lower levels/hour (8-15 vs 20+ on PC)

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Modifiers not activating | Provider not implemented | Verify provider interfaces are registered in DI |
| Difficulty changes too aggressive | Configuration values too high | Reduce step sizes and max adjustments |
| Not enough sensitivity | Thresholds too high | Lower trigger thresholds |
| Conflicting adjustments | Multiple modifiers fighting | Review priority order and adjust weights |
| Mobile performance issues | Expensive provider calls | Cache provider data and update strategically |

### Debug Tips

1. **Enable Debug Logging**: Set logging level to Debug in modifier configs
2. **Monitor Metadata**: Check ModifierResult.Metadata for detailed calculations
3. **Test Provider Data**: Verify provider methods return expected values
4. **Validate Configurations**: Use DifficultyConfigValidator in editor
5. **Profile Performance**: Monitor Calculate() execution times

### Validation Checklist

- [ ] All required providers are implemented and registered
- [ ] Configuration values are within recommended ranges
- [ ] Priority order matches game's difficulty philosophy
- [ ] Mobile-specific considerations are addressed
- [ ] Debug logging is disabled in production builds
- [ ] Performance testing confirms <10ms calculation times

---

*For more detailed information on specific modifiers, see the individual modifier documentation files in `Documentation/Modifiers/`.*