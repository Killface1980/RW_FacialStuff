
using UnityEngine;
using Verse;

namespace RW_FacialStuff       // Replace with yours.
{       // This code is mostly borrowed from Pawn State Icons mod by Dan Sadler, which has open source and no license I could find, so...
    [StaticConstructorOnStartup]
    public class MapComponentInjectorBehavior : MonoBehaviour
    {
        public static readonly string mapComponentName = "RW_FacialStuff.MapComponent_FacialStuff";       // Ditto.
        private static readonly MapComponent_FacialStuff mapComponent = new MapComponent_FacialStuff();       // Ditto.

        #region No editing required
        protected bool reinjectNeeded = false;
        protected float reinjectTime = 0;

        public void OnLevelWasLoaded(int level)
        {
            reinjectNeeded = true;
            if (level >= 0)
            {
                reinjectTime = 1;
            }
            else
            {
                reinjectTime = 0;
            }
        }

        public void FixedUpdate()
        {
            if (Current.ProgramState != ProgramState.MapPlaying)
            {
                return;
            }

            if (reinjectNeeded)
            {
                Log.Message("FacialStuffInjector - needs reinject");
                reinjectTime -= Time.fixedDeltaTime;
                if (reinjectTime <= 0)
                {
                    reinjectNeeded = false;
                    reinjectTime = 0;
                    if (Find.Map != null && Find.Map.components != null)
                    {
                        if (Find.Map.components.FindAll(x => x.GetType().ToString() == mapComponentName).Count != 0)
                        {
                            Log.Message("MapComponentInjector: map already has " + mapComponentName + ".");
                            //Destroy(gameObject);
                        }
                        else
                        {
                            Log.Message("MapComponentInjector: adding " + mapComponentName + "...");
                            Find.Map.components.Add(mapComponent);
                            Log.Message("MapComponentInjector: success!");
                            //Destroy(gameObject);
                        }
                    }
                }
            }
        }

        public void Start()
        {
            OnLevelWasLoaded(-1);
        }
    }

    class MapComponentInjector : ITab
    {
        protected GameObject initializer;

        public MapComponentInjector()
        {
            Log.Message("MapComponentInjector: initializing for " + MapComponentInjectorBehavior.mapComponentName);
            initializer = new GameObject("FSMapComponentInjector");
            initializer.AddComponent<MapComponentInjectorBehavior>();
            Object.DontDestroyOnLoad((Object)initializer);
        }

        protected override void FillTab()
        {

        }
    }
}
#endregion