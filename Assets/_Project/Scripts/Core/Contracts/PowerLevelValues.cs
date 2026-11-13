using System;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The numbers of one level of one power. A power ignores the fields it does not use.</summary>
    [Serializable]
    public struct PowerLevelValues
    {
        /// <summary>Seconds before the power can be used again, counted after its use ends.</summary>
        public float Cooldown;
        /// <summary>Seconds the power lasts; 0 for an instant power.</summary>
        public float Duration;
        /// <summary>Distance or range in world units.</summary>
        public float Distance;
        /// <summary>How hard the power pushes things.</summary>
        public float PushStrength;
        /// <summary>Width of the power's reach in world units.</summary>
        public float Width;
        /// <summary>A second time or number a power needs (Thunder stride: the on-foot dash time in seconds).</summary>
        public float Secondary;

        public PowerLevelValues(float cooldown, float duration, float distance = 0f, float pushStrength = 0f, float width = 0f, float secondary = 0f)
        {
            Cooldown = cooldown; Duration = duration; Distance = distance; PushStrength = pushStrength; Width = width; Secondary = secondary;
        }

        /// <summary>The value of a field.</summary>
        public float Get(PowerField field)
        {
            switch (field)
            {
                case PowerField.Cooldown: return Cooldown;
                case PowerField.Duration: return Duration;
                case PowerField.Distance: return Distance;
                case PowerField.PushStrength: return PushStrength;
                case PowerField.Width: return Width;
                default: return Secondary;
            }
        }

        /// <summary>Sets the value of a field.</summary>
        public void Set(PowerField field, float value)
        {
            switch (field)
            {
                case PowerField.Cooldown: Cooldown = value; break;
                case PowerField.Duration: Duration = value; break;
                case PowerField.Distance: Distance = value; break;
                case PowerField.PushStrength: PushStrength = value; break;
                case PowerField.Width: Width = value; break;
                default: Secondary = value; break;
            }
        }
    }
}
