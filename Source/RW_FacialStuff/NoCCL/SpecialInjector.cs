using System.Linq;
using System.Reflection;
using Verse;

namespace RW_FacialStuff.NoCCL
{
    public class SpecialInjector
    {

#if NoCCL
        public virtual bool Inject()
        {
            throw new System.NotImplementedException();
        }
    }

    internal static class DetourInjector
    {
        private static Assembly Assembly { get { return Assembly.GetAssembly(typeof(DetourInjector)); } }
        private static string AssemblyName { get { return Assembly.FullName.Split(',').First(); } }
        static DetourInjector()
        {
            LongEventHandler.QueueLongEvent(Inject, "LibraryStartup", true, null);
        }

        private static void Inject()
        {
            var injector = new Hospitality_SpecialInjector();
            if (injector.Inject()) Log.Message(AssemblyName + " injected.");
            else Log.Error(AssemblyName + " failed to get injected properly.");
        }
#endif
    }
}