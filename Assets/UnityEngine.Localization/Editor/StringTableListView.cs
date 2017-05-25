using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Audio;
using Object = UnityEngine.Object;
using UnityEditor.IMGUI.Controls;

namespace UnityEditor
 {
	namespace Localization
	{
		public class StringTableListView : TreeView
		{
			MultiLangStringDatabase m_Database;
			SystemLanguage m_ReferenceLanguage = SystemLanguage.Unknown;
			
			const string k_DragId = "StringTableListViewDragging";

			public MultiLangStringDatabase Database
			{
				get { return m_Database; }
				set 
				{
					m_Database = value;
					Reload();
				}
			}

			public SystemLanguage CurrentEditingLanguage { get; set; }

			public SystemLanguage ReferenceLanguage 
			{ 
				get{ return m_ReferenceLanguage; }
				set
				{
					m_ReferenceLanguage = value; 
				}
			}

			public StringTableListView(MultiLangStringDatabase db) 
				: base(new TreeViewState())
			{
				showAlternatingRowBackgrounds = true;

				MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[3];
				for (int i = 0; i < columns.Length; ++i)
				{
					columns[i] = new MultiColumnHeaderState.Column();
					columns[i].headerTextAlignment = TextAlignment.Center;
					columns[i].canSort = false;
				}
				columns[0].headerContent = new GUIContent("Source Text");
				columns[1].headerContent = new GUIContent("Translation");
				columns[2].headerContent = new GUIContent("Reference");
				var multiColState = new MultiColumnHeaderState(columns);
				multiColumnHeader = new MultiColumnHeader(multiColState);
				multiColumnHeader.ResizeToFit();

				Database = db;
			}

            protected override TreeViewItem BuildRoot()
            {	
				var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
				
				var allItems = new List<TreeViewItem>();
				if(m_Database != null)
				{
					for (int i = 0; i < m_Database.Count; ++i)
					{
						var item = new TreeViewItem(i, 0, m_Database[i]);
						allItems.Add(item);
					}
				}
				
				SetupParentsAndChildrenFromDepths(root, allItems);
			
				return root;
            }

			protected override void RowGUI(RowGUIArgs args)
			{
				for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
				{
					CellGUI(args.GetCellRect(i), args.item, args.GetColumn(i));
				}
			}

			protected void CellGUI(Rect cellRect, TreeViewItem item, int col)
			{
				CenterRectUsingSingleLineHeight(ref cellRect);
				switch(col)
				{
					case 0:
					EditorGUI.LabelField(cellRect, m_Database[item.id]);
					break;

					case 1: 
					{
						var translationText = string.Empty;
						var language = m_Database[CurrentEditingLanguage];
						if(language.values != null && item.id < language.values.Count)
							translationText = language.values[item.id].text;
						EditorGUI.LabelField(cellRect, translationText);
					}
					break;

					case 2:
					{
						var translationText = string.Empty;
						if(ReferenceLanguage != SystemLanguage.Unknown)
						{
							var language = m_Database[ReferenceLanguage];
							if(language.values != null && item.id < language.values.Count)
								translationText = language.values[item.id].text;
						}
						EditorGUI.LabelField(cellRect, translationText);
					}
					break;
				}
			}
		}
	}
}