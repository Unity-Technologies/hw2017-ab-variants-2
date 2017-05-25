using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEditor;
using Object = UnityEngine.Object;

namespace UnityEditor {

	namespace Localization {
	
		public class StringTableKeyGUI
		{
			MultiLangStringDatabase m_db;
			PopupList.InputData m_Keys;
			string m_ChangedKey;
			bool m_CurrentChanged = false;
			bool m_ChangeWasAdd = false;
			bool m_IgnoreNextAssetLabelsChangedCall = false;

			public Action<int, string> selectionChangedDelegate;

			public void OnEnable()
			{
			}

			public void OnDisable()
			{
				SaveKey();
			}
			
			public void OnLostFocus()
			{
				SaveKey();
			}
			
			public void KeyChangedForObject(Object asset)
			{
				if (!m_IgnoreNextAssetLabelsChangedCall && m_db != null)
				{
					m_Keys = null; // someone else changed the labels for one of our selected assets, so invalidate cache
				}
				m_IgnoreNextAssetLabelsChangedCall = false;
			}
			
			public void SaveKey()
			{
				if (m_CurrentChanged && m_Keys != null && m_db != null)
				{
					int index = m_db.IndexOfKey(m_ChangedKey);
					if(index < 0) {

						Debug.Log("Adding new key:" + m_ChangedKey + " ChangeWasAdd:" + m_ChangeWasAdd);

						m_db.AddTextEntry(m_ChangedKey);
						EditorUtility.SetDirty(m_db);

						index = m_db.IndexOfKey(m_ChangedKey);
					}

					if (selectionChangedDelegate != null) {
						selectionChangedDelegate(index, m_ChangedKey);
					}

					m_CurrentChanged = false;
				}
			}

			private void KeyListSelectionCallback (PopupList.ListElement element)
			{
				m_Keys.DeselectAll();

                m_ChangedKey = element.text;
				element.selected = !element.selected;
				m_ChangeWasAdd = element.selected;
				m_CurrentChanged = true;
				SaveKey();
				InspectorWindow.RepaintAllInspectors();
			}

			public void InitKeynameCache(MultiLangStringDatabase db, string currentKey)
			{			
				// Init only if new asset
				if (m_db != db)
				{
					m_Keys = new PopupList.InputData
									{
										m_CloseOnSelection = true,
										m_AllowCustom = true,
										m_OnSelectCallback = KeyListSelectionCallback,
										m_MaxCount = 15,
										m_EnableAutoCompletion = true,
										m_SortAlphabetically = true
					                };
					m_db = db;
				}

				for (int i=0; i < db.Count; ++i)
				{
					PopupList.ListElement element = m_Keys.NewOrMatchingElement(db[i]);
	//					if ( element.filterScore < pair.Value )
	//					{
	//						element.filterScore = pair.Value;
	//					}
					element.selected = (db[i] == currentKey);
				}
			
				m_CurrentChanged = false;
			}
			
			public void OnKeyGUI(MultiLangStringDatabase db, string currentKey)
			{
				if(db == null) {
					return;
				}

				InitKeynameCache(db, currentKey);
				
				GUIStyle labelButton = "ExposablePopupMenu";

				GUILayout.FlexibleSpace();
				Rect r = GUILayoutUtility.GetRect (150, 150, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);

				if (GUI.Button(r, currentKey, labelButton))
				{
					PopupWindow.Show(r, new PopupList(m_Keys));
				}
			}
		}
	}
}
