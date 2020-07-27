using System.Diagnostics.CodeAnalysis;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.Genetics;
using FacialStuff.Utilities;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class PawnFace : IExposable
    {
        #region Private Fields

        private Color _beardColor;

        [CanBeNull]
        private BeardDef _beardDef;

        [CanBeNull]
        private BrowDef _browDef;

        // public float Cuticula;
        private bool _drawMouth = true;

        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private float _euMelanin;

        [CanBeNull]
        private EyeDef _eyeDef;
        private EarDef _earDef;

        private float _greyness;

        private Color _hairColor;

        private bool _hasSameBeardColor;

        private MoustacheDef _moustacheDef;

        private float _pheoMelanin;

        private WrinkleDef _wrinkleDef;

        private float _wrinkleIntensity;

        #endregion Private Fields

        #region Public Constructors

        public PawnFace([NotNull] CompFace face, FactionDef pawnFactionDef, bool newPawn = true)
        {
            Pawn pawn = face.Pawn;
            this.DrawMouth = true;
            if (pawnFactionDef == null)
            {
                pawnFactionDef = FactionDefOf.PlayerColony;
            }

            this.EyeDef = PawnFaceMaker.RandomEyeDefFor(pawn, pawnFactionDef);
            this.EarDef = PawnFaceMaker.RandomEarDefFor(pawn, pawnFactionDef);
            this.BrowDef = PawnFaceMaker.RandomBrowDefFor(pawn, pawnFactionDef);

            this.WrinkleDef = PawnFaceMaker.AssignWrinkleDefFor(pawn);

            this.HasSameBeardColor = Rand.Value > 0.3f;

            this.GenerateHairDNA(pawn, false, newPawn);

            PawnFaceMaker.RandomBeardDefFor(face, pawnFactionDef, out this._beardDef, out this._moustacheDef);

            this.WrinkleIntensity = Mathf.InverseLerp(45f, 80f, pawn.ageTracker.AgeBiologicalYearsFloat);
            this.WrinkleIntensity -= pawn.story.melanin / 2;

            // this.MelaninOrg = pawn.story.melanin;
        }

        // public Baldness Baldness;
        public PawnFace()
        {
            // for RW to not bug out
        }

        #endregion Public Constructors

        #region Public Properties

        public Color BeardColor
        {
            get => this._beardColor;
            set => this._beardColor = value;
        }

        public BeardDef BeardDef
        {
            get => this._beardDef;
            set => this._beardDef = value;
        }

        public BrowDef BrowDef
        {
            get => this._browDef;
            set => this._browDef = value;
        }

        public bool DrawMouth
        {
            get => this._drawMouth;
            set => this._drawMouth = value;
        }

        public float EuMelanin
        {
            get => this._euMelanin;
            set => this._euMelanin = value;
        }

        public EyeDef EyeDef
        {
            get => this._eyeDef;
            set => this._eyeDef = value;
        }
        public EarDef EarDef
        {
            get => this._earDef == null? EarDefOf.Ear_Default : this._earDef;
            set => this._earDef = value;
        }

        public float Greyness
        {
            get => this._greyness;
            set => this._greyness = value;
        }

        public Color HairColor
        {
            get => this._hairColor;
            set => this._hairColor = value;
        }

        public bool HasSameBeardColor
        {
            get => this._hasSameBeardColor;
            set => this._hasSameBeardColor = value;
        }

        public MoustacheDef MoustacheDef
        {
            get => this._moustacheDef;
            set => this._moustacheDef = value;
        }

        public float PheoMelanin
        {
            get => this._pheoMelanin;
            set => this._pheoMelanin = value;
        }

        [NotNull]
        public WrinkleDef WrinkleDef
        {
            get => this._wrinkleDef;
            set => this._wrinkleDef = value;
        }

        public float WrinkleIntensity
        {
            get => this._wrinkleIntensity;
            set => this._wrinkleIntensity = value;
        }

        #endregion Public Properties

        #region Public Methods

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this._eyeDef, "EyeDef");
            Scribe_Defs.Look(ref this._browDef, "BrowDef");

            Scribe_Defs.Look(ref this._wrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this._beardDef, "BeardDef");
            Scribe_Defs.Look(ref this._moustacheDef, "MoustacheDef");

            Scribe_Values.Look(ref this._drawMouth, "drawMouth");

            Scribe_Values.Look(ref this._beardColor, "BeardColor");
            Scribe_Values.Look(ref this._hasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this._euMelanin, "euMelanin");
            Scribe_Values.Look(ref this._pheoMelanin, "pheoMelanin");
            Scribe_Values.Look(ref this._greyness, "greyness");

            Scribe_Values.Look(ref this._hairColor, "hairColor");
            Scribe_Values.Look(ref this._wrinkleIntensity, "wrinkles");

            // Scribe_Values.Look(ref this.MelaninOrg, "melaninOrg");

            // Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            // Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            // Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            // Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            // Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");
        }

        public void GenerateHairDNA([NotNull] Pawn pawn, bool ignoreRelative = false, bool newPawn = true)
        {
            HairDNA hairDna =
                HairMelanin.GenerateHairMelaninAndCuticula(pawn, this.HasSameBeardColor, ignoreRelative);
            this.EuMelanin = hairDna.HairColorRequest.EuMelanin;
            this.PheoMelanin = hairDna.HairColorRequest.PheoMelanin;
            this.Greyness = hairDna.HairColorRequest.Greyness;

            // this.Baldness = hairDNA.HairColorRequest.Baldness;
            if (newPawn)
            {
                this.HairColor = hairDna.HairColor;
                this.BeardColor = hairDna.BeardColor;
            }
            else
            {
                this.HairColor = pawn.story.hairColor;
                this.BeardColor = HairMelanin.ShuffledBeardColor(this.HairColor);
            }

        }

        #endregion Public Methods

        // public float MelaninOrg;
    }
}