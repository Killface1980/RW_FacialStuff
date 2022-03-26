using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialStuff
{
    class ___EXCLUDED
    {
                    {



            List<Material> bodyBaseAt = null;
        bool flag = true;
            if (!portrait && Controller.settings.HideShellWhileRoofed)
            {
                if (this.CompAnimator.InRoom && this.CompAnimator.HideShellLayer)
                {
            MaxLayerToShow layer;
            if (this.CompAnimator.InPrivateRoom)
            {
                layer = renderBody
                        ? Controller.settings.LayerInPrivateRoom
                        : Controller.settings.LayerInOwnedBed;
            }
            else
            {
                layer = renderBody ? Controller.settings.LayerInRoom : Controller.settings.LayerInBed;
            }

            bodyBaseAt = this.BodyBaseAt(this.Graphics, this.BodyFacing, bodyDrawType, layer);
            flag = false;
        }
        }

            if (flag)
            {
                bodyBaseAt = this.Graphics.MatsBodyBaseAt(this.BodyFacing, bodyDrawType);
        }

            foreach (Material material in bodyBaseAt)
            {
                Material damagedMat = this.Graphics.flasher.GetDamagedMat(material);
        GenDraw.DrawMeshNowOrLater(bodyMesh, bodyLoc, quat, damagedMat, portrait);
                bodyLoc.y += Offsets.YOffsetInterval_Clothes;
            }



}

    }
}
