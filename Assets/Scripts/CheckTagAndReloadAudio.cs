using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTagAndReloadAudio : MonoBehaviour {

    [SerializeField] string activeTag;

	// Use this for initialization
	void Start () {
        activeTag = VirtualAssetManager.activeTag;
	}
	
	// Update is called once per frame
	void Update () {
        if(VirtualAssetManager.activeTag != activeTag)
        {
            this.GetComponent<AudioSource>().Stop();
            this.GetComponent<AudioSource>().Play();
            this.activeTag = VirtualAssetManager.activeTag;
        }
	}
}
