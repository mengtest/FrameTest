using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.Res
{
    public class AssetSyncLoaderImp : CacheOptImp, IAssetSyncLoader
    {

        public T LoadAsset<T>(string name) where T : Object
        {
            UnityEngine.Object obj = GetAssetCache(name);
            if (obj != null && obj is T)
            {
                return obj as T;
            }

            string bundleName = ResUtility.GetBundleName(name);
            AssetBundle bundle = GetAssetBundleCache(bundleName);
            if (bundle == null)
            {
                string url = ResUtility.GetSyncAssetUrl(bundleName);
                bundle = AssetBundle.LoadFromFile(url);
                //需要下载 bundle file
                if (bundle == null)
                {

                }else
                    AddToCache(bundle);
            }

            if (bundle != null)
            {
                obj = bundle.LoadAsset(name);
                if (obj != null)
                {
                    if (obj is T)
                    {
                        AddToCache(obj);
                        return obj as T;
                    }
                    else
                    {
                        SDebug.Error("LoadAsset Type Error , assetName:" + name +
                            "bundleName:" + bundleName + "Type:" + typeof(T) + "AssetType:" + obj.GetType());
                    }
                }
                else
                {
                    SDebug.Error("LoadAsset Error, assetName:" + name + "bundleName:" + bundleName);
                }

                return null;
            }
            return null;

        }

        public AssetBundle LoadAssetBundle(string assetBundleName)
        {
            AssetBundle bundle = GetAssetBundleCache(assetBundleName);
            if (bundle != null)
                return bundle;
            string url = ResUtility.GetSyncAssetUrl(assetBundleName);
            if (url == null)
                return null;
            bundle = AssetBundle.LoadFromFile(url);
            AddToCache(bundle);
            return bundle;
        }
    }
}