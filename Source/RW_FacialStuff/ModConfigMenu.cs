#if !NoCCL

using CommunityCoreLibrary;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public class ModConfigMenu : ModConfigurationMenu
    {
        #region Fields

        public string Page = "main";
        public Window OptionsDialog;

        #endregion

        #region Methods

        public override float DoWindowContents(Rect rect)
        {
            float curY = 0f;

            rect.xMin += 15f;
            rect.width -= 15f;

            var listing = new Listing_Standard(rect);
            {
                FillPageMain(listing, rect.width, ref curY);
            }

            return 680f;
            //return curY;
        }
        
        private void FillPageMain(Listing_Standard listing, float columnwidth, ref float curY)
        {
        //  listing.ColumnWidth = columnwidth / 2;
        //
        //  if (listing.ButtonText("RW_FacialStuff.Settings.RevertSettings".Translate()))
        //  {
        //  }
        //  listing.ColumnWidth = columnwidth;
            listing.CheckboxLabeled("RW_FacialStuff.Settings.useMouth".Translate(), ref useMouth, null);
            listing.Gap();

            listing.End();
            curY += listing.CurHeight;

        }

        public static bool useMouth = false;

        public override void ExposeData()
        {
          Scribe_Values.LookValue(ref useMouth, "useMouth", false, false);
        }

        #endregion

    }
}
#endif