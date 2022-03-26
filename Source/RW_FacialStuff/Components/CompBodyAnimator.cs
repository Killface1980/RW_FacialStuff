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

        [CanBeNull]
        public WalkCycleDef WalkCycle { get; private set; }

        public Quaternion WeaponQuat = new();

        #endregion Public Fields

        #region Private Fields

        private static readonly FieldInfo _infoJitterer;

        [NotNull] private readonly List<Material> _cachedNakedMatsBodyBase = new();

        private readonly List<Material> _cachedSkinMatsBodyBase = new();

        private int _cachedNakedMatsBodyBaseHash = -1;
        private int _cachedSkinMatsBodyBaseHash = -1;
        private int _lastRoomCheck;

        private bool _initialized;

        [CanBeNull] private Room _theRoom;

        #endregion Private Fields

        #region Public Properties

        public BodyAnimator BodyAnimator { get; private set; }

        public bool HideShellLayer => this.InRoom && Controller.settings.HideShellWhileRoofed && (this.ThePawn.IsColonistPlayerControlled && this.ThePawn.Faction.IsPlayer && !this.ThePawn.HasExtraHomeFaction());

        public bool InPrivateRoom
        {
            get
            {
                if (!this.InRoom || this.ThePawn.IsPrisoner)
                {
                    return false;
                }

                Room ownedRoom = this.ThePawn.ownership?.OwnedRoom;
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
                if (TheRoom != null && !TheRoom.UsesOutdoorTemperature)
                {
                    // Pawn is indoors
                    return !this.ThePawn.Drafted || !Controller.settings.IgnoreWhileDrafted;
                }

                return false;

                // return !room?.Group.UsesOutdoorTemperature == true && Controller.settings.IgnoreWhileDrafted || !this.pawn.Drafted;
            }
        }

        public JitterHandler Jitterer
            => GetHiddenValue(typeof(Pawn_DrawTracker), this.ThePawn.Drawer, "jitterer", _infoJitterer) as
                JitterHandler;

        [NotNull]
        public Pawn ThePawn => this.parent as Pawn;

        public List<PawnBodyDrawer> PawnBodyDrawers { get; private set; }

        public CompProperties_BodyAnimator Props => (CompProperties_BodyAnimator)this.props;

        public bool HideHat => this.InRoom && Controller.settings.HideHatWhileRoofed && (this.ThePawn.IsColonistPlayerControlled && this.ThePawn.Faction.IsPlayer && !this.ThePawn.HasExtraHomeFaction());

        #endregion Public Properties

        #region Private Properties

        [CanBeNull]
        private Room TheRoom
        {
            get
            {
                if (this.ThePawn.Dead)
                {
                    return null;
                }

                if (Find.TickManager.TicksGame < this._lastRoomCheck + 60f)
                {
                    return this._theRoom;
                }

                this._theRoom = this.ThePawn.GetRoom();
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

        public void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos)
        {
            if (this.PawnBodyDrawers == null)
            {
                return;
            }

            int i = 0;
            int count = this.PawnBodyDrawers.Count;
            while (i < count)
            {
                this.PawnBodyDrawers[i].ApplyBodyWobble(ref rootLoc, ref footPos);
                i++;
            }
        }

        // Verse.PawnGraphicSet
        public void ClearCache()
        {
            this._cachedSkinMatsBodyBaseHash = -1;
            this._cachedNakedMatsBodyBaseHash = -1;
        }

        public void DrawApparel(Quaternion quat, Vector3 vector, PawnRenderFlags flags, bool renderBody)
        {
            if (this.PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = this.PawnBodyDrawers.Count;
            while (i < count)
            {
                this.PawnBodyDrawers[i].DrawApparel(quat, vector, renderBody, flags);
                i++;
            }
        }

        // public override string CompInspectStringExtra()
        // {
        //     string extra = this.Pawn.DrawPos.ToString();
        //     return extra;
        // }

        // off for now
        public void DrawBody(Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, out Mesh bodyMesh)
        {
            bodyMesh = null;

            if (this.PawnBodyDrawers.NullOrEmpty())
            {
                InitializePawnDrawer();
            }

            int i = 0;

            while (i < this.PawnBodyDrawers.Count)
            {
                this.PawnBodyDrawers[i].DrawPawnBody(
                                                 rootLoc,
                                                 angle,
                                                 facing,
                                                 bodyDrawType,
                                                 flags,
                                                 out bodyMesh);
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

        public void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait, float factor = 1f)
        {
            if (!this.PawnBodyDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = this.PawnBodyDrawers.Count;
                while (i < count)
                {
                    this.PawnBodyDrawers[i].DrawFeet(bodyQuat, footQuat, rootLoc, portrait, factor);
                    i++;
                }
            }
        }

        public void DrawHands(Quaternion bodyQuat, Vector3 rootLoc, bool portrait, Thing carriedThing = null, bool flip = false, float factor = 1f)
        {
            if (this.PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }

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
                    thingComp.ThePawn = this.ThePawn;
                    this.PawnBodyDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
            else
            {
                this.PawnBodyDrawers = new List<PawnBodyDrawer>();
                bool isQuaduped = this.ThePawn.GetCompAnim().BodyAnim.quadruped;
                PawnBodyDrawer thingComp = isQuaduped
                    ? (PawnBodyDrawer)Activator.CreateInstance(typeof(QuadrupedDrawer))
                    : (PawnBodyDrawer)Activator.CreateInstance(typeof(HumanBipedDrawer));
                thingComp.CompAnimator = this;
                thingComp.ThePawn = this.ThePawn;
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
                PawnGraphicSet graphics = this.ThePawn.Drawer.renderer.graphics;
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

                    if (this.ThePawn.Dead)
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

        //ToDO: Move this and the Face PostDraw to Postfix // Verse.Pawn_DrawTracker.DrawTrackerTick()
        public override void PostDraw()
        {
            base.PostDraw();

            // Children & Pregnancy || Werewolves transformed
            if (this.ThePawn.Map == null || !this.ThePawn.Spawned || this.ThePawn.Dead || this.ThePawn.GetCompAnim().Deactivated)
            {
                return;
            }
            //if (Find.TickManager.Paused)
            //{
            //    if (!HarmonyPatchesFS.AnimatorIsOpen() || MainTabWindow_BaseAnimator.Pawn != this.Pawn)
            //    {
            //        if (!Pawn.IsChild()) return;
            //    }
            //}

            if (this.Props.bipedWithHands)
            {
                BodyAnimator.AnimatorTick();
            }

                // Tweener
                Vector3Tween eqTween = this.Vector3Tweens[(int)HarmonyPatchesFS.equipment];

                FloatTween angleTween = this.AimAngleTween;
                Vector3Tween leftHand = this.Vector3Tweens[(int)TweenThing.HandLeft];
                Vector3Tween rightHand = this.Vector3Tweens[(int)TweenThing.HandRight];
                if (!Find.TickManager.Paused)
                {
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

                    if (this.ThePawn.IsChild())
                {
                    this.TickDrawers(this.ThePawn.Rotation, new PawnGraphicSet(this.ThePawn));
                }
            }


            if (this.ThePawn.IsChild())
            {
                float angle = this.ThePawn.Drawer.renderer.BodyAngle();
                Quaternion bodyQuat = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 rootLoc = this.ThePawn.Drawer.DrawPos;
                if (Controller.settings.UseHands)
                {
                    this.DrawHands(bodyQuat, rootLoc, false, null, false, this.ThePawn.GetBodysizeScaling());
                }

                if (Controller.settings.UseFeet)
                {
                    this.DrawFeet(bodyQuat, bodyQuat, rootLoc, false);
                }
            }
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
            this.BodyAnimator = new BodyAnimator(this.ThePawn, this);
            this.ThePawn.CheckForAddedOrMissingParts();
            this.PawnBodyGraphic = new PawnBodyGraphic(this);

            string bodyType = "Undefined";

            if (this.ThePawn.story?.bodyType != null)
            {
                bodyType = this.ThePawn.story.bodyType.ToString();
            }

            string defaultName = "BodyAnimDef_" + this.ThePawn.def.defName + "_" + bodyType;
            List<string> names = new()
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

            if (this.PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }

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
                PawnGraphicSet graphics = this.ThePawn.Drawer.renderer.graphics;
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
                if (this._cachedSkinMatsBodyBase.Count < 1)
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

        public float MovedPercent { get; private set; }
        public float BodyAngle;

        public float LastAimAngle = 143f;

        //  public float lastWeaponAngle = 53f;
        public readonly Vector3[] LastPosition = new Vector3[(int)TweenThing.Max];

        public readonly FloatTween AimAngleTween = new();
        public bool HasLeftHandPosition => this.SecondHandPosition != Vector3.zero;

        public Vector3 LastEqPos = Vector3.zero;
        public float DrawOffsetY;

        public void CheckMovement()
        {
            if (HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.thePawn == this.ThePawn)
            {
                this.IsMoving = true;
                this.MovedPercent = MainTabWindow_BaseAnimator.AnimationPercent;
                return;
            }

            if (this.IsRider)
            {
                this.IsMoving = false;
                return;
            }
            // pawn started pathing

            this.MovedPercent = PawnMovedPercent(ThePawn);

        }
        private float PawnMovedPercent(Pawn pawn)
        {
            Pawn_PathFollower pather = pawn.pather;
            if (pather.Moving)
            {
                if (pawn.stances.FullBodyBusy)
                {
                    return 0f;
                }
                if (pather.BuildingBlockingNextPathCell() != null)
                {
                    return 0f;
                }
                if (pather.NextCellDoorToWaitForOrManuallyOpen() != null)
                {
                    return 0f;
                }
                if (pather.WillCollideWithPawnOnNextPathCell())
                {
                    return 0f;
                }
                this.IsMoving = true;
                return 1f - pather.nextCellCostLeft / pather.nextCellCostTotal;
            }
            return 0f;
        }
        public bool IsRider { get; set; } = false;

        public void DrawAlienBodyAddons(Quaternion quat, Vector3 vector, PawnRenderFlags flags, bool renderBody, Rot4 rotation,
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
                this.PawnBodyDrawers[i].DrawAlienBodyAddons(flags, vector, quat, renderBody, rotation, invisible);
                i++;
            }
        }

        public void SetWalkCycle(WalkCycleDef walkCycleDef)
        {
            this.WalkCycle = walkCycleDef;
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
                        {
                            return curve.Evaluate(this.MovedPercent);
                        }
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
                        {
                            return curve.Evaluate(this.MovedPercent);
                        }
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
                        {
                            return curve.Evaluate(this.MovedPercent);
                        }
                    }
                }

                return 0f;
            }
        }

        public bool IsMoving { get; private set; }
        internal bool MeshFlipped;
        internal float LastWeaponAngle;
        internal readonly int[] LastPosUpdate = new int[(int)TweenThing.Max];
        internal int LastAngleTick;
    }
}