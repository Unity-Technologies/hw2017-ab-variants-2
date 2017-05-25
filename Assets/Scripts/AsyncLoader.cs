using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement;

public class AsyncLoader : MonoBehaviour {

    [System.Serializable]
    public struct LoadInfo
    {
        public string address;
        public Transform parent;
        public bool enabled;
    }

    public LoadInfo[] loadList;

	void Start ()
    {
		foreach(var item in loadList)
        {
            if(item.enabled)
            {
                ResourceManager.InstantiateAsync<GameObject>(item.address).complete += (obj) => {
                    obj.result.transform.SetParent(item.parent, false);
                };
            }
        }
	}
}