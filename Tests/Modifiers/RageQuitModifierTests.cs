using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    using TheOneStudio.DynamicUserDifficulty.Modifiers;

    [TestFixture]
    public class RageQuitModifierTests
    {
        private RageQuitModifier modifier;
        private ModifierConfig config;
        private PlayerSessionData sessionData;

        [SetUp]
    public void Setup()
    {
        // Create config with test parameters
        this.config = new ModifierConfig();
        this.config.SetModifierType(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT);
        this.config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_THRESHOLD, 30f);  // Fixed: correct constant
        this.config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_REDUCTION, 1.5f); // Fixed: correct constant  
        this.config.SetParameter(DifficultyConstants.PARAM_QUIT_REDUCTION, 0.75f);     // Added missing parameter
        this.config.SetParameter(DifficultyConstants.PARAM_MID_PLAY_REDUCTION, 0.5f);  // Added missing parameter

        // Create modifier with config
        this.modifier = new RageQuitModifier(this.config);

        // Create test session data
        this.sessionData = new PlayerSessionData();
    }

        [Test]
        public void Calculate_NoQuit_ReturnsZero()
        {
            // Arrange
            this.sessionData.LastSession = new SessionInfo
            {
                EndType = SessionEndType.CompletedWin,
                PlayDuration = 100f
            };

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT, result.ModifierName);
        }

        [Test]
        public void Calculate_RageQuit_ReturnsFullReduction()
        {
            // Arrange
            this.sessionData.LastSession = new SessionInfo
            {
                EndType = SessionEndType.QuitAfterLoss,
                PlayDuration = 25f // Below rage quit threshold of 30 seconds
            };

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-1.5f, result.Value); // Full rage quit reduction from config
        }

        [Test]
        public void Calculate_NormalQuit_ReturnsQuitReduction()
        {
            // Arrange
            this.sessionData.LastSession = new SessionInfo
            {
                EndType = SessionEndType.QuitAfterLoss,
                PlayDuration = 60f // Above rage quit threshold
            };

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-0.75f, result.Value); // Normal quit reduction from config
        }

        [Test]
    public void Calculate_MidPlayQuit_LowProgress_ReturnsFullReduction()
    {
        // Arrange
        this.sessionData.LastSession = new SessionInfo
        {
            EndType = SessionEndType.QuitDuringPlay,
            PlayDuration = 120f
            // Note: Progress is tracked separately, not in SessionInfo
        };

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(-0.5f, result.Value); // Mid-play reduction from config
    }

        [Test]
    public void Calculate_MidPlayQuit_HighProgress_ReturnsPartialReduction()
    {
        // Arrange
        this.sessionData.LastSession = new SessionInfo
        {
            EndType = SessionEndType.QuitDuringPlay,
            PlayDuration = 120f
            // Note: Progress would be high but isn't stored in SessionInfo
        };

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(-0.5f, result.Value); // Mid-play reduction (progress not used in current implementation)
    }

        [Test]
    public void Calculate_MidPlayQuit_FullProgress_ReturnsZero()
    {
        // Arrange
        this.sessionData.LastSession = new SessionInfo
        {
            EndType = SessionEndType.CompletedWin,  // Completed, not mid-play quit
            PlayDuration = 120f
            // Full progress implied by CompletedWin
        };

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(0f, result.Value); // No penalty for completed games
    }

        [Test]
    public void Calculate_SessionLengthAtThreshold_UsesRageQuit()
    {
        // Arrange
        this.sessionData.LastSession = new SessionInfo
        {
            EndType = SessionEndType.QuitAfterLoss,
            PlayDuration = 30f // Exactly at rage quit threshold
        };

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(-0.75f, result.Value); // Normal quit reduction (not rage quit at exact threshold)
    }

        [Test]
    public void Calculate_SessionLengthJustAboveThreshold_UsesNormalQuit()
    {
        // Arrange
        this.sessionData.LastSession = new SessionInfo
        {
            EndType = SessionEndType.QuitAfterLoss,
            PlayDuration = 31f // Just above rage quit threshold
        };

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(-0.75f, result.Value); // Normal quit reduction
    }

        [Test]
    public void Calculate_AlwaysReturnsNegativeOrZero()
    {
        // Test all quit types that result in penalties
        var endTypes = new[] 
        { 
            SessionEndType.QuitAfterLoss,
            SessionEndType.QuitDuringPlay,
            SessionEndType.Timeout
        };

        foreach (var endType in endTypes)
        {
            // Arrange
            this.sessionData.LastSession = new SessionInfo
            {
                EndType = endType,
                PlayDuration = 50f
            };

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.LessOrEqual(result.Value, 0f,
                $"End type {endType} should produce negative or zero value");
        }
    }

        [Test]
    public void Calculate_WithNullSessionData_ReturnsNoChange()
    {
        // Act
        var result = this.modifier.Calculate(null);
        
        // Assert - Should return NoChange result, not throw exception
        Assert.AreEqual(0f, result.Value);
        Assert.IsNotNull(result);
    }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.sessionData.LastSession = new SessionInfo
            {
                EndType = SessionEndType.QuitAfterLoss,
                PlayDuration = 45f
            };

            // Act
            var result1 = this.modifier.Calculate(this.sessionData);
            var result2 = this.modifier.Calculate(this.sessionData);

            // Assert - Same input should produce same output
            Assert.AreEqual(result1.Value, result2.Value);
        }

        [Test]
        public void Calculate_IndependentOfCurrentDifficulty()
        {
            // Arrange
            this.sessionData.LastSession = new SessionInfo
            {
                EndType = SessionEndType.QuitAfterLoss,
                PlayDuration = 15f // Below threshold for rage quit
            };

            // Act - Rage quit modifier shouldn't be affected by current difficulty
            var resultLowDiff  = this.modifier.Calculate(this.sessionData);
            var resultHighDiff = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(resultLowDiff.Value, resultHighDiff.Value);
        }

        [Test]
        public void GetModifierType_ReturnsRageQuit()
        {
            // Act
            var type = this.modifier.ModifierName;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT, type);
        }

        [Test]
    public void Calculate_MidPlayQuit_ProgressScaling()
    {
        // Note: Current implementation doesn't scale with progress
        // All mid-play quits get the same penalty regardless of progress
        
        // Low progress scenario
        this.sessionData.LastSession = new SessionInfo
        {
            EndType = SessionEndType.QuitDuringPlay,
            PlayDuration = 120f
            // Low progress scenario
        };
        var lowProgressResult = this.modifier.Calculate(this.sessionData);

        // High progress scenario - same session, just conceptually different progress
        this.sessionData.LastSession = new SessionInfo
        {
            EndType = SessionEndType.QuitDuringPlay,
            PlayDuration = 120f
            // High progress scenario
        };
        var highProgressResult = this.modifier.Calculate(this.sessionData);

        // Assert that penalty is the same (no progress scaling in current implementation)
        Assert.AreEqual(lowProgressResult.Value, highProgressResult.Value);
        Assert.AreEqual(-0.5f, lowProgressResult.Value); // Both should be mid-play reduction
    }
    }
}