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
            Scribe_Defs.LookDef(ref EyeDef, "EyeDef");
            Scribe_Defs.LookDef(ref BrowDef, "BrowDef");
            Scribe_Defs.LookDef(ref MouthDef, "MouthDef");
            Scribe_Defs.LookDef(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.LookDef(ref BeardDef, "BeardDef");
            Scribe_Values.LookValue(ref optimized, "optimized");
            Scribe_Values.LookValue(ref drawMouth, "drawMouth");

            Scribe_Values.LookValue(ref headGraphicIndex, "headGraphicIndex");
            Scribe_Values.LookValue(ref type, "type");
            Scribe_Values.LookValue(ref SkinColorHex, "SkinColorHex");
            Scribe_Values.LookValue(ref HairColorOrg, "HairColorOrg");
        }
    }
}
