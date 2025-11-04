# Changelog

All notable changes to the Dynamic User Difficulty package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- **Branch Migration** - Migrated default branch from `main` to `master` (2025-11-04)
  - Updated GitHub repository default branch setting
  - Created comprehensive migration documentation
  - Both branches remain available for backward compatibility
  - See [BRANCH_MIGRATION.md](Documentation/BRANCH_MIGRATION.md) for details

## [2.0.1] - 2025-10-10

### Fixed
- **Code Quality Improvements** - Resolved all code review findings
  - Fixed Runtime/Editor assembly separation (removed Odin Inspector from Runtime)
  - Added division-by-zero safety to all modifier config generation formulas
  - Expanded GameStats validation from 8 to 17 fields
  - Fixed indentation inconsistencies in modifier config files
  - Added missing `[CustomEditor]` attribute to DifficultyConfigEditor
  - Corrected LevelProgressConfig.cs structure and indentation
- **Compilation Errors** - Fixed all C# compilation errors
  - SessionPatternConfig.cs: Fixed indentation and missing namespace brace
  - LevelProgressConfig.cs: Fixed class body indentation and closing braces
  - LossStreakConfig.cs: Fixed indentation consistency
  - TimeDecayConfig.cs: Fixed indentation consistency
- **UI Functionality** - Generation buttons now visible in Unity Inspector

### Technical Details
- **Division-by-Zero Protection**: All 5 modifier configs now use `Mathf.Max()` guards
- **Comprehensive Validation**: All 17 GameStats fields validated before generation
- **Code Duplication**: Eliminated duplicate generation logic in preview
- **Formatting**: All files follow C# coding standards

## [2.0.0] - 2025-01-22

### 🎉 Major Release - Production Ready with 7 Comprehensive Modifiers

This is a major release that significantly expands the difficulty analysis capabilities while maintaining the stateless architecture. The system now provides comprehensive player behavior analysis through 7 specialized modifiers.

### Added
- **3 New Comprehensive Modifiers** - Expanding from 4 to 7 total modifiers
  - `CompletionRateModifier` - Analyzes overall player success rates
  - `LevelProgressModifier` - Tracks progression patterns and level attempts
  - `SessionPatternModifier` - Detects session behavior and engagement patterns
- **Complete Provider Method Utilization** - 21/21 provider methods now used (100% utilization)
- **Enhanced Test Suite** - 153 total tests with ~95% coverage
  - 34 new tests for the 3 new modifiers
  - Comprehensive provider method testing
  - Integration test scenarios covering all 7 modifiers
- **Detailed Analytics Support** - Rich metadata for all modifier calculations
- **Production-Ready Configuration** - Single ScriptableObject with embedded modifier configs

### Enhanced
- **Stateless Architecture** - Reinforced pure calculation engine design
- **Provider Interfaces** - Enhanced with additional methods for comprehensive analysis
  - `IWinStreakProvider` - Now supports total wins/losses for completion rate analysis
  - `ILevelProgressProvider` - Enhanced with level difficulty and completion rate methods
  - `IRageQuitProvider` - Added session pattern analysis methods
- **Documentation** - Complete overhaul with current implementation details
- **Test Framework** - Improved with shared mocks, categories, and better assertions

### Fixed
- **Version Consistency** - Updated package.json to reflect v2.0.0
- **Configuration Structure** - Corrected to use single ScriptableObject approach
- **Test Compilation** - Fixed all compilation errors and test failures
- **Provider Method Coverage** - Ensured all provider methods are utilized

### Changed
- **Version Bump** - From 1.0.0 to 2.0.0 to reflect major feature additions
- **Description Updates** - Enhanced package descriptions to reflect 7-modifier system
- **Keywords** - Added new keywords for better discoverability

### Technical Details

#### New Modifier Implementation Details

##### CompletionRateModifier
- **Purpose**: Analyzes overall player success rates to adjust difficulty
- **Provider Usage**: `IWinStreakProvider.GetTotalWins()`, `GetTotalLosses()`, `ILevelProgressProvider.GetCompletionRate()`
- **Algorithm**: Weighted average of overall and level-specific completion rates
- **Thresholds**: Low (<40%), Normal (40-70%), High (>70%)
- **Test Coverage**: 10 comprehensive tests

##### LevelProgressModifier
- **Purpose**: Tracks progression patterns, attempts, and completion times
- **Provider Usage**: `ILevelProgressProvider.GetAttemptsOnCurrentLevel()`, `GetAverageCompletionTime()`, `GetCurrentLevelDifficulty()`
- **Algorithm**: Multi-factor analysis of attempts, timing, and difficulty scaling
- **Features**: Fast completion bonus, attempt-based difficulty reduction
- **Test Coverage**: 12 comprehensive tests

##### SessionPatternModifier
- **Purpose**: Detects session behavior patterns and engagement levels
- **Provider Usage**: `IRageQuitProvider.GetAverageSessionDuration()`, `GetRecentRageQuitCount()`, `GetCurrentSessionDuration()`
- **Algorithm**: Pattern recognition for session duration and rage quit behavior
- **Features**: Engagement bonus for long sessions, frustration detection
- **Test Coverage**: 12 comprehensive tests

#### Provider Method Utilization
- **Total Methods**: 21 across 5 provider interfaces
- **Methods Used**: 21/21 (100% utilization) ✅
- **Coverage Distribution**:
  - `IWinStreakProvider`: 4/4 methods (100%)
  - `ITimeDecayProvider`: 3/3 methods (100%)
  - `IRageQuitProvider`: 4/4 methods (100%)
  - `ILevelProgressProvider`: 5/5 methods (100%)
  - `IDifficultyDataProvider`: 2/2 methods (100%)

#### Test Suite Improvements
- **Total Tests**: 164 tests across 12 test files
- **New Test Files**: 3 additional test suites for new modifiers
- **Test Categories**: Unit, Integration, Modifiers, NewModifiers
- **Coverage**: ~95% of core functionality
- **Performance**: Full suite runs in ~3 seconds

### Breaking Changes
- None - this release is fully backward compatible

### Migration Guide
- No migration required from v1.0.0
- New modifiers are automatically registered and activated
- Existing configurations continue to work unchanged
- New provider methods are optional - modifiers gracefully handle missing data

### Dependencies
- `com.theone.logging`: 1.0.6 (unchanged)
- Unity 2021.3+ (unchanged)
- VContainer 1.16.0+ (unchanged)

## [1.0.0] - 2024-12-15

### Added
- Initial release of Dynamic User Difficulty system
- Stateless calculation engine with provider pattern
- 4 core difficulty modifiers:
  - `WinStreakModifier` - Consecutive win detection
  - `LossStreakModifier` - Consecutive loss compensation
  - `TimeDecayModifier` - Returning player bonuses
  - `RageQuitModifier` - Frustration detection and adjustment
- Provider interfaces for external data integration
- VContainer dependency injection setup
- Basic test suite with ~85% coverage
- ScriptableObject configuration system
- Unity assembly definitions

### Technical Details
- Stateless architecture with no internal state storage
- Provider-based data access for clean separation of concerns
- Configurable difficulty ranges (1-10 scale)
- Performance optimized (<10ms calculations)
- Unity 2021.3+ compatibility

---

## Version Comparison

| Feature | v1.0.0 | v2.0.0 |
|---------|--------|--------|
| **Modifiers** | 4 basic | 7 comprehensive |
| **Provider Methods Used** | ~12/21 (57%) | 21/21 (100%) |
| **Test Coverage** | ~85% | ~95% |
| **Total Tests** | ~130 | 164 |
| **Behavioral Analysis** | Basic | Comprehensive |
| **Production Ready** | Beta | ✅ Production |

## Upcoming Releases

### [2.1.0] - Planned
- Machine learning predictions
- Advanced analytics integration
- Performance optimizations
- Cloud synchronization support

### [2.2.0] - Planned
- A/B testing framework
- Multi-factor analysis
- Enhanced visualization tools
- Additional provider interfaces

---

*For detailed documentation, see [README.md](README.md) and [Documentation/](Documentation/) folder.*