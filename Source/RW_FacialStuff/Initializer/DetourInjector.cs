using System;
using System.Reflection;
using CommunityCoreLibrary;
using RimWorld;
using Verse;

namespace RW_FacialStuff
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class DetourAttribute : Attribute
    {
        public Type source;

        public BindingFlags bindingFlags;

        public DetourAttribute(Type source)
        {
            this.source = source;
        }
    }

    class DetourInjector : SpecialInjector
    {
        private static readonly BindingFlags[] bindingFlagCombos = new BindingFlags[]
  {
            BindingFlags.Instance | BindingFlags.Public,
            BindingFlags.Static | BindingFlags.Public,
            BindingFlags.Instance | BindingFlags.NonPublic,
            BindingFlags.Static | BindingFlags.NonPublic
  };

        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(DetourInjector));
            }
        }

        public override bool Inject()
        {
            Type[] types = DetourInjector.Assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                BindingFlags[] array = DetourInjector.bindingFlagCombos;
                for (int j = 0; j < array.Length; j++)
                {
                    BindingFlags bindingFlags = array[j];
                    MethodInfo[] methods = type.GetMethods(bindingFlags);
                    for (int k = 0; k < methods.Length; k++)
                    {
                        MethodInfo methodInfo = methods[k];
                        object[] customAttributes = methodInfo.GetCustomAttributes(typeof(DetourAttribute), true);
                        int l = 0;
                        while (l < customAttributes.Length)
                        {
                            DetourAttribute detourAttribute = (DetourAttribute)customAttributes[l];
                            BindingFlags bindingFlags2 = (detourAttribute.bindingFlags != BindingFlags.Default) ? detourAttribute.bindingFlags : bindingFlags;
                            MethodInfo method = detourAttribute.source.GetMethod(methodInfo.Name, bindingFlags2);
                            bool result;
                            if (method == null)
                            {
                                Log.Error(string.Format("FacialStuff :: Detours :: Can't find source method '{0} with bindingflags {1}", methodInfo.Name, bindingFlags2));
                                result = false;
                            }
                            else
                            {
                                if (Detours.TryDetourFromTo(method, methodInfo))
                                {
                                    l++;
                                    continue;
                                }
                                result = false;
                            }
                            return result;
                        }
                    }
                }
            }
            return true;
        }

    }
}
