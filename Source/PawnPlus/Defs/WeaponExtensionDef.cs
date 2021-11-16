namespace RimWorld
{
    using UnityEngine;

    using Verse;

    public class WeaponExtensionDef : Def
    {
        public string weapon;
        public float? attackAngleOffset;
        public Vector3 weaponPositionOffset;
        public Vector3 aimedWeaponPositionOffset;
        public Vector3 firstHandPosition;
        public Vector3 secondHandPosition;
    }
}
