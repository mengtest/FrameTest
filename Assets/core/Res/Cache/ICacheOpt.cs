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

public abstract class CacheOptImp: ICacheOpt
{
    public virtual void AddToCache(AssetBundle assetBundle)
    {
        AssetCache.Instance.AddToCache(assetBundle);
    }

    public virtual void AddToCache(Object obj)
    {
        AssetCache.Instance.AddToCache(obj);
    }

    public virtual Object GetAssetCache(string assetName)
    {
        return AssetCache.Instance.GetAssetCache(assetName);
    }

    public virtual AssetBundle GetAssetBundleCache(string assetBundleName)
    {
        return AssetCache.Instance.GetAssetBundleCache(assetBundleName);
    }
}


