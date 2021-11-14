using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PawnPlus.AnimatorWindows;
using PawnPlus.Graphics;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static PawnPlus.Offsets;

namespace PawnPlus.Harmony
{    
    [HarmonyPatch(
        typeof(PawnRenderer),
        "RenderPawnInternal",
        new[]
        {
            typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode),
            typeof(bool), typeof(bool), typeof(bool)
        })]
    public static class HarmonyPatch_PawnRenderer
    {
        public struct ExtraLocalVar
        {
            public CompFace compFace;
            public CompBodyAnimator compAnim;
            public Vector3 footPos;
            public Vector3 headOffsetBodyWobble;
        }
        
        // PawnRenderer.RenderPawnInternal from version 1.2.7528
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            int bodyPatchState = 0;
            int headPatchState = 0;
            int hairPatchState = 0;
            int handPatchState = 0;

            // Declare ExtraLocalVar
            LocalBuilder extraLocalVar = il.DeclareLocal(typeof(ExtraLocalVar));
            yield return new CodeInstruction(OpCodes.Ldloca_S, extraLocalVar);
            yield return new CodeInstruction(OpCodes.Initobj, typeof(ExtraLocalVar));
            // Push function arguments for InitializeExtraLocals()
            yield return new CodeInstruction(OpCodes.Ldloca_S, extraLocalVar);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            // Call InitializeExtraLocals() function
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(InitializeExtraLocals)));

            List<CodeInstruction> instList = instructions.ToList();
            for(int i = 0; i < instList.Count; ++i)
            {
                var code = instList[i];

                /*
                // Before running the transpiler
                ...
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                Mesh mesh = null;
                if (renderBody)
                {
                ...

                // After running the transpiler
                ...
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                Mesh mesh = null;
                TryUpdateCompBody(ref ExtraLocalVar extraLocalVar, PawnRenderer pawnRenderer, Rot4 bodyFacing, Vector3 rootLoc, ref Quaternion bodyQuat, bool portrait);
                if (renderBody)
                {
                ...
                */
                if(code.opcode == OpCodes.Ldarg_3 && bodyPatchState == 0)
                {
                    // 1st argument - ExtraLocalVar extraLocalVar
                    yield return new CodeInstruction(OpCodes.Ldloca_S, extraLocalVar);
                    // 2nd argument - PawnRenderer instance ("this")
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    // 3rd argument - Rot4 bodyFacing
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    // 4th argument - ref Rot4 headFacing
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 5);
                    // 5th argument - ref Vector4 rootLoc
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
                    // 6th argument - ref Quaternion quaternion ("bodyQuat")
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    // 7th argument - bool portrait
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
                    // Call TryUpdateCompBody using the given arguments
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(TryUpdateBodyOrientation)));
                    bodyPatchState = 1;
                }
                
                /*
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
                if(code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder lb1 && lb1.LocalIndex == 11 && headPatchState == 0)
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
                        Log.Error("Pawn Plus: Could not patch RenderPawnInternal. Branch label was not found.");
                    } else
                    {
                        // Insert the following codes after the Stloc.S 11 instructon
                        // 1st argument - ExtraLocalVar extraLocalVar
                        yield return new CodeInstruction(OpCodes.Ldloca_S, extraLocalVar);
                        // 2nd argument - PawnRenderer instance ("this")
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        // 3rd argument - RotDrawMode bodyDrawType
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                        // 4th argument - bool portrait
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
                        // 5th argument - bool headStump
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 8);
                        // 6th argument - Rot4 bodyFacing
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                        // 7th argument - Rot4 headFacing
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        // 8th argument - Vec3 headPos ("a")
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        // 9th argument - Quaternion bodyQuat ("quaternion")
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        // Call TryRenderFacialStuffFace using the given arguments
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(TryRenderFacialStuffHead)));
                        // If TryRenderFacialStuffFace returns true, skip the vanilla routine for rendering face
                        yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                        headPatchState = 1;
                    }
                    // Continue because Stloc.S 11 instruction has already been emitted.
                    continue;
                }

                /*
                // Before running the transpiler
                ...
                if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
		        {
			        GenDraw.DrawMeshNowOrLater(graphics.HairMeshSet.MeshAt(headFacing), mat: graphics.HairMatAt_NewTemp(headFacing, portrait), loc: loc2, quat: quaternion, drawNow: portrait);
		        }
                ...

                // After running the transpiler
                ...
                flag = true;
                if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
		        {
			        GenDraw.DrawMeshNowOrLater(graphics.HairMeshSet.MeshAt(headFacing), mat: graphics.HairMatAt_NewTemp(headFacing, portrait), loc: loc2, quat: quaternion, drawNow: portrait);
		        }
                ...
                */
                if(code.opcode == OpCodes.Ldloc_S && code.operand is LocalBuilder lb2 && lb2.LocalIndex == 14 && hairPatchState == 0)
				{
                    if(instList[i+1].opcode == OpCodes.Brtrue_S)
					{
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Stloc_S, lb2);
                        hairPatchState = 1;
                    }
				}

                /*
                // Before running the transpiler
                ...
                if (portrait)
	            {
		            return;
	            }
	            DrawEquipment(rootLoc);
	            if (pawn.apparel != null)
                ...

                // After running the transpiler
                ...
                if (portrait)
	            {
		            return;
	            }
	            DrawEquipment(rootLoc);
                TryRenderHandAndFoot(ref extraLocalVar, rootLoc, bodyQuat, renderBody);
	            if (pawn.apparel != null)
                ...
                */
                // Hand and feet positions are also modified in RenderPawnInternal.DrawEquipment. Therefore hand and feet need to be rendered after that.
                if(code.opcode == OpCodes.Call && ((MethodInfo)code.operand).Name.Equals("DrawEquipment") && handPatchState == 0)
				{
                    // Emit CallVirt DrawEquipment instruction. The newly added codes need to come after DrawEquipment().
                    yield return code;
                    // 1st argument - ExtraLocalVar extraLocalVar
                    yield return new CodeInstruction(OpCodes.Ldloca_S, extraLocalVar);
                    // 2nd argument - Vector3 rootLoc
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    // 3rd argument - Quaternion bodyQuat ("quaternion")
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    // 4th argument - bool renderBody
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    // Call TryRenderHandAndFoot() function
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(TryRenderHandAndFoot)));

                    handPatchState = 1;
                    // CallVirt DrawEquipment instruction has already been emitted, so continue without emitting the same code again.
                    continue;
				}

                yield return code;
            }
            if(bodyPatchState != 1)
            {
                Log.Warning("Pawn Plus: code for body animation wasn't injected");
            }
            if(headPatchState != 1)
            {
                Log.Warning("Pawn Plus: code for rendering head wasn't injected");
            }
            if(hairPatchState != 1)
			{
                Log.Warning("Pawn Plus: code for overriding head rendering wasn't injected");
			}
            if(handPatchState != 1)
			{
                Log.Warning("Pawn Plus: code for rendering hand and feet wasn't injected");
			}
        }

        public static void InitializeExtraLocals(ref ExtraLocalVar extraLocalVar, PawnRenderer pawnRenderer)
		{
            Pawn pawn = pawnRenderer.graphics.pawn;
            extraLocalVar.compFace = pawn.GetCompFace();
            extraLocalVar.compAnim = pawn.GetCompAnim();
		}

        public static void TryUpdateBodyOrientation(
            ref ExtraLocalVar extraLocalVar,
            PawnRenderer pawnRenderer, 
            Rot4 bodyFacing,
            ref Rot4 headFacing,
            ref Vector3 rootLoc, 
            ref Quaternion bodyQuat, 
            bool portrait)
        {
            PawnGraphicSet graphics = pawnRenderer.graphics;
            CompBodyAnimator compAnim = extraLocalVar.compAnim;
            CompFace compFace = extraLocalVar.compFace;

            if(compAnim != null)
			{
                compAnim.TickDrawers(bodyFacing, graphics);
                if(Controller.settings.UseFeet)
                {
                    Vector3 footPos = rootLoc;
                    compAnim.ApplyBodyWobble(ref rootLoc, ref footPos, ref bodyQuat);
                    extraLocalVar.footPos = footPos;
                }
                if(compAnim.BodyAnim != null)
				{
                    extraLocalVar.headOffsetBodyWobble = compAnim.BodyAnim.headOffset;
                }
                else
				{
                    extraLocalVar.headOffsetBodyWobble = Vector3.zero;
                }
            }
            if(compFace != null && !portrait)
			{
                // headFacing needs to be updated for vanilla hair and headwear rendering routine
                headFacing = compFace.GetHeadFacing();
            }
        }

        // Render Facial Stuff head if applicable. Returns true if Facial Stuff head was rendered, return false if vanilla behavior is desired.
        public static bool TryRenderFacialStuffHead(
            ref ExtraLocalVar extraLocalVar,
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
            CompFace compFace = extraLocalVar.compFace;
            if(compFace != null)
            {
                Vector3 headBaseOffset = pawnRenderer.BaseHeadOffsetAt(headFacing) + extraLocalVar.headOffsetBodyWobble;
                Vector3 headPosOffset = bodyQuat * headBaseOffset;
                Vector3 headDrawLoc = headPos + headPosOffset;
                return compFace.DrawHead(graphics, bodyDrawType, portrait, headStump, bodyFacing, headFacing, headDrawLoc, bodyQuat);
            }
            return false;
        }

        public static void TryRenderHandAndFoot(ref ExtraLocalVar extraLocalVar, Vector3 rootLoc, Quaternion bodyQuat, bool renderBody)
        {
            CompBodyAnimator compAnim = extraLocalVar.compAnim;
            if(compAnim != null)
            {
                Vector3 handPos = rootLoc;
                Quaternion footQuat = bodyQuat;
                if(renderBody || Controller.settings.IgnoreRenderBody)
                {
                    if(Controller.settings.UseHands)
                    {
                        compAnim.DrawHands(bodyQuat, handPos, false);
                    }
                    if(Controller.settings.UseFeet)
                    {
                        compAnim.DrawFeet(bodyQuat, footQuat, extraLocalVar.footPos, false);
                    }
                }
            }
        }
    }
}
