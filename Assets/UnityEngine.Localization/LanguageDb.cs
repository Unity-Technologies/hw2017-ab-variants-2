
using UnityEngine;
using UnityEngine.Localization;

public class LanguageDb : MonoBehaviour
{
	public MultiLangStringDatabase db;

	void Awake()
	{
		db.MakeCurrent();
	}

	void OnGUI()
	{
		GUILayout.Label(MultiLangStringDatabase.currentDb.ToString());
	
	}
}
