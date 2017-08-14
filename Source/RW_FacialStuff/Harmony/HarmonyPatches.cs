namespace FacialStuff.Detouring
{
    using System.Collections.Generic;
    using System.Linq;

    using static GraphicDatabaseHeadRecordsModded;

    using Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.facialstuff.rimworld.mod");

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

            #region Hair

            harmony.Patch(
                AccessTools.Method(typeof(PawnHairChooser), nameof(PawnHairChooser.RandomHairDefFor)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(RandomHairDefFor_PreFix)),
               null);

            #endregion

            #region SkinColors

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

            #endregion

            // harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message(
                "Facial Stuff successfully completed " + harmony.GetPatchedMethods().Count()
                + " patches with harmony.");

            CheckAllInjected();
        }

        private static void CheckAllInjected()
        {
            //
            // Now to enjoy the benefits of having made a popular mod!
            // This will be our little secret.
            //
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
            adultMale.spawnCategories.AddRange(new string[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
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

            BuildDatabaseIfNecessary();

            // Inital definition of a pawn's appearance. Run only once - ever.
            if (!faceComp.IsOptimized)
            {
                faceComp.DefineFace();
            }

            // Need: get the traditional habitat of a faction => not suitable, as factions are scattered around the globe
            // if (!faceComp.IsSkinDNAoptimized)
            // {
            //     faceComp.DefineSkinDNA();
            // }

            // Hair color is defined here. Can't use RandomHairColor as FS checks for existing relations
            if (!faceComp.IsDNAoptimized)
            {
                faceComp.DefineHairDNA();

                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    pawn.story.hairDef.texPath,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    pawn.story.hairColor);
            }


            // Custom rotting color, mixed with skin tone
            Color rotColor = pawn.story.SkinColor * FacialGraphics.SkinRottingMultiplyColor;

            if (faceComp.SetHeadType())
            {
                if (faceComp.InitializeGraphics())
                {
                    // Set up the hair cut graphic
                    HairCutPawn hairPawn = CutHairDB.GetHairCache(pawn);
                    hairPawn.HairCutGraphic = CutHairDB.Get<Graphic_Multi>(
                        pawn.story.hairDef.texPath,
                        ShaderDatabase.Cutout,
                        Vector2.one,
                        pawn.story.hairColor);

                    __instance.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        pawn.story.SkinColor);
                    __instance.headGraphic = GetModdedHeadNamed(pawn, pawn.story.SkinColor);
                    __instance.desiccatedHeadGraphic = GetModdedHeadNamed(pawn, rotColor);
                    __instance.desiccatedHeadStumpGraphic = GetStump(rotColor);
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

        public static void RestorePart_Postfix(
            Pawn_HealthTracker __instance,
            BodyPartRecord part)
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
    }

    #region Hair 

    // [HarmonyPatch(typeof(PawnHairColors), "RandomHairColor")]
    public static class PawnHairColors_PostFix
    {
        public static Color HairPlatinum = new Color32(255, 245, 226, 255);

        public static Color HairYellowBlonde = new Color32(255, 203, 89, 255);

        public static Color HairTerraCotta = new Color32(185, 49, 4, 255);

        public static Color HairMediumDarkBrown = new Color32(110, 70, 10, 255);

        public static Color HairDarkBrown = new Color32(64, 41, 19, 255);

        public static Color HairMidnightBlack = new Color32(30, 30, 30, 255);

        public static Color HairDarkPurple = new Color32(162, 47, 115, 255);

        public static Color HairBlueSteel = new Color32(57, 115, 199, 255);

        public static Color HairBurgundyBistro = new Color32(206, 38, 58, 255);

        public static Color HairGreenGrape = new Color32(124, 189, 14, 255);

        public static Color HairMysticTurquois = new Color32(71, 191, 165, 255);

        public static Color HairPinkPearl = new Color32(230, 74, 153, 255);

        public static Color HairPurplePassion = new Color32(145, 50, 191, 255);

        public static Color HairRosaRosa = new Color32(215, 168, 255, 255);

        public static Color HairRubyRed = new Color32(227, 35, 41, 255);

        public static Color HairUltraViolet = new Color32(191, 53, 132, 255);

    }

    #endregion
}