namespace PawnPlus.Graphics
{
    using RimWorld;
    using System.IO;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class Shaders
    {
        public static Shader Hair { get; private set; }

        public static Material FacePart { get; private set; }

        public static int MainTexPropID { get; private set; }

        public static int ColorOnePropID { get; private set; }

        public static int TexIndexPropID { get; private set; }

        static Shaders()
        {
            ModMetaData thisMod = ModLister.GetModWithIdentifier("killface.pawnplus");
            if (thisMod == null)
            {
                Log.Error("Pawn Plus: failed to load shader - could not find mod root directory");
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
            if (shaderAssets == null)
            {
                Log.Error("Pawn Plus: failed to load shader. Could not locate shader asset bundle at " + shaderAssetBundlePath);
                return;
            }

            Shader[] shaders = shaderAssets.LoadAllAssets<Shader>();
            Shader facePartShader = null;
            foreach (Shader loadedShader in shaders)
            {
                if (loadedShader == null) { continue; }

                // Log.Message("Shader Name: " + loadedShader.name);

                // There is only the hair shader
                switch (loadedShader.name)
                {
                    case "Custom/Mod/FacialStuff/Hair":
                        Hair = LoadShader(loadedShader);
                        break;

                    case "Custom/Mod/FacialStuff/FacePart":
                        {
                            facePartShader = LoadShader(loadedShader);

                            break;
                        }
                }
            }
            // Use hair shader instead
            if (facePartShader != null)
            {
                FacePart = new Material(facePartShader);
            }
            else
            {
                FacePart = new Material(Hair);
            }

            if (Hair == null)
            {
                Log.Error("Pawn Plus: could not find shader Custom/Mod/FacialStuff/Hair in shader asset bundle");
            }

            if (FacePart == null)
            {
                Log.Error("Pawn Plus: could not find shader Custom/Mod/FacialStuff/FacePart in shader asset bundle");
            }

            MainTexPropID = Shader.PropertyToID("_MainTex");
            ColorOnePropID = Shader.PropertyToID("_Color");
            TexIndexPropID = Shader.PropertyToID("_TexIndex");
        }

        private static Shader LoadShader(Shader shader)
        {
            if (shader)
            {
                Log.Message("Pawn Plus: successfully loaded shader " + shader.name);
                return shader;
            }

            Log.Message("Pawn Plus: could not load shader " + shader.name + ". This shader is not supported");
            return null;
        }
    }
}