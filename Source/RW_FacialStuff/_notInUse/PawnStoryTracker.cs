using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public class Pawn_StoryTrackerModded : RimWorld.Pawn_StoryTracker
    {
              public extern Pawn_StoryTrackerModded();

        public Pawn pawn;


        private string headGraphicPath;

        public new string HeadGraphicPath
        {
            get
            {
                if (headGraphicPath == null)
                {
                    headGraphicPath = GraphicDatabaseModdedHeadRecords.GetHeadRandom(pawn, pawn.gender, pawn.story.SkinColor, this.pawn.story.crownType).GraphicPath;
                }
                return headGraphicPath;
            }
        }

        private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
{
    new CurvePoint(0f, 0f),
    new CurvePoint(10f, 0.7f),
    new CurvePoint(35f, 1f),
    new CurvePoint(60f, 1.6f)
};

        private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
{
    new CurvePoint(0f, 0f),
    new CurvePoint(10f, 10f),
    new CurvePoint(20f, 16f),
    new CurvePoint(30f, 22f)
};

        private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
{
    new CurvePoint(0f, 0f),
    new CurvePoint(3f, 200f),
    new CurvePoint(7f, 25f),
    new CurvePoint(10f, 25f),
    new CurvePoint(15f, 5f),
    new CurvePoint(20f, 0f)
};
        private const float PassionChancePerLevel = 0.11f;

        private const float PassionMajorChance = 0.2f;

        // RimWorld.Pawn_StoryTracker
        private int FinalLevelOfSkill(SkillDef sk)
        {
            float num;
            if (sk.definedInBackstories)
            {
                num = (float)Rand.RangeInclusive(1, 4);
                foreach (Backstory current in from bs in this.AllBackstories
                                              where bs != null
                                              select bs)
                {
                    foreach (KeyValuePair<SkillDef, int> current2 in current.skillGainsResolved)
                    {
                        if (current2.Key == sk)
                        {
                            num += (float)current2.Value * Rand.Range(1f, 1.4f);
                        }
                    }
                }
            }
            else
            {
                num = Rand.ByCurve(LevelRandomCurve, 100);
            }
            float num2 = Rand.Range(1f, AgeSkillMaxFactorCurve.Evaluate((float)this.pawn.ageTracker.AgeBiologicalYears));
            num *= num2;
            for (int i = 0; i < this.traits.allTraits.Count; i++)
            {
                int num3 = 0;
                if (this.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out num3))
                {
                    num += (float)num3;
                }
            }
            num = LevelFinalAdjustmentCurve.Evaluate(num);
            int value = Mathf.RoundToInt(num);
            return Mathf.Clamp(value, 0, 20);
        }


        public new void ExposeData()
        {
            string saveKey = (childhood == null) ? null : childhood.uniqueSaveKey;
            Scribe_Values.LookValue(ref saveKey, "childhood", null, false);
            childhood = BackstoryDatabase.GetWithKey(saveKey);
            string saveKey2 = (adulthood == null) ? null : this.adulthood.uniqueSaveKey;
            Scribe_Values.LookValue(ref saveKey2, "adulthood", null, false);
            adulthood = BackstoryDatabase.GetWithKey(saveKey2);
            Scribe_Values.LookValue(ref skinWhiteness, "skinWhiteness", 0f, false);
            Scribe_Values.LookValue(ref hairColor, "hairColor", default(Color), false);
            Scribe_Values.LookValue(ref crownType, "crownType", CrownType.Undefined, false);
            Scribe_Values.LookValue(ref headGraphicPath, "headGraphicPath", null, false);
            Scribe_Defs.LookDef(ref hairDef, "hairDef");
            Scribe_Deep.LookDeep(ref traits, "traits", new object[]
            {
                pawn
            });
            if (Scribe.mode == LoadSaveMode.PostLoadInit && hairDef == null)
            {
                hairDef = DefDatabase<HairDef>.AllDefs.RandomElement();
            }
        }
    }
}
