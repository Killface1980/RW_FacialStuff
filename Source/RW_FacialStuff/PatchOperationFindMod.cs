using System.Linq;
using System.Xml;

using Verse;

namespace FacialStuff
{
    // ReSharper disable once UnusedMember.Global
    public class PatchOperationFindMod : PatchOperation
    {
        private string modName;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (this.modName.NullOrEmpty())
            {
                return false;
            }

            return ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == this.modName);
        }
    }
}