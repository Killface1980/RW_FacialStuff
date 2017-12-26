namespace FacialStuff
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using FacialStuff.Animator;
    using FacialStuff.Defs;
    using FacialStuff.Graphics;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class CompBodyAnimator : ThingComp
    {
        #region Public Fields


        public bool AnimatorOpen;

        public BodyPartStats bodyStat;

        [CanBeNull]
        public PawnBodyGraphic PawnBodyGraphic;

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

        public BodyAnimDef bodyAnim;


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
        public static FieldInfo infoJitterer;

        public float JitterMax = 0.35f;

        public JitterHandler Jitterer
            => GetHiddenValue(typeof(Pawn_DrawTracker), Pawn.Drawer, "jitterer", infoJitterer) as
                   JitterHandler;
        public void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
            if (this.PawnDrawers != null)
            {
                int i = 0;
                int count = this.PawnDrawers.Count;
                while (i < count)
                {
                    this.PawnDrawers[i].DrawEquipment(rootLoc, portrait);
                    i++;
                }
            }
        }
        public static object GetHiddenValue(Type type, object instance, string fieldName, [CanBeNull] FieldInfo info)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, GenGeneric.BindingFlagsAll);
            }

            return info?.GetValue(instance);
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

            BodyType bodyType = BodyType.Undefined;

            if (this.Pawn.story?.bodyType != null)
            {
                bodyType = this.Pawn.story.bodyType;
            }

            string defName = "BodyAnimDef_" + this.Pawn.def.defName + "_" + bodyType;


            BodyAnimDef newDef = DefDatabase<BodyAnimDef>.GetNamedSilentFail(defName);


            if (newDef != null)
            {

                this.bodyAnim = newDef;
            }
            else
            {
                this.bodyAnim = new BodyAnimDef { defName = defName, label = defName };
            }

            this.InitializePawnDrawer();


        }

        #endregion Public Methods
    }
}
