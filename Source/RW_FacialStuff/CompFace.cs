namespace FacialStuff
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Animator;
    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Graphics;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class CompFace : ThingComp
    {
        public bool DontRender;

        [NotNull]
        public FaceGraphic FaceGraphic = new FaceGraphic();

        public bool IgnoreRenderer;

        public bool IsChild;

        public bool NeedsStyling = true;

        public Faction originFaction;

        [NotNull]
        public Pawn pawn;

        public bool Roofed;

        public int rotationInt;

        private Vector2 eyeOffset = Vector2.zero;

        [NotNull]
        private PawnEyeWiggler eyeWiggler;

        private float factionMelanin;

        private PawnHeadRotator headRotator;

        // private float blinkRate;
        // public PawnHeadWiggler headWiggler;

        private Vector2 mouthOffset = Vector2.zero;

        // must be null, always initialize with pawn
        [CanBeNull]
        private PawnFace pawnFace;

        private FaceMaterial faceMaterial;

        // public bool IgnoreRenderer;
        public GraphicVectorMeshSet EyeMeshSet => MeshPoolFS.HumanEyeSet[(int)this.FullHeadType];

        [NotNull]
        public PawnEyeWiggler EyeWiggler => this.eyeWiggler;

        public float FactionMelanin
        {
            get => this.factionMelanin;
            set => this.factionMelanin = value;
        }

        public FullHead FullHeadType { get; set; } = FullHead.Undefined;

        public PawnHeadRotator HeadRotator => this.headRotator;

        [NotNull]
        public GraphicVectorMeshSet MouthMeshSet => MeshPoolFS.HumanlikeMouthSet[(int)this.FullHeadType];

        public CrownType PawnCrownType => this.pawn?.story.crownType ?? CrownType.Average;

        [CanBeNull]
        public PawnFace PawnFace => this.pawnFace;

        public HeadType PawnHeadType
        {
            get
            {
                if (this.pawn.story?.HeadGraphicPath == null)
                {
                    return HeadType.Normal;
                }

                if (this.pawn.story.HeadGraphicPath.Contains("Pointy"))
                {
                    return HeadType.Pointy;
                }

                if (this.pawn.story.HeadGraphicPath.Contains("Wide"))
                {
                    return HeadType.Wide;
                }

                return HeadType.Normal;
            }
        }

        public FaceMaterial FaceMaterial => this.faceMaterial;

        // only for development
        public Vector3 BaseEyeOffsetAt(Rot4 rotation)
        {
            bool male = this.pawn.gender == Gender.Male;

            if (this.PawnCrownType == CrownType.Average)
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleAverageNormalOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleAveragePointyOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleAverageWideOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleAverageWideOffset;
                        }

                        break;
                }
            }
            else
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleNarrowNormalOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleNarrowNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleNarrowPointyOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleNarrowPointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.eyeOffset = MeshPoolFS.EyeMaleNarrowWideOffset;
                        }
                        else
                        {
                            this.eyeOffset = MeshPoolFS.EyeFemaleNarrowWideOffset;
                        }

                        break;
                }
            }

            switch (rotation.AsInt)
            {
                case 1: return new Vector3(this.eyeOffset.x, 0f, -this.eyeOffset.y);
                case 2: return new Vector3(0, 0f, -this.eyeOffset.y);
                case 3: return new Vector3(-this.eyeOffset.x, 0f, -this.eyeOffset.y);
                default: return Vector3.zero;
            }
        }

        // only for development
        public Vector3 BaseMouthOffsetAt(Rot4 rotation)
        {
            bool male = this.pawn.gender == Gender.Male;

            if (this.PawnCrownType == CrownType.Average)
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            this.mouthOffset = MeshPoolFS.MouthMaleAverageNormalOffset;
                        }
                        else
                        {
                            this.mouthOffset = MeshPoolFS.MouthFemaleAverageNormalOffset;
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            this.mouthOffset = MeshPoolFS.MouthMaleAveragePointyOffset;
                        }
                        else
                        {
                            this.mouthOffset = MeshPoolFS.MouthFemaleAveragePointyOffset;
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            this.mouthOffset = MeshPoolFS.MouthMaleAverageWideOffset;
                        }
                        else
                        {
                            this.mouthOffset = MeshPoolFS.MouthFemaleAverageWideOffset;
                        }

                        break;
                }
            }
            else
            {
                switch (this.PawnHeadType)
                {
                    case HeadType.Normal:
                        this.mouthOffset = male ? MeshPoolFS.MouthMaleNarrowNormalOffset : MeshPoolFS.MouthFemaleNarrowNormalOffset;

                        break;

                    case HeadType.Pointy:
                        this.mouthOffset = male ? MeshPoolFS.MouthMaleNarrowPointyOffset : MeshPoolFS.MouthFemaleNarrowPointyOffset;

                        break;

                    case HeadType.Wide:
                        this.mouthOffset = male ? MeshPoolFS.MouthMaleNarrowWideOffset : MeshPoolFS.MouthFemaleNarrowWideOffset;

                        break;
                }
            }

            switch (rotation.AsInt)
            {
                case 1: return new Vector3(this.mouthOffset.x, 0f, -this.mouthOffset.y);
                case 2: return new Vector3(0, 0f, -this.mouthOffset.y);
                case 3: return new Vector3(-this.mouthOffset.x, 0f, -this.mouthOffset.y);
                default: return Vector3.zero;
            }
        }

        [NotNull]
        public string BrowTexPath([NotNull] BrowDef browDef)
        {
            return "Brows/Brow_" + this.pawn.gender + "_" + browDef.texPath;
        }

        // Can be called externally
        public void CheckForAddedOrMissingParts([NotNull] Pawn p)
        {
            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            this.pawn = p;

            // no head => no face
            if (!this.pawn?.health.hediffSet.HasHead != true)
            {
                return;
            }

            List<BodyPartRecord> body = this.pawn?.RaceProps?.body?.AllParts;
            List<Hediff> hediffs = this.pawn?.health?.hediffSet?.hediffs;

            if (hediffs.NullOrEmpty() || body.NullOrEmpty())
            {
                // || hediffs.Any(x => x.def == HediffDefOf.MissingBodyPart && x.Part.def == BodyPartDefOf.Head))
                return;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            foreach (Hediff diff in hediffs.Where(diff => diff?.def?.defName != null))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                this.CheckPart(body, diff);
            }
        }

        // TODO: Remove or make usable
        // public void DefineSkinDNA()
        // {
        // HairMelanin.SkinGenetics(this.pawn, this, out this.factionMelanin);
        // this.IsSkinDNAoptimized = true;
        // }
        [NotNull]
        public string EyeTexPath([NotNull] string eyeDefPath, Side side)
        {
            // ReSharper disable once PossibleNullReferenceException
            string path = "Eyes/Eye_" + eyeDefPath + "_" + this.pawn.gender + "_" + side;

            return path;
        }

        [NotNull]
        public string GetBeardPath(BeardDef def)
        {
            if (def == BeardDefOf.Beard_Shaved)
            {
                return "Beards/Beard_Shaved";
            }

            return "Beards/Beard_" + this.PawnHeadType + "_" + def.texPath + "_" + this.PawnCrownType;
        }

        [NotNull]
        public string GetMoustachePath(MoustacheDef def)
        {
            if (def == MoustacheDefOf.Shaved)
            {
                return this.GetBeardPath(BeardDefOf.Beard_Shaved);
            }

            return def.texPath + "_" + this.PawnCrownType;
        }

        public Quaternion HeadQuat(Rot4 rotation)
        {
            float num = 1f;
            Quaternion asQuat = rotation.AsQuat;
            float x = 1f * Mathf.Sin(num * (this.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            float z = 1f * Mathf.Cos(num * (this.HeadRotator.CurrentMovement * 0.1f) % (2 * Mathf.PI));
            asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);
            return asQuat;
        }

        public bool HasEyePatchLeft { get; set; }

        public bool HasEyePatchRight { get; set; }

        public override void PostDraw()
        {
            base.PostDraw();

            // Children & Pregnancy || Werewolves transformed
            if (this.pawn?.Map == null || !this.pawn.Spawned || this.pawn.Dead || this.IsChild || this.DontRender)
            {
                return;
            }

            this.Roofed = this.pawn.Position.Roofed(this.pawn.Map);

            if (Find.TickManager.Paused)
            {
                return;
            }

            // CellRect viewRect = Find.CameraDriver.CurrentViewRect;
            // viewRect = viewRect.ExpandedBy(5);
            // if (!viewRect.Contains(this.pawn.Position))
            // {
            // return;
            // }
            this.EyeWiggler.WigglerTick();

            if (!this.EyeWiggler.IsAsleep)
            {
                this.headRotator.RotatorTick();
            }

            // Low-prio stats
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                this.FaceGraphic.SetMouthAccordingToMoodLevel();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref this.pawn, "Pawn");
            Scribe_References.Look(ref this.originFaction, "pawnFaction");

            // Scribe_Values.Look(ref this.pawnFace.MelaninOrg, "MelaninOrg");

            // Log.Message(
            // "Facial Stuff updated pawn " + this.parent.Label + "-" + face.BeardDef + "-" + face.EyeDef);

            // Force ResolveAllGraphics
            Scribe_Deep.Look(ref this.pawnFace, "pawnFace");

            // Scribe_References.Look(ref this.pawn, "pawn");
            Scribe_Values.Look(ref this.IsChild, "isChild");
            Scribe_Values.Look(ref this.DontRender, "dontrender");
            Scribe_Values.Look(ref this.Roofed, "roofed");
            Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
        }

        /// <summary>
        /// Basic pawn initialization.
        /// </summary>
        /// <param name="p">
        /// The pawn.
        /// </param>
        /// <returns>
        /// Success if all initialized.
        /// </returns>
        public bool SetHeadType([NotNull] Pawn p)
        {
            this.pawn = p;

            if (this.originFaction == null)
            {
                this.originFaction = this.pawn.Faction ?? Faction.OfPlayer;
            }

            if (this.PawnFace == null)
            {
                this.SetPawnFace(new PawnFace(this.pawn, this.originFaction.def));
            }

            // this.isMasochist = this.pawn.story.traits.HasTrait(TraitDef.Named("Masochist"));
            this.eyeWiggler = new PawnEyeWiggler(this.pawn);
            this.headRotator = new PawnHeadRotator(this.pawn);

            // this.headWiggler = new PawnHeadWiggler(this.pawn);

            // ReSharper disable once PossibleNullReferenceException
            this.ResetBoolsAndPaths();
            this.CheckForAddedOrMissingParts();

            // Only for the crowntype ...
            CrownTypeChecker.SetHeadOffsets(this.pawn, this);

            return true;
        }

        public void SetPawnFace([NotNull] PawnFace inportedFace)
        {
            this.pawnFace = inportedFace;
        }
        public bool hasNaturalJaw = true;

        private void CheckForAddedOrMissingParts()
        {
            this.CheckForAddedOrMissingParts(this.parent as Pawn);
        }

        private void CheckPart([NotNull] List<BodyPartRecord> body, [NotNull] Hediff hediff)
        {
            if (body.NullOrEmpty() || hediff.def == null)
            {
                return;
            }

            BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
            BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);
            BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);
            AddedBodyPartProps addedPartProps = hediff.def?.addedPartProps;

            if (addedPartProps != null)
            {
                if (hediff.def?.defName != null && hediff.Part != null)
                {
                    if (hediff.Part == leftEye)
                    {
                        this.FaceGraphic.texPathEyeLeftPatch = "AddedParts/" + hediff.def.defName + "_Left" + "_"
                                                   + this.PawnCrownType;
                    }

                    if (hediff.Part == rightEye)
                    {
                        this.FaceGraphic.texPathEyeRightPatch = "AddedParts/" + hediff.def.defName + "_Right" + "_"
                                                    + this.PawnCrownType;
                    }

                    if (hediff.Part == jaw)
                    {
                        this.FaceGraphic.texPathJawAddedPart = "Mouth/Mouth_" + hediff.def.defName;
                    }
                }
            }

            if (hediff.def != HediffDefOf.MissingBodyPart)
            {
                return;
            }

            if (leftEye != null && hediff.Part == leftEye)
            {
                this.FaceGraphic.texPathEyeLeft = this.EyeTexPath("Missing", Side.Left);
                this.EyeWiggler.EyeLeftCanBlink = false;
            }

            // ReSharper disable once InvertIf
            if (rightEye != null && hediff.Part == rightEye)
            {
                this.FaceGraphic.texPathEyeRight = this.EyeTexPath("Missing", Side.Right);
                this.EyeWiggler.EyeRightCanBlink = false;
            }
        }

        [NotNull]
        private string EyeClosedTexPath(Side side)
        {
            return this.EyeTexPath("Closed", side);
        }



        private void ResetBoolsAndPaths()
        {
            // Fix for PrepC for pre-FS pawns, also sometimes the brows are not defined?!?
            if (this.PawnFace?.EyeDef == null || this.PawnFace.BrowDef == null || this.PawnFace.BeardDef == null)
            {
                this.SetPawnFace(new PawnFace(this.pawn, Faction.OfPlayer.def));
            }

            this.FaceGraphic.texPathEyeLeftPatch = null;
            this.FaceGraphic.texPathEyeRightPatch = null;
            this.FaceGraphic.texPathJawAddedPart = null;

            EyeDef pawnFaceEyeDef = this.PawnFace.EyeDef;
            this.FaceGraphic.texPathEyeRight = this.EyeTexPath(pawnFaceEyeDef.texPath, Side.Right);
            this.FaceGraphic.texPathEyeLeft = this.EyeTexPath(pawnFaceEyeDef.texPath, Side.Left);

            this.FaceGraphic.texPathEyeRightClosed = this.EyeClosedTexPath(Side.Right);
            this.FaceGraphic.texPathEyeLeftClosed = this.EyeClosedTexPath(Side.Left);

            this.EyeWiggler.EyeLeftCanBlink = true;
            this.EyeWiggler.EyeRightCanBlink = true;

            this.FaceGraphic.texPathBrow = this.BrowTexPath(this.PawnFace.BrowDef);
        }

        // Only call this AFTER the FaceGraphic is set!!!
        public void SetFaceMaterial()
        {
            this.faceMaterial = new FaceMaterial(this, this.FaceGraphic);
        }
    }
}