using UnityEngine;
using Verse;
using CommunityCoreLibrary;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace RW_FacialStuff
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
            if (_reinjectNeeded)
            {
                _reinjectTime -= Time.fixedDeltaTime;

                if (_reinjectTime <= 0)
                {
                    _reinjectNeeded = false;
                    _reinjectTime = 0;

#if LOG
                    Log.Message("AutoEquip Injected");
#endif
                    MapComponent_FacialStuff component = MapComponent_FacialStuff.Get;
                }
            }
        }

        public void Start()
        {
                 MethodInfo coreMethod = typeof(Verse.PawnGraphicSet).GetMethod("ResolveAllGraphics", BindingFlags.Instance | BindingFlags.Public);
                 MethodInfo autoEquipMethod = typeof(RW_FacialStuff.PawnGraphicHairSet).GetMethod("ResolveAllGraphics", BindingFlags.Instance | BindingFlags.Public);

     


   //       MethodInfo coreMethod2 = typeof(Verse.PawnRenderer).GetMethod("RenderPawnInternal",
   //           BindingFlags.Instance | BindingFlags.NonPublic,
   //           Type.DefaultBinder, new[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode) }, null);
   //
   //       MethodInfo autoEquipMethod2 = typeof(FS_PawnRenderer).GetMethod("RenderPawnInternal", BindingFlags.Instance | BindingFlags.NonPublic,
   //           Type.DefaultBinder, new[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode) }, null);

            try
            {
                            Detours.TryDetourFromTo(coreMethod, autoEquipMethod);

     //           Detours.TryDetourFromTo(coreMethod2, autoEquipMethod2);
            }
            catch (Exception)
            {
                Log.Error("Could not Detour Graphics.");
                throw;
            }


        }
    }
}
