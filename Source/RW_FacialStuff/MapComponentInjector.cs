
using System;
using UnityEngine;
using Verse;

namespace RW_FacialStuff       // Replace with yours.
{
    using Object = UnityEngine.Object;

    // This code is mostly borrowed from Pawn State Icons mod by Dan Sadler, which has open source and no license I could find, so...
    public class MapComponentInjector : MonoBehaviour
    {
        private static Type facialStuff = typeof(MapComponent_FacialStuff);

        public void FixedUpdate()
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }



            if (Find.VisibleMap.components.FindAll(c => c.GetType() == facialStuff).Count == 0)
            {
                Find.VisibleMap.components.Add((MapComponent)Activator.CreateInstance(facialStuff));

                Log.Message("Facial Stuff :: Added an FS to the map.");
            }

            Destroy(this);
        }

        private void Start()
        {
            GameObject initializer = new GameObject("FSMapComponentInjector");
            initializer.AddComponent<MapComponentInjector>();
            Object.DontDestroyOnLoad(initializer);
        }
    }


}