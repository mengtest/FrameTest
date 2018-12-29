using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IAssetSyncLoader
{
    T LoadAsset<T>(string name) where T : Object;

    AssetBundle LoadAssetBundle(string assetBundleName);
}
