using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Finds the nearest usable IInteractable in range and uses it on the interact key.</summary>
    public sealed class Interactor : MonoBehaviour
    {
        [SerializeField] private float _range = 2f;

        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly List<IInteractable> _found = new List<IInteractable>(16);
        private readonly List<Vector2> _positions = new List<Vector2>(16);
        private IInputSource _input;

        /// <summary>Prompt for the debug overlay, or empty when nothing is in range.</summary>
        public string CurrentPrompt { get; private set; } = string.Empty;

        public void SetInput(IInputSource input) => _input = input;

        private void Update()
        {
            var target = FindNearest();
            CurrentPrompt = target != null ? target.Prompt : string.Empty;
            if (target != null && _input != null && _input.InteractPressed) target.Interact(gameObject);
        }

        private void OnDisable() => CurrentPrompt = string.Empty;

        private IInteractable FindNearest()
        {
            _found.Clear();
            _positions.Clear();
            var count = Physics2D.OverlapCircleNonAlloc(transform.position, _range, _hits);
            for (var i = 0; i < count; i++)
            {
                var item = _hits[i].GetComponentInParent<IInteractable>();
                if (item == null || _found.Contains(item) || !item.CanInteract(gameObject)) continue;
                _found.Add(item);
                _positions.Add(item.Position);
            }
            var index = NearestPicker.Pick(transform.position, _positions, _range);
            return index >= 0 ? _found[index] : null;
        }
    }
}
