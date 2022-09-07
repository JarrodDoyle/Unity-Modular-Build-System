using System.Collections.Generic;
using UnityEngine;

public enum GridItemSlot
{
    North,
    East,
    South,
    West,
    Bottom
}

public class GridItem
{
    public Dictionary<GridItemSlot, Transform> Slots;

    public GridItem()
    {
        Slots = new Dictionary<GridItemSlot, Transform>
        {
            [GridItemSlot.North] = null,
            [GridItemSlot.East] = null,
            [GridItemSlot.South] = null,
            [GridItemSlot.West] = null,
            [GridItemSlot.Bottom] = null
        };
    }
}
