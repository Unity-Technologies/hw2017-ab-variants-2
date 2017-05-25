using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationTagListener : MonoBehaviour
{
    void Start()
    {
        UpdateActiveTag(Application.currentLanguage.ToString());
    }

    // Listen for language changes and update the current tag
    void OnApplicationLanguageChanged()
    {
        var newTag = Application.currentLanguage.ToString();
        Debug.Log ("Language changed to:" + newTag);

        UpdateActiveTag(newTag);
    }

    void UpdateActiveTag(string newTag)
    {
        // Hackweek!
        if(newTag == "English")
            newTag = "";
        
        VirtualAssetManager.activeTag = newTag;
        VirtualAssetManager.ApplyRemap(newTag);
    }
}
