using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Localization
{
    [System.Serializable]
    public class LocalizedString : IStringDatabaseObserver
    {
        string m_ReferenceString;
        int m_ReferenceStringHash; // Use this for faster lookups.
        string m_LocalizedString;

        public UnityEngine.Events.UnityEvent textUpdated;

        public SystemLanguage Language { get { return Application.currentLanguage; } }

        public string key
        {
            get { return m_ReferenceString; }
            set
            {
                int hash = value.GetHashCode();
                if (hash != m_ReferenceStringHash)
                {
                    m_ReferenceString = value;
                    m_ReferenceStringHash = hash;
                    NotifyDataChange();
                }
            }
        }

        public string Text { get{ return m_LocalizedString; } }

        public MultiLangStringDatabase database
        {
            get
            {
                // TODO: Should probabaly make the database a player setting.
                return null;
            }
        }

        public LocalizedString(string referenceString)
    	{
    	}

        public void NotifyDataChange()
        {
            if(textUpdated != null)
                textUpdated.Invoke();
        }
    }
}