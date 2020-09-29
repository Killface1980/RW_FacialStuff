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

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
            // Check for dupicate UniqueIDs
            for(int i = 0; i < partBehaviors.Count; ++i)
			{
                int lastIndex = partBehaviors.FindLastIndex(behavior => behavior.UniqueID == partBehaviors[i].UniqueID);
                if(lastIndex > i)
				{
                    partBehaviors.RemoveAt(i);
                    Log.Warning(
                        "Pawn Plus: there are more than one IPartBehavior implementation with the same UniqueID " + 
                        partBehaviors[i].UniqueID + 
                        " . Only the last duplicate implementation in the list will be used.");
                    continue;
				}
            }
		}
	}
}
