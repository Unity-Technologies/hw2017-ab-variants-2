using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine.Localization
{
	[System.Serializable]
	public class MultiLangString {

		[System.Serializable]
		public class MultiLangPuralString {
			public string value;
			public long rangeMin;
			public long rangeMax;

			public MultiLangPuralString(string v, long rMin, long rMax) {
				value = v;
				rangeMin = rMin;
				rangeMax = rMax;
			}
		}

		[SerializeField]
		private string m_value;

		[SerializeField]
		private string[] m_Comments;

		[SerializeField]
		private List<MultiLangPuralString> m_pluralValues;

		public MultiLangString() {
			m_value = string.Empty;
		}

		public MultiLangString(string value) {
			m_value = value;
		}

		public string text {
			get {
				return m_value;
			}
			set {
				m_value = value;
			}
		}

		public string[] comments
		{
			get{ return m_Comments; }
			set{ m_Comments = value; }
		}

		public List<MultiLangPuralString> plurals {
			get {
				return m_pluralValues;
			}
		}

		public void AddPluralString(string v, long rMin, long rMax) {
			if(m_pluralValues == null) {
				m_pluralValues = new List<MultiLangPuralString>();
			}
			m_pluralValues.Add(new MultiLangPuralString(v, rMin, rMax));
		}

		public string GetPluralString(long n) {
			MultiLangPuralString s = m_pluralValues.Find( i => ( n >= i.rangeMin && n < i.rangeMax ) );
			if(s != null) {
				return s.value;
			} 
			return null;
		}

		public string Format(object[] args) {
			return string.Format(m_value, args);
		}

		public string PluralFormat(long n) {
			string s = GetPluralString(n);
			if(s != null) {
				return string.Format(s, n);
			}
			return null;
		}
	}
}