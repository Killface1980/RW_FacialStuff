using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FacialStuff.Harmony;
using UnityEngine;
using Verse;

namespace FacialStuff.Utilities
{
    class Class1
    {
        public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Mesh  mesh = null;
            float num = aimAngle - 90f;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh =  MeshPool.plane10;
                num  += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh =  MeshPool.plane10Flip;
                num  -= 180f;
                num  -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh =  MeshPool.plane10;
                num  += eq.def.equippedAngleOffset;
            }
            num                                   %= 360f;
            Material           matSingle = null;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }

            Vector3 position = drawLoc;
            HarmonyPatchesFS.DoCalculations(eq, ref position, ref num, 1);

            Graphics.DrawMesh(mesh, position, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
        }

    }
}
