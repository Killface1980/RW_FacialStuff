namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Genetics;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnFace : IExposable
    {
        public Color BeardColor;

        [NotNull]
        public BeardDef BeardDef;

        [NotNull]
        public BrowDef BrowDef;

        public CrownType CrownType;

        public float Cuticula;

        public bool DrawMouth;

        public float EuMelanin;

        [NotNull]
        public EyeDef EyeDef;

        public float Greyness;

        public Color HairColor;

        public bool HasSameBeardColor;

        [CanBeNull]
        public MoustacheDef MoustacheDef;

        public float PheoMelanin;

        [NotNull]
        public WrinkleDef WrinkleDef;

        public PawnFace([NotNull] Pawn pawn, bool setColors = true)
        {
            this.DrawMouth = true;
            FactionDef faction = pawn.Faction.def;

            this.EyeDef = PawnFaceChooser.RandomEyeDefFor(pawn, faction);
            this.BrowDef = PawnFaceChooser.RandomBrowDefFor(pawn, faction);

            this.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn);
            PawnFaceChooser.RandomBeardDefFor(pawn, faction, out this.BeardDef, out this.MoustacheDef);
            this.HasSameBeardColor = Rand.Value > 0.3f;
            HairDNA hairDNA = HairMelanin.GenerateHairMelaninAndCuticula(pawn, this.HasSameBeardColor);
            this.EuMelanin = hairDNA.HairColorRequest.EuMelanin;
            this.PheoMelanin = hairDNA.HairColorRequest.PheoMelanin;
            this.Cuticula = hairDNA.HairColorRequest.Cuticula;
            this.Greyness = hairDNA.HairColorRequest.Greyness;

            this.HairColor = hairDNA.HairColor;
            this.BeardColor = hairDNA.BeardColor;

            this.CrownType = pawn.story.crownType;

            if (setColors)
            {
                pawn.story.hairColor = this.HairColor;
            }

            // this.MelaninOrg = pawn.story.melanin;
        }

        public PawnFace()
        {
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");

            Scribe_Defs.Look(ref this.WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
            Scribe_Defs.Look(ref this.MoustacheDef, "MoustacheDef");

            Scribe_Values.Look(ref this.DrawMouth, "drawMouth");

            Scribe_Values.Look(ref this.CrownType, "crownType");
            Scribe_Values.Look(ref this.BeardColor, "BeardColor");
            Scribe_Values.Look(ref this.HasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this.EuMelanin, "euMelanin");
            Scribe_Values.Look(ref this.PheoMelanin, "pheoMelanin");
            Scribe_Values.Look(ref this.Cuticula, "cuticula");

            Scribe_Values.Look(ref this.HairColor, "hairColor");

            // Scribe_Values.Look(ref this.MelaninOrg, "melaninOrg");

            // Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            // Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            // Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            // Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            // Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");
        }

        // public float MelaninOrg;
    }
}