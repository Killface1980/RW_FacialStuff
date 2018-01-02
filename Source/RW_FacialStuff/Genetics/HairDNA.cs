using UnityEngine;

namespace FacialStuff.Genetics
{
    public struct HairDna
    {
        public HairColorRequest HairColorRequest { get; set; }

        public Color HairColor { get; set; }

        public Color BeardColor { get; set; }
    }
}