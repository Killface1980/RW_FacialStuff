namespace PawnPlus
{
    using System.Collections.Generic;
    using System.Linq;

    using PawnPlus.Defs;
    using PawnPlus.Graphics;
    using PawnPlus.Harmony;
    using PawnPlus.Parts;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {
        private FactionDef _originalFaction;

        private BodyPartSignals _bodyPartSignals;

        private BodyPartStatus _bodyPartStatus;

        private Rot4 _cachedHeadFacing;

        private PawnState _pawnState;

        private IHeadBehavior _headBehavior;

        private List<IPartBehavior> _partBehaviors;

        private Dictionary<PartCategoryDef, Part> _dispPartData;

        private Dictionary<PartCategoryDef, Part> _realPartData;

        public Dictionary<PartCategoryDef, PartDef> _partDefs;

        // Used for distance culling of face details
        private GameComponent_PawnPlus _fsGameComp;

        public bool Initialized { get; private set; }

        public CompProperties_Face Props { get; private set; }

        public Pawn Pawn { get; private set; }

        public HeadCoverage CurrentHeadCoverage { get; set; }

        // Return true if head was drawn. False if not.
        public bool DrawHead(
            PawnGraphicSet graphicSet,
            RotDrawMode bodyDrawType,
            PawnRenderFlags flags,
            Rot4 bodyFacing,
            // Remove headFacing as PP uses compface
            // Rot4 headFacing,
            Vector3 headPos,
            Quaternion headQuat)
        {
            if (Pawn.IsChild())
            {
                return false;
            }

            bool headDrawn = false;
            bool portrait = flags.FlagSet(PawnRenderFlags.Portrait);
            bool headStump = flags.FlagSet(PawnRenderFlags.HeadStump);

            Rot4 headFacing = Pawn.GetCompFace().GetHeadFacing();
            // Draw head
            headFacing = portrait ? _headBehavior.GetRotationForPortrait() : headFacing;
            Material headMaterial = graphicSet.HeadMatAt(headFacing, bodyDrawType, headStump);
            if (headMaterial != null)
            {
                GenDraw.DrawMeshNowOrLater(
                    MeshPool.humanlikeHeadSet.MeshAt(headFacing),
                    headPos,
                    headQuat,
                    headMaterial,
                    portrait);
                headDrawn = true;
                if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                {
                    if (portrait || _fsGameComp.ShouldRenderFaceDetails)
                    {
                        foreach (KeyValuePair<PartCategoryDef, Part> pair in _dispPartData)
                        {
                            pair.Value.RenderPart(
                                graphicSet,
                                bodyDrawType,
                                portrait,
                                headStump,
                                bodyFacing,
                                headFacing,
                                headPos,
                                headQuat);
                        }
                    }
                }
            }

            return headDrawn;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            _fsGameComp = Current.Game.GetComponent<GameComponent_PawnPlus>();
            Props = (CompProperties_Face)props;
            Pawn = (Pawn)parent;

            if (_headBehavior == null)
            {
                _headBehavior = (IHeadBehavior)Props.headBehavior.Clone();
            }

            BuildPartBehaviors();

            _pawnState = new PawnState(Pawn);
            _bodyPartSignals = new BodyPartSignals(Pawn.RaceProps.body);
            _bodyPartStatus = new BodyPartStatus(Pawn);
        }

        // Graphics and faction data aren't available in ThingComp.Initialize(). Initialize the members related to those in this method,
        // which is called by a postfix to ResolveAllGraphics() method.
        public void InitializeFace()
        {
            if (_originalFaction == null)
            {
                _originalFaction = Pawn.Faction.def ?? Faction.OfPlayer.def;
            }

            _pawnState.UpdateState();

            if (_partDefs == null)
            {
                if (Props.partGenHelper == null)
                {
                    Log.Error(
                        "Pawn Plus: partGenHelper in CompProperties_Face can't be null. No parts will be generated.");
                    Initialized = false;
                    return;
                }

                Props.partGenHelper.PartsPreGeneration(Pawn);
                _partDefs = Props.partGenHelper.GeneratePartInCategory(
                    Pawn,
                    _originalFaction,
                    PartDef.GetCategoriesInRace(Pawn.RaceProps.body));
                Props.partGenHelper.PartsPostGeneration(Pawn);
            }

            _headBehavior.Initialize(Pawn);
            foreach (IPartBehavior partBehavior in _partBehaviors)
            {
                partBehavior.Initialize(Pawn.RaceProps.body, _bodyPartSignals);
            }

            _realPartData = new Dictionary<PartCategoryDef, Part>();
            foreach (KeyValuePair<PartCategoryDef, PartDef> pair in _partDefs)
            {
                PartDef partDef = pair.Value;
                Part partData = Part.Create(Pawn, partDef, _bodyPartSignals);
                if (partData != null)
                {
                    _realPartData.Add(pair.Key, partData);
                }
            }

            HashSet<string> occludedRenderNodes = new HashSet<string>();
            foreach (KeyValuePair<PartCategoryDef, PartDef> pair in _partDefs)
            {
                PartDef partDef = pair.Value;
                foreach (PartDef.SinglePart part in partDef.parts)
                {
                    if (part.occludedRenderNodes.NullOrEmpty())
                    {
                        continue;
                    }

                    foreach (string occludedRenderNode in part.occludedRenderNodes)
                    {
                        occludedRenderNodes.Add(occludedRenderNode);
                    }
                }
            }

            foreach (KeyValuePair<PartCategoryDef, Part> pair in _realPartData)
            {
                pair.Value.UpdatePartOcclusion(occludedRenderNodes);
            }

            _dispPartData = _realPartData;
            Initialized = true;
        }

        public void OnResolveGraphics(PawnGraphicSet graphics)
        {
            if (!Initialized)
            {
                InitializeFace();
            }

            graphics.hairGraphic = GraphicDatabase.Get<Graphic_Hair>(
                Pawn.story.hairDef.texPath,
                Shaders.Hair,
                Vector2.one,
                Pawn.story.hairColor);
        }

        public void OnResolveApparelGraphics(PawnGraphicSet graphics)
        {
            // Update head coverage status
            if (Controller.settings.MergeHair)
            {
                CurrentHeadCoverage = HeadCoverage.None;
                List<Apparel> wornApparel = Pawn.apparel.WornApparel.Where(
                    x => x.def.apparel.LastLayer == ApparelLayerDefOf.Overhead).ToList();
                if (!wornApparel.NullOrEmpty())
                {
                    // Full head coverage has precedence over upper head coverage
                    if (wornApparel.Any(x => x.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)))
                    {
                        CurrentHeadCoverage = HeadCoverage.FullHead;
                    }
                    else if (wornApparel.Any(x => x.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)))
                    {
                        CurrentHeadCoverage = HeadCoverage.UpperHead;
                    }
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!this.Initialized) return;

            bool canUpdatePawn = this.Pawn.Map != null && !this.Pawn.InContainerEnclosed && this.Pawn.Spawned;
            if (!canUpdatePawn) return;

            this._pawnState.UpdateState();
            this._headBehavior.Update(this.Pawn, this._pawnState, out this._cachedHeadFacing);

            foreach (IPartBehavior partBehavior in this._partBehaviors)
            {
                partBehavior.Update(this.Pawn, this._pawnState);
            }

            bool updatePortrait = false;
            foreach (KeyValuePair<PartCategoryDef, Part> pair in this._dispPartData)
            {
                bool updatePortraitTemp = false;
                pair.Value.Update(this._pawnState, this._bodyPartStatus, ref updatePortraitTemp);
                updatePortrait |= updatePortraitTemp;
            }

            if (updatePortrait)
            {
                PortraitsCache.SetDirty(this.Pawn);
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!this.Initialized) return;
            bool canUpdatePawn = this.Pawn.Map != null && !this.Pawn.InContainerEnclosed && this.Pawn.Spawned;
            if (!canUpdatePawn) return;
            bool updatePortrait = false;
            foreach (KeyValuePair<PartCategoryDef, Part> pair in this._dispPartData)
            {
                bool updatePortraitTemp = false;
                pair.Value.UpdateRare(this._pawnState, this._bodyPartStatus, ref updatePortraitTemp);
                updatePortrait |= updatePortraitTemp;
            }

            if (updatePortrait)
            {
                PortraitsCache.SetDirty(this.Pawn);
            }
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            if (!this.Initialized) return;
            bool canUpdatePawn = this.Pawn.Map != null && !this.Pawn.InContainerEnclosed && this.Pawn.Spawned;
            if (!canUpdatePawn) return;
            bool updatePortrait = false;
            foreach (KeyValuePair<PartCategoryDef, Part> pair in this._dispPartData)
            {
                bool updatePortraitTemp = false;
                pair.Value.UpdateLong(this._pawnState, this._bodyPartStatus, ref updatePortraitTemp);
                updatePortrait |= updatePortraitTemp;
            }

            if (updatePortrait)
            {
                PortraitsCache.SetDirty(this.Pawn);
            }
        }

        public Dictionary<PartCategoryDef, Part> ClonePartData()
        {
            Dictionary<PartCategoryDef, Part> clonedParts = new Dictionary<PartCategoryDef, Part>();
            foreach (KeyValuePair<PartCategoryDef, Part> pair in _realPartData)
            {
                clonedParts.Add(pair.Key, pair.Value.ClonePart());
            }

            return clonedParts;
        }

        public void SetPartDisplay(Dictionary<PartCategoryDef, Part> parts)
        {
            _dispPartData = parts;
        }

        public void RestorePartDisplay()
        {
            _dispPartData = _realPartData;
        }

        private void BuildPartBehaviors()
        {
            if (_partBehaviors == null)
            {
                _partBehaviors = new List<IPartBehavior>();
            }

            // Check if there is an existing part behavior class from the previous save. If so, use that instance instead of
            // creating a new one.
            // Also, drop existing part behavior instance if it is not defined in <partBehaviors> list anymore.
            List<IPartBehavior> newPartBehaviors = new List<IPartBehavior>();

            // Note that items in Props.partBehaviors are guaranteed to have unique string IDs (IPartBehavior.UniqueID)
            // Look CompProperties_Face.ResolveReferences()
            for (int i = 0; i < Props.partBehaviors.Count; ++i)
            {
                IPartBehavior partBehavior = null;
                int partBehaviorIdx =
                    _partBehaviors.FindIndex(behavior => behavior.UniqueID == Props.partBehaviors[i].UniqueID);
                if (partBehaviorIdx >= 0)
                {
                    partBehavior = _partBehaviors[partBehaviorIdx];
                    _partBehaviors.RemoveAt(partBehaviorIdx);
                }
                else
                {
                    partBehavior = (IPartBehavior)Props.partBehaviors[i].Clone();
                }

                newPartBehaviors.Add(partBehavior);
            }

            foreach (IPartBehavior oldPartBehavior in _partBehaviors)
            {
                // Scribe_Collections inserts null item if the IPartBehavior implementation class does not exist anymore in the assembly
                if (oldPartBehavior == null)
                {
                    continue;
                }

                Log.Warning(
                    "Pawn Plus: The previously-saved part behavior class (" + oldPartBehavior.GetType() + " UniqueID: "
                    + oldPartBehavior.UniqueID + ") for pawn (" + Pawn
                    + ") will be dropped because it is no longer defined in <partBehaviors> list in CompProperties_Face.");
            }

            _partBehaviors = newPartBehaviors;
        }

        public void NotifyBodyPartHediffGained(BodyPartRecord bodyPart, Hediff hediff)
        {
            _bodyPartStatus.NotifyBodyPartHediffGained(bodyPart, hediff);
        }

        public void NotifyBodyPartHediffLost(BodyPartRecord bodyPart, Hediff hediff)
        {
            _bodyPartStatus.NotifyBodyPartHediffLost(bodyPart, hediff);
        }

        public void NotifyBodyPartRestored(BodyPartRecord bodyPart)
        {
            _bodyPartStatus.NotifyBodyPartRestored(bodyPart);
        }

        public void SetHeadTarget(Thing target, IHeadBehavior.TargetType targetType)
        {
            _headBehavior.SetTarget(target, targetType);
        }

        public Rot4 GetHeadFacing()
        {
            return _cachedHeadFacing;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _originalFaction, "originalFaction");
            Scribe_Deep.Look(ref _headBehavior, "headBehavior");
            Scribe_Collections.Look(ref _partBehaviors, "partBehaviors", LookMode.Deep);
            Scribe_Collections.Look(ref _partDefs, "partDefs", LookMode.Def);
        }
    }
}
