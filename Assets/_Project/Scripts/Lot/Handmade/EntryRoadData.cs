using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// The car entry road: the lane (travel order, starting off screen), the point on the lane where a car turns into the queue, and the
    /// bend points from that turn towards the first queue slot. A route to slot k is: turn point, bend points, centre of slot k.
    /// </summary>
    public readonly struct EntryRoadData
    {
        public readonly string Name;
        public readonly Vector2[] Lane;
        public readonly Vector2 QueueTurn;
        public readonly Vector2[] Bend;

        public EntryRoadData(string name, Vector2[] lane, Vector2 queueTurn, Vector2[] bend)
        {
            Name = name;
            Lane = lane;
            QueueTurn = queueTurn;
            Bend = bend;
        }
    }
}
