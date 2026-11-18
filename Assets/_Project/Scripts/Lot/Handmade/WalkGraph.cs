using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// The walk nodes and links as a graph. All arrays are allocated in the constructor; <see cref="FindPath"/> (Dijkstra) and
    /// <see cref="Nearest"/> allocate nothing, so people can ask for routes while the game runs. Not reentrant: the path lives in a shared buffer.
    /// </summary>
    public sealed class WalkGraph
    {
        private readonly Vector2[] _nodes;
        private readonly int[] _first;
        private readonly int[] _neighbour;
        private readonly int[] _component;
        private readonly float[] _dist;
        private readonly int[] _previous;
        private readonly bool[] _done;
        private readonly int[] _path;
        private int _pathCount;

        public WalkGraph(IReadOnlyList<NamedPoint> nodes, IReadOnlyList<WalkLinkData> links)
        {
            var n = nodes.Count;
            _nodes = new Vector2[n];
            for (var i = 0; i < n; i++) _nodes[i] = nodes[i].Position;
            _first = new int[n + 1];
            _neighbour = new int[links.Count * 2];
            _component = new int[n];
            _dist = new float[n];
            _previous = new int[n];
            _done = new bool[n];
            _path = new int[n];
            Index(links);
            Label();
        }

        public int NodeCount => _nodes.Length;

        public Vector2 Position(int node) => _nodes[node];

        /// <summary>The node closest to the point; -1 when the graph has no nodes.</summary>
        public int Nearest(Vector2 point)
        {
            var best = -1;
            var bestSqr = float.MaxValue;
            for (var i = 0; i < _nodes.Length; i++)
            {
                var d = (_nodes[i] - point).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = i; }
            }
            return best;
        }

        /// <summary>True when a walk exists between the two nodes (setup-time answer, no search).</summary>
        public bool Connected(int a, int b) => a >= 0 && b >= 0 && _component[a] == _component[b];

        /// <summary>Finds the shortest walk; read it with <see cref="PathNode"/>. Returns false when there is none.</summary>
        public bool FindPath(int from, int to)
        {
            _pathCount = 0;
            if (!Connected(from, to)) return false;
            for (var i = 0; i < _nodes.Length; i++) { _dist[i] = float.MaxValue; _previous[i] = -1; _done[i] = false; }
            _dist[from] = 0f;
            while (true)
            {
                var u = Cheapest();
                if (u < 0 || u == to) break;
                _done[u] = true;
                for (var k = _first[u]; k < _first[u + 1]; k++) Relax(u, _neighbour[k]);
            }
            return Unwind(from, to);
        }

        /// <summary>Number of nodes of the last path found.</summary>
        public int PathCount => _pathCount;

        /// <summary>Node <paramref name="i"/> of the last path, from start to end.</summary>
        public int PathNode(int i) => _path[i];

        private void Relax(int u, int v)
        {
            var d = _dist[u] + Vector2.Distance(_nodes[u], _nodes[v]);
            if (d >= _dist[v]) return;
            _dist[v] = d;
            _previous[v] = u;
        }

        private int Cheapest()
        {
            var best = -1;
            var bestDist = float.MaxValue;
            for (var i = 0; i < _nodes.Length; i++)
                if (!_done[i] && _dist[i] < bestDist) { bestDist = _dist[i]; best = i; }
            return best;
        }

        private bool Unwind(int from, int to)
        {
            var count = 0;
            for (var at = to; at >= 0; at = _previous[at]) _path[count++] = at;
            if (_path[count - 1] != from) return false;
            for (var i = 0; i < count / 2; i++) (_path[i], _path[count - 1 - i]) = (_path[count - 1 - i], _path[i]);
            _pathCount = count;
            return true;
        }

        // Compressed adjacency lists: links are counted per node, then filled in.
        private void Index(IReadOnlyList<WalkLinkData> links)
        {
            var fill = new int[_nodes.Length];
            for (var i = 0; i < links.Count; i++)
            {
                if (!Valid(links[i])) continue;
                _first[links[i].A + 1]++;
                _first[links[i].B + 1]++;
            }
            for (var i = 0; i < _nodes.Length; i++) _first[i + 1] += _first[i];
            for (var i = 0; i < links.Count; i++)
            {
                if (!Valid(links[i])) continue;
                _neighbour[_first[links[i].A] + fill[links[i].A]++] = links[i].B;
                _neighbour[_first[links[i].B] + fill[links[i].B]++] = links[i].A;
            }
        }

        private bool Valid(WalkLinkData link) => link.A >= 0 && link.B >= 0 && link.A < _nodes.Length && link.B < _nodes.Length && link.A != link.B;

        private void Label()
        {
            for (var i = 0; i < _component.Length; i++) _component[i] = -1;
            var stack = new int[_nodes.Length];
            for (var seed = 0; seed < _nodes.Length; seed++)
            {
                if (_component[seed] >= 0) continue;
                var top = 0;
                stack[top++] = seed;
                _component[seed] = seed;
                while (top > 0)
                {
                    var u = stack[--top];
                    for (var k = _first[u]; k < _first[u + 1]; k++)
                    {
                        var v = _neighbour[k];
                        if (_component[v] >= 0) continue;
                        _component[v] = seed;
                        stack[top++] = v;
                    }
                }
            }
        }
    }
}
