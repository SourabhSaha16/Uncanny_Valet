using System;
using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Pie;
using UncannyValet.UI.Speech;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Everything drawn while a shift is on: service icons under cars, the pickup-timer pies, patron mood bubbles, curb and occupied hints,
    /// the HUD trackers and the interact prompt. Owns the ticket board and the hint trackers.
    /// </summary>
    public sealed class PlayingHud : IDisposable
    {
        private readonly List<TicketRow> _rows = new List<TicketRow>(8);
        private readonly TicketBoard _board;
        private readonly HudPainter _hud;
        private readonly NoticePainter _notices;
        private readonly PickupPieLayer _pies;
        private readonly WorldTicketPainter _world;
        private readonly PatronMoodPainter _mood;
        private readonly BlessingPopTracker _blessingPopTracker;
        private readonly CursePopTracker _cursePopTracker;
        private readonly BlessingPopPainter _blessingPops;
        private readonly CurbHintTracker _curbHints;
        private readonly CurbHintPainter _hintPainter;
        private readonly OccupiedHintTracker _occupiedHints;
        private readonly OccupiedHintPainter _occupiedPainter;
        private readonly PowerBarTracker _powerTracker;
        private readonly PowerBarPainter _power;
        private readonly FreezeTracker _freezeTracker;
        private readonly FreezeVeilPainter _freezeVeil;
        private readonly SpeechBubbleTracker _speech;
        private readonly SpeechBubblePainter _speechPainter;

        public PlayingHud(PixelCanvas canvas, IEventBus bus, IGameClock clock, IShiftBlessingsView blessings, ILotGoal goal, ICarFactory cars, EmotionStyle style,
            BlessingPopSettings blessingPop, PowerBarSettings powerBar, SpeechBubbleSettings speechBubble = null, PickupPieSettings pickupPie = null)
        {
            _powerTracker = new PowerBarTracker(bus, powerBar, goal: goal);
            _power = new PowerBarPainter(canvas, _powerTracker);
            _freezeTracker = new FreezeTracker(bus, new FreezeFade(FreezeLook.FadeInSeconds, FreezeLook.FadeOutSeconds));
            _freezeVeil = new FreezeVeilPainter(canvas, _freezeTracker.Fade);
            _board = new TicketBoard(bus, Services.Get<IModifierCatalog>());
            _hud = new HudPainter(canvas, clock, blessings, goal);
            _notices = new NoticePainter(canvas);
            _world = new WorldTicketPainter(canvas, cars);
            _pies = new PickupPieLayer(cars, style, pickupPie ?? new PickupPieSettings());
            _mood = new PatronMoodPainter(canvas, style);
            var pops = new BlessingPops(blessingPop);
            _blessingPopTracker = new BlessingPopTracker(bus, cars, pops, blessingPop, UnscaledTime);
            _cursePopTracker = new CursePopTracker(bus, pops, blessingPop, UnscaledTime, FrameCount);
            _blessingPops = new BlessingPopPainter(canvas, pops, blessingPop);
            _curbHints = new CurbHintTracker(bus);
            _hintPainter = new CurbHintPainter(canvas, cars, _curbHints);
            _occupiedHints = new OccupiedHintTracker(bus, UnscaledTime);
            _occupiedPainter = new OccupiedHintPainter(canvas, _occupiedHints);
            var bubbles = speechBubble != null ? speechBubble.Enabled : SpeechBubbleSettings.DefaultEnabled;
            _speech = new SpeechBubbleTracker(bus, UnscaledTime, bubbles);
            if (bubbles) _speechPainter = new SpeechBubblePainter(canvas, _speech, speechBubble);
        }

        private static float UnscaledTime() => Time.unscaledTime;

        private static int FrameCount() => Time.frameCount;

        public void Draw(Camera camera, float time, string prompt)
        {
            _freezeTracker.Tick(time);
            _freezeVeil.Draw(camera); // first, so every icon and the HUD sit on top of the cold wash
            _board.CollectRows(_rows);
            _world.Draw(_rows, camera);
            _pies.Update(_rows);
            _mood.Draw(_rows, camera);
            _blessingPops.Draw(camera, Time.unscaledTime);
            _hintPainter.Draw(camera);
            _occupiedPainter.Draw(camera);
            _speechPainter?.Draw(camera, time);
            _hud.Draw(time);
            _power.Draw(time);
            _notices.Draw(prompt);
        }

        public void Dispose()
        {
            _board.Dispose();
            _pies.Dispose();
            _blessingPopTracker.Dispose();
            _cursePopTracker.Dispose();
            _powerTracker.Dispose();
            _freezeTracker.Dispose();
            _curbHints.Dispose();
            _occupiedHints.Dispose();
            _speech.Dispose();
        }
    }
}
