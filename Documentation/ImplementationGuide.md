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

### Step 4: Configuration Implementation

#### 4.1 Create ModifierConfig.cs
```csharp
// Path: Runtime/Configuration/ModifierConfig.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration
{
    [Serializable]
    public class ModifierConfig
    {
        public string ModifierType;
        public bool Enabled = true;
        public int Priority = 0;

        [Header("Response Curve")]
        public AnimationCurve ResponseCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Parameters")]
        public List<ModifierParameter> Parameters = new List<ModifierParameter>();

        public float GetParameter(string key, float defaultValue = 0f)
        {
            var param = Parameters.Find(p => p.Key == key);
            return param != null ? param.Value : defaultValue;
        }
    }

    [Serializable]
    public class ModifierParameter
    {
        public string Key;
        public float Value;
    }
}
```

#### 4.2 Create DifficultyConfig.cs
```csharp
// Path: Runtime/Configuration/DifficultyConfig.cs
using System.Collections.Generic;
using UnityEngine;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration
{
    [CreateAssetMenu(fileName = "DifficultyConfig",
                     menuName = "DynamicDifficulty/Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Difficulty Range")]
        public float MinDifficulty = 1f;
        public float MaxDifficulty = 10f;
        public float DefaultDifficulty = 3f;
        public float MaxChangePerSession = 2f;

        [Header("Modifiers")]
        public List<ModifierConfig> ModifierConfigs = new List<ModifierConfig>();

        [Header("Debug")]
        public bool EnableDebugLogs = false;
    }
}
```

### Step 5: Base Modifier Implementation

#### 5.1 Create BaseDifficultyModifier.cs
```csharp
// Path: Runtime/Modifiers/Base/BaseDifficultyModifier.cs
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public abstract class BaseDifficultyModifier : IDifficultyModifier
    {
        protected readonly ModifierConfig config;

        public abstract string ModifierName { get; }
        public virtual int Priority => config?.Priority ?? 0;
        public bool IsEnabled { get; set; }

        protected BaseDifficultyModifier(ModifierConfig config)
        {
            this.config = config;
            this.IsEnabled = config?.Enabled ?? true;
        }

        public abstract ModifierResult Calculate(PlayerSessionData sessionData);

        public virtual void OnApplied(DifficultyResult result)
        {
            // Optional hook for post-application logic
        }

        protected float GetParameter(string key, float defaultValue = 0f)
        {
            return config?.GetParameter(key, defaultValue) ?? defaultValue;
        }

        protected float ApplyCurve(float input)
        {
            if (config?.ResponseCurve != null)
                return config.ResponseCurve.Evaluate(input);
            return input;
        }
    }
}
```

### Step 6: Modifier Implementations

#### 6.1 Create WinStreakModifier.cs
```csharp
// Path: Runtime/Modifiers/Implementations/WinStreakModifier.cs
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public class WinStreakModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "WinStreak";

        public WinStreakModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            var winThreshold = GetParameter("WinThreshold", 3f);
            var stepSize = GetParameter("StepSize", 0.5f);
            var maxBonus = GetParameter("MaxBonus", 2f);

            float value = 0f;
            string reason = "No win streak";

            if (sessionData.WinStreak >= winThreshold)
            {
                value = sessionData.WinStreak * stepSize;
                value = UnityEngine.Mathf.Min(value, maxBonus);
                value = ApplyCurve(value / maxBonus) * maxBonus;
                reason = $"Win streak: {sessionData.WinStreak} wins";
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata = { ["streak"] = sessionData.WinStreak }
            };
        }
    }
}
```

#### 6.2 Create LossStreakModifier.cs
```csharp
// Path: Runtime/Modifiers/Implementations/LossStreakModifier.cs
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public class LossStreakModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "LossStreak";

        public LossStreakModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            var lossThreshold = GetParameter("LossThreshold", 2f);
            var stepSize = GetParameter("StepSize", 0.3f);
            var maxReduction = GetParameter("MaxReduction", 1.5f);

            float value = 0f;
            string reason = "No loss streak";

            if (sessionData.LossStreak >= lossThreshold)
            {
                value = -(sessionData.LossStreak * stepSize);
                value = UnityEngine.Mathf.Max(value, -maxReduction);
                reason = $"Loss streak: {sessionData.LossStreak} losses";
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata = { ["streak"] = sessionData.LossStreak }
            };
        }
    }
}
```

#### 6.3 Create TimeDecayModifier.cs
```csharp
// Path: Runtime/Modifiers/Implementations/TimeDecayModifier.cs
using System;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public class TimeDecayModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "TimeDecay";

        public TimeDecayModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            var hoursSincePlay = (DateTime.Now - sessionData.LastPlayTime).TotalHours;
            var decayPerDay = GetParameter("DecayPerDay", 0.5f);
            var maxDecay = GetParameter("MaxDecay", 2f);
            var graceHours = GetParameter("GraceHours", 6f);

            float value = 0f;
            string reason = "Recently played";

            if (hoursSincePlay > graceHours)
            {
                var daysAway = (hoursSincePlay - graceHours) / 24;
                value = -(float)(daysAway * decayPerDay);
                value = UnityEngine.Mathf.Max(value, -maxDecay);

                if (daysAway < 1)
                    reason = $"Away for {hoursSincePlay:F1} hours";
                else
                    reason = $"Away for {daysAway:F1} days";
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata = { ["hours_away"] = hoursSincePlay }
            };
        }
    }
}
```

#### 6.4 Create RageQuitModifier.cs
```csharp
// Path: Runtime/Modifiers/Implementations/RageQuitModifier.cs
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Configuration;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;

namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public class RageQuitModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "RageQuit";

        public RageQuitModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            if (sessionData.LastSession == null)
                return new ModifierResult
                {
                    ModifierName = ModifierName,
                    Value = 0f,
                    Reason = "No previous session"
                };

            var rageQuitThreshold = GetParameter("RageQuitThreshold", 30f);
            var rageQuitReduction = GetParameter("RageQuitReduction", 1f);
            var quitReduction = GetParameter("QuitReduction", 0.5f);

            float value = 0f;
            string reason = "Normal session end";

            var lastSession = sessionData.LastSession;

            if (lastSession.EndType == SessionEndType.QuitAfterLoss)
            {
                if (lastSession.PlayDuration < rageQuitThreshold)
                {
                    value = -rageQuitReduction;
                    reason = "Rage quit detected";
                }
                else
                {
                    value = -quitReduction;
                    reason = "Quit after loss";
                }
            }
            else if (lastSession.EndType == SessionEndType.QuitDuringPlay)
            {
                value = -quitReduction * 0.5f;
                reason = "Quit during play";
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata =
                {
                    ["last_session_type"] = lastSession.EndType.ToString(),
                    ["play_duration"] = lastSession.PlayDuration
                }
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

### Step 8: Integration Points

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

3. **Configuration (2 files)**
   - ModifierConfig.cs
   - DifficultyConfig.cs

4. **Base Classes (1 file)**
   - BaseDifficultyModifier.cs

5. **Modifiers (4 files)**
   - WinStreakModifier.cs
   - LossStreakModifier.cs
   - TimeDecayModifier.cs
   - RageQuitModifier.cs

6. **Calculators (2 files)**
   - DifficultyCalculator.cs
   - ModifierAggregator.cs

7. **Providers (2 files)**
   - SessionDataProvider.cs
   - DifficultyDataProvider.cs

8. **Service (2 files)**
   - DynamicDifficultyService.cs
   - DifficultyConstants.cs

9. **DI (1 file)**
   - DynamicDifficultyModule.cs

10. **Editor (2 files)**
    - DifficultyDebugWindow.cs
    - ModifierConfigEditor.cs

## Testing Guide

### Unit Test Template
```csharp
using NUnit.Framework;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models;
using TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers;

[TestFixture]
public class ModifierTests
{
    [Test]
    public void WinStreak_AboveThreshold_IncreasesDifficulty()
    {
        // Arrange
        var config = CreateTestConfig();
        var modifier = new WinStreakModifier(config);
        var sessionData = new PlayerSessionData { WinStreak = 5 };

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        Assert.Greater(result.Value, 0);
        Assert.Contains("Win streak", result.Reason);
    }
}
```

## Integration Guide

### Step 1: Add to UITemplateVContainer
```csharp
#if THEONE_DYNAMIC_DIFFICULTY
var difficultyConfig = Resources.Load<DifficultyConfig>("Configs/DifficultyConfig");
builder.RegisterModule(new DynamicDifficultyModule(difficultyConfig));
#endif
```

### Step 2: Create ScriptableObject
1. Right-click in Project window
2. Create → DynamicDifficulty → Config
3. Configure parameters
4. Save in Resources/Configs/

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

3. **Null Reference Exceptions**
   - Initialize session data with defaults
   - Check config is loaded from Resources

4. **Difficulty Not Changing**
   - Verify modifiers are enabled
   - Check threshold values in config
   - Enable debug logs in DifficultyConfig

---

*Last Updated: 2025-09-16*