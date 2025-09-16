using System.Collections.Generic;

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
            Metadata = new Dictionary<string, object>();
        }

        public ModifierResult(string name, float value, string reason)
        {
            ModifierName = name;
            Value = value;
            Reason = reason;
            Metadata = new Dictionary<string, object>();
        }

        public static ModifierResult NoChange()
        {
            return new ModifierResult("NoChange", 0f, "No change required");
        }
    }
}