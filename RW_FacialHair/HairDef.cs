using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RW_FacialHair
{
	public class BeardDef : Def
	{

        public string texPath;

        public HairGender hairGender = HairGender.Any;

        public List<string> beardTags = new List<string>();

    }

    //     if (pawn.beard.Equals(null))
    //     pawn.beard.beardDef = PawnBeardChooser.RandomBeardDefFor(pawn, pawn.Faction.def);

    //       beardGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.beard.beardDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);

}
