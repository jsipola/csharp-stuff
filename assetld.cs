using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class assetld : Monobehaviour {
    public string url = "";
    IEnumerator Start () {
        WWW www = new WWW(url);
        yield return www;
        AssetBundle bundle = www.assetBundle;
        AssetBundleRequest req = bundle.LoadAssetAsync("cube",typeof(GameObject));
        yield return request;
        GameObject cube = req.asset as GameObject;
        bundle.Unload(false);
        www.Dispose();
/*
        if (AssetName == "") {
            Instantiate(bundle.mainAsset);
        } else {
            Instantiate(bundle.LoadAsset(AssetName));
            bundle.Unload(false);
        }
*/
    }
}
