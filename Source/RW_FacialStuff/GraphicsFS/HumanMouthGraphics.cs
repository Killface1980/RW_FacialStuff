using System.Collections.Generic;
using FacialStuff.DefOfs;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    [StaticConstructorOnStartup]
    public class HumanMouthGraphics
    {
        public List<MouthGraphicData> HumanMouthGraphic;

        public Graphic_Multi_NaturalHeadParts MouthGraphicCrying;

        public HumanMouthGraphics([NotNull] Pawn p)
        {
            Color color = Color.white;
            Graphic_Multi_NaturalHeadParts mouthGraphic01 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood01.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic02 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood02.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic03 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood03.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic04 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood04.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic05 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood05.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            Graphic_Multi_NaturalHeadParts mouthGraphic06 =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Mood06.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            color = Color.Lerp(Color.white, new Color(0.96f, 0.89f, 0.75f), Rand.Value);

            Graphic_Multi_NaturalHeadParts mouthGraphicGrin =
                GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                    MouthDefOf.Mouth_Grin.texPath,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color) as Graphic_Multi_NaturalHeadParts;

            this.MouthGraphicCrying = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                          MouthDefOf.Mouth_Crying.texPath,
                                          ShaderDatabase.CutoutSkin,
                                          Vector2.one,
                                          color) as Graphic_Multi_NaturalHeadParts;

            bool flag = Controller.settings.UseNastyGrin;

            if (p.mindState?.mentalBreaker != null)
            {
                float minor = p.mindState.mentalBreaker.BreakThresholdMinor;
                float major = p.mindState.mentalBreaker.BreakThresholdMajor;
                float extreme = p.mindState.mentalBreaker.BreakThresholdExtreme;
                float part = (1f - minor) / (flag ? 5 : 4);

                this.HumanMouthGraphic = new List<MouthGraphicData>
                                             {
                                                 new MouthGraphicData(0f, mouthGraphic06),
                                                 new MouthGraphicData(extreme, mouthGraphic05),
                                                 new MouthGraphicData(major, mouthGraphic04),
                                                 new MouthGraphicData(minor, mouthGraphic03),
                                                 new MouthGraphicData(minor + part, mouthGraphic02),
                                                 new MouthGraphicData(minor + 2 * part, mouthGraphic01)
                                             };
                if (flag)
                {

                    this.HumanMouthGraphic.Add(new MouthGraphicData(minor + 4 * part, mouthGraphicGrin));
                }
            }
            else
            {
                this.HumanMouthGraphic = new List<MouthGraphicData>
                                             {
                                                 new MouthGraphicData(0f, mouthGraphic06),
                                                 new MouthGraphicData(0.25f, mouthGraphic05),
                                                 new MouthGraphicData(0.4f, mouthGraphic04),
                                                 new MouthGraphicData(0.55f, mouthGraphic03),
                                                 new MouthGraphicData(0.7f, mouthGraphic02),
                                                 new MouthGraphicData(flag ? 0.8f : 0.85f, mouthGraphic01)
                                             };
                if (flag)
                {

                    this.HumanMouthGraphic.Add(new MouthGraphicData(0.95f, mouthGraphicGrin));
                }
            }
        }

        public int GetMouthTextureIndexOfMood(float mood)
        {
            int result = 0;
            for (int i = 0; i < this.HumanMouthGraphic.Count; i++)
            {
                if (mood < this.HumanMouthGraphic[i].Mood)
                {
                    break;
                }

                result = i;
            }

            return result;
        }

        public struct MouthGraphicData
        {
#region Public Fields

            public readonly Graphic_Multi_NaturalHeadParts Graphic;

            public readonly float Mood;

#endregion Public Fields

#region Public Constructors

            public MouthGraphicData(float mood, Graphic_Multi_NaturalHeadParts graphic)
            {
                this.Mood = mood;
                this.Graphic = graphic;
            }

#endregion Public Constructors
        }
    }
}