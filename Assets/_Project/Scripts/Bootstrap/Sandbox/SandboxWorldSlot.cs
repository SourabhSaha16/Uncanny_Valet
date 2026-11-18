using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// The inactive template of one sandbox world: a root holding the lot builder and the decor, both set up for its lot definition by the
    /// scene builder. The sandbox clones it to show the world, so the lot builds from scratch (mud, props, lighting) every time.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SandboxWorldSlot : MonoBehaviour
    {
        [SerializeField] private LotDefinition _definition;
        [SerializeField] private LotLayoutBuilder _lot;
        [SerializeField] private LotDecor _decor;

        public LotDefinition Definition => _definition;
        public LotLayoutBuilder Lot => _lot;
        public LotDecor Decor => _decor;
    }
}
