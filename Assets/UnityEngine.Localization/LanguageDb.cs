
using UnityEngine;
using UnityEngine.Localization;

public class LanguageDb : MonoBehaviour
{
	public MultiLangStringDatabase db;
	SystemLanguage currentLanguage;

	void Awake()
	{
		db.MakeCurrent();
	}

	void FixedUpdate()
	{
		if (currentLanguage != Application.currentLanguage)
		{
			currentLanguage = Application.currentLanguage;

			// Inform observers.
			var obs = Object.FindObjectsOfType<MonoBehaviour>();
			foreach (var o in obs)
			{
				var lang = o as IStringDatabaseObserver;
				if (lang != null)
					lang.NotifyDataChange();
			}
		}
	}
}
