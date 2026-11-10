using System;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Owns the shift's score, combo and complaints (design: the run state owner that replaces the debt ledger).
    /// A tip adds its amount and raises the combo; a $0 tip resets the combo; a patron who runs out of patience
    /// costs the complaint penalty and resets the combo. Everything starts clean on ShiftStarted and the final
    /// numbers go out as ShiftResultReady after ShiftEnded.
    /// </summary>
    public sealed class ShiftScore : IShiftScoreView, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ShiftConfig _config;
        private readonly ComboTracker _combo;
        private bool _open;

        public int Score { get; private set; }
        public int Complaints { get; private set; }
        public int ComboMultiplier => _combo.Multiplier;
        public int Stars => StarRating.Stars(Score, OneStarScore, TwoStarScore, ThreeStarScore);
        public int OneStarScore => _config.OneStarScore;
        public int TwoStarScore => _config.TwoStarScore;
        public int ThreeStarScore => _config.ThreeStarScore;

        public ShiftScore(IEventBus bus, ShiftConfig config)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _combo = new ComboTracker(config.MaxComboMultiplier);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<TipEarned>(OnTipEarned);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<TipEarned>(OnTipEarned);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            Score = 0;
            Complaints = 0;
            _combo.Reset();
            _open = true;
            Publish();
        }

        private void OnShiftEnded(ShiftEnded e)
        {
            if (!_open) return;
            _open = false;
            _bus.Publish(new ShiftResultReady(new ShiftResult(Score, Stars, Complaints,
                OneStarScore, TwoStarScore, ThreeStarScore)));
        }

        private void OnTipEarned(TipEarned e)
        {
            if (!_open) return;
            if (e.Amount > 0)
            {
                Score += e.Amount;
                _combo.RecordHappy();
            }
            else
            {
                _combo.Reset();
            }
            Publish();
        }

        // The pickup timer running out means an angry patron: the tip is $0 and the score pays the complaint penalty.
        private void OnTimerExpired(TicketTimerExpired e)
        {
            if (!_open) return;
            Complaints++;
            Score = Math.Max(0, Score - _config.ComplaintPenalty);
            _combo.Reset();
            _bus.Publish(new ComplaintFiled(e.Ticket, _config.ComplaintPenalty));
            Publish();
        }

        private void Publish() => _bus.Publish(new ScoreChanged(Score, ComboMultiplier, Complaints, Stars));
    }
}
