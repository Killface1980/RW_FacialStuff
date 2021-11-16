namespace PawnPlus.Graphics
{
    using System.Collections.Generic;
    using System.Linq;

    using UnityEngine;

    using Verse;

    public class TextureSet
	{
		private static Dictionary<string, TextureSet> cachedTextureSets = 
			new Dictionary<string, TextureSet>();

		private Texture2DArray _textureArray;
		private float[] _texIndices = new float[4];

		public static TextureSet Create(string texturePath)
		{
			if(texturePath == null)
			{
				return null;
			}

			if(!cachedTextureSets.TryGetValue(texturePath, out TextureSet textureSet))
			{
				textureSet = new TextureSet();
				textureSet.Init(texturePath);
			}

			return textureSet;
		}
		
		public void GetIndexForRot(Rot4 rot, out float index)
		{
			index = _texIndices[rot.AsInt];
		}

		public Texture2DArray GetTextureArray()
		{
			return _textureArray;
		}

		private TextureSet()
		{

		}

		private void Init(string texturePath)
		{
			_textureArray = LoadTextures(texturePath);
		}

		private Texture2DArray LoadTextures(string texturePath)
		{
			Texture2D[] texArray = new Texture2D[4];
			texArray[0] = ContentFinder<Texture2D>.Get(texturePath + "_north", reportFailure: false);
			texArray[1] = ContentFinder<Texture2D>.Get(texturePath + "_east", reportFailure: false);
			texArray[2] = ContentFinder<Texture2D>.Get(texturePath + "_south", reportFailure: false);
			texArray[3] = ContentFinder<Texture2D>.Get(texturePath + "_west", reportFailure: false);
			_texIndices = new float[4];
			if(texArray.All(i => i == null))
			{
				texArray[0] = ContentFinder<Texture2D>.Get(texturePath + "_north", reportFailure: false);
				if(texArray[0] == null)
				{
					Log.Error("Failed to find any textures at " + texturePath + " while constructing " + this.ToStringSafe());
					texArray[0] = BaseContent.BadTex;
				}

				for(int i = 0; i < this._texIndices.Length; ++i)
				{
					_texIndices[i] = 0f;
				}

				return CreateTextureArray(texArray);
			}

			int texCount = 0;
			for(int i = 0; i < texArray.Length; ++i)
			{
				if(texArray[i] != null)
				{
					_texIndices[i] = texCount;
					++texCount;
				}
				else
				{
					_texIndices[i] = -1;
				}
			}
	
			if(_texIndices[0] < 0f)
			{
				if(_texIndices[2] >= 0f)
				{
					_texIndices[0] = _texIndices[2];
				} else if(_texIndices[1] >= 0f)
				{
					_texIndices[0] = _texIndices[1];
				} else if(_texIndices[3] >= 0f)
				{
					_texIndices[0] = _texIndices[3];
				}
			}

			if(_texIndices[2] < 0f)
			{
				_texIndices[2] = _texIndices[0];
			}

			if(_texIndices[1] < 0f)
			{
				if(_texIndices[3] >= 0f)
				{
					_texIndices[1] = _texIndices[3];
				} else
				{
					_texIndices[1] = _texIndices[0];
				}
			}

			if(_texIndices[3] < 0f)
			{
				if(_texIndices[1] >= 0f)
				{
					_texIndices[3] = _texIndices[1];
				} else
				{
					_texIndices[3] = _texIndices[0];
				}
			}

			return CreateTextureArray(texArray);
		}

		private Texture2DArray CreateTextureArray(Texture2D[] sparseTextureArray)
		{
			int texCount = sparseTextureArray.Count(i => i != null);
			Texture2D firstTex = sparseTextureArray.First(i => i != null);
			Texture2DArray textureArray = new Texture2DArray(
				firstTex.width, 
				firstTex.height, 
				texCount, 
				firstTex.format, 
				true);
			int curTexIndex = 0;
			for(int i = 0; i < sparseTextureArray.Length; ++i)
			{
				if(sparseTextureArray[i] != null)
				{
					for(int mipLevel = 0; mipLevel < sparseTextureArray[i].mipmapCount; ++mipLevel)
					{
						Graphics.CopyTexture(sparseTextureArray[i], 0, mipLevel, textureArray, curTexIndex, mipLevel);
					}

					++curTexIndex;
				}
			}

			return textureArray;
		}
	}
}
