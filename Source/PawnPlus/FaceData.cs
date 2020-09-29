using System.Diagnostics.CodeAnalysis;
using PawnPlus.DefOfs;
using PawnPlus.Defs;
using PawnPlus.Genetics;
using PawnPlus.Utilities;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnPlus
{
    public class FaceData : IExposable
    {
        #region Private Fields

        private BeardDef _beardDef;
        private BrowDef _browDef;
        private PartDef _eyeDef;
        private MoustacheDef _moustacheDef;
        private WrinkleDef _wrinkleDef;
        private Color _hairColor;
        private Color _beardColor;
        private float _euMelanin;
        private float _pheoMelanin;
        private float _greyness;
        private bool _hasSameBeardColor;
        private float _wrinkleIntensity;

        #endregion Private Fields
        
        #region Public Constructors

        public FaceData(CompFace face, FactionDef pawnFactionDef, bool newPawn = true)
        {
            Pawn pawn = face.Pawn;
            if (pawnFactionDef == null)
            {
                pawnFactionDef = FactionDefOf.PlayerColony;
            }
            
            BrowDef = PawnFaceMaker.RandomBrowDefFor(pawn, pawnFactionDef);
            WrinkleDef = PawnFaceMaker.AssignWrinkleDefFor(pawn);
            PawnFaceMaker.RandomBeardDefFor(face, pawnFactionDef, out _beardDef, out _moustacheDef);
            
            HasSameBeardColor = Rand.Value > 0.3f;
            GenerateHairDNA(pawn, false, newPawn);
            WrinkleIntensity = Mathf.InverseLerp(45f, 80f, pawn.ageTracker.AgeBiologicalYearsFloat) - pawn.story.melanin / 2;
        }

        public FaceData()
        {
            
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
        
        public float EuMelanin
        {
            get => this._euMelanin;
            set => this._euMelanin = value;
        }

        public PartDef EyeDef
        {
            get => this._eyeDef;
            set => this._eyeDef = value;
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
            Scribe_Defs.Look(ref _eyeDef, "EyeDef");
            Scribe_Defs.Look(ref _browDef, "BrowDef");
            Scribe_Defs.Look(ref _wrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref _beardDef, "BeardDef");
            Scribe_Defs.Look(ref _moustacheDef, "MoustacheDef");
            
            Scribe_Values.Look(ref _beardColor, "BeardColor");
            Scribe_Values.Look(ref _hairColor, "hairColor");
            Scribe_Values.Look(ref _hasSameBeardColor, "sameBeardColor");
            Scribe_Values.Look(ref _euMelanin, "euMelanin");
            Scribe_Values.Look(ref _pheoMelanin, "pheoMelanin");
            Scribe_Values.Look(ref _greyness, "greyness");
            Scribe_Values.Look(ref _wrinkleIntensity, "wrinkles");
        }

        public void GenerateHairDNA(Pawn pawn, bool ignoreRelative = false, bool newPawn = true)
        {
            HairDNA hairDna =
                HairMelaninUtil.GenerateHairMelaninAndCuticula(pawn, HasSameBeardColor, ignoreRelative);
            EuMelanin = hairDna.HairColorRequest.EuMelanin;
            PheoMelanin = hairDna.HairColorRequest.PheoMelanin;
            Greyness = hairDna.HairColorRequest.Greyness;

            // this.Baldness = hairDNA.HairColorRequest.Baldness;
            if (newPawn)
            {
                HairColor = hairDna.HairColor;
                BeardColor = hairDna.BeardColor;
            }
            else
            {
                HairColor = pawn.story.hairColor;
                BeardColor = HairMelaninUtil.ShuffledBeardColor(HairColor);
            }

        }

        #endregion Public Methods
    }
}