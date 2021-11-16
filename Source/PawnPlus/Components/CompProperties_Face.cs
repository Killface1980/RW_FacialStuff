namespace PawnPlus
{
    using System.Collections.Generic;

    using PawnPlus.Parts;

    using Verse;

    public class CompProperties_Face : CompProperties
    {               
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
            for (int i = 0; i < partBehaviors.Count; ++i)
            {
                int lastIndex = partBehaviors.FindLastIndex(behavior => behavior.UniqueID == partBehaviors[i].UniqueID);
                if (lastIndex > i)
                {
                    partBehaviors.RemoveAt(i);
                    Log.Warning(
                        "Pawn Plus: there are more than one IPartBehavior implementation with the same UniqueID "
                        + partBehaviors[i].UniqueID
                        + " . Only the last duplicate implementation in the list will be used.");
                }
            }
        }
    }
}
