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

        public static bool useMouth = true;

        public override void ExposeData()
        {
          Scribe_Values.LookValue(ref useMouth, "useMouth", false, false);
//          Scribe_Values.LookValue(ref Settings.useCustomBaseSpacingHorizontal, "useCustomBaseSpacingHorizontal", false, false);
//          Scribe_Values.LookValue(ref Settings.useCustomBaseSpacingVertical, "useCustomBaseSpacingVertical", false, false);
//          Scribe_Values.LookValue(ref Settings.useCustomIconSize, "useCustomIconSize", false, false);
//          Scribe_Values.LookValue(ref Settings.useCustomPawnTextureCameraVerticalOffset, "useCustomPawnTextureCameraVerticalOffset", false, false);
//          Scribe_Values.LookValue(ref Settings.useCustomPawnTextureCameraZoom, "useCustomPawnTextureCameraZoom", false, false);
//          Scribe_Values.LookValue(ref Settings.useCustomMaxColonistBarWidth, "useCustomMaxColonistBarWidth", false, false);
//          Scribe_Values.LookValue(ref Settings.useCustomDoubleClickTime, "useCustomDoubleClick", false, false);
//          Scribe_Values.LookValue(ref Settings.useGender, "useGender", false, false);
//          //        Scribe_Values.LookValue(ref Settings.useExtraIcons, "useExtraIcons", false, false);
//
//          Scribe_Values.LookValue(ref Settings.MarginTop, "MarginTop", 21f, false);
//          Scribe_Values.LookValue(ref Settings.BaseSpacingHorizontal, "BaseSpacingHorizontal", 24f, false);
//          Scribe_Values.LookValue(ref Settings.BaseSpacingVertical, "BaseSpacingVertical", 32f, false);
//          Scribe_Values.LookValue(ref Settings.BaseSizeFloat, "BaseSizeFloat", 48f, false);
//          Scribe_Values.LookValue(ref Settings.BaseIconSize, "BaseIconSize", 20f, false);
//          Scribe_Values.LookValue(ref Settings.PawnTextureCameraVerticalOffset, "PawnTextureCameraVerticalOffset", 0.3f, false);
//          Scribe_Values.LookValue(ref Settings.PawnTextureCameraZoom, "PawnTextureCameraZoom", 1.28205f, false);
//          Scribe_Values.LookValue(ref Settings.MaxColonistBarWidth, "MaxColonistBarWidth");
//
//          Scribe_Values.LookValue(ref Settings.DoubleClickTime, "DoubleClickTime", 0.5f, false);
//
//          Scribe_Values.LookValue(ref Settings.FemaleColor, "FemaleColor");
//          Scribe_Values.LookValue(ref Settings.MaleColor, "MaleColor");
//
//
//          Settings.reloadsettings = false;
          if (Scribe.mode == LoadSaveMode.PostLoadInit)
          {
        //      maleColorField.Value = Settings.MaleColor;
          }
        }

        #endregion

    }
}