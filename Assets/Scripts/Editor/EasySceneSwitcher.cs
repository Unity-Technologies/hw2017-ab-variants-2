//Based off of the scene auto loader found here: http://wiki.unity3d.com/index.php/SceneAutoLoader

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EasySceneSwitcher
{
	const string loaderScene = 	"Assets/Scenes/Persistent.unity";
	const string scene1 = 		"Assets/Scenes/SecurityRoom.unity";
	const string scene2 = 		"Assets/Scenes/Market.unity";

	const string ONNAME = 		"Scene Switcher/Enable Scene Switcher";
	const string OFFNAME = 		"Scene Switcher/Disable Scene Switcher";	
	const string SCENE1MENU =	"Scene Switcher/Open Loader Scene %1";
	const string SCENE2MENU = 	"Scene Switcher/Open Gameplay Scene 1 %2";
	const string SCENE3MENU = 	"Scene Switcher/Open Gameplay Scene 2 %3";

	static bool isEnabled;

	const string _prevSceneVal = "EasySceneSwitcher.PreviousScene";
	static string PreviousScene
	{
		get { return EditorPrefs.GetString(_prevSceneVal, EditorSceneManager.GetActiveScene().path); }
		set { EditorPrefs.SetString(_prevSceneVal, value); }
	}

	static EasySceneSwitcher()
	{
		EditorApplication.playmodeStateChanged += OnPlayModeChanged;

		isEnabled = EditorPrefs.GetBool (ONNAME, false);

		EditorApplication.delayCall += () =>
		{
			ToggleMenuItem(isEnabled);
		};
	}

	[MenuItem(ONNAME)]
	static void EnableSceneSwitcher()
	{
		ToggleMenuItem (true);
	}

	[MenuItem(ONNAME, true)]
	static bool EnableValidate()
	{
		return !isEnabled;
	}

	[MenuItem(OFFNAME)]
	static void DisableSceneSwitcher()
	{
		ToggleMenuItem (false);
	}

	[MenuItem(OFFNAME, true)]
	static bool DisableValidate()
	{
		return isEnabled;
	}

	static void ToggleMenuItem(bool enabled)
	{
		Menu.SetChecked (ONNAME, enabled);
		Menu.SetChecked (OFFNAME, !enabled);

		EditorPrefs.SetBool (ONNAME, enabled);
		isEnabled = enabled;
	}

	[MenuItem(SCENE1MENU)]
	public static void OpenLoaderScene()
	{
		if ( EditorApplication.isPlaying == true )
		{
			EditorApplication.isPlaying = false;
			return;
		}

		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ()) 
			EditorSceneManager.OpenScene (loaderScene);
	}

	[MenuItem(SCENE2MENU)]
	public static void OpenGameplayScene1()
	{
		if ( EditorApplication.isPlaying == true )
		{
			EditorApplication.isPlaying = false;
			return;
		}

		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ()) 
			EditorSceneManager.OpenScene (scene1);
	}

	[MenuItem(SCENE3MENU)]
	public static void OpenGameplayScene2()
	{
		if ( EditorApplication.isPlaying == true )
		{
			EditorApplication.isPlaying = false;
			return;
		}

		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ()) 
			EditorSceneManager.OpenScene (scene2);
	}

	// Play mode change callback handles the scene load/reload.
	static void OnPlayModeChanged()
	{
		if (!isEnabled)
			return;
		
		if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
		{
			// User pressed play -- autoload master scene.
			PreviousScene = EditorSceneManager.GetActiveScene().path;
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				EditorSceneManager.OpenScene(loaderScene);
			}
			else
			{
				// User cancelled the save operation -- cancel play as well.
				EditorApplication.isPlaying = false;
			}
		}
		if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
		{
			// User pressed stop -- reload previous scene.
			EditorSceneManager.OpenScene(PreviousScene);
		}
	}
}