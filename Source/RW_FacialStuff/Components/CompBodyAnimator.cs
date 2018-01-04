using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FacialStuff.Animator;
using FacialStuff.DefOfs;
using FacialStuff.GraphicsFS;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class CompBodyAnimator : ThingComp
    {
        #region Public Fields

        private static FieldInfo _infoJitterer;
        public float JitterMax = 0.35f;
        public bool AnimatorOpen;
        [NotNull] public BodyAnimDef BodyAnim;
        public BodyPartStats BodyStat;
        public PawnBodyGraphic PawnBodyGraphic;
        [CanBeNull] public WalkCycleDef WalkCycle = WalkCycleDefOf.Biped_Walk;

        #endregion Public Fields

        #region Private Fields

        [NotNull] private readonly List<Material> _cachedNakedMatsBodyBase = new List<Material>();

        private int _cachedNakedMatsBodyBaseHash = -1;
        private readonly List<Material> _cachedSkinMatsBodyBase = new List<Material>();
        private int _cachedSkinMatsBodyBaseHash = -1;
        private bool _initialized;
        private int _lastRoomCheck;
        private Room _theRoom;

        #endregion Private Fields

        #region Public Properties

        public BodyAnimator BodyAnimator { get; private set; }

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
                if (this.TheRoom != null)
                {
                    RoomGroup theRoomGroup = this.TheRoom.Group;
                    if (theRoomGroup != null && !theRoomGroup.UsesOutdoorTemperature)
                    {
                        // Pawn is indoors
                        return !this.Pawn.Drafted || !Controller.settings.IgnoreWhileDrafted;
                    }
                }

                return false;

                // return !room?.Group.UsesOutdoorTemperature == true && Controller.settings.IgnoreWhileDrafted || !this.pawn.Drafted;
            }
        }

        public JitterHandler Jitterer
            => GetHiddenValue(typeof(Pawn_DrawTracker), this.Pawn.Drawer, "jitterer", _infoJitterer) as
               JitterHandler;

        [NotNull]
        public Pawn Pawn => this.parent as Pawn;

        public List<PawnBodyDrawer> PawnBodyDrawers { get; private set; }

        public void DrawApparel(Quaternion quat, Vector3 vector, bool portrait, bool renderBody)
        {
            if (this.PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnBodyDrawers.Count;
            while (i < count)
            {
                this.PawnBodyDrawers[i].DrawApparel(quat, vector, renderBody, portrait);
                i++;
            }
        }

        public CompProperties_BodyAnimator Props
        {
            get { return (CompProperties_BodyAnimator)this.props; }
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

                if (Find.TickManager.TicksGame < this._lastRoomCheck + 60f)
                {
                    return this._theRoom;
                }

                this._theRoom = this.Pawn.GetRoom();
                this._lastRoomCheck = Find.TickManager.TicksGame;

                return this._theRoom;
            }
        }

        #endregion Private Properties

        #region Public Methods

        public static object GetHiddenValue(Type type, object __instance, string fieldName, [CanBeNull] FieldInfo info)
        {
            if (info == null)
            {
                info = type.GetField(fieldName, GenGeneric.BindingFlagsAll);
            }

            return info?.GetValue(__instance);
        }

        public void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if (this.PawnBodyDrawers != null)
            {
                int i = 0;
                int count = this.PawnBodyDrawers.Count;
                while (i < count)
                {
                    this.PawnBodyDrawers[i].ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
                    i++;
                }
            }
        }

        // Verse.PawnGraphicSet
        public void ClearCache()
        {
            this._cachedSkinMatsBodyBaseHash = -1;
            this._cachedNakedMatsBodyBaseHash = -1;
        }

        // public override string CompInspectStringExtra()
        // {
        //     string extra = this.Pawn.DrawPos.ToString();
        //     return extra;
        // }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void DrawBody(Vector3 rootLoc, Quaternion quat, RotDrawMode bodyDrawType,
                             [CanBeNull] PawnWoundDrawer woundDrawer, bool renderBody, bool portrait)
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

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
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

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait)
        {
            if (!this.PawnBodyDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnBodyDrawers.Count;
                while (i < count)
                {
                    this.PawnBodyDrawers[i].DrawFeet(bodyQuat, footQuat, rootLoc, portrait);
                    i++;
                }
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void DrawHands(Quaternion bodyQuat, Vector3 rootLoc, bool portrait, bool carrying)
        {
            if (!this.PawnBodyDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnBodyDrawers.Count;
                while (i < count)
                {
                    this.PawnBodyDrawers[i].DrawHands(bodyQuat, rootLoc, portrait, carrying);
                    i++;
                }
            }
        }

        public void InitializePawnDrawer()
        {
            if (this.Props.drawers.Any())
            {
                this.PawnBodyDrawers = new List<PawnBodyDrawer>();
                for (int i = 0; i < this.Props.drawers.Count; i++)
                {
                    PawnBodyDrawer thingComp =
                    (PawnBodyDrawer)Activator.CreateInstance(this.Props.drawers[i].GetType());
                    thingComp.CompAnimator = this;
                    thingComp.Pawn = this.Pawn;
                    this.PawnBodyDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
            else
            {
                this.PawnBodyDrawers = new List<PawnBodyDrawer>();

                PawnBodyDrawer thingComp = (PawnBodyDrawer)Activator.CreateInstance(typeof(HumanBipedDrawer));
                thingComp.CompAnimator = this;
                thingComp.Pawn = this.Pawn;
                this.PawnBodyDrawers.Add(thingComp);
                thingComp.Initialize();
            }
        }

        public List<Material> NakedMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if (num != this._cachedNakedMatsBodyBaseHash)
            {
                this._cachedNakedMatsBodyBase.Clear();
                this._cachedNakedMatsBodyBaseHash = num;
                PawnGraphicSet graphics = this.Pawn.Drawer.renderer.graphics;
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    this._cachedNakedMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    this._cachedNakedMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    this._cachedNakedMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing));
                }

                for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayer lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    if (this.Pawn.Dead)
                    {
                        if (lastLayer != ApparelLayer.Shell && lastLayer != ApparelLayer.Overhead)
                        {
                            this._cachedNakedMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                        }
                    }
                }
            }

            return this._cachedNakedMatsBodyBase;
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
            Scribe_Values.Look(ref this._lastRoomCheck, "lastRoomCheck");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.BodyAnimator = new BodyAnimator(this.Pawn, this);

            this.PawnBodyGraphic = new PawnBodyGraphic(this);

            BodyType bodyType = BodyType.Undefined;

            if (this.Pawn.story?.bodyType != null)
            {
                bodyType = this.Pawn.story.bodyType;
            }

            string defaultName = "BodyAnimDef_" + this.Pawn.def.defName + "_" + bodyType;
            List<string> names = new List<string>
                                       {
                                       defaultName,
                                       "BodyAnimDef_" + ThingDefOf.Human.defName + "_" + bodyType
                                       };
            foreach (string name in names)
            {
                BodyAnimDef newDef = DefDatabase<BodyAnimDef>.GetNamedSilentFail(name);
                if (newDef != null)
                {
                    this.BodyAnim = newDef;
                    return;
                }
            }

            this.BodyAnim = new BodyAnimDef { defName = defaultName, label = defaultName };
            DefDatabase<BodyAnimDef>.Add(this.BodyAnim);
        }

        public void TickDrawers(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            if (!this._initialized)
            {
                this.InitializePawnDrawer();
                this._initialized = true;
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
            if (num != this._cachedSkinMatsBodyBaseHash)
            {
                this._cachedSkinMatsBodyBase.Clear();
                this._cachedSkinMatsBodyBaseHash = num;
                PawnGraphicSet graphics = this.Pawn.Drawer.renderer.graphics;
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    this._cachedSkinMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    this._cachedSkinMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    this._cachedSkinMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing));
                }

                for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayer lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    // if (lastLayer != ApparelLayer.Shell && lastLayer != ApparelLayer.Overhead)
                    if (lastLayer == ApparelLayer.OnSkin)
                    {
                        this._cachedSkinMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                    }
                }
            }

            return this._cachedSkinMatsBodyBase;
        }

        #endregion Public Methods
    }
}