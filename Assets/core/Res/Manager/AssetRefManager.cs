using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AssetRefManager: Singletion<AssetRefManager>
{
    private Dictionary<string, WeakReference> assetRefDic = new Dictionary<string, WeakReference>();
    private Dictionary<string, WeakReference> assetBundleRefDic = new Dictionary<string, WeakReference>();

    public void RegisterAssetBundle(string assetbundleName, AssetBundle assetBundle)
    {
        if (assetBundleRefDic == null)
            assetBundleRefDic = new Dictionary<string, WeakReference>();
        WeakReference weakReference;
        if (!assetBundleRefDic.TryGetValue(assetbundleName, out weakReference))
        {

            weakReference = new WeakReference(assetBundle);
            assetBundleRefDic.Add(assetbundleName, weakReference);
        }
        else
        {
            if (weakReference.IsAlive)
            {
                SDebug.Debug("SetWeakReference 已有资源");
                (weakReference.Target as AssetBundle).Unload(true);
                weakReference.Target = assetBundle;
            }
            else
            {
                weakReference.Target = assetBundle;
            }
        }
    }

    public void RegisterAsset(string assetName, UnityEngine.Object assetObj)
    {
        if (assetRefDic == null)
            assetRefDic = new Dictionary<string, WeakReference>();
        WeakReference weakReference;
        if (!assetRefDic.TryGetValue(assetName, out weakReference))
        {

            weakReference = new WeakReference(assetObj);
            assetRefDic.Add(assetName, weakReference);
        }
        else
        {
            if (weakReference.IsAlive)
            {
                SDebug.Debug("SetWeakReference 已有资源");
                (weakReference.Target as AssetBundle).Unload(true);
                weakReference.Target = assetObj;
            }
            else
            {
                weakReference.Target = assetObj;
            }
        }
    }

    public void FindAssetBundle(string assetbundleName)
    {

    }

    public void FindAsset(string assetName)
    {

    }
}
