using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UnityEngine.Localization
{
	public interface IStringDatabaseObserver {

		MultiLangStringDatabase database { get; }
		string key { get; }

		void NotifyDataChange();
	}


	[CreateAssetMenuAttribute( fileName = "Languages", menuName = "Localization/Languages", order = 400)]
	public class MultiLangStringDatabase : ScriptableObject {

		
		[SerializeField]
		private List<string> m_keys;

		// TODO: make it flat
		[SerializeField]
		private MultiLangStringTable[] m_database;

		public string this[int i] {
			get {
				return m_keys[i];
			}
			set {
				m_keys[i] = value;
			}
		}

		public int Count {
			get {
				return m_keys.Count;
			}
		}

		public MultiLangStringTable this[SystemLanguage l] {
			get {
				return m_database[(int)l];
			}
		}

		public MultiLangStringTable current {
			get {
                //TODO
                //return null;
                // This will be supplied from editor later in hw
				int i = (int)Application.currentLanguage;
				return m_database[i];
			}
		}

		public int languageCount {
			get {
				int c = 0;
				for(int i =0; i < m_database.Length;++i) {
					if(m_database[i]!=null) ++c;
				}
				return c;
			}
		}

		public SystemLanguage[] languages {
			get {
				SystemLanguage[] l = new SystemLanguage[languageCount];
				int c = 0;
				for(int i =0; i < m_database.Length;++i) {
					if(m_database[i]!=null){ 
						l[c++] = (SystemLanguage)i;
					}
				}
				return l;
			}
		}

		void OnEnable() {
			if( m_database == null ) {
				int langMax = 0; 
				Array a = Enum.GetValues(typeof(SystemLanguage));
				foreach(object o in a) {
					int v = (int)o;
					langMax = UnityEngine.Mathf.Max(v, langMax);
				}
				m_database = new MultiLangStringTable[langMax];

				for(int i=0; i<m_database.Length; ++i) {
					m_database[i] = new MultiLangStringTable((SystemLanguage)i);
				}
			}

			if( m_keys == null )
			{
				m_keys = new List<string>();
			}
		}

		public bool ContainsKey(string key) {
            return m_keys.Contains(key);
		}

		public int IndexOfKey(string key) {
            return m_keys.IndexOf(key);
		}

		public void AddLanguage( SystemLanguage lang ) {
			if( m_database[(int)lang] == null ) {
				m_database[(int)lang] = new MultiLangStringTable(lang);
			}
		}

		public void RemoveLanguage( SystemLanguage lang ) {
			m_database[(int)lang] = null;
		}

	//	public MultiLangStringTable GetStringTable( SystemLanguage lang ) {
	//		return m_database[(int)lang];
	//	}
	//
	//	public MultiLangStringTable GetStringTable() {
	//		return m_database[(int)Application.currentLanguage];
	//	}

		public void AddTextEntry(string key) {
			Assert.IsNotNull(key);

			if( !m_keys.Contains(key) ) {
				m_keys.Add(key);
			}

			for(int i = 0; i < m_database.Length; ++i) {

				if( m_database[i] == null ) {
					continue;
				}
				m_database[i].EnsureValuesForKeys(m_keys.Count);
			}
		}

		public void SetTextEntry(SystemLanguage lang, string key, string value, string[] comments = null) {
			Assert.IsNotNull(key);
			Assert.IsNotNull(value);
			Assert.IsNotNull(m_database[(int)lang]);

			int index = 0;

			if( !m_keys.Contains(key) ) {
				index = m_keys.Count;
				m_keys.Add(key);
			} else {
				index = m_keys.IndexOf(key);
			}

			for(int i = 0; i < m_database.Length; ++i) {

				if( m_database[i] == null ) {
					continue;
				}
				m_database[i].EnsureValuesForKeys(m_keys.Count);
			}

			m_database[(int)lang].values[index].text = value;
			m_database [(int)lang].values[index].comments = comments;
		}

		public void RemoveTextEntry(string key) {
			Assert.IsNotNull(key);

			if( !m_keys.Contains(key) ) {
				return;
			}

			int index = m_keys.IndexOf(key);
			m_keys.RemoveAt(index);

			for(int i = 0; i < m_database.Length; ++i) {

				if( m_database[i] == null ) {
					continue;
				}
				m_database[i].values.RemoveAt(index);
			}
		}

		public void RenameTextEntryKey( string oldKey, string newKey ) {
			Assert.IsNotNull(oldKey);
			Assert.IsNotNull(newKey);

			if( !m_keys.Contains(oldKey) ) {
				return;
			}

			int index = m_keys.IndexOf(oldKey);

			m_keys[index] = newKey;
		}
	}
}