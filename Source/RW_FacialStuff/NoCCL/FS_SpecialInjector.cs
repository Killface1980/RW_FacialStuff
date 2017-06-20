// Toggle in Hospitality Properties

using System;
using System.Reflection;
using FacialStuff.NoCCL;
using Verse;

namespace FacialStuff.Initializer
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.NoCCL;

    public class FS_SpecialInjector : SpecialInjector
    {

        private static Assembly Assembly { get { return Assembly.GetAssembly(typeof(FS_SpecialInjector)); } }

        private static readonly BindingFlags[] bindingFlagCombos = {
            BindingFlags.Instance | BindingFlags.Public, BindingFlags.Static | BindingFlags.Public,
            BindingFlags.Instance | BindingFlags.NonPublic, BindingFlags.Static | BindingFlags.NonPublic
        };

        public override bool Inject()
        {

            #region Automatic hookup
            // Loop through all detour attributes and try to hook them up
            foreach (Type targetType in Assembly.GetTypes())
            {
                foreach (BindingFlags bindingFlags in bindingFlagCombos)
                {
                    foreach (MethodInfo targetMethod in targetType.GetMethods(bindingFlags))
                    {
                        foreach (DetourAttribute detour in targetMethod.GetCustomAttributes(typeof(DetourAttribute), true))
                        {
                            BindingFlags flags = detour.bindingFlags != default(BindingFlags) ? detour.bindingFlags : bindingFlags;
                            MethodInfo sourceMethod = detour.source.GetMethod(targetMethod.Name, flags);
                            if (sourceMethod == null)
                            {
                                Log.Error(string.Format("Facial Stuff :: Detours :: Can't find source method '{0} with bindingflags {1}", targetMethod.Name, flags));
                                return false;
                            }
                            {                                
                            if (!Detours.TryDetourFromTo(sourceMethod, targetMethod))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

#if develop
            // Inject Dev Face tab
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(td => td.category == ThingCategory.Pawn && td.race.Humanlike))
            {
                if (def.inspectorTabs == null || def.inspectorTabs.Count == 0)
                {
                    def.inspectorTabs = new List<Type>();
                    def.inspectorTabsResolved = new List<InspectTabBase>();
                }
                if (def.inspectorTabs.Contains(typeof(ITab_Pawn_Face)))
                {
                    return false;
                }

                def.inspectorTabs.Add(typeof(ITab_Pawn_Face));
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Face)));
            }
#endif

            return true;
        }

    }
}
