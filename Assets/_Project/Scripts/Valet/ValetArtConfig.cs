using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// The valet's layered sprites (body, outfit, hair; 24 columns each, same layout) and the animation numbers.
    /// Sheet layout: 6-frame run cycles for Down, then a side view, then Up, then the other side view.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Valet Art Config", fileName = "ValetArtConfig")]
    public sealed class ValetArtConfig : ScriptableObject
    {
        [SerializeField] private Sprite[] _model = new Sprite[0];
        [SerializeField] private Sprite[] _outfit = new Sprite[0];
        [SerializeField] private Sprite[] _hair = new Sprite[0];
        [Header("Animation")]
        [SerializeField, Min(1)] private int _framesPerGroup = 6;
        [SerializeField, Min(1f), Tooltip("Run-cycle frames per second.")] private float _framesPerSecond = 10f;
        [SerializeField, Min(0), Tooltip("Frame of the facing group shown when standing still.")] private int _idleFrame;
        [SerializeField, Min(0f), Tooltip("Speed (units/s) under which the valet counts as standing still.")]
        private float _moveThreshold = 0.1f;
        [Header("Sheet columns")]
        [SerializeField, Min(0)] private int _downStart;
        [SerializeField, Min(0)] private int _rightStart = 6;
        [SerializeField, Min(0)] private int _upStart = 12;
        [SerializeField, Min(0)] private int _leftStart = 18;
        [SerializeField, Tooltip("Mirror the Right group (use if it shows the valet facing left).")] private bool _flipRight;
        [SerializeField, Tooltip("Mirror the Left group (use if it shows the valet facing right).")] private bool _flipLeft;

        public int FramesPerGroup => _framesPerGroup;
        public float FramesPerSecond => _framesPerSecond;
        public int IdleFrame => _idleFrame;
        public float MoveThreshold => _moveThreshold;

        /// <summary>True when every layer has the same non-zero number of sprites.</summary>
        public bool IsComplete => _model.Length > 0 && _model.Length == _outfit.Length && _model.Length == _hair.Length;

        public Sprite Model(int column) => _model[column];
        public Sprite Outfit(int column) => _outfit[column];
        public Sprite Hair(int column) => _hair[column];

        /// <summary>First sheet column of the run cycle for a facing.</summary>
        public int GroupStart(ValetFacing facing)
        {
            switch (facing)
            {
                case ValetFacing.Right: return _rightStart;
                case ValetFacing.Up: return _upStart;
                case ValetFacing.Left: return _leftStart;
                default: return _downStart;
            }
        }

        /// <summary>True when the sprites of this facing must be mirrored.</summary>
        public bool Flip(ValetFacing facing)
            => (facing == ValetFacing.Right && _flipRight) || (facing == ValetFacing.Left && _flipLeft);
    }
}
