# Dynamic User Difficulty - Technical Design Document

## 1. Architecture Overview

### Design Principles
- **Single Responsibility**: Each class has one clear purpose
- **Open/Closed**: Extensible for new modifiers without changing core code
- **Dependency Injection**: All dependencies injected via VContainer
- **Modular Modifiers**: Easy to add/remove difficulty modifiers
- **Clean Separation**: Data, Logic, and Service layers clearly separated

### Core Components
```
DynamicUserDifficulty/
├── Core Service (Orchestrator)
├── Modifier System (Extensible)
├── Data Layer (Models & Storage)
├── Calculator Engine (Processing)
└── Configuration (ScriptableObjects)
```

## 2. Module Structure

### Folder Organization
```
DynamicUserDifficulty/
├── Runtime/
│   ├── Core/
│   │   ├── IDynamicDifficultyService.cs
│   │   ├── DynamicDifficultyService.cs
│   │   └── DifficultyConstants.cs
│   │
│   ├── Modifiers/
│   │   ├── Base/
│   │   │   ├── IDifficultyModifier.cs
│   │   │   └── BaseDifficultyModifier.cs
│   │   ├── Implementations/
│   │   │   ├── WinStreakModifier.cs
│   │   │   ├── TimeDecayModifier.cs
│   │   │   ├── LastSessionModifier.cs
│   │   │   └── RageQuitModifier.cs
│   │   └── ModifierPriority.cs
│   │
│   ├── Models/
│   │   ├── PlayerSessionData.cs
│   │   ├── DifficultyResult.cs
│   │   ├── ModifierResult.cs
│   │   └── SessionInfo.cs
│   │
│   ├── Calculators/
│   │   ├── IDifficultyCalculator.cs
│   │   ├── DifficultyCalculator.cs
│   │   └── ModifierAggregator.cs
│   │
│   ├── Providers/
│   │   ├── ISessionDataProvider.cs
│   │   ├── SessionDataProvider.cs
│   │   └── DifficultyDataProvider.cs
│   │
│   ├── Configuration/
│   │   ├── DifficultyConfig.cs
│   │   ├── ModifierConfig.cs
│   │   └── DifficultyPresets.cs
│   │
│   └── DI/
│       └── DynamicDifficultyModule.cs
│
├── Editor/
│   ├── DifficultyDebugWindow.cs
│   └── ModifierInspector.cs
│
├── Tests/
│   ├── Runtime/
│   │   └── DifficultyCalculatorTests.cs
│   └── Editor/
│       └── ModifierTests.cs
│
└── Documentation/
    ├── TechnicalDesign.md
    ├── DynamicUserDifficulty.md
    └── CLAUDE.md
```

## 3. Core Interfaces

### 3.1 Service Interface
```csharp
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
    }
}
```

### 3.2 Modifier Interface
```csharp
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

### 3.3 Calculator Interface
```csharp
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

### 3.4 Data Provider Interface
```csharp
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

## 4. Data Models

### 4.1 Core Data Models
```csharp
// PlayerSessionData.cs
namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Models
{
    [Serializable]
    public class PlayerSessionData
    {
        public float CurrentDifficulty;
        public int WinStreak;
        public int LossStreak;
        public DateTime LastPlayTime;
        public SessionInfo LastSession;
        public Queue<SessionInfo> RecentSessions;
    }

    // SessionInfo.cs
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

    // ModifierResult.cs
    public class ModifierResult
    {
        public string ModifierName;
        public float Value;
        public string Reason;
        public Dictionary<string, object> Metadata;
    }

    // DifficultyResult.cs
    public class DifficultyResult
    {
        public float PreviousDifficulty;
        public float NewDifficulty;
        public List<ModifierResult> AppliedModifiers;
        public DateTime CalculatedAt;
        public string PrimaryReason;
    }
}
```

### 4.2 Configuration Models
```csharp
// DifficultyConfig.cs
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
        public List<ModifierConfig> ModifierConfigs;
    }

    // ModifierConfig.cs
    [Serializable]
    public class ModifierConfig
    {
        public string ModifierType;
        public bool Enabled = true;
        public int Priority = 0;
        public AnimationCurve ResponseCurve;
        public Dictionary<string, float> Parameters;
    }
}
```

## 5. Modifier Implementations

### 5.1 Base Modifier
```csharp
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
            return config?.Parameters?.GetValueOrDefault(key, defaultValue)
                   ?? defaultValue;
        }
    }
}
```

### 5.2 Win Streak Modifier
```csharp
namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public class WinStreakModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "WinStreak";

        public WinStreakModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            var threshold = GetParameter("WinThreshold", 3f);
            var stepSize = GetParameter("StepSize", 0.5f);

            float value = 0f;
            string reason = "No streak";

            if (sessionData.WinStreak >= threshold)
            {
                value = sessionData.WinStreak * stepSize;
                reason = $"Win streak: {sessionData.WinStreak}";
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata = new() { ["streak"] = sessionData.WinStreak }
            };
        }
    }
}
```

### 5.3 Time Decay Modifier
```csharp
namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Modifiers
{
    public class TimeDecayModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "TimeDecay";

        public TimeDecayModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            var hoursSincePlay = (DateTime.Now - sessionData.LastPlayTime).TotalHours;
            var decayRate = GetParameter("DecayPerDay", 0.5f);
            var maxDecay = GetParameter("MaxDecay", 2f);

            var daysAway = hoursSincePlay / 24;
            var value = -Math.Min(daysAway * decayRate, maxDecay);

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = (float)value,
                Reason = $"Away for {daysAway:F1} days",
                Metadata = new() { ["hours_away"] = hoursSincePlay }
            };
        }
    }
}
```

## 6. Core Service Implementation

### 6.1 Dynamic Difficulty Service
```csharp
namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Core
{
    public class DynamicDifficultyService : IDynamicDifficultyService, IDisposable
    {
        private readonly ISessionDataProvider dataProvider;
        private readonly IDifficultyCalculator calculator;
        private readonly List<IDifficultyModifier> modifiers;
        private readonly DifficultyConfig config;

        public float CurrentDifficulty { get; private set; }

        public DynamicDifficultyService(
            ISessionDataProvider dataProvider,
            IDifficultyCalculator calculator,
            DifficultyConfig config)
        {
            this.dataProvider = dataProvider;
            this.calculator = calculator;
            this.config = config;
            this.modifiers = new List<IDifficultyModifier>();
        }

        public void Initialize()
        {
            var sessionData = dataProvider.GetCurrentSession();
            CurrentDifficulty = sessionData?.CurrentDifficulty
                              ?? config.DefaultDifficulty;
        }

        public DifficultyResult CalculateDifficulty()
        {
            var sessionData = dataProvider.GetCurrentSession();
            var enabledModifiers = modifiers.Where(m => m.IsEnabled)
                                           .OrderBy(m => m.Priority);

            return calculator.Calculate(sessionData, enabledModifiers);
        }

        public void ApplyDifficulty(DifficultyResult result)
        {
            CurrentDifficulty = result.NewDifficulty;

            var sessionData = dataProvider.GetCurrentSession();
            sessionData.CurrentDifficulty = result.NewDifficulty;
            dataProvider.SaveSession(sessionData);

            foreach (var modifier in modifiers)
            {
                modifier.OnApplied(result);
            }
        }

        public void RegisterModifier(IDifficultyModifier modifier)
        {
            if (!modifiers.Contains(modifier))
                modifiers.Add(modifier);
        }

        public void UnregisterModifier(IDifficultyModifier modifier)
        {
            modifiers.Remove(modifier);
        }

        public void Dispose()
        {
            modifiers.Clear();
        }
    }
}
```

### 6.2 Difficulty Calculator
```csharp
namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.Calculators
{
    public class DifficultyCalculator : IDifficultyCalculator
    {
        private readonly DifficultyConfig config;
        private readonly ModifierAggregator aggregator;

        public DifficultyCalculator(DifficultyConfig config, ModifierAggregator aggregator)
        {
            this.config = config;
            this.aggregator = aggregator;
        }

        public DifficultyResult Calculate(
            PlayerSessionData sessionData,
            IEnumerable<IDifficultyModifier> modifiers)
        {
            var modifierResults = modifiers
                .Select(m => m.Calculate(sessionData))
                .Where(r => r != null)
                .ToList();

            var totalModifier = aggregator.Aggregate(modifierResults);
            var newDifficulty = ClampDifficulty(
                sessionData.CurrentDifficulty + totalModifier);

            return new DifficultyResult
            {
                PreviousDifficulty = sessionData.CurrentDifficulty,
                NewDifficulty = newDifficulty,
                AppliedModifiers = modifierResults,
                CalculatedAt = DateTime.Now,
                PrimaryReason = GetPrimaryReason(modifierResults)
            };
        }

        private float ClampDifficulty(float value)
        {
            return Mathf.Clamp(value, config.MinDifficulty, config.MaxDifficulty);
        }

        private string GetPrimaryReason(List<ModifierResult> results)
        {
            return results.OrderByDescending(r => Math.Abs(r.Value))
                         .FirstOrDefault()?.Reason ?? "No change";
        }
    }
}
```

## 7. Dependency Injection Setup

### 7.1 VContainer Module
```csharp
namespace TheOneStudio.UITemplate.Services.DynamicUserDifficulty.DI
{
    public class DynamicDifficultyModule : IInstaller
    {
        private readonly DifficultyConfig config;

        public DynamicDifficultyModule(DifficultyConfig config)
        {
            this.config = config;
        }

        public void Install(IContainerBuilder builder)
        {
            // Core service
            builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(
                Lifetime.Singleton);

            // Providers
            builder.Register<ISessionDataProvider, SessionDataProvider>(
                Lifetime.Singleton);

            // Calculator
            builder.Register<IDifficultyCalculator, DifficultyCalculator>(
                Lifetime.Singleton);
            builder.Register<ModifierAggregator>(Lifetime.Singleton);

            // Configuration
            builder.RegisterInstance(config);

            // Register modifiers
            RegisterModifiers(builder);
        }

        private void RegisterModifiers(IContainerBuilder builder)
        {
            // Register each modifier type
            builder.Register<WinStreakModifier>(Lifetime.Singleton)
                   .As<IDifficultyModifier>();

            builder.Register<TimeDecayModifier>(Lifetime.Singleton)
                   .As<IDifficultyModifier>();

            builder.Register<LastSessionModifier>(Lifetime.Singleton)
                   .As<IDifficultyModifier>();

            builder.Register<RageQuitModifier>(Lifetime.Singleton)
                   .As<IDifficultyModifier>();

            // Auto-register all modifiers to the service
            builder.RegisterBuildCallback(container =>
            {
                var service = container.Resolve<IDynamicDifficultyService>();
                var modifiers = container.Resolve<IEnumerable<IDifficultyModifier>>();

                foreach (var modifier in modifiers)
                {
                    service.RegisterModifier(modifier);
                }
            });
        }
    }
}
```

### 7.2 Integration in UITemplateVContainer
```csharp
// Add to UITemplateVContainer.cs
protected override void Configure(IContainerBuilder builder)
{
    // ... existing code ...

    #if THEONE_DYNAMIC_DIFFICULTY
    var difficultyConfig = Resources.Load<DifficultyConfig>(
        "Configs/DifficultyConfig");
    builder.RegisterModule(new DynamicDifficultyModule(difficultyConfig));
    #endif
}
```

## 8. Usage Examples

### 8.1 Basic Usage
```csharp
public class GameplayController
{
    private readonly IDynamicDifficultyService difficultyService;

    public GameplayController(IDynamicDifficultyService difficultyService)
    {
        this.difficultyService = difficultyService;
    }

    public void OnGameStart()
    {
        var result = difficultyService.CalculateDifficulty();
        difficultyService.ApplyDifficulty(result);

        Debug.Log($"Difficulty: {result.NewDifficulty} " +
                  $"(Reason: {result.PrimaryReason})");
    }
}
```

### 8.2 Adding Custom Modifier
```csharp
public class CustomEngagementModifier : BaseDifficultyModifier
{
    public override string ModifierName => "Engagement";

    public CustomEngagementModifier(ModifierConfig config) : base(config) { }

    public override ModifierResult Calculate(PlayerSessionData sessionData)
    {
        // Your custom logic here
        var engagement = CalculateEngagementScore(sessionData);

        return new ModifierResult
        {
            ModifierName = ModifierName,
            Value = engagement * 0.1f,
            Reason = $"Engagement score: {engagement}"
        };
    }
}

// Register in DI
builder.Register<CustomEngagementModifier>(Lifetime.Singleton)
       .As<IDifficultyModifier>();
```

## 9. Testing Framework Architecture

### 9.1 Test Structure Overview

```
Tests/
├── Runtime/                      # Play mode tests
│   ├── Unit/                    # Unit tests
│   │   ├── Modifiers/
│   │   ├── Calculators/
│   │   ├── Providers/
│   │   └── Models/
│   ├── Integration/             # Integration tests
│   │   ├── ServiceTests/
│   │   ├── ModifierPipelineTests/
│   │   └── DIContainerTests/
│   └── EndToEnd/               # Full system tests
│       └── DifficultyFlowTests/
│
├── Editor/                      # Edit mode tests
│   ├── Configuration/
│   ├── ScriptableObjects/
│   └── EditorTools/
│
├── TestFramework/              # Test infrastructure
│   ├── Base/
│   │   ├── BaseDifficultyTest.cs
│   │   └── BaseModifierTest.cs
│   ├── Mocks/
│   │   ├── MockSessionDataProvider.cs
│   │   ├── MockDifficultyCalculator.cs
│   │   └── MockModifier.cs
│   ├── Stubs/
│   │   └── StubDifficultyConfig.cs
│   ├── Builders/
│   │   ├── SessionDataBuilder.cs
│   │   └── ConfigBuilder.cs
│   └── Utilities/
│       ├── TestConstants.cs
│       └── AssertExtensions.cs
│
└── TestData/                   # Test fixtures
    ├── Configs/
    └── SessionData/
```

### 9.2 Test Framework Components

#### Base Test Classes

```csharp
// BaseDifficultyTest.cs
public abstract class BaseDifficultyTest
{
    protected DifficultyConfig defaultConfig;
    protected MockSessionDataProvider mockProvider;
    protected IContainerBuilder containerBuilder;

    [SetUp]
    public virtual void Setup()
    {
        defaultConfig = ConfigBuilder.CreateDefault();
        mockProvider = new MockSessionDataProvider();
        containerBuilder = new ContainerBuilder();
    }

    [TearDown]
    public virtual void TearDown()
    {
        mockProvider.Reset();
    }

    protected T CreateService<T>() where T : class
    {
        var container = containerBuilder.Build();
        return container.Resolve<T>();
    }
}

// BaseModifierTest.cs
public abstract class BaseModifierTest<TModifier> : BaseDifficultyTest
    where TModifier : IDifficultyModifier
{
    protected TModifier modifier;
    protected ModifierConfig modifierConfig;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        modifierConfig = CreateModifierConfig();
        modifier = CreateModifier(modifierConfig);
    }

    protected abstract ModifierConfig CreateModifierConfig();
    protected abstract TModifier CreateModifier(ModifierConfig config);

    protected void AssertModifierResult(
        ModifierResult result,
        float expectedValue,
        string expectedReason = null)
    {
        Assert.NotNull(result);
        Assert.AreEqual(expectedValue, result.Value, 0.01f);
        if (expectedReason != null)
            StringAssert.Contains(expectedReason, result.Reason);
    }
}
```

#### Mock Implementations

```csharp
// MockSessionDataProvider.cs
public class MockSessionDataProvider : ISessionDataProvider
{
    private PlayerSessionData mockData;
    public int SaveCallCount { get; private set; }

    public void SetMockData(PlayerSessionData data)
    {
        mockData = data;
    }

    public PlayerSessionData GetCurrentSession()
    {
        return mockData ?? new PlayerSessionData();
    }

    public void SaveSession(PlayerSessionData data)
    {
        mockData = data;
        SaveCallCount++;
    }

    public void Reset()
    {
        mockData = null;
        SaveCallCount = 0;
    }
}

// MockModifier.cs
public class MockModifier : IDifficultyModifier
{
    public string ModifierName => "Mock";
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
    public float ReturnValue { get; set; }
    public int CalculateCallCount { get; private set; }

    public ModifierResult Calculate(PlayerSessionData sessionData)
    {
        CalculateCallCount++;
        return new ModifierResult("Mock", ReturnValue, "Mock result");
    }

    public void OnApplied(DifficultyResult result) { }
}
```

#### Test Builders

```csharp
// SessionDataBuilder.cs
public class SessionDataBuilder
{
    private PlayerSessionData data = new PlayerSessionData();

    public static SessionDataBuilder Create() => new SessionDataBuilder();

    public SessionDataBuilder WithDifficulty(float difficulty)
    {
        data.CurrentDifficulty = difficulty;
        return this;
    }

    public SessionDataBuilder WithWinStreak(int streak)
    {
        data.WinStreak = streak;
        return this;
    }

    public SessionDataBuilder WithLossStreak(int streak)
    {
        data.LossStreak = streak;
        return this;
    }

    public SessionDataBuilder WithLastPlayTime(DateTime time)
    {
        data.LastPlayTime = time;
        return this;
    }

    public SessionDataBuilder WithRecentSessions(params SessionInfo[] sessions)
    {
        data.RecentSessions = new Queue<SessionInfo>(sessions);
        return this;
    }

    public PlayerSessionData Build() => data;
}

// ConfigBuilder.cs
public class ConfigBuilder
{
    private DifficultyConfig config;

    public static ConfigBuilder Create()
    {
        var builder = new ConfigBuilder();
        builder.config = ScriptableObject.CreateInstance<DifficultyConfig>();
        return builder;
    }

    public ConfigBuilder WithRange(float min, float max)
    {
        // Use reflection or public setters
        return this;
    }

    public ConfigBuilder WithModifier(ModifierConfig modifierConfig)
    {
        config.ModifierConfigs.Add(modifierConfig);
        return this;
    }

    public DifficultyConfig Build() => config;
}
```

### 9.3 Test Categories

#### Unit Tests

```csharp
[TestFixture]
[Category("Unit")]
public class WinStreakModifierTests : BaseModifierTest<WinStreakModifier>
{
    protected override ModifierConfig CreateModifierConfig()
    {
        return new ModifierConfig
        {
            ModifierType = "WinStreak",
            Parameters = new List<ModifierParameter>
            {
                new ModifierParameter("WinThreshold", 3),
                new ModifierParameter("StepSize", 0.5f)
            }
        };
    }

    protected override WinStreakModifier CreateModifier(ModifierConfig config)
    {
        return new WinStreakModifier(config);
    }

    [Test]
    public void Calculate_BelowThreshold_ReturnsNoChange()
    {
        // Arrange
        var sessionData = SessionDataBuilder.Create()
            .WithWinStreak(2)
            .Build();

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        AssertModifierResult(result, 0f, "No win streak");
    }

    [TestCase(3, 0.5f)]
    [TestCase(5, 1.5f)]
    [TestCase(10, 2.0f)] // Max bonus
    public void Calculate_AboveThreshold_ReturnsCorrectValue(
        int winStreak, float expectedValue)
    {
        // Arrange
        var sessionData = SessionDataBuilder.Create()
            .WithWinStreak(winStreak)
            .Build();

        // Act
        var result = modifier.Calculate(sessionData);

        // Assert
        AssertModifierResult(result, expectedValue);
    }
}
```

#### Integration Tests

```csharp
[TestFixture]
[Category("Integration")]
public class DifficultyServiceIntegrationTests : BaseDifficultyTest
{
    private IDynamicDifficultyService service;

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        containerBuilder.Register<ISessionDataProvider>(_ => mockProvider);
        containerBuilder.Register<IDifficultyCalculator, DifficultyCalculator>();
        containerBuilder.Register<ModifierAggregator>();
        containerBuilder.RegisterInstance(defaultConfig);
        containerBuilder.Register<IDynamicDifficultyService, DynamicDifficultyService>();

        service = CreateService<IDynamicDifficultyService>();
    }

    [Test]
    public void CalculateDifficulty_WithMultipleModifiers_AggregatesCorrectly()
    {
        // Arrange
        var winModifier = new MockModifier { ReturnValue = 1.0f };
        var lossModifier = new MockModifier { ReturnValue = -0.5f };

        service.RegisterModifier(winModifier);
        service.RegisterModifier(lossModifier);

        // Act
        var result = service.CalculateDifficulty();

        // Assert
        Assert.AreEqual(2, result.AppliedModifiers.Count);
        Assert.AreEqual(0.5f, result.NewDifficulty - result.PreviousDifficulty, 0.01f);
    }
}
```

#### End-to-End Tests

```csharp
[TestFixture]
[Category("E2E")]
[RequiresPlayMode]
public class DifficultyFlowE2ETests
{
    [UnityTest]
    public IEnumerator CompleteGameFlow_AdjustsDifficultyCorrectly()
    {
        // Setup
        var container = CreateTestContainer();
        var service = container.Resolve<IDynamicDifficultyService>();

        // Simulate game flow
        service.OnSessionStart();
        yield return null;

        service.OnLevelStart(1);
        yield return new WaitForSeconds(0.1f);

        service.OnLevelComplete(true, 120f);
        yield return null;

        // Verify
        var result = service.CalculateDifficulty();
        Assert.Greater(result.NewDifficulty, result.PreviousDifficulty);
    }
}
```

### 9.4 Test Utilities

#### Custom Assertions

```csharp
public static class DifficultyAssert
{
    public static void IsInRange(float value, float min, float max)
    {
        Assert.GreaterOrEqual(value, min,
            $"Value {value} is less than minimum {min}");
        Assert.LessOrEqual(value, max,
            $"Value {value} is greater than maximum {max}");
    }

    public static void HasModifier(DifficultyResult result, string modifierName)
    {
        Assert.IsTrue(
            result.AppliedModifiers.Any(m => m.ModifierName == modifierName),
            $"Result does not contain modifier: {modifierName}");
    }

    public static void DifficultyChanged(DifficultyResult result, float expectedChange)
    {
        var actualChange = result.NewDifficulty - result.PreviousDifficulty;
        Assert.AreEqual(expectedChange, actualChange, 0.01f,
            $"Expected difficulty change of {expectedChange}, but got {actualChange}");
    }
}
```

#### Test Constants

```csharp
public static class TestDifficultyConstants
{
    public const float DEFAULT_DIFFICULTY = 3f;
    public const float MIN_DIFFICULTY = 1f;
    public const float MAX_DIFFICULTY = 10f;

    public static readonly DateTime TestDateTime = new DateTime(2024, 1, 1);
    public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
    public static readonly TimeSpan OneWeek = TimeSpan.FromDays(7);
}
```

### 9.5 Test Assembly Definitions

```json
// DynamicUserDifficulty.Tests.asmdef
{
    "name": "DynamicUserDifficulty.Tests",
    "references": [
        "DynamicUserDifficulty.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}

// DynamicUserDifficulty.Tests.Runtime.asmdef
{
    "name": "DynamicUserDifficulty.Tests.Runtime",
    "references": [
        "DynamicUserDifficulty.Runtime",
        "UnityEngine.TestRunner"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

## 10. Performance Considerations

### Optimization Guidelines
1. **Cache Calculations**: Store results for 1 level duration
2. **Lazy Loading**: Load modifiers only when needed
3. **Batch Updates**: Update session data in batches
4. **Async Operations**: Use UniTask for I/O operations

### Memory Management
```csharp
public class SessionDataProvider : ISessionDataProvider, IDisposable
{
    private PlayerSessionData cachedData;
    private DateTime cacheTime;
    private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(5);

    public PlayerSessionData GetCurrentSession()
    {
        if (cachedData == null || DateTime.Now - cacheTime > cacheExpiry)
        {
            cachedData = LoadFromDisk();
            cacheTime = DateTime.Now;
        }
        return cachedData;
    }

    public void Dispose()
    {
        cachedData = null;
    }
}
```

## 11. Debug Tools

### Editor Window
```csharp
public class DifficultyDebugWindow : EditorWindow
{
    [MenuItem("TheOne/Debug/Difficulty Monitor")]
    public static void ShowWindow()
    {
        GetWindow<DifficultyDebugWindow>("Difficulty Monitor");
    }

    private void OnGUI()
    {
        // Display current difficulty
        // Show modifier values
        // Allow manual adjustment
        // Graph historical data
    }
}
```

## 12. Configuration Examples

### Easy Mode Preset
```json
{
  "MinDifficulty": 1,
  "MaxDifficulty": 5,
  "DefaultDifficulty": 2,
  "ModifierConfigs": [
    {
      "ModifierType": "WinStreak",
      "Parameters": {
        "WinThreshold": 5,
        "StepSize": 0.2
      }
    }
  ]
}
```

### Competitive Mode Preset
```json
{
  "MinDifficulty": 3,
  "MaxDifficulty": 10,
  "DefaultDifficulty": 5,
  "ModifierConfigs": [
    {
      "ModifierType": "WinStreak",
      "Parameters": {
        "WinThreshold": 2,
        "StepSize": 0.8
      }
    }
  ]
}
```

## 13. Checklist for Adding New Modifiers

- [ ] Create class extending `BaseDifficultyModifier`
- [ ] Implement `Calculate()` method
- [ ] Add configuration in `ModifierConfig`
- [ ] Register in `DynamicDifficultyModule`
- [ ] Add unit tests
- [ ] Update documentation
- [ ] Add analytics tracking

---

*Version: 1.0.0*
*Last Updated: 2025-09-16*