using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

// TODO: Use reflection to find all string fields on scripts etc.
// TODO: prefabs and scriptable objects on disk
public class ExtractKeysWindows : EditorWindow
{
	public MultiLangStringDatabase db;
	public bool parseScenes = true;

	private HashSet<string> keys;

	public Scene[] scenes; 

	[MenuItem("Localization/Extract Keys")]
	static void ShowWindow()
	{
		ExtractKeysWindows window = ScriptableObject.CreateInstance<ExtractKeysWindows>();
		window.ShowUtility();
	}

	void OnGUI()
	{
		db = (MultiLangStringDatabase)EditorGUILayout.ObjectField ("Database", db, typeof(MultiLangStringDatabase));
		parseScenes = EditorGUILayout.Toggle ("Scenes", parseScenes);

		using (new EditorGUI.DisabledScope (db == null))
		{
			if (GUILayout.Button ("Start")) 
			{
				keys = new HashSet<string> ();

				if (parseScenes)
					IterateScenes ();
			}
		}

		if (keys != null && keys.Count > 0) 
		{
			foreach (var key in keys) 
			{
				EditorGUILayout.LabelField (key);
			}
		}
	}

	void IterateScenes()
	{
		var assets = AssetDatabase.FindAssets ("t:Scene");
		for(int i = 0; i < assets.Length; ++i)
		{
			EditorUtility.DisplayProgressBar ("Processing Scenes", i.ToString () + "/" + assets.Length, (float)i / assets.Length);
			var path = AssetDatabase.GUIDToAssetPath (assets[i]);
			EditorSceneManager.OpenScene (path, OpenSceneMode.Single);

			// UI Text
			var textObjects = Object.FindObjectsOfType<UnityEngine.UI.Text> ();
			foreach (var to in textObjects) 
			{
				if(!string.IsNullOrEmpty(to.text))
					keys.Add(to.text);
			}

			// UI Inout
			var inputObjects = Object.FindObjectsOfType<UnityEngine.UI.InputField> ();
			foreach (var io in inputObjects) 
			{
				if(!string.IsNullOrEmpty(io.text))
					keys.Add(io.text);
			}

			// Text Mesh
			var textMeshObjects = Object.FindObjectsOfType<UnityEngine.TextMesh> ();
			foreach (var tm in textMeshObjects) 
			{
				if(!string.IsNullOrEmpty(tm.text))
					keys.Add(tm.text);
			}
		}

		EditorUtility.ClearProgressBar ();

		UpdateDB ();
	}

	void UpdateDB()
	{
		if (db == null)
			return;

		foreach (var key in keys) 
		{
			db.AddTextEntry (key);
		}
	}
}

