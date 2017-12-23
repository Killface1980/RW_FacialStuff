using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Graphics
{
    using FacialStuff.Components;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    public class PawnBodyGraphic
    {
        [CanBeNull]
        public Graphic HandGraphicLeft;
        public Graphic HandGraphicRight;
        public Graphic FootGraphicLeft;
        public Graphic FootGraphicRight;

        public Graphic FootGraphicRightCol;

        public Graphic FootGraphicLeftCol;

        public Graphic HandGraphicLeftCol;

        public Graphic HandGraphicRightCol;

        public CompBodyAnimator compAni;

        private Pawn pawn;

        public PawnBodyGraphic(CompBodyAnimator compAni)
        {
            this.compAni = compAni;
            this.pawn = compAni.Pawn;

            LongEventHandler.ExecuteWhenFinished(
                () =>
                    {
                        this.InitializeGraphicsHand();
                        this.InitializeGraphicsFeet();
                    });

        }

        private void InitializeGraphicsHand()
        {
            if (!this.compAni.Props.bipedWithHands)
            {
                return;
            }
            string texNameHand = "Hands/" + this.compAni.Props.handType + "_Hand";

            Color skinColor = this.pawn.story.SkinColor;

            Color rightColorHand = Color.cyan;
            Color leftColorHand = Color.magenta;

            Color rightHandColor;
            Color leftHandColor;

            Color metal = new Color(0.51f, 0.61f, 0.66f);

            switch (this.compAni.bodyStat.handRight)
            {
                case PartStatus.Artificial:
                    rightHandColor = metal;
                    break;
                default:
                    rightHandColor = skinColor;
                    break;
            }

            switch (this.compAni.bodyStat.handLeft)
            {
                case PartStatus.Artificial:
                    leftHandColor = metal;
                    break;
                default:
                    leftHandColor = skinColor;
                    break;
            }


            this.HandGraphicRight = GraphicDatabase.Get<Graphic_Single>(
                texNameHand,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                rightHandColor,
                skinColor);

            this.HandGraphicLeft = GraphicDatabase.Get<Graphic_Single>(
                texNameHand,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                leftHandColor,
                skinColor);

            // for development
            this.HandGraphicRightCol = GraphicDatabase.Get<Graphic_Single>(
                texNameHand,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                rightColorHand,
                skinColor);

            this.HandGraphicLeftCol = GraphicDatabase.Get<Graphic_Single>(
                texNameHand,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                leftColorHand,
                skinColor);
        }

        private void InitializeGraphicsFeet()
        {

            string texNameFoot = "Hands/" + this.compAni.Props.handType + "_Foot";

            Color skinColor;
            if (this.pawn.RaceProps.Animal)
            {
                skinColor = Color.white;
            }
            else
            {
                skinColor = this.pawn.story.SkinColor;
            }

            Color rightColorFoot = Color.red;
            Color leftColorFoot = Color.blue;

            Color rightFootColor = skinColor;
            Color leftFootColor =skinColor;
            Color metal = new Color(0.51f, 0.61f, 0.66f);

            switch (this.compAni.bodyStat.footRight)
            {
                case PartStatus.Artificial:
                    rightFootColor = metal;
                    break;
            }

            switch (this.compAni.bodyStat.footLeft)
            {
                case PartStatus.Artificial:
                    leftFootColor = metal;
                    break;
            }

            this.FootGraphicRight = GraphicDatabase.Get<Graphic_Multi>(
                texNameFoot,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                rightFootColor,
                skinColor);

            this.FootGraphicLeft = GraphicDatabase.Get<Graphic_Multi>(
                texNameFoot,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                leftFootColor,
                skinColor);

            this.FootGraphicRightCol = GraphicDatabase.Get<Graphic_Multi>(
                texNameFoot,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                rightColorFoot,
                skinColor);

            this.FootGraphicLeftCol = GraphicDatabase.Get<Graphic_Multi>(
                texNameFoot,
                ShaderDatabase.CutoutSkin,
                new Vector2(1f, 1f),
                leftColorFoot,
                skinColor);
        }


    }
}
