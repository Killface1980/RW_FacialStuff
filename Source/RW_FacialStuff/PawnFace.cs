namespace FacialStuff
{
    using System.Diagnostics.CodeAnalysis;

    using FacialStuff.Defs;
    using FacialStuff.Genetics;
    using FacialStuff.Utilities;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnFace : IExposable
    {
        private Color beardColor;

        [CanBeNull]
        private BeardDef beardDef;

        [CanBeNull]
        private BrowDef browDef;

        // public float Cuticula;
        private bool drawMouth = true;

        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private float euMelanin;

        [CanBeNull]
        private EyeDef eyeDef;

        private float greyness;

        private Color hairColor;

        private bool hasSameBeardColor;

        private MoustacheDef moustacheDef;

        private float pheoMelanin;

        private WrinkleDef wrinkleDef;

        private float wrinkleIntensity;

        public PawnFace([NotNull] CompFace face, FactionDef pawnFactionDef, bool newPawn = true)
        {
            Pawn pawn = face.Pawn;
            this.DrawMouth = true;

            this.EyeDef = PawnFaceMaker.RandomEyeDefFor(pawn, pawnFactionDef);
            this.BrowDef = PawnFaceMaker.RandomBrowDefFor(pawn, pawnFactionDef);

            this.WrinkleDef = PawnFaceMaker.AssignWrinkleDefFor(pawn);

            this.HasSameBeardColor = Rand.Value > 0.3f;

            this.GenerateHairDNA(pawn, false, newPawn);

            PawnFaceMaker.RandomBeardDefFor(face, pawnFactionDef, out this.beardDef, out this.moustacheDef);

            this.WrinkleIntensity = Mathf.InverseLerp(45f, 80f, pawn.ageTracker.AgeBiologicalYearsFloat);
            this.WrinkleIntensity -= pawn.story.melanin / 2;

            // this.MelaninOrg = pawn.story.melanin;
        }

        // public Baldness Baldness;
        public PawnFace()
        {
            // for RW to not bug out
        }

        public Color BeardColor
        {
            get => this.beardColor;
            set => this.beardColor = value;
        }

        public BeardDef BeardDef
        {
            get => this.beardDef;
            set => this.beardDef = value;
        }

        public BrowDef BrowDef
        {
            get => this.browDef;
            set => this.browDef = value;
        }

        public bool DrawMouth
        {
            get => this.drawMouth;
            set => this.drawMouth = value;
        }

        public float EuMelanin
        {
            get => this.euMelanin;
            set => this.euMelanin = value;
        }

        public EyeDef EyeDef
        {
            get => this.eyeDef;
            set => this.eyeDef = value;
        }

        public float Greyness
        {
            get => this.greyness;
            set => this.greyness = value;
        }

        public Color HairColor
        {
            get => this.hairColor;
            set => this.hairColor = value;
        }

        public bool HasSameBeardColor
        {
            get => this.hasSameBeardColor;
            set => this.hasSameBeardColor = value;
        }

        public MoustacheDef MoustacheDef
        {
            get => this.moustacheDef;
            set => this.moustacheDef = value;
        }

        public float PheoMelanin
        {
            get => this.pheoMelanin;
            set => this.pheoMelanin = value;
        }

        [NotNull]
        public WrinkleDef WrinkleDef
        {
            get => this.wrinkleDef;
            set => this.wrinkleDef = value;
        }

        public float WrinkleIntensity
        {
            get => this.wrinkleIntensity;
            set => this.wrinkleIntensity = value;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.eyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.browDef, "BrowDef");

            Scribe_Defs.Look(ref this.wrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.beardDef, "BeardDef");
            Scribe_Defs.Look(ref this.moustacheDef, "MoustacheDef");

            Scribe_Values.Look(ref this.drawMouth, "drawMouth");

            Scribe_Values.Look(ref this.beardColor, "BeardColor");
            Scribe_Values.Look(ref this.hasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this.euMelanin, "euMelanin");
            Scribe_Values.Look(ref this.pheoMelanin, "pheoMelanin");
            Scribe_Values.Look(ref this.greyness, "greyness");

            Scribe_Values.Look(ref this.hairColor, "hairColor");
            Scribe_Values.Look(ref this.wrinkleIntensity, "wrinkles");

            // Scribe_Values.Look(ref this.MelaninOrg, "melaninOrg");

            // Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            // Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            // Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            // Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            // Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");
        }

        public void GenerateHairDNA([NotNull] Pawn pawn, bool ignoreRelative = false, bool newPawn = true)
        {
            bool sameColor = Controller.settings.SameBeardColor;

            HairDNA hairDNA =
                HairMelanin.GenerateHairMelaninAndCuticula(pawn, this.HasSameBeardColor || sameColor, ignoreRelative);
            this.EuMelanin = hairDNA.HairColorRequest.EuMelanin;
            this.PheoMelanin = hairDNA.HairColorRequest.PheoMelanin;
            this.Greyness = hairDNA.HairColorRequest.Greyness;

            // this.Baldness = hairDNA.HairColorRequest.Baldness;
            if (newPawn)
            {
                this.HairColor = hairDNA.HairColor;
                this.BeardColor = hairDNA.BeardColor;
            }
            else
            {
                this.HairColor = pawn.story.hairColor;
                this.BeardColor = HairMelanin.DarkerBeardColor(this.HairColor);
            }
        }

        // public float MelaninOrg;
    }
}