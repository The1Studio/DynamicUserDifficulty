# Dynamic User Difficulty - Implementation Guide

## Table of Contents
1. [Overview](#overview)
2. [Implementation Order](#implementation-order)
3. [Step-by-Step Implementation](#step-by-step-implementation)
4. [File Creation Guide](#file-creation-guide)
5. [Testing Guide](#testing-guide)
6. [Integration Guide](#integration-guide)

## Overview

This guide provides step-by-step instructions for implementing the Dynamic User Difficulty system. Follow the implementation order to ensure all dependencies are satisfied.

## Implementation Order

### Phase 1: Foundation (Core Structure)
1. Create folder structure
2. Implement data models
3. Create core interfaces
4. Set up assembly definition

### Phase 2: Core Components
1. Implement base modifier class
2. Create modifier implementations
3. Implement calculators
4. Create data providers

### Phase 3: Service Layer
1. Implement main service
2. Create configuration system
3. Set up dependency injection

### Phase 4: Integration
1. Integrate with UITemplate
2. Connect to game signals
3. Add analytics tracking

### Phase 5: Polish
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
│   ├── Modifiers/
│   │   ├── Base/
│   │   └── Implementations/
│   ├── Models/
│   ├── Calculators/
│   ├── Providers/
│   ├── Configuration/
│   │   └── ModifierConfigs/
│   └── DI/
├── Editor/
└── Tests/
    ├── Runtime/
    └── Editor/
```

### Step 2: Data Models Implementation

#### 2.1 Create SessionEndType.cs
```csharp
// Path: Runtime/Models/SessionEndType.cs
namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models
{
    public enum SessionEndType
    {
        CompletedWin,
        CompletedLoss,
        QuitDuringPlay,
        QuitAfterWin,
        QuitAfterLoss,
        Timeout
    }
}
```

#### 2.2 Create SessionInfo.cs
```csharp
// Path: Runtime/Models/SessionInfo.cs
using System;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models
{
    [Serializable]
    public class SessionInfo
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public SessionEndType EndType;
        public int LevelId;
        public float PlayDuration;
        public bool Won;
    }
}
```

#### 2.3 Create PlayerSessionData.cs
```csharp
// Path: Runtime/Models/PlayerSessionData.cs
using System;
using System.Collections.Generic;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models
{
    [Serializable]
    public class PlayerSessionData
    {
        public float CurrentDifficulty = 3f;
        public int WinStreak;
        public int LossStreak;
        public DateTime LastPlayTime = DateTime.Now;
        public SessionInfo LastSession;
        public Queue<SessionInfo> RecentSessions = new Queue<SessionInfo>(10);
    }
}
```

#### 2.4 Create ModifierResult.cs
```csharp
// Path: Runtime/Models/ModifierResult.cs
using System.Collections.Generic;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models
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

#### 2.5 Create DifficultyResult.cs
```csharp
// Path: Runtime/Models/DifficultyResult.cs
using System;
using System.Collections.Generic;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models
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

### Step 3: Core Interfaces Implementation

#### 3.1 Create IDynamicDifficultyService.cs
```csharp
// Path: Runtime/Core/IDynamicDifficultyService.cs
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Core
{
    public interface IDynamicDifficultyService
    {
        float CurrentDifficulty { get; }
        void Initialize();
        DifficultyResult CalculateDifficulty();
        void ApplyDifficulty(DifficultyResult result);
        void RegisterModifier(IDifficultyModifier modifier);
        void UnregisterModifier(IDifficultyModifier modifier);
        void OnSessionStart();
        void OnLevelStart(int levelId);
        void OnLevelComplete(bool won, float completionTime);
        void OnSessionEnd(SessionEndType endType);
    }
}
```

#### 3.2 Create IDifficultyModifier.cs
```csharp
// Path: Runtime/Modifiers/Base/IDifficultyModifier.cs
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public interface IDifficultyModifier
    {
        string ModifierName { get; }
        int Priority { get; }
        bool IsEnabled { get; set; }
        ModifierResult Calculate(PlayerSessionData sessionData);
        void OnApplied(DifficultyResult result);
    }
}
```

#### 3.3 Create ISessionDataProvider.cs
```csharp
// Path: Runtime/Providers/ISessionDataProvider.cs
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Providers
{
    public interface ISessionDataProvider
    {
        PlayerSessionData GetCurrentSession();
        void SaveSession(PlayerSessionData data);
        void UpdateWinStreak(int streak);
        void UpdateLossStreak(int streak);
        void RecordSessionEnd(SessionEndType endType);
    }
}
```

#### 3.4 Create IDifficultyCalculator.cs
```csharp
// Path: Runtime/Calculators/IDifficultyCalculator.cs
using System.Collections.Generic;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Calculators
{
    public interface IDifficultyCalculator
    {
        DifficultyResult Calculate(
            PlayerSessionData sessionData,
            IEnumerable<IDifficultyModifier> modifiers);
    }
}
```

### Step 4: ⚠️ **CORRECTED Configuration Implementation**

**IMPORTANT: The configuration system uses a SINGLE ScriptableObject approach:**

#### 4.1 Create BaseModifierConfig.cs (Base class for all modifier configs)
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

#### 4.2 Create ModifierConfigContainer.cs
```csharp
// Path: Runtime/Configuration/ModifierConfigContainer.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Container for modifier configurations with polymorphic serialization support.
    /// Uses SerializeReference to support different config types within a single ScriptableObject.
    /// </summary>
    [Serializable]
    public class ModifierConfigContainer : IEnumerable<IModifierConfig>
    {
        [SerializeReference]
        [Tooltip("List of modifier configurations. Use + to add new configs.")]
        private List<BaseModifierConfig> configs = new();

        /// <summary>
        /// Gets a strongly-typed configuration for a specific modifier type
        /// </summary>
        public T GetConfig<T>(string modifierType) where T : class, IModifierConfig
        {
            var config = this.configs?.FirstOrDefault(c => c?.ModifierType == modifierType);
            return config as T;
        }

        /// <summary>
        /// Adds or updates a modifier configuration
        /// </summary>
        public void SetConfig(IModifierConfig config)
        {
            if (config == null || !(config is BaseModifierConfig baseConfig)) return;

            // Remove existing config of the same type
            this.configs?.RemoveAll(c => c?.ModifierType == config.ModifierType);

            // Add new config
            if (this.configs == null) this.configs = new();
            this.configs.Add(baseConfig);
        }

        /// <summary>
        /// Initializes with default configurations for all 7 modifiers
        /// </summary>
        public void InitializeDefaults()
        {
            this.configs = new()
            {
                (WinStreakConfig)new WinStreakConfig().CreateDefault(),
                (LossStreakConfig)new LossStreakConfig().CreateDefault(),
                (TimeDecayConfig)new TimeDecayConfig().CreateDefault(),
                (RageQuitConfig)new RageQuitConfig().CreateDefault(),
                (CompletionRateConfig)new CompletionRateConfig().CreateDefault(),
                (LevelProgressConfig)new LevelProgressConfig().CreateDefault(),
                (SessionPatternConfig)new SessionPatternConfig().CreateDefault()
            };
        }
    }
}
```

#### 4.3 Create DifficultyConfig.cs (ONLY ScriptableObject)
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

        private void OnValidate()
        {
            // Ensure min <= default <= max
            if (this.defaultDifficulty < this.minDifficulty) this.defaultDifficulty = this.minDifficulty;
            if (this.defaultDifficulty > this.maxDifficulty) this.defaultDifficulty = this.maxDifficulty;
            if (this.minDifficulty > this.maxDifficulty) this.minDifficulty = this.maxDifficulty;
        }
    }
}
```

#### 4.4 Create Individual Modifier Config Classes ([Serializable], NOT [CreateAssetMenu])

**Example: WinStreakConfig.cs**
```csharp
// Path: Runtime/Configuration/ModifierConfigs/WinStreakConfig.cs
using System;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Win Streak modifier.
    /// [Serializable] class embedded in DifficultyConfig, NOT a separate ScriptableObject.
    /// </summary>
    [Serializable]
    public class WinStreakConfig : BaseModifierConfig
    {
        [SerializeField] private float winThreshold = 3f;
        [SerializeField] private float stepSize = 0.5f;
        [SerializeField] private float maxBonus = 2f;

        public float WinThreshold => this.winThreshold;
        public float StepSize => this.stepSize;
        public float MaxBonus => this.maxBonus;

        public override string ModifierType => "WinStreak";

        public override BaseModifierConfig CreateDefault()
        {
            var config = new WinStreakConfig();
            config.winThreshold = 3f;
            config.stepSize = 0.5f;
            config.maxBonus = 2f;
            return config;
        }
    }
}
```

**Repeat this pattern for all 7 modifier configs:**
- `WinStreakConfig.cs` ✅
- `LossStreakConfig.cs`
- `TimeDecayConfig.cs`
- `RageQuitConfig.cs`
- `CompletionRateConfig.cs`
- `LevelProgressConfig.cs`
- `SessionPatternConfig.cs`

### Step 5: Base Modifier Implementation

#### 5.1 Create BaseDifficultyModifier.cs
```csharp
// Path: Runtime/Modifiers/Base/BaseDifficultyModifier.cs
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Type-safe base class for difficulty modifiers with strongly-typed configuration
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

        public abstract ModifierResult Calculate(PlayerSessionData sessionData);

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

### Step 6: Modifier Implementations (Using Type-Safe Config)

#### 6.1 Create WinStreakModifier.cs
```csharp
// Path: Runtime/Modifiers/Implementations/WinStreakModifier.cs
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    public class WinStreakModifier : BaseDifficultyModifier<WinStreakConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => "WinStreak";

        public WinStreakModifier(WinStreakConfig config, IWinStreakProvider provider)
            : base(config)
        {
            this.winStreakProvider = provider;
        }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
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

### Step 7: Required Unity Assemblies

Add references to these Unity assemblies in your .asmdef:
- Unity.TextMeshPro
- Unity.Addressables
- UnityEngine.UI

### Step 8: ⚠️ **CORRECTED Editor Validation System**

#### 8.1 DifficultyConfigValidator (Updated for Single ScriptableObject)
```csharp
// Path: Editor/DifficultyConfigValidator.cs
[InitializeOnLoad]
public static class DifficultyConfigValidator
{
    static DifficultyConfigValidator()
    {
        // Check for single DifficultyConfig asset on load
        ValidateConfiguration();
    }

    private static void ValidateConfiguration()
    {
        var configs = Resources.LoadAll<DifficultyConfig>("");

        if (configs.Length == 0)
        {
            ShowConfigurationSetupDialog();
        }
        else if (configs.Length > 1)
        {
            Debug.LogWarning($"Multiple DifficultyConfig assets found ({configs.Length}). " +
                           "Only ONE is needed. Consider removing duplicates.");
        }
    }

    private static void ShowConfigurationSetupDialog()
    {
        var create = EditorUtility.DisplayDialog(
            "Dynamic Difficulty Configuration",
            "No DifficultyConfig found. This single asset contains ALL 7 modifier configurations.\n\n" +
            "Would you like to create a default configuration?",
            "Create Config",
            "Skip");

        if (create)
        {
            CreateDefaultConfiguration();
        }
    }

    [MenuItem("Tools/Dynamic Difficulty/Create Single Config")]
    private static void CreateDefaultConfiguration()
    {
        // Create the single DifficultyConfig asset with all 7 modifiers
        var config = DifficultyConfig.CreateDefault();

        var path = "Assets/Resources/GameConfigs/DifficultyConfig.asset";
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created DifficultyConfig at {path} with all 7 modifier configurations embedded.");
        Selection.activeObject = config;
    }
}
```

### Step 9: Integration Points

#### Signal Subscriptions Required
```csharp
// Subscribe to these signals from Screw3D
- WonSignal
- LostSignal
- GamePausedSignal
- GameResumedSignal
- LevelStartSignal
```

#### Controller Dependencies
```csharp
// Inject these controllers
- UITemplateLevelDataController
- UITemplateGameSessionDataController
- UITemplateInventoryDataController
- UITemplateAnalyticService
```

## File Creation Guide

### Complete File List (in order of creation)

1. **Models (5 files)**
   - SessionEndType.cs
   - SessionInfo.cs
   - PlayerSessionData.cs
   - ModifierResult.cs
   - DifficultyResult.cs

2. **Interfaces (4 files)**
   - IDynamicDifficultyService.cs
   - IDifficultyModifier.cs
   - ISessionDataProvider.cs
   - IDifficultyCalculator.cs

3. **Configuration (9 files) - ⚠️ CORRECTED**
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

4. **Base Classes (1 file)**
   - BaseDifficultyModifier.cs

5. **Modifiers (7 files)**
   - WinStreakModifier.cs
   - LossStreakModifier.cs
   - TimeDecayModifier.cs
   - RageQuitModifier.cs
   - CompletionRateModifier.cs
   - LevelProgressModifier.cs
   - SessionPatternModifier.cs

6. **Calculators (2 files)**
   - DifficultyCalculator.cs
   - ModifierAggregator.cs

7. **Providers (5 files)**
   - IDifficultyDataProvider.cs
   - IWinStreakProvider.cs
   - ITimeDecayProvider.cs
   - IRageQuitProvider.cs
   - ILevelProgressProvider.cs

8. **Service (2 files)**
   - DynamicDifficultyService.cs
   - DifficultyConstants.cs

9. **DI (1 file)**
   - DynamicDifficultyModule.cs

10. **Editor (3 files)**
    - DifficultyConfigValidator.cs
    - DifficultyDebugWindow.cs
    - ModifierConfigEditor.cs

## Testing Guide

### Unit Test Template
```csharp
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;

[TestFixture]
public class ModifierTests
{
    [Test]
    public void WinStreak_AboveThreshold_IncreasesDifficulty()
    {
        // Arrange
        var config = CreateTestConfig();
        var mockProvider = new Mock<IWinStreakProvider>();
        mockProvider.Setup(p => p.GetWinStreak()).Returns(5);
        var modifier = new WinStreakModifier(config, mockProvider.Object);
        var sessionData = new PlayerSessionData();

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.Greater(result.Value, 0);
        Assert.Contains("Win streak", result.Reason);
        mockProvider.Verify(p => p.GetWinStreak(), Times.Once);
    }
}
```

## Integration Guide

### Step 1: Add to UITemplateVContainer
```csharp
#if THEONE_DYNAMIC_DIFFICULTY
var difficultyConfig = Resources.Load<DifficultyConfig>("GameConfigs/DifficultyConfig");
builder.RegisterModule(new DynamicDifficultyModule(difficultyConfig));
#endif
```

### Step 2: Create ScriptableObject (ONE asset only)
1. Right-click in Project window
2. Create → DynamicDifficulty → Config
3. Configure parameters (ALL 7 modifiers in one place)
4. Save in Resources/GameConfigs/

### Step 3: Add Compiler Flag
1. Edit → Project Settings → Player
2. Add to Scripting Define Symbols: `THEONE_DYNAMIC_DIFFICULTY`

### Step 4: Subscribe to Signals
```csharp
public class DifficultyGameplayBridge : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IDynamicDifficultyService difficultyService;

    public void Initialize()
    {
        signalBus.Subscribe<WonSignal>(OnWon);
        signalBus.Subscribe<LostSignal>(OnLost);
    }

    private void OnWon(WonSignal signal)
    {
        difficultyService.OnLevelComplete(true, signal.CompletionTime);
    }
}
```

## Troubleshooting

### Common Issues

1. **Assembly Reference Errors**
   - Ensure all assemblies are referenced in .asmdef
   - Check GUID references are correct

2. **DI Registration Errors**
   - Verify all interfaces are registered
   - Check singleton vs transient lifetimes

3. **Configuration Issues** ⚠️ **UPDATED**
   - Only create ONE DifficultyConfig asset
   - All 7 modifiers are configured within this single asset
   - Config classes use [Serializable], not [CreateAssetMenu]

4. **Difficulty Not Changing**
   - Verify modifiers are enabled in the single config
   - Check threshold values in modifier configurations
   - Enable debug logs in DifficultyConfig

---

*Last Updated: 2025-01-22*
*Configuration Structure Corrected - Single ScriptableObject Approach*