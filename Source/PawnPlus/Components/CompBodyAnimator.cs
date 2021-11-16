namespace PawnPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using JetBrains.Annotations;

    using PawnPlus.AnimatorWindows;
    using PawnPlus.Graphics;
    using PawnPlus.Harmony;
    using PawnPlus.Tweener;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.AI;

    public class CompBodyAnimator : ThingComp
    {
        #region Public Fields

        public bool Deactivated;
        public bool IgnoreRenderer;

        [CanBeNull] public BodyAnimDef BodyAnim;

        public BodyPartStats BodyStat;

        public float JitterMax = 0.35f;
        public readonly Vector3Tween[] Vector3Tweens = new Vector3Tween[(int)TweenThing.Max];

        // [CanBeNull] public PawnPartsTweener PartTweener;
        [CanBeNull] public PawnBodyGraphic PawnBodyGraphic;

        [CanBeNull] public PoseCycleDef PoseCycle;

        public Vector3 FirstHandPosition;
        public Vector3 SecondHandPosition;

        [CanBeNull] public WalkCycleDef WalkCycle => _walkCycle;

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
        
        public bool InRoom
        {
            get
            {
                // RoomGroup theRoomGroup = TheRoom?.Group;
                if(TheRoom != null && !TheRoom.UsesOutdoorTemperature)
                {
                    // Pawn is indoors
                    return !Pawn.Drafted || !Controller.settings.IgnoreWhileDrafted;
                }

                return false;
            }
        }

        public JitterHandler Jitterer => GetHiddenValue(typeof(Pawn_DrawTracker), Pawn.Drawer, "jitterer", _infoJitterer) as JitterHandler;

        [NotNull]
        public Pawn Pawn => parent as Pawn;

        public List<PawnBodyDrawer> PawnBodyDrawers { get; private set; }

        public CompProperties_BodyAnimator Props => (CompProperties_BodyAnimator)props;

        public bool HideHat => InRoom && Controller.settings.HideHatWhileRoofed && (Pawn.IsColonistPlayerControlled && Pawn.Faction.IsPlayer && !Pawn.HasExtraHomeFaction());

        #endregion Public Properties

        #region Private Properties

        [CanBeNull]
        private Room TheRoom
        {
            get
            {
                if (Pawn.Dead)
                {
                    return null;
                }

                if (Find.TickManager.TicksGame < _lastRoomCheck + 60f)
                {
                    return _theRoom;
                }

                _theRoom = Pawn.GetRoom();
                _lastRoomCheck = Find.TickManager.TicksGame;

                return _theRoom;
            }
        }

        #endregion Private Properties

        #region Public Methods

        public static object GetHiddenValue(Type type, object __instance, string fieldName, [CanBeNull] FieldInfo info)
        {
            if(info == null)
            {
                info = type.GetField(fieldName, GenGeneric.BindingFlagsAll);
            }

            return info?.GetValue(__instance);
        }

        public void ApplyBodyWobble(ref Vector3 rootLoc, ref Vector3 footPos, ref Quaternion quat)
        {
            if(PawnBodyDrawers == null)
            {
                return;
            }

            int i = 0;
            int count = PawnBodyDrawers.Count;
            while(i < count)
            {
                PawnBodyDrawers[i].ApplyBodyWobble(ref rootLoc, ref footPos, ref quat);
                i++;
            }
        }

        // Verse.PawnGraphicSet
        public void ClearCache()
        {
            _cachedSkinMatsBodyBaseHash = -1;
            _cachedNakedMatsBodyBaseHash = -1;
        }
        
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void DrawFeet(Quaternion bodyQuat, Quaternion footQuat, Vector3 rootLoc, bool portrait, float factor = 1f)
        {
            if(!PawnBodyDrawers.NullOrEmpty())
            {
                int i = 0;
                int count = PawnBodyDrawers.Count;
                while(i < count)
                {
                    PawnBodyDrawers[i].DrawFeet(bodyQuat, footQuat, rootLoc, portrait, factor);
                    i++;
                }
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void DrawHands(Quaternion bodyQuat, Vector3 rootLoc, bool portrait, Thing carriedThing = null, bool flip = false, float factor = 1f)
        {
            if (PawnBodyDrawers.NullOrEmpty()) return;
            int i = 0;
            int count = PawnBodyDrawers.Count;
            while(i < count)
            {
                PawnBodyDrawers[i].DrawHands(bodyQuat, rootLoc, portrait, carriedThing, flip, factor);
                i++;
            }
        }

        public void InitializePawnDrawer()
        {
            if(Props.bodyDrawers.Any())
            {
                PawnBodyDrawers = new List<PawnBodyDrawer>();
                for(int i = 0; i < Props.bodyDrawers.Count; i++)
                {
                    PawnBodyDrawer thingComp =
                    (PawnBodyDrawer)Activator.CreateInstance(Props.bodyDrawers[i].GetType());
                    thingComp.CompAnimator = this;
                    thingComp.Pawn = Pawn;
                    PawnBodyDrawers.Add(thingComp);
                    thingComp.Initialize();
                }
            }
            else
            {
                PawnBodyDrawers = new List<PawnBodyDrawer>();
                bool isQuaduped = Pawn.GetCompAnim().BodyAnim.quadruped;
                PawnBodyDrawer thingComp = isQuaduped
                                               ? (PawnBodyDrawer)Activator.CreateInstance(typeof(QuadrupedDrawer))
                                               : (PawnBodyDrawer)Activator.CreateInstance(typeof(HumanBipedDrawer));
                thingComp.CompAnimator = this;
                thingComp.Pawn = Pawn;
                PawnBodyDrawers.Add(thingComp);
                thingComp.Initialize();
            }
        }

        public List<Material> NakedMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if(num != _cachedNakedMatsBodyBaseHash)
            {
                _cachedNakedMatsBodyBase.Clear();
                _cachedNakedMatsBodyBaseHash = num;
                PawnGraphicSet graphics = Pawn.Drawer.renderer.graphics;
                if(bodyCondition == RotDrawMode.Fresh)
                {
                    _cachedNakedMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing));
                }
                else if(bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    _cachedNakedMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing));
                }
                else if(bodyCondition == RotDrawMode.Dessicated)
                {
                    _cachedNakedMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing));
                }

                for(int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayerDef lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    if(Pawn.Dead)
                    {
                        if(lastLayer != ApparelLayerDefOf.Shell && lastLayer != ApparelLayerDefOf.Overhead)
                        {
                            _cachedNakedMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                        }
                    }
                }
            }

            return _cachedNakedMatsBodyBase;
        }
        
        public override void PostDraw()
        {
            base.PostDraw();

            // Children & Pregnancy || Werewolves transformed
            if(Pawn.Map == null || !Pawn.Spawned || Pawn.Dead || Pawn.GetCompAnim().Deactivated)
            {
                return;
            }
            
            // Tweener
            Vector3Tween eqTween = Vector3Tweens[(int)HarmonyPatchesFS.equipment];
            FloatTween angleTween = AimAngleTween;
            Vector3Tween leftHand = Vector3Tweens[(int)TweenThing.HandLeft];
            Vector3Tween rightHand = Vector3Tweens[(int)TweenThing.HandRight];
            if(!Find.TickManager.Paused)
            {
                if(leftHand.State == TweenState.Running)
                {
                    leftHand.Update(1f * Find.TickManager.TickRateMultiplier);
                }

                if(rightHand.State == TweenState.Running)
                {
                    rightHand.Update(1f * Find.TickManager.TickRateMultiplier);
                }

                if(eqTween.State == TweenState.Running)
                {
                    eqTween.Update(1f * Find.TickManager.TickRateMultiplier);
                }

                if(angleTween.State == TweenState.Running)
                {
                    AimAngleTween.Update(3f * Find.TickManager.TickRateMultiplier);
                }

                CheckMovement();

                if(Pawn.IsChild())
                TickDrawers(Pawn.Rotation, new PawnGraphicSet(Pawn));
            }

            if(Pawn.IsChild())
            {
                float angle = Pawn.Drawer.renderer.BodyAngle();
                Quaternion bodyQuat = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 rootLoc = Pawn.Drawer.DrawPos;
                if(Controller.settings.UseHands)
				{
                    DrawHands(bodyQuat, rootLoc, false, null, false, Pawn.GetBodysizeScaling());
                }

                if(Controller.settings.UseFeet)
				{
                    DrawFeet(bodyQuat, bodyQuat, rootLoc, false);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _lastRoomCheck, "lastRoomCheck");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            for (int i = 0; i < Vector3Tweens.Length; i++)
            {
                Vector3Tweens[i] = new Vector3Tween();
            }

            PawnBodyGraphic = new PawnBodyGraphic(this);

            string bodyType = "Undefined";

            if(Pawn.story?.bodyType != null)
            {
                bodyType = Pawn.story.bodyType.ToString();
            }

            string defaultName = "BodyAnimDef_" + Pawn.def.defName + "_" + bodyType;
            List<string> names = new List<string>
                                     {
                                         defaultName,
                                     };

            bool needsNewDef = true;
            foreach(string name in names)
            {
                BodyAnimDef dbDef = DefDatabase<BodyAnimDef>.GetNamedSilentFail(name);
                if(dbDef == null)
                {
                    continue;
                }

                BodyAnim = dbDef;
                needsNewDef = false;
                break;
            }

            if(needsNewDef)
            {
                BodyAnim = new BodyAnimDef { defName = defaultName, label = defaultName };
                DefDatabase<BodyAnimDef>.Add(BodyAnim);
            }
        }

        public void TickDrawers(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            if(!_initialized)
            {
                InitializePawnDrawer();
                _initialized = true;
            }

            if(PawnBodyDrawers.NullOrEmpty()) return;
            int i = 0;
            int count = PawnBodyDrawers.Count;
            while(i < count)
            {
                PawnBodyDrawers[i].Tick(bodyFacing, graphics);
                i++;
            }
        }

        public List<Material> UnderwearMatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if(num != _cachedSkinMatsBodyBaseHash)
            {
                _cachedSkinMatsBodyBase.Clear();
                _cachedSkinMatsBodyBaseHash = num;
                PawnGraphicSet graphics = Pawn.Drawer.renderer.graphics;
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    _cachedSkinMatsBodyBase.Add(graphics.nakedGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Rotting || graphics.dessicatedGraphic == null)
                {
                    _cachedSkinMatsBodyBase.Add(graphics.rottingGraphic.MatAt(facing));
                }
                else if (bodyCondition == RotDrawMode.Dessicated)
                {
                    _cachedSkinMatsBodyBase.Add(graphics.dessicatedGraphic.MatAt(facing));
                }

                for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                {
                    ApparelLayerDef lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                    if (lastLayer == ApparelLayerDefOf.OnSkin)
                    {
                        _cachedSkinMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                    }
                }

                // One more time to get at least one pieces of cloth
                if (_cachedSkinMatsBodyBase.Count < 1)
                {
                    for (int i = 0; i < graphics.apparelGraphics.Count; i++)
                    {
                        ApparelLayerDef lastLayer = graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer;

                        if (lastLayer == ApparelLayerDefOf.Middle)
                        {
                            _cachedSkinMatsBodyBase.Add(graphics.apparelGraphics[i].graphic.MatAt(facing));
                        }
                    }
                }
            }

            return _cachedSkinMatsBodyBase;
        }

        #endregion Public Methods

        public float MovedPercent => _movedPercent;
        public float BodyAngle;

        public float LastAimAngle = 143f;

        // public float lastWeaponAngle = 53f;
        public readonly Vector3[] LastPosition = new Vector3[(int)TweenThing.Max];

        public readonly FloatTween AimAngleTween = new FloatTween();
        public bool HasLeftHandPosition => SecondHandPosition != Vector3.zero;

        public Vector3 LastEqPos = Vector3.zero;
        public float DrawOffsetY;

        public void CheckMovement()
        {
            if (HarmonyPatchesFS.AnimatorIsOpen() && MainTabWindow_BaseAnimator.Pawn == Pawn)
            {
                _isMoving = true;
                _movedPercent = MainTabWindow_BaseAnimator.AnimationPercent;
                return;
            }

            if (IsRider)
            {
                _isMoving = false;
                return;
            }

            // pawn started pathing
            Pawn_PathFollower pather = Pawn.pather;
            if ((pather != null) && (pather.Moving) && !Pawn.stances.FullBodyBusy
                && (pather.BuildingBlockingNextPathCell() == null)
                && (pather.NextCellDoorToWaitForOrManuallyOpen() == null)
                && !pather.WillCollideWithPawnOnNextPathCell())
            {
                _movedPercent = 1f - pather.nextCellCostLeft / pather.nextCellCostTotal;
                _isMoving = true;
            }
            else
            {
                _isMoving = false;
            }
        }

        public bool IsRider { get; set; }

        public void DrawAlienBodyAddons(
            Quaternion quat, 
            Vector3 vector, 
            bool portrait, 
            bool renderBody, 
            Rot4 rotation,
            bool invisible)
        {
            if(PawnBodyDrawers.NullOrEmpty())
            {
                return;
            }

            int i = 0;
            int count = PawnBodyDrawers.Count;
            while(i < count)
            {
                PawnBodyDrawers[i].DrawAlienBodyAddons(portrait, vector, quat, renderBody, rotation, invisible);
                i++;
            }
        }

        public void SetWalkCycle(WalkCycleDef walkCycleDef)
        {
            _walkCycle = walkCycleDef;
        }
        
        public float BodyOffsetZ
        {
            get
            {
                if(Controller.settings.UseFeet)
                {
                    WalkCycleDef walkCycle = WalkCycle;
                    if(walkCycle != null)
                    {
                        SimpleCurve curve = walkCycle.BodyOffsetZ;
                        if(curve.PointsCount > 0)
						{
                            return curve.Evaluate(MovedPercent);
                        }
                    }
                }

                return 0f;
            }
        }

        public bool IsMoving => _isMoving;
        internal bool MeshFlipped;
        internal float LastWeaponAngle;
        internal readonly int[] LastPosUpdate = new int[(int)TweenThing.Max];
        internal int LastAngleTick;
        private float _movedPercent;
        private bool _isMoving;
        private WalkCycleDef _walkCycle;
    }
}