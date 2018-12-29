using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Res
{
    public interface IAssetAsyncLoader
    {
        void LoadAssetAsync<T>(string name, LoadAssetCallback callback) where T : Object;

        void LoadAssetBundleAsync(string assetBundleName, LoadAssetCallback callback);
        void OnUpdate();
    }

    public interface IAssetLoader: IAssetAsyncLoader, IAssetSyncLoader
    {
    }
}