using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Everything drawn while a shift is on: floating car plates and info boxes, patron mood bubbles, curb and occupied hints,
    /// the HUD trackers, the CAR SLOTS panel and the interact prompt. Owns the ticket board and the hint trackers.
    /// </summary>
    public sealed class PlayingHud : IDisposable
    {
        private readonly List<TicketRow> _rows = new List<TicketRow>(8);
        private readonly List<TicketRow> _keyRows = new List<TicketRow>(8);
        private readonly TicketBoard _board;
        private readonly HudPainter _hud;
        private readonly KeyBoardPainter _keyBoard;
        private readonly NoticePainter _notices;
        private readonly WorldTicketPainter _world;
        private readonly PatronMoodPainter _mood;
        private readonly CurbHintTracker _curbHints;
        private readonly CurbHintPainter _hintPainter;
        private readonly OccupiedHintTracker _occupiedHints;
        private readonly OccupiedHintPainter _occupiedPainter;

        public PlayingHud(PixelCanvas canvas, IEventBus bus, IGameClock clock, IShiftScoreView score, ICarFactory cars, EmotionStyle style)
        {
            _board = new TicketBoard(bus);
            _hud = new HudPainter(canvas, clock, score);
            _keyBoard = new KeyBoardPainter(canvas, style);
            _notices = new NoticePainter(canvas);
            _world = new WorldTicketPainter(canvas, cars, style);
            _mood = new PatronMoodPainter(canvas, style);
            _curbHints = new CurbHintTracker(bus);
            _hintPainter = new CurbHintPainter(canvas, cars, _curbHints);
            _occupiedHints = new OccupiedHintTracker(bus, UnscaledTime);
            _occupiedPainter = new OccupiedHintPainter(canvas, _occupiedHints);
        }

        private static float UnscaledTime() => Time.unscaledTime;

        public void Draw(Camera camera, float time, string prompt)
        {
            _board.CollectRows(_rows);
            _world.Draw(_rows, camera, time);
            _mood.Draw(_rows, camera);
            _hintPainter.Draw(camera);
            _occupiedPainter.Draw(camera);
            _hud.Draw(time);
            _board.CollectKeyBoardRows(_keyRows);
            _keyBoard.Draw(_keyRows, time);
            _notices.Draw(prompt);
        }

        public void Dispose()
        {
            _board.Dispose();
            _curbHints.Dispose();
            _occupiedHints.Dispose();
        }
    }
}
