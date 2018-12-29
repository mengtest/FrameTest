using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResUtility
{
    //static AssetBundleManifest manifest;
    public static string GetBundleName(string assetName)
    {
        return assetName;
    }

    const string bundleSuffix = ".unity3d";
    //用于立即加载的url
    public static string GetSyncAssetUrl(string assetBundleName)
    {
#if UNITY_EDITOR
        string url = Application.streamingAssetsPath + "/" + assetBundleName+ bundleSuffix;

#else
       string url = Application.streamingAssetsPath + "/" + assetBundleName+bundleSuffix;
#endif
        if (File.Exists(url))
        {
            return url;
        }
        //else
        //    return pkgFileBase + assetBundleName;
        UnityEngine.Debug.LogError("not Exist url:" + url);
        return null;
    }
}
