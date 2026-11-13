using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UncannyValet.Run
{
    /// <summary>
    /// The one object that outlives a lot scene (DontDestroyOnLoad). It owns the <see cref="RunProgress"/> for the
    /// browser session and loads lot scenes. The first lot scene creates it; later scenes find it, so the run state
    /// survives every scene load. Lost on a page reload by design.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunHost : MonoBehaviour, ILotPicker
    {
        private RunDefinition _run;
        private List<LotChoice> _choices;

        /// <summary>The run state shared by every lot scene.</summary>
        public RunProgress Progress { get; private set; }

        /// <summary>The lots of the run.</summary>
        public RunDefinition Definition => _run;

        public IReadOnlyList<LotChoice> Choices => _choices;

        /// <summary>Creates the progress and the picker list; call once, right after adding the component.</summary>
        public void Initialize(RunDefinition run)
        {
            _run = run;
            Progress = new RunProgress(run);
            _choices = LotChoices.From(run);
            DontDestroyOnLoad(gameObject);
        }

        public bool TryChoose(int index)
        {
            if (index < 0 || index >= _choices.Count || !_run.TryGet(_choices[index].Id, out var lot)) return false;
            SceneManager.LoadScene(lot.SceneName);
            return true;
        }
    }
}
