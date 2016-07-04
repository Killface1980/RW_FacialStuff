using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public static class PawnHairColors
    {
        public static Color _ColorMidnightBlack = new Color32(10, 10, 10, 255);//(0.2f, 0.2f, 0.2f);
        public static Color _ColorDarkBrown = new Color32(64, 41, 19, 255);//(0.25f, 0.2f, 0.15f);

        public static Color _ColorMediumDarkBrown = new Color32(110, 70, 10, 255);//(0.25f, 0.2f, 0.15f);

        public static Color _ColorTerraCotta = new Color32(200, 50, 0, 255);//(0,3529412f, 0,227450982f, 0,1254902f);
        public static Color _ColorYellowBlonde = new Color32(255, 203, 89, 255);//(0,75686276f, 0,572549045f, 0,333333343f);

        public static Color _ColorPlatinum = new Color32(255, 245, 226, 255);//(0,929411769f, 0,7921569f, 0,6117647f);

        //      public static Color Dark01 = new Color32(50, 50, 50, 255);//(0.2f, 0.2f, 0.2f);
        //      public static Color Dark02 = new Color32(79, 71, 66, 255);//(0.31f, 0.28f, 0.26f);
        //      public static Color Dark03 = new Color32(64, 50, 37, 255);//(0.25f, 0.2f, 0.15f);
        //      public static Color Dark04 = new Color32(75, 50, 25, 255);//(0.3f, 0.2f, 0.1f);
        //
        //      public static Color Color01 = new Color32(90, 58, 32, 255);//(0,3529412f, 0,227450982f, 0,1254902f);
        //      public static Color Color02 = new Color32(132, 83, 47, 255);//(0,5176471f, 0,3254902f, 0,184313729f);
        //      public static Color Color03 = new Color32(193, 146, 85, 255);//(0,75686276f, 0,572549045f, 0,333333343f);
        //      public static Color Color04 = new Color32(237, 202, 156, 255);//(0,929411769f, 0,7921569f, 0,6117647f);


        public static Color RandomHairColor(Color skinColor, int ageYears)
        {

            Color chosenColor;
            Color tempColor = skinColor;

          //if (Rand.Value < 0.02f)
          //{
          //    return new Color(Rand.Value, Rand.Value, Rand.Value);
          //}


            //if (ageYears > 40)
            //{
            //    float num = GenMath.SmootherStep(40f, 75f, (float)ageYears);
            //    if (Rand.Value < num)
            //    {
            //        float num2 = Rand.Range(0.65f, 0.85f);
            //        return new Color(num2, num2, num2);
            //    }
            //}
            if (PawnSkinColors.IsDarkSkin(skinColor))// || Rand.Value < 0.4f)
            {
                tempColor = Color.Lerp(_ColorMidnightBlack, _ColorDarkBrown, Rand.Range(0f, 0.75f));




                //else if (value < 0.5f)
                //    tempColor = Dark02;

                //else tempColor = Dark04;

                tempColor = Color.Lerp(tempColor, _ColorTerraCotta, Rand.Range(0f, 0.25f));
                tempColor = Color.Lerp(tempColor, _ColorPlatinum, Rand.Range(0f, 0.25f));
            }
            else
            {
                float value2 = Rand.Value;

                //dark hair
                if (value2 < 0.35f)
                {
                    tempColor = Color.Lerp(_ColorPlatinum, _ColorYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, _ColorTerraCotta, Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, _ColorMediumDarkBrown, Rand.Range(0.3f, 7f));
                    tempColor = Color.Lerp(tempColor, _ColorMidnightBlack, Rand.Range(0.3f, 1f));
                }
                //brown hair
                else if (value2 < 0.6f)
                {
                    tempColor = Color.Lerp(_ColorPlatinum, _ColorYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, _ColorTerraCotta, Rand.Range(0f, 0.6f));
                    tempColor = Color.Lerp(tempColor, _ColorMediumDarkBrown, Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, _ColorMidnightBlack, Rand.Range(0f, 0.7f));
                }
                //dirty blonde hair
                else  if (value2 < 0.75f)
                {
                    tempColor = Color.Lerp(_ColorPlatinum, _ColorYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, _ColorMediumDarkBrown, Rand.Range(0f, 0.5f));
                }
                // dark red/brown hair
                else if (value2 < 0.85f)
                {
                    tempColor = Color.Lerp(_ColorPlatinum, _ColorTerraCotta, Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, _ColorMidnightBlack, Rand.Range(0f, 0.7f));
                }
                // pure blond / albino
                else if (value2 < 0.95f)
                    tempColor = Color.Lerp(_ColorPlatinum, _ColorYellowBlonde, Rand.Range(0f, 0.5f));

                // red hair
                else 
                {
                    tempColor = Color.Lerp(_ColorYellowBlonde, _ColorTerraCotta, Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, _ColorMidnightBlack, Rand.Range(0f, 0.35f));
                }

                //  else if (value2 < 0.75f)
                //  {
                //  }
                //  else // if (value2 < 0.8f)
                //  {
                //  tempColor = Color.Lerp(_ColorPlatin, _ColorYellowBlonde, Rand.Value);
                //  tempColor = Color.Lerp(tempColor, _ColorTerraCotta, Rand.Value);
                //  tempColor = Color.Lerp(tempColor, _ColorMediumDarkBrown, Rand.Value);
                //  tempColor = Color.Lerp(tempColor, _ColorMidnightBlack, Rand.Value);
                //  }

                //           tempColor = ColorWhiteBlonde;
                //
                //       else if (value2 < 0.8f)
                //           tempColor = Color04;
                //
                //       else tempColor = ColPlatin;


            }

            // age to become gray as float
            float agingGreyFloat = Rand.Range(0.35f, 0.6f);

            float greyness = 0f;

            if ((float)ageYears / 100 > agingGreyFloat)
            { 
                greyness = ((float)ageYears / 100 - agingGreyFloat)*5f;
                
              if (greyness > 0.85f)
              {
                  greyness = 0.85f;
              }

                if (PawnSkinColors.IsDarkSkin(skinColor))
                    greyness *= Rand.Range(0.4f, 0.8f);
            }

            chosenColor = Color.Lerp(tempColor, new Color32(245, 245, 245, 255), greyness);
            return chosenColor;
            //  else if (value3 < 0.6f)



        }
    }
}
