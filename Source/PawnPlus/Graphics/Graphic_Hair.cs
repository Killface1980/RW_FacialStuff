﻿namespace PawnPlus.Graphics
{
    using System;
    using System.Linq;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
	class Graphic_Hair : Graphic
	{
		#region Private Variables

		// private static string _maskedHairBaseDir;
		private Material[,] hairMat = new Material[4, Enum.GetNames(typeof(HeadCoverage)).Length];
		private bool westFlipped;
		private bool eastFlipped;
		private float drawRotatedExtraAngleOffset;

		#endregion

		// public CompFace FaceComponent { get; set; }
        public override Material MatSingle => MatSouth;
		public override Material MatWest => hairMat[3, 0];
		public override Material MatSouth => hairMat[2, 0];
		public override Material MatEast => hairMat[1, 0];
		public override Material MatNorth => hairMat[0, 0];
		public override bool WestFlipped => westFlipped;
		public override bool EastFlipped => eastFlipped;
		public override bool ShouldDrawRotated
		{
			get
			{
				if (data != null && !data.drawRotated)
				{
					return false;
				}

				if (!(MatEast == MatNorth))
				{
					return MatWest == MatNorth;
				}

				return true;
			}
		}
		public override float DrawRotatedExtraAngleOffset => drawRotatedExtraAngleOffset;
		
		public override void Init(GraphicRequest req)
		{
			if(req.shader != Shaders.Hair)
			{
				Log.Warning("Pawn Plus: tried to create hair graphic with wrong shader. Hair must be rendered using hair shader");
			}

			data = req.graphicData;
			path = req.path;
			color = req.color;
			colorTwo = req.colorTwo;
			drawSize = req.drawSize;
			Texture2D[] defaultHairTex = new Texture2D[hairMat.GetLength(0)];
			defaultHairTex[0] = ContentFinder<Texture2D>.Get(req.path + "_north", reportFailure: false);
			defaultHairTex[1] = ContentFinder<Texture2D>.Get(req.path + "_east", reportFailure: false);
			defaultHairTex[2] = ContentFinder<Texture2D>.Get(req.path + "_south", reportFailure: false);
			defaultHairTex[3] = ContentFinder<Texture2D>.Get(req.path + "_west", reportFailure: false);
			if(defaultHairTex[0] == null)
			{
				if(defaultHairTex[2] != null)
				{
					defaultHairTex[0] = defaultHairTex[2];
					drawRotatedExtraAngleOffset = 180f;
				} else if(defaultHairTex[1] != null)
				{
					defaultHairTex[0] = defaultHairTex[1];
					drawRotatedExtraAngleOffset = -90f;
				} else if(defaultHairTex[3] != null)
				{
					defaultHairTex[0] = defaultHairTex[3];
					drawRotatedExtraAngleOffset = 90f;
				} else
				{
					defaultHairTex[0] = ContentFinder<Texture2D>.Get(req.path, reportFailure: false);
				}
			}

			if(defaultHairTex[0] == null)
			{
				Log.Error("Pawn Plus: Failed to find any textures at " + req.path + " while constructing " + this.ToStringSafe());
				return;
			}

			if(defaultHairTex[2] == null)
			{
				defaultHairTex[2] = defaultHairTex[0];
			}

			if(defaultHairTex[1] == null)
			{
				if(defaultHairTex[3] != null)
				{
					defaultHairTex[1] = defaultHairTex[3];
					eastFlipped = this.DataAllowsFlip;
				} else
				{
					defaultHairTex[1] = defaultHairTex[0];
				}
			}

			if(defaultHairTex[3] == null)
			{
				if(defaultHairTex[1] != null)
				{
					defaultHairTex[3] = defaultHairTex[1];
					westFlipped = this.DataAllowsFlip;
				} else
				{
					defaultHairTex[3] = defaultHairTex[0];
				}
			}

			foreach(HeadCoverage headCoverage in Enum.GetValues(typeof(HeadCoverage)).Cast<HeadCoverage>())
            {
                Texture2D[] maskTex = new Texture2D[4];

                // Don't need to have mask texture for full hair. If matReq.maskTex is null, the mask will default 
                // to white texture which will do nothing.
                if (headCoverage != HeadCoverage.None)
                {
                    maskTex[0] = ContentFinder<Texture2D>.Get("HairMask/Mask_" + headCoverage + "_FrontBack");
                    maskTex[1] = ContentFinder<Texture2D>.Get("HairMask/Mask_" + headCoverage + "_Side");
                    maskTex[2] = maskTex[0];
                    maskTex[3] = maskTex[1];
                }

                for (int i = 0; i < maskTex.Length; ++i)
                {
                    MaterialRequest matReq = default;
                    matReq.mainTex = defaultHairTex[i];
                    matReq.shader = req.shader;
                    matReq.color = color;
                    matReq.colorTwo = colorTwo;
                    matReq.maskTex = headCoverage != HeadCoverage.None ? maskTex[i] : null;
                    matReq.shaderParameters = req.shaderParameters;
                    hairMat[i, (int)headCoverage] = MaterialPool.MatFrom(matReq);
                }
            }
        }
		
		public Material MatAt(Rot4 rot, HeadCoverage coverage)
		{
			int rotation = rot.AsInt;
			if(rotation >= 0 && rotation <= 3)
			{
				return hairMat[rotation, (int)coverage];
			}

			return BaseContent.BadMat;
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			// Hairs must always be rendered with custom hair shader
			if(newShader != Shaders.Hair)
			{
				Log.Warning("Pawn Plus: tried to copy hair graphic with wrong shader. Hair must be rendered using hair shader");
			}

			return GraphicDatabase.Get<Graphic_Hair>(path, Shaders.Hair, drawSize, newColor, newColorTwo, data);
		}

		public override string ToString()
		{
			return "FacialStuff_Hair(initPath=" + path + ", color=" + color + ", colorTwo=" + colorTwo + ")";
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(Gen.HashCombineStruct(Gen.HashCombineStruct(Gen.HashCombine(0, path), color), colorTwo), "FacialStuffHair");
		}
	}
}
