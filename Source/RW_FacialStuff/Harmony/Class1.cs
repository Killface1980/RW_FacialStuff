using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Harmony
{
    using System.Reflection;
    using System.Reflection.Emit;

    using AlienRace;

    using global::Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public static class AlienFaces
    {

        public static void DrawAddons(bool portrait, Pawn pawn, Rot4 headFacing, Vector3 vector)
        {
            
        }

        public static Mesh GetPawnHairMesh(bool portrait, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics)
        {
            
        }

        public static bool GetPawnMesh(ref Mesh __result, bool portrait, Pawn pawn, Rot4 facing, bool wantsBody)
        {
           __result = Traverse.Create("AlienRace.HarmonyPatches".GetType()).Field("GetPawnMesh").GetValue<Mesh>();

            return false;
        }
    }
}
