# Dynamic User Difficulty - Integration Guide

## 📋 Table of Contents
1. [Quick Start](#quick-start)
2. [Integration Overview](#integration-overview)
3. [Step-by-Step Implementation](#step-by-step-implementation)
4. [Code Templates](#code-templates)
5. [Game-Specific Adapters](#game-specific-adapters)
6. [UITemplate Integration](#uitemplates-integration)
7. [Screw3D Integration](#screw3d-integration)
8. [Signal Subscriptions](#signal-subscriptions)
9. [Analytics Integration](#analytics-integration)
10. [Configuration Setup](#configuration-setup)
11. [Common Integration Patterns](#common-integration-patterns)
12. [Testing Your Integration](#testing-your-integration)
13. [Troubleshooting](#troubleshooting)
14. [Complete Integration Example](#complete-integration-example)

## 🔥 Important: Complete Screw3D Integration Available

A **full working implementation** for Screw3D has been created at:
- **Location**: `Assets/Scripts/Services/Difficulty/`
- **Documentation**: `Assets/Scripts/Services/Difficulty/INTEGRATION_EXAMPLE.md`
- **Components**:
  - `DifficultyGameplayAdapter.cs` - Converts difficulty to gameplay parameters
  - `DifficultyGameplayBridge.cs` - Connects to Screw3D signals
  - `LevelDifficultyModifier.cs` - Modifies levels when loaded
  - `Screw3DServiceExtension.cs` - Integration helpers
  - `DifficultyDebugUI.cs` - Debug interface (F9 to toggle)

See the [Complete Integration Example](#complete-integration-example) section at the bottom for details.

## Quick Start

### Prerequisites
- Unity 2021.3 or higher
- VContainer dependency injection framework
- Dynamic Difficulty module installed via Package Manager

### Basic Integration Steps
1. Enable the feature flag: `THEONE_DYNAMIC_DIFFICULTY`
2. Create your game adapter
3. Create gameplay bridge
4. Register services in DI
5. Test and adjust parameters

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

## Integration Overview

The Dynamic Difficulty system requires three main components:

```
┌─────────────────────────────────────────┐
│           Your Game                      │
├─────────────────────────────────────────┤
│  GameplayAdapter  │  GameplayBridge     │
│  (Parameters)     │  (Event Tracking)   │
├─────────────────────────────────────────┤
│       Dynamic Difficulty Service        │
│  (Core Logic, Modifiers, Persistence)   │
└─────────────────────────────────────────┘
```

## Step-by-Step Implementation

### Step 1: Enable the Feature Flag

Add to Unity Player Settings → Scripting Define Symbols:
```
THEONE_DYNAMIC_DIFFICULTY
```

### Step 2: Create Your Game Adapter

Create a file `YourGameDifficultyAdapter.cs`:

```csharp
using UnityEngine;
using VContainer;
using TheOneStudio.DynamicUserDifficulty.Services;

namespace YourGame.Services.Difficulty
{
    /// <summary>
    /// Converts difficulty level (1-10) to game-specific parameters
    /// </summary>
    public class YourGameDifficultyAdapter
    {
        private readonly IDynamicUserDifficultyService difficultyService;

        [Inject]
        public YourGameDifficultyAdapter(IDynamicUserDifficultyService difficultyService)
        {
            this.difficultyService = difficultyService;
        }

        /// <summary>
        /// Get all adjusted parameters for current difficulty
        /// </summary>
        public GameParameters GetAdjustedParameters()
        {
            float difficulty = difficultyService.CurrentDifficulty;

            return new GameParameters
            {
                // Example: Enemy count increases with difficulty
                EnemyCount = CalculateEnemyCount(difficulty),

                // Example: Time limit decreases with difficulty
                TimeLimit = CalculateTimeLimit(difficulty),

                // Example: Player health decreases with difficulty
                PlayerHealth = CalculatePlayerHealth(difficulty),

                // Example: Score multiplier increases with difficulty
                ScoreMultiplier = CalculateScoreMultiplier(difficulty),

                // Add your game-specific parameters here
                // ...
            };
        }

        // Linear scaling: 5 enemies at difficulty 1, 20 at difficulty 10
        private int CalculateEnemyCount(float difficulty)
        {
            return Mathf.RoundToInt(5 + (difficulty - 1) * 1.67f);
        }

        // Inverse scaling: 300 seconds at difficulty 1, 60 at difficulty 10
        private float CalculateTimeLimit(float difficulty)
        {
            return 300f - ((difficulty - 1) * 26.67f);
        }

        // Inverse scaling: 100 HP at difficulty 1, 30 HP at difficulty 10
        private int CalculatePlayerHealth(float difficulty)
        {
            return Mathf.RoundToInt(100 - ((difficulty - 1) * 7.78f));
        }

        // Linear scaling: 1.0x at difficulty 1, 2.0x at difficulty 10
        private float CalculateScoreMultiplier(float difficulty)
        {
            return 1.0f + ((difficulty - 1) / 9f);
        }
    }

    /// <summary>
    /// Container for all game-specific parameters
    /// </summary>
    public class GameParameters
    {
        public int EnemyCount { get; set; }
        public float TimeLimit { get; set; }
        public int PlayerHealth { get; set; }
        public float ScoreMultiplier { get; set; }
        // Add your game-specific parameters here
    }
}
```

### Step 3: Create Gameplay Bridge

Create a file `YourGameDifficultyBridge.cs`:

```csharp
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using TheOneStudio.DynamicUserDifficulty.Services;
using Zenject;

namespace YourGame.Services.Difficulty
{
    /// <summary>
    /// Connects game events to difficulty system using SignalBus
    /// </summary>
    public class YourGameDifficultyBridge : IInitializable, IDisposable
    {
        private readonly IDynamicUserDifficultyService difficultyService;
        private readonly YourGameDifficultyAdapter adapter;
        private readonly SignalBus signalBus;

        private DateTime levelStartTime;

        [Inject]
        public YourGameDifficultyBridge(
            IDynamicUserDifficultyService difficultyService,
            YourGameDifficultyAdapter adapter,
            SignalBus signalBus)
        {
            this.difficultyService = difficultyService;
            this.adapter = adapter;
            this.signalBus = signalBus;
        }

        public void Initialize()
        {
            Debug.Log($"[DifficultyBridge] Initializing with difficulty: {difficultyService.CurrentDifficulty}");

            // Load saved difficulty data
            difficultyService.LoadData();

            // Subscribe to game events
            signalBus.Subscribe<WonSignal>(OnPlayerWon);
            signalBus.Subscribe<LostSignal>(OnPlayerLost);
            signalBus.Subscribe<QuitSignal>(OnPlayerQuit);

            // Track level start time
            levelStartTime = DateTime.Now;
        }

        private void OnPlayerWon(WonSignal signal)
        {
            float playTime = (float)(DateTime.Now - levelStartTime).TotalSeconds;

            Debug.Log($"[DifficultyBridge] Player won! Time: {playTime:F1}s");

            // Record the win
            difficultyService.RecordWin();
            difficultyService.OnLevelComplete(true, playTime);

            // Calculate new difficulty
            var oldDifficulty = difficultyService.CurrentDifficulty;
            var result = difficultyService.CalculateDifficulty();
            difficultyService.ApplyDifficulty(result);
            var newDifficulty = difficultyService.CurrentDifficulty;

            // Save the data
            difficultyService.SaveData();

            // Notify about difficulty change
            if (Math.Abs(oldDifficulty - newDifficulty) > 0.01f)
            {
                var parameters = adapter.GetAdjustedParameters();
                signalBus.Fire(new DifficultyChangedSignal
                {
                    OldDifficulty = oldDifficulty,
                    NewDifficulty = newDifficulty,
                    Parameters = parameters
                });
            }

            // Reset for next level
            levelStartTime = DateTime.Now;
        }

        private void OnPlayerLost(LostSignal signal)
        {
            float playTime = (float)(DateTime.Now - levelStartTime).TotalSeconds;

            Debug.Log($"[DifficultyBridge] Player lost! Time: {playTime:F1}s");

            // Record the loss
            difficultyService.RecordLoss();
            difficultyService.OnLevelComplete(false, playTime);

            // Calculate new difficulty
            var oldDifficulty = difficultyService.CurrentDifficulty;
            var result = difficultyService.CalculateDifficulty();
            difficultyService.ApplyDifficulty(result);
            var newDifficulty = difficultyService.CurrentDifficulty;

            // Save the data
            difficultyService.SaveData();

            // Notify about difficulty change
            if (Math.Abs(oldDifficulty - newDifficulty) > 0.01f)
            {
                var parameters = adapter.GetAdjustedParameters();
                signalBus.Fire(new DifficultyChangedSignal
                {
                    OldDifficulty = oldDifficulty,
                    NewDifficulty = newDifficulty,
                    Parameters = parameters
                });
            }

            // Reset for next level
            levelStartTime = DateTime.Now;
        }

        private void OnPlayerQuit(QuitSignal signal)
        {
            float playTime = (float)(DateTime.Now - levelStartTime).TotalSeconds;

            // Check for rage quit (quit within 30 seconds)
            if (playTime < 30f)
            {
                Debug.Log($"[DifficultyBridge] Rage quit detected! Time: {playTime:F1}s");
                difficultyService.RecordRageQuit(playTime);

                // Apply difficulty adjustment
                var result = difficultyService.CalculateDifficulty();
                difficultyService.ApplyDifficulty(result);
                difficultyService.SaveData();
            }
        }

        public void Dispose()
        {
            // Unsubscribe from signals
            signalBus.TryUnsubscribe<WonSignal>(OnPlayerWon);
            signalBus.TryUnsubscribe<LostSignal>(OnPlayerLost);
            signalBus.TryUnsubscribe<QuitSignal>(OnPlayerQuit);

            // Save data on dispose
            difficultyService.SaveData();
        }
    }

    // Signal definitions (if not already defined in your game)
    public sealed record WonSignal;
    public sealed record LostSignal;
    public sealed record QuitSignal;

    public class DifficultyChangedSignal
    {
        public float OldDifficulty { get; set; }
        public float NewDifficulty { get; set; }
        public GameParameters Parameters { get; set; }
    }
```

### Step 4: Register in Dependency Injection

Add to your DI composition root (e.g., `GameLifetimeScope.cs`):

```csharp
using VContainer;
using VContainer.Unity;
using TheOneStudio.DynamicUserDifficulty.DI;
using TheOneStudio.DynamicUserDifficulty.Configuration;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // ... existing registrations ...

        #if THEONE_DYNAMIC_DIFFICULTY
        RegisterDynamicDifficulty(builder);
        #endif
    }

    private void RegisterDynamicDifficulty(IContainerBuilder builder)
    {
        // Register the Dynamic Difficulty module
        DynamicDifficultyModule.RegisterServices(builder);

        // Register your game-specific adapters
        builder.Register<YourGameDifficultyAdapter>(Lifetime.Singleton);
        builder.RegisterEntryPoint<YourGameDifficultyBridge>();

        // Optional: Register debug UI
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        builder.RegisterEntryPoint<DifficultyDebugUI>();
        #endif
    }
}
```

### Step 5: Create Level Modifier (Optional)

If you need to modify levels when they load:

```csharp
using UnityEngine;
using VContainer;

namespace YourGame.Services.Difficulty
{
    public class LevelDifficultyModifier : MonoBehaviour
    {
        [Inject] private YourGameDifficultyAdapter adapter;

        private void Start()
        {
            ApplyDifficultyToLevel();
        }

        private void ApplyDifficultyToLevel()
        {
            var parameters = adapter.GetAdjustedParameters();

            // Example: Spawn enemies based on difficulty
            var enemySpawner = GetComponent<EnemySpawner>();
            if (enemySpawner != null)
            {
                enemySpawner.SetEnemyCount(parameters.EnemyCount);
            }

            // Example: Set time limit
            var timer = GetComponent<LevelTimer>();
            if (timer != null)
            {
                timer.SetTimeLimit(parameters.TimeLimit);
            }

            // Example: Set player health
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var health = player.GetComponent<Health>();
                health.SetMaxHealth(parameters.PlayerHealth);
            }
        }
    }
}
```

## Code Templates

### Template 1: Simple Integration

For games that just need basic win/loss tracking:

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;
using TheOneStudio.DynamicUserDifficulty.Services;

public class SimpleDifficultyIntegration : IInitializable
{
    private readonly IDynamicUserDifficultyService difficultyService;

    [Inject]
    public SimpleDifficultyIntegration(IDynamicUserDifficultyService difficultyService)
    {
        this.difficultyService = difficultyService;
    }

    public void Initialize()
    {
        difficultyService.LoadData();
        Debug.Log($"Starting difficulty: {difficultyService.CurrentDifficulty}");
    }

    public void OnLevelComplete(bool won, float timeSpent)
    {
        if (won)
        {
            difficultyService.RecordWin();
        }
        else
        {
            difficultyService.RecordLoss();
        }

        difficultyService.OnLevelComplete(won, timeSpent);

        var result = difficultyService.CalculateDifficulty();
        difficultyService.ApplyDifficulty(result);
        difficultyService.SaveData();

        Debug.Log($"New difficulty: {difficultyService.CurrentDifficulty}");
    }

    public float GetDifficultyMultiplier()
    {
        // Returns 0.5 to 1.5 based on difficulty 1-10
        return 0.5f + (difficultyService.CurrentDifficulty - 1) / 9f;
    }
}
```

### Template 2: RPG Game Integration

For RPG games with complex stats:

```csharp
public class RPGDifficultyAdapter
{
    private readonly IDynamicUserDifficultyService difficultyService;

    public RPGStats GetAdjustedStats()
    {
        float d = difficultyService.CurrentDifficulty;

        return new RPGStats
        {
            // Enemy stats scale up
            EnemyHP = 100 + (int)((d - 1) * 50),
            EnemyDamage = 10 + (int)((d - 1) * 5),
            EnemyDefense = 5 + (int)((d - 1) * 3),

            // Loot scales up
            GoldMultiplier = 0.8f + (d - 1) * 0.13f,
            ExpMultiplier = 0.9f + (d - 1) * 0.12f,
            RareDropChance = 0.05f + (d - 1) * 0.022f,

            // Player disadvantages at higher difficulty
            PotionEffectiveness = 1.2f - (d - 1) * 0.022f,
            SkillCooldownMultiplier = 0.8f + (d - 1) * 0.044f
        };
    }
}
```

### Template 3: Puzzle Game Integration

For puzzle games like match-3 or Screw3D:

```csharp
public class PuzzleDifficultyAdapter
{
    private readonly IDynamicUserDifficultyService difficultyService;

    public PuzzleParameters GetAdjustedParameters()
    {
        float d = difficultyService.CurrentDifficulty;

        return new PuzzleParameters
        {
            // Piece variety increases difficulty
            ColorCount = Mathf.RoundToInt(3 + (d - 1) * 0.44f), // 3-7 colors

            // Time pressure
            TimeLimit = 180f - (d - 1) * 13.33f, // 180-60 seconds

            // Help availability
            HintCount = Mathf.Max(0, 5 - (int)((d - 1) * 0.55f)),
            UndoMoves = Mathf.Max(0, 3 - (int)((d - 1) * 0.33f)),

            // Board complexity
            BoardSize = 6 + (int)((d - 1) * 0.44f), // 6x6 to 10x10
            ObstacleCount = (int)((d - 1) * 1.11f), // 0-10 obstacles

            // Scoring
            ScoreMultiplier = 0.8f + (d - 1) * 0.044f,
            ComboMultiplier = 1.0f + (d - 1) * 0.111f
        };
    }
}
```

### Template 4: Platformer Integration

For platformer games:

```csharp
public class PlatformerDifficultyAdapter
{
    private readonly IDynamicUserDifficultyService difficultyService;

    public PlatformerSettings GetAdjustedSettings()
    {
        float d = difficultyService.CurrentDifficulty;

        return new PlatformerSettings
        {
            // Player abilities
            JumpHeight = 5.0f - (d - 1) * 0.111f,
            MoveSpeed = 8.0f - (d - 1) * 0.222f,
            Lives = Mathf.Max(1, 5 - (int)((d - 1) * 0.44f)),

            // Level hazards
            EnemySpeed = 3.0f + (d - 1) * 0.333f,
            PlatformSpeed = 2.0f + (d - 1) * 0.222f,
            SpikeDamage = 10 + (int)((d - 1) * 3.33f),

            // Checkpoints
            CheckpointDistance = 100f + (d - 1) * 11.11f,

            // Collectibles
            CoinValue = (int)(10 * (0.8f + (d - 1) * 0.044f)),
            PowerupDuration = 10f - (d - 1) * 0.666f
        };
    }
}
```

## Game-Specific Adapters

### For Match-3 Games
```csharp
public class Match3DifficultyAdapter
{
    // Difficulty 1-10 affects:
    // - Number of gem types (4-8)
    // - Move limit (50-20)
    // - Required score (1000-5000)
    // - Special gem spawn rate (20%-5%)
    // - Cascade bonus multiplier (1.0x-2.0x)
}
```

### For Tower Defense
```csharp
public class TowerDefenseDifficultyAdapter
{
    // Difficulty 1-10 affects:
    // - Enemy wave size (10-30)
    // - Enemy HP multiplier (1.0x-3.0x)
    // - Enemy speed (1.0x-1.5x)
    // - Starting money (1000-500)
    // - Tower damage (1.2x-0.8x)
}
```

### For Racing Games
```csharp
public class RacingDifficultyAdapter
{
    // Difficulty 1-10 affects:
    // - AI driver skill (30%-95%)
    // - Player car grip (1.2x-0.8x)
    // - Nitro recharge rate (2.0x-0.5x)
    // - Weather effects (0%-50%)
    // - Track obstacles (0-10)
}
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
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Zenject;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Core;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;
using Screw3D.Scripts.Models.Signals;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Integration
{
    public class DifficultyScrew3DBridge : IInitializable, IDisposable
    {
        private readonly IDynamicDifficultyService difficultyService;
        private readonly SignalBus signalBus;

        private DateTime levelStartTime;

        [Inject]
        public DifficultyScrew3DBridge(
            IDynamicDifficultyService difficultyService,
            SignalBus signalBus)
        {
            this.difficultyService = difficultyService;
            this.signalBus = signalBus;
        }

        public void Initialize()
        {
            // Subscribe to game signals
            signalBus.Subscribe<WonSignal>(OnWon);
            signalBus.Subscribe<LostSignal>(OnLost);
            signalBus.Subscribe<GamePausedSignal>(OnPaused);
            signalBus.Subscribe<LevelStartSignal>(OnLevelStart);
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

            // Fire difficulty changed signal if needed
            var result = difficultyService.CalculateDifficulty();
            if (Math.Abs(result.NewDifficulty - result.PreviousDifficulty) > 0.01f)
            {
                difficultyService.ApplyDifficulty(result);
                signalBus.Fire(new DifficultyChangedSignal
                {
                    PreviousDifficulty = result.PreviousDifficulty,
                    NewDifficulty = result.NewDifficulty,
                    Reason = "Win"
                });
            }
        }

        private void OnLost(LostSignal signal)
        {
            var playTime = (float)(DateTime.Now - levelStartTime).TotalSeconds;
            difficultyService.OnLevelComplete(false, playTime);

            // Fire difficulty changed signal if needed
            var result = difficultyService.CalculateDifficulty();
            if (Math.Abs(result.NewDifficulty - result.PreviousDifficulty) > 0.01f)
            {
                difficultyService.ApplyDifficulty(result);
                signalBus.Fire(new DifficultyChangedSignal
                {
                    PreviousDifficulty = result.PreviousDifficulty,
                    NewDifficulty = result.NewDifficulty,
                    Reason = "Loss"
                });
            }
        }

        private void OnPaused(GamePausedSignal signal)
        {
            // Track potential quit
            var playTime = (float)(DateTime.Now - levelStartTime).TotalSeconds;
            if (playTime < 30f)
            {
                difficultyService.RecordRageQuit(playTime);
            }
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
            // Unsubscribe from signals
            signalBus.TryUnsubscribe<WonSignal>(OnWon);
            signalBus.TryUnsubscribe<LostSignal>(OnLost);
            signalBus.TryUnsubscribe<GamePausedSignal>(OnPaused);
            signalBus.TryUnsubscribe<LevelStartSignal>(OnLevelStart);
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

### SignalBus Setup

```csharp
// Path: Runtime/Integration/SignalConfiguration.cs

using Zenject;
using VContainer;

public static class DifficultySignalConfiguration
{
    public static void RegisterSignals(IContainerBuilder builder)
    {
        // Register SignalBus (if not already registered)
        builder.Register<SignalBus>(Lifetime.Singleton);

        // Declare signals for SignalBus
        builder.RegisterBuildCallback(container =>
        {
            var signalBus = container.Resolve<SignalBus>();

            // Declare game signals
            signalBus.DeclareSignal<WonSignal>();
            signalBus.DeclareSignal<LostSignal>();
            signalBus.DeclareSignal<GamePausedSignal>();
            signalBus.DeclareSignal<GameResumedSignal>();
            signalBus.DeclareSignal<LevelStartSignal>();

            // Declare difficulty change signal
            signalBus.DeclareSignal<DifficultyChangedSignal>();
        });
    }
}

// Custom signal for difficulty changes
public class DifficultyChangedSignal
{
    public float PreviousDifficulty { get; set; }
    public float NewDifficulty { get; set; }
    public string Reason { get; set; }
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

## Complete Integration Example

### 🎮 Screw3D Full Integration

A complete, production-ready integration has been implemented for the Screw3D project. This includes all necessary components, adapters, and UI elements.

#### Implementation Files

Located at `Assets/Scripts/Services/Difficulty/`:

1. **DifficultyGameplayAdapter.cs**
   - Translates difficulty (1-10) to gameplay parameters
   - Provides color counts, time limits, hints, score multipliers
   - Returns level ranges and reward multipliers

2. **DifficultyGameplayBridge.cs**
   - Connects to Screw3D signals (Win/Loss/Progress)
   - Automatically tracks player performance
   - Publishes DifficultyChangedSignal when difficulty adjusts

3. **LevelDifficultyModifier.cs**
   - Modifies ScrewColorRandomizer when levels load
   - Adjusts piece physics based on difficulty
   - Stores metadata for tracking

4. **Screw3DServiceExtension.cs**
   - Helper service for easy integration
   - Provides methods for scores, hints, rewards
   - Manages difficulty-aware gameplay

5. **DifficultyDebugUI.cs**
   - Press F9 to toggle debug interface
   - Shows current difficulty, parameters, stats
   - Test buttons for simulating wins/losses (Editor only)

#### Documentation

See **`Assets/Scripts/Services/Difficulty/INTEGRATION_EXAMPLE.md`** for:
- Complete code examples with full implementation
- Step-by-step integration walkthrough
- Real game flow diagrams
- Testing scenarios and patterns
- Troubleshooting guide

#### Quick Usage

```csharp
// The system works automatically once enabled!

// 1. Enable flag
Add THEONE_DYNAMIC_DIFFICULTY to Scripting Define Symbols

// 2. Play the game
- Difficulty adjusts based on wins/losses
- Colors, time, hints all adapt automatically
- Press F9 to see debug info

// 3. Access in code (if needed)
[Inject] Screw3DServiceExtension extension;
var timeLimit = extension.GetAdjustedTimeLimit();
var score = extension.GetAdjustedScore(baseScore);
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

## Testing Your Integration

### 1. Unit Test Your Adapter
```csharp
[Test]
public void TestAdapterScaling()
{
    var mockService = new Mock<IDynamicUserDifficultyService>();
    mockService.Setup(s => s.CurrentDifficulty).Returns(5f);

    var adapter = new YourGameDifficultyAdapter(mockService.Object);
    var parameters = adapter.GetAdjustedParameters();

    Assert.AreEqual(expectedEnemyCount, parameters.EnemyCount);
    Assert.AreEqual(expectedTimeLimit, parameters.TimeLimit);
}
```

### 2. Integration Test Checklist
- [ ] Win 3 games → Difficulty increases
- [ ] Lose 2 games → Difficulty decreases
- [ ] Quit early → Rage quit penalty applied
- [ ] Wait 2 days → Time decay reduces difficulty
- [ ] Parameters update correctly at each difficulty level

### 3. Debug UI Testing

Create a simple debug UI:

```csharp
public class DifficultyDebugUI : MonoBehaviour
{
    [Inject] private IDynamicUserDifficultyService difficultyService;
    [Inject] private YourGameDifficultyAdapter adapter;

    private bool showDebug = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
            showDebug = !showDebug;
    }

    void OnGUI()
    {
        if (!showDebug) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.Box("Difficulty Debug");

        GUILayout.Label($"Current: {difficultyService.CurrentDifficulty:F1}");

        var stats = difficultyService.GetDifficultyStats();
        GUILayout.Label($"Win Streak: {stats["WinStreak"]}");
        GUILayout.Label($"Loss Streak: {stats["LossStreak"]}");

        var parameters = adapter.GetAdjustedParameters();
        GUILayout.Label($"Enemies: {parameters.EnemyCount}");
        GUILayout.Label($"Time: {parameters.TimeLimit:F0}s");

        if (GUILayout.Button("Simulate Win"))
        {
            difficultyService.RecordWin();
            difficultyService.OnLevelComplete(true, 120f);
            var result = difficultyService.CalculateDifficulty();
            difficultyService.ApplyDifficulty(result);
        }

        if (GUILayout.Button("Simulate Loss"))
        {
            difficultyService.RecordLoss();
            difficultyService.OnLevelComplete(false, 90f);
            var result = difficultyService.CalculateDifficulty();
            difficultyService.ApplyDifficulty(result);
        }

        GUILayout.EndArea();
    }
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

## Common Patterns

### Pattern 1: Difficulty Ramps

```csharp
public static class DifficultyRamps
{
    // Linear: Even progression
    public static float Linear(float difficulty, float min, float max)
    {
        return min + (max - min) * (difficulty - 1) / 9f;
    }

    // Exponential: Slow start, rapid end
    public static float Exponential(float difficulty, float min, float max)
    {
        float t = (difficulty - 1) / 9f;
        return min + (max - min) * t * t;
    }

    // Logarithmic: Fast start, slow end
    public static float Logarithmic(float difficulty, float min, float max)
    {
        float t = Mathf.Log10(difficulty) / Mathf.Log10(10);
        return min + (max - min) * t;
    }

    // S-Curve: Slow start and end, fast middle
    public static float SCurve(float difficulty, float min, float max)
    {
        float t = (difficulty - 1) / 9f;
        float curve = t * t * (3f - 2f * t);
        return min + (max - min) * curve;
    }
}

// Usage:
float enemyHealth = DifficultyRamps.Exponential(difficulty, 50, 500);
float timeLimit = DifficultyRamps.Linear(difficulty, 180, 60);
```

### Pattern 2: Difficulty Presets

```csharp
public class DifficultyPresets
{
    public static GameParameters GetPreset(string difficulty)
    {
        return difficulty switch
        {
            "Easy" => new GameParameters { /* easy settings */ },
            "Normal" => new GameParameters { /* normal settings */ },
            "Hard" => new GameParameters { /* hard settings */ },
            "Nightmare" => new GameParameters { /* nightmare settings */ },
            _ => new GameParameters { /* default */ }
        };
    }

    public static string GetPresetName(float difficulty)
    {
        return difficulty switch
        {
            <= 3 => "Easy",
            <= 5 => "Normal",
            <= 7 => "Hard",
            <= 9 => "Expert",
            _ => "Nightmare"
        };
    }
}
```

### Pattern 3: Adaptive Tutorials

```csharp
public class AdaptiveTutorial
{
    [Inject] private IDynamicUserDifficultyService difficultyService;

    public bool ShouldShowHint(string hintType)
    {
        float difficulty = difficultyService.CurrentDifficulty;

        return hintType switch
        {
            "basic" => difficulty <= 3,
            "movement" => difficulty <= 5,
            "combat" => difficulty <= 7,
            _ => false
        };
    }

    public float GetTutorialSpeed()
    {
        // Slower tutorials for struggling players
        return 2.0f - (difficultyService.CurrentDifficulty - 1) * 0.111f;
    }
}
```

## Common Troubleshooting Solutions

### Issue: Difficulty not changing
**Solution:** Check that you're calling all required methods:
```csharp
difficultyService.RecordWin(); // or RecordLoss()
difficultyService.OnLevelComplete(won, time);
var result = difficultyService.CalculateDifficulty();
difficultyService.ApplyDifficulty(result);
difficultyService.SaveData();
```

### Issue: Difficulty changes too quickly
**Solution:** Configure max change per session:
```csharp
// In DifficultyConfig
config.MaxDifficultyChangePerSession = 1f; // Max ±1 per session
```

### Issue: Parameters not updating
**Solution:** Ensure you're getting fresh parameters:
```csharp
// Wrong - cached parameters
var parameters = cachedParameters;

// Right - fresh calculation
var parameters = adapter.GetAdjustedParameters();
```

### Issue: Data not persisting
**Solution:** Call SaveData() and LoadData():
```csharp
// On game start
difficultyService.LoadData();

// After changes
difficultyService.SaveData();

// On application pause/quit
void OnApplicationPause(bool paused)
{
    if (paused) difficultyService.SaveData();
}
```

## Best Practices

1. **Start with moderate difficulty**: Default to 3-5, not 1
2. **Test edge cases**: Difficulty 1 and 10 should both be playable
3. **Provide manual override**: Let players choose difficulty if they want
4. **Show difficulty changes**: Notify players when difficulty adjusts
5. **Log everything**: Track difficulty changes in analytics
6. **Gradual changes**: Avoid jarring difficulty jumps
7. **Consider fun factor**: Easier isn't always more fun
8. **Test with real players**: Playtesting is crucial

## Analytics Events

Track these events for insights:

```csharp
public void TrackDifficultyChange(float oldDiff, float newDiff, string reason)
{
    Analytics.CustomEvent("difficulty_changed", new Dictionary<string, object>
    {
        {"old_difficulty", oldDiff},
        {"new_difficulty", newDiff},
        {"change_amount", newDiff - oldDiff},
        {"reason", reason}, // "win_streak", "loss_streak", "rage_quit", etc.
        {"session_time", Time.timeSinceLevelLoad},
        {"total_playtime", PlayerPrefs.GetFloat("TotalPlaytime", 0)}
    });
}
```

## Summary

The Dynamic Difficulty system is designed to be flexible and work with any game type. The key steps are:

1. **Create an adapter** that converts difficulty (1-10) to your game parameters
2. **Create a bridge** that connects your game events to the difficulty system
3. **Register everything** in your DI container
4. **Test thoroughly** with different play patterns
5. **Monitor and adjust** based on player data

Remember: The goal is to keep players in the "flow" state - not too easy (boring), not too hard (frustrating), but just right for their skill level.

---

*Last Updated: 2025-09-19*