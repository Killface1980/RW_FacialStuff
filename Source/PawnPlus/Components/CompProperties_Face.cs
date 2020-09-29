// ReSharper disable UnassignedField.Global
// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable InconsistentNaming

using PawnPlus.Defs;
using PawnPlus.Parts;
using System.Collections.Generic;

using Verse;

namespace PawnPlus
{
    public class CompProperties_Face : CompProperties
    {
        public bool canRotateHead;
        
        public bool hasBeard;
        
        public bool hasMouth;
        
        public bool hasWrinkles;
        
        public bool needsBlankHumanHead;
                
        public IHeadBehavior headBehavior;
                
        public List<IPartBehavior> partBehaviors;

        public PartGenHelper partGenHelper;

        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }
}
