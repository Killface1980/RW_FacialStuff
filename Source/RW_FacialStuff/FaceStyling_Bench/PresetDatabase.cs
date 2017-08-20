using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace FacialStuff.FaceStyler
{
    public sealed class PresetDatabase : IExposable
    {
        private List<FacePreset> facePresets = new List<FacePreset>();

        public List<FacePreset> AllFacePresets
        {
            get
            {
                return this.facePresets;
            }
        }

        public FacePreset DefaultFacePreset()
        {
            if (this.facePresets.Count == 0)
            {
                this.MakeNewFacePreset();
            }

            return this.facePresets[0];
        }

        // RimWorld.FacePresetDatabase
        public void ExposeData()
        {
            Scribe_Collections.LookList<FacePreset>(ref this.facePresets, "facePresets", LookMode.Deep, new object[0]);
        }

        // RimWorld.FacePresetDatabase
        public FacePreset MakeNewFacePreset()
        {
            int arg_40_0;
            if (this.facePresets.Any<FacePreset>())
            {
                arg_40_0 = this.facePresets.Max((FacePreset o) => o.uniqueId) + 1;
            }
            else
            {
                arg_40_0 = 1;
            }

            int uniqueId = arg_40_0;
            FacePreset outfit = new FacePreset(uniqueId, "FacePreset".Translate() + " " + uniqueId.ToString());
            outfit.filter.SetAllow(ThingCategoryDefOf.Apparel, true);
            this.facePresets.Add(outfit);
            return outfit;
        }

        // RimWorld.FacePresetDatabase
        public AcceptanceReport TryDelete(FacePreset outfit)
        {
            this.facePresets.Remove(outfit);
            return AcceptanceReport.WasAccepted;
        }
    }
}