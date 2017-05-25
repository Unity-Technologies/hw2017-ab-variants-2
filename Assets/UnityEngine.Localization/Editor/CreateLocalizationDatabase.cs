using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization;

public class CreateLocalizationDatabase : MonoBehaviour 
{
	[MenuItem("Localization/Create Database")]
	public static void CreateDB()
	{
		var path = EditorUtility.SaveFilePanelInProject("Create Localization Database", "Localization Database", "asset", "");
		if (!string.IsNullOrEmpty (path)) 
		{
			var db = ScriptableObject.CreateInstance<MultiLangStringDatabase> ();
			AssetDatabase.CreateAsset (db, path);
		}
	}
}
