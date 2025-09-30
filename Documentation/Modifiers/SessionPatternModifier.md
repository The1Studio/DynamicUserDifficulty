# SessionPatternModifier - Detailed Documentation

The SessionPatternModifier analyzes player session behavior patterns to detect frustration, disengagement, and optimal engagement levels, particularly important for mobile puzzle games where session patterns reveal player satisfaction.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Analysis Components](#analysis-components)
- [Configuration Deep Dive](#configuration-deep-dive)
- [Algorithm Details](#algorithm-details)
- [Mobile Pattern Recognition](#mobile-pattern-recognition)
- [Implementation Examples](#implementation-examples)
- [Troubleshooting](#troubleshooting)

## Overview

### Purpose
Analyzes session duration patterns, quit behavior, and engagement consistency to detect when players are becoming frustrated or disengaged, providing proactive difficulty adjustments.

### Key Features
- **Session Duration Analysis**: Identifies unusually short sessions indicating frustration
- **Pattern Recognition**: Detects consistent patterns across multiple sessions
- **Rage Quit Integration**: Incorporates rage quit detection from RageQuitModifier
- **Mobile Optimized**: Distinguishes between interruptions and intentional short sessions
- **Predictive Adjustment**: Adjusts difficulty before complete disengagement

### Why It Matters for Mobile
Mobile games have unique session patterns:
- **Interruption-Heavy**: Calls, notifications, app switching
- **Context-Sensitive**: Commuting, waiting, short breaks
- **Variable Engagement**: Attention levels fluctuate more than desktop
- **Retention Critical**: Easier to abandon than desktop games

## Architecture

### Provider Dependencies
```csharp
IRageQuitProvider:
  - GetCurrentSessionDuration()     // Current session length
  - GetAverageSessionDuration()     // Historical session average
  - GetRecentRageQuitCount()        // Recent rage quit frequency
  - GetLastQuitType()               // How last session ended
```

### Stateless Design
```csharp
public override ModifierResult Calculate() // NO parameters - stateless!
{
    // Get all session data from provider
    var currentDuration = rageQuitProvider.GetCurrentSessionDuration();
    var avgDuration = rageQuitProvider.GetAverageSessionDuration();
    // ... analysis logic
}
```

## Analysis Components

### 1. Very Short Session Detection
**Purpose**: Identify sessions that are unusually short, potentially indicating immediate frustration.

**Algorithm**:
```csharp
if (currentSessionDuration < VeryShortSessionThreshold)
{
    adjustment = -VeryShortSessionDecrease;
    reason = "Very short session detected";
}
```

**Mobile Considerations**:
- Threshold should be 45-90 seconds to avoid false positives from interruptions
- Consider time-of-day patterns (rushed morning commutes vs relaxed evening play)
- Account for app launching time and loading screens

### 2. Consistent Short Session Pattern
**Purpose**: Detect when a player consistently has short sessions, indicating potential systematic frustration.

**Algorithm**:
```csharp
// Analyze recent session history
var recentSessions = GetRecentSessions(SessionHistorySize);
var shortSessions = CountSessionsShorterThan(MinNormalSessionDuration);
var shortRatio = shortSessions / totalSessions;

if (shortRatio > ShortSessionRatio)
{
    adjustment = -ConsistentShortSessionsDecrease;
    reason = "Consistent short session pattern";
}
```

**Pattern Recognition**:
- Looks at 3-7 recent sessions to establish patterns
- Distinguishes between occasional short sessions and consistent patterns
- Accounts for player's personal session length baseline

### 3. Rage Quit Pattern Analysis
**Purpose**: Integrate with rage quit detection to identify when rage quitting becomes a pattern rather than isolated incidents.

**Algorithm**:
```csharp
var rageQuitCount = rageQuitProvider.GetRecentRageQuitCount();

if (rageQuitCount >= RageQuitCountThreshold)
{
    var patternPenalty = RageQuitPatternDecrease * RageQuitPenaltyMultiplier;
    adjustment -= patternPenalty;
    reason = "Rage quit pattern detected";
}
```

**Enhanced Analysis**:
- Considers frequency of rage quits over time
- Applies heavier penalties for recurring rage quit behavior
- Integrates with individual rage quit events from RageQuitModifier

### 4. Mid-Level Quit Analysis
**Purpose**: Detect when players frequently quit in the middle of levels, indicating content difficulty issues.

**Algorithm**:
```csharp
var midLevelQuits = CountMidLevelQuits(recentSessions);
var midLevelRatio = midLevelQuits / totalSessions;

if (midLevelRatio > MidLevelQuitRatio)
{
    adjustment -= MidLevelQuitDecrease;
    reason = "Frequent mid-level quits";
}
```

**Behavioral Insights**:
- Mid-level quits often indicate content is too difficult
- Different from rage quits (which might be immediate)
- Suggests player engagement but inability to complete

## Configuration Deep Dive

### Session Duration Settings

#### MinNormalSessionDuration
**Purpose**: Defines what constitutes a "normal" session length for pattern analysis.
**Range**: 60-600 seconds (1-10 minutes)
**Mobile Recommendations**:
- **Casual Games**: 120-180 seconds (2-3 minutes)
- **Mid-core Games**: 180-300 seconds (3-5 minutes)
- **Hardcore Games**: 300-600 seconds (5-10 minutes)

```csharp
// Casual mobile puzzle (like Unscrew Factory)
MinNormalSessionDuration = 180f; // 3 minutes

// Competitive mobile game
MinNormalSessionDuration = 300f; // 5 minutes
```

#### VeryShortSessionThreshold
**Purpose**: Immediate frustration detection threshold.
**Range**: 30-120 seconds
**Mobile Considerations**:
- Must be above typical app launch + first level load time
- Account for tutorial skipping behavior
- Consider onboarding session lengths

```csharp
// Conservative (avoid false positives)
VeryShortSessionThreshold = 90f; // 1.5 minutes

// Aggressive (quick intervention)
VeryShortSessionThreshold = 45f; // 45 seconds
```

### Pattern Detection Settings

#### SessionHistorySize
**Purpose**: Number of recent sessions to analyze for patterns.
**Range**: 3-10 sessions
**Considerations**:
- **Smaller**: Faster adaptation, more noise sensitivity
- **Larger**: More stable patterns, slower to adapt

```csharp
// Quick adaptation for mobile retention
SessionHistorySize = 3;

// Stable pattern detection
SessionHistorySize = 7;
```

#### ShortSessionRatio
**Purpose**: Percentage of sessions that must be short to trigger pattern detection.
**Range**: 0.3-0.8 (30%-80%)
**Mobile Optimization**:

| Game Type | Ratio | Reasoning |
|-----------|-------|-----------|
| Casual | 0.6 | More tolerance for varied session lengths |
| Mid-core | 0.5 | Balanced detection |
| Competitive | 0.4 | Quick intervention for engaged players |

### Rage Quit Integration

#### RageQuitTimeThreshold
**Purpose**: Time threshold for detecting rage quits in session analysis.
**Range**: 10-60 seconds
**Mobile Specific**:
- Lower than desktop due to shorter attention spans
- Must distinguish from interruptions

#### RageQuitCountThreshold
**Purpose**: Number of rage quits needed to establish a pattern.
**Range**: 1-5 occurrences
**Recommendation**: 2-3 for mobile (quick intervention)

## Algorithm Details

### Complete Calculation Flow

```csharp
public override ModifierResult Calculate()
{
    var value = 0f;
    var reasons = new List<string>();

    // 1. IMMEDIATE SHORT SESSION DETECTION
    var currentDuration = rageQuitProvider.GetCurrentSessionDuration();
    if (currentDuration > 0 && currentDuration < config.VeryShortSessionThreshold)
    {
        value -= config.VeryShortSessionDecrease;
        reasons.Add($"Very short session ({currentDuration:F0}s)");
    }

    // 2. SESSION PATTERN ANALYSIS
    var avgDuration = rageQuitProvider.GetAverageSessionDuration();
    if (avgDuration > 0 && avgDuration < config.MinNormalSessionDuration)
    {
        // Check if this is a consistent pattern
        var historicalData = AnalyzeSessionHistory();
        var shortSessionCount = CountShortSessions(historicalData);
        var totalSessions = historicalData.Count;

        if (totalSessions >= 3) // Minimum sessions for pattern
        {
            var shortRatio = shortSessionCount / (float)totalSessions;
            if (shortRatio > config.ShortSessionRatio)
            {
                value -= config.ConsistentShortSessionsDecrease;
                reasons.Add($"Consistent short sessions ({shortRatio:P0})");
            }
        }
    }

    // 3. RAGE QUIT PATTERN DETECTION
    var rageQuitCount = rageQuitProvider.GetRecentRageQuitCount();
    if (rageQuitCount >= config.RageQuitCountThreshold)
    {
        var patternPenalty = config.RageQuitPatternDecrease * config.RageQuitPenaltyMultiplier;
        value -= patternPenalty;
        reasons.Add($"Rage quit pattern ({rageQuitCount} recent)");
    }

    // 4. MID-LEVEL QUIT ANALYSIS
    var lastQuitType = rageQuitProvider.GetLastQuitType();
    if (lastQuitType == QuitType.MidLevel)
    {
        var midLevelPattern = AnalyzeMidLevelQuitPattern();
        if (midLevelPattern.Ratio > config.MidLevelQuitRatio)
        {
            value -= config.MidLevelQuitDecrease;
            reasons.Add("Frequent mid-level quits");
        }
    }

    // 5. DIFFICULTY IMPROVEMENT ANALYSIS (EXPERIMENTAL)
    var improvementMeasure = AnalyzeDifficultyEffectiveness();
    if (improvementMeasure < config.DifficultyImprovementThreshold)
    {
        // Previous difficulty adjustments not effective enough
        value *= 1.2f; // Slightly amplify this adjustment
        reasons.Add("Amplified due to insufficient improvement");
    }

    return new ModifierResult
    {
        ModifierName = ModifierName,
        Value = value,
        Reason = reasons.Count > 0 ? string.Join(", ", reasons) : "Normal session pattern",
        Metadata = new Dictionary<string, object>
        {
            ["currentDuration"] = currentDuration,
            ["avgDuration"] = avgDuration,
            ["rageQuitCount"] = rageQuitCount,
            ["lastQuitType"] = lastQuitType.ToString(),
            ["applied"] = Math.Abs(value) > 0.01f
        }
    };
}
```

### Session History Analysis

```csharp
private SessionHistoryAnalysis AnalyzeSessionHistory()
{
    var sessions = GetRecentSessions(config.SessionHistorySize);
    var analysis = new SessionHistoryAnalysis();

    foreach (var session in sessions)
    {
        analysis.TotalSessions++;

        if (session.Duration < config.MinNormalSessionDuration)
        {
            analysis.ShortSessions++;
        }

        if (session.EndType == QuitType.RageQuit)
        {
            analysis.RageQuits++;
        }

        if (session.EndType == QuitType.MidLevel)
        {
            analysis.MidLevelQuits++;
        }
    }

    analysis.ShortSessionRatio = analysis.ShortSessions / (float)analysis.TotalSessions;
    analysis.RageQuitRatio = analysis.RageQuits / (float)analysis.TotalSessions;
    analysis.MidLevelQuitRatio = analysis.MidLevelQuits / (float)analysis.TotalSessions;

    return analysis;
}
```

## Mobile Pattern Recognition

### Distinguishing Mobile Patterns

#### Interruption vs Frustration
**Challenge**: Mobile sessions can be short due to external interruptions, not game difficulty.

**Solutions**:
1. **Time-of-Day Analysis**:
   - Morning commute sessions naturally shorter
   - Evening sessions typically longer
   - Weekend vs weekday patterns

2. **Session End Analysis**:
   - Clean app backgrounding ≠ rage quit
   - Immediate restart = likely interruption
   - No return for hours = potential frustration

3. **Level Progress Correlation**:
   - Short session + level progress = likely interruption
   - Short session + no progress = likely frustration

#### Platform-Specific Thresholds

| Platform | Very Short | Normal Min | Reasoning |
|----------|------------|------------|-----------|
| **Phone** | 45s | 120s | Smaller screen, more interruptions |
| **Tablet** | 60s | 180s | Better for longer sessions |
| **Desktop** | 90s | 300s | Dedicated play environment |

### Engagement Pattern Recognition

#### Positive Patterns (No Adjustment)
- Consistent 3-5 minute sessions with progress
- Gradual session length increases over time
- Mix of short and long sessions with overall progress

#### Warning Patterns (Light Adjustment)
- Decreasing average session length
- Increased frequency of mid-level quits
- Consistent 1-2 minute sessions without clear interruption cause

#### Critical Patterns (Strong Adjustment)
- Multiple consecutive sessions under 60 seconds
- High rage quit frequency (>30% of sessions)
- Complete disengagement pattern (no sessions for 24+ hours after short session spree)

## Implementation Examples

### Casual Mobile Puzzle Game (Unscrew Factory)
```csharp
var config = new SessionPatternConfig
{
    // Duration Settings: Forgiving for casual mobile play
    MinNormalSessionDuration = 150f,        // 2.5 minutes
    VeryShortSessionThreshold = 60f,        // 1 minute
    VeryShortSessionDecrease = 0.4f,

    // Pattern Detection: Quick adaptation
    SessionHistorySize = 4,                 // Last 4 sessions
    ShortSessionRatio = 0.6f,               // 60% must be short
    ConsistentShortSessionsDecrease = 0.6f,

    // Rage Quit: Sensitive to frustration
    RageQuitTimeThreshold = 25f,            // 25 seconds
    RageQuitCountThreshold = 2,             // 2 rage quits = pattern
    RageQuitPatternDecrease = 0.8f,
    RageQuitPenaltyMultiplier = 0.6f,

    // Mid-Level Quits: Common in puzzle games
    MidLevelQuitDecrease = 0.3f,
    MidLevelQuitRatio = 0.3f,               // 30% threshold

    // Improvement Analysis
    DifficultyImprovementThreshold = 1.3f   // Expect 30% improvement
};
```

### Competitive Mobile Game
```csharp
var config = new SessionPatternConfig
{
    // Duration Settings: Higher expectations for engaged players
    MinNormalSessionDuration = 240f,        // 4 minutes
    VeryShortSessionThreshold = 90f,        // 1.5 minutes
    VeryShortSessionDecrease = 0.3f,

    // Pattern Detection: Stable analysis
    SessionHistorySize = 5,                 // Last 5 sessions
    ShortSessionRatio = 0.4f,               // 40% threshold (more aggressive)
    ConsistentShortSessionsDecrease = 0.5f,

    // Rage Quit: Quick intervention for competitive players
    RageQuitTimeThreshold = 20f,            // 20 seconds
    RageQuitCountThreshold = 2,
    RageQuitPatternDecrease = 1.0f,         // Stronger response
    RageQuitPenaltyMultiplier = 0.8f,

    // Mid-Level Quits: Less tolerance in competitive
    MidLevelQuitDecrease = 0.4f,
    MidLevelQuitRatio = 0.25f,              // 25% threshold

    // Improvement Analysis
    DifficultyImprovementThreshold = 1.2f   // Expect 20% improvement
};
```

### Educational Mobile Game
```csharp
var config = new SessionPatternConfig
{
    // Duration Settings: Very forgiving for learning environment
    MinNormalSessionDuration = 180f,        // 3 minutes
    VeryShortSessionThreshold = 75f,        // 1.25 minutes
    VeryShortSessionDecrease = 0.5f,        // Stronger help

    // Pattern Detection: Conservative
    SessionHistorySize = 6,                 // Longer history for stability
    ShortSessionRatio = 0.7f,               // 70% tolerance
    ConsistentShortSessionsDecrease = 0.8f, // Strong intervention

    // Rage Quit: Immediate help for learners
    RageQuitTimeThreshold = 30f,
    RageQuitCountThreshold = 1,             // Single rage quit triggers pattern
    RageQuitPatternDecrease = 1.2f,         // Very strong response
    RageQuitPenaltyMultiplier = 1.0f,

    // Mid-Level Quits: Learning indicator
    MidLevelQuitDecrease = 0.5f,            // Significant help
    MidLevelQuitRatio = 0.2f,               // 20% threshold

    // Improvement Analysis
    DifficultyImprovementThreshold = 1.5f   // Expect 50% improvement
};
```

## Troubleshooting

### Common Issues

#### 1. Too Many False Positives from Interruptions
**Symptoms**: Frequent difficulty decreases for normal mobile interruptions
**Solutions**:
- Increase `VeryShortSessionThreshold` to 75-90 seconds
- Raise `ShortSessionRatio` to 0.6-0.7
- Implement time-of-day awareness in provider
- Check for immediate session restarts (interruption indicator)

#### 2. Not Detecting Real Frustration
**Symptoms**: Players rage quit but modifier doesn't respond
**Solutions**:
- Lower `RageQuitTimeThreshold` to 15-25 seconds
- Reduce `RageQuitCountThreshold` to 1-2
- Decrease `VeryShortSessionThreshold`
- Verify rage quit detection in provider is working

#### 3. Pattern Detection Too Slow
**Symptoms**: Takes too many sessions to detect patterns
**Solutions**:
- Reduce `SessionHistorySize` to 3-4
- Lower ratio thresholds for faster triggering
- Implement weighted recent session analysis
- Add immediate pattern detection for extreme cases

#### 4. Conflicting with Other Modifiers
**Symptoms**: Erratic behavior when combined with RageQuitModifier
**Solutions**:
- Ensure SessionPatternModifier has lower priority than RageQuitModifier
- Adjust penalty amounts to avoid double-penalization
- Review aggregation weights for session-related modifiers

### Debug Information

#### Key Metadata Fields
```csharp
Metadata = {
    ["currentDuration"] = currentDuration,          // Current session length (seconds)
    ["avgDuration"] = avgDuration,                  // Historical average duration
    ["rageQuitCount"] = rageQuitCount,              // Recent rage quit count
    ["lastQuitType"] = lastQuitType,                // How last session ended
    ["shortSessionRatio"] = shortSessionRatio,      // Percentage of recent short sessions
    ["midLevelQuitRatio"] = midLevelQuitRatio,      // Percentage of mid-level quits
    ["applied"] = Math.Abs(value) > 0.01f           // Whether adjustment was applied
}
```

#### Logging Examples
```csharp
// Immediate detection
"Very short session (45s) -> decrease 0.40"

// Pattern detection
"Consistent short sessions (70%) -> decrease 0.60"

// Rage quit pattern
"Rage quit pattern (3 recent) -> decrease 0.80"

// Mid-level quit analysis
"Frequent mid-level quits (40%) -> decrease 0.30"
```

### Mobile-Specific Validation

#### Session Quality Checks
- [ ] Average session duration matches expected mobile patterns (2-5 minutes)
- [ ] Very short threshold above app launch time + one level
- [ ] Pattern detection accounts for natural mobile interruptions
- [ ] Time-of-day patterns considered if available

#### Frustration Detection Accuracy
- [ ] Rage quit threshold appropriate for mobile attention spans
- [ ] Pattern thresholds avoid false positives from interruptions
- [ ] Mid-level quit analysis distinguishes difficulty from interruption
- [ ] Improvement analysis tracks effectiveness of adjustments

#### Performance Validation
- [ ] Session history storage efficient for mobile memory constraints
- [ ] Pattern analysis completes in <5ms
- [ ] Provider methods optimized for frequent calls
- [ ] Metadata collection doesn't impact game performance

---

*The SessionPatternModifier is crucial for mobile game retention, as it detects and responds to player disengagement patterns before they lead to complete abandonment. Proper configuration for mobile patterns is essential for accurate frustration detection.*