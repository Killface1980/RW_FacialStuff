using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FacialStuff.Components
{
    class Class1:MapComponent
    {
        public Class1(Map map) : base(map)
        {

        }
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            var currentPawns = new List<Pawn>();

            CellRect _viewRect = Find.CameraDriver.CurrentViewRect;
            _viewRect = _viewRect.ExpandedBy(1);

            foreach (Pawn pawn in this.map.mapPawns.AllPawnsSpawned)
            {
                if (!_viewRect.Contains(pawn.Position)) { continue; }
                currentPawns.Add(pawn);
            }
            if (currentPawns.NullOrEmpty()) { return; }
            // top to bottom

            for (int i = 0; i < currentPawns.Count -1; i++)
            {
                var current = currentPawns[i];
                var next = currentPawns[i + 1];
             
                // too close together
                if (Mathf.Abs(current.Position.z - next.Position.z) < 1f) { }
            }
        }
    }
}
