using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Result from a single modifier calculation
    /// </summary>
    public class ModifierResult
    {
        public string ModifierName { get; set; }
        public float Value { get; set; }
        public string Reason { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public ModifierResult()
        {
            this.Metadata = new();
        }

        public ModifierResult(string name, float value, string reason)
        {
            this.ModifierName = name;
            this.Value        = value;
            this.Reason       = reason;
            this.Metadata        = new();
        }

        public static ModifierResult NoChange()
        {
            return new("NoChange", DifficultyConstants.ZERO_VALUE, "No change required");
        }
    }
}