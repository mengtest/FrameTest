using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class AssetCache :Singletion<AssetCache>, ICacheOpt
{
    Dictionary<string, AssetBundle> assetBundleMap = new Dictionary<string, AssetBundle>();
    Dictionary<string, UnityEngine.Object> assetMap = new Dictionary<string, Object>();

    public void AddToCache(AssetBundle assetBundle)
    {

    }

    public void AddToCache(UnityEngine.Object asset)
    {

    }

    public AssetBundle GetAssetBundleCache(string assetBundleName)
    {
        if (assetBundleName == null)
            return null;
        AssetBundle assetBundle;
        if (assetBundleMap.TryGetValue(assetBundleName, out assetBundle))
        {
            return assetBundle;
        }
        return null;
    }

    public UnityEngine.Object GetAssetCache(string assetName)
    {
        if (assetName == null)
            return null;

        UnityEngine.Object asset;
        if (assetMap.TryGetValue(assetName, out asset))
        {
            return asset;
        }
        return null;
    }
}
