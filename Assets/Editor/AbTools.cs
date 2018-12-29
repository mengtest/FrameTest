using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EdotprTools
{
    public class AbTools 
    {
        [MenuItem("Assets/AbTools/DebugAssetType")]
        public static void DebugAssetType()
        {
            string info = UnityEditor.AssetDatabase.GetImplicitAssetBundleName("ims.red.bing");
            Debug.Log("info:    "+ info);
        }

        [MenuItem("Assets/AbTools/UpdateAbName")]
        public static void  UpdateAbName()
        {
            var sprites = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            if (sprites != null && sprites.Length > 0)
            {
                foreach (Object sprite in sprites)
                {
                    string path = AssetDatabase.GetAssetPath(sprite);
                    AssetImporter ai = AssetImporter.GetAtPath(path);
                    string abName = path.ToLower().Replace("assets/", "").Replace("/", ".").Replace(".gif", "");
                    ai.SetAssetBundleNameAndVariant(abName, "unity3d");
                    ai.SaveAndReimport();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/AbTools/BuildImgs")]
        public static void BuildImgs()
        {
            UnityEditor.BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }

        const string bundleSuffix = "unity3d";
        [MenuItem("Assets/AbTools/BuildSingle")]
        public static void BuildSingle()
        {
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
            if (objs == null || objs.Length <= 0)
                return;
            AssetBundleBuild[] builds= new AssetBundleBuild[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = objs[i].name;
                build.assetBundleVariant = bundleSuffix;
                string objPath = AssetDatabase.GetAssetPath(objs[i]);
                build.assetNames = new string[] { objPath };
                builds[i] = build;
            }

            UnityEditor.BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, builds, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
    }
}
