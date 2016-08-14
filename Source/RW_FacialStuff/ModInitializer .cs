using System;
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
            modInitializerControllerObject = new GameObject("Facial Stuff");
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
  //        MethodInfo method = typeof(GraphicDatabaseUtility).GetMethod("GraphicNamesInFolder", BindingFlags.Static | BindingFlags.Public);
  //        MethodInfo method2 = typeof(GraphicDatabaseUtilityFS).GetMethod("GraphicNamesInFolder", BindingFlags.Static | BindingFlags.Public);

            MethodInfo coreMethod = typeof(PawnGraphicSet).GetMethod("ResolveAllGraphics", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo moddedHeadMethod = typeof(PawnGraphicSetModded).GetMethod("ResolveAllGraphicsModded", BindingFlags.Instance | BindingFlags.Public);

            MethodInfo coreMethod2 = typeof(PawnHairChooser).GetMethod("RandomHairDefFor", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod2 = typeof(PawnFaceChooser).GetMethod("RandomHairDefFor", BindingFlags.Static | BindingFlags.Public);

            MethodInfo coreMethod3 = typeof(RimWorld.PawnHairColors).GetMethod("RandomHairColor", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod3 = typeof(PawnHairColors).GetMethod("RandomHairColorModded", BindingFlags.Static | BindingFlags.Public);

            MethodInfo coreMethod4 = typeof(PawnSkinColors).GetMethod("GetSkinColor", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod4 = typeof(PawnSkinColorsModded).GetMethod("GetSkinColor", BindingFlags.Static | BindingFlags.Public);

            MethodInfo coreMethod5 = typeof(PawnSkinColors).GetMethod("IsDarkSkin", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod5 = typeof(PawnSkinColorsModded).GetMethod("IsDarkSkin", BindingFlags.Static | BindingFlags.Public);

            MethodInfo coreMethod6 = typeof(PawnSkinColors).GetMethod("RandomSkinWhiteness", BindingFlags.Static | BindingFlags.Public);
            MethodInfo moddedHeadMethod6 = typeof(PawnSkinColorsModded).GetMethod("RandomSkinWhiteness", BindingFlags.Static | BindingFlags.Public);


            //       MethodInfo coreMethod6 = typeof(ZombieMod_Utility).GetMethod("Zombify", BindingFlags.Static | BindingFlags.Public);
            //        MethodInfo moddedHeadMethod6 = typeof(ZombieMod_UtilityFS).GetMethod("Zombify", BindingFlags.Static | BindingFlags.Public);
            try
            {
      //          Detours.TryDetourFromTo(method, method2);
                Detours.TryDetourFromTo(coreMethod, moddedHeadMethod);
                //       Detours.TryDetourFromTo(coreMethod2, moddedHeadMethod2);
                //       Detours.TryDetourFromTo(coreMethod3, moddedHeadMethod3);
                Detours.TryDetourFromTo(coreMethod4, moddedHeadMethod4);
                Detours.TryDetourFromTo(coreMethod5, moddedHeadMethod5);
                Detours.TryDetourFromTo(coreMethod6, moddedHeadMethod6);
                //        Detours.TryDetourFromTo(coreMethod6, moddedHeadMethod6);

            }
            catch (Exception)
            {
                Log.Error("Could not detour graphics");
                throw;
            }


        }
    }
}
