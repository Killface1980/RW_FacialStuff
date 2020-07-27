using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
    public class WeaponExtensionDef : Def
    {
        public List<DefHyperlink> descriptionHyperlinks = new List<DefHyperlink>();
        public string weapon;

        public float? attackAngleOffset;
        public Vector3 weaponPositionOffset;
        public Vector3 aimedWeaponPositionOffset;
        public Vector3 firstHandPosition;
        public Vector3 secondHandPosition;



    }
}
