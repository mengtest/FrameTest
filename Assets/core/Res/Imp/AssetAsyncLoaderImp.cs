using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.Res
{

    public class AssetAsyncLoaderImp : IAssetAsyncLoader, ICacheOpt
    {
        public void AddToCache(AssetBundle assetBundle)
        {
            throw new System.NotImplementedException();
        }

        public void AddToCache(Object asset)
        {
            throw new System.NotImplementedException();
        }

        public AssetBundle GetAssetBundleCache(string assetBundleName)
        {
            throw new System.NotImplementedException();
        }

        public Object GetAssetCache(string assetName)
        {
            throw new System.NotImplementedException();
        }

        public void LoadAssetAsync<T>(string name, LoadAssetCallback callback) where T : Object
        {
            //AssetBundleRequest
            throw new System.NotImplementedException();
        }

        public void LoadAssetBundleAsync(string url, LoadAssetCallback callback)
        {
            //AssetBundleCreateRequest
            throw new System.NotImplementedException();
        }

        //public UnityEngine.Object LoadAsset(string assetBundleName, string assetName, System.Type type, LoadAssetCallback callback, params object[] param)
        //{
        //    UnityEngine.Object obj;
        //    if (assetMap != null && assetMap.TryGetValue(abObjName, out obj))
        //    {
        //        return obj;
        //    }

        //    AssetBundle assetBundle;
        //    if (assetBundleMap != null && assetBundleMap.TryGetValue(abObjName, out assetBundle))
        //    {
        //        obj = assetBundle.LoadAsset(assetName);

        //        return obj;
        //    }


        //    string resPath = Application.streamingAssetsPath + "/" + assetBundleName;
        //    assetBundle = AssetBundle.LoadFromFile(resPath);
        //    if (assetBundleMap == null)
        //        assetBundleMap = new Dictionary<string, AssetBundle>();

        //    if (!assetBundleMap.ContainsKey(assetBundleName))
        //        assetBundleMap.Add(assetBundleName, assetBundle);
        //    obj = assetBundle.LoadAsset(assetName);
        //    if (assetMap == null)
        //        assetMap = new Dictionary<string, UnityEngine.Object>();
        //    if (!assetMap.ContainsKey(assetName))
        //        assetMap.Add(abObjName, obj);
        //    return obj;
        //}


        //AssetBundle LoadAssetBundleInternalSync(string assetBundleName, bool loadDepend = true, bool noDestroy = false)
        //{
        //    AssetBundle assetBundle;
        //    if (assetBundleMap != null && assetBundleMap.TryGetValue(assetBundleName, out assetBundle))
        //    {
        //        return assetBundle;
        //    }

        //    string url = GetSyncAssetUrl(assetBundleName);
        //    assetBundle = AssetBundle.LoadFromFile(url);
        //    if (assetBundle != null)
        //    {
        //        assetBundleMap[assetBundleName] = assetBundle;
        //    }
        //    return assetBundle;
        //}


        public void OnUpdate()
        {

        }
    }
}