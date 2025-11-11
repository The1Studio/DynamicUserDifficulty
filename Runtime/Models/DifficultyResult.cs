#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Complete result of difficulty calculation
    /// </summary>
    public sealed class DifficultyResult
    {
        public float PreviousDifficulty { get; set; }
        public float NewDifficulty { get; set; }
        public List<ModifierResult> AppliedModifiers { get; set; }
        public DateTime CalculatedAt { get; set; }
        public string PrimaryReason { get; set; }

        public DifficultyResult()
        {
            this.AppliedModifiers = new();
            this.CalculatedAt     = DateTime.Now;
        }

        public DifficultyResult(float previous, float newDiff, List<ModifierResult> modifiers)
        {
            this.PreviousDifficulty = previous;
            this.NewDifficulty      = newDiff;
            this.AppliedModifiers   = modifiers ?? new List<ModifierResult>();
            this.CalculatedAt       = DateTime.Now;
            this.PrimaryReason      = this.DeterminePrimaryReason();
        }

        private string DeterminePrimaryReason()
        {
            if (this.AppliedModifiers == null || this.AppliedModifiers.Count == 0)
                return "No change";

            var primaryModifier = this.AppliedModifiers
                .OrderByDescending(m => Math.Abs(m.Value))
                .FirstOrDefault();

            return primaryModifier?.Reason ?? "No change";
        }

        public float TotalAdjustment => this.NewDifficulty - this.PreviousDifficulty;

        public bool HasChanged => Math.Abs(this.TotalAdjustment) > DifficultyConstants.EPSILON;
    }
}