using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core.Res
{
    public delegate void LoadAssetCallback(UnityEngine.Object obj, object[] param);
    public class AssetLoaderManager : Singletion<AssetLoaderManager>, IAssetLoader
    {
        AssetSyncLoaderImp loader = null;
        AssetAsyncLoaderImp asyncLoader = null;
        AssetBundleManifest manifest;

        public void Initialize()
        {
            loader = new AssetSyncLoaderImp();
            asyncLoader = new AssetAsyncLoaderImp();
        }

        /// <summary>
        /// 同步加载
        /// </summary>
        /// <typeparam name="T">asset 类型</typeparam>
        /// <param name="name">asset 名字需唯一</param>
        /// <returns>asset，加载不到返回null</returns>
        public T LoadAsset<T>(string name) where T : Object
        {
            if (loader == null)
            {
                SDebug.Error("AssetLoaderManager loader is null");
                return null;
            }
            return loader.LoadAsset<T>(name);
        }

        public void LoadAssetAsync<T>(string name, LoadAssetCallback callback) where T : Object
        {
            //UnityEngine.Object obj = LoadAsset<T>(name);
            //if (obj != null)
            //{
            //    if (callback != null)
            //        callback(obj, new object[] { name });
            //    return;
            //}

            if (asyncLoader == null)
            {
                SDebug.Error("AssetLoaderManager asyncLoader is null");
                callback(null, new object[] { });
                return;
            }
            asyncLoader.LoadAssetAsync<T>(name, callback);
        }

        public AssetBundle LoadAssetBundle(string assetBundleName)
        {
            if (loader == null)
            {
                SDebug.Error("AssetLoaderManager loader is null");
                return null;
            }
            return loader.LoadAssetBundle(assetBundleName); ;
        }

        public void LoadAssetBundleAsync(string assetBundleName, LoadAssetCallback callback)
        {
            if (asyncLoader == null)
            {
                SDebug.Error("AssetLoaderManager asyncLoader is null");
                callback(null, new object[] { });
                return;
            }
            asyncLoader.LoadAssetBundleAsync(assetBundleName, callback);
        }

        public void OnUpdate()
        {
            if (asyncLoader != null)
            {
                asyncLoader.OnUpdate();
            }
        }
    }
}