using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEditor.IMGUI.Controls;

namespace UnityEditor {
	namespace Localization {
		public class StringTableEditorWindow : EditorWindow {

			//the language database that is selected
			public MultiLangStringDatabase mLanguages;

			//currently editing Language
			public SystemLanguage mCurrentLanguage;
			public SystemLanguage mCurrentReferenceLanguage;

			private SystemLanguage[] loadedLanguages;
			private string[] loadedLanguagesString;

			private string mReferenceTextValue;
			private string mTextValue;

			private bool mShowReference;
			private bool mShowComment;

			private int newKeyIncrementer;

			private SplitterState mHorizontalSplitterState;
			private SplitterState mVerticalSplitterState;

			private StringTableListView m_StringTableListView;

			private SearchField m_SearchField;

			private bool m_isInitialized;

			public class Styles {

				public readonly int kToolbarHeight = 20;
				public readonly int kEditorPaneHeight = 400;
			
			}

			static Styles s_Styles;

			//GUI text information
			private const string windowTitle = "Localization";
			private const string selectLanguageFile = "Select A Language To Edit";
			private const string commentButtonTitle = "Comment";
			private const string referenceButtonTitle = "Reference";
			private const string importFromPOTitle = "Import from .po ...";
			private const string exportFromPOTitle = "Export to .po ...";
			private const string exportEnableReferenceTitle = "To export, enable Reference";
			private const string referenceTextTitle = "Reference Text";


			[MenuItem("Window/Localization %r")]
			static void ShowEditor() {

				//create the editor window
				StringTableEditorWindow editor = EditorWindow.GetWindow<StringTableEditorWindow>();
				//the editor window must have a min size
				editor.titleContent = new GUIContent(windowTitle);
				editor.minSize = new Vector2(600,400);
				//call the init method after we create our window
				editor.Init();

			}

			public static void RepaintEditor() {
				StringTableEditorWindow editor = EditorWindow.GetWindow<StringTableEditorWindow>();
				editor.DetectLanguageFileFromSelection ();
				editor.Repaint();
			}

			public static void SelectItemForKey(string key, bool reload) {
				StringTableEditorWindow editor = EditorWindow.GetWindow<StringTableEditorWindow>();
				editor.DetectLanguageFileFromSelection ();
				editor._SelectItemForKey(key, reload);
				editor.Repaint();
			}

			private void _SelectItemForKey(string key, bool reload) {
				if (m_StringTableListView != null) {
					if(reload) {
					//	m_StringTableListView.ReloadTree();
					}
					//m_StringTableListView.SelectItemForKey(key);
				}
			}

			// Use this for initialization
			public void Init() 
			{
				if( m_StringTableListView == null ) 
					m_isInitialized = false;

				if(m_isInitialized)
					return;

				if( m_StringTableListView == null ) 
				{
					m_StringTableListView = new StringTableListView(mLanguages);
					m_SearchField = new SearchField ();
					m_SearchField.downOrUpArrowKeyPressed += m_StringTableListView.SetFocusAndEnsureSelectedItem;
				}

				if( mHorizontalSplitterState == null ) 
				{
					mHorizontalSplitterState = new SplitterState(new int[] { 200, 100 }, null, null);
				}

				if( mVerticalSplitterState == null )
				{
					mVerticalSplitterState = new SplitterState(new int[] { 200, 100 }, null, null);
				}

				ResetEditorStatus();

				m_isInitialized = true;
			}

			public void ResetEditorStatus() 
			{
				//mCurrentLanguage = EditorSettings.editorPreviewLanguage;

				loadedLanguages = null;
				loadedLanguagesString = null;

				mReferenceTextValue = string.Empty;
				mTextValue = string.Empty;
			}

			void OnEnable ()
			{
				Undo.undoRedoPerformed += UndoRedoCallback;
				Init ();
				DetectLanguageFileFromSelection ();
			}

			void OnDisable()
			{
				Undo.undoRedoPerformed -= UndoRedoCallback;
			}

			public void UndoRedoCallback()
			{
				m_StringTableListView.Reload();
			}
				
			void OnGUI()
			{
				Init();

				if(s_Styles == null) {
					s_Styles = new Styles();
				}

				//draw the main area 
				GUILayout.BeginArea(new Rect(0,0, position.width, position.height));

				//check if the current language is loaded or not
				if(mLanguages != null)
				{
					Rect menubarRect   = new Rect(0,0,position.width, s_Styles.kToolbarHeight);

					//the menu that will always display on top
					DoLanguageMenuBar(menubarRect);

					// Do layouting
					if( mShowComment ) {
						SplitterGUILayout.BeginHorizontalSplit(mHorizontalSplitterState, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
						GUILayout.BeginVertical ();
					}
					SplitterGUILayout.BeginVerticalSplit(mVerticalSplitterState, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
					GUILayout.BeginHorizontal ();
					GUILayout.EndHorizontal ();
					SplitterGUILayout.EndVerticalSplit();
					if( mShowComment ) {
						GUILayout.EndVertical ();
						SplitterGUILayout.EndHorizontalSplit();
					}

					// do split here
					int editorWidth = (mShowComment) ? (int)mHorizontalSplitterState.realSizes[0] : (int)position.width;
					int commentWidth = (int)mHorizontalSplitterState.realSizes[1];

					int listViewHeight = (int)mVerticalSplitterState.realSizes[0];
					int editorHeight = (int)mVerticalSplitterState.realSizes[1];

					Rect leftPaneRect_listview = new Rect(0, s_Styles.kToolbarHeight, editorWidth, listViewHeight - s_Styles.kToolbarHeight);
					Rect leftPaneRect_editorview = new Rect(0, leftPaneRect_listview.height + s_Styles.kToolbarHeight, editorWidth, editorHeight);

					Rect rightPaneRect = new Rect(editorWidth, s_Styles.kToolbarHeight, commentWidth, position.height - s_Styles.kToolbarHeight);


					//display keys and values
					DoLanguageKeyValueListView(leftPaneRect_listview);

					//EditorGUI.DrawRect(leftPaneRect_listview, Color.yellow);

					//display edit field
					DoLanguageKeyValueEditor(leftPaneRect_editorview);

					//display edit field
					if( mShowComment ) {
						DoRightSideView(rightPaneRect);
					}

					//remove notification if we were displaying one
					RemoveNotification();
				}
				else
				{
					//the language is not loaded
					ShowNotification(new GUIContent(selectLanguageFile)); 
				}

				GUILayout.EndArea();

			}

			private void DoRightSideView(Rect paneRectSize)
			{
				Assert.IsNotNull(mLanguages);

		        GUILayout.BeginArea(paneRectSize, EditorStyles.helpBox);
		        
		        GUILayout.Label("comment displays here", "HelpBox");
		        GUILayout.TextArea("write a new comment here. When Saved, this will overwrite the above comment", GUILayout.Height(100));
		        
		        GUILayout.EndArea();

			}

			private void DoLanguageKeyValueListView(Rect paneRectSize)
			{
				Assert.IsNotNull(mLanguages);

				if (m_StringTableListView != null) {
					m_StringTableListView.OnGUI(paneRectSize);
				}
			}

			private void DoLanguageKeyValueEditor(Rect paneRectSize)
			{
				Assert.IsNotNull(mLanguages);

				Rect buttonAddRect = paneRectSize;

				buttonAddRect.height = 20;
				paneRectSize.yMin += buttonAddRect.height; 

				//EditorGUI.DrawRect(buttonAddRect, Color.blue);

				GUILayout.BeginArea(buttonAddRect);
				GUILayout.Space(2);
				DoAddRemoveKeys();
				GUILayout.EndArea();

				buttonAddRect.height = 1.0f;
				EditorGUI.DrawRect(buttonAddRect, Color.gray);

				GUILayout.BeginArea(paneRectSize, EditorStyles.helpBox);

				GUILayout.Space(2);

				int keySelected  = m_StringTableListView.HasSelection() ? m_StringTableListView.GetSelection()[0] : -1;

				string key = string.Empty;
				bool isValidKeySelected = mLanguages.Count > keySelected && keySelected >= 0;
				if(isValidKeySelected) {
					key = mLanguages[keySelected];
					mReferenceTextValue = mLanguages[mCurrentReferenceLanguage].values[keySelected].text;
					mTextValue = mLanguages[mCurrentLanguage].values[keySelected].text;
				}

				GUILayout.BeginHorizontal();
				GUILayout.Label("Key", EditorStyles.boldLabel);
				GUI.changed = false;
				string newKey = EditorGUILayout.TextField(key);
				if(GUI.changed) {
					mLanguages.RenameTextEntryKey(key, newKey);
					SetDatabaseDirty();
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginVertical();

				if( mShowReference ) {
					GUILayout.Label(referenceTextTitle, EditorStyles.boldLabel);

					using (new EditorGUI.DisabledScope(true)) {
						mReferenceTextValue = EditorGUILayout.TextArea(mReferenceTextValue, GUILayout.Height(40));
					}
				}

				GUILayout.Label("Text", EditorStyles.boldLabel);

				EditorGUI.BeginChangeCheck();
				mTextValue = EditorGUILayout.TextArea(mTextValue, GUILayout.Height(60));
				if(EditorGUI.EndChangeCheck()) 
				{
					Undo.RecordObject(mLanguages, "Edit Text");
					mLanguages.SetTextEntry(mCurrentLanguage, key, mTextValue);
					SetDatabaseDirty();
				}
					
				GUILayout.EndVertical();

				EditorGUILayout.Space();

				GUILayout.EndArea();
			}

			private void SetDatabaseDirty() {

				EditorUtility.SetDirty(mLanguages);

				if(m_StringTableListView != null)
					m_StringTableListView.Reload ();

				InspectorWindow.RepaintAllInspectors();

//				IStringDatabaseObserver[] observers = FindObjectsOfType(typeof(IStringDatabaseObserver)) as IStringDatabaseObserver[];
//				foreach(IStringDatabaseObserver o in observers) {
//					o.NotifyDataChange();
//				}
			}

			//create the top menu bar
			private void DoLanguageMenuBar (Rect menubarRect)
			{
				GUILayout.BeginArea(menubarRect, EditorStyles.toolbar);

				//the menu bar 
				GUILayout.BeginHorizontal();

				//display the origin language selection
				DoLanguagePopup();

				//select the language you want to display
				DoReferenceLanguagePopup();

				GUILayout.FlexibleSpace();

				m_StringTableListView.searchString = m_SearchField.OnToolbarGUI (m_StringTableListView.searchString);

				DoShowCommentButton();

				DoContextMenuButton();

				GUILayout.EndHorizontal();

				GUILayout.EndArea();
			}

			private void DoAddRemoveKeys()
			{
				GUILayout.BeginHorizontal();

				if(GUILayout.Button(" ", "OL Plus", GUILayout.Width(20)))
				{
					Undo.RecordObject(mLanguages, "Added new key");
					string newKeyName = "new_key_" + (++newKeyIncrementer);
					while(mLanguages.ContainsKey(newKeyName)) 
					{
						newKeyName = "new_key_" + (++newKeyIncrementer);
					}

					mLanguages.AddTextEntry(newKeyName);
					m_StringTableListView.Reload();
					m_StringTableListView.SetSelection(new []{ mLanguages.Count - 1 });
				}

				if(m_StringTableListView.HasSelection() && GUILayout.Button(" ", "OL Minus", GUILayout.Width(80)))
				{
					Undo.RecordObject(mLanguages, "Remove key");
					mLanguages.RemoveTextEntry(mLanguages[m_StringTableListView.GetSelection()[0]]);
					m_StringTableListView.Reload();
				}

				GUILayout.EndHorizontal();
			}

			private void DoContextMenuButton ()
			{
				if(GUILayout.Button("▼", EditorStyles.toolbarButton, GUILayout.Width(20)))
				{
					GenericMenu pm = new GenericMenu();

					pm.AddItem(new GUIContent(importFromPOTitle), false, PerformImportLanguageFile);

					if( mShowReference ) {
						pm.AddItem(new GUIContent(exportFromPOTitle), false, PerformExportLanguageFile);
					} else {
						pm.AddDisabledItem (new GUIContent(exportFromPOTitle));
						pm.AddSeparator("/");
						pm.AddDisabledItem (new GUIContent(exportEnableReferenceTitle));
					}


					pm.ShowAsContext();
				}
			}

			private void PerformImportLanguageFile ()
			{
				//start parsing the PO File
				//get the file path
				var newPath = EditorUtility.OpenFilePanel(
					"Select PO file",
					"",
					"po");
				if(newPath.Length == 0) {
					return;
				}

				//start importing the file
				POUtility.ImportFile(mLanguages, newPath, mCurrentReferenceLanguage, false);

				SetDatabaseDirty();
			}

			//exports the language translations to a .po file
			private void PerformExportLanguageFile ()
			{
				//check if the mLanguages file is null
				Assert.IsNotNull(mLanguages);

				//prompt user to export the file at a location
				var path = EditorUtility.SaveFilePanel(
						"Save Language as .po",
						"",
						mCurrentLanguage.ToString() + ".po",
						"po");
				//check if the file path is zero 
				if(path.Length == 0) {
					return;
				}

				//get the current language selected
				POUtility.ExportFile(mLanguages, mCurrentLanguage, mCurrentReferenceLanguage, path);
			}

			public void DoLanguagePopup()
			{
				if(mLanguages == null)
				{
					string[] nodata = {""};
					EditorGUILayout.Popup(0, nodata, EditorStyles.toolbarPopup);
					return;
				}

				if( loadedLanguages == null || loadedLanguages.Length != mLanguages.languageCount ) {
					loadedLanguages = mLanguages.languages;
					loadedLanguagesString = new string[loadedLanguages.Length];
					for(int i = 0; i < loadedLanguages.Length; ++i) {
						loadedLanguagesString[i] = loadedLanguages[i].ToString();
					}
				}

				int selectionIndex = 0;
				for(int i = 0; i < loadedLanguages.Length; ++i) {
					if(loadedLanguages[i] == mCurrentLanguage) {
						selectionIndex = i;
					}
				}

				// Select the language you want to display
				selectionIndex = EditorGUILayout.Popup(selectionIndex, loadedLanguagesString, EditorStyles.toolbarPopup, GUILayout.Width(100));
				mCurrentLanguage = loadedLanguages[selectionIndex];
				m_StringTableListView.CurrentEditingLanguage = mCurrentLanguage;
			}

			public void DoShowCommentButton() {
				mShowComment = GUILayout.Toggle(mShowComment, commentButtonTitle, EditorStyles.toolbarButton, GUILayout.Width(60));
			}

			public void DoReferenceLanguagePopup()
			{
				if(mLanguages == null)
				{
					string[] nodata = {""};
					EditorGUILayout.Popup(0, nodata, EditorStyles.toolbarPopup);
					return;
				}

				mShowReference = GUILayout.Toggle(mShowReference, referenceButtonTitle, EditorStyles.toolbarButton, GUILayout.Width(60));
				if( mShowReference ) {
					int selectionIndex = 0;
					for(int i = 0; i < loadedLanguages.Length; ++i) {
						if(loadedLanguages[i] == mCurrentReferenceLanguage) {
							selectionIndex = i;
						}
					}

					//select the language you want to display
					selectionIndex = EditorGUILayout.Popup(selectionIndex, loadedLanguagesString, EditorStyles.toolbarPopup, GUILayout.Width(100));
					mCurrentReferenceLanguage = loadedLanguages[selectionIndex];
					m_StringTableListView.ReferenceLanguage = mCurrentReferenceLanguage;
				}
			}

			public void DetectLanguageFileFromSelection ()
			{
				MultiLangStringDatabase selectedAsset = null;

				if (Selection.activeObject is MultiLangStringDatabase && EditorUtility.IsPersistent(Selection.activeObject))
				{
					selectedAsset = Selection.activeObject as MultiLangStringDatabase;
				}

				if (Selection.activeGameObject)
				{
					IStringDatabaseObserver observer = Selection.activeGameObject.GetComponent<IStringDatabaseObserver>();
					if (observer != null)
					{
						selectedAsset = observer.database;
					    m_StringTableListView.SetSelection(new [] { observer.database.IndexOfKey(observer.key) } );
					}
				}
						
				if (selectedAsset != null && selectedAsset != mLanguages)
				{
					ResetEditorStatus();
					mLanguages = selectedAsset;
					if (m_StringTableListView != null) {
						m_StringTableListView.Database = selectedAsset;
					}
				}
			}

			public void OnFocus ()
			{
				DetectLanguageFileFromSelection();
			}


			public void OnProjectChange ()
			{
				DetectLanguageFileFromSelection ();
			}

			public void OnSelectionChange ()
			{
				DetectLanguageFileFromSelection();
				Repaint();
			}

			// public void OnLostFocus () 
			// {
			// 	EndRenaming();
			// }

			// void PlaymodeChanged()
			// {
			// 	if (mLanguages != null)
			// 	{
			// 		Repaint();
			// 	}

			// 	EndRenaming();
			// }

			// void EndRenaming()
			// {
			// 	if (m_StringTableListView != null) {
			// 		m_StringTableListView.EndRenaming();
			// 	}
			// }

			void OnProjectChanged ()
			{
				if (m_StringTableListView == null)
					Init ();
				else
					m_StringTableListView.Reload ();
			}
		}
	}
}
