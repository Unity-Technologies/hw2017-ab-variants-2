using UnityEngine;
using UnityEngine.Localization;
using System.Collections;
using UnityEditor;

namespace UnityEditor {
	namespace Localization {

		[CustomEditor(typeof(MultiLangStringDatabase))]
		public class MultiLangStringDatabaseEditor : Editor 
		{

			private bool showDebugData = false;

		    public override void OnInspectorGUI()
		    {
		        showDebugData = EditorGUILayout.Toggle("Show Debug Data", showDebugData);

		        if(showDebugData == true)
		        {
					DrawDefaultInspector();
				}
		    }
		}
	}
}
