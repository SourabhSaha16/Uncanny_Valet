using System;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Core.Modifiers
{
    /// <summary>
    /// One kind of service a car can need (Muddy, NeedsCharge, later Snowy and Overheating): what clears it, how long
    /// that takes, what it pays, how it looks and how often it arrives. Read-only at runtime. Dynamic modifiers add a
    /// behaviour class on top of this data.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Service Modifier", fileName = "ServiceModifier")]
    public sealed class ServiceModifierDefinition : ScriptableObject
    {
        /// <summary>The numbers of a definition built in code (tests and the setup tool).</summary>
        [Serializable]
        public struct Values
        {
            public ModifierId Id;
            public string DisplayName;
            public StationKind ClearedAt;
            public float ClearMinutes;
            public float StartProgress01;
            public int Bonus;
            public string ActiveWord;
            public string DoneWord;
            public ModifierIcon Icon;
            public ModifierLook Look;
            public bool RampsLate;
            public TuningKey ClearMinutesKey;
            public TuningKey BonusKey;
            public TuningKey ChanceKey;
        }

        [SerializeField] private ModifierId _id;
        [SerializeField, Tooltip("Plain-language name shown in tools and on the F2 panel.")]
        private string _displayName;
        [SerializeField, Tooltip("The kind of station that clears this modifier.")]
        private StationKind _clearedAt;
        [SerializeField, Min(1f), Tooltip("Game minutes in the station to go from the starting progress to cleared.")]
        private float _clearMinutes = 15f;
        [SerializeField, Range(0f, 0.99f), Tooltip("Progress 0 to 1 the car arrives with (a car that arrives 20% charged starts at 0.2).")]
        private float _startProgress01;
        [SerializeField, Min(0), Tooltip("Dollars added to the tip for clearing this modifier (before the combo).")]
        private int _bonus = 15;
        [SerializeField, Tooltip("Status word while the station works on it, for example Washing.")]
        private string _activeWord;
        [SerializeField, Tooltip("Status word once it is cleared, for example Washed.")]
        private string _doneWord;
        [SerializeField, Tooltip("The small icon drawn next to the plate while the need is open.")]
        private ModifierIcon _icon;
        [SerializeField, Tooltip("How the modifier shows on the car sprite.")]
        private ModifierLook _look;
        [SerializeField, Tooltip("When on, the arrival chance ramps toward the plan's late chance at the end of the shift.")]
        private bool _rampsLate;
        [SerializeField, Tooltip("The live key holding the clear time in game minutes (F2 panel).")]
        private TuningKey _clearMinutesKey;
        [SerializeField, Tooltip("The live key holding the clear bonus (F2 panel).")]
        private TuningKey _bonusKey;
        [SerializeField, Tooltip("The live key holding the arrival chance; the lot seeds it.")]
        private TuningKey _chanceKey;

        public ModifierId Id => _id;
        public string DisplayName => _displayName;
        public StationKind ClearedAt => _clearedAt;
        public float ClearMinutes => _clearMinutes;
        public float StartProgress01 => _startProgress01;
        public int Bonus => _bonus;
        public string ActiveWord => _activeWord;
        public string DoneWord => _doneWord;
        public ModifierIcon Icon => _icon;
        public ModifierLook Look => _look;
        public bool RampsLate => _rampsLate;
        public TuningKey ClearMinutesKey => _clearMinutesKey;
        public TuningKey BonusKey => _bonusKey;
        public TuningKey ChanceKey => _chanceKey;

        /// <summary>Builds an unsaved definition from <paramref name="values"/> (tests and the setup tool).</summary>
        public static ServiceModifierDefinition Create(in Values values)
        {
            var d = CreateInstance<ServiceModifierDefinition>();
            d.hideFlags = HideFlags.HideAndDontSave;
            d.name = values.DisplayName;
            d._id = values.Id; d._displayName = values.DisplayName; d._clearedAt = values.ClearedAt;
            d._clearMinutes = values.ClearMinutes; d._startProgress01 = values.StartProgress01; d._bonus = values.Bonus;
            d._activeWord = values.ActiveWord; d._doneWord = values.DoneWord; d._icon = values.Icon; d._look = values.Look;
            d._rampsLate = values.RampsLate; d._clearMinutesKey = values.ClearMinutesKey;
            d._bonusKey = values.BonusKey; d._chanceKey = values.ChanceKey;
            return d;
        }

        private void OnValidate()
        {
            if (_id == ModifierId.None) Debug.LogError("ServiceModifierDefinition " + name + " has no id.", this);
        }
    }
}
