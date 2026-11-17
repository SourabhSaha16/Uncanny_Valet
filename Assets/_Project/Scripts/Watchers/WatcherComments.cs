using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Watchers
{
    /// <summary>
    /// Makes a watcher comment on what happens in the shift: a curse (angry patrons included), a perfect park or drop-off, a big crash,
    /// the blessing milestones and the last 30 seconds. Each trigger asks for one line (round robin) at comment priority; the speech
    /// director decides whether it is shown.
    /// </summary>
    public sealed class WatcherComments : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly WatcherVoice _voice;
        private readonly WatcherTextAsset _text;
        private readonly float _bigCrashDamage;
        private readonly Func<ILotGoal> _goal;
        private readonly LinePicker _lines = new LinePicker();
        private readonly MilestoneTracker _milestones = new MilestoneTracker();

        public WatcherComments(IEventBus bus, WatcherVoice voice, WatcherTextAsset text, float bigCrashDamage, Func<ILotGoal> goal)
        {
            _bus = bus; _voice = voice; _text = text; _bigCrashDamage = bigCrashDamage; _goal = goal;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<CurseApplied>(OnCurse);
            _bus.Subscribe<PerfectPark>(OnPerfectPark);
            _bus.Subscribe<PerfectDropOff>(OnPerfectDropOff);
            _bus.Subscribe<CarDamaged>(OnCarDamaged);
            _bus.Subscribe<BlessingsChanged>(OnBlessingsChanged);
            _bus.Subscribe<ShiftLast30Seconds>(OnLast30);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<CurseApplied>(OnCurse);
            _bus.Unsubscribe<PerfectPark>(OnPerfectPark);
            _bus.Unsubscribe<PerfectDropOff>(OnPerfectDropOff);
            _bus.Unsubscribe<CarDamaged>(OnCarDamaged);
            _bus.Unsubscribe<BlessingsChanged>(OnBlessingsChanged);
            _bus.Unsubscribe<ShiftLast30Seconds>(OnLast30);
        }

        private void OnShiftStarted(ShiftStarted e) => _milestones.Reset();

        private void OnCurse(CurseApplied e) => Comment(WatcherEvent.Curse);

        private void OnPerfectPark(PerfectPark e) => Comment(WatcherEvent.PerfectPark);

        private void OnPerfectDropOff(PerfectDropOff e) => Comment(WatcherEvent.PerfectDropOff);

        private void OnLast30(ShiftLast30Seconds e) => Comment(WatcherEvent.Last30Seconds);

        private void OnCarDamaged(CarDamaged e)
        {
            if (SituationRules.IsBigCrash(e.Delta01, _bigCrashDamage)) Comment(WatcherEvent.BigCrash);
        }

        private void OnBlessingsChanged(BlessingsChanged e)
        {
            var goal = _goal();
            if (goal == null) return;
            var milestone = _milestones.Update(e.Blessings, goal.BlessingThreshold);
            if (milestone == GoalMilestone.Halfway) Comment(WatcherEvent.HalfwayToGoal);
            else if (milestone == GoalMilestone.Reached) Comment(WatcherEvent.GoalReached);
        }

        private void Comment(WatcherEvent watcherEvent)
        {
            if (_lines.TryNext(watcherEvent, _text.Lines(watcherEvent), out var line)) _voice.Say(line, SpeechPriority.Comment);
        }
    }
}
