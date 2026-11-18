using System.Collections.Generic;
using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// Window > Uncanny Valet > Lot Validator: runs the hand-made lot rules over every lot of the open scene or prefab stage and lists the findings.
    /// Double-click a finding to select and frame the marker it is about.
    /// </summary>
    public sealed class LotValidatorWindow : EditorWindow
    {
        private const float DoubleClickSeconds = 0.35f;
        private const float AutoLinkDistance = 14f;

        private readonly List<(HandmadeLot Lot, LotIssue Issue)> _rows = new List<(HandmadeLot, LotIssue)>();
        private readonly List<HandmadeLot> _lots = new List<HandmadeLot>();
        private Vector2 _scroll;
        private double _lastClick;
        private int _lastRow = -1;

        [MenuItem("Window/Uncanny Valet/Lot Validator")]
        public static void Open() => GetWindow<LotValidatorWindow>("Lot Validator");

        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += Refresh;
            Refresh();
        }

        private void OnDisable() => EditorApplication.hierarchyChanged -= Refresh;

        private void OnGUI()
        {
            DrawButtons();
            if (_lots.Count == 0)
            {
                EditorGUILayout.HelpBox("No hand-made lot (HandmadeLot component) in the open scene or prefab.", MessageType.Info);
                return;
            }
            if (_rows.Count == 0) EditorGUILayout.HelpBox("No findings. " + _lots.Count + " lot(s) checked.", MessageType.None);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (var i = 0; i < _rows.Count; i++) DrawRow(i);
            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Validate", EditorStyles.toolbarButton)) Refresh();
                if (GUILayout.Button("Frame all", EditorStyles.toolbarButton)) FrameAll();
                if (GUILayout.Button("Auto-link walk nodes", EditorStyles.toolbarButton)) AutoLink();
            }
        }

        private void DrawRow(int index)
        {
            var (lot, issue) = _rows[index];
            var icon = issue.IsError ? "console.erroricon.sml" : "console.warnicon.sml";
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 2f);
            var content = new GUIContent(" " + issue, EditorGUIUtility.IconContent(icon).image, issue.Message);
            GUI.Label(rect, content);
            if (Event.current.type != EventType.MouseDown || !rect.Contains(Event.current.mousePosition)) return;
            var twice = _lastRow == index && EditorApplication.timeSinceStartup - _lastClick < DoubleClickSeconds;
            _lastRow = index;
            _lastClick = EditorApplication.timeSinceStartup;
            if (twice) Select(lot, issue);
            Event.current.Use();
        }

        private static void Select(HandmadeLot lot, LotIssue issue)
        {
            var target = LotValidation.Find(lot, issue);
            Selection.activeGameObject = target;
            SceneView.lastActiveSceneView?.Frame(new Bounds(target.transform.position, Vector3.one * 6f), false);
        }

        private void Refresh()
        {
            _lots.Clear();
            _rows.Clear();
            _lots.AddRange(LotValidation.FindLots());
            foreach (var lot in _lots)
                foreach (var issue in LotValidation.Check(lot)) _rows.Add((lot, issue));
            _rows.Sort((a, b) => b.Issue.IsError.CompareTo(a.Issue.IsError));
            Repaint();
        }

        private void FrameAll()
        {
            if (_lots.Count == 0) return;
            var bounds = new Bounds(_lots[0].transform.position, Vector3.zero);
            foreach (var lot in _lots)
                foreach (var child in lot.GetComponentsInChildren<Transform>(true)) bounds.Encapsulate(child.position);
            SceneView.lastActiveSceneView?.Frame(bounds, false);
        }

        private void AutoLink()
        {
            var made = 0;
            foreach (var lot in _lots) made += WalkLinkCreator.Create(lot, AutoLinkDistance);
            Debug.Log("Auto-link walk nodes made " + made + " link(s).");
            Refresh();
        }
    }
}
