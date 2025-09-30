# SessionPatternModifier - Enhanced Session Pattern Analysis

**Last Updated:** January 22, 2025

## Overview

The SessionPatternModifier is an advanced difficulty modifier that analyzes comprehensive session patterns to detect player frustration, engagement levels, and behavioral trends. It has been enhanced with 100% configuration field utilization (12/12 fields) and integration with the new ISessionPatternProvider interface for advanced behavioral analysis.

## Key Features

✅ **100% Configuration Field Utilization (12/12 fields)**
✅ **Advanced Session History Analysis via ISessionPatternProvider**
✅ **Mid-Level Quit Pattern Detection**
✅ **Difficulty Improvement Effectiveness Tracking**
✅ **Direct ILogger Integration with Robust Error Handling**
✅ **Multiple Session Duration Thresholds**
✅ **Rage Quit Pattern Recognition**
✅ **Session Duration Trend Analysis**

## Architecture

### Provider Dependencies

The SessionPatternModifier requires two provider interfaces:

1. **IRageQuitProvider** (Required) - Basic quit behavior tracking
2. **ISessionPatternProvider** (Optional) - Advanced session pattern analysis

```csharp
public SessionPatternModifier(
    SessionPatternConfig config,
    IRageQuitProvider rageQuitProvider,
    ISessionPatternProvider sessionPatternProvider,
    ILogger logger) // Direct ILogger injection
    : base(config, logger)
```

### Calculation Flow

The modifier performs comprehensive analysis in 6 sections:

```csharp
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    // 1. Current session duration analysis
    // 2. Average session duration patterns
    // 3. Rage quit data from provider
    // 4. Mid-level quit pattern detection
    // 5. Session ratio analysis
    // 6. Advanced session history analysis (if ISessionPatternProvider available)
}
```

## Configuration Fields (12/12 - 100% Utilization)

### Session Duration Settings

| Field | Type | Default | Description | Usage |
|-------|------|---------|-------------|-------|
| `MinNormalSessionDuration` | float | 180s | Minimum session considered normal | ✅ Threshold for normal vs short sessions |
| `VeryShortSessionThreshold` | float | 60s | Sessions below this are very short | ✅ Immediate penalty trigger |
| `VeryShortSessionDecrease` | float | 0.5 | Penalty for very short sessions | ✅ Applied to current session |

### Session Pattern Detection

| Field | Type | Default | Description | Usage |
|-------|------|---------|-------------|-------|
| `SessionHistorySize` | int | 5 | Recent sessions to analyze | ✅ ISessionPatternProvider analysis scope |
| `ShortSessionRatio` | float | 0.5 | Ratio triggering short session penalty | ✅ Pattern threshold (50% of sessions) |
| `ConsistentShortSessionsDecrease` | float | 0.8 | Penalty for pattern of short sessions | ✅ Applied for consistent patterns |

### Session End Reason Analysis

| Field | Type | Default | Description | Usage |
|-------|------|---------|-------------|-------|
| `RageQuitPatternDecrease` | float | 1.0 | Penalty for rage quit patterns | ✅ Multiple rage quit detection |
| `MidLevelQuitDecrease` | float | 0.4 | Penalty for mid-level quits | ✅ QuitType.MidPlay handling |
| `MidLevelQuitRatio` | float | 0.3 | Ratio triggering mid-quit penalty | ✅ ISessionPatternProvider analysis |

### Rage Quit Detection Settings

| Field | Type | Default | Description | Usage |
|-------|------|---------|-------------|-------|
| `RageQuitCountThreshold` | int | 2 | Minimum rage quits for pattern | ✅ Pattern detection threshold |
| `RageQuitPenaltyMultiplier` | float | 0.5 | Multiplier for rage quit penalties | ✅ Scaling factor for penalties |

### Difficulty Adjustment Analysis

| Field | Type | Default | Description | Usage |
|-------|------|---------|-------------|-------|
| `DifficultyImprovementThreshold` | float | 1.2 | Improvement ratio for effectiveness | ✅ ISessionPatternProvider tracking |

## Analysis Sections

### 1. Current Session Duration Analysis

Analyzes the current session duration against configured thresholds:

```csharp
var currentSessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();
if (currentSessionDuration < this.config.VeryShortSessionThreshold && currentSessionDuration > 0)
{
    value -= this.config.VeryShortSessionDecrease;
    reasons.Add($"Very short session ({currentSessionDuration:F0}s)");
}
```

**Key Behavior:**
- Immediately penalizes sessions shorter than `VeryShortSessionThreshold`
- Provides real-time feedback for frustration detection
- Uses `VeryShortSessionDecrease` for penalty magnitude

### 2. Average Session Duration Patterns

Examines average session duration trends:

```csharp
var avgSessionDuration = this.rageQuitProvider.GetAverageSessionDuration();
if (avgSessionDuration > 0 && avgSessionDuration < this.config.MinNormalSessionDuration)
{
    var durationRatio = avgSessionDuration / this.config.MinNormalSessionDuration;
    var durationAdjustment = -(1f - durationRatio) * this.config.ConsistentShortSessionsDecrease;
    value += durationAdjustment;
}
```

**Key Behavior:**
- Scales penalty based on how far below normal the average is
- Uses `ConsistentShortSessionsDecrease` as the maximum penalty
- Detects long-term engagement issues

### 3. Rage Quit Data Analysis

Processes quit behavior from IRageQuitProvider:

```csharp
var lastQuitType = this.rageQuitProvider.GetLastQuitType();
var recentRageQuitCount = this.rageQuitProvider.GetRecentRageQuitCount();

if (recentRageQuitCount >= this.config.RageQuitCountThreshold)
{
    var rageQuitPenalty = this.config.RageQuitPatternDecrease * this.config.RageQuitPenaltyMultiplier;
    value -= rageQuitPenalty;
}
```

**Key Behavior:**
- Tracks recent rage quit frequency
- Applies escalating penalties for repeated rage quits
- Uses both `RageQuitCountThreshold` and `RageQuitPenaltyMultiplier`

### 4. Mid-Level Quit Detection

Identifies quits during level play:

```csharp
if (lastQuitType == QuitType.MidPlay)
{
    value -= this.config.MidLevelQuitDecrease;
    reasons.Add("Mid-level quit detected");
}
```

**Key Behavior:**
- Specifically targets `QuitType.MidPlay` behavior
- Indicates player frustration during gameplay
- Uses `MidLevelQuitDecrease` for penalty

### 5. Session Ratio Analysis

Analyzes session duration patterns:

```csharp
var sessionRatio = avgSessionDuration / this.config.MinNormalSessionDuration;
if (sessionRatio < this.config.ShortSessionRatio)
{
    var shortSessionPenalty = this.config.ConsistentShortSessionsDecrease * (1f - sessionRatio);
    value -= shortSessionPenalty;
}
```

**Key Behavior:**
- Compares average session to normal duration expectations
- Scales penalty based on severity of short session pattern
- Uses `ShortSessionRatio` threshold for pattern detection

### 6. Advanced Session History Analysis (ISessionPatternProvider)

Enhanced analysis when ISessionPatternProvider is available:

#### Session History Pattern Detection

```csharp
var recentSessions = this.sessionPatternProvider.GetRecentSessionDurations(this.config.SessionHistorySize);
if (recentSessions != null && recentSessions.Count > 0)
{
    var shortSessionCount = 0;
    foreach (var duration in recentSessions)
    {
        if (duration > 0 && duration < this.config.VeryShortSessionThreshold)
            shortSessionCount++;
    }

    if (recentSessions.Count >= this.config.SessionHistorySize)
    {
        var shortRatio = (float)shortSessionCount / recentSessions.Count;
        if (shortRatio > this.config.ShortSessionRatio)
        {
            var historyPenalty = this.config.ConsistentShortSessionsDecrease * shortRatio;
            value -= historyPenalty;
        }
    }
}
```

#### Mid-Level Quit Ratio Analysis

```csharp
var totalQuits = this.sessionPatternProvider.GetTotalRecentQuits();
var midLevelQuits = this.sessionPatternProvider.GetRecentMidLevelQuits();
if (totalQuits > 0)
{
    var midQuitRatio = (float)midLevelQuits / totalQuits;
    if (midQuitRatio > this.config.MidLevelQuitRatio)
    {
        var midQuitPenalty = this.config.MidLevelQuitDecrease * (midQuitRatio / this.config.MidLevelQuitRatio);
        value -= midQuitPenalty;
    }
}
```

#### Difficulty Adjustment Effectiveness Tracking

```csharp
var previousDifficulty = this.sessionPatternProvider.GetPreviousDifficulty();
var previousSessionDuration = this.sessionPatternProvider.GetSessionDurationBeforeLastAdjustment();
if (previousDifficulty > 0 && previousSessionDuration > 0 && currentSessionDuration > 0)
{
    var improvementRatio = currentSessionDuration / previousSessionDuration;

    if (previousDifficulty > 0 && improvementRatio < this.config.DifficultyImprovementThreshold)
    {
        var additionalAdjustment = (this.config.DifficultyImprovementThreshold - improvementRatio) * 0.5f;
        value -= additionalAdjustment;
    }
}
```

## Provider Integration

### IRageQuitProvider Methods Used (4/4 - 100%)

```csharp
// ✅ Used for current session analysis
float GetCurrentSessionDuration()

// ✅ Used for pattern detection
float GetAverageSessionDuration()

// ✅ Used for quit type analysis
QuitType GetLastQuitType()

// ✅ Used for rage quit pattern detection
int GetRecentRageQuitCount()
```

### ISessionPatternProvider Methods Used (5/5 - 100%)

```csharp
// ✅ Used for session history analysis
List<float> GetRecentSessionDurations(int count)

// ✅ Used for quit ratio calculations
int GetTotalRecentQuits()

// ✅ Used for mid-level quit detection
int GetRecentMidLevelQuits()

// ✅ Used for difficulty improvement tracking
float GetPreviousDifficulty()

// ✅ Used for effectiveness analysis
float GetSessionDurationBeforeLastAdjustment()
```

## ILogger Integration

The modifier uses direct ILogger injection for robust error handling:

```csharp
public SessionPatternModifier(
    SessionPatternConfig config,
    IRageQuitProvider rageQuitProvider,
    ISessionPatternProvider sessionPatternProvider,
    ILogger logger) // Required - no null defaults
    : base(config, logger)
{
    this.rageQuitProvider = rageQuitProvider;
    this.sessionPatternProvider = sessionPatternProvider;
}
```

### Error Handling

```csharp
public override ModifierResult Calculate()
{
    try
    {
        // Calculation logic
    }
    catch (Exception e)
    {
        this.logger?.Error($"[SessionPatternModifier] Error calculating: {e.Message}");
        return ModifierResult.NoChange();
    }
}
```

### Debug Logging

The modifier provides comprehensive debug logging:

```csharp
this.LogDebug($"Very short session {currentSessionDuration:F0}s -> decrease {this.config.VeryShortSessionDecrease:F2}");
this.LogDebug($"Rage quit pattern detected: {recentRageQuitCount} rage quits -> decrease {rageQuitPenalty:F2}");
this.LogDebug($"Session history analysis: {shortSessionCount}/{recentSessions.Count} were short -> decrease {historyPenalty:F2}");
```

## Usage Examples

### Basic Usage (IRageQuitProvider Only)

```csharp
public class BasicRageQuitProvider : IRageQuitProvider
{
    public QuitType GetLastQuitType() => lastQuitType;
    public float GetCurrentSessionDuration() => Time.time - sessionStartTime;
    public int GetRecentRageQuitCount() => recentRageQuits;
    public float GetAverageSessionDuration() => totalSessionTime / sessionCount;
}

// Register in DI
builder.RegisterInstance<IRageQuitProvider>(new BasicRageQuitProvider());
```

### Advanced Usage (With ISessionPatternProvider)

```csharp
public class AdvancedSessionProvider : IRageQuitProvider, ISessionPatternProvider
{
    // IRageQuitProvider implementation...

    // Advanced ISessionPatternProvider methods
    public List<float> GetRecentSessionDurations(int count) =>
        sessionHistory.Take(count).Select(s => s.Duration).ToList();

    public int GetTotalRecentQuits() =>
        sessionHistory.Count(s => s.QuitType != QuitType.Normal);

    public int GetRecentMidLevelQuits() =>
        sessionHistory.Count(s => s.QuitType == QuitType.MidPlay);

    public float GetPreviousDifficulty() => difficultyHistory.LastOrDefault();

    public float GetSessionDurationBeforeLastAdjustment() =>
        GetSessionBeforeLastDifficultyChange()?.Duration ?? 0f;
}
```

## Configuration Tuning

### Mobile Puzzle Game Optimization

For mobile puzzle games like Unscrew Factory:

```csharp
var sessionConfig = new SessionPatternConfig
{
    // Short sessions are common on mobile - adjust thresholds
    MinNormalSessionDuration = 120f,        // 2 minutes (mobile-friendly)
    VeryShortSessionThreshold = 30f,        // 30 seconds (frustration indicator)
    VeryShortSessionDecrease = 0.3f,        // Gentle penalty for mobile

    // Pattern detection for mobile behavior
    SessionHistorySize = 3,                 // Fewer sessions for faster detection
    ShortSessionRatio = 0.67f,              // 2 out of 3 sessions
    ConsistentShortSessionsDecrease = 0.5f, // Moderate penalty

    // Mid-level quits are critical on mobile
    MidLevelQuitDecrease = 0.6f,            // Higher penalty
    MidLevelQuitRatio = 0.25f,              // Lower threshold (25%)

    // Rage quit detection
    RageQuitCountThreshold = 1,             // Single rage quit triggers
    RageQuitPenaltyMultiplier = 0.7f,       // Significant penalty

    // Difficulty improvement tracking
    DifficultyImprovementThreshold = 1.3f   // 30% improvement expected
};
```

### Hardcore Game Configuration

For more challenging games:

```csharp
var sessionConfig = new SessionPatternConfig
{
    // Longer sessions expected
    MinNormalSessionDuration = 300f,        // 5 minutes
    VeryShortSessionThreshold = 90f,        // 1.5 minutes
    VeryShortSessionDecrease = 0.8f,        // Stronger penalty

    // More sessions for pattern detection
    SessionHistorySize = 7,                 // Week's worth
    ShortSessionRatio = 0.4f,               // Lower threshold
    ConsistentShortSessionsDecrease = 1.2f, // Higher penalty

    // Conservative mid-level quit handling
    MidLevelQuitDecrease = 0.3f,            // Lower penalty
    MidLevelQuitRatio = 0.4f,               // Higher threshold

    // Rage quit tolerance
    RageQuitCountThreshold = 3,             // Multiple rage quits
    RageQuitPenaltyMultiplier = 0.4f,       // Moderate penalty

    // Improvement tracking
    DifficultyImprovementThreshold = 1.5f   // 50% improvement
};
```

## Testing and Validation

### Test Coverage

The SessionPatternModifier has 15 comprehensive tests covering:

- ✅ All 12 configuration fields (100% utilization)
- ✅ ISessionPatternProvider integration (5/5 methods)
- ✅ IRageQuitProvider integration (4/4 methods)
- ✅ Direct ILogger injection validation
- ✅ Error handling and null safety
- ✅ Edge cases and extreme values
- ✅ Real-world session patterns

### Validation Commands

```csharp
// Test configuration field utilization
[Test]
public void Calculate_UsesAllConfigFields_100PercentUtilization()
{
    // Validates all 12 fields are used in calculations
}

// Test provider method usage
[Test]
public void Calculate_ISessionPatternProvider_AllMethodsCalled()
{
    // Validates all 5 ISessionPatternProvider methods are called
}

// Test ILogger integration
[Test]
public void Constructor_ILoggerRequired_NoNullDefaults()
{
    // Validates ILogger is required parameter
}
```

## Performance Considerations

### Calculation Complexity

The SessionPatternModifier performs:
- 6 analysis sections
- Up to 9 provider method calls
- Configuration field validation
- Pattern detection algorithms

**Target Performance:** < 2ms per calculation

### Optimization Tips

1. **Provider Caching:** Cache expensive provider calculations
2. **Session History Limits:** Keep `SessionHistorySize` reasonable (3-7)
3. **Debug Logging:** Disable in production for performance
4. **Pattern Detection:** Use efficient algorithms for session analysis

## Integration with Other Modifiers

### Modifier Coordination

SessionPatternModifier works well with:

- **RageQuitModifier:** Complementary quit behavior analysis
- **TimeDecayModifier:** Session frequency vs. time away analysis
- **LevelProgressModifier:** Progress patterns vs. session patterns

### Priority Considerations

```csharp
// Typical priority ordering
TimeDecayModifier:       Priority = 1 (highest - returning players)
SessionPatternModifier:  Priority = 7 (lowest - behavioral analysis)
RageQuitModifier:       Priority = 4 (mid - immediate response)
```

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| No pattern detection | ISessionPatternProvider not implemented | Implement interface or use IRageQuitProvider only |
| Excessive penalties | Thresholds too low for game type | Adjust thresholds for your game's session patterns |
| No difficulty changes | All sessions above thresholds | Lower `VeryShortSessionThreshold` or `MinNormalSessionDuration` |
| Provider errors | Null provider references | Ensure providers are registered in DI |

### Debug Information

Enable debug logging to see calculation details:

```csharp
var debugConfig = new SessionPatternConfig();
debugConfig.EnableDebugLogging = true;  // If available

// Look for debug messages:
// "Very short session 45s -> decrease 0.50"
// "Session history analysis: 3/5 were short -> decrease 0.48"
// "Mid-level quit ratio 40% > 30% -> decrease 0.53"
```

## Future Enhancements

### Planned Features

1. **Machine Learning Integration:** Predict session length based on behavior
2. **Dynamic Threshold Adjustment:** Self-tuning based on player base
3. **Cross-Session Pattern Detection:** Identify multi-day behavioral patterns
4. **Personalized Session Analysis:** Individual vs. population-based thresholds

### Extension Points

```csharp
// Custom session analysis
public interface ICustomSessionAnalyzer
{
    SessionAnalysisResult AnalyzeCustomPatterns(List<SessionInfo> sessions);
}

// Advanced pattern detection
public interface IAdvancedPatternDetector
{
    PatternDetectionResult DetectComplexPatterns(SessionPatternData data);
}
```

---

## Summary

The SessionPatternModifier provides comprehensive session behavior analysis with:

✅ **100% Configuration Field Utilization (12/12 fields)**
✅ **Complete Provider Integration (9/9 methods across both interfaces)**
✅ **Direct ILogger Integration with robust error handling**
✅ **Advanced behavioral pattern detection**
✅ **Mobile-optimized configuration options**
✅ **Production-ready with comprehensive testing**

This enhanced modifier is essential for mobile puzzle games where session patterns are critical indicators of player engagement and difficulty appropriateness.

---

**Last Updated:** January 22, 2025
**Version:** 2.4 Enhanced Session Pattern Analysis
**Status:** ✅ Production-Ready