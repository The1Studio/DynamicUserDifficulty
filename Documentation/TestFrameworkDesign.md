# Dynamic User Difficulty - Test Framework Design

## Table of Contents
1. [Overview](#overview)
2. [Test Architecture](#test-architecture)
3. [Test Categories](#test-categories)
4. [Test Infrastructure](#test-infrastructure)
5. [Mock and Stub Strategy](#mock-and-stub-strategy)
6. [Test Data Management](#test-data-management)
7. [Test Execution Strategy](#test-execution-strategy)
8. [Coverage Requirements](#coverage-requirements)
9. [Best Practices](#best-practices)
10. [Implementation Guide](#implementation-guide)

## Overview

### Purpose
The test framework provides comprehensive testing coverage for the Dynamic User Difficulty system, ensuring reliability, maintainability, and correctness across all components.

### Key Principles
- **Isolation**: Each test should be independent
- **Repeatability**: Tests must produce consistent results
- **Speed**: Fast execution for rapid feedback
- **Clarity**: Clear test names and assertions
- **Coverage**: Comprehensive coverage of critical paths

### Technology Stack
- **Unity Test Framework**: Core testing framework
- **NUnit**: Assertion framework
- **NSubstitute**: Mocking framework (optional)
- **Unity Test Runner**: Test execution and reporting

## Test Architecture

### Layered Testing Approach

```
┌─────────────────────────────────────────┐
│         End-to-End Tests                │  ← Full system validation
├─────────────────────────────────────────┤
│       Integration Tests                  │  ← Component interaction
├─────────────────────────────────────────┤
│          Unit Tests                      │  ← Individual components
├─────────────────────────────────────────┤
│     Test Infrastructure                  │  ← Support layer
└─────────────────────────────────────────┘
```

### Directory Structure

```
Tests/
├── Runtime/                                # Play mode tests
│   ├── Unit/
│   │   ├── Modifiers/
│   │   │   ├── WinStreakModifierTests.cs
│   │   │   ├── LossStreakModifierTests.cs
│   │   │   ├── TimeDecayModifierTests.cs
│   │   │   └── RageQuitModifierTests.cs
│   │   ├── Calculators/
│   │   │   ├── DifficultyCalculatorTests.cs
│   │   │   └── ModifierAggregatorTests.cs
│   │   ├── Providers/
│   │   │   └── SessionDataProviderTests.cs
│   │   └── Models/
│   │       ├── PlayerSessionDataTests.cs
│   │       └── DifficultyResultTests.cs
│   │
│   ├── Integration/
│   │   ├── ServiceIntegrationTests.cs
│   │   ├── ModifierPipelineTests.cs
│   │   └── DIContainerTests.cs
│   │
│   └── EndToEnd/
│       ├── DifficultyFlowTests.cs
│       └── PlayerJourneyTests.cs
│
├── Editor/                                 # Edit mode tests
│   ├── Configuration/
│   │   ├── DifficultyConfigTests.cs
│   │   └── ModifierConfigTests.cs
│   ├── ScriptableObjects/
│   │   └── ConfigValidationTests.cs
│   └── EditorTools/
│       └── TestRunnerExtensions.cs
│
├── TestFramework/
│   ├── Base/
│   │   ├── BaseDifficultyTest.cs
│   │   ├── BaseModifierTest.cs
│   │   └── BaseIntegrationTest.cs
│   │
│   ├── Mocks/
│   │   ├── MockSessionDataProvider.cs
│   │   ├── MockDifficultyCalculator.cs
│   │   ├── MockModifier.cs
│   │   └── MockSignalBus.cs
│   │
│   ├── Stubs/
│   │   ├── StubDifficultyConfig.cs
│   │   └── StubModifierConfig.cs
│   │
│   ├── Builders/
│   │   ├── SessionDataBuilder.cs
│   │   ├── ConfigBuilder.cs
│   │   ├── ModifierResultBuilder.cs
│   │   └── DifficultyResultBuilder.cs
│   │
│   ├── Fixtures/
│   │   ├── DifficultyFixtures.cs
│   │   └── SessionFixtures.cs
│   │
│   └── Utilities/
│       ├── TestConstants.cs
│       ├── AssertExtensions.cs
│       ├── TestHelpers.cs
│       └── RandomTestData.cs
│
├── TestData/
│   ├── Configs/
│   │   ├── TestDifficultyConfig.asset
│   │   └── TestModifierConfigs.json
│   └── Sessions/
│       ├── SampleSessions.json
│       └── EdgeCaseSessions.json
│
└── TestAssemblies/
    ├── DynamicUserDifficulty.Tests.asmdef
    ├── DynamicUserDifficulty.Tests.Runtime.asmdef
    └── DynamicUserDifficulty.Tests.Editor.asmdef
```

## Test Categories

### 1. Unit Tests

#### Purpose
Test individual components in isolation

#### Scope
- Single class/method testing
- Mock all dependencies
- Fast execution (< 10ms per test)

#### Example Structure
```csharp
[TestFixture]
[Category("Unit")]
public class WinStreakModifierTests : BaseModifierTest<WinStreakModifier>
{
    [Test]
    public void Calculate_WithNoStreak_ReturnsZero()
    {
        // Arrange
        var data = SessionDataBuilder.Create().Build();

        // Act
        var result = modifier.Calculate(data);

        // Assert
        Assert.AreEqual(0f, result.Value);
    }

    [TestCase(3, 0.5f)]
    [TestCase(5, 1.5f)]
    public void Calculate_WithStreak_ReturnsExpectedValue(int streak, float expected)
    {
        // Arrange
        var data = SessionDataBuilder.Create()
            .WithWinStreak(streak)
            .Build();

        // Act
        var result = modifier.Calculate(data);

        // Assert
        Assert.AreEqual(expected, result.Value, 0.01f);
    }
}
```

### 2. Integration Tests

#### Purpose
Test interaction between multiple components

#### Scope
- Component interaction
- Partial mocking
- Medium execution time (< 100ms per test)

#### Example Structure
```csharp
[TestFixture]
[Category("Integration")]
public class DifficultyServiceIntegrationTests : BaseIntegrationTest
{
    [Test]
    public void Service_WithRealComponents_CalculatesCorrectly()
    {
        // Arrange
        var service = CreateServiceWithRealComponents();
        SetupTestData();

        // Act
        var result = service.CalculateDifficulty();

        // Assert
        AssertDifficultyInRange(result);
        AssertModifiersApplied(result);
    }
}
```

### 3. End-to-End Tests

#### Purpose
Test complete user scenarios

#### Scope
- Full system testing
- No mocking
- Slower execution (< 1s per test)

#### Example Structure
```csharp
[TestFixture]
[Category("E2E")]
[RequiresPlayMode]
public class PlayerJourneyTests
{
    [UnityTest]
    public IEnumerator NewPlayer_CompletesLevels_DifficultyAdjusts()
    {
        // Setup
        var system = CreateFullSystem();

        // Simulate new player
        yield return SimulateNewPlayer(system);

        // Play and lose first levels
        yield return SimulateMultipleLosses(system, 3);

        // Verify difficulty decreased
        AssertDifficultyDecreased(system);

        // Play and win next levels
        yield return SimulateMultipleWins(system, 5);

        // Verify difficulty increased
        AssertDifficultyIncreased(system);
    }
}
```

## Test Infrastructure

### Base Test Classes

#### BaseDifficultyTest
```csharp
public abstract class BaseDifficultyTest
{
    protected DifficultyConfig defaultConfig;
    protected MockSessionDataProvider mockProvider;
    protected TestContainer container;

    [SetUp]
    public virtual void Setup()
    {
        defaultConfig = ConfigBuilder.Default();
        mockProvider = new MockSessionDataProvider();
        container = new TestContainer();
    }

    [TearDown]
    public virtual void TearDown()
    {
        container?.Dispose();
        mockProvider?.Reset();
    }

    protected T GetService<T>() where T : class
    {
        return container.Resolve<T>();
    }

    protected void RegisterMock<T>(T mock) where T : class
    {
        container.RegisterInstance(mock);
    }
}
```

#### BaseModifierTest
```csharp
public abstract class BaseModifierTest<TModifier> : BaseDifficultyTest
    where TModifier : IDifficultyModifier
{
    protected TModifier modifier;
    protected ModifierConfig config;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        config = CreateConfig();
        modifier = CreateModifier(config);
    }

    protected abstract ModifierConfig CreateConfig();
    protected abstract TModifier CreateModifier(ModifierConfig config);

    protected void AssertModifierResult(
        ModifierResult result,
        float expectedValue,
        string containsReason = null)
    {
        Assert.NotNull(result);
        Assert.AreEqual(expectedValue, result.Value, 0.01f);

        if (containsReason != null)
        {
            StringAssert.Contains(containsReason, result.Reason);
        }
    }
}
```

## Mock and Stub Strategy

### Mock vs Stub Decision Matrix

| Use Case | Mock | Stub | Real |
|----------|------|------|------|
| External dependencies | ✓ | | |
| Simple data objects | | ✓ | |
| Configuration | | ✓ | |
| Complex behavior | ✓ | | |
| Performance critical | | | ✓ |
| Integration tests | | | ✓ |

### Mock Implementations

#### MockSessionDataProvider
```csharp
public class MockSessionDataProvider : ISessionDataProvider
{
    private PlayerSessionData mockData;
    private readonly List<string> methodCalls = new();

    public int SaveCount { get; private set; }
    public bool ThrowOnSave { get; set; }

    public void SetMockData(PlayerSessionData data)
    {
        mockData = data;
    }

    public PlayerSessionData GetCurrentSession()
    {
        methodCalls.Add(nameof(GetCurrentSession));
        return mockData ?? new PlayerSessionData();
    }

    public void SaveSession(PlayerSessionData data)
    {
        methodCalls.Add(nameof(SaveSession));

        if (ThrowOnSave)
            throw new InvalidOperationException("Mock save failed");

        mockData = data;
        SaveCount++;
    }

    public void VerifyMethodCalled(string methodName, int times = 1)
    {
        var count = methodCalls.Count(m => m == methodName);
        Assert.AreEqual(times, count,
            $"Expected {methodName} to be called {times} times, but was called {count} times");
    }

    public void Reset()
    {
        mockData = null;
        SaveCount = 0;
        methodCalls.Clear();
        ThrowOnSave = false;
    }
}
```

#### MockModifier
```csharp
public class MockModifier : IDifficultyModifier
{
    public string ModifierName { get; set; } = "Mock";
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;

    public Func<PlayerSessionData, ModifierResult> CalculateFunc { get; set; }
    public Action<DifficultyResult> OnAppliedAction { get; set; }

    public int CalculateCallCount { get; private set; }
    public int OnAppliedCallCount { get; private set; }

    public ModifierResult Calculate(PlayerSessionData sessionData)
    {
        CalculateCallCount++;

        if (CalculateFunc != null)
            return CalculateFunc(sessionData);

        return new ModifierResult(ModifierName, 0f, "Mock result");
    }

    public void OnApplied(DifficultyResult result)
    {
        OnAppliedCallCount++;
        OnAppliedAction?.Invoke(result);
    }

    public void Reset()
    {
        CalculateCallCount = 0;
        OnAppliedCallCount = 0;
    }
}
```

### Stub Implementations

#### StubDifficultyConfig
```csharp
public class StubDifficultyConfig
{
    public static DifficultyConfig Create(
        float min = 1f,
        float max = 10f,
        float defaultDiff = 3f)
    {
        var config = ScriptableObject.CreateInstance<DifficultyConfig>();

        // Use reflection to set private fields
        SetPrivateField(config, "minDifficulty", min);
        SetPrivateField(config, "maxDifficulty", max);
        SetPrivateField(config, "defaultDifficulty", defaultDiff);

        return config;
    }

    public static DifficultyConfig WithModifiers(params ModifierConfig[] modifiers)
    {
        var config = Create();
        SetPrivateField(config, "modifierConfigs", modifiers.ToList());
        return config;
    }
}
```

## Test Data Management

### Test Data Builders

#### SessionDataBuilder
```csharp
public class SessionDataBuilder
{
    private readonly PlayerSessionData data = new();

    public static SessionDataBuilder Create() => new();

    public SessionDataBuilder WithDefaults()
    {
        data.CurrentDifficulty = 3f;
        data.WinStreak = 0;
        data.LossStreak = 0;
        data.LastPlayTime = DateTime.Now;
        return this;
    }

    public SessionDataBuilder WithWinStreak(int streak)
    {
        data.WinStreak = streak;
        data.LossStreak = 0;
        return this;
    }

    public SessionDataBuilder WithLossStreak(int streak)
    {
        data.LossStreak = streak;
        data.WinStreak = 0;
        return this;
    }

    public SessionDataBuilder WithTimeSinceLastPlay(TimeSpan time)
    {
        data.LastPlayTime = DateTime.Now - time;
        return this;
    }

    public SessionDataBuilder WithLastSession(SessionEndType endType, float duration)
    {
        data.LastSession = new SessionInfo
        {
            EndType = endType,
            PlayDuration = duration,
            Won = endType == SessionEndType.CompletedWin
        };
        return this;
    }

    public SessionDataBuilder WithRandomData()
    {
        var random = new System.Random();
        data.CurrentDifficulty = random.Next(1, 10);
        data.WinStreak = random.Next(0, 10);
        data.LossStreak = random.Next(0, 5);
        return this;
    }

    public PlayerSessionData Build() => data;
}
```

### Test Fixtures

#### DifficultyFixtures
```csharp
public static class DifficultyFixtures
{
    public static class Players
    {
        public static PlayerSessionData NewPlayer =>
            SessionDataBuilder.Create()
                .WithDefaults()
                .Build();

        public static PlayerSessionData WinningPlayer =>
            SessionDataBuilder.Create()
                .WithWinStreak(5)
                .WithDifficulty(5f)
                .Build();

        public static PlayerSessionData StrugglingPlayer =>
            SessionDataBuilder.Create()
                .WithLossStreak(3)
                .WithDifficulty(2f)
                .Build();

        public static PlayerSessionData ReturningPlayer =>
            SessionDataBuilder.Create()
                .WithTimeSinceLastPlay(TimeSpan.FromDays(7))
                .Build();
    }

    public static class Configs
    {
        public static DifficultyConfig Default =>
            StubDifficultyConfig.Create();

        public static DifficultyConfig Competitive =>
            StubDifficultyConfig.Create(3f, 10f, 5f);

        public static DifficultyConfig Casual =>
            StubDifficultyConfig.Create(1f, 5f, 2f);
    }
}
```

## Test Execution Strategy

### Test Phases

#### 1. Continuous Integration (CI)
```yaml
test-phases:
  - name: "Fast Tests"
    categories: ["Unit"]
    timeout: 60s
    parallel: true

  - name: "Integration Tests"
    categories: ["Integration"]
    timeout: 300s
    parallel: false

  - name: "E2E Tests"
    categories: ["E2E"]
    timeout: 600s
    parallel: false
    run-on: ["nightly", "release"]
```

#### 2. Local Development
```bash
# Run all unit tests
Unity -runTests -testCategory Unit

# Run specific test class
Unity -runTests -testFilter WinStreakModifierTests

# Run with coverage
Unity -runTests -enableCodeCoverage
```

### Performance Benchmarks

| Test Type | Target Time | Max Time |
|-----------|------------|----------|
| Unit Test | < 10ms | 50ms |
| Integration Test | < 100ms | 500ms |
| E2E Test | < 1000ms | 5000ms |

## Coverage Requirements

### Minimum Coverage Targets

| Component | Line Coverage | Branch Coverage |
|-----------|--------------|-----------------|
| Core Service | 90% | 85% |
| Modifiers | 95% | 90% |
| Calculators | 90% | 85% |
| Providers | 85% | 80% |
| Models | 80% | 75% |
| Overall | 85% | 80% |

### Critical Paths (100% Coverage Required)

1. **Difficulty Calculation Pipeline**
   - CalculateDifficulty()
   - ApplyDifficulty()
   - All modifier Calculate() methods

2. **Data Persistence**
   - SaveSession()
   - GetCurrentSession()

3. **Edge Cases**
   - Min/Max difficulty clamping
   - Null data handling
   - Invalid configuration

## Best Practices

### Test Naming Convention

```csharp
[Test]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Example:
    // Calculate_WithWinStreakBelowThreshold_ReturnsZero()
    // SaveSession_WhenDataIsNull_ThrowsArgumentException()
}
```

### Assertion Patterns

```csharp
// Use custom assertions for clarity
DifficultyAssert.IsInRange(result.NewDifficulty, 1f, 10f);
DifficultyAssert.HasModifier(result, "WinStreak");
DifficultyAssert.DifficultyChanged(result, expectedChange);

// Multiple assertions with clear messages
Assert.Multiple(() =>
{
    Assert.NotNull(result, "Result should not be null");
    Assert.Greater(result.NewDifficulty, 0, "Difficulty should be positive");
    Assert.Contains("WinStreak", result.PrimaryReason, "Should mention win streak");
});
```

### Test Data Patterns

```csharp
// Use builders for complex objects
var sessionData = SessionDataBuilder.Create()
    .WithWinStreak(5)
    .WithLastSession(SessionEndType.CompletedWin, 120f)
    .Build();

// Use fixtures for common scenarios
var strugglingPlayer = DifficultyFixtures.Players.StrugglingPlayer;

// Use parameterized tests for multiple scenarios
[TestCase(0, 0f)]
[TestCase(3, 0.5f)]
[TestCase(5, 1.5f)]
public void TestMultipleScenarios(int input, float expected) { }
```

### Mock Usage Patterns

```csharp
// Setup mock behavior
mockProvider.SetMockData(testData);
mockProvider.ThrowOnSave = true;

// Verify interactions
mockProvider.VerifyMethodCalled(nameof(SaveSession), times: 2);

// Use functional mocks for complex behavior
var mockModifier = new MockModifier
{
    CalculateFunc = (data) => new ModifierResult("Test", data.WinStreak * 0.5f, "Test")
};
```

## Implementation Guide

### Step 1: Create Test Assembly Definitions

```json
// Tests/TestAssemblies/DynamicUserDifficulty.Tests.asmdef
{
    "name": "DynamicUserDifficulty.Tests",
    "rootNamespace": "DynamicUserDifficulty.Tests",
    "references": [
        "GUID:...", // DynamicUserDifficulty.Runtime
        "GUID:...", // UnityEngine.TestRunner
        "GUID:...", // UnityEditor.TestRunner
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### Step 2: Implement Base Classes

1. Create `BaseDifficultyTest.cs`
2. Create `BaseModifierTest.cs`
3. Create `BaseIntegrationTest.cs`

### Step 3: Implement Mock Objects

1. Create `MockSessionDataProvider.cs`
2. Create `MockModifier.cs`
3. Create `MockDifficultyCalculator.cs`

### Step 4: Implement Builders

1. Create `SessionDataBuilder.cs`
2. Create `ConfigBuilder.cs`
3. Create `ModifierResultBuilder.cs`

### Step 5: Write Tests

1. Start with unit tests for models
2. Add unit tests for modifiers
3. Add integration tests for service
4. Add E2E tests for complete flows

### Step 6: Configure Test Runner

1. Open Window → General → Test Runner
2. Create test configuration
3. Set up categories
4. Configure code coverage

## Test Execution Commands

```bash
# Run all tests
Unity -batchmode -runTests -projectPath . -testResults results.xml

# Run specific category
Unity -batchmode -runTests -projectPath . -testCategory Unit

# Run with coverage
Unity -batchmode -runTests -projectPath . -enableCodeCoverage -coverageResultsPath ./Coverage

# Generate coverage report
Unity -batchmode -runTests -projectPath . -enableCodeCoverage -coverageOptions generateHtmlReport
```

---

*Test Framework Version: 1.0.0*
*Last Updated: 2025-01-16*