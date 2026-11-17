using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>The read-only <see cref="IPowerView"/>: levels from the run progress, numbers from the live settings, phases from the stack.</summary>
    public sealed class PowerView : IPowerView
    {
        private readonly Func<IRunProgress> _progress;
        private readonly PowerSettings _settings;
        private readonly PowerStack _stack;

        /// <param name="progress">The run state; a function because a lot scene registers it after the power objects exist.</param>
        public PowerView(Func<IRunProgress> progress, PowerSettings settings, PowerStack stack)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _stack = stack ?? throw new ArgumentNullException(nameof(stack));
        }

        public bool TryGetState(StoryPower power, out PowerState state)
        {
            state = default;
            var progress = _progress();
            var level = progress != null ? progress.PowerLevel(power) : 0;
            if (level < 1 || !_settings.TryGetValues(power, level, out var values)) return false;
            var phase = _stack.PhaseOf(power, out var remaining01);
            state = new PowerState(power, level, phase, remaining01, values);
            return true;
        }

        public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values) => _settings.TryGetValues(power, level, out values);
    }
}
