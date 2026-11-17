using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The fixed bay grid of lot v2 (docs/redesign/lot_v2_layout.md): two rows of four bays. Cells are (column, row)
    /// with column 0 nearest the curb and row 0 the top row. Top row: four regular bays. Bottom row: the EV bay, a
    /// regular bay, the wash slot, a regular bay. Regular ids 1..6 run in reading order; the EV bay is id 7.
    /// </summary>
    public static class BayPlan
    {
        public const int Columns = 4;
        public const int Rows = 2;

        /// <summary>Cells of regular spots 1..6.</summary>
        public static readonly Vector2Int[] Regular =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0),
            new Vector2Int(1, 1), new Vector2Int(3, 1)
        };

        /// <summary>
        /// Cells of regular spots 1..8 in a lot whose services are closed: the EV bay and the wash slot become two more regular bays, so both
        /// rows are four regular bays in reading order.
        /// </summary>
        public static readonly Vector2Int[] RegularWithoutServices =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0),
            new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1)
        };

        /// <summary>Cells of the EV charging bays (ids after the regular spots).</summary>
        public static readonly Vector2Int[] Charging = { new Vector2Int(0, 1) };

        /// <summary>Cell of the single wash slot.</summary>
        public static readonly Vector2Int Wash = new Vector2Int(2, 1);
    }
}
