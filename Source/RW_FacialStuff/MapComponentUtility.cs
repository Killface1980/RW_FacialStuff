using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RW_FacialStuff
{
    public static class MapComponentUtility
    {
        /**
* Injects a map component into the current map if it does not already exist. 
* Required for new MapComponents that were not active at map creation.
* The injection is performed at ExecuteWhenFinished to allow calling this method in MapComponent constructors.
*/
        public static void EnsureIsActive(this MapComponent mapComponent)
        {
            if (mapComponent == null) throw new Exception("MapComponent is null");
            LongEventHandler.ExecuteWhenFinished(() => {
                if (mapComponent.map == null || mapComponent.map.components == null) throw new Exception("The map component requires a loaded map to be made active.");
                var components = mapComponent.map.components;
                if (components.Any(c => c == mapComponent)) return;
                components.Add(mapComponent);
            });
        }
    }
}
