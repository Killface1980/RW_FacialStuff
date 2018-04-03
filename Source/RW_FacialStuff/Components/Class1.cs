using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FacialStuff.Components
{
    class Class1 : MapComponent
    {
        public static Dictionary<Pawn, float> pawnPositionOffset = new Dictionary<Pawn, float>();

        public Class1(Map map) : base(map)
        {
        }

        private static void ExtendGrid(ref List<IntVec3> grid, IntVec3 sourcePos)
        {
            //  grid.Add(sourcePos);
            sourcePos.z -= 1;
            grid.Add(sourcePos);
            sourcePos.x -= 1;
            grid.Add(sourcePos);
            sourcePos.x += 1;
            grid.Add(sourcePos);
        }
        private static void ExtendSearchGrid(IntVec3 searchPos, ref List<IntVec3> cells)
        {
            List<IntVec3> allcells = GenAdj.CellsAdjacent8Way(searchPos, Rot4.North, new IntVec2(1, 1)).ToList();
            foreach (IntVec3 c in allcells)
            {
                if (!cells.Contains(c))
                {
                    cells.Add(c);
                }
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            var currentPawns = new List<Pawn>();
            var checkedPawns = new List<Pawn>();

            CellRect _viewRect = Find.CameraDriver.CurrentViewRect;
            _viewRect = _viewRect.ExpandedBy(1);

            foreach (Pawn pawn in this.map.mapPawns.AllPawnsSpawned)
            {
                if (!_viewRect.Contains(pawn.Position)) { continue; }
                currentPawns.Add(pawn);
            }
            if (currentPawns.NullOrEmpty() || currentPawns.Count > 32) { return; }
            // top to bottom
            int minX = currentPawns.Min(x => x.Position.x);
            int maxX = currentPawns.Max(x => x.Position.x);
            int minZ = currentPawns.Min(x => x.Position.z);
            int maxZ = currentPawns.Max(x => x.Position.z);
            //  Log.Message(minX + " - " + maxX + " - " + minZ + " - " + maxZ);
            Dictionary<IntVec3, List<Pawn>> pawnsOn = new Dictionary<IntVec3, List<Pawn>>();
            var checkedGrid = new List<IntVec3>();
            int currentZ = maxZ;
            while (currentZ > minZ)
            {
                currentZ--;

                int currentX = minX;

                while (currentX < maxX)
                {
                    currentX++;

                    if (currentPawns.Count == checkedPawns.Count)
                    {
                        break;
                    }
                    IntVec3 currentCell = new IntVec3(currentX, 0, currentZ);

                    if (checkedGrid.Contains(currentCell)) { continue; }

                    // start with the topmost pawn
                    List<Pawn> pawnsOnCell = currentPawns.FindAll(x => x.Position.x == currentX && x.Position.z == currentZ);

                    if (pawnsOnCell.NullOrEmpty())
                    {
                        continue;
                    }

                    checkedGrid.Add(currentCell);
                    pawnsOn.Add(currentCell, pawnsOnCell);
                    checkedPawns.AddRange(pawnsOnCell);
                }
            }

            // string log = "FS values: ";
            foreach (KeyValuePair<IntVec3, List<Pawn>> x in pawnsOn)
            {
                IntVec3 y = x.Key;
                List<Pawn> z = x.Value;
                //        log += "\n vector at: " + y;
                foreach (Pawn p in z)
                {
                    //          log += "\n" + p.LabelCap + " - " + p.Position;
                }
            }
            //    Log.Message(log);
        }
    }
}