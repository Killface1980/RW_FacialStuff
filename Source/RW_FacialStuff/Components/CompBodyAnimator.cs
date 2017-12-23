namespace FacialStuff
{
    using System;
    using System.Collections.Generic;

    using FacialStuff.Animator;
    using FacialStuff.DefOfs;
    using FacialStuff.Defs;
    using FacialStuff.Graphics;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class CompBodyAnimator : ThingComp
    {
        #region Public Fields

        public float AnimationPercent;

        public bool AnimatorOpen;

        public BodyPartStats bodyStat;

        [CanBeNull]
        public PawnBodyGraphic PawnBodyGraphic;

        public Rot4 rotation = Rot4.East;

        public WalkCycleDef walkCycle = WalkCycleDefOf.Human_Walk;

        #endregion Public Fields

        #region Private Fields

        private BodyAnimator bodyAnimator;

        #endregion Private Fields

        #region Public Properties

        public BodyAnimator BodyAnimator => bodyAnimator;

        [NotNull]
        public Pawn Pawn => this.parent as Pawn;

        public CompProperties_BodyAnimator Props
        {
            get
            {
                return (CompProperties_BodyAnimator)this.props;
            }
        }

        #endregion Public Properties
        private List<PawnBodyDrawer> pawnDrawers;

        public BodyAnimDef bodySizeDefinition;

        public List<PawnBodyDrawer> PawnDrawers => pawnDrawers;

        public void TickDrawers(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            if (!this.PawnDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].Tick(bodyFacing, graphics);
                    i++;
                }
            }
        }

        public void InitializePawnDrawer()
        {
            if (this.Props.drawers.Any())
            {
                this.pawnDrawers = new List<PawnBodyDrawer>();
                for (int i = 0; i < this.Props.drawers.Count; i++)
                {
                    PawnBodyDrawer thingComp = (PawnBodyDrawer)Activator.CreateInstance(this.Props.drawers[i].GetType());
                    thingComp.CompAnimator = this;
                    thingComp.Pawn = this.Pawn;
                    this.PawnDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
        }

        #region Public Methods

        public void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].DrawFeet(rootLoc, portrait);
                    i++;
                }
            }
        }
        public void ApplyBodyWobble(ref Vector3 rootLoc, ref Quaternion quat)
        {
            if (this.pawnDrawers != null)
            {
                int i = 0;
                int count = this.pawnDrawers.Count;
                while (i < count)
                {
                    this.pawnDrawers[i].ApplyBodyWobble(ref rootLoc, ref quat);
                    i++;
                }
            }
        }


        public override void PostDraw()
        {
            base.PostDraw();

            // Children & Pregnancy || Werewolves transformed
            if (this.Pawn.Map == null || !this.Pawn.Spawned || this.Pawn.Dead)
            {
                return;
            }

            if (Find.TickManager.Paused)
            {
                return;
            }
            if (this.Props.bipedWithHands)
            {
                this.BodyAnimator.AnimatorTick();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.bodyAnimator = new BodyAnimator(this.Pawn, this);
            this.PawnBodyGraphic = new PawnBodyGraphic(this);


            if (this.Props.bodyAnimType != null)
            {
                this.bodySizeDefinition = this.Props.bodyAnimType;
            }
            else
            {
                if (this.Pawn.RaceProps.Humanlike)
                {
                    switch (this.Pawn.story.bodyType)
                    {
                        case BodyType.Undefined:
                        case BodyType.Male:
                            this.bodySizeDefinition = BodyAnimDefOf.BodyAnimDef_Male;
                            break;
                        case BodyType.Female:
                            this.bodySizeDefinition = BodyAnimDefOf.BodyAnimDef_Female;
                            break;
                        case BodyType.Hulk:
                            this.bodySizeDefinition = BodyAnimDefOf.BodyAnimDef_Hulk;
                            break;
                        case BodyType.Fat:
                            this.bodySizeDefinition = BodyAnimDefOf.BodyAnimDef_Fat;
                            break;
                        case BodyType.Thin:
                            this.bodySizeDefinition = BodyAnimDefOf.BodyAnimDef_Thin;
                            break;
                    }
                }
            }

            this.InitializePawnDrawer();


        }

        #endregion Public Methods
    }
}
