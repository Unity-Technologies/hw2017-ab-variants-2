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

	private Dictionary<string, HashSet<string>> keys;

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
				keys = new Dictionary<string, HashSet<string>> ();

				if (parseScenes)
					IterateScenes ();
			}
		}

		if (keys != null && keys.Count > 0) 
		{
			foreach (var key in keys) 
			{
				foreach (var c in key.Value)
					EditorGUILayout.LabelField ("#: " + c);
				EditorGUILayout.LabelField (key.Key);
				EditorGUILayout.Space ();
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
				if (!string.IsNullOrEmpty (to.text))
					AddKey (to.text, path + "::" + GetGameObjectPath(to.gameObject) + "/Text:text");
			}

			// UI Inout
			var inputObjects = Object.FindObjectsOfType<UnityEngine.UI.InputField> ();
			foreach (var io in inputObjects) 
			{
				if(!string.IsNullOrEmpty(io.text))
					AddKey (io.text, path + "::" + GetGameObjectPath(io.gameObject) + "/InputField:text");
			}

			// Text Mesh
			var textMeshObjects = Object.FindObjectsOfType<UnityEngine.TextMesh> ();
			foreach (var tm in textMeshObjects) 
			{
				if(!string.IsNullOrEmpty(tm.text))
					AddKey (tm.text, path + "::" + GetGameObjectPath(tm.gameObject) + "/TextMesh:text");
			}

			// Reaction Collection - specific to this project. We should do relfection here for user scripts.
			var reactions = Object.FindObjectsOfType<ReactionCollection> ();
			foreach (var r in reactions) 
			{
				foreach (var currentReaction in r.reactions) 
				{
					TextReaction tr = currentReaction as TextReaction;
					if (tr)
						AddKey (tr.message, path + "::" + GetGameObjectPath(r.gameObject) + "/TextReaction:message");
				}
			}
		}

		EditorUtility.ClearProgressBar ();

		UpdateDB ();
	}

	void AddKey(string key, string comment)
	{
		if (!keys.ContainsKey (key)) {
			keys.Add (key, new HashSet<string>());
		}

		keys [key].Add (comment);
	}

	public static string GetGameObjectPath(GameObject obj)
	{
		string path = "/" + obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = "/" + obj.name + path;
		}
		return path;
	}

	void UpdateDB()
	{
		if (db == null)
			return;

		foreach (var key in keys) 
		{
			db.AddTextEntry (key.Key);
			string[] array = new string[key.Value.Count];
			key.Value.CopyTo (array);
			db.SetTextEntry (SystemLanguage.English, key.Key, key.Key, array);
		}
	}
}

