using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FacialStuff.Defs
{
    public class EyeDef : Def
    {
        public string texCollection;

        public HairGender hairGender;

        public string texBasePath;

        public string texName;

        public EyeDef closedEyeDef;

        public EyeDef missingEyeDef;

        public List<string> hairTags = new List<string>();
    }
}