namespace PawnPlus.Harmony
{
    using HarmonyLib;

    using PawnPlus.Graphics;

    using UnityEngine;

    using Verse;

    [HarmonyPatch(
        typeof(ShaderUtility),
        "SupportsMaskTex",
        typeof(Shader))]
    class HarmonyPatch_ShaderUtility
	{
        public static bool Prefix(ref bool __result, Shader shader)
		{
            if(shader == Shaders.Hair)
			{
                __result = true;
                return false;
			}

            return true;
		}
	}
}
