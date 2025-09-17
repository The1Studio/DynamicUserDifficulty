using System;
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Complete result of difficulty calculation
    /// </summary>
    public class DifficultyResult
    {
        public float PreviousDifficulty { get; set; }
        public float NewDifficulty { get; set; }
        public List<ModifierResult> AppliedModifiers { get; set; }
        public DateTime CalculatedAt { get; set; }
        public string PrimaryReason { get; set; }

        public DifficultyResult()
        {
            AppliedModifiers = new List<ModifierResult>();
            CalculatedAt = DateTime.Now;
        }

        public DifficultyResult(float previous, float newDiff, List<ModifierResult> this.modifiers)
        {
            PreviousDifficulty = previous;
            NewDifficulty = newDiff;
            AppliedModifiers = this.modifiers ?? new List<ModifierResult>();
            CalculatedAt = DateTime.Now;
            PrimaryReason = DeterminePrimaryReason();
        }

        private string DeterminePrimaryReason()
        {
            if (AppliedModifiers == null || AppliedModifiers.Count == 0)
                return "No change";

            var primaryModifier = AppliedModifiers
                .OrderByDescending(m => Math.Abs(m.Value))
                .FirstOrDefault();

            return primaryModifier?.Reason ?? "No change";
        }

        public float TotalAdjustment => NewDifficulty - PreviousDifficulty;

        public bool HasChanged => Math.Abs(TotalAdjustment) > DifficultyConstants.EPSILON;
    }
}