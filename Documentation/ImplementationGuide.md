# Dynamic User Difficulty - Implementation Guide

## Table of Contents
1. [Overview](#overview)
2. [Stateless Architecture](#stateless-architecture)
3. [Implementation Order](#implementation-order)
4. [Step-by-Step Implementation](#step-by-step-implementation)
5. [File Creation Guide](#file-creation-guide)
6. [Testing Guide](#testing-guide)
7. [Integration Guide](#integration-guide)

## Overview

This guide provides step-by-step instructions for implementing the Dynamic User Difficulty system. The module uses a **STATELESS ARCHITECTURE** where all data comes from external provider interfaces, and modifiers use a parameterless `Calculate()` method.

## Stateless Architecture

### Core Principle
**The module is a PURE CALCULATION ENGINE that ONLY stores the current difficulty value.**

- **What the module stores**: Only `float currentDifficulty` (single value)
- **Where other data comes from**: External game services via provider interfaces
- **How modifiers work**: `Calculate()` method takes NO parameters, gets data from injected providers

### Provider Pattern
```csharp
// Modifiers get data from providers, NOT from method parameters
public override ModifierResult Calculate() // NO PARAMETERS - stateless!
{
    var winStreak = this.winStreakProvider.GetWinStreak();
    var lossStreak = this.winStreakProvider.GetLossStreak();

    // Pure calculation logic using provider data
    if (winStreak >= this.config.WinThreshold)
    {
        var bonus = (winStreak - this.config.WinThreshold) * this.config.StepSize;
        return new ModifierResult { Value = bonus, Reason = "Win streak bonus" };
    }

    return new ModifierResult { Value = 0f, Reason = "No win streak" };
}
```

## Implementation Order

### Phase 1: Foundation (Core Structure)
1. Create folder structure
2. Implement data models
3. Create core interfaces
4. Set up assembly definition

### Phase 2: Provider System (Stateless Data Access)
1. Implement provider interfaces
2. Create base modifier class with provider injection
3. Set up provider implementations
4. Create data persistence layer

### Phase 3: Core Components
1. Implement base modifier class
2. Create modifier implementations (with stateless Calculate())
3. Implement calculators
4. Create main service

### Phase 4: Configuration System
1. Create single ScriptableObject configuration
2. Implement modifier config classes (Serializable)
3. Set up dependency injection

### Phase 5: Integration
1. Integrate with UITemplate
2. Connect to game signals
3. Add analytics tracking

### Phase 6: Polish
1. Create editor tools
2. Add debug utilities
3. Write tests

## Step-by-Step Implementation

### Step 1: Create Folder Structure

```bash
DynamicUserDifficulty/
├── Documentation/
│   ├── ImplementationGuide.md (this file)
│   ├── APIReference.md
│   ├── ModifierGuide.md
│   └── IntegrationGuide.md
├── Runtime/
│   ├── Core/
│   ├── Providers/              # NEW: Provider interfaces for stateless design
│   ├── Modifiers/
│   │   ├── Base/
│   │   └── Implementations/
│   ├── Models/
│   ├── Calculators/
│   ├── Configuration/
│   │   └── ModifierConfigs/
│   └── DI/
├── Editor/
└── Tests/
    ├── Runtime/
    └── Editor/
```

### Step 2: Provider Interfaces Implementation (Stateless Foundation)

#### 2.1 Create IDifficultyDataProvider.cs
```csharp
// Path: Runtime/Providers/IDifficultyDataProvider.cs
namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Base interface for difficulty data storage.
    /// This is the ONLY data the module stores - just the current difficulty value.
    /// </summary>
    public interface IDifficultyDataProvider
    {
        float GetCurrentDifficulty();
        void SetCurrentDifficulty(float newDifficulty);
    }
}
```

#### 2.2 Create IWinStreakProvider.cs
```csharp
// Path: Runtime/Providers/IWinStreakProvider.cs
namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provides win/loss streak data from external game systems.
    /// Used by WinStreakModifier, LossStreakModifier, CompletionRateModifier.
    /// </summary>
    public interface IWinStreakProvider
    {
        int GetWinStreak();
        int GetLossStreak();
        int GetTotalWins();
        int GetTotalLosses();
    }
}
```

#### 2.3 Create ITimeDecayProvider.cs
```csharp
// Path: Runtime/Providers/ITimeDecayProvider.cs
using System;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provides time-based data from external game systems.
    /// Used by TimeDecayModifier.
    /// </summary>
    public interface ITimeDecayProvider
    {
        TimeSpan GetTimeSinceLastPlay();
        DateTime GetLastPlayTime();
        int GetDaysAwayFromGame();
    }
}
```

#### 2.4 Create IRageQuitProvider.cs
```csharp
// Path: Runtime/Providers/IRageQuitProvider.cs
namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provides rage quit and session data from external game systems.
    /// Used by RageQuitModifier, SessionPatternModifier.
    /// </summary>
    public interface IRageQuitProvider
    {
        QuitType GetLastQuitType();
        float GetCurrentSessionDuration();
        int GetRecentRageQuitCount();
        float GetAverageSessionDuration();
    }
}
```

#### 2.5 Create ILevelProgressProvider.cs
```csharp
// Path: Runtime/Providers/ILevelProgressProvider.cs
namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provides level progress data from external game systems.
    /// Used by CompletionRateModifier, LevelProgressModifier.
    /// </summary>
    public interface ILevelProgressProvider
    {
        int GetCurrentLevel();
        float GetAverageCompletionTime();
        int GetAttemptsOnCurrentLevel();
        float GetCompletionRate();
        float GetCurrentLevelDifficulty();
        float GetCurrentLevelTimePercentage();
    }
}
```

### Step 3: Data Models Implementation

#### 3.1 Create QuitType.cs
```csharp
// Path: Runtime/Models/QuitType.cs
namespace TheOneStudio.DynamicUserDifficulty.Models
{
    public enum QuitType
    {
        Normal,
        RageQuit,
        Timeout,
        Crash
    }
}
```

#### 3.2 Create ModifierResult.cs
```csharp
// Path: Runtime/Models/ModifierResult.cs
using System.Collections.Generic;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    public class ModifierResult
    {
        public string ModifierName;
        public float Value;
        public string Reason;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }
}
```

#### 3.3 Create DifficultyResult.cs
```csharp
// Path: Runtime/Models/DifficultyResult.cs
using System;
using System.Collections.Generic;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    public class DifficultyResult
    {
        public float PreviousDifficulty;
        public float NewDifficulty;
        public List<ModifierResult> AppliedModifiers = new List<ModifierResult>();
        public DateTime CalculatedAt;
        public string PrimaryReason;
    }
}
```

### Step 4: Core Interfaces Implementation

#### 4.1 Create IDynamicDifficultyService.cs
```csharp
// Path: Runtime/Core/IDynamicDifficultyService.cs
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    public interface IDynamicDifficultyService
    {
        float CurrentDifficulty { get; }
        void Initialize();
        DifficultyResult CalculateDifficulty();
        void ApplyDifficulty(DifficultyResult result);
        void OnLevelComplete(bool won, float completionTime);
    }
}
```

#### 4.2 Create IDifficultyModifier.cs (STATELESS)
```csharp
// Path: Runtime/Modifiers/Base/IDifficultyModifier.cs
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Interface for stateless difficulty modifiers.
    /// Calculate() method takes NO parameters - gets data from injected providers.
    /// </summary>
    public interface IDifficultyModifier
    {
        string ModifierName { get; }
        int Priority { get; }
        bool IsEnabled { get; set; }

        /// <summary>
        /// STATELESS calculation method - NO parameters!
        /// Gets all data from injected provider interfaces.
        /// </summary>
        ModifierResult Calculate();

        void OnApplied(DifficultyResult result);
    }
}
```

#### 4.3 Create IDifficultyCalculator.cs (STATELESS)
```csharp
// Path: Runtime/Calculators/IDifficultyCalculator.cs
using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;

namespace TheOneStudio.DynamicUserDifficulty.Calculators
{
    /// <summary>
    /// Calculator interface for stateless difficulty calculation.
    /// </summary>
    public interface IDifficultyCalculator
    {
        DifficultyResult Calculate(IEnumerable<IDifficultyModifier> modifiers);
    }
}
```

### Step 5: ⚠️ **CORRECTED Configuration Implementation**

**IMPORTANT: The configuration system uses a SINGLE ScriptableObject approach:**

#### 5.1 Create IModifierConfig.cs
```csharp
// Path: Runtime/Configuration/IModifierConfig.cs
namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    public interface IModifierConfig
    {
        bool IsEnabled { get; }
        int Priority { get; }
        string ModifierType { get; }
    }
}
```

#### 5.2 Create BaseModifierConfig.cs (Base class for all modifier configs)
```csharp
// Path: Runtime/Configuration/BaseModifierConfig.cs
using System;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Base class for all modifier configurations.
    /// Uses [Serializable] not [CreateAssetMenu] - these are NOT separate ScriptableObjects
    /// </summary>
    [Serializable]
    public abstract class BaseModifierConfig : IModifierConfig
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private int priority = 0;

        public bool IsEnabled => this.enabled;
        public int Priority => this.priority;

        /// <summary>
        /// Unique identifier for this modifier type
        /// </summary>
        public abstract string ModifierType { get; }

        /// <summary>
        /// Creates a default configuration instance
        /// </summary>
        public abstract BaseModifierConfig CreateDefault();
    }
}
```

#### 5.3 Create DifficultyConfig.cs (ONLY ScriptableObject)
```csharp
// Path: Runtime/Configuration/DifficultyConfig.cs
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Main configuration ScriptableObject for difficulty settings.
    /// This is the ONLY ScriptableObject - contains ALL 7 modifier configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "DynamicDifficulty/Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Difficulty Range")]
        [SerializeField][Range(1f, 10f)] private float minDifficulty = 1f;
        [SerializeField][Range(1f, 10f)] private float maxDifficulty = 10f;
        [SerializeField][Range(1f, 10f)] private float defaultDifficulty = 3f;
        [SerializeField][Range(0.1f, 5f)] private float maxChangePerSession = 2f;

        [Header("Modifiers - ALL 7 configurations in one place")]
        [SerializeField] private ModifierConfigContainer modifierConfigs = new();

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        // Properties
        public float MinDifficulty => this.minDifficulty;
        public float MaxDifficulty => this.maxDifficulty;
        public float DefaultDifficulty => this.defaultDifficulty;
        public float MaxChangePerSession => this.maxChangePerSession;
        public ModifierConfigContainer ModifierConfigs => this.modifierConfigs;
        public bool EnableDebugLogs => this.enableDebugLogs;

        /// <summary>
        /// Gets a strongly-typed modifier configuration
        /// </summary>
        public T GetModifierConfig<T>(string modifierType) where T : class, IModifierConfig
        {
            return this.modifierConfigs?.GetConfig<T>(modifierType);
        }

        /// <summary>
        /// Creates a default configuration with all 7 modifiers
        /// </summary>
        public static DifficultyConfig CreateDefault()
        {
            var config = CreateInstance<DifficultyConfig>();

            // Set default values
            config.minDifficulty = 1f;
            config.maxDifficulty = 10f;
            config.defaultDifficulty = 3f;
            config.maxChangePerSession = 2f;

            // Initialize all 7 modifier configurations
            config.modifierConfigs = new();
            config.modifierConfigs.InitializeDefaults();

            return config;
        }
    }
}
```

### Step 6: Base Modifier Implementation (STATELESS)

#### 6.1 Create BaseDifficultyModifier.cs
```csharp
// Path: Runtime/Modifiers/Base/BaseDifficultyModifier.cs
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Type-safe base class for STATELESS difficulty modifiers with strongly-typed configuration.
    /// Modifiers get data from injected providers, NOT from method parameters.
    /// </summary>
    public abstract class BaseDifficultyModifier<TConfig> : IDifficultyModifier
        where TConfig : class, IModifierConfig
    {
        protected readonly TConfig config;

        public abstract string ModifierName { get; }
        public virtual int Priority => this.config?.Priority ?? 0;
        public bool IsEnabled { get; set; }

        protected BaseDifficultyModifier(TConfig config)
        {
            this.config = config;
            this.IsEnabled = config?.IsEnabled ?? true;
        }

        /// <summary>
        /// STATELESS calculation method - NO parameters!
        /// Gets all data from injected provider interfaces.
        /// </summary>
        public abstract ModifierResult Calculate();

        public virtual void OnApplied(DifficultyResult result)
        {
            // Optional hook for post-application logic
        }

        /// <summary>
        /// Helper method to create "no change" results
        /// </summary>
        protected ModifierResult NoChange(string reason = "No adjustment needed")
        {
            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = 0f,
                Reason = reason
            };
        }
    }
}
```

### Step 7: Modifier Implementations (STATELESS Examples)

#### 7.1 Create WinStreakModifier.cs (STATELESS)
```csharp
// Path: Runtime/Modifiers/Implementations/WinStreakModifier.cs
using System;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// STATELESS Win Streak modifier.
    /// Gets data from IWinStreakProvider, calculates adjustment using provider data.
    /// </summary>
    public class WinStreakModifier : BaseDifficultyModifier<WinStreakConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => "WinStreak";

        public WinStreakModifier(WinStreakConfig config, IWinStreakProvider provider)
            : base(config)
        {
            this.winStreakProvider = provider;
        }

        /// <summary>
        /// STATELESS calculation - NO parameters!
        /// Gets win streak data from provider interface.
        /// </summary>
        public override ModifierResult Calculate()
        {
            var winStreak = this.winStreakProvider.GetWinStreak();

            if (winStreak < this.config.WinThreshold)
                return NoChange($"Win streak {winStreak} below threshold {this.config.WinThreshold}");

            var bonus = (winStreak - this.config.WinThreshold) * this.config.StepSize;
            bonus = Math.Min(bonus, this.config.MaxBonus);

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = bonus,
                Reason = $"Win streak bonus ({winStreak} consecutive wins)",
                Metadata = { ["streak"] = winStreak, ["threshold"] = this.config.WinThreshold }
            };
        }
    }
}
```

#### 7.2 Create LossStreakModifier.cs (STATELESS)
```csharp
// Path: Runtime/Modifiers/Implementations/LossStreakModifier.cs
using System;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// STATELESS Loss Streak modifier.
    /// Gets data from IWinStreakProvider, calculates adjustment using provider data.
    /// </summary>
    public class LossStreakModifier : BaseDifficultyModifier<LossStreakConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => "LossStreak";

        public LossStreakModifier(LossStreakConfig config, IWinStreakProvider provider)
            : base(config)
        {
            this.winStreakProvider = provider;
        }

        /// <summary>
        /// STATELESS calculation - NO parameters!
        /// Gets loss streak data from provider interface.
        /// </summary>
        public override ModifierResult Calculate()
        {
            var lossStreak = this.winStreakProvider.GetLossStreak();

            if (lossStreak < this.config.LossThreshold)
                return NoChange($"Loss streak {lossStreak} below threshold {this.config.LossThreshold}");

            var reduction = (lossStreak - this.config.LossThreshold) * this.config.StepSize;
            reduction = Math.Min(reduction, this.config.MaxReduction);

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = -reduction, // Negative for difficulty reduction
                Reason = $"Loss streak reduction ({lossStreak} consecutive losses)",
                Metadata = { ["streak"] = lossStreak, ["threshold"] = this.config.LossThreshold }
            };
        }
    }
}
```

#### 7.3 Create CompletionRateModifier.cs (STATELESS with Multiple Providers)
```csharp
// Path: Runtime/Modifiers/Implementations/CompletionRateModifier.cs
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// STATELESS Completion Rate modifier.
    /// Uses MULTIPLE providers: IWinStreakProvider and ILevelProgressProvider.
    /// </summary>
    public class CompletionRateModifier : BaseDifficultyModifier<CompletionRateConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;
        private readonly ILevelProgressProvider levelProgressProvider;

        public override string ModifierName => "CompletionRate";

        public CompletionRateModifier(
            CompletionRateConfig config,
            IWinStreakProvider winStreakProvider,
            ILevelProgressProvider levelProgressProvider)
            : base(config)
        {
            this.winStreakProvider = winStreakProvider;
            this.levelProgressProvider = levelProgressProvider;
        }

        /// <summary>
        /// STATELESS calculation - NO parameters!
        /// Gets completion rate from multiple providers.
        /// </summary>
        public override ModifierResult Calculate()
        {
            // Get data from multiple providers
            var totalWins = this.winStreakProvider.GetTotalWins();
            var totalLosses = this.winStreakProvider.GetTotalLosses();
            var completionRate = this.levelProgressProvider.GetCompletionRate();

            var totalGames = totalWins + totalLosses;
            if (totalGames == 0)
                return NoChange("No games played yet");

            var actualRate = totalGames > 0 ? (float)totalWins / totalGames : completionRate;

            if (actualRate < this.config.LowCompletionThreshold)
            {
                return new ModifierResult
                {
                    ModifierName = ModifierName,
                    Value = this.config.LowCompletionAdjustment,
                    Reason = $"Low completion rate ({actualRate:P0})",
                    Metadata = { ["completion_rate"] = actualRate, ["total_games"] = totalGames }
                };
            }

            if (actualRate > this.config.HighCompletionThreshold)
            {
                return new ModifierResult
                {
                    ModifierName = ModifierName,
                    Value = this.config.HighCompletionAdjustment,
                    Reason = $"High completion rate ({actualRate:P0})",
                    Metadata = { ["completion_rate"] = actualRate, ["total_games"] = totalGames }
                };
            }

            return NoChange($"Completion rate in normal range ({actualRate:P0})");
        }
    }
}
```

### Step 8: Main Service Implementation (STATELESS)

#### 8.1 Create DynamicDifficultyService.cs (STATELESS)
```csharp
// Path: Runtime/Core/DynamicDifficultyService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// STATELESS Dynamic Difficulty Service.
    /// Only stores current difficulty - all other data comes from providers.
    /// </summary>
    public class DynamicDifficultyService : IDynamicDifficultyService
    {
        private readonly IDifficultyCalculator calculator;
        private readonly IDifficultyDataProvider dataProvider;
        private readonly DifficultyConfig config;
        private readonly List<IDifficultyModifier> modifiers;

        public float CurrentDifficulty => this.dataProvider.GetCurrentDifficulty();

        public DynamicDifficultyService(
            IDifficultyCalculator calculator,
            IDifficultyDataProvider dataProvider,
            DifficultyConfig config,
            IEnumerable<IDifficultyModifier> modifiers)
        {
            this.calculator = calculator;
            this.dataProvider = dataProvider;
            this.config = config;
            this.modifiers = modifiers?.ToList() ?? new List<IDifficultyModifier>();
        }

        public void Initialize()
        {
            // Initialize with default difficulty if not set
            var currentDifficulty = this.dataProvider.GetCurrentDifficulty();
            if (currentDifficulty <= 0)
            {
                this.dataProvider.SetCurrentDifficulty(this.config.DefaultDifficulty);
            }
        }

        /// <summary>
        /// STATELESS calculation - gets all data from providers via modifiers.
        /// </summary>
        public DifficultyResult CalculateDifficulty()
        {
            var enabledModifiers = this.modifiers.Where(m => m.IsEnabled).ToList();
            return this.calculator.Calculate(enabledModifiers);
        }

        public void ApplyDifficulty(DifficultyResult result)
        {
            if (result == null) return;

            // Clamp to valid range
            var clampedDifficulty = Math.Max(this.config.MinDifficulty,
                Math.Min(this.config.MaxDifficulty, result.NewDifficulty));

            // Store ONLY the current difficulty value
            this.dataProvider.SetCurrentDifficulty(clampedDifficulty);

            // Notify modifiers
            foreach (var modifier in this.modifiers.Where(m => m.IsEnabled))
            {
                modifier.OnApplied(result);
            }
        }

        public void OnLevelComplete(bool won, float completionTime)
        {
            // Calculate and apply new difficulty automatically
            var result = CalculateDifficulty();
            ApplyDifficulty(result);
        }
    }
}
```

### Step 9: DI Registration (STATELESS)

#### 9.1 Create DynamicDifficultyModule.cs
```csharp
// Path: Runtime/DI/DynamicDifficultyModule.cs
using VContainer;
using VContainer.Unity;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.DI
{
    /// <summary>
    /// VContainer module for STATELESS Dynamic User Difficulty system.
    /// Registers all services with provider-based dependencies.
    /// </summary>
    public class DynamicDifficultyModule : IInstaller
    {
        private readonly DifficultyConfig config;

        public DynamicDifficultyModule(DifficultyConfig config)
        {
            this.config = config;
        }

        public void Install(IContainerBuilder builder)
        {
            // Register configuration
            builder.RegisterInstance(this.config);

            // Register providers (must be implemented by game)
            // These are interfaces - game must provide implementations
            builder.Register<IDifficultyDataProvider, PlayerPrefsDifficultyDataProvider>(Lifetime.Singleton);

            // Register calculators
            builder.Register<IDifficultyCalculator, DifficultyCalculator>(Lifetime.Singleton);

            // Register main service
            builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(Lifetime.Singleton);

            // Register all 7 modifiers with their configurations
            RegisterModifiers(builder);
        }

        private void RegisterModifiers(IContainerBuilder builder)
        {
            var configContainer = this.config.ModifierConfigs;

            // WinStreakModifier
            var winStreakConfig = configContainer.GetConfig<WinStreakConfig>("WinStreak");
            if (winStreakConfig != null)
            {
                builder.Register<WinStreakModifier>(Lifetime.Singleton)
                       .WithParameter(winStreakConfig)
                       .As<IDifficultyModifier>();
            }

            // LossStreakModifier
            var lossStreakConfig = configContainer.GetConfig<LossStreakConfig>("LossStreak");
            if (lossStreakConfig != null)
            {
                builder.Register<LossStreakModifier>(Lifetime.Singleton)
                       .WithParameter(lossStreakConfig)
                       .As<IDifficultyModifier>();
            }

            // CompletionRateModifier (uses multiple providers)
            var completionRateConfig = configContainer.GetConfig<CompletionRateConfig>("CompletionRate");
            if (completionRateConfig != null)
            {
                builder.Register<CompletionRateModifier>(Lifetime.Singleton)
                       .WithParameter(completionRateConfig)
                       .As<IDifficultyModifier>();
            }

            // Register other modifiers (TimeDecay, RageQuit, LevelProgress, SessionPattern)...
        }
    }
}
```

## File Creation Guide

### Complete File List (in order of creation)

1. **Provider Interfaces (5 files) - STATELESS FOUNDATION**
   - IDifficultyDataProvider.cs
   - IWinStreakProvider.cs
   - ITimeDecayProvider.cs
   - IRageQuitProvider.cs
   - ILevelProgressProvider.cs

2. **Models (3 files)**
   - QuitType.cs
   - ModifierResult.cs
   - DifficultyResult.cs

3. **Interfaces (3 files) - STATELESS**
   - IDynamicDifficultyService.cs
   - IDifficultyModifier.cs (with parameterless Calculate())
   - IDifficultyCalculator.cs (takes modifiers only)

4. **Configuration (10 files) - ⚠️ CORRECTED**
   - IModifierConfig.cs (interface)
   - BaseModifierConfig.cs (base class for all configs)
   - ModifierConfigContainer.cs (container with SerializeReference)
   - DifficultyConfig.cs (ONLY ScriptableObject)
   - ModifierConfigs/WinStreakConfig.cs ([Serializable])
   - ModifierConfigs/LossStreakConfig.cs ([Serializable])
   - ModifierConfigs/TimeDecayConfig.cs ([Serializable])
   - ModifierConfigs/RageQuitConfig.cs ([Serializable])
   - ModifierConfigs/CompletionRateConfig.cs ([Serializable])
   - ModifierConfigs/LevelProgressConfig.cs ([Serializable])
   - ModifierConfigs/SessionPatternConfig.cs ([Serializable])

5. **Base Classes (1 file) - STATELESS**
   - BaseDifficultyModifier.cs (with parameterless Calculate())

6. **Modifiers (7 files) - ALL STATELESS**
   - WinStreakModifier.cs (uses IWinStreakProvider)
   - LossStreakModifier.cs (uses IWinStreakProvider)
   - TimeDecayModifier.cs (uses ITimeDecayProvider)
   - RageQuitModifier.cs (uses IRageQuitProvider)
   - CompletionRateModifier.cs (uses IWinStreakProvider + ILevelProgressProvider)
   - LevelProgressModifier.cs (uses ILevelProgressProvider)
   - SessionPatternModifier.cs (uses IRageQuitProvider)

7. **Calculators (2 files) - STATELESS**
   - DifficultyCalculator.cs (takes modifiers only)
   - ModifierAggregator.cs

8. **Service (2 files) - STATELESS**
   - DynamicDifficultyService.cs (stores only current difficulty)
   - DifficultyConstants.cs

9. **DI (1 file)**
   - DynamicDifficultyModule.cs

10. **Provider Implementations (1 file)**
    - PlayerPrefsDifficultyDataProvider.cs (simple storage implementation)

## Testing Guide

### Unit Test Template (STATELESS)
```csharp
using NUnit.Framework;
using Moq;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;

[TestFixture]
public class StatelessModifierTests
{
    [Test]
    public void WinStreak_AboveThreshold_IncreasesDifficulty()
    {
        // Arrange
        var config = CreateTestConfig();
        var mockProvider = new Mock<IWinStreakProvider>();
        mockProvider.Setup(p => p.GetWinStreak()).Returns(5);

        var modifier = new WinStreakModifier(config, mockProvider.Object);

        // Act - NO PARAMETERS in Calculate()!
        var result = modifier.Calculate();

        // Assert
        Assert.Greater(result.Value, 0);
        Assert.Contains("Win streak", result.Reason);
        mockProvider.Verify(p => p.GetWinStreak(), Times.Once);
    }

    [Test]
    public void CompletionRate_MultipleProviders_WorksCorrectly()
    {
        // Arrange
        var config = CreateCompletionRateConfig();
        var mockWinStreakProvider = new Mock<IWinStreakProvider>();
        var mockLevelProgressProvider = new Mock<ILevelProgressProvider>();

        mockWinStreakProvider.Setup(p => p.GetTotalWins()).Returns(7);
        mockWinStreakProvider.Setup(p => p.GetTotalLosses()).Returns(3);
        mockLevelProgressProvider.Setup(p => p.GetCompletionRate()).Returns(0.7f);

        var modifier = new CompletionRateModifier(config,
            mockWinStreakProvider.Object,
            mockLevelProgressProvider.Object);

        // Act - STATELESS calculation
        var result = modifier.Calculate();

        // Assert
        Assert.NotNull(result);
        mockWinStreakProvider.Verify(p => p.GetTotalWins(), Times.Once);
        mockWinStreakProvider.Verify(p => p.GetTotalLosses(), Times.Once);
        // Level progress provider might or might not be called depending on implementation
    }
}
```

## Integration Guide

### Step 1: Provider Implementation
```csharp
// Implement providers for your game's data sources
public class GameWinStreakProvider : IWinStreakProvider
{
    private readonly UITemplateLevelDataController levelController;

    public GameWinStreakProvider(UITemplateLevelDataController levelController)
    {
        this.levelController = levelController;
    }

    public int GetWinStreak() => this.levelController.GetWinStreak();
    public int GetLossStreak() => this.levelController.GetLossStreak();
    public int GetTotalWins() => this.levelController.GetTotalWins();
    public int GetTotalLosses() => this.levelController.GetTotalLosses();
}
```

### Step 2: Add to UITemplateVContainer
```csharp
#if THEONE_DYNAMIC_DIFFICULTY
// Load the single configuration asset
var difficultyConfig = Resources.Load<DifficultyConfig>("GameConfigs/DifficultyConfig");

// Register your provider implementations
builder.Register<IWinStreakProvider, GameWinStreakProvider>(Lifetime.Singleton);
builder.Register<ITimeDecayProvider, GameTimeDecayProvider>(Lifetime.Singleton);
builder.Register<IRageQuitProvider, GameRageQuitProvider>(Lifetime.Singleton);
builder.Register<ILevelProgressProvider, GameLevelProgressProvider>(Lifetime.Singleton);

// Register the module (will use your providers automatically)
builder.RegisterModule(new DynamicDifficultyModule(difficultyConfig));
#endif
```

### Step 3: Create ScriptableObject (ONE asset only)
1. Right-click in Project window
2. Create → DynamicDifficulty → Config
3. Configure parameters (ALL 7 modifiers in one place)
4. Save in Resources/GameConfigs/

### Step 4: Usage in Game Code
```csharp
public class GameController
{
    private readonly IDynamicDifficultyService difficultyService;

    public void StartLevel()
    {
        // STATELESS calculation - modifiers get data from providers automatically
        var result = difficultyService.CalculateDifficulty();
        difficultyService.ApplyDifficulty(result);

        // Use result.NewDifficulty to configure level
        ConfigureLevel(result.NewDifficulty);
    }

    public void OnLevelComplete(bool won, float time)
    {
        // Service handles everything automatically via providers
        difficultyService.OnLevelComplete(won, time);
    }
}
```

## Troubleshooting

### Common Issues

1. **Assembly Reference Errors**
   - Ensure all assemblies are referenced in .asmdef
   - Check GUID references are correct

2. **Provider Not Found Errors**
   - Verify all provider interfaces are implemented
   - Check DI registration order
   - Ensure providers are registered before module

3. **Configuration Issues** ⚠️ **UPDATED**
   - Only create ONE DifficultyConfig asset
   - All 7 modifiers are configured within this single asset
   - Config classes use [Serializable], not [CreateAssetMenu]

4. **Stateless Calculation Errors**
   - Verify modifiers use Calculate() with NO parameters
   - Check provider injection in modifier constructors
   - Ensure providers return valid data

5. **Difficulty Not Changing**
   - Verify providers return correct data
   - Check threshold values in modifier configurations
   - Enable debug logs in DifficultyConfig
   - Verify Calculate() methods are being called

6. **Provider Interface Errors**
   - Implement ALL required provider methods
   - Use dependency injection to inject providers into modifiers
   - Check provider registration in DI container

---

*Last Updated: 2025-01-26*
*Architecture: STATELESS with Provider Pattern*
*Configuration Structure: Single ScriptableObject with Embedded [Serializable] Configs*
*Modifier Method Signature: Calculate() - NO PARAMETERS*