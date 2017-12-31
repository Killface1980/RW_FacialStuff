namespace FacialStuff
{
    using FacialStuff.Animator;
    using FacialStuff.Defs;
    using FacialStuff.Graphics;
    using JetBrains.Annotations;
    using RimWorld;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using Verse;

    public class CompBodyAnimator : ThingComp
    {
        #region Public Fields

        public static FieldInfo infoJitterer;
        public bool AnimatorOpen;

        [CanBeNull]
        public BodyAnimDef bodyAnim;
        public BodyPartStats bodyStat;

        public float JitterMax = 0.35f;

        public int lastRoomCheck;

        [CanBeNull]
        public PawnBodyGraphic PawnBodyGraphic;

        public WalkCycleDef walkCycle = WalkCycleDefOf.Biped_Walk;

        #endregion Public Fields

        #region Private Fields

        private BodyAnimator bodyAnimator;

        private List<Material> cachedNakedMatsBodyBase = new List<Material>();
        private int cachedNakedMatsBodyBaseHash = -1;
        private List<Material> cachedSkinMatsBodyBase = new List<Material>();
        private int cachedSkinMatsBodyBaseHash = -1;
        private bool initialized;
        private List<PawnBodyDrawer> pawnBodyDrawers;
        private Room theRoom;

        #endregion Private Fields

        #region Public Properties

        [CanBeNull]
        public BodyAnimator BodyAnimator => this.bodyAnimator;

        public bool HideShellLayer => this.InRoom && Controller.settings.HideShellWhileRoofed;

        public bool InPrivateRoom
        {
            get
            {
                if (!this.InRoom || this.Pawn.IsPrisoner)
                {
                    return false;
                }
                Room ownedRoom = this.Pawn.ownership?.OwnedRoom;
                if (ownedRoom != null)
                {
                    return ownedRoom == this.TheRoom;
                }

                return false;
            }
        }

        public bool InRoom
        {
            get
            {
                if (this.TheRoom != null && (!this.TheRoom.Group.UsesOutdoorTemperature))
                {
                    // Pawn is indoors
                    return !this.Pawn.Drafted || !Controller.settings.IgnoreWhileDrafted;
                }

                return false;

                // return !room?.Group.UsesOutdoorTemperature == true && Controller.settings.IgnoreWhileDrafted || !this.pawn.Drafted;
            }
        }

        public JitterHandler Jitterer
            => GetHiddenValue(typeof(Pawn_DrawTracker), Pawn.Drawer, "jitterer", infoJitterer) as
                   JitterHandler;

        [NotNull]
        public Pawn Pawn => this.parent as Pawn;

        public List<PawnBodyDrawer> PawnBodyDrawers => this.pawnBodyDrawers;

        public CompProperties_BodyAnimator Props
        {
            get
            {
                return (CompProperties_BodyAnimator)this.props;
            }
        }

        public bool HideHat => this.InRoom && Controller.settings.HideHatWhileRoofed;

        #endregion Public Properties

        #region Private Properties

        [CanBeNull]
        private Room TheRoom
        {
            get
            {
                if (this.Pawn.Dead)
                {
                    return null;
                }

                if ((Find.TickManager.TicksGame < this.lastRoomCheck + 60f))
                {
                    return this.theRoom;
                }

                this.theRoom = this.Pawn.GetRoom();
                this.lastRoomCheck = Find.TickManager.TicksGame;

                return this.theRoom;
            }
        }

        #endregion Private Properties

        #region Public Methods

        public static object GetHiddenValue(Type type, object instance, string fieldName, [CanBeNull] FieldInfo info)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, GenGeneric.BindingFlagsAll);
            }

            return info?.GetValue(instance);
        }

        public void ApplyBodyWobble(ref Vector3 rootLoc, ref Quaternion quat)
        {
            if (this.pawnBodyDrawers != null)
            {
                int i = 0;
                int count = this.pawnBodyDrawers.Count;
                while (i < count)
                {
                    this.pawnBodyDrawers[i].ApplyBodyWobble(ref rootLoc, ref quat);
                    i++;
                }
            }
        }

        // Verse.PawnGraphicSet
        public void ClearCache()
        {
            this.cachedSkinMatsBodyBaseHash = -1;
            this.cachedNakedMatsBodyBaseHash = -1;
        }

        public void DrawBody(Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType, [CanBeNull] PawnWoundDrawer woundDrawer, bool renderBody, bool portrait)
        {
            if (this.PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }
            int i = 0;
            while (i < this.PawnBodyDrawers.Count)
            {
                this.PawnBodyDrawers[i].DrawBody(
                    woundDrawer,
                    rootLoc,
                    quat,
                    bodyDrawType,
                    renderBody,
                    portrait);
                i++;
            }
        }

        public void DrawEquipment(Vector3 rootLoc, bool portrait)
        {
            if (!this.PawnBodyDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnBodyDrawers.Count;
                while (i < count)
                {
                    this.PawnBodyDrawers[i].DrawEquipment(rootLoc, portrait);
                    i++;
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            string extra = this.Pawn.DrawPos.ToString();
            return extra;
        }

        public void DrawFeet(Vector3 rootLoc, bool portrait)
        {
            if (this.pawnBodyDrawers != null)
            {
                int i = 0;
                int count = this.pawnBodyDrawers.Count;
                while (i < count)
                {
                    this.pawnBodyDrawers[i].DrawFeet(rootLoc, portrait);
                    i++;
                }
            }
        }

        public void DrawHands(Vector3 rootLoc, bool portrait, bool carrying)
        {
            if (this.pawnBodyDrawers != null)
            {
                int i = 0;
                int count = this.pawnBodyDrawers.Count;
                while (i < count)
                {
                    this.pawnBodyDrawers[i].DrawHands(rootLoc, portrait, carrying);
                    i++;
                }
            }
        }

        public void InitializePawnDrawer()
        {
            if (this.Props.drawers.Any())
            {
                this.pawnBodyDrawers = new List<PawnBodyDrawer>();
                for (int i = 0; i < this.Props.drawers.Count; i++)
                {
                    PawnBodyDrawer thingComp = (PawnBodyDrawer)Activator.CreateInstance(this.Props.drawers[i].GetType());
                    thingComp.CompAnimator = this;
                    thingComp.Pawn = this.Pawn;
                    this.PawnBodyDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
        }

        public List<Material> NakedMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if (num != this.cachedNakedMatsBodyBaseHash)
            {
                this.cachedNakedMatsBodyBase.Clear();
                this.cachedNakedMatsBodyBaseHash = num;
                PawnGraphicSet graphics = this.Pawn.Drawer.renderer.graphics;
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    this.cachedNakedMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    this.cachedNakedMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    this.cachedNakedMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing, null));
                }

                for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayer lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    if (this.Pawn.Dead)
                    {
                        if (lastLayer != ApparelLayer.Shell && lastLayer != ApparelLayer.Overhead)
                        {
                            this.cachedNakedMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing, null));
                        }
                    }
                }
            }

            return this.cachedNakedMatsBodyBase;
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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.lastRoomCheck, "lastRoomCheck");
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
        }

        public void TickDrawers(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            if (!initialized)
            {
                this.InitializePawnDrawer();
                initialized = true;
            }

            if (!this.PawnBodyDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnBodyDrawers.Count;
                while (i < count)
                {
                    this.PawnBodyDrawers[i].Tick(bodyFacing, graphics);
                    i++;
                }
            }
        }

        public List<Material> UnderwearMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if (num != this.cachedSkinMatsBodyBaseHash)
            {
                this.cachedSkinMatsBodyBase.Clear();
                this.cachedSkinMatsBodyBaseHash = num;
                PawnGraphicSet graphics = this.Pawn.Drawer.renderer.graphics;
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    this.cachedSkinMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    this.cachedSkinMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing, null));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    this.cachedSkinMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing, null));
                }

                for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayer lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    // if (lastLayer != ApparelLayer.Shell && lastLayer != ApparelLayer.Overhead)
                    if (lastLayer == ApparelLayer.OnSkin)
                    {
                        this.cachedSkinMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing, null));
                    }
                }
            }

            return this.cachedSkinMatsBodyBase;
        }

        #endregion Public Methods
    }
}