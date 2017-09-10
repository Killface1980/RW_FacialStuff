// ReSharper disable All
namespace FacialStuff.Detouring
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using FacialStuff.FaceStyling_Bench;

    using global::Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.Sound;

    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            harmony.Patch(
                AccessTools.Method(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents)),
                null,
                null,
                new HarmonyMethod(
                    typeof(Dialog_Options_DoWindowContents_Patch),
                    nameof(Dialog_Options_DoWindowContents_Patch.Transpiler)));

            harmony.Patch(
                AccessTools.Method(typeof(Page_ConfigureStartingPawns), nameof(Page_ConfigureStartingPawns.DoWindowContents)),
                null,
                new HarmonyMethod(
                    typeof(HarmonyPatches),
                    nameof(HarmonyPatches.AddFaceEditButton)));

            harmony.Patch(
                AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ResolveAllGraphics_Postfix)));

            // harmony.Patch(
            // AccessTools.Method(
            // typeof(PawnRenderer),
            // "RenderPawnInternal",
            // new[]
            // {
            // typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4),
            // typeof(RotDrawMode), typeof(bool), typeof(bool)
            // }),
            // new HarmonyMethod(
            // typeof(HarmonyPatch_PawnRenderer),
            // nameof(HarmonyPatch_PawnRenderer.RenderPawnInternal_Prefix)),
            // null);
            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn_HealthTracker),
                    nameof(Pawn_HealthTracker.AddHediff),
                    new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) }),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHediff_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(HediffSet), nameof(HediffSet.DirtyCache)),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DirtyCache_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(GraphicDatabaseHeadRecords), nameof(GraphicDatabaseHeadRecords.Reset)),
                null,
                new HarmonyMethod(typeof(GraphicDatabaseHeadRecordsModded), nameof(GraphicDatabaseHeadRecordsModded.Reset)));

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
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.RandomMelanin)),
                new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.RandomMelanin_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetMelaninCommonalityFactor)),
                new HarmonyMethod(
                    typeof(PawnSkinColors_FS),
                    nameof(PawnSkinColors_FS.GetMelaninCommonalityFactor_Prefix)),
                null);

            Log.Message(
                "Facial Stuff successfully completed " + harmony.GetPatchedMethods().Count()
                + " patches with harmony.");

#if develop
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(
                td => td.category == ThingCategory.Pawn && td.race.Humanlike))
            {
                if (def.inspectorTabs == null || def.inspectorTabs.Count == 0)
                {
                    def.inspectorTabs = new List<Type>();
                    def.inspectorTabsResolved = new List<InspectTabBase>();
                }

                if (def.inspectorTabs.Contains(typeof(ITab_Pawn_Face)))
                {
                    return;
                }

                def.inspectorTabs.Add(typeof(ITab_Pawn_Face));
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Face)));
            }

#endif
            CheckAllInjected();
        }

        public static void AddFaceEditButton(Page_ConfigureStartingPawns __instance)
        {
            FieldInfo PawnFieldInfo = typeof(Page_ConfigureStartingPawns).GetField("curPawn", BindingFlags.NonPublic | BindingFlags.Instance);

            Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);

            CompFace face = pawn.TryGetComp<CompFace>();
            if (face == null)
            {
                return;
            }

            // Shitty Transpiler, doin' it on my own
            Rect rect = new Rect(540f, 92f, 25f, 25f);
            if (rect.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.cyan;

                // GUI.color = new Color(0.97647f, 0.97647f, 0.97647f);
            }
            else
            {
                GUI.color = new Color(0.623529f, 0.623529f, 0.623529f);
            }


            GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("Buttons/ButtonFace", true));
            GUI.color = Color.white;
            string tip = "FacialStuffEditor.FaceStylerTitle".Translate();
            TooltipHandler.TipRegion(rect, tip);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect, false))
            {
                SoundDefOf.TickLow.PlayOneShotOnCamera(null);
                Find.WindowStack.Add(new DialogFaceStyling(pawn));
            }
        }


        // [HarmonyAfter("net.pardeike.zombieland")]
        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            // Check if race has face, else return
            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp == null)
            {
                return;
            }

            faceComp.IsChild = pawn.ageTracker.AgeBiologicalYearsFloat < 14;

            // Return if child
            if (faceComp.IsChild || faceComp.DontRender)
            {
                return;
            }

            __instance.ClearCache();

            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            // Need: get the traditional habitat of a faction => not suitable, as factions are scattered around the globe
            // if (!faceComp.IsSkinDNAoptimized)
            // {
            // faceComp.DefineSkinDNA();
            // }

            // Custom rotting color, mixed with skin tone
            Color rotColor = pawn.story.SkinColor * FaceTextures.SkinRottingMultiplyColor;
            if (!faceComp.SetHeadType(pawn))
            {
                return;
            }

            if (!faceComp.InitializeGraphics())
            {
                return;
            }

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
            __instance.rottingGraphic =
                GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                    pawn.story.bodyType,
                    ShaderDatabase.CutoutSkin,
                    rotColor);
            __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                pawn.story.hairDef.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                pawn.story.hairColor);
            PortraitsCache.SetDirty(pawn);
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
                        // Look for the werewolf
                        face.CheckForAddedOrMissingParts();
                        if (!face.DontRender)
                        {
                            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                        }
                    }
                }
            }
        }

        public static void DirtyCache_Postfix(HediffSet __instance)
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            Pawn pawn = __instance.pawn;
            if (pawn == null)
            {
                return;
            }

            if (!pawn.Spawned || !pawn.RaceProps.Humanlike)
            {
                return;
            }

            CompFace face = pawn.TryGetComp<CompFace>();
            if (face == null)
            {
                return;
            }

            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            face.CheckForAddedOrMissingParts();
            if (!face.DontRender)
            {
                pawn.Drawer.renderer.graphics.nakedGraphic = null;
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

            __result = source.RandomElementByWeight(hair => PawnFaceMaker.HairChoiceLikelihoodFor(hair, pawn));
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
                                              "HECAP left the military early on and acquired his skills on his own. HECAP doesn't like doctors, thus HECAP prefers to tend his wounds himself.",
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
                name = NameTriple.FromString("Teegee 'Killface' Stinkwater")
            };
            me.PostLoad();
            SolidBioDatabase.allBios.Add(me);
            BackstoryDatabase.AddBackstory(childMe);

            BackstoryDatabase.AddBackstory(adultMale);
        }
    }

    // [HarmonyPatch(typeof(Dialog_Options))]
    // [HarmonyPatch("DoWindowContents")]
    static class Dialog_Options_DoWindowContents_Patch
    {
        private static bool hatsOnlyOnMap;
        private static bool noHatsInBed;

        static void MoreStuff(Listing_Standard listing_Standard)
        {
            hatsOnlyOnMap = Controller.settings.HideHatWhileRoofed;
            listing_Standard.CheckboxLabeled("Settings.HideHatWhileRoofed".Translate(), ref hatsOnlyOnMap, "Settings.HideHatWhileRoofedTooltip".Translate());

            noHatsInBed = Controller.settings.HideHatInBed;
            listing_Standard.CheckboxLabeled("Settings.HideHatInBed".Translate(), ref noHatsInBed, "Settings.HideHatInBedTooltip".Translate());

            if (GUI.changed)
            {

                if (hatsOnlyOnMap != Controller.settings.HideHatWhileRoofed)
                {
                    Controller.settings.HideHatWhileRoofed = hatsOnlyOnMap;
                    Controller.settings.Write();
                }

                if (noHatsInBed != Controller.settings.HideHatInBed)
                {
                    Controller.settings.HideHatInBed = noHatsInBed;
                    Controller.settings.Write();
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo m_set_HatsOnlyOnMap = AccessTools.Method(typeof(Prefs), "set_HatsOnlyOnMap");
            MethodInfo m_MoreStuff = AccessTools.Method(typeof(Dialog_Options_DoWindowContents_Patch), "MoreStuff");

            foreach (CodeInstruction instruction in instructions)
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