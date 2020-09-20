using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Graphics
{
	[StaticConstructorOnStartup]
	public class Shaders
	{
		public static Shader Hair { get; private set; }

		public static Shader FacePart { get; private set; }

		static Shaders()
		{
			var thisMod = ModLister.GetModWithIdentifier("killface.facialstuff");
			if(thisMod == null)
			{
				Log.Error("Facial Stuff: failed to load shader - could not find mod root directory");
				return;
			}
			string shaderAssetBundlePath = Path.Combine(
				thisMod.RootDir.FullName +
					Path.DirectorySeparatorChar +
					VersionControl.CurrentVersion.ToString(2) +
					Path.DirectorySeparatorChar +
					"Assets" +
					Path.DirectorySeparatorChar,
				"shaders.assets");
			AssetBundle shaderAssets = AssetBundle.LoadFromFile(shaderAssetBundlePath);
			if(shaderAssets == null)
			{
				Log.Error("Facial Stuff: failed to load shader. Could not locate shader asset bundle at " + shaderAssetBundlePath);
				return;
			}
			Shader[] shaders = shaderAssets.LoadAllAssets<Shader>();
			for(int i = 0; i < shaders.Length; i++)
			{
				if(shaders[i].name.Equals("Custom/Mod/FacialStuff/Hair"))
				{
					if(shaders[i].isSupported)
					{
						Log.Message("Facial Stuff: successfully loaded shader " + shaders[i].name);
						Hair = shaders[i];
					} 
					else
					{
						Log.Message("Facial Stuff: could not load shader " + Hair.name + ". This shader is not supported");
						Hair = null;
					}
				} else if(shaders[i].name.Equals("Custom/Mod/FacialStuff/FacePart"))
				{
					if(shaders[i].isSupported)
					{
						Log.Message("Facial Stuff: successfully loaded shader " + shaders[i].name);
						FacePart = shaders[i];
					} 
					else
					{
						Log.Message("Facial Stuff: could not load shader " + FacePart.name + ". This shader is not supported");
						FacePart = null;
					}
				}
			}
			if(Hair == null)
			{
				Log.Error("Facial Stuff: could not find shader Custom/Mod/FacialStuff/Hair in shader asset bundle");
			} 
			if(FacePart == null)
			{
				Log.Error("Facial Stuff: could not find shader Custom/Mod/FacialStuff/FacePart in shader asset bundle");
			}
		}
	}
}
