﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine.Localization
{
	[System.Serializable]
	public struct MultiLangStringReference {

		[SerializeField]
		private MultiLangStringDatabase m_db;

		[SerializeField]
		private int m_index;

		[SerializeField]
		private string m_selectedKey;

		private object[] m_args;

		// TODO: Editor Only
	//	[SerializeField]
	//	private string m_key;

		public MultiLangStringReference(MultiLangStringDatabase db, ref string key) {
			m_db = db;
			Assert.IsTrue(m_db.ContainsKey(key));
			m_index = m_db.IndexOfKey(key);
			m_selectedKey = key;
			m_args = null;
			// TODO: register delegate event to keep track of db change (if editor)
		}

		public string text {
			get {
				if(m_db == null || m_db.Count < m_index || m_index < 0) return string.Empty;

				if( m_args != null ) {
					return m_db.current.values[m_index].Format(args);
				} else {
					return m_db.current.values[m_index].text;
				}
			}
		}

		public object[] args {
			get {
				return m_args;
			}
			set {
				m_args = value;
			}
		}

		public MultiLangStringDatabase database {
			get {
				return m_db;
			}
			set {
				m_db = value;
			}
		}

		public string key {
			get {
				if(m_db == null || m_db.Count < m_index || m_index < 0) return string.Empty;
				return m_db[m_index];
			}
			set {
				Assert.IsTrue(m_db.ContainsKey(value));
				m_index = m_db.IndexOfKey(value);
				m_selectedKey = value;
			}
		}

		public string selectedKey {
			get {
				return m_selectedKey;
			}
		}

		public void SetArgs(params object[] args) {
			this.args = args;
		}

		public string GetPluralString(long n) {
			return m_db.current.values[m_index].GetPluralString(n);
		}

		public string Format(object[] args) {
			return m_db.current.values[m_index].Format(args);
		}

		public string PluralFormat(long n) {
			return m_db.current.values[m_index].PluralFormat(n);
		}
	}
}