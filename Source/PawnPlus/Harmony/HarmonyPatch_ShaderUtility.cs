﻿using PawnPlus.Graphics;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Harmony
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
            if(shader == Shaders.Hair)
			{
                __result = true;
                return false;
			}
            return true;
		}
	}
}
