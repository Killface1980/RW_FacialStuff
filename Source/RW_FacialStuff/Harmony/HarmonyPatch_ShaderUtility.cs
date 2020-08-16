using FacialStuff.GraphicsFS;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FacialStuff.Harmony
{
    [HarmonyPatch(
        typeof(ShaderUtility),
        "SupportsMaskTex",
        new[]
        {
            typeof(Shader)
        })]
    class HarmonyPatch_ShaderUtility
	{
        public static bool Prefix(ref bool __result, Shader shader)
		{
            if(shader == Graphic_Hair.HairShader)
			{
                __result = true;
                return false;
			}
            return true;
		}
	}
}
