using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FacialStuff.AnimatorWindows;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static FacialStuff.Offsets;

namespace FacialStuff.Harmony
{
    // Not working, no idea what it is for.
    //[HarmonyPatch(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawPos), MethodType.Getter)]
	public static class DrawPos_Patch
    {
        public static bool offsetEnabled = false;
        public static Vector3 offset = Vector3.zero;

        public static void Postfix(ref Vector3 __result)
        {
            if (offsetEnabled)
            {
                __result = offset;
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    // private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, 
    // RotDrawMode bodyDrawType, bool portrait, bool headStump)

    [HarmonyPatch(
        typeof(PawnRenderer),
        "RenderPawnInternal",
        new[]
        {
            typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode),
            typeof(bool), typeof(bool), typeof(bool)
        })]
    [HarmonyBefore("com.showhair.rimworld.mod")]
    public static class HarmonyPatch_PawnRenderer
    {

        // Verse.Altitudes
        public const float LayerSpacing = 0.46875f;

        private static readonly Type _pawnRendererType = typeof(PawnRenderer);

        private static readonly MethodInfo DrawEquipmentMethodInfo = _pawnRendererType.GetMethod(
        "DrawEquipment",
        BindingFlags.NonPublic | BindingFlags.Instance);


        // private static FieldInfo PawnFieldInfo;
        private static readonly FieldInfo WoundOverlayFieldInfo = _pawnRendererType.GetField(
                                                                                     "woundOverlays",
                                                                                     BindingFlags.NonPublic | BindingFlags.Instance);

/*
        private static bool _logY = false;
*/
        private static readonly FieldInfo PawnHeadOverlaysFieldInfo = _pawnRendererType.GetField(
                                                                                        "statusOverlays",
                                                                                        BindingFlags.NonPublic | BindingFlags.Instance);




        private static void RecalcRootLocY(ref Vector3 rootLoc, Pawn pawn, CompBodyAnimator compAnimator)
        {
            Vector3 loc = rootLoc;
            CellRect viewRect = Find.CameraDriver.CurrentViewRect;
            viewRect = viewRect.ExpandedBy(1);

            List<Pawn> pawns = new List<Pawn>();
            foreach (Pawn otherPawn in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!viewRect.Contains(otherPawn.Position)) { continue; }
                if (otherPawn == pawn) { continue; }
                if (otherPawn.DrawPos.x < loc.x - 0.5f)
                { continue; }

                if (otherPawn.DrawPos.x > loc.x + 0.5f)
                { continue; }

                if (otherPawn.DrawPos.z >= loc.z) { continue; } // ignore above

                pawns.Add(otherPawn);
            }
            // pawns = pawn.Map.mapPawns.AllPawnsSpawned
            //                .Where(
            //                       otherPawn => _viewRect.Contains(otherPawn.Position) &&
            //                       otherPawn != pawn &&
            //                                    otherPawn.DrawPos.x >= loc.x - 1 &&
            //                                    otherPawn.DrawPos.x <= loc.x + 1 &&
            //                                    otherPawn.DrawPos.z <= loc.z).ToList();
            //  List<Pawn> leftOfPawn = pawns.Where(other => other.DrawPos.x <= loc.x).ToList();
            bool flag = compAnimator != null;
            if (!pawns.NullOrEmpty())
            {
                float pawnOffset = YOffsetPawns * pawns.Count;
                loc.y -= pawnOffset;
                if (flag)
                {
                    compAnimator.DrawOffsetY = pawnOffset;
                }
                //   loc.y -= 0.1f * leftOfPawn.Count;
            }
            else
            {
                if (flag)
                {
                    compAnimator.DrawOffsetY = 0f;
                }
            }
            rootLoc = loc;
        }

        // Original Facial Stuff's method of prefixing RenderPawnInternal and completely bypassing the original routine had 
        // compatibility issues with CombatExtended which modifies the existing routine.
        // Therefore, using a transpiler instead of prefix may be a better option.
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool drawHairMatchFound = false;
            var hairLocVar = il.DeclareLocal(typeof(Vector3));
            List<CodeInstruction> instList = instructions.ToList();
            for(int i = 0; i < instList.Count; ++i)
            {
                var code = instList[i];
                /*
                // In PawnRenderer.RenderPawnInternal from version 1.2.7528
                // Before running the transpiler
                ...
                if(graphics.headGraphic != null)
                {
                    Vector3 b = quaternion * BaseHeadOffsetAt(headFacing);
                    Material material = graphics.HeadMatAt_NewTemp(headFacing, bodyDrawType, headStump, portrait);
                    if (material != null)
                    {
                        GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(headFacing), a + b, quaternion, material, portrait);
                    }
                ...

                // After running the transpiler

                ...
                if(graphics.headGraphic != null)
                {
                    Vector3 b = quaternion * BaseHeadOffsetAt(headFacing);
                    if(!HarmonyPatch_PawnRenderer.TryRenderFacialStuffFace(this, bodyDrawType, portrait, headStump, bodyFacing, headFacing, a, quaternion))
                    {
                        Material material = graphics.HeadMatAt_NewTemp(headFacing, bodyDrawType, headStump, portrait);
                        if(material != null)
                        {
                            GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(headFacing), a + b, quaternion, material, portrait);
                        }
                    }
                ...
                */
                // There is only one Stloc.S 11 instruction in the method
                if(code.opcode == OpCodes.Stloc_S && ((LocalBuilder)code.operand).LocalIndex == 11)
                {                   
                    // Emit Stloc.S 11. Injecting a call instruction right before Stloc.S 11 will cause IL compiler to fail
                    // because there is a returned value from op_Multiply() on top of the stack.
                    yield return code;

                    // Need to find the label to branch to. Upcoming Brfalse.S instruction (should be 12 instructions away 
                    // from the Stloc.S 11 instruction) has the label we want.
                    Label label = new Label();
                    bool foundLabel = false;
                    int j = i;
                    while(++j < instList.Count)
					{
                        CodeInstruction tempCode = instList[j];
                        // Next Brfalse.S instruction has the label we want
                        if(tempCode.opcode == OpCodes.Brfalse_S)
                        {
                            label = (Label)tempCode.operand;
                            foundLabel = true;
                            break;
                        }
                    }
                    if(!foundLabel)
                    {
                        Log.Error("Facial Stuff: Could not patch RenderPawnInternal. Branch label was not found.");
                    } else
                    {
                        // Insert the following codes after the Stloc.S 11 instructon
                        // 1st argument - PawnRenderer instance ("this")
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        // 2nd argument - RotDrawMode bodyDrawType
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                        // 3rd argument - bool portrait
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
                        // 4th argument - bool headStump
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 8);
                        // 5th argument - Rot4 bodyFacing
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                        // 6th argument - Rot4 headFacing
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        // 7th argument - Vec3 headPos ("a")
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        // 8th argument - Quaternion bodyQuat ("quaternion")
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        // Call TryRenderFacialStuffFace using the given arguments
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(TryRenderFacialStuffFace)));
                        // If TryRenderFacialStuffFace returns true, skip the vanilla routine for rendering face
                        yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    }
                    // Continue because Stloc.S 11 instruction has already been emitted.
                    continue;
                }

                /*
                // In PawnRenderer.RenderPawnInternal from version 1.2.7528
                // Before running the transpiler
                ...
                Vector3 loc2 = rootLoc + b;
                loc2.y += 3f / 98f;
		        bool flag = false;
		        if (!portrait || !Prefs.HatsOnlyOnMap)
                {
                ...

                // After running the transpiler
                ...
                Vector3 loc2 = rootLoc + b;
                Vector3 hairLoc = loc2; // Create a new local variable
                loc2.y += 3f / 98f;
		        bool flag = false;
		        if (!portrait || !Prefs.HatsOnlyOnMap)
                {
                ...
                */
                // There is only one Stloc.S 13 instruction in the method.
                if(code.opcode == OpCodes.Stloc_S && ((LocalBuilder)code.operand).LocalIndex == 13)
				{
                    // Duplicate the return value from rootLoc + b because the following Stloc.S 13 instruction
                    // needs the value also
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc_S, hairLocVar);
                }

                /*
                // In PawnRenderer.RenderPawnInternal from version 1.2.7528
                // Before running the transpiler
                ...
                if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
		        {
			        GenDraw.DrawMeshNowOrLater(graphics.HairMeshSet.MeshAt(headFacing), mat: graphics.HairMatAt_NewTemp(headFacing, portrait), loc: loc2, quat: quaternion, drawNow: portrait);
		        }
                ...

                // After running the transpiler
                ...
                if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
		        {
			        GenDraw.DrawMeshNowOrLater(graphics.HairMeshSet.MeshAt(headFacing), mat: graphics.HairMatAt_NewTemp(headFacing, portrait), loc: loc2, quat: quaternion, drawNow: portrait);
		        }
                HarmonyPatch_PawnRenderer.DrawCoveredHair(this, flag, bodyDrawType, portrait, headStump, headFacing, hairLoc, quaternion);
                ...
                */
                // There are two Ldarg.3 instructions in the method. Second instruction is the one we want for injecting method call
                if(code.opcode == OpCodes.Ldarg_3)
				{
                    int j = i;
                    while(++j < instList.Count)
					{
                        var tempCode = instList[j];
                        if(tempCode.opcode == OpCodes.Ldarg_3)
						{
                            // Current Ldarg_3 instruction is not the second ldarg.3 instruction.
                            break;
						}
                        // Stloc.S 21 instructions follow the second Ldarg_3 instruction, but not the first.
                        else if(tempCode.opcode == OpCodes.Stloc_S && ((LocalBuilder)tempCode.operand).LocalIndex == 21)
						{
                            drawHairMatchFound = true;
                            break;
						}
					}
                    if(drawHairMatchFound)
					{
                        // if(!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump) needs to jump to 
                        // the beginning of newly added instructions when the condition isn't met.
                        // Otherwise the instructions will be inside the if block, not outside.
                        List<Label> tempLabels = new List<Label>();
                        code.labels.ForEach(lbl => tempLabels.Add(lbl));
                        code.labels.Clear();
                        // 1st argument - PawnRenderer instance ("this")
                        var ldarg0Inst = new CodeInstruction(OpCodes.Ldarg_0);
                        //  Copy over the labels to the new instruction
                        tempLabels.ForEach(lbl => ldarg0Inst.labels.Add(lbl));
                        yield return ldarg0Inst;
                        // 2nd argument - bool hideHair ("flag")
                        //  Combat Extended's transpiler searches for Ldloc.S 14 instruction but the instruction added by this transpiler
                        //  comes after the Ldloc.S 14 instruction that Combat Extended looks for. Therefore this shouldn't be a problem.
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 14);
                        // 3rd argument - RotDrawMode bodyDrawType
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                        // 4th argument - bool portrait
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
                        // 5th argument - bool headStump
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 8);
                        // 6th argument - Rot4 headFacing
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        // 7th argument - Vector3 hairLoc
                        yield return new CodeInstruction(OpCodes.Ldloc_S, hairLocVar);
                        // 8th argument - Quaternion bodyQuat ("quaternion")
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        // Call DrawCoveredHair() using the given arguments
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(TryDrawCoveredHair)));
                    }
                }

                yield return code;
            }
            if(!drawHairMatchFound)
			{
                Log.Error("Facial Stuff: Failed to inject TryDrawCoveredHair method call.");
			}
        }

        // Render Facial Stuff face if applicable. Returns true if Facial Stuff face was rendered, return false if vanilla behavior is desired.
        public static bool TryRenderFacialStuffFace(
            PawnRenderer pawnRenderer, 
            RotDrawMode bodyDrawType,
            bool portrait,
            bool headStump, 
            Rot4 bodyFacing,
            Rot4 headFacing, 
            Vector3 headPos, 
            Quaternion bodyQuat)
        {
            PawnGraphicSet graphics = pawnRenderer.graphics;
            Pawn pawn = graphics.pawn;
            CompFace compFace = pawn.GetCompFace();

            if(compFace != null)
            {
                compFace.TickDrawers(bodyFacing, headFacing, graphics);
                return compFace.DrawHead(bodyDrawType, portrait, headStump, bodyFacing, headFacing, headPos, bodyQuat);
            }
            return false;
        }
        
        public static void TryDrawCoveredHair(
            PawnRenderer pawnRenderer, 
            bool hideHair, 
            RotDrawMode bodyDrawType, 
            bool portrait, 
            bool headStump, 
            Rot4 headFacing, 
            Vector3 hairLoc, 
            Quaternion bodyQuat)
		{
            CompFace compFace = pawnRenderer.graphics.pawn.GetCompFace();
            if(compFace != null && hideHair && bodyDrawType != RotDrawMode.Dessicated && !headStump)
			{
                // Draw masked hair if headwear "hides" the hair
                HairCut.HairCutPawn hairPawn = HairCut.CutHairDB.GetHairCache(pawnRenderer.graphics.pawn);
                Material hairCutMat = hairPawn.HairCutMatAt(headFacing);
                Mesh hairMesh = pawnRenderer.graphics.HairMeshSet.MeshAt(headFacing);
                if(hairCutMat != null)
                {
                    GenDraw.DrawMeshNowOrLater(hairMesh, hairLoc, bodyQuat, hairCutMat, portrait);
                }
            }
		}

        public static bool DoNotUseThisPrefix(PawnRenderer __instance,
                                  ref Vector3 rootLoc,
                                  float angle,
                                  bool renderBody,
                                  Rot4 bodyFacing,
                                  Rot4 headFacing,
                                  RotDrawMode bodyDrawType,
                                  bool portrait,
                                  bool headStump, 
                                  bool invisible)
        {
            // Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
            PawnGraphicSet graphics = __instance.graphics;

            Pawn pawn = graphics.pawn;

            if (!pawn.RaceProps.Humanlike && !Controller.settings.UsePaws)
            {
                return true;

            }

            if (!graphics.AllResolved)
            {
                graphics.ResolveAllGraphics();
            }

            CompFace compFace = pawn.GetCompFace();
            bool hasFace = compFace != null;

            // Let vanilla do the job if no FacePawn or pawn not a teenager or any other known mod accessing the renderer
            if (hasFace)
            {
                if (pawn.IsChild() || pawn.GetCompAnim().Deactivated)
                {
                    return true;
                }

            }

            CompBodyAnimator compAnim = pawn.GetCompAnim();
            bool showFeet = compAnim != null && Controller.settings.UseFeet;

            // No face, no animator, return
            if (!hasFace && compAnim == null)
            {
                return true;
            }

            PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);

            // if (Patches2.Plants)
            // {
            //     if (pawn.Spawned)
            //     {
            //         Plant plant = (pawn.Position + IntVec3.South).GetPlant(pawn.Map);
            //         if (plant != null && Patches2.plantMoved.Contains(plant))
            //         {
            //             rootLoc.y = plant.DrawPos.y - (Patches2.steps / 2);
            //         }
            //     }
            // }

            // Try to move the y position behind while another pawn is standing near
            if (compAnim != null && (!portrait && pawn.Spawned && !compAnim.IsRider))
            {
                RecalcRootLocY(ref rootLoc, pawn, compAnim);
            }

            Vector3 baseDrawLoc = rootLoc;
            // Let's save the basic location for later
            Vector3 footPos = baseDrawLoc;



            // No face => must be animal, simplify it
            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Quaternion bodyQuat = quat;
            Quaternion footQuat = bodyQuat;


            if (HarmonyPatchesFS.AnimatorIsOpen())
            {
                bodyFacing = MainTabWindow_BaseAnimator.BodyRot;
                headFacing = MainTabWindow_BaseAnimator.HeadRot;
            }

            compFace?.TickDrawers(bodyFacing, headFacing, graphics);

            compAnim?.TickDrawers(bodyFacing, graphics);

            // Use the basic quat
            Quaternion headQuat = bodyQuat;

            // Rotate head if possible and wobble around
            if (!portrait || HarmonyPatchesFS.AnimatorIsOpen())
            {
                if (showFeet)
                {
                    compAnim.ApplyBodyWobble(ref baseDrawLoc, ref footPos, ref bodyQuat);
                }

                // Reset the quat as it has been changed
                headQuat = bodyQuat;
                compFace?.ApplyHeadRotation(renderBody, ref headQuat);
            }

            // Regular FacePawn rendering 14+ years

            // Render body
            // if (renderBody)
            compAnim?.DrawBody(baseDrawLoc, bodyQuat, bodyDrawType, woundDrawer, renderBody, portrait);

            Vector3 bodyPos = baseDrawLoc;
            Vector3 headPos = baseDrawLoc;
            if (bodyFacing == Rot4.North)
            {
                headPos.y += YOffset_Shell;
                bodyPos.y += YOffset_Head;
            }
            else
            {
                headPos.y += YOffset_Head;
                bodyPos.y += YOffset_Shell;
            }


            if (graphics.headGraphic != null)
            {
                // Rendererd pawn faces

                /*Vector3 offsetAt = !hasFace
                                   ? __instance.BaseHeadOffsetAt(bodyFacing)
                                   : compFace.BaseHeadOffsetAt(portrait, pawn);

                Vector3 b = bodyQuat * offsetAt;
                Vector3 headDrawLoc = headPos + b;

                if (!hasFace)
                {
                    Material material = graphics.HeadMatAt_NewTemp(headFacing, bodyDrawType, headStump);
                    if (material != null)
                    {
                        Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                        GenDraw.DrawMeshNowOrLater(mesh2, headDrawLoc, quat, material, portrait);
                    }
                }
                else
                {
                    compFace.DrawBasicHead(out bool headDrawn, bodyDrawType, portrait, headStump, headDrawLoc, headQuat);
                    if (headDrawn)
                    {
                        if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                        {
                            if (compFace.Props.hasWrinkles)
                            {
                                Vector3 wrinkleLoc = headDrawLoc;
                                wrinkleLoc.y += YOffset_Wrinkles;
                                compFace.DrawWrinkles(bodyDrawType, wrinkleLoc, headQuat, portrait);
                            }

                            if (compFace.Props.hasEyes)
                            {
                                Vector3 eyeLoc = headDrawLoc;
                                eyeLoc.y += YOffset_Eyes;

                                compFace.DrawNaturalEyes(eyeLoc, portrait, headQuat);

                                Vector3 browLoc = headDrawLoc;
                                browLoc.y += YOffset_Brows;
                                // the brow above
                                compFace.DrawBrows(browLoc, headQuat, portrait);

                                // and now the added eye parts
                                Vector3 unnaturalEyeLoc = headDrawLoc;
                                unnaturalEyeLoc.y += YOffset_UnnaturalEyes;
                                compFace.DrawUnnaturalEyeParts(unnaturalEyeLoc, headQuat, portrait);
                            }
                            if (compFace.Props.hasEars && Controller.settings.Develop)
                            {
                                Vector3 earLor = headDrawLoc;
                                earLor.y += YOffset_Eyes;

                                compFace.DrawNaturalEars(earLor, portrait, headQuat);

                                // and now the added ear parts
                                Vector3 drawLoc = headDrawLoc;
                                drawLoc.y += YOffset_UnnaturalEyes;
                                compFace.DrawUnnaturalEarParts(drawLoc, headQuat, portrait);
                            }

                            // Portrait obviously ignores the y offset, thus render the beard after the body apparel (again)
                            if (compFace.Props.hasBeard)
                            {
                                Vector3 beardLoc = headDrawLoc;
                                Vector3 tacheLoc = headDrawLoc;

                                beardLoc.y += headFacing == Rot4.North ? -YOffset_Head - YOffset_Beard : YOffset_Beard;
                                tacheLoc.y += headFacing == Rot4.North ? -YOffset_Head- YOffset_Tache : YOffset_Tache;

                                compFace.DrawBeardAndTache(beardLoc, tacheLoc, portrait, headQuat);
                            }

                            if (compFace.Props.hasMouth)
                            {
                                Vector3 mouthLoc = headDrawLoc;
                                mouthLoc.y += YOffset_Mouth;
                                compFace.DrawNaturalMouth(mouthLoc, portrait, headQuat);
                            }
                            // Deactivated, looks kinda crappy ATM
                            // if (pawn.Dead)
                            // {
                            // Material deadEyeMat = faceComp.DeadEyeMatAt(headFacing, bodyDrawType);
                            // if (deadEyeMat != null)
                            // {
                            // GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, deadEyeMat, portrait);
                            // locFacialY.y += YOffsetInterval_OnFace;
                            // }

                            // }
                            // else
                        }
                    }

                }


                if (!headStump)
                {
                    Vector3 overHead = baseDrawLoc + b;
                    overHead.y += YOffset_OnHead;

                    Vector3 hairLoc = overHead;
                    Vector3 headgearLoc = overHead;
                    Vector3 hatInFrontOfFace = baseDrawLoc + b;


                    hairLoc.y += YOffset_HairOnHead;
                    headgearLoc.y += YOffset_GearOnHead;
                    hatInFrontOfFace.y += ((!(headFacing == Rot4.North)) ? YOffset_PostHead : YOffset_Behind);

                    compFace?.DrawHairAndHeadGear(hairLoc, headgearLoc,
                                                 bodyDrawType,
                                                 portrait,
                                                 renderBody,
                                                 headQuat, hatInFrontOfFace);

                    compFace?.DrawAlienHeadAddons(headPos, portrait, headQuat, overHead);
                }*/

            }

            if (!portrait)
            {
                //   Traverse.Create(__instance).Method("DrawEquipment", rootLoc).GetValue();

                DrawEquipmentMethodInfo?.Invoke(__instance, new object[] { baseDrawLoc });
            }

            if (!portrait)
            {
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    foreach (Apparel ap in wornApparel)
                    {
                        DrawPos_Patch.offset = baseDrawLoc;
						DrawPos_Patch.offsetEnabled = true;
						ap.DrawWornExtras();
						DrawPos_Patch.offsetEnabled = false;
                    }
                }

                Vector3 bodyLoc = baseDrawLoc;
                bodyLoc.y += YOffset_Status;

                PawnHeadOverlays headOverlays = (PawnHeadOverlays)PawnHeadOverlaysFieldInfo?.GetValue(__instance);
                if (headOverlays != null)
                {
                    compFace?.DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                }
            }


            compAnim?.DrawApparel(bodyQuat, bodyPos, portrait, renderBody);

            compAnim?.DrawAlienBodyAddons(bodyQuat, bodyPos, portrait, renderBody, bodyFacing, invisible);

            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0
             && graphics.packGraphic != null)
            {
                Mesh mesh = graphics.nakedGraphic.MeshAt(bodyFacing);
                Graphics.DrawMesh(mesh, bodyPos, quat, graphics.packGraphic.MatAt(bodyFacing), 0);
            }

            // No wobble for equipment, looks funnier - nah!
            // Vector3 equipPos = rootLoc;
            // equipPos.y = drawPos.y;

            //compAnim.DrawEquipment(drawPos, portrait);



            bool showHands = Controller.settings.UseHands;
            Vector3 handPos = bodyPos;
            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                if (showHands)
                {
                    // Reset the position for the hands
                    handPos.y = baseDrawLoc.y;
                    compAnim?.DrawHands(bodyQuat, handPos, portrait);
                }

                if (showFeet)
                {
                    compAnim.DrawFeet(bodyQuat, footQuat, footPos, portrait);
                }
            }

            return false;
        }
        
        private static float GetBodysizeScaling(float bodySizeFactor, Pawn pawn)
        {
        float num = bodySizeFactor;
        float num2 = 1f;
            try
        {
            int curLifeStageIndex = pawn.ageTracker.CurLifeStageIndex;
            int num3 = pawn.RaceProps.lifeStageAges.Count - 1;
            LifeStageAge val = pawn.RaceProps.lifeStageAges[curLifeStageIndex];
            if (num3 == curLifeStageIndex && curLifeStageIndex != 0 && bodySizeFactor != 1f)
            {
                LifeStageAge val2 = pawn.RaceProps.lifeStageAges[curLifeStageIndex - 1];
                num = val2.def.bodySizeFactor + (float)Math.Round((val.def.bodySizeFactor - val2.def.bodySizeFactor) / (val.minAge - val2.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val2.minAge), 2);
            }
            else if (num3 == curLifeStageIndex)
            {
                num = bodySizeFactor;
            }
            else if (curLifeStageIndex == 0)
            {
                LifeStageAge val3 = pawn.RaceProps.lifeStageAges[curLifeStageIndex + 1];
                num = val.def.bodySizeFactor + (float)Math.Round((val3.def.bodySizeFactor - val.def.bodySizeFactor) / (val3.minAge - val.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val.minAge), 2);
            }
            else
            {
                LifeStageAge val3 = pawn.RaceProps.lifeStageAges[curLifeStageIndex + 1];
                num = val.def.bodySizeFactor + (float)Math.Round((val3.def.bodySizeFactor - val.def.bodySizeFactor) / (val3.minAge - val.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val.minAge), 2);
            }
            if (pawn.RaceProps.baseBodySize > 0f)
            {
                num2 = pawn.RaceProps.baseBodySize;
            }
        }
        catch
        {
        }
        return num * num2;
        }
    }
}
