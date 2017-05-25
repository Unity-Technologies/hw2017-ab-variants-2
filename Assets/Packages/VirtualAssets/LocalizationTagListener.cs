using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationTagListener : MonoBehaviour
{
    // Listen for language changes and update the current tag
    void OnApplicationLanguageChanged()
    {
        Debug.Log ("Language changed to:" + Application.currentLanguage);
    }
}
