# Dynamic User Difficulty - Integration Guide

## Table of Contents
1. [Quick Start](#quick-start)
2. [UITemplate Integration](#uitemplates-integration)
3. [Screw3D Integration](#screw3d-integration)
4. [Signal Subscriptions](#signal-subscriptions)
5. [Analytics Integration](#analytics-integration)
6. [Configuration Setup](#configuration-setup)
7. [Common Integration Patterns](#common-integration-patterns)
8. [Troubleshooting](#troubleshooting)

## Quick Start

### 1. Enable Feature Flag
```bash
Edit → Project Settings → Player → Scripting Define Symbols
Add: THEONE_DYNAMIC_DIFFICULTY
```

### 2. Create Configuration Asset
```bash
Right-click in Project → Create → DynamicDifficulty → Config
Save as: Assets/Resources/Configs/DifficultyConfig.asset
```

### 3. Register in VContainer
```csharp
// In UITemplateVContainer.cs
#if THEONE_DYNAMIC_DIFFICULTY
var difficultyConfig = Resources.Load<DifficultyConfig>("Configs/DifficultyConfig");
builder.RegisterModule(new DynamicDifficultyModule(difficultyConfig));
#endif
```

## UITemplate Integration

### Step 1: Modify UITemplateVContainer

```csharp
// Path: Assets/UITemplate/Scripts/UITemplateVContainer.cs

using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.DI;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;

public class UITemplateVContainer : BaseVContainer
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Existing registrations...

        #if THEONE_DYNAMIC_DIFFICULTY
        RegisterDynamicDifficulty(builder);
        #endif
    }

    #if THEONE_DYNAMIC_DIFFICULTY
    private void RegisterDynamicDifficulty(IContainerBuilder builder)
    {
        var config = Resources.Load<DifficultyConfig>("Configs/DifficultyConfig");
        if (config == null)
        {
            Debug.LogError("DifficultyConfig not found in Resources/Configs/");
            return;
        }

        builder.RegisterModule(new DynamicDifficultyModule(config));
    }
    #endif
}
```

### Step 2: Create Bridge Service

```csharp
// Path: Runtime/Integration/DifficultyUITemplateBridge.cs

using System;
using VContainer.Unity;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Core;
using TheOneStudio.UITemplate.Models.Controllers;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Integration
{
    public class DifficultyUITemplateBridge : IInitializable, IDisposable
    {
        private readonly IDynamicDifficultyService difficultyService;
        private readonly UITemplateLevelDataController levelController;
        private readonly UITemplateGameSessionDataController sessionController;

        public DifficultyUITemplateBridge(
            IDynamicDifficultyService difficultyService,
            UITemplateLevelDataController levelController,
            UITemplateGameSessionDataController sessionController)
        {
            this.difficultyService = difficultyService;
            this.levelController = levelController;
            this.sessionController = sessionController;
        }

        public void Initialize()
        {
            // Subscribe to level events
            levelController.OnLevelStart += HandleLevelStart;
            levelController.OnLevelComplete += HandleLevelComplete;

            // Initialize difficulty service
            difficultyService.Initialize();
        }

        private void HandleLevelStart(int levelId)
        {
            // Calculate and apply difficulty
            var result = difficultyService.CalculateDifficulty();
            difficultyService.ApplyDifficulty(result);

            // Log for debugging
            Debug.Log($"Level {levelId} starting with difficulty: {result.NewDifficulty}");
        }

        private void HandleLevelComplete(int levelId, bool won, float time)
        {
            difficultyService.OnLevelComplete(won, time);
        }

        public void Dispose()
        {
            levelController.OnLevelStart -= HandleLevelStart;
            levelController.OnLevelComplete -= HandleLevelComplete;
        }
    }
}
```

## Screw3D Integration

### Step 1: Create Gameplay Bridge

```csharp
// Path: Runtime/Integration/DifficultyScrew3DBridge.cs

using System;
using VContainer.Unity;
using MessagePipe;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Core;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;
using Screw3D.Scripts.Models.Signals;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Integration
{
    public class DifficultyScrew3DBridge : IInitializable, IDisposable
    {
        private readonly IDynamicDifficultyService difficultyService;
        private readonly ISubscriber<WonSignal> wonSubscriber;
        private readonly ISubscriber<LostSignal> lostSubscriber;
        private readonly ISubscriber<GamePausedSignal> pauseSubscriber;
        private readonly ISubscriber<LevelStartSignal> levelStartSubscriber;

        private IDisposable wonSubscription;
        private IDisposable lostSubscription;
        private IDisposable pauseSubscription;
        private IDisposable levelStartSubscription;

        private DateTime levelStartTime;

        public DifficultyScrew3DBridge(
            IDynamicDifficultyService difficultyService,
            ISubscriber<WonSignal> wonSubscriber,
            ISubscriber<LostSignal> lostSubscriber,
            ISubscriber<GamePausedSignal> pauseSubscriber,
            ISubscriber<LevelStartSignal> levelStartSubscriber)
        {
            this.difficultyService = difficultyService;
            this.wonSubscriber = wonSubscriber;
            this.lostSubscriber = lostSubscriber;
            this.pauseSubscriber = pauseSubscriber;
            this.levelStartSubscriber = levelStartSubscriber;
        }

        public void Initialize()
        {
            // Subscribe to game signals
            wonSubscription = wonSubscriber.Subscribe(OnWon);
            lostSubscription = lostSubscriber.Subscribe(OnLost);
            pauseSubscription = pauseSubscriber.Subscribe(OnPaused);
            levelStartSubscription = levelStartSubscriber.Subscribe(OnLevelStart);
        }

        private void OnLevelStart(LevelStartSignal signal)
        {
            levelStartTime = DateTime.Now;
            difficultyService.OnLevelStart(signal.LevelId);

            // Apply difficulty to game
            ApplyDifficultyToGame();
        }

        private void OnWon(WonSignal signal)
        {
            var playTime = (float)(DateTime.Now - levelStartTime).TotalSeconds;
            difficultyService.OnLevelComplete(true, playTime);
        }

        private void OnLost(LostSignal signal)
        {
            var playTime = (float)(DateTime.Now - levelStartTime).TotalSeconds;
            difficultyService.OnLevelComplete(false, playTime);
        }

        private void OnPaused(GamePausedSignal signal)
        {
            // Track potential quit
            difficultyService.OnSessionEnd(SessionEndType.QuitDuringPlay);
        }

        private void ApplyDifficultyToGame()
        {
            var difficulty = difficultyService.CurrentDifficulty;

            // Map difficulty to game parameters
            ConfigureScrewDistribution(difficulty);
            ConfigurePieceComplexity(difficulty);
            ConfigureTimeLimit(difficulty);
        }

        private void ConfigureScrewDistribution(float difficulty)
        {
            // Inject into ScrewDistributionHelper
            int colorCount = Mathf.FloorToInt(2 + difficulty * 0.5f);
            int originalWeight = Mathf.FloorToInt(10 - difficulty);
            int additionalWeight = Mathf.FloorToInt(difficulty * 2);

            // Apply to screw distribution service
            // screwDistributionService.Configure(colorCount, originalWeight, additionalWeight);
        }

        private void ConfigurePieceComplexity(float difficulty)
        {
            // Configure piece settings based on difficulty
            // pieceService.SetComplexity(difficulty / 10f);
        }

        private void ConfigureTimeLimit(float difficulty)
        {
            // Optional: Add time pressure at higher difficulties
            if (difficulty > 7)
            {
                // Enable time limit
                // timerService.SetTimeLimit(300 - (difficulty * 10));
            }
        }

        public void Dispose()
        {
            wonSubscription?.Dispose();
            lostSubscription?.Dispose();
            pauseSubscription?.Dispose();
            levelStartSubscription?.Dispose();
        }
    }
}
```

### Step 2: Modify ScrewDistributionHelper

```csharp
// Path: Assets/Scripts/Services/Difficulty/ScrewDistributionHelper.cs

using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Core;

public class ScrewDistributionHelper
{
    private readonly IDynamicDifficultyService difficultyService;

    public ScrewDistributionHelper(IDynamicDifficultyService difficultyService = null)
    {
        this.difficultyService = difficultyService;
    }

    public int[] GetDistributionScrew(int screwNumber, int colorAmount,
                                      int originalWeight, int additionalWeight)
    {
        // Apply difficulty modifiers if service available
        if (difficultyService != null)
        {
            var difficulty = difficultyService.CurrentDifficulty;

            // Adjust parameters based on difficulty
            colorAmount = AdjustColorAmount(colorAmount, difficulty);
            originalWeight = AdjustOriginalWeight(originalWeight, difficulty);
            additionalWeight = AdjustAdditionalWeight(additionalWeight, difficulty);
        }

        // Continue with existing logic...
        return CalculateDistribution(screwNumber, colorAmount,
                                    originalWeight, additionalWeight);
    }

    private int AdjustColorAmount(int baseAmount, float difficulty)
    {
        // More colors at higher difficulty
        return Mathf.Clamp(
            baseAmount + Mathf.FloorToInt((difficulty - 3) * 0.3f),
            3, 8
        );
    }

    private int AdjustOriginalWeight(int baseWeight, float difficulty)
    {
        // Lower weight at higher difficulty (more random)
        return Mathf.Clamp(
            baseWeight - Mathf.FloorToInt(difficulty * 0.5f),
            1, 20
        );
    }

    private int AdjustAdditionalWeight(int baseWeight, float difficulty)
    {
        // Higher clustering at lower difficulty (easier)
        return Mathf.Clamp(
            baseWeight + Mathf.FloorToInt((10 - difficulty) * 0.5f),
            1, 10
        );
    }
}
```

## Signal Subscriptions

### MessagePipe Setup

```csharp
// Path: Runtime/Integration/SignalConfiguration.cs

using MessagePipe;
using VContainer;

public static class DifficultySignalConfiguration
{
    public static void RegisterSignals(IContainerBuilder builder)
    {
        // Register MessagePipe
        var options = builder.RegisterMessagePipe();

        // Register game signals
        builder.RegisterMessageBroker<WonSignal>(options);
        builder.RegisterMessageBroker<LostSignal>(options);
        builder.RegisterMessageBroker<GamePausedSignal>(options);
        builder.RegisterMessageBroker<GameResumedSignal>(options);
        builder.RegisterMessageBroker<LevelStartSignal>(options);

        // Register difficulty change signal
        builder.RegisterMessageBroker<DifficultyChangedSignal>(options);
    }
}

// Custom signal for difficulty changes
public struct DifficultyChangedSignal
{
    public float PreviousDifficulty;
    public float NewDifficulty;
    public string Reason;
}
```

### Signal Flow Diagram

```
LevelStart → DifficultyService.Calculate() → Apply to Game
    ↓
Player Plays Level
    ↓
Win/Loss → Update Streaks → Save Session Data
    ↓
Next Level → Recalculate Difficulty
```

## Analytics Integration

### Track Difficulty Events

```csharp
// Path: Runtime/Integration/DifficultyAnalytics.cs

using System.Collections.Generic;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

public class DifficultyAnalytics
{
    private readonly IAnalyticService analyticService;

    public void TrackDifficultyChange(DifficultyResult result)
    {
        analyticService.Track("difficulty_changed", new Dictionary<string, object>
        {
            ["previous_difficulty"] = result.PreviousDifficulty,
            ["new_difficulty"] = result.NewDifficulty,
            ["primary_reason"] = result.PrimaryReason,
            ["modifier_count"] = result.AppliedModifiers.Count,
            ["timestamp"] = result.CalculatedAt.ToString("O")
        });

        // Track individual modifiers
        foreach (var modifier in result.AppliedModifiers)
        {
            if (Math.Abs(modifier.Value) > 0.01f)
            {
                analyticService.Track($"modifier_{modifier.ModifierName}", new Dictionary<string, object>
                {
                    ["value"] = modifier.Value,
                    ["reason"] = modifier.Reason,
                    ["metadata"] = modifier.Metadata
                });
            }
        }
    }

    public void TrackRageQuit(SessionInfo session)
    {
        analyticService.Track("rage_quit_detected", new Dictionary<string, object>
        {
            ["level_id"] = session.LevelId,
            ["play_duration"] = session.PlayDuration,
            ["session_type"] = session.EndType.ToString()
        });
    }

    public void TrackPlayerReturn(TimeSpan timeSinceLastPlay)
    {
        analyticService.Track("player_returned", new Dictionary<string, object>
        {
            ["hours_away"] = timeSinceLastPlay.TotalHours,
            ["days_away"] = timeSinceLastPlay.TotalDays
        });
    }
}
```

## Configuration Setup

### 1. Create Default Config

```csharp
// Editor script to create default config
[MenuItem("Assets/Create/DynamicDifficulty/Default Config")]
public static void CreateDefaultConfig()
{
    var config = ScriptableObject.CreateInstance<DifficultyConfig>();

    // Set default values
    config.MinDifficulty = 1f;
    config.MaxDifficulty = 10f;
    config.DefaultDifficulty = 3f;
    config.MaxChangePerSession = 2f;

    // Add default modifiers
    config.ModifierConfigs = new List<ModifierConfig>
    {
        CreateModifierConfig("WinStreak", 0, true),
        CreateModifierConfig("LossStreak", 1, true),
        CreateModifierConfig("TimeDecay", 2, true),
        CreateModifierConfig("RageQuit", 3, true)
    };

    // Save asset
    AssetDatabase.CreateAsset(config,
        "Assets/Resources/Configs/DifficultyConfig.asset");
    AssetDatabase.SaveAssets();
}
```

### 2. Configure in Inspector

1. Select DifficultyConfig asset
2. Adjust difficulty range (1-10 recommended)
3. Configure each modifier:
   - Set parameters
   - Adjust response curves
   - Set priorities
4. Enable debug logs for testing

### 3. Environment-Specific Configs

```csharp
// Load different configs per environment
public static DifficultyConfig LoadConfig()
{
    string configPath = "Configs/DifficultyConfig";

    #if DEVELOPMENT_BUILD
    configPath = "Configs/DifficultyConfig_Dev";
    #elif UNITY_EDITOR
    configPath = "Configs/DifficultyConfig_Editor";
    #endif

    return Resources.Load<DifficultyConfig>(configPath);
}
```

## Common Integration Patterns

### 1. Level Generation Integration

```csharp
public class LevelGenerator
{
    private readonly IDynamicDifficultyService difficultyService;

    public LevelData GenerateLevel(int levelId)
    {
        var difficulty = difficultyService.CurrentDifficulty;

        return new LevelData
        {
            ScrewCount = CalculateScrewCount(difficulty),
            ColorCount = CalculateColorCount(difficulty),
            PieceComplexity = CalculatePieceComplexity(difficulty),
            TimeLimit = CalculateTimeLimit(difficulty)
        };
    }

    private int CalculateScrewCount(float difficulty)
    {
        // Base: 20-50, scales with difficulty
        return Mathf.FloorToInt(20 + difficulty * 3);
    }

    private int CalculateColorCount(float difficulty)
    {
        // 3-7 colors based on difficulty
        return Mathf.Clamp(2 + Mathf.FloorToInt(difficulty * 0.5f), 3, 7);
    }
}
```

### 2. Booster System Integration

```csharp
public class BoosterManager
{
    private readonly IDynamicDifficultyService difficultyService;

    public void ConfigureBoosters()
    {
        var difficulty = difficultyService.CurrentDifficulty;

        // Fewer boosters at lower difficulty
        if (difficulty < 3)
        {
            EnableBooster("AddBox", true);
            EnableBooster("RemoveScrew", false);
        }
        // More boosters at higher difficulty
        else if (difficulty > 7)
        {
            EnableBooster("AddBox", true);
            EnableBooster("RemoveScrew", true);
            EnableBooster("ClearQueue", true);
        }
    }
}
```

### 3. Tutorial Integration

```csharp
public class TutorialManager
{
    private readonly IDynamicDifficultyService difficultyService;

    public bool ShouldShowHint(int hintId)
    {
        var difficulty = difficultyService.CurrentDifficulty;

        // More hints at lower difficulty
        if (difficulty < 3)
            return true;

        // Selective hints at medium difficulty
        if (difficulty < 6)
            return IsEssentialHint(hintId);

        // No hints at high difficulty
        return false;
    }
}
```

## Troubleshooting

### Common Integration Issues

#### 1. Service Not Initialized
```csharp
// Problem: NullReferenceException
// Solution: Ensure Initialize() is called

public class GameStartup : IInitializable
{
    public void Initialize()
    {
        difficultyService.Initialize(); // Required!
    }
}
```

#### 2. Signals Not Firing
```csharp
// Problem: Difficulty not updating
// Solution: Verify signal subscriptions

// Check registration in VContainer
builder.RegisterMessageBroker<WonSignal>(options);

// Verify subscription
wonSubscription = wonSubscriber.Subscribe(OnWon);
```

#### 3. Config Not Loading
```csharp
// Problem: Config is null
// Solution: Check Resources path

// Must be in Resources folder
var config = Resources.Load<DifficultyConfig>("Configs/DifficultyConfig");

if (config == null)
{
    Debug.LogError("Config not found at Resources/Configs/DifficultyConfig");
}
```

#### 4. Difficulty Not Applying
```csharp
// Problem: Game doesn't reflect difficulty
// Solution: Ensure ApplyDifficulty is called

var result = difficultyService.CalculateDifficulty();
difficultyService.ApplyDifficulty(result); // Don't forget this!
```

### Debug Checklist

- [ ] Feature flag `THEONE_DYNAMIC_DIFFICULTY` enabled
- [ ] DifficultyConfig in Resources/Configs/
- [ ] Service registered in VContainer
- [ ] Bridges registered and initialized
- [ ] Signals properly subscribed
- [ ] Analytics tracking events
- [ ] Debug logs enabled in config

---

*Last Updated: 2025-09-16*