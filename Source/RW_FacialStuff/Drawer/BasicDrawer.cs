using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Drawer
{
    using UnityEngine;

    using Verse;

    public abstract class BasicDrawer
    {
        protected BasicDrawer()
        {
        }

        protected List<Vector2> GetJointPositions(
            Rot4 rot,
            float jointOffsetHorizontalX,
            float jointOffsetZ,
            float jointOffsetVerticalX)
        {
            List<Vector2> positions = new List<Vector2>();
            if (rot.IsHorizontal)
            {
                positions.Add(new Vector2((rot == Rot4.East ? 1 : -1) * jointOffsetHorizontalX, jointOffsetZ));
            }
            else
            {
                bool north = rot == Rot4.North;
                positions.Add(new Vector2(north ? 1 : -1 * jointOffsetVerticalX, jointOffsetZ));
                positions.Add(new Vector2(north ? -1 : 1 * jointOffsetVerticalX, jointOffsetZ));
            }
            return positions;
        }
    }
}
