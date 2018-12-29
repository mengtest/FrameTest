using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICacheOpt
{
    void AddToCache(AssetBundle assetBundle);

    void AddToCache(UnityEngine.Object asset);

    AssetBundle GetAssetBundleCache(string assetBundleName);

    UnityEngine.Object GetAssetCache(string assetName);
}
