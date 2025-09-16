# Dynamic User Difficulty - Test Strategy

## Executive Summary

This document outlines the comprehensive testing strategy for the Dynamic User Difficulty system, ensuring high quality, reliability, and maintainability through systematic testing approaches.

## Test Philosophy

### Core Principles
1. **Test Pyramid**: More unit tests, fewer integration tests, minimal E2E tests
2. **Fast Feedback**: Quick test execution for rapid development cycles
3. **Isolation**: Tests should not depend on external systems or state
4. **Clarity**: Tests serve as documentation of expected behavior
5. **Coverage**: Critical paths must have 100% coverage

## Testing Layers

### Layer 1: Unit Tests (70% of tests)
**Purpose**: Test individual components in isolation
**Execution Time**: < 10ms per test
**Dependencies**: All mocked
**Location**: `Tests/Runtime/Unit/`

#### What to Test
- Individual modifier calculations
- Data model behavior
- Calculator logic
- Utility functions
- Edge cases and boundaries

#### Example Test Cases
```
WinStreakModifier:
✓ No streak returns zero adjustment
✓ Below threshold returns zero
✓ At threshold returns step size
✓ Above threshold scales linearly
✓ Respects maximum bonus limit
✓ Applies response curve correctly
```

### Layer 2: Integration Tests (20% of tests)
**Purpose**: Test component interactions
**Execution Time**: < 100ms per test
**Dependencies**: Partially mocked
**Location**: `Tests/Runtime/Integration/`

#### What to Test
- Service with real calculators
- Modifier pipeline processing
- Data persistence flow
- DI container configuration
- Signal subscriptions

#### Example Test Cases
```
DifficultyService Integration:
✓ Calculates with multiple modifiers
✓ Persists data correctly
✓ Applies configuration limits
✓ Handles modifier registration
✓ Processes session lifecycle
```

### Layer 3: End-to-End Tests (10% of tests)
**Purpose**: Test complete user scenarios
**Execution Time**: < 1s per test
**Dependencies**: None mocked
**Location**: `Tests/Runtime/EndToEnd/`

#### What to Test
- Complete player journeys
- System behavior over time
- Real configuration loading
- Performance characteristics
- Memory management

#### Example Test Cases
```
Player Journey:
✓ New player onboarding flow
✓ Winning streak progression
✓ Losing streak assistance
✓ Returning player adjustment
✓ Rage quit detection and recovery
```

## Test Coverage Strategy

### Coverage Targets by Component

| Component | Line Coverage | Branch Coverage | Method Coverage |
|-----------|--------------|-----------------|-----------------|
| **Critical Path** |
| DynamicDifficultyService | 95% | 90% | 100% |
| All Modifiers | 95% | 90% | 100% |
| DifficultyCalculator | 95% | 90% | 100% |
| **Core Components** |
| SessionDataProvider | 90% | 85% | 95% |
| ModifierAggregator | 90% | 85% | 95% |
| DI Module | 85% | 80% | 90% |
| **Supporting Components** |
| Models | 80% | 75% | 85% |
| Configuration | 75% | 70% | 80% |
| **Overall Target** | **90%** | **85%** | **95%** |

### Critical Paths (100% Coverage Required)

1. **Difficulty Calculation Pipeline**
   ```
   CalculateDifficulty() → Calculate() → Aggregate() → ApplyDifficulty()
   ```

2. **Session Management**
   ```
   OnLevelStart() → OnLevelComplete() → RecordWin/Loss() → SaveSession()
   ```

3. **Modifier Execution**
   ```
   RegisterModifier() → Calculate() → OnApplied()
   ```

## Test Data Strategy

### Test Data Categories

#### 1. Boundary Data
```csharp
// Minimum values
MinDifficulty = 1f
MinWinStreak = 0
MinSessionDuration = 0f

// Maximum values
MaxDifficulty = 10f
MaxWinStreak = int.MaxValue
MaxSessionDuration = float.MaxValue

// Edge cases
JustBelowThreshold = threshold - 0.01f
ExactlyAtThreshold = threshold
JustAboveThreshold = threshold + 0.01f
```

#### 2. Typical Data
```csharp
// Average player
WinStreak = 2-3
LossStreak = 1-2
SessionDuration = 60-180 seconds
DaysAway = 1-3

// Good player
WinStreak = 5-7
LossStreak = 0
SessionDuration = 30-60 seconds
```

#### 3. Extreme Data
```csharp
// Exceptional cases
WinStreak = 100
TimeSincePlay = 365 days
SessionDuration = 3600 seconds
RageQuitTime = 5 seconds
```

### Test Data Generation

```csharp
public static class TestDataGenerator
{
    public static IEnumerable<PlayerSessionData> GenerateRandomSessions(int count)
    {
        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            yield return new PlayerSessionData
            {
                CurrentDifficulty = random.Next(1, 11),
                WinStreak = random.Next(0, 10),
                LossStreak = random.Next(0, 5),
                LastPlayTime = DateTime.Now.AddHours(-random.Next(0, 168))
            };
        }
    }

    public static IEnumerable<TestCaseData> BoundaryTestCases()
    {
        yield return new TestCaseData(0f).Returns(1f).SetName("MinimumBoundary");
        yield return new TestCaseData(11f).Returns(10f).SetName("MaximumBoundary");
        yield return new TestCaseData(5.5f).Returns(5.5f).SetName("MidPoint");
    }
}
```

## Test Execution Plan

### Continuous Integration Pipeline

```yaml
stages:
  - name: "Quick Validation"
    parallel: true
    tests:
      - category: "Unit"
      - category: "Smoke"
    timeout: 2m

  - name: "Full Validation"
    parallel: false
    tests:
      - category: "Integration"
      - category: "Regression"
    timeout: 5m

  - name: "Extended Validation"
    parallel: false
    tests:
      - category: "E2E"
      - category: "Performance"
    timeout: 10m
    when: ["main", "release/*"]
```

### Local Development Workflow

```bash
# Before commit - run unit tests
npm run test:unit

# Before push - run unit + integration
npm run test:integration

# Before merge - run full suite
npm run test:all

# With coverage
npm run test:coverage
```

### Test Categorization

| Category | Description | When to Run |
|----------|-------------|-------------|
| `[Category("Unit")]` | Fast, isolated tests | Every build |
| `[Category("Integration")]` | Component interaction | Before push |
| `[Category("E2E")]` | Full scenarios | Before merge |
| `[Category("Smoke")]` | Critical path validation | Every build |
| `[Category("Regression")]` | Bug prevention | Daily |
| `[Category("Performance")]` | Speed/memory tests | Weekly |
| `[Category("Slow")]` | Tests > 1s | Nightly |

## Mock Strategy

### When to Mock

```
┌─────────────────────────────────────────┐
│         External Systems                 │ ← Always Mock
├─────────────────────────────────────────┤
│         I/O Operations                   │ ← Always Mock
├─────────────────────────────────────────┤
│         Complex Dependencies             │ ← Usually Mock
├─────────────────────────────────────────┤
│         Simple Data Objects              │ ← Use Real
├─────────────────────────────────────────┤
│         Value Objects                    │ ← Use Real
└─────────────────────────────────────────┘
```

### Mock Types

#### 1. Behavior Verification Mocks
```csharp
// Verify method was called
mockProvider.VerifyMethodCalled("SaveSession", times: 2);
```

#### 2. State Verification Stubs
```csharp
// Check final state
Assert.AreEqual(expectedData, mockProvider.GetCurrentSession());
```

#### 3. Test Doubles
```csharp
// Simplified implementation for testing
public class TestCalculator : IDifficultyCalculator
{
    public DifficultyResult Calculate(...) => TestResult;
}
```

## Performance Testing

### Performance Benchmarks

| Operation | Target | Maximum | Test Method |
|-----------|--------|---------|-------------|
| Calculate() | 5ms | 10ms | Stopwatch |
| ApplyDifficulty() | 2ms | 5ms | Stopwatch |
| SaveSession() | 10ms | 20ms | Stopwatch |
| Full Pipeline | 20ms | 50ms | Profiler |

### Performance Test Example

```csharp
[Test]
[Category("Performance")]
public void CalculateDifficulty_WithAllModifiers_CompletesQuickly()
{
    // Arrange
    var service = CreateServiceWithAllModifiers();
    var stopwatch = Stopwatch.StartNew();

    // Act
    for (int i = 0; i < 100; i++)
    {
        service.CalculateDifficulty();
    }
    stopwatch.Stop();

    // Assert
    var avgTime = stopwatch.ElapsedMilliseconds / 100.0;
    Assert.Less(avgTime, 10, $"Average time {avgTime}ms exceeds 10ms limit");
}
```

## Error Testing

### Error Scenarios to Test

1. **Null Inputs**
   - Null session data
   - Null configuration
   - Null modifiers

2. **Invalid Data**
   - Negative difficulties
   - Out of range values
   - Corrupted save data

3. **System Failures**
   - Save operation fails
   - Configuration missing
   - DI registration fails

### Error Test Example

```csharp
[Test]
public void CalculateDifficulty_WithNullData_HandlesGracefully()
{
    // Arrange
    mockProvider.SetMockData(null);

    // Act & Assert
    Assert.DoesNotThrow(() => service.CalculateDifficulty());
    var result = service.CalculateDifficulty();
    Assert.AreEqual(defaultDifficulty, result.NewDifficulty);
}
```

## Test Maintenance

### Test Review Checklist

- [ ] Test name clearly describes what is being tested
- [ ] Single assertion per test (or Assert.Multiple)
- [ ] No magic numbers - use constants
- [ ] Proper setup and teardown
- [ ] No test interdependencies
- [ ] Fast execution time
- [ ] Clear failure messages

### Test Refactoring Guidelines

1. **Extract Common Setup**
   ```csharp
   // Before
   var config = new Config();
   config.Value = 5;

   // After
   var config = CreateDefaultConfig();
   ```

2. **Use Builder Pattern**
   ```csharp
   // Before
   var data = new PlayerSessionData();
   data.WinStreak = 5;
   data.Difficulty = 3;

   // After
   var data = SessionDataBuilder.Create()
       .WithWinStreak(5)
       .WithDifficulty(3)
       .Build();
   ```

3. **Create Test DSL**
   ```csharp
   // Domain-specific test language
   GivenPlayerHasWinStreak(5);
   WhenDifficultyIsCalculated();
   ThenDifficultyShouldIncrease();
   ```

## Test Reporting

### Metrics to Track

1. **Coverage Metrics**
   - Line coverage %
   - Branch coverage %
   - Method coverage %

2. **Quality Metrics**
   - Test execution time
   - Test flakiness rate
   - Test failure rate

3. **Maintenance Metrics**
   - Tests added/modified per feature
   - Test-to-code ratio
   - Average test complexity

### Test Report Format

```xml
<testsuites>
  <testsuite name="DynamicUserDifficulty.Tests">
    <properties>
      <property name="coverage.lines" value="92%"/>
      <property name="coverage.branches" value="87%"/>
    </properties>
    <testcase name="Calculate_WithWinStreak_IncreasesDifficulty" time="0.005">
      <passed/>
    </testcase>
  </testsuite>
</testsuites>
```

## Troubleshooting Tests

### Common Issues and Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Flaky tests | Time dependencies | Use fixed time in tests |
| Slow tests | Real I/O operations | Mock external dependencies |
| Test pollution | Shared state | Proper setup/teardown |
| Hard to debug | Complex setup | Simplify, extract helpers |
| Brittle tests | Implementation coupling | Test behavior, not implementation |

### Debug Techniques

1. **Isolate Failing Test**
   ```bash
   Unity -runTests -testFilter "SpecificTestName"
   ```

2. **Add Diagnostic Output**
   ```csharp
   Debug.Log($"State before: {JsonConvert.SerializeObject(data)}");
   ```

3. **Use Test Replay**
   ```csharp
   [Test]
   [Repeat(100)] // Find intermittent failures
   public void PotentiallyFlakyTest() { }
   ```

## Conclusion

This comprehensive test strategy ensures the Dynamic User Difficulty system maintains high quality through:
- Systematic testing at multiple levels
- Clear coverage targets
- Well-defined test data strategies
- Efficient execution plans
- Proper mock usage
- Performance validation
- Error handling verification

Regular review and maintenance of tests ensures they continue to provide value and confidence in the system's correctness.

---

*Test Strategy Version: 1.0.0*
*Last Updated: 2025-01-16*