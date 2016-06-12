using UnityEngine;
using Verse;
using CommunityCoreLibrary;
using System.Reflection;
using System;

namespace RW_FacialHair
{

    public class ModInitializer : ITab
    {
        protected GameObject modInitializerControllerObject;

        public ModInitializer()
        {
            modInitializerControllerObject = new GameObject("BeardyFaces");
            modInitializerControllerObject.AddComponent<ModInitializerBehaviour>();
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)modInitializerControllerObject);
        }

        protected override void FillTab() { }
    }
    class ModInitializerBehaviour : MonoBehaviour
    {
      

        public void Start()
        {
            MethodInfo coreMethod = typeof(Verse.PawnGraphicSet).GetMethod("ResolveAllGraphics", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo autoEquipMethod = typeof(RW_FacialHair.PawnGraphicHairSet).GetMethod("ResolveAllGraphics", BindingFlags.Instance | BindingFlags.Public);

    //      MethodInfo coreMethod2 = typeof(Verse.PawnRenderer).GetMethod("RenderPawnInternal", BindingFlags.Instance | BindingFlags.NonPublic);
    //      MethodInfo autoEquipMethod2 = typeof(RW_FacialHair.PawnBeardRenderer).GetMethod("RenderPawnInternal", BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                Detours.TryDetourFromTo(coreMethod, autoEquipMethod);

      //          Detours.TryDetourFromTo(coreMethod2, autoEquipMethod2);
            }
            catch (Exception)
            {
                Log.Error("Could not Detour Graphics.");
                throw;
            }


        }
    }
}
