namespace FacialStuff.Detouring
{
    using Harmony;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using UnityEngine;
    using Verse;
    using Verse.AI;

    // [HarmonyPatch(typeof(RimWorld.Dialog_Options))]
    // [HarmonyPatch("DoWindowContents")]
    // public static class Dialog_FormCaravan_CheckForErrors_Patch
    // {
    //     static IEnumerable<CodeInstruction>Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         var foundMassUsageMethod = false;
    //         int startIndex = -1, endIndex = -1;
    // 
    //         var codes = new List<CodeInstruction>(instructions);
    //         for (int i = 0; i < codes.Count; i++)
    //         {
    //             if (codes[i].opcode == OpCodes.Ret)
    //             {
    //                 if (foundMassUsageMethod)
    //                 {
    //                     Log.Error("END " + i);
    // 
    //                     endIndex = i; // include current 'ret'
    //                     break;
    //                 }
    //                 else
    //                 {
    //                     Log.Error("START " + (i + 1));
    // 
    //                     startIndex = i + 1; // exclude current 'ret'
    // 
    //                     for (int j = startIndex; j < codes.Count; j++)
    //                     {
    //                         if (codes[j].opcode == OpCodes.Ret)
    //                             break;
    //                         var strOperand = codes[j].operand as String;
    //                         if (strOperand == "TooBigCaravanMassUsage")
    //                         {
    //                             foundMassUsageMethod = true;
    //                             break;
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //         if (startIndex > -1 && endIndex > -1)
    //         {
    //             // we cannot remove the first code of our range since some jump actually jumps to
    //             // it, so we replace it with a no-op instead of fixing that jump (easier).
    //             codes[startIndex].opcode = OpCodes.Nop;
    //             codes.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
    //         }
    // 
    //         return codes.AsEnumerable();
    //     }
    // }

    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.facialstuff.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            harmony.Patch(
                AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ResolveAllGraphics_Postfix)));

            harmony.Patch(
                AccessTools.Method(
                    typeof(PawnRenderer),
                    "RenderPawnInternal",
                    new[]
                        {
                            typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4),
                            typeof(RotDrawMode), typeof(bool), typeof(bool)
                        }),
                new HarmonyMethod(
                    typeof(HarmonyPatch_PawnRenderer),
                    nameof(HarmonyPatch_PawnRenderer.RenderPawnInternal_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn_HealthTracker),
                    nameof(Pawn_HealthTracker.AddHediff),
                    new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) }),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHediff_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RestorePart)),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(RestorePart_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(PawnHairChooser), nameof(PawnHairChooser.RandomHairDefFor)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(RandomHairDefFor_PreFix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), "GetSkinDataIndexOfMelanin"),
                new HarmonyMethod(
                    typeof(PawnSkinColors_FS),
                    nameof(PawnSkinColors_FS.GetSkinDataIndexOfMelanin_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetSkinColor)),
                new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.GetSkinColor_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetSkinColor)),
                new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.GetSkinColor_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.RandomMelanin)),
                new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.RandomMelanin_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetMelaninCommonalityFactor)),
                new HarmonyMethod(
                    typeof(PawnSkinColors_FS),
                    nameof(PawnSkinColors_FS.GetMelaninCommonalityFactor_Prefix)),
                null);


            FacialGraphics.InitializeMouthGraphics();

            Log.Message(
                "Facial Stuff successfully completed " + harmony.GetPatchedMethods().Count()
                + " patches with harmony.");

            CheckAllInjected();
        }


        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            // Check if race has face, else return
            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp == null)
            {
                return;
            }

            if (pawn.Faction == null)
            {
                pawn.SetFactionDirect(Faction.OfPlayer);
            }

            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            // Hair color is defined here. Can't use RandomHairColor as FS checks for existing relations
            // todo: will be merged with IsOptimized in A18
            if (!faceComp.IsDNAoptimized)
            {
                faceComp.DefineHairDNA();

                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    pawn.story.hairDef.texPath,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    pawn.story.hairColor);
            }

            // Inital definition of a pawn's appearance. Run only once - ever.
            if (!faceComp.IsOptimized)
            {
                faceComp.DefineFace();
            }

            // Need: get the traditional habitat of a faction => not suitable, as factions are scattered around the globe
            // if (!faceComp.IsSkinDNAoptimized)
            // {
            // faceComp.DefineSkinDNA();
            // }



            // Custom rotting color, mixed with skin tone
            Color rotColor = pawn.story.SkinColor * FacialGraphics.SkinRottingMultiplyColor;

            if (faceComp.SetHeadType())
            {
                if (faceComp.InitializeGraphics())
                {
                    // Set up the hair cut graphic
                    if (Controller.settings.MergeHair)
                    {
                        HairCutPawn hairPawn = CutHairDB.GetHairCache(pawn);
                        hairPawn.HairCutGraphic = CutHairDB.Get<Graphic_Multi>(
                            pawn.story.hairDef.texPath,
                            ShaderDatabase.Cutout,
                            Vector2.one,
                            pawn.story.hairColor);
                    }

                    __instance.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        pawn.story.SkinColor);
                    __instance.headGraphic = GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, pawn.story.SkinColor);
                    __instance.desiccatedHeadGraphic = GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, rotColor);
                    __instance.desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecordsModded.GetStump(rotColor);
                    __instance.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        rotColor);
                    PortraitsCache.Clear();
                }
            }
        }

        public static void AddHediff_Postfix(
            Pawn_HealthTracker __instance,
            Hediff hediff,
            BodyPartRecord part = null,
            DamageInfo? dinfo = null)
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.Spawned)
            {
                return;
            }

            if (part == null)
            {
                return;
            }

            AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
            if (addedPartProps != null)
            {
                if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye
                    || part.def == BodyPartDefOf.Jaw)
                {
                    CompFace face = pawn.TryGetComp<CompFace>();
                    if (face != null)
                    {
                        face.CheckForAddedOrMissingParts();
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }
                }
            }
        }

        public static void RestorePart_Postfix(Pawn_HealthTracker __instance, BodyPartRecord part)
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.Spawned)
            {
                return;
            }

            if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye
                || part.def == BodyPartDefOf.Head)
            {
                CompFace face = pawn.TryGetComp<CompFace>();
                if (face != null)
                {
                    face.CheckForAddedOrMissingParts();
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }
            }
        }

        public static bool RandomHairDefFor_PreFix(Pawn pawn, FactionDef factionType, ref HairDef __result)
        {
            if (pawn.TryGetComp<CompFace>() == null)
            {
                return true;
            }

            IEnumerable<HairDef> source = from hair in DefDatabase<HairDef>.AllDefs
                                          where hair.hairTags.SharesElementWith(factionType.hairTags)
                                          select hair;

            __result = source.RandomElementByWeight(hair => PawnFaceChooser.HairChoiceLikelihoodFor(hair, pawn));
            return false;
        }

        private static void CheckAllInjected()
        {
            // Now to enjoy the benefits of having made a popular mod!
            // This will be our little secret.
            Backstory childMe = new Backstory
            {
                bodyTypeMale = BodyType.Male,
                bodyTypeFemale = BodyType.Female,
                slot = BackstorySlot.Childhood,
                baseDesc =
                                            "NAME never believed what was common sense and always doubted other people. HECAP later went on inflating toads with HIS sushi stick. It was there HE earned HIS nickname.",
                requiredWorkTags = WorkTags.Violent,
                shuffleable = false
            };
            childMe.SetTitle("Lost child");
            childMe.SetTitleShort("Seeker");
            childMe.skillGains.Add("Shooting", 4);
            childMe.skillGains.Add("Medicine", 2);
            childMe.skillGains.Add("Social", 1);
            childMe.PostLoad();
            childMe.ResolveReferences();

            Backstory adultMale = new Backstory
            {
                bodyTypeMale = BodyType.Male,
                bodyTypeFemale = BodyType.Female,
                slot = BackstorySlot.Adulthood,
                baseDesc =
                                              "HECAP continued to serve in the military, being promoted through the ranks as HIS skill increased. HECAP learned how to treat more serious wounds as HIS role slowly transitioned from scout to medic, as well as how to make good use of army rations. HECAP built good rapport with HIS squad as a result.",
                shuffleable = false,
                spawnCategories = new List<string>()
            };
            adultMale.spawnCategories.AddRange(new[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
            adultMale.SetTitle("Lone gunman");
            adultMale.SetTitleShort("Gunman");
            adultMale.skillGains.Add("Shooting", 4);
            adultMale.skillGains.Add("Medicine", 3);
            adultMale.skillGains.Add("Cooking", 2);
            adultMale.skillGains.Add("Social", 1);
            adultMale.PostLoad();
            adultMale.ResolveReferences();

            PawnBio me = new PawnBio
            {
                childhood = childMe,
                adulthood = adultMale,
                gender = GenderPossibility.Male,
                name = NameTriple.FromString("Tom 'TomJee' Stinkwater")
            };
            me.PostLoad();
            SolidBioDatabase.allBios.Add(me);
            BackstoryDatabase.AddBackstory(childMe);

            BackstoryDatabase.AddBackstory(adultMale);
        }

    }
    [HarmonyPatch(typeof(Dialog_Options))]
    [HarmonyPatch("DoWindowContents")]
    static class Dialog_Options_DoWindowContents_Patch
    {
        static void MoreStuff(Listing_Standard listing_Standard)
        {
            bool hatsOnlyOnMap = Controller.settings.HideHatWhileRoofed;
            listing_Standard.CheckboxLabeled("Settings.HideHatWhileRoofed".Translate(), ref hatsOnlyOnMap, "Settings.HideHatWhileRoofedTooltip".Translate());
            if (hatsOnlyOnMap != Controller.settings.HideHatWhileRoofed)
            {
                Controller.settings.HideHatWhileRoofed = hatsOnlyOnMap;
                Controller.settings.Write();
                //    PortraitsCache.Clear();
            }

            bool noHatsInBed = Controller.settings.HideHatInBed;
            listing_Standard.CheckboxLabeled("Settings.HideHatInBed".Translate(), ref noHatsInBed, "Settings.HideHatInBedTooltip".Translate());
            if (noHatsInBed != Controller.settings.HideHatWhileRoofed)
            {
                Controller.settings.HideHatInBed = noHatsInBed;
                Controller.settings.Write();
                // PortraitsCache.Clear();
            }


            // bool hatsOnlyOnMap = Prefs.HatsOnlyOnMap;
            // listing_Standard.CheckboxLabeled("HatsShownOnlyOnMap".Translate(), ref hatsOnlyOnMap, null);
            // if (hatsOnlyOnMap != Prefs.HatsOnlyOnMap)
            // {
            //     PortraitsCache.Clear();
            // }
            // Prefs.HatsOnlyOnMap = hatsOnlyOnMap;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_set_HatsOnlyOnMap = AccessTools.Method(typeof(Prefs), "set_HatsOnlyOnMap");
            var m_MoreStuff = AccessTools.Method(typeof(Dialog_Options_DoWindowContents_Patch), "MoreStuff");

            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Call && instruction.operand == m_set_HatsOnlyOnMap)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Call, m_MoreStuff);
                }
            }

        }
    }
}