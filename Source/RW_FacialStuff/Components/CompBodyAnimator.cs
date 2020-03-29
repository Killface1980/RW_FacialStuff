using FacialStuff.Animator;
using FacialStuff.AnimatorWindows;
using FacialStuff.GraphicsFS;
using FacialStuff.Harmony;
using FacialStuff.Tweener;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff
{
    public class CompBodyAnimator : ThingComp
    {
        #region Public Fields
        public bool Deactivated;
        public bool IgnoreRenderer;

        [CanBeNull] public BodyAnimDef BodyAnim;

        public BodyPartStats BodyStat;

        public float JitterMax = 0.35f;
        public readonly Vector3Tween[] Vector3Tweens = new Vector3Tween[(int)TweenThing.Max];

        //   [CanBeNull] public PawnPartsTweener PartTweener;

        [CanBeNull] public PawnBodyGraphic PawnBodyGraphic;

        [CanBeNull] public PoseCycleDef PoseCycle;

        public Vector3 FirstHandPosition;
        public Vector3 SecondHandPosition;

        [CanBeNull] public WalkCycleDef WalkCycle => this._walkCycle;

        public Quaternion WeaponQuat = new Quaternion();

        #endregion Public Fields

        #region Private Fields

        private static FieldInfo _infoJitterer;

        [NotNull] private readonly List<Material> _cachedNakedMatsBodyBase = new List<Material>();

        private readonly List<Material> _cachedSkinMatsBodyBase = new List<Material>();

        private int _cachedNakedMatsBodyBaseHash = -1;
        private int _cachedSkinMatsBodyBaseHash = -1;
        private int _lastRoomCheck;

        private bool _initialized;

        [CanBeNull] private Room _theRoom;

        #endregion Private Fields

        #region Public Properties

        public BodyAnimator BodyAnimator { get; private set; }

        public bool HideShellLayer => this.InRoom && Controller.settings.HideShellWhileRoofed && (Pawn.IsColonistPlayerControlled && Pawn.Faction.IsPlayer && !Pawn.HasExtraHomeFaction());

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
                RoomGroup theRoomGroup = this.TheRoom?.Group;
                if (theRoomGroup != null && !theRoomGroup.UsesOutdoorTemperature)
                {
                    // Pawn is indoors
                    return !this.Pawn.Drafted || !Controller.settings.IgnoreWhileDrafted;
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

        public CompProperties_BodyAnimator Props => (CompProperties_BodyAnimator)this.props;

        public bool HideHat => this.InRoom && Controller.settings.HideHatWhileRoofed && (Pawn.IsColonistPlayerControlled && Pawn.Faction.IsPlayer && !Pawn.HasExtraHomeFaction());

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
            if (this.PawnBodyDrawers == null)
            {
                return;
            }

            int i = 0;
            int count = this.PawnBodyDrawers.Count;
            while (i < count)
            {
                this.PawnBodyDrawers[i].ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
                i++;
            }
        }

        // Verse.PawnGraphicSet
        public void ClearCache()
        {
            this._cachedSkinMatsBodyBaseHash = -1;
            this._cachedNakedMatsBodyBaseHash = -1;
        }

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
        public void DrawHands(Quaternion bodyQuat, Vector3 rootLoc, bool portrait, Thing carriedThing = null, bool flip = false, float factor = 1f)
        {
            if (this.PawnBodyDrawers.NullOrEmpty()) return;
            int i = 0;
            int count = this.PawnBodyDrawers.Count;
            while (i < count)
            {
                this.PawnBodyDrawers[i].DrawHands(bodyQuat, rootLoc, portrait, carriedThing, flip, factor);
                i++;
            }
        }

        public void InitializePawnDrawer()
        {
            if (this.Props.bodyDrawers.Any())
            {
                this.PawnBodyDrawers = new List<PawnBodyDrawer>();
                for (int i = 0; i < this.Props.bodyDrawers.Count; i++)
                {
                    PawnBodyDrawer thingComp =
                    (PawnBodyDrawer)Activator.CreateInstance(this.Props.bodyDrawers[i].GetType());
                    thingComp.CompAnimator = this;
                    thingComp.Pawn = this.Pawn;
                    this.PawnBodyDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
            else
            {
                this.PawnBodyDrawers = new List<PawnBodyDrawer>();
                bool isQuaduped = Pawn.GetCompAnim().BodyAnim.quadruped;
                PawnBodyDrawer thingComp = isQuaduped
                    ? (PawnBodyDrawer) Activator.CreateInstance(typeof(QuadrupedDrawer))
                    : (PawnBodyDrawer) Activator.CreateInstance(typeof(HumanBipedDrawer));
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
                    ApparelLayerDef lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    if (this.Pawn.Dead)
                    {
                        if (lastLayer != ApparelLayerDefOf.Shell && lastLayer != ApparelLayerDefOf.Overhead)
                        {
                            this._cachedNakedMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                        }
                    }
                }
            }

            return this._cachedNakedMatsBodyBase;
        }

        public override string CompInspectStringExtra()
        {
            // var tween = Vector3Tweens[(int)TweenThing.Equipment];
            // var log = tween.State + " =>"+  tween.StartValue + " - " + tween.EndValue + " / " + tween.CurrentTime + " / " + tween.CurrentValue;
            // return log;
            //  return MoveState.ToString() + " - " + MovedPercent;

          //  return  lastAimAngle.ToString() ;

            return base.CompInspectStringExtra();
        }

        public override void PostDraw()
        {
            base.PostDraw();

            // Children & Pregnancy || Werewolves transformed
            if (this.Pawn.Map == null || !this.Pawn.Spawned || this.Pawn.Dead || this.Pawn.GetCompAnim().Deactivated)
            {
                return;
            }

            if (Find.TickManager.Paused)
            {
                if (!HarmonyPatchesFS.AnimatorIsOpen() || MainTabWindow_BaseAnimator.Pawn != this.Pawn)
                {
                    return;
                }
            }

            if (this.Props.bipedWithHands)
            {
                this.BodyAnimator.AnimatorTick();
            }

            // Tweener
            Vector3Tween eqTween = this.Vector3Tweens[(int)HarmonyPatchesFS.equipment];

            FloatTween angleTween = this.AimAngleTween;
            Vector3Tween leftHand = this.Vector3Tweens[(int)TweenThing.HandLeft];
            Vector3Tween rightHand = this.Vector3Tweens[(int)TweenThing.HandRight];

            if (leftHand.State == TweenState.Running)
            {
                leftHand.Update(1f * Find.TickManager.TickRateMultiplier);
            }
            if (rightHand.State == TweenState.Running)
            {
                rightHand.Update(1f * Find.TickManager.TickRateMultiplier);
            }
            if (eqTween.State == TweenState.Running)
            {
                eqTween.Update(1f * Find.TickManager.TickRateMultiplier);
            }

            if (angleTween.State == TweenState.Running)
            {
                this.AimAngleTween.Update(3f * Find.TickManager.TickRateMultiplier);
            }

            this.CheckMovement();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this._lastRoomCheck, "lastRoomCheck");
            // Scribe_Values.Look(ref this.PawnBodyGraphic, "PawnBodyGraphic");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            for (int i = 0; i < this.Vector3Tweens.Length; i++)
            {
                this.Vector3Tweens[i] = new Vector3Tween();
            }
            this.BodyAnimator = new BodyAnimator(this.Pawn, this);
            this.Pawn.CheckForAddedOrMissingParts();
            this.PawnBodyGraphic = new PawnBodyGraphic(this);

            string bodyType = "Undefined";

            if (this.Pawn.story?.bodyType != null)
            {
                bodyType = this.Pawn.story.bodyType.ToString();
            }

            string defaultName = "BodyAnimDef_" + this.Pawn.def.defName + "_" + bodyType;
            List<string> names = new List<string>
                                       {
                                       defaultName,
                                       // "BodyAnimDef_" + ThingDefOf.Human.defName + "_" + bodyType
                                       };

            bool needsNewDef = true;
            foreach (string name in names)
            {
                BodyAnimDef dbDef = DefDatabase<BodyAnimDef>.GetNamedSilentFail(name);
                if (dbDef == null)
                {
                    continue;
                }

                this.BodyAnim = dbDef;
                needsNewDef = false;
                break;
            }

            if (needsNewDef)
            {
                this.BodyAnim = new BodyAnimDef { defName = defaultName, label = defaultName };
                DefDatabase<BodyAnimDef>.Add(this.BodyAnim);
            }
        }

        public void TickDrawers(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            if (!this._initialized)
            {
                this.InitializePawnDrawer();
                this._initialized = true;
            }

            if (this.PawnBodyDrawers.NullOrEmpty()) return;
            int i = 0;
            int count = this.PawnBodyDrawers.Count;
            while (i < count)
            {
                this.PawnBodyDrawers[i].Tick(bodyFacing, graphics);
                i++;
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
                    ApparelLayerDef lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    // if (lastLayer != ApparelLayerDefOf.Shell && lastLayer != ApparelLayerDefOf.Overhead)
                    if (lastLayer == ApparelLayerDefOf.OnSkin)
                    {
                        this._cachedSkinMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                    }
                }
                // One more time to get at least one pieces of cloth
                if (_cachedSkinMatsBodyBase.Count < 1)
                {
                    for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                    {
                        ApparelLayerDef lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                        // if (lastLayer != ApparelLayerDefOf.Shell && lastLayer != ApparelLayerDefOf.Overhead)
                        if (lastLayer == ApparelLayerDefOf.Middle)
                        {
                            this._cachedSkinMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                        }
                    }

                }
            }

            return this._cachedSkinMatsBodyBase;
        }

#endregion Public Methods

        public float MovedPercent => this._movedPercent;
        public float BodyAngle;

        public float LastAimAngle = 143f;

        //  public float lastWeaponAngle = 53f;
        public readonly Vector3[] LastPosition = new Vector3[(int)TweenThing.Max];

        public readonly FloatTween AimAngleTween = new FloatTween();
        public bool HasLeftHandPosition => this.SecondHandPosition != Vector3.zero;

        public Vector3 LastEqPos = Vector3.zero;
        public float DrawOffsetY;

        public void CheckMovement()
        {
            if (HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn == this.Pawn)
            {
                this._isMoving = true;
                this._movedPercent = MainTabWindow_BaseAnimator.AnimationPercent;
                return;
            }

            if (this.IsRider)
            {
                this._isMoving = false;
                return;
            }
            // pawn started pathing

            Pawn_PathFollower pather = this.Pawn.pather;
            if ((pather != null) && (pather.Moving) && !this.Pawn.stances.FullBodyBusy && (pather.BuildingBlockingNextPathCell() == null) && (pather.NextCellDoorToWaitForOrManuallyOpen() == null) && !pather.WillCollideWithPawnOnNextPathCell())
            {
                this._movedPercent = 1f - pather.nextCellCostLeft / pather.nextCellCostTotal;
                this._isMoving = true;
            }
            else
            {
                this._isMoving = false;
            }
        }

        public bool IsRider { get; set; }

        public void DrawAlienBodyAddons(Quaternion quat, Vector3 vector, bool portrait, bool renderBody, Rot4 rotation,
            bool invisible)
        {
            if (this.PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnBodyDrawers.Count;
            while (i < count)
            {
                this.PawnBodyDrawers[i].DrawAlienBodyAddons(portrait, vector, quat, renderBody, rotation, invisible);
                i++;
            }
        }

        public void SetWalkCycle(WalkCycleDef walkCycleDef)
        {
            this._walkCycle = walkCycleDef;
        }

        public float HeadffsetZ
        {
            get
            {
                if (Controller.settings.UseFeet)
                {
                    WalkCycleDef walkCycle = this.WalkCycle;
                    if (walkCycle != null)
                    {
                        SimpleCurve curve = walkCycle.HeadOffsetZ;
                        if (curve.PointsCount > 0)
                        return curve.Evaluate(this.MovedPercent);
                    }
                }

                return 0f;
            }
        }

        public float HeadAngleX
        {
            get
            {
                if (Controller.settings.UseFeet)
                {
                    WalkCycleDef walkCycle = this.WalkCycle;
                    if (walkCycle != null)
                    {
                        SimpleCurve curve = walkCycle.HeadAngleX;
                        if (curve.PointsCount > 0)
                            return curve.Evaluate(this.MovedPercent);
                    }
                }

                return 0f;
            }
        }

        public float BodyOffsetZ
        {
            get
            {
                if (Controller.settings.UseFeet)
                {
                    WalkCycleDef walkCycle = this.WalkCycle;
                    if (walkCycle != null)
                    {
                        SimpleCurve curve = walkCycle.BodyOffsetZ;
                        if (curve.PointsCount > 0)
                        return curve.Evaluate(this.MovedPercent);
                    }
                }

                return 0f;
            }
        }

        public bool IsMoving => this._isMoving;
        internal bool MeshFlipped;
        internal float LastWeaponAngle;
        internal readonly int[] LastPosUpdate = new int[(int)TweenThing.Max];
        internal int LastAngleTick;
        private float _movedPercent;
        private bool _isMoving;
        private WalkCycleDef _walkCycle;
    }
}