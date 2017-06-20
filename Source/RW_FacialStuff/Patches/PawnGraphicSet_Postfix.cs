using System.Collections.Generic;

using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.Detouring
{

    using static GraphicDatabaseHeadRecordsModded;

    public static class SaveableCache
    {
        public static List<HaircutPawn> _pawnHairCache = new List<HaircutPawn>();

        public static HaircutPawn GetHairCache(Pawn pawn)
        {
            foreach (HaircutPawn c in _pawnHairCache)
                if (c.Pawn == pawn)
                    return c;
            HaircutPawn n = new HaircutPawn { Pawn = pawn };
            _pawnHairCache.Add(n);
            return n;

            // if (!PawnApparelStatCaches.ContainsKey(pawn))
            // {
            //     PawnApparelStatCaches.Add(pawn, new StatCache(pawn));
            // }
            // return PawnApparelStatCaches[pawn];
        }

    }

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    static class ResolveAllGraphics_Patch
    {
   //   [HarmonyPrefix]
   //   [HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
   //   public static void ResolveAllGraphics_Prefix(PawnGraphicSet __instance)
   //   {
   //       Pawn pawn = __instance.pawn;
   //
   //       CompFace faceComp = pawn.TryGetComp<CompFace>();
   //  //     ThingDef_AlienRace alienProps = pawn.def as ThingDef_AlienRace;
   //
   //       if (faceComp != null)// || alienProps != null)
   //       {
   //           HaircutPawn hairPawn = SaveableCache.GetHairCache(pawn);
   //           hairPawn.HairCutGraphic = CutHairDb.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
   //       }
   //   }

        [HarmonyPostfix]
        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp == null)
            {
                return;
            }

            BuildDatabaseIfNecessary();


            if (!faceComp.optimized)
            {
                faceComp.DefineFace();
            }

            if (pawn.RaceProps.hasGenders)
            {
                Color rotColor = pawn.story.SkinColor * Headhelper.skinRottingMultiplyColor;

                if (faceComp.SetHeadType())
                {
                    if (faceComp.InitializeGraphics())
                    {
                        HaircutPawn hairPawn = SaveableCache.GetHairCache(pawn);
                        hairPawn.HairCutGraphic = CutHairDb.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);

                        __instance.headGraphic = GetModdedHeadNamed(pawn, pawn.story.SkinColor);
                        __instance.desiccatedHeadGraphic = GetModdedHeadNamed(pawn, rotColor);
                        __instance.desiccatedHeadStumpGraphic = GetStump(rotColor);
                        __instance.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                            pawn.story.bodyType, ShaderDatabase.CutoutSkin,
                            rotColor);
                        PortraitsCache.Clear();
                    }

                }

            }
        }

    }
}
