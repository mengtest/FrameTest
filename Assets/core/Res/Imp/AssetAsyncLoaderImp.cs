using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Res
{
    public class AssetAsyncLoaderImp : CacheOptImp, IAssetAsyncLoader
    {
        /// <summary>
        /// 使用Get 别使用new
        /// </summary>
        class Loader:CacheOptImp, IDisposable
        {
            public static Queue<Loader> pool = new Queue<Loader>();
            public static Loader Get()
            {
                if (pool == null)
                {
                    throw new Exception("AsynLoader pool is null");
                }

                if (pool.Count > 0)
                {
                    return pool.Dequeue();
                }
                else
                {
                    return new Loader();
                }
            }

            public static void Dispose(Loader loader)
            {
                if (pool == null)
                {
                    throw new Exception("AsynLoader pool is null");
                }
                loader.Dispose();
                pool.Enqueue(loader);
            }

            public string name = string.Empty;
            public AsyncOperation request = null;
            public LoadAssetCallback callback;
            public bool Update()
            {
                if (request == null)
                {
                    SDebug.Error("AsynLoader request is null,   " + name + " ," + request.GetType());
                    return true;
                }

                if (request.isDone)
                {
                    // DownloadCompleteHandler
                    if (request is AssetBundleRequest)
                    {
                        UnityEngine.Object obj = (request as AssetBundleRequest).asset;
                        AddToCache(obj);
                        callback(obj, null);
                    }
                    else if(request is AssetBundleCreateRequest)
                    {
                        AssetBundle bundle = (request as AssetBundleCreateRequest).assetBundle;
                        AddToCache(bundle);
                        callback(bundle, null);
                    }

                    return true;
                }
                else
                {
                    SDebug.Debug("下载进度:"+ name + "  "+ request.GetType()+"  "+ request.progress);
                    //进度
                    return false;
                }
            }

            public void Dispose()
            {
                name = string.Empty;
                request = null;
                callback = null;
            }
        }

        ArrayList loading = new ArrayList();
        public void LoadAssetAsync<T>(string name, LoadAssetCallback callback) where T : UnityEngine.Object
        {
            //AssetBundleRequest
            UnityEngine.Object obj = GetAssetCache(name);
            if (obj != null && obj is T)
            {
                //TODO
                callback( obj as T, null);
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

                }
                else
                    AddToCache(bundle);
            }

            if (bundle != null)
            {
                AssetBundleRequest  request = bundle.LoadAssetAsync<T>(name);
                Loader loader = Loader.Get();
                loader.name = name;
                loader.callback = callback;
                loader.request = request;
                loading.Insert(loading.Count, loader);
            }
        }

        public void LoadAssetBundleAsync(string assetBundleName, LoadAssetCallback callback)
        {
            AssetBundle bundle = GetAssetBundleCache(assetBundleName);
            if (bundle != null)
                callback(bundle, null);
            string url = ResUtility.GetSyncAssetUrl(assetBundleName);
            if (url == null)
                throw new Exception("null url: " + assetBundleName);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(url);
            Loader loader = Loader.Get();
            loader.name = assetBundleName;
            loader.callback = callback;
            loader.request = request;
            loading.Insert(loading.Count, loader);
        }

        public void OnUpdate()
        {
            if (loading.Count > 0)
            {
                for(int i= 0;i< loading.Count; i++)
                {
                    var loader = loading[i] as Loader;
                    if (loader.Update())
                    {
                        loading.Remove(loader);
                    }
                }
            }
        }
    }
}