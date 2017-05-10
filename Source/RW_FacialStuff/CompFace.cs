using System.Collections.Generic;
using RW_FacialStuff.Defs;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public class CompFace : ThingComp
    {
        public BeardDef BeardDef;
        public EyeDef EyeDef;
        public BrowDef BrowDef;
        public MouthDef MouthDef;
        public WrinkleDef WrinkleDef;
        public string headGraphicIndex;
        public string type;

        public string SkinColorHex;
        public Color HairColorOrg;

        public bool optimized;
        public bool sessionOptimized;
        public bool drawMouth = true;

        public void DefineFace(Pawn pawn)
        {

            if (pawn.story.HeadGraphicPath.Contains("Normal"))
                type = "Normal";

            if (pawn.story.HeadGraphicPath.Contains("Pointy"))
                type = "Pointy";

            if (pawn.story.HeadGraphicPath.Contains("Wide"))
                type = "Wide";


            EyeDef = PawnFaceChooser.RandomEyeDefFor(pawn, pawn.Faction.def);

            BrowDef = PawnFaceChooser.RandomBrowDefFor(pawn, pawn.Faction.def);

            WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn, pawn.Faction.def);

            MouthDef = PawnFaceChooser.RandomMouthDefFor(pawn, pawn.Faction.def);

            if (pawn.gender == Gender.Male)
            {
                BeardDef = PawnFaceChooser.RandomBeardDefFor(pawn, pawn.Faction.def);
            }

            HairColorOrg = pawn.story.hairColor;

            optimized = true;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref EyeDef, "EyeDef");
            Scribe_Defs.Look(ref BrowDef, "BrowDef");
            Scribe_Defs.Look(ref MouthDef, "MouthDef");
            Scribe_Defs.Look(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref BeardDef, "BeardDef");
            Scribe_Values.Look(ref optimized, "optimized");
            Scribe_Values.Look(ref drawMouth, "drawMouth");

            Scribe_Values.Look(ref headGraphicIndex, "headGraphicIndex");
            Scribe_Values.Look(ref type, "type");
            Scribe_Values.Look(ref SkinColorHex, "SkinColorHex");
            Scribe_Values.Look(ref HairColorOrg, "HairColorOrg");
        }
    }
}
