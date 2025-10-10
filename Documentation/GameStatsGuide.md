# GameStats Configuration Guide

## 📋 Table of Contents

1. [Overview](#overview)
2. [Critical Distinction: Target vs Current](#critical-distinction-target-vs-current)
3. [Understanding Each Field](#understanding-each-field)
4. [How to Choose Values](#how-to-choose-values)
5. [Common Mistakes](#common-mistakes)
6. [Examples by Game Type](#examples-by-game-type)
7. [Iteration Workflow](#iteration-workflow)
8. [FAQ](#faq)

---

## Overview

The `GameStats` struct is used to **automatically generate optimal modifier configurations** based on your game design goals. Understanding what these fields represent is critical for proper system configuration.

### What GameStats Is

GameStats represents **your game design targets** - the player experience you want to achieve.

### What GameStats Is NOT

GameStats is **NOT** your current player analytics data - it's not a performance report.

---

## Critical Distinction: Target vs Current

### ⚠️ The Most Common Mistake

Many developers mistakenly use their **current analytics data** instead of **design targets**.

#### ❌ WRONG Approach (Using Current Stats)

```csharp
// Analytics show players currently win 1.2 levels in a row
// Developer thinks: "I'll use what I'm seeing"
gameStats.avgConsecutiveWins = 1.2f;

// PROBLEM: This tells the system "keep players at 1.2 wins"
// The system will INCREASE difficulty to maintain this low number!
```

#### ✅ CORRECT Approach (Using Design Targets)

```csharp
// Analytics show players win 1.2 levels in a row
// But I WANT them to win 3-4 levels for better engagement
gameStats.avgConsecutiveWins = 3.5f;

// SUCCESS: The system will REDUCE difficulty to achieve your target!
```

### How It Works

```
┌─────────────────┐
│  GameStats      │ ← Your design targets (what you WANT)
│  (Input)        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Generation     │ ← System calculates thresholds
│  Algorithm      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Modifier       │ ← Applied during gameplay
│  Configs        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Player         │ ← Actual behavior (should match targets)
│  Experience     │
└─────────────────┘
```

---

## Understanding Each Field

### Player Behavior Stats (4 fields)

#### avgConsecutiveWins
- **Type**: `float`
- **Meaning**: **How many wins you WANT players to achieve** before a loss
- **Purpose**: Defines engagement sweet spot
- **Typical Values**: 2.5 - 5.0
- **Example**:
  - `3.5` = "I want players to win 3-4 levels before losing"
  - System will increase difficulty after 3 wins to keep average around 3.5

#### avgConsecutiveLosses
- **Type**: `float`
- **Meaning**: **How many losses you WANT** before a win (frustration tolerance)
- **Purpose**: Prevents player churn from frustration
- **Typical Values**: 1.5 - 3.0
- **Example**:
  - `2.0` = "I want only 2 losses before they succeed"
  - System will reduce difficulty after 2 losses to help players win

#### winRatePercentage
- **Type**: `float` (0-100)
- **Meaning**: **Target overall success rate** for optimal engagement
- **Purpose**: Balances challenge and achievement
- **Typical Values**:
  - Casual: 70-80%
  - Mid-Core: 60-70%
  - Hardcore: 50-60%
- **Example**:
  - `65` = "I want players winning 65% of the time"

#### avgAttemptsPerLevel
- **Type**: `float`
- **Meaning**: **Target number of attempts** per level before completion
- **Purpose**: Allows learning without frustration
- **Typical Values**: 1.5 - 3.0
- **Example**:
  - `2.5` = "I want players to need 2-3 tries on average"

---

### Session & Time Stats (4 fields)

#### avgHoursBetweenSessions
- **Type**: `float`
- **Meaning**: **Expected time between play sessions** (retention target)
- **Purpose**: Influences time decay calculations
- **Typical Values**:
  - Daily games: 24 hours
  - Casual games: 48-72 hours
  - Hardcore games: 12-24 hours
- **Example**:
  - `24` = "I want daily active users"

#### avgSessionDurationMinutes
- **Type**: `float`
- **Meaning**: **Target length of play sessions**
- **Purpose**: Defines engagement window
- **Typical Values**:
  - Mobile casual: 5-15 minutes
  - Mobile mid-core: 15-30 minutes
  - PC/Console: 30-60+ minutes
- **Example**:
  - `15` = "I want 15-minute sessions (mobile sweet spot)"

#### avgLevelsPerSession
- **Type**: `float`
- **Meaning**: **How many levels you WANT completed** per session
- **Purpose**: Defines progression pacing
- **Typical Values**: 3-10 levels
- **Calculation**: `avgSessionDurationMinutes / avgLevelCompletionTimeSeconds * 60`
- **Example**:
  - `5` = "I want players to complete 5 levels per session"

#### rageQuitPercentage
- **Type**: `float` (0-100)
- **Meaning**: **Acceptable frustration quit rate**
- **Purpose**: Some frustration is normal; this sets tolerance
- **Typical Values**: 5-15%
- **Example**:
  - `10` = "I accept 10% of sessions ending in frustration"

---

### Level Design Stats (4 fields)

#### difficultyMin
- **Type**: `float`
- **Meaning**: **Minimum difficulty value** in your game's scale
- **Purpose**: Defines easiest possible setting
- **Typical Values**: 0.0, 1.0, or your game's minimum
- **Example**:
  - `1.0` = "My easiest difficulty setting"

#### difficultyMax
- **Type**: `float`
- **Meaning**: **Maximum difficulty value** in your game's scale
- **Purpose**: Defines hardest possible setting
- **Typical Values**: 5.0, 10.0, or your game's maximum
- **Example**:
  - `10.0` = "My hardest difficulty setting"

#### difficultyDefault
- **Type**: `float`
- **Meaning**: **Starting difficulty** for new players
- **Purpose**: Initial player experience
- **Typical Values**: 20-40% of range (easier side)
- **Example**:
  - `3.0` (in 1-10 range) = "Gentle introduction for new players"

#### avgLevelCompletionTimeSeconds
- **Type**: `float`
- **Meaning**: **Target time to complete a level**
- **Purpose**: Defines pacing and session length calculations
- **Typical Values**: 30-120 seconds for mobile
- **Example**:
  - `60` = "I want levels to take about 1 minute"

---

### Progression Stats (5 fields)

#### totalLevels
- **Type**: `int`
- **Meaning**: **Total number of levels** in your game
- **Purpose**: Defines content scope
- **Typical Values**: 50-500+ levels
- **Example**:
  - `100` = "My game has 100 levels"

#### difficultyIncreaseStartLevel
- **Type**: `int`
- **Meaning**: **When difficulty ramp should begin**
- **Purpose**: Allows tutorial/learning period
- **Typical Values**: 10-25% of total levels
- **Example**:
  - `20` = "Start increasing difficulty after level 20"

#### targetRetentionDays
- **Type**: `int`
- **Meaning**: **Retention goal for time decay**
- **Purpose**: How long before returning players get full decay benefit
- **Typical Values**: 3-14 days
- **Example**:
  - `7` = "I want to retain weekly active users"

#### maxDifficultyChangePerSession
- **Type**: `float`
- **Meaning**: **Maximum difficulty adjustment** in one session
- **Purpose**: Prevents sudden spikes/drops
- **Typical Values**: 1.0-3.0
- **Example**:
  - `2.0` = "Difficulty can't change more than 2 points per session"

#### gameCompletionRate
- **Type**: `float` (0-100)
- **Meaning**: **Target percentage of players** who complete the game
- **Purpose**: Defines end-game difficulty
- **Typical Values**:
  - Casual: 30-50%
  - Mid-Core: 10-30%
  - Hardcore: 1-10%
- **Example**:
  - `5` = "I want 5% completion rate (challenging game)"

---

## How to Choose Values

### 1. Start with Industry Benchmarks

**Mobile Puzzle Games (Like Screw3D):**

```csharp
GameStats benchmarks = new GameStats
{
    // Player Behavior
    avgConsecutiveWins = 3.5f,        // 3-4 wins before loss
    avgConsecutiveLosses = 2.0f,      // 2 losses before win
    winRatePercentage = 65f,          // 65% success rate
    avgAttemptsPerLevel = 2.5f,       // 2-3 tries per level

    // Session & Time
    avgHoursBetweenSessions = 24f,    // Daily players
    avgSessionDurationMinutes = 15f,  // 15-minute sessions
    avgLevelsPerSession = 5f,         // 5 levels per session
    rageQuitPercentage = 10f,         // 10% rage quit rate

    // Level Design
    difficultyMin = 1f,               // 1-10 scale
    difficultyMax = 10f,
    difficultyDefault = 3f,           // Easy start
    avgLevelCompletionTimeSeconds = 60f, // 1 minute per level

    // Progression
    totalLevels = 100,                // 100 levels
    difficultyIncreaseStartLevel = 20, // Ramp after level 20
    targetRetentionDays = 7,          // Weekly retention
    maxDifficultyChangePerSession = 2f, // Max +/-2 change
    gameCompletionRate = 5f           // 5% completion
};
```

### 2. Adjust for Your Game's Philosophy

#### Casual Game (High Accessibility)
```csharp
avgConsecutiveWins = 5.0f;      // Longer win streaks
avgConsecutiveLosses = 1.5f;    // Fewer losses tolerated
winRatePercentage = 75f;        // Higher win rate
```

#### Mid-Core Game (Balanced)
```csharp
avgConsecutiveWins = 3.5f;      // Moderate win streaks
avgConsecutiveLosses = 2.0f;    // Moderate loss tolerance
winRatePercentage = 65f;        // Balanced win rate
```

#### Hardcore Game (High Challenge)
```csharp
avgConsecutiveWins = 2.0f;      // Shorter win streaks
avgConsecutiveLosses = 3.0f;    // Higher loss tolerance
winRatePercentage = 55f;        // Lower win rate
```

### 3. Consider Your Monetization

#### Ad-Supported (Free-to-Play)
```csharp
avgSessionDurationMinutes = 10f;  // Shorter sessions, more frequent
avgHoursBetweenSessions = 12f;    // Multiple sessions per day
rageQuitPercentage = 5f;          // Low tolerance (fewer ad impressions)
```

#### IAP-Focused (In-App Purchases)
```csharp
avgSessionDurationMinutes = 20f;  // Longer sessions
avgHoursBetweenSessions = 24f;    // Daily sessions
avgConsecutiveLosses = 3.0f;      // Allow more frustration (sell boosters)
```

### 4. Platform Considerations

#### Mobile
```csharp
avgSessionDurationMinutes = 10-15f; // Shorter attention span
avgLevelCompletionTimeSeconds = 45-60f; // Quick levels
avgLevelsPerSession = 5-8f;         // Multiple levels per session
```

#### PC/Console
```csharp
avgSessionDurationMinutes = 30-60f; // Longer sessions
avgLevelCompletionTimeSeconds = 120-300f; // Longer levels
avgLevelsPerSession = 3-5f;         // Fewer, longer levels
```

---

## Common Mistakes

### ❌ Mistake #1: Using Current Analytics Data

**Problem:**
```csharp
// From analytics: players currently win 52% of levels
gameStats.winRatePercentage = 52f;  // WRONG!
```

**Solution:**
```csharp
// I WANT 65% win rate for better engagement
gameStats.winRatePercentage = 65f;  // CORRECT!
```

### ❌ Mistake #2: Unrealistic Targets

**Problem:**
```csharp
gameStats.avgConsecutiveWins = 10f;   // Too high
gameStats.avgConsecutiveLosses = 0.5f; // Impossible
gameStats.winRatePercentage = 95f;    // Too easy
```

**Solution:**
```csharp
// Realistic, achievable targets
gameStats.avgConsecutiveWins = 3.5f;
gameStats.avgConsecutiveLosses = 2.0f;
gameStats.winRatePercentage = 65f;
```

### ❌ Mistake #3: Inconsistent Values

**Problem:**
```csharp
avgSessionDurationMinutes = 15f;      // 15 minutes
avgLevelsPerSession = 20f;            // 20 levels
avgLevelCompletionTimeSeconds = 120f; // 2 minutes per level
// Math doesn't work: 20 levels * 2 min = 40 minutes ≠ 15 minutes!
```

**Solution:**
```csharp
avgSessionDurationMinutes = 15f;      // 15 minutes
avgLevelCompletionTimeSeconds = 60f;  // 1 minute per level
avgLevelsPerSession = 10f;            // 10 levels
// Math works: 10 levels * 1 min = 10 minutes (within 15-min session)
```

### ❌ Mistake #4: Ignoring Game Type

**Problem:**
```csharp
// Hardcore roguelike with casual mobile targets
avgConsecutiveWins = 5.0f;  // Too easy for roguelike
winRatePercentage = 75f;    // Way too high
```

**Solution:**
```csharp
// Appropriate for hardcore roguelike
avgConsecutiveWins = 2.0f;
winRatePercentage = 45f;    // Challenge is the appeal
```

---

## Examples by Game Type

### Example 1: Casual Mobile Puzzle (Like Screw3D)

```csharp
var casualPuzzle = GameStats.CreateDefault();
casualPuzzle.avgConsecutiveWins = 4.0f;
casualPuzzle.avgConsecutiveLosses = 2.0f;
casualPuzzle.winRatePercentage = 70f;
casualPuzzle.avgSessionDurationMinutes = 12f;
casualPuzzle.avgLevelsPerSession = 6f;
casualPuzzle.avgLevelCompletionTimeSeconds = 45f;
```

**Rationale:**
- Higher win rate (70%) keeps casual players engaged
- Moderate win streaks (4) provide satisfaction
- Short levels (45s) fit mobile attention spans
- Multiple levels per session create progression feel

### Example 2: Mid-Core Strategy Game

```csharp
var midCoreStrategy = new GameStats
{
    avgConsecutiveWins = 3.0f,
    avgConsecutiveLosses = 2.5f,
    winRatePercentage = 60f,
    avgSessionDurationMinutes = 25f,
    avgLevelsPerSession = 4f,
    avgLevelCompletionTimeSeconds = 180f, // 3 minutes
    avgHoursBetweenSessions = 12f, // Twice daily
    // ... other fields
};
```

**Rationale:**
- Balanced win rate (60%) rewards skill
- Longer levels (3 min) allow strategic depth
- Shorter session intervals (12h) for engaged players

### Example 3: Hardcore Roguelike

```csharp
var hardcoreRoguelike = new GameStats
{
    avgConsecutiveWins = 1.5f,
    avgConsecutiveLosses = 4.0f,
    winRatePercentage = 45f,
    avgSessionDurationMinutes = 45f,
    avgLevelsPerSession = 2f,
    avgLevelCompletionTimeSeconds = 600f, // 10 minutes
    gameCompletionRate = 2f, // Only 2% beat the game
    // ... other fields
};
```

**Rationale:**
- Low win rate (45%) creates challenge
- High loss tolerance (4) fits roguelike expectations
- Long levels (10 min) for deep runs

---

## Iteration Workflow

### Phase 1: Initial Configuration

1. **Define Your Design Goals**
   ```
   - What player experience do I want?
   - What's my monetization model?
   - Who is my target audience?
   ```

2. **Set Initial GameStats**
   ```csharp
   var stats = GameStats.CreateDefault();
   // Adjust based on your answers above
   ```

3. **Generate Configs**
   ```
   Unity Inspector → DifficultyConfig → Fill GameStats → Generate
   ```

### Phase 2: Launch & Measure

1. **Launch to Test Group**
   ```
   - Soft launch or limited beta
   - 500-1000 players minimum
   - 7-14 days of data collection
   ```

2. **Collect Analytics**
   ```
   Track:
   - Actual win rate vs target
   - Actual session duration vs target
   - Actual retention vs target
   - Player feedback/reviews
   ```

### Phase 3: Analyze & Adjust

1. **Compare Actual vs Target**
   ```
   Target Win Rate: 65%
   Actual Win Rate: 52%
   → Difficulty is too hard, increase target to 70%
   ```

2. **Update GameStats**
   ```csharp
   // Increase target to reduce difficulty
   stats.winRatePercentage = 70f;
   ```

3. **Regenerate Configs**
   ```
   Unity Inspector → Generate All Configs from Stats
   ```

### Phase 4: Iterate

1. **Deploy Updated Config**
2. **Measure Again** (7-14 days)
3. **Repeat Until Targets Met**

### Example Iteration Cycle

```
Iteration 1:
  Target: 65% win rate
  Actual: 52% win rate
  Action: Increase to 70%

Iteration 2:
  Target: 70% win rate
  Actual: 68% win rate
  Action: Keep at 70% (close enough)

Final:
  Target: 70% win rate
  Actual: 69% win rate ✅
  Status: Production ready
```

---

## FAQ

### Q: Should I use my current player data or my design targets?

**A:** Always use your **design targets** (what you WANT), not current data (what IS happening).

### Q: What if I don't have any players yet?

**A:** Start with industry benchmarks for your game type, then iterate after launch.

### Q: How often should I regenerate configs?

**A:**
- Initially: After each major playtest
- Post-launch: Every 2-4 weeks based on analytics
- Mature game: Only when adding new content or changing monetization

### Q: Can I manually adjust configs after generation?

**A:** Yes! Generated configs are starting points. You can fine-tune individual modifiers afterward.

### Q: What if my actual metrics never match my targets?

**A:** Your targets might be unrealistic for your game type. Consider:
- Are you a casual game with hardcore targets?
- Are your levels inherently harder/easier than targets suggest?
- Does your monetization model conflict with targets?

### Q: How do I know if my targets are good?

**A:** Compare to industry benchmarks, then test with real players. Good targets should:
- Feel achievable to players
- Create engaging gameplay loops
- Support your monetization model
- Maintain retention

### Q: Should avgLevelsPerSession * avgLevelCompletionTime = avgSessionDuration?

**A:** Not exactly. Session time includes:
- Level completion time
- Menu navigation
- Failed attempts
- Watching ads (if applicable)

A good rule: `avgLevelsPerSession * avgLevelCompletionTime ≈ 60-80% of avgSessionDuration`

### Q: What if I want different difficulty for different player segments?

**A:** The automatic generation creates a baseline. For segmentation:
1. Generate baseline configs from primary audience targets
2. Use A/B testing tools to create variants
3. Manually adjust specific modifier configs for each segment

---

## Quick Reference Card

### For Mobile Puzzle Games (Screw3D-like)

| Stat | Casual | Mid-Core | Hardcore |
|------|--------|----------|----------|
| avgConsecutiveWins | 4-5 | 3-4 | 2-3 |
| avgConsecutiveLosses | 1.5-2 | 2-2.5 | 3-4 |
| winRatePercentage | 70-80% | 60-70% | 50-60% |
| avgSessionDurationMinutes | 10-15 | 15-25 | 20-40 |
| avgLevelsPerSession | 6-10 | 4-8 | 3-6 |
| rageQuitPercentage | 5-8% | 8-12% | 12-20% |

---

## Related Documentation

- [Implementation Guide](ImplementationGuide.md) - How to integrate the system
- [API Reference](APIReference.md) - Complete API documentation
- [Modifier Guide](ModifierGuide.md) - Understanding modifier behavior
- [Integration Guide](IntegrationGuide.md) - Integration with UITemplate

---

**Last Updated:** 2025-10-10
**Version:** 2.0.1
