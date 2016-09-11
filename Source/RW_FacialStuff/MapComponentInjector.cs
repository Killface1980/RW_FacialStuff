
using System;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace RW_FacialStuff       // Replace with yours.
{       // This code is mostly borrowed from Pawn State Icons mod by Dan Sadler, which has open source and no license I could find, so...
    public class MapComponentInjector : MonoBehaviour
    {
        private static Type facialStuff = typeof(MapComponent_FacialStuff);

        public void FixedUpdate()
        {
            if (Current.ProgramState != ProgramState.MapPlaying)
            {
                return;
            }

            if (Find.Map.components.FindAll(c => c.GetType() == facialStuff).Count == 0)
            {
                Find.Map.components.Add((MapComponent)Activator.CreateInstance(facialStuff));

                Log.Message("Facial Stuff :: Added an FS to the map.");
            }

            Destroy(this);
        }
    }


}