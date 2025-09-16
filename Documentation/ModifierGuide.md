# Dynamic User Difficulty - Modifier Development Guide

## Table of Contents
1. [Overview](#overview)
2. [Creating Custom Modifiers](#creating-custom-modifiers)
3. [Modifier Lifecycle](#modifier-lifecycle)
4. [Configuration System](#configuration-system)
5. [Best Practices](#best-practices)
6. [Example Modifiers](#example-modifiers)
7. [Testing Modifiers](#testing-modifiers)

## Overview

Modifiers are the core extensibility mechanism of the Dynamic User Difficulty system. Each modifier calculates a difficulty adjustment based on specific player behavior or game state.

### Key Concepts
- **Modular**: Each modifier is independent
- **Configurable**: Parameters via ScriptableObjects
- **Prioritized**: Execution order controlled by priority
- **Toggleable**: Can be enabled/disabled at runtime

## Creating Custom Modifiers

### Step 1: Create the Modifier Class

```csharp
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers;

namespace YourNamespace
{
    public class YourCustomModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "YourModifierName";

        public YourCustomModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            // Your calculation logic here
            float adjustment = 0f;
            string reason = "No change";

            // Example: Check some condition
            if (SomeCondition(sessionData))
            {
                adjustment = GetParameter("AdjustmentAmount", 0.5f);
                reason = "Condition met";
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = adjustment,
                Reason = reason,
                Metadata = new Dictionary<string, object>
                {
                    ["your_data"] = "value"
                }
            };
        }

        private bool SomeCondition(PlayerSessionData data)
        {
            // Your condition logic
            return data.WinStreak > 0;
        }
    }
}
```

### Step 2: Register in DI Module

```csharp
// In DynamicDifficultyModule.cs
private void RegisterModifiers(IContainerBuilder builder)
{
    // Existing modifiers...

    // Add your custom modifier
    builder.Register<YourCustomModifier>(Lifetime.Singleton)
           .As<IDifficultyModifier>();
}
```

### Step 3: Configure in ScriptableObject

1. Create DifficultyConfig asset
2. Add ModifierConfig entry:
```
ModifierType: "YourModifierName"
Enabled: true
Priority: 10
Parameters:
  - Key: "AdjustmentAmount"
    Value: 0.5
```

## Modifier Lifecycle

### 1. Initialization
```csharp
public YourModifier(ModifierConfig config) : base(config)
{
    // One-time setup
    // Config is injected and stored
}
```

### 2. Calculation Phase
```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Called each time difficulty is calculated
    // Should be stateless and pure
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

### Parameter Access

```csharp
// Get parameter with default
float threshold = GetParameter("Threshold", 3.0f);

// Get multiple parameters
var config = new
{
    MinValue = GetParameter("MinValue", 1.0f),
    MaxValue = GetParameter("MaxValue", 10.0f),
    StepSize = GetParameter("StepSize", 0.5f)
};
```

### Response Curves

```csharp
// Apply animation curve to value
float rawValue = CalculateRawValue();
float curvedValue = ApplyCurve(rawValue);

// Curve maps 0-1 input to 0-1 output
// Configure curve shape in Unity Inspector
```

### Dynamic Configuration

```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Check if should run
    if (!ShouldRun(sessionData))
        return ModifierResult.NoChange();

    // Get dynamic threshold based on player level
    float threshold = GetDynamicThreshold(sessionData);

    // Calculate with dynamic values
    return CalculateWithThreshold(sessionData, threshold);
}

private float GetDynamicThreshold(PlayerSessionData data)
{
    // Example: Scale threshold with player progression
    var baseThreshold = GetParameter("BaseThreshold", 3.0f);
    var scaleFactor = GetParameter("ScaleFactor", 0.1f);

    // Assuming we track player level somehow
    int playerLevel = GetPlayerLevel(data);

    return baseThreshold + (playerLevel * scaleFactor);
}
```

## Best Practices

### 1. Keep Calculations Pure

✅ **Good: Pure function**
```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Only read from sessionData
    // No side effects
    // Deterministic result
    return new ModifierResult { Value = sessionData.WinStreak * 0.5f };
}
```

❌ **Bad: Side effects**
```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Don't do this!
    SaveToDatabase(sessionData);  // Side effect
    Random.Range(0, 1);           // Non-deterministic
    sessionData.WinStreak++;      // Modifying input
}
```

### 2. Use Meaningful Names

```csharp
// Good: Descriptive names
public override string ModifierName => "ConsecutiveWinBonus";

// Bad: Generic names
public override string ModifierName => "Modifier1";
```

### 3. Provide Clear Reasons

```csharp
// Good: Specific reason
reason = $"Win streak bonus ({sessionData.WinStreak} consecutive wins)";

// Bad: Generic reason
reason = "Difficulty changed";
```

### 4. Handle Edge Cases

```csharp
public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    // Check for null
    if (sessionData?.LastSession == null)
        return ModifierResult.NoChange();

    // Validate data
    if (sessionData.WinStreak < 0)
    {
        Debug.LogWarning($"Invalid win streak: {sessionData.WinStreak}");
        return ModifierResult.NoChange();
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
        ["raw_value"] = rawValue,
        ["threshold"] = threshold,
        ["curve_applied"] = curveApplied,
        ["calculation_time"] = calculationTime
    }
};
```

## Example Modifiers

### 1. Engagement Score Modifier

```csharp
public class EngagementModifier : BaseDifficultyModifier
{
    public override string ModifierName => "Engagement";

    public EngagementModifier(ModifierConfig config) : base(config) { }

    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        if (sessionData.RecentSessions.Count < 3)
            return ModifierResult.NoChange();

        var engagement = CalculateEngagementScore(sessionData);
        var adjustment = MapEngagementToAdjustment(engagement);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = GetEngagementReason(engagement),
            Metadata = new Dictionary<string, object>
            {
                ["engagement_score"] = engagement,
                ["sessions_analyzed"] = sessionData.RecentSessions.Count
            }
        };
    }

    private float CalculateEngagementScore(PlayerSessionData data)
    {
        var recentSessions = data.RecentSessions.ToList();

        // Calculate average session duration
        var avgDuration = recentSessions.Average(s => s.PlayDuration);

        // Calculate session frequency
        var timeSpans = new List<TimeSpan>();
        for (int i = 1; i < recentSessions.Count; i++)
        {
            timeSpans.Add(recentSessions[i].StartTime - recentSessions[i-1].EndTime);
        }
        var avgTimeBetween = timeSpans.Any()
            ? timeSpans.Average(t => t.TotalHours)
            : 24;

        // Score based on duration and frequency
        float durationScore = Mathf.Clamp01(avgDuration / 300f); // 5 min = 1.0
        float frequencyScore = Mathf.Clamp01(24f / avgTimeBetween); // Daily = 1.0

        return (durationScore + frequencyScore) / 2f;
    }

    private float MapEngagementToAdjustment(float engagement)
    {
        var minAdjustment = GetParameter("MinAdjustment", -0.5f);
        var maxAdjustment = GetParameter("MaxAdjustment", 0.5f);

        // Low engagement = negative adjustment
        // High engagement = positive adjustment
        return Mathf.Lerp(minAdjustment, maxAdjustment, engagement);
    }

    private string GetEngagementReason(float engagement)
    {
        if (engagement < 0.3f) return "Low engagement detected";
        if (engagement < 0.7f) return "Normal engagement";
        return "High engagement detected";
    }
}
```

### 2. Performance Trend Modifier

```csharp
public class PerformanceTrendModifier : BaseDifficultyModifier
{
    public override string ModifierName => "PerformanceTrend";

    public PerformanceTrendModifier(ModifierConfig config) : base(config) { }

    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        if (sessionData.RecentSessions.Count < 5)
            return ModifierResult.NoChange();

        var trend = CalculatePerformanceTrend(sessionData);
        var adjustment = GetTrendAdjustment(trend);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = GetTrendDescription(trend),
            Metadata = new Dictionary<string, object>
            {
                ["trend_value"] = trend,
                ["trend_direction"] = trend > 0 ? "improving" : "declining"
            }
        };
    }

    private float CalculatePerformanceTrend(PlayerSessionData data)
    {
        var sessions = data.RecentSessions.ToList();
        var winRates = new List<float>();

        // Calculate win rate over time windows
        for (int i = 0; i <= sessions.Count - 3; i++)
        {
            var window = sessions.Skip(i).Take(3);
            var winRate = window.Count(s => s.Won) / 3f;
            winRates.Add(winRate);
        }

        // Calculate trend (positive = improving, negative = declining)
        if (winRates.Count < 2) return 0;

        float trend = 0;
        for (int i = 1; i < winRates.Count; i++)
        {
            trend += winRates[i] - winRates[i - 1];
        }

        return trend / (winRates.Count - 1);
    }

    private float GetTrendAdjustment(float trend)
    {
        var sensitivity = GetParameter("TrendSensitivity", 1.0f);
        var maxAdjustment = GetParameter("MaxTrendAdjustment", 0.5f);

        // Improving = increase difficulty
        // Declining = decrease difficulty
        return Mathf.Clamp(trend * sensitivity, -maxAdjustment, maxAdjustment);
    }

    private string GetTrendDescription(float trend)
    {
        if (trend > 0.2f) return "Performance improving rapidly";
        if (trend > 0.05f) return "Performance improving";
        if (trend < -0.2f) return "Performance declining rapidly";
        if (trend < -0.05f) return "Performance declining";
        return "Performance stable";
    }
}
```

### 3. Skill Ceiling Modifier

```csharp
public class SkillCeilingModifier : BaseDifficultyModifier
{
    public override string ModifierName => "SkillCeiling";

    public SkillCeilingModifier(ModifierConfig config) : base(config) { }

    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        // Detect if player is at skill ceiling
        if (!IsAtSkillCeiling(sessionData))
            return ModifierResult.NoChange();

        // Apply special adjustment for skilled players
        var adjustment = GetParameter("CeilingBonus", 0.3f);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = adjustment,
            Reason = "Skill ceiling detected - adding challenge",
            Metadata = new Dictionary<string, object>
            {
                ["current_difficulty"] = sessionData.CurrentDifficulty,
                ["win_rate"] = CalculateWinRate(sessionData)
            }
        };
    }

    private bool IsAtSkillCeiling(PlayerSessionData data)
    {
        // Check multiple conditions
        var highDifficulty = data.CurrentDifficulty >= 8.0f;
        var highWinRate = CalculateWinRate(data) > 0.8f;
        var consistentPerformance = IsPerformanceConsistent(data);

        return highDifficulty && highWinRate && consistentPerformance;
    }

    private float CalculateWinRate(PlayerSessionData data)
    {
        if (data.RecentSessions.Count == 0) return 0;
        return data.RecentSessions.Count(s => s.Won) / (float)data.RecentSessions.Count;
    }

    private bool IsPerformanceConsistent(PlayerSessionData data)
    {
        if (data.RecentSessions.Count < 5) return false;

        var times = data.RecentSessions
            .Where(s => s.Won)
            .Select(s => s.PlayDuration)
            .ToList();

        if (times.Count < 3) return false;

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));

        // Low standard deviation = consistent
        return stdDev < avg * 0.2f;
    }
}
```

## Testing Modifiers

### Unit Test Template

```csharp
using NUnit.Framework;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;

[TestFixture]
public class YourModifierTests
{
    private YourCustomModifier modifier;
    private ModifierConfig config;

    [SetUp]
    public void Setup()
    {
        config = CreateTestConfig();
        modifier = new YourCustomModifier(config);
    }

    [Test]
    public void Calculate_WithConditionMet_ReturnsPositiveAdjustment()
    {
        // Arrange
        var sessionData = new PlayerSessionData
        {
            WinStreak = 5,
            CurrentDifficulty = 5.0f
        };

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.Greater(result.Value, 0);
        Assert.IsNotNull(result.Reason);
        Assert.AreEqual("YourModifierName", result.ModifierName);
    }

    [Test]
    public void Calculate_WithNoData_ReturnsNoChange()
    {
        // Arrange
        var sessionData = new PlayerSessionData();

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.AreEqual(0, result.Value);
    }

    [TestCase(0, 0f)]
    [TestCase(3, 1.5f)]
    [TestCase(5, 2.5f)]
    public void Calculate_WithVariousStreaks_ReturnsExpectedValue(
        int streak, float expected)
    {
        // Arrange
        var sessionData = new PlayerSessionData { WinStreak = streak };

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.AreEqual(expected, result.Value, 0.01f);
    }

    private ModifierConfig CreateTestConfig()
    {
        return new ModifierConfig
        {
            ModifierType = "YourModifierName",
            Enabled = true,
            Priority = 10,
            Parameters = new List<ModifierParameter>
            {
                new ModifierParameter { Key = "AdjustmentAmount", Value = 0.5f }
            }
        };
    }
}
```

### Integration Test

```csharp
[Test]
public void Modifier_IntegratesWithService_Correctly()
{
    // Arrange
    var service = CreateServiceWithModifier();
    var sessionData = CreateTestSessionData();

    // Act
    var result = service.CalculateDifficulty();

    // Assert
    var yourModifierResult = result.AppliedModifiers
        .FirstOrDefault(m => m.ModifierName == "YourModifierName");

    Assert.IsNotNull(yourModifierResult);
    Assert.Greater(yourModifierResult.Value, 0);
}
```

### Manual Testing Checklist

- [ ] Modifier registers correctly in DI
- [ ] Parameters load from config
- [ ] Calculation returns expected values
- [ ] Edge cases handled gracefully
- [ ] Performance impact minimal (< 1ms)
- [ ] Debug logs work correctly
- [ ] Metadata populated for debugging
- [ ] OnApplied hook fires correctly

## Troubleshooting

### Common Issues

1. **Modifier not running**
   - Check if registered in DI
   - Verify IsEnabled = true
   - Check priority order

2. **Wrong values**
   - Verify parameter names match
   - Check config values
   - Test response curve

3. **Performance issues**
   - Cache expensive calculations
   - Avoid LINQ in hot paths
   - Profile with Unity Profiler

4. **Null reference exceptions**
   - Check sessionData for null
   - Verify config injected
   - Handle empty collections

---

*Last Updated: 2025-09-16*