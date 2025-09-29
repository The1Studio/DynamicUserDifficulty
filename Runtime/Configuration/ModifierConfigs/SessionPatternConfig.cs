using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Session Pattern modifier with type-safe properties
    /// </summary>
    [Serializable]
public class SessionPatternConfig : BaseModifierConfig
{
    [Header("Session Duration Settings")]

    [SerializeField][Range(60f, 600f)]
    [Tooltip("Minimum session duration in seconds to be considered normal")]
    private float minNormalSessionDuration = 180f; // 3 minutes

    [SerializeField][Range(30f, 120f)]
    [Tooltip("Session duration below this is considered very short (potential frustration)")]
    private float veryShortSessionThreshold = 60f; // 1 minute

    [SerializeField][Range(0.2f, 1f)]
    [Tooltip("Difficulty decrease for very short sessions")]
    private float veryShortSessionDecrease = 0.5f;

    [Header("Session Pattern Detection")]

    [SerializeField][Range(3, 10)]
    [Tooltip("Number of recent sessions to analyze for patterns")]
    private int sessionHistorySize = 5;

    [SerializeField][Range(0.3f, 0.8f)]
    [Tooltip("Percentage of sessions that must be short to trigger adjustment")]
    private float shortSessionRatio = 0.5f;

    [SerializeField][Range(0.3f, 1.5f)]
    [Tooltip("Difficulty adjustment for consistent short sessions")]
    private float consistentShortSessionsDecrease = 0.8f;

    [Header("Session End Reason Analysis")]

    [SerializeField][Range(0.5f, 2f)]
    [Tooltip("Difficulty decrease for rage quit patterns")]
    private float rageQuitPatternDecrease = 1f;

    [SerializeField][Range(0.2f, 1f)]
    [Tooltip("Difficulty decrease for quitting mid-level frequently")]
    private float midLevelQuitDecrease = 0.4f;

    [SerializeField][Range(0.2f, 0.6f)]
    [Tooltip("Ratio of mid-level quits to trigger adjustment")]
    private float midLevelQuitRatio = 0.3f;

    [Header("Rage Quit Detection Settings")]

    [SerializeField][Range(10f, 60f)]
    [Tooltip("Time threshold for detecting rage quits in session analysis (seconds)")]
    private float rageQuitTimeThreshold = 30f;

    [SerializeField][Range(1, 5)]
    [Tooltip("Minimum number of rage quits to trigger pattern detection")]
    private int rageQuitCountThreshold = 2;

    [SerializeField][Range(0.1f, 1f)]
    [Tooltip("Multiplier for recent rage quit penalty")]
    private float rageQuitPenaltyMultiplier = 0.5f;

    [Header("Difficulty Adjustment Analysis")]

    [SerializeField][Range(1.1f, 2f)]
    [Tooltip("Improvement ratio to consider difficulty adjustments effective")]
    private float difficultyImprovementThreshold = 1.2f;

    // BaseModifierConfig implementation
    public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_SESSION_PATTERN;

    // Type-safe properties
    public float MinNormalSessionDuration => this.minNormalSessionDuration;
    public float VeryShortSessionThreshold => this.veryShortSessionThreshold;
    public float VeryShortSessionDecrease => this.veryShortSessionDecrease;
    public int SessionHistorySize => this.sessionHistorySize;
    public float ShortSessionRatio => this.shortSessionRatio;
    public float ConsistentShortSessionsDecrease => this.consistentShortSessionsDecrease;
    public float RageQuitPatternDecrease => this.rageQuitPatternDecrease;
    public float MidLevelQuitDecrease => this.midLevelQuitDecrease;
    public float MidLevelQuitRatio => this.midLevelQuitRatio;
    public float RageQuitTimeThreshold => this.rageQuitTimeThreshold;
    public int RageQuitCountThreshold => this.rageQuitCountThreshold;
    public float RageQuitPenaltyMultiplier => this.rageQuitPenaltyMultiplier;
    public float DifficultyImprovementThreshold => this.difficultyImprovementThreshold;

    public override IModifierConfig CreateDefault()
    {
        var config = new SessionPatternConfig
        {
            minNormalSessionDuration = 180f,
            veryShortSessionThreshold = 60f,
            veryShortSessionDecrease = 0.5f,
            sessionHistorySize = 5,
            shortSessionRatio = 0.5f,
            consistentShortSessionsDecrease = 0.8f,
            rageQuitPatternDecrease = 1f,
            midLevelQuitDecrease = 0.4f,
            midLevelQuitRatio = 0.3f,
            rageQuitTimeThreshold = 30f,
            rageQuitCountThreshold = 2,
            rageQuitPenaltyMultiplier = 0.5f,
            difficultyImprovementThreshold = 1.2f,
        };
        config.SetEnabled(true);
        config.SetPriority(7);
        return config;
    }
}
}