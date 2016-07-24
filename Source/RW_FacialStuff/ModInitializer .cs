using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommunityCoreLibrary;
using RimWorld;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace RW_FacialStuff
{

    public class ModInitializer : ITab
    {
        protected GameObject modInitializerControllerObject;

        public ModInitializer()
        {
            modInitializerControllerObject = new GameObject("BeardyFaces");
            modInitializerControllerObject.AddComponent<ModInitializerBehaviour>();
            Object.DontDestroyOnLoad(modInitializerControllerObject);
        }

        protected override void FillTab() { }
    }


    class ModInitializerBehaviour : MonoBehaviour
    {
        protected bool _reinjectNeeded;
        protected float _reinjectTime;

        public void OnLevelWasLoaded(int level)
        {
            _reinjectNeeded = true;
            if (level >= 0)
                _reinjectTime = 1;
            else
                _reinjectTime = 0;
        }

        public void FixedUpdate()
        {
        }



        public void Start()
        {
            MethodInfo coreMethod = typeof(PawnGraphicSet).GetMethod("ResolveAllGraphics", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo moddedHeadMethod = typeof(PawnGraphicSetModded).GetMethod("ResolveAllGraphicsModded", BindingFlags.Instance | BindingFlags.Public);

            MethodInfo coreMethod3 = typeof(RimWorld.PawnHairColors).GetMethod("RandomHairColor", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod3 = typeof(PawnHairColors).GetMethod("RandomHairColor", BindingFlags.Static | BindingFlags.Public);

            MethodInfo coreMethod4 = typeof(PawnSkinColors).GetMethod("GetSkinColor", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod4 = typeof(PawnSkinColorsModded).GetMethod("GetSkinColor", BindingFlags.Static | BindingFlags.Public);

            MethodInfo coreMethod5 = typeof(PawnHairChooser).GetMethod("RandomHairDefFor", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod5 = typeof(PawnFaceMaker).GetMethod("RandomHairDefFor", BindingFlags.Static | BindingFlags.Public);

            try
            {
                Detours.TryDetourFromTo(coreMethod, moddedHeadMethod);
                Detours.TryDetourFromTo(coreMethod3, moddedHeadMethod3);
                Detours.TryDetourFromTo(coreMethod4, moddedHeadMethod4);
                Detours.TryDetourFromTo(coreMethod5, moddedHeadMethod5);

            }
            catch (Exception)
            {
                Log.Error("Could not detour graphics");
                throw;
            }


        }
    }
}
