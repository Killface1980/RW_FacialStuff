// ReSharper disable StyleCop.SA1401

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class Settings_Animator : ModSettings
    {
        public Dictionary<ThingDef, RaceOption> Options = new Dictionary<ThingDef, RaceOption>();

        #region Public Methods

        public void DoWindowContents(Rect inRect)
        {
            Rect rect = inRect.ContractedBy(15f);
            Listing_Standard list = new Listing_Standard(GameFont.Small) { ColumnWidth = (rect.width / 3) - 17f };

            list.Begin(rect);

            List<ThingDef> kinds = new List<ThingDef>();
            foreach (PawnKindDef kindDef in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(
                kindDef => !kinds.Contains(kindDef.race)))
            {
                kinds.Add(kindDef.race);
            }

            kinds = kinds.OrderBy(x => x.defName).ToList();
            foreach (ThingDef thingDef in kinds)
            {
                string label = thingDef.defName;
                if (thingDef.HasComp(typeof(CompBodyAnimator)))
                {
                    label += " is animated";
                }

                list.Label(label);
            }

            list.End();

            if (GUI.changed)
            {
                this.Mod.WriteSettings();
            }

        }

        #endregion Public Methods
    }
}