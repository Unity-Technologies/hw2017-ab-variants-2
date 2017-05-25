using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Localization
{
	[Serializable]
	public class MultiLangStringTable {

		[SerializeField]
		public SystemLanguage language;

		[SerializeField]
		public List<MultiLangString> values = new List<MultiLangString>();

		[SerializeField]
		public bool isEditing = false;

		public MultiLangStringTable(SystemLanguage lang) {
			language = lang;
		}

		public void EnsureValuesForKeys(int keyCount) {

			if(values.Count < keyCount) {
				while(values.Count < keyCount) {
					values.Add(new MultiLangString());
				}
			}
		}
	}
}