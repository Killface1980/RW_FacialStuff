// ReSharper disable UnassignedField.Global
// ReSharper disable StyleCop.SA1307
// ReSharper disable StyleCop.SA1401
// ReSharper disable InconsistentNaming

using FacialStuff.Defs;
using System.Collections.Generic;

using Verse;

namespace FacialStuff
{
    public class CompProperties_Face : CompProperties
    {
        public bool canRotateHead;
        
        public bool hasBeard;
        
        public bool hasMouth;
        
        public bool hasWrinkles;
        
        public bool needsBlankHumanHead;
                
        public IHeadBehavior headBehavior;

        public IMouthBehavior mouthBehavior;
        
        public IEyeBehavior eyeBehavior;

        public CompProperties_Face()
        {
            this.compClass = typeof(CompFace);
        }
    }
}
