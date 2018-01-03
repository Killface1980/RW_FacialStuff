// ReSharper disable All

namespace FacialStuff.Harmony
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FaceEditor;
    using Genetics;
    using GraphicsFS;
    using Utilities;
    using global::Harmony;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;

    [StaticConstructorOnStartup]
    public static class HarmonyPatchesFS
    {
        static HarmonyPatchesFS()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }), null, null, new HarmonyMethod(typeof(Alien), nameof(Alien.RenderPawnInternalTranspiler)));

            // harmony.Patch(
            // AccessTools.Method(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents)),
            // null,
            // null,
            // new HarmonyMethod(
            // typeof(Dialog_Options_DoWindowContents_Patch),
            // nameof(Dialog_Options_DoWindowContents_Patch.Transpiler)));
            harmony.Patch(
                          AccessTools.Method(typeof(Page_ConfigureStartingPawns), "DrawPortraitArea"),
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(AddFaceEditButton)));

            harmony.Patch(
                          AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(ResolveAllGraphics_Postfix)));

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
            // harmony.Patch(
            // AccessTools.Method(
            // typeof(Pawn_HealthTracker),
            // nameof(Pawn_HealthTracker.AddHediff),
            // new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) }),
            // null,
            // new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(AddHediff_Postfix)));

            harmony.Patch(
                          AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt),
                                             new[] {typeof(Vector3), typeof(RotDrawMode), typeof(bool)}),
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(HarmonyPatchesFS.RenderPawnAt)),
                          null
                         );

            harmony.Patch(
                          AccessTools.Method(typeof(HediffSet), nameof(HediffSet.DirtyCache)),
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DirtyCache_Postfix)));

            harmony.Patch(
                          AccessTools.Method(typeof(GraphicDatabaseHeadRecords),
                                             nameof(GraphicDatabaseHeadRecords.Reset)),
                          null,
                          new HarmonyMethod(
                                            typeof(GraphicDatabaseHeadRecordsModded),
                                            nameof(GraphicDatabaseHeadRecordsModded.Reset)));

            harmony.Patch(
                          AccessTools.Method(typeof(PawnHairChooser), nameof(PawnHairChooser.RandomHairDefFor)),
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(RandomHairDefFor_PreFix)),
                          null);

            // if (!skinPatched)
            // {
            // AccessTools.Field(typeof(PawnSkinColors), "SkinColors").SetValue(
            // typeof(PawnSkinColors_FS.SkinColorData),
            // PawnSkinColors_FS.SkinColors);
            // skinPatched = true;
            // }
            harmony.Patch(
                          AccessTools.Method(typeof(PawnSkinColors), "GetSkinDataIndexOfMelanin"),
                          new HarmonyMethod(
                                            typeof(PawnSkinColors_Fs),
                                            nameof(PawnSkinColors_Fs.GetSkinDataIndexOfMelanin_Prefix)),
                          null);

            harmony.Patch(
                          AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetSkinColor)),
                          new HarmonyMethod(typeof(PawnSkinColors_Fs), nameof(PawnSkinColors_Fs.GetSkinColor_Prefix)),
                          null);

            harmony.Patch(
                          AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.RandomMelanin)),
                          new HarmonyMethod(typeof(PawnSkinColors_Fs), nameof(PawnSkinColors_Fs.RandomMelanin_Prefix)),
                          null);

            harmony.Patch(
                          AccessTools.Method(typeof(PawnSkinColors),
                                             nameof(PawnSkinColors.GetMelaninCommonalityFactor)),
                          new HarmonyMethod(
                                            typeof(PawnSkinColors_Fs),
                                            nameof(PawnSkinColors_Fs.GetMelaninCommonalityFactor_Prefix)),
                          null);

            harmony.Patch(
                          AccessTools.Method(typeof(Pawn_InteractionsTracker),
                                             nameof(Pawn_InteractionsTracker.TryInteractWith)),
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(TryInteractWith_Postfix)));

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

        public static void AddFaceEditButton(Page_ConfigureStartingPawns __instance, Rect rect)
        {
            FieldInfo PawnFieldInfo =
                typeof(Page_ConfigureStartingPawns).GetField("curPawn", BindingFlags.NonPublic | BindingFlags.Instance);

            Pawn pawn = (Pawn) PawnFieldInfo?.GetValue(__instance);

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            // Shitty Transpiler, doin' it on my own
            Rect rect2 = new Rect(rect.x + 500f, rect.y, 25f, 25f);
            if (rect2.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.cyan;

                // GUI.color = new Color(0.97647f, 0.97647f, 0.97647f);
            }
            else
            {
                GUI.color = new Color(0.623529f, 0.623529f, 0.623529f);
            }

            GUI.DrawTexture(rect2, ContentFinder<Texture2D>.Get("Buttons/ButtonFace", true));
            GUI.color  = Color.white;
            string tip = "FacialStuffEditor.EditFace".Translate();
            TooltipHandler.TipRegion(rect2, tip);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect2, false))
            {
                SoundDefOf.TickLow.PlayOneShotOnCamera(null);
                OpenStylingWindow(pawn);
            }
        }

        public static void OpenStylingWindow(Pawn pawn)
        {
            pawn.GetCompFace(out CompFace face);
            Find.WindowStack.Add(new Dialog_FaceStyling(face));
        }

        public static void DirtyCache_Postfix(HediffSet __instance)
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            Pawn pawn = __instance.pawn; // Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (pawn == null || !pawn.Spawned || pawn.Map == null)
            {
                return;
            }

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            pawn.CheckForAddedOrMissingParts();
            if (!compFace.Deactivated)
            {
                pawn.Drawer.renderer.graphics.nakedGraphic = null;
                PortraitsCache.SetDirty(pawn);
            }
        }

        public static bool RandomHairDefFor_PreFix(Pawn pawn, FactionDef factionType, ref HairDef __result)
        {
            Log.Message("1 - " + pawn);
            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return true;
            }

            Log.Message("2 - " + pawn.def.defName);

            if (compFace.Props.useAlienRacesHairTags)
            {
                return true;
            }

            FactionDef faction = factionType;

            if (faction == null)
            {
                faction = FactionDefOf.PlayerColony;
            }

            List<string> hairTags = faction.hairTags;

            if (pawn.def == ThingDefOf.Human)
            {
                List<string> vanillatags = new List<string> {"Urban", "Rural", "Punk", "Tribal"};
                if (!hairTags.Any(x => vanillatags.Contains(x)))
                {
                    hairTags.AddRange(vanillatags);
                }
            }

            IEnumerable<HairDef> source = from hair in DefDatabase<HairDef>.AllDefs
                                          where hair.hairTags.SharesElementWith(hairTags)
                                          select hair;

            __result = source.RandomElementByWeight(hair => PawnFaceMaker.HairChoiceLikelihoodFor(hair, pawn));
            return false;
        }

        // [HarmonyAfter("net.pardeike.zombieland")]
        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn == null)
            {
                return;
            }

            pawn.CheckForAddedOrMissingParts();

            // Check if race has face, else return
            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            compFace.IsChild = pawn.ageTracker.AgeBiologicalYearsFloat < 14;

            // Return if child
            if (compFace.IsChild || compFace.Deactivated)
            {
                return;
            }

            __instance.ClearCache();
            pawn.GetComp<CompBodyAnimator>()?.ClearCache();

            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            // Need: get the traditional habitat of a faction => not suitable, as factions are scattered around the globe
            // if (!faceComp.IsSkinDNAoptimized)
            // {
            // faceComp.DefineSkinDNA();
            // }

            // Custom rotting color, mixed with skin tone
            Color rotColor = pawn.story.SkinColor * FaceTextures.SkinRottingMultiplyColor;
            if (!compFace.InitializeCompFace())
            {
                return;
            }

            // Set up the hair cut graphic
            if (Controller.settings.MergeHair)
            {
                HairCutPawn hairPawn    = CutHairDB.GetHairCache(pawn);
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

            if (compFace.Props.needsBlankHumanHead)
            {
                __instance.headGraphic =
                    GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn,
                                                                        pawn.story.SkinColor);
                __instance.desiccatedHeadGraphic =
                    GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, rotColor);
                __instance.desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecordsModded.GetStump(rotColor);
            }

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

        public static void TryInteractWith_Postfix(Pawn_InteractionsTracker __instance, bool __result, Pawn recipient)
        {
            if (__instance == null)
            {
                return;
            }

            FieldInfo PawnFieldInfo =
                typeof(Pawn_InteractionsTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            Pawn pawn = (Pawn) PawnFieldInfo?.GetValue(__instance);

            if (pawn == null || recipient == null)
            {
                return;
            }

            if (__result)
            {
                if (pawn.GetCompFace(out CompFace compFace))
                {
                    if (compFace.Props.canRotateHead)
                    {
                        if (compFace.HeadRotator != null && !compFace.IsChild)
                        {
                            compFace.HeadRotator.LookAtPawn(recipient);
                        }
                    }
                }

                if (recipient.GetCompFace(out CompFace recipientFace))
                {
                    if (recipientFace.Props.canRotateHead)
                    {
                        if (recipientFace.HeadRotator != null && !recipientFace.IsChild)
                        {
                            recipientFace.HeadRotator.LookAtPawn(pawn);
                        }
                    }
                }
            }
        }

        private static void CheckAllInjected()
        {
            // Now to enjoy the benefits of having made a popular mod!
            // This will be our little secret.
            Backstory childMe = new Backstory
                                {
                                    bodyTypeMale   = BodyType.Male,
                                    bodyTypeFemale = BodyType.Female,
                                    slot           = BackstorySlot.Childhood,
                                    baseDesc       =
                                        "NAME never believed what was common sense and always doubted other people. HECAP later went on inflating toads with HIS sushi stick. It was there HE earned HIS nickname.",
                                    requiredWorkTags = WorkTags.Violent,
                                    shuffleable      = false
                                };
            childMe.SetTitle("Lost child");
            childMe.SetTitleShort("Seeker");
            childMe.skillGains.Add("Shooting", 4);
            childMe.skillGains.Add("Medicine", 2);
            childMe.skillGains.Add("Social",   1);
            childMe.PostLoad();
            childMe.ResolveReferences();

            Backstory adultMale = new Backstory
                                  {
                                      bodyTypeMale   = BodyType.Male,
                                      bodyTypeFemale = BodyType.Female,
                                      slot           = BackstorySlot.Adulthood,
                                      baseDesc       =
                                          "HECAP left the military early on and acquired his skills on his own. HECAP doesn't like doctors, thus HECAP prefers to tend his wounds himself.",
                                      shuffleable     = false,
                                      spawnCategories = new List<string>()
                                  };
            adultMale.spawnCategories.AddRange(new[] {"Civil", "Raider", "Slave", "Trader", "Traveler"});
            adultMale.SetTitle("Lone gunman");
            adultMale.SetTitleShort("Gunman");
            adultMale.skillGains.Add("Shooting", 4);
            adultMale.skillGains.Add("Medicine", 3);
            adultMale.skillGains.Add("Cooking",  2);
            adultMale.skillGains.Add("Social",   1);
            adultMale.PostLoad();
            adultMale.ResolveReferences();

            PawnBio me = new PawnBio
                         {
                             childhood = childMe,
                             adulthood = adultMale,
                             gender    = GenderPossibility.Male,
                             name      = NameTriple.FromString("Gator 'Killface' Stinkwater")
                         };
            me.PostLoad();
            SolidBioDatabase.allBios.Add(me);
            BackstoryDatabase.AddBackstory(childMe);

            BackstoryDatabase.AddBackstory(adultMale);
        }

        private static Graphic_Shadow _shadowGraphic;

        public static bool Plants;

        public static float Steps;

        public static List<Thing> PlantMoved = new List<Thing>();

        // Verse.PawnRenderer
        public static bool RenderPawnAt(PawnRenderer __instance, Vector3 drawLoc, RotDrawMode bodyDrawType,
                                        bool         headStump)
        {
            Pawn pawn = __instance.graphics.pawn;
            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }

            if (pawn.RaceProps.Animal)
            {
                return true;
            }

            if (!pawn.GetCompAnim(out CompBodyAnimator compAnim))
            {
                return true;
            }

            bool showHands = compAnim.Props.bipedWithHands && Controller.settings.UseHands;
            if (!showHands)
            {
                return true;
            }

            if (pawn.GetPosture() != PawnPosture.Standing)
            {
                return true;
            }

            Thing carriedThing = pawn.carryTracker?.CarriedThing;
            if (carriedThing == null)
            {
                return true;
            }

            Vector3 loc = drawLoc;
            HarmonyPatch_PawnRenderer.Prefix(
                                             __instance,
                                             drawLoc,
                                             Quaternion.identity,
                                             true,
                                             pawn.Rotation,
                                             pawn.Rotation,
                                             bodyDrawType,
                                             false,
                                             headStump, ref loc);

            bool behind = false;
            bool flip   = false;


            if (pawn.CurJob == null || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref loc, ref behind, ref flip))
            {
                if (carriedThing is Pawn || carriedThing is Corpse)
                {
                    loc += new Vector3(0.44f, 0f, 0f);
                }
                else
                {
                    loc += new Vector3(0.18f, 0f, 0.05f);
                }
            }

            loc.y += (pawn.Rotation == Rot4.North ? -1f : 1f) * Offsets.YOffset_CarriedThing;


            carriedThing.DrawAt(loc, flip);
            compAnim.DrawHands(Quaternion.identity, loc, false, true);
            

            if (pawn.def.race.specialShadowData != null)
            {
                if (_shadowGraphic == null)
                {
                    _shadowGraphic = new Graphic_Shadow(pawn.def.race.specialShadowData);
                }

                _shadowGraphic.Draw(drawLoc, Rot4.North, pawn);
            }

            if (__instance.graphics.nakedGraphic != null && __instance.graphics.nakedGraphic.ShadowGraphic != null)
            {
                __instance.graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, pawn);
            }

            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
            }

            // __instance.DrawDebug();
            return false;
        }

        // [HarmonyPatch(typeof(Dialog_Options))]
        // [HarmonyPatch("DoWindowContents")]
        // internal static class Dialog_Options_DoWindowContents_Patch
        // {
        // public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        // {
        // MethodInfo m_set_HatsOnlyOnMap = AccessTools.Method(typeof(Prefs), "set_HatsOnlyOnMap");
        // MethodInfo m_MoreStuff = AccessTools.Method(typeof(Dialog_Options_DoWindowContents_Patch), "MoreStuff");
        // foreach (CodeInstruction instruction in instructions)
        // {
        // yield return instruction;
        // if (instruction.opcode == OpCodes.Call && instruction.operand == m_set_HatsOnlyOnMap)
        // {
        // yield return new CodeInstruction(OpCodes.Ldloc_1);
        // yield return new CodeInstruction(OpCodes.Call, m_MoreStuff);
        // }
        // }
        // }
        // private static void MoreStuff(Listing_Standard listing_Standard)
        // {
        // bool hideHatWhileRoofed = Controller.settings.HideHatWhileRoofed;
        // listing_Standard.CheckboxLabeled(
        // "Settings.HideHatWhileRoofed".Translate(),
        // ref hideHatWhileRoofed,
        // "Settings.HideHatWhileRoofedTooltip".Translate());
        // bool showHeadWear = Controller.settings.FilterHats;
        // listing_Standard.CheckboxLabeled(
        // "Settings.FilterHats".Translate(),
        // ref showHeadWear,
        // "Settings.FilterHatsTooltip".Translate());
        // bool hideHatsInBed = Controller.settings.HideHatInBed;
        // listing_Standard.CheckboxLabeled(
        // "Settings.HideHatInBed".Translate(),
        // ref hideHatsInBed,
        // "Settings.HideHatInBedTooltip".Translate());
        // if (GUI.changed)
        // {
        // if (showHeadWear != Controller.settings.FilterHats)
        // {
        // Controller.settings.FilterHats = showHeadWear;
        // Controller.settings.Write();
        // }
        // if (hideHatWhileRoofed != Controller.settings.HideHatWhileRoofed)
        // {
        // Controller.settings.HideHatWhileRoofed = hideHatWhileRoofed;
        // Controller.settings.Write();
        // }
        // if (hideHatsInBed != Controller.settings.HideHatInBed)
        // {
        // Controller.settings.HideHatInBed = hideHatsInBed;
        // Controller.settings.Write();
        // }
        // }
        // }
        // }
    }
}