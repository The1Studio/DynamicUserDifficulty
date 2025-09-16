# Dynamic User Difficulty Service - Design Document

## Executive Summary

The Dynamic User Difficulty (DUD) service adaptively adjusts game difficulty based on three key factors:
1. **Win Streak** - Consecutive wins/losses pattern
2. **Time Since Last Play** - Duration between gaming sessions
3. **Last Session Outcome** - Whether the player won or lost when they quit

This creates a personalized experience that keeps players engaged by preventing frustration (too hard) and boredom (too easy).

## Core Concepts

### 1. Player Performance Metrics

#### Win Streak Tracking
- **Positive streak**: Consecutive wins increase difficulty
- **Negative streak**: Consecutive losses decrease difficulty
- **Streak reset**: Occurs after opposite outcome or time decay

#### Time-Based Adjustments
- **Short break** (< 6 hours): Maintain current difficulty
- **Medium break** (6-24 hours): Slight difficulty reduction
- **Long break** (1-7 days): Moderate difficulty reduction
- **Extended break** (> 7 days): Significant difficulty reduction

#### Last Session Impact
- **Rage quit detection**: Lost level + immediate quit = stronger difficulty reduction
- **Success quit**: Won level + quit = maintain or slight increase
- **Mid-level quit**: Incomplete level = consider as loss with time factor

### 2. Difficulty Calculation Formula

```
FinalDifficulty = BaseDifficulty
                + WinStreakModifier
                + TimeDecayModifier
                + LastSessionModifier
                + EngagementModifier
```

## Data Models

### PlayerSessionData
```csharp
public class PlayerSessionData
{
    // Core metrics
    public int CurrentWinStreak { get; set; }
    public int CurrentLossStreak { get; set; }
    public int HighestWinStreak { get; set; }
    public DateTime LastPlayTime { get; set; }
    public DateTime LastSessionEndTime { get; set; }

    // Last session info
    public SessionEndType LastSessionEndType { get; set; } // Won, Lost, Quit
    public int LastSessionLevelId { get; set; }
    public float LastSessionDuration { get; set; } // In seconds
    public float LastLevelCompletionPercentage { get; set; }

    // Historical data
    public List<SessionHistory> RecentSessions { get; set; } // Last 10 sessions
    public float AverageSessionDuration { get; set; }
    public float AverageTimeBetweenSessions { get; set; }
}

public enum SessionEndType
{
    CompletedWin,      // Finished level successfully
    CompletedLoss,     // Failed level completely
    QuitDuringPlay,    // Left mid-level
    QuitAfterWin,      // Left after winning
    QuitAfterLoss,     // Left after losing (potential rage quit)
    Timeout            // Session timed out
}

public class SessionHistory
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SessionEndType EndType { get; set; }
    public int LevelId { get; set; }
    public float DifficultyLevel { get; set; }
    public int MovesUsed { get; set; }
    public int OptimalMoves { get; set; }
}
```

### DifficultyConfiguration
```csharp
public class DifficultyConfiguration
{
    // Difficulty levels
    public float MinDifficulty { get; set; } = 1.0f;
    public float MaxDifficulty { get; set; } = 10.0f;
    public float DefaultDifficulty { get; set; } = 3.0f;

    // Win streak thresholds
    public int WinStreakEasyThreshold { get; set; } = 3;
    public int WinStreakHardThreshold { get; set; } = 5;
    public float WinStreakModifierStep { get; set; } = 0.5f;

    // Loss streak thresholds
    public int LossStreakEasyThreshold { get; set; } = 2;
    public int LossStreakHardThreshold { get; set; } = 4;
    public float LossStreakModifierStep { get; set; } = 0.3f;

    // Time decay settings
    public TimeSpan ShortBreakThreshold { get; set; } = TimeSpan.FromHours(6);
    public TimeSpan MediumBreakThreshold { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan LongBreakThreshold { get; set; } = TimeSpan.FromDays(7);

    public float ShortBreakModifier { get; set; } = 0.0f;
    public float MediumBreakModifier { get; set; } = -0.5f;
    public float LongBreakModifier { get; set; } = -1.0f;
    public float ExtendedBreakModifier { get; set; } = -2.0f;

    // Rage quit detection
    public float RageQuitTimeThreshold { get; set; } = 30.0f; // Seconds
    public float RageQuitModifier { get; set; } = -1.0f;

    // Engagement bonus
    public float PerfectPlayModifier { get; set; } = 0.3f;
    public float QuickWinModifier { get; set; } = 0.2f;
}
```

### DifficultyCalculationResult
```csharp
public class DifficultyCalculationResult
{
    public float FinalDifficulty { get; set; }
    public float PreviousDifficulty { get; set; }
    public DifficultyChangeReason ChangeReason { get; set; }
    public Dictionary<string, float> ModifierBreakdown { get; set; }

    // Recommendations
    public bool ShouldShowTutorialHint { get; set; }
    public bool ShouldOfferBooster { get; set; }
    public bool ShouldSimplifyLevel { get; set; }

    // Debug info
    public string CalculationLog { get; set; }
}

public enum DifficultyChangeReason
{
    WinStreak,
    LossStreak,
    TimeDecay,
    RageQuit,
    ReturningPlayer,
    EngagementBonus,
    NoChange
}
```

## Service Interfaces

### IDynamicUserDifficultyService
```csharp
public interface IDynamicUserDifficultyService
{
    // Core functionality
    DifficultyCalculationResult CalculateDifficulty(PlayerSessionData sessionData);
    void ApplyDifficulty(float difficulty);

    // Session tracking
    void OnSessionStart();
    void OnLevelStart(int levelId);
    void OnLevelComplete(bool won, float completionTime, int movesUsed);
    void OnSessionEnd(SessionEndType endType);

    // Data access
    PlayerSessionData GetCurrentSessionData();
    float GetCurrentDifficulty();
    DifficultyConfiguration GetConfiguration();

    // Analytics
    DifficultyAnalytics GetAnalytics();
}
```

### IDifficultyCalculator
```csharp
public interface IDifficultyCalculator
{
    float CalculateWinStreakModifier(int winStreak, int lossStreak, DifficultyConfiguration config);
    float CalculateTimeDecayModifier(TimeSpan timeSinceLastPlay, DifficultyConfiguration config);
    float CalculateLastSessionModifier(SessionEndType lastEndType, float sessionDuration, DifficultyConfiguration config);
    float CalculateEngagementModifier(SessionHistory[] recentSessions, DifficultyConfiguration config);
}
```

## Calculation Algorithms

### 1. Win Streak Modifier Algorithm
```
IF winStreak >= HardThreshold:
    modifier = winStreak * ModifierStep * 1.5
ELSE IF winStreak >= EasyThreshold:
    modifier = winStreak * ModifierStep
ELSE IF lossStreak >= HardThreshold:
    modifier = -(lossStreak * ModifierStep * 1.5)
ELSE IF lossStreak >= EasyThreshold:
    modifier = -(lossStreak * ModifierStep)
ELSE:
    modifier = 0
```

### 2. Time Decay Algorithm
```
timeSincePlay = Now - LastPlayTime

IF timeSincePlay < ShortBreakThreshold:
    modifier = 0
ELSE IF timeSincePlay < MediumBreakThreshold:
    modifier = MediumBreakModifier
ELSE IF timeSincePlay < LongBreakThreshold:
    modifier = LongBreakModifier
ELSE:
    modifier = ExtendedBreakModifier

// Apply smooth decay curve
decayFactor = 1 - exp(-timeSincePlay.Hours / 24)
modifier = modifier * decayFactor
```

### 3. Rage Quit Detection
```
IF LastSessionEndType == QuitAfterLoss:
    IF LastSessionDuration < RageQuitTimeThreshold:
        modifier = RageQuitModifier * 2
    ELSE:
        modifier = RageQuitModifier
ELSE IF LastSessionEndType == QuitDuringPlay:
    IF CompletionPercentage < 0.5:
        modifier = RageQuitModifier * 0.5
```

### 4. Engagement Modifier
```
// Reward consistent good performance
IF AverageMovesUsed <= OptimalMoves * 1.1:
    modifier += PerfectPlayModifier

// Reward quick completion
IF AverageCompletionTime < ExpectedTime * 0.7:
    modifier += QuickWinModifier

// Penalize very slow play (possible struggle)
IF AverageCompletionTime > ExpectedTime * 2:
    modifier -= 0.3
```

## Integration Points

### 1. UITemplate Controllers Integration
```csharp
// Required controller dependencies
- UITemplateLevelDataController: Access level progression
- UITemplateGameSessionDataController: Track session timing
- UITemplateInventoryDataController: Adjust rewards based on difficulty
- UITemplateAnalyticService: Track difficulty changes
```

### 2. Screw3D Gameplay Integration
```csharp
// Signal subscriptions
- WonSignal: Update win streak
- LostSignal: Update loss streak
- GamePausedSignal: Track potential quit
- GameResumedSignal: Calculate break time
- LevelStartSignal: Apply difficulty settings
```

### 3. Level Configuration Adjustment
```csharp
// Difficulty affects:
- Screw color distribution (via ScrewDistributionHelper)
- Number of screws per level
- Piece complexity
- Time limits (if applicable)
- Booster availability
- Hint frequency
```

## Implementation Plan

### Phase 1: Core Data Models (Week 1)
- [ ] Create PlayerSessionData model
- [ ] Create DifficultyConfiguration ScriptableObject
- [ ] Implement data persistence layer
- [ ] Set up VContainer registration

### Phase 2: Calculation Engine (Week 1-2)
- [ ] Implement IDifficultyCalculator
- [ ] Create modifier calculation methods
- [ ] Add rage quit detection
- [ ] Implement engagement scoring

### Phase 3: Service Implementation (Week 2)
- [ ] Create DynamicUserDifficultyService
- [ ] Integrate with UITemplate controllers
- [ ] Add signal subscriptions
- [ ] Implement session tracking

### Phase 4: Game Integration (Week 3)
- [ ] Modify ScrewDistributionHelper integration
- [ ] Adjust level loading based on difficulty
- [ ] Update UI difficulty indicators
- [ ] Add debug visualization

### Phase 5: Testing & Tuning (Week 3-4)
- [ ] Create unit tests for calculators
- [ ] Integration testing with game flow
- [ ] Parameter tuning based on playtesting
- [ ] Performance optimization

## Testing Strategy

### Unit Tests
```csharp
[Test] CalculateWinStreakModifier_ThreeWins_IncreasesDifficulty()
[Test] TimeDecay_LongBreak_DecreasesDifficulty()
[Test] RageQuit_Detection_AppliesStrongReduction()
[Test] EngagementBonus_PerfectPlay_IncreasesSlightly()
```

### Integration Tests
```csharp
[Test] SessionFlow_WinStreakToRageQuit_CorrectlyAdjusts()
[Test] DataPersistence_AcrossSessions_Maintains()
[Test] SignalIntegration_AllEvents_TrackedProperly()
```

### Manual Test Scenarios
1. **New Player Experience**
   - Start at default difficulty
   - Lose 3 times → Verify decrease
   - Win after losses → Check stabilization

2. **Returning Player**
   - Play session, quit after win
   - Return after 1 day → Verify slight decrease
   - Return after 1 week → Verify significant decrease

3. **Skilled Player**
   - Win 5+ consecutive levels
   - Verify difficulty increases appropriately
   - Check if reaches difficulty ceiling properly

4. **Rage Quit Scenario**
   - Fail level quickly
   - Quit immediately
   - Return and verify appropriate reduction

## Analytics Events

```csharp
// Track all difficulty changes
AnalyticService.Track("difficulty_calculated", new {
    previous_difficulty = 3.5f,
    new_difficulty = 4.0f,
    change_reason = "win_streak",
    win_streak = 3,
    time_since_play_hours = 2.5f,
    last_session_type = "completed_win",
    modifiers = {
        win_streak = 0.5f,
        time_decay = 0.0f,
        last_session = 0.0f,
        engagement = 0.0f
    }
});

// Track edge cases
AnalyticService.Track("rage_quit_detected", new {
    level_id = 42,
    play_duration = 15.3f,
    completion_percentage = 0.23f
});

// Track returning players
AnalyticService.Track("returning_player", new {
    days_away = 14,
    difficulty_reduction = 2.0f,
    previous_streak = 5
});
```

## Configuration Examples

### Casual Mode
```json
{
  "WinStreakEasyThreshold": 5,
  "WinStreakModifierStep": 0.3,
  "LossStreakEasyThreshold": 1,
  "LossStreakModifierStep": 0.5,
  "RageQuitModifier": -1.5
}
```

### Competitive Mode
```json
{
  "WinStreakEasyThreshold": 2,
  "WinStreakModifierStep": 0.7,
  "LossStreakEasyThreshold": 3,
  "LossStreakModifierStep": 0.2,
  "RageQuitModifier": -0.5
}
```

## Performance Considerations

1. **Caching**: Cache difficulty calculations for 1 level
2. **Async Operations**: Use UniTask for data persistence
3. **Memory**: Limit session history to last 10 sessions
4. **Calculation Time**: Target < 10ms for difficulty calculation

## Security & Anti-Cheat

1. **Server Validation**: Verify difficulty changes server-side
2. **Data Encryption**: Encrypt local session data
3. **Sanity Checks**: Validate time stamps and progression
4. **Rate Limiting**: Limit difficulty adjustments per session

## Future Enhancements

1. **Machine Learning**: Use player behavior patterns for prediction
2. **A/B Testing**: Test different difficulty curves
3. **Social Features**: Compare difficulty with friends
4. **Adaptive Tutorials**: Show hints based on struggle patterns
5. **Multi-factor Analysis**: Consider device type, play time patterns

## Appendix: Difficulty Mapping

### Level Difficulty to Game Parameters
```
Difficulty 1-2: Tutorial Mode
- 3 screw colors max
- Simple patterns
- Generous hints

Difficulty 3-4: Easy
- 4 screw colors
- Basic patterns
- Occasional hints

Difficulty 5-6: Normal
- 5 screw colors
- Complex patterns
- Rare hints

Difficulty 7-8: Hard
- 6+ screw colors
- Advanced patterns
- No hints

Difficulty 9-10: Expert
- Maximum colors
- Expert patterns
- Time pressure
```

## References

- [Game Design: Adaptive Difficulty](https://www.gamedeveloper.com/design/the-designer-s-notebook-difficulty-modes-and-dynamic-difficulty-adjustment)
- [Player Retention through Difficulty](https://deltadna.com/blog/difficulty-curve-balance/)
- [Rage Quit Psychology](https://www.psychologyofgames.com/2015/07/the-psychology-of-rage-quitting/)

---

*Document Version: 1.0*
*Last Updated: 2025-09-16*
*Author: DynamicUserDifficulty Service Team*