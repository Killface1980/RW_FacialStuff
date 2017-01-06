// Toggle in Hospitality Properties

using System;
using System.Collections.Generic;
using System.Reflection;
using CommunityCoreLibrary;
using RW_FacialStuff.NoCCL;
using Verse;
#if NoCCL

#else
using CommunityCoreLibrary;
#endif

namespace RW_FacialStuff.Initializer
{

    public class FS_SpecialInjector : SpecialInjector
    {

        private static Assembly Assembly { get { return Assembly.GetAssembly(typeof(FS_SpecialInjector)); } }

        private static readonly BindingFlags[] bindingFlagCombos = {
            BindingFlags.Instance | BindingFlags.Public, BindingFlags.Static | BindingFlags.Public,
            BindingFlags.Instance | BindingFlags.NonPublic, BindingFlags.Static | BindingFlags.NonPublic,
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
                            if (!Detours.TryDetourFromTo(sourceMethod, targetMethod)) return false;
                            }
                        }
                    }
                }
            }
            #endregion

            CompInjectionSet injectionSet = new CompInjectionSet
            {
                targetDefs = new List<string>(),
                compProps = new CompProperties()
            };

            injectionSet.targetDefs.Add("Human");
            injectionSet.targetDefs.Add("Jaffa");

            injectionSet.compProps.compClass = typeof(CompFace);
            List<ThingDef> thingDefs = DefInjectionQualifier.FilteredThingDefs(injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs);


            if (!thingDefs.NullOrEmpty())
            {
                foreach (ThingDef thingDef in thingDefs)
                {
                    // TODO:  Make a full copy using the comp in this def as a template
                    // Currently adds the comp in this def so all target use the same def
                    if (!thingDef.HasComp(injectionSet.compProps.compClass))
                    {
                        thingDef.comps.Add(injectionSet.compProps);
                    }
                }
            }

            return true;
        }

    }
}
