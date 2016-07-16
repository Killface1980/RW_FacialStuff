using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RW_FacialStuff
{

    public class MapComponent_FacialStuff : MapComponent
    {


        public static MapComponent_FacialStuff Get
        {
            get
            {
                MapComponent_FacialStuff getComponent = Find.Map.components.OfType<MapComponent_FacialStuff>().FirstOrDefault();
                if (getComponent == null)
                {
                    getComponent = new MapComponent_FacialStuff();
                    Find.Map.components.Add(getComponent);
                }

                return getComponent;
            }
        }


    }
}
