using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Graphics
{
	class MatProps_Multi
	{
		private MaterialPropertyBlock[] _propertyBlocks = new MaterialPropertyBlock[4];
		private int _originalInstanceFlag;

		public static MatProps_Multi Create(string texturePath)
		{
			MatProps_Multi matProps = new MatProps_Multi();
			matProps.Init(texturePath);
			return matProps;
		}
		
		public MaterialPropertyBlock GetMaterialProperty(Rot4 rot4)
		{
			return _propertyBlocks[rot4.AsInt];
		}
		
		public void SetVector(int propertyID, Vector4 vector)
		{
			for(int i = 0; i < _propertyBlocks.Length; ++i)
			{
				if((_originalInstanceFlag & 1 << i) != 0)
				{
					_propertyBlocks[i].SetVector(propertyID, vector);
				}
			}
		}

		public void SetVector(string propertyName, Vector4 vector)
		{
			SetVector(Shader.PropertyToID(propertyName), vector);
		}

		public void SetColor(int propertyID, Color color)
		{
			for(int i = 0; i < _propertyBlocks.Length; ++i)
			{
				if((_originalInstanceFlag & 1 << i) != 0)
				{
					_propertyBlocks[i].SetColor(propertyID, color);
				}
			}
		}

		public void SetColor(string propertyName, Color color)
		{
			SetColor(Shader.PropertyToID(propertyName), color);
		}
				
		private MatProps_Multi()
		{

		}

		private void Init(string texturePath)
		{
			Texture2D[] texArray = new Texture2D[4];
			texArray[0] = ContentFinder<Texture2D>.Get(texturePath + "_north", reportFailure: false);
			texArray[1] = ContentFinder<Texture2D>.Get(texturePath + "_east", reportFailure: false);
			texArray[2] = ContentFinder<Texture2D>.Get(texturePath + "_south", reportFailure: false);
			texArray[3] = ContentFinder<Texture2D>.Get(texturePath + "_west", reportFailure: false);
			for(int i = 0; i < _propertyBlocks.Length; ++i)
			{
				if(texArray[i] != null)
				{
					_propertyBlocks[i] = new MaterialPropertyBlock();
					_propertyBlocks[i].SetTexture("_MainTex", texArray[i]);
					_originalInstanceFlag |= 1 << i;
				}
			}
			if(_propertyBlocks[0] == null)
			{
				if(_propertyBlocks[2] != null)
				{
					_propertyBlocks[0] = _propertyBlocks[2];
				} else if(_propertyBlocks[1] != null)
				{
					_propertyBlocks[0] = _propertyBlocks[1];
				} else if(_propertyBlocks[3] != null)
				{
					_propertyBlocks[0] = _propertyBlocks[3];
				} else
				{
					Texture2D singleTex = ContentFinder<Texture2D>.Get(texturePath, reportFailure: false);
					if(singleTex == null)
					{
						Log.Error("Failed to find any textures at " + texturePath + " while constructing " + this.ToStringSafe());
						MaterialPropertyBlock badMatProp = new MaterialPropertyBlock();
						badMatProp.SetTexture("_MainTex", BaseContent.BadTex);
						for(int i = 0; i < _propertyBlocks.Length; ++i)
						{
							_propertyBlocks[i] = badMatProp;
						}
						_originalInstanceFlag |= 1;
						return;
					}
					_propertyBlocks[0] = new MaterialPropertyBlock();
					_propertyBlocks[0].SetTexture("_MainTex", singleTex);
				}
			}
			if(_propertyBlocks[2] == null)
			{
				_propertyBlocks[2] = _propertyBlocks[0];
			}
			if(_propertyBlocks[1] == null)
			{
				if(_propertyBlocks[3] != null)
				{
					_propertyBlocks[1] = _propertyBlocks[3];
				} else
				{
					_propertyBlocks[1] = _propertyBlocks[0];
				}
			}
			if(_propertyBlocks[3] == null)
			{
				if(_propertyBlocks[1] != null)
				{
					_propertyBlocks[3] = _propertyBlocks[1];
				} else
				{
					_propertyBlocks[3] = _propertyBlocks[0];
				}
			}
		}
	}
}
