using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace UnityEngine.Variant
{
    [System.Serializable]
    public class Variant {
        [SerializeField] private string m_name;
        [SerializeField] private string m_shortName;
        [SerializeField] private bool m_isReadOnly;

        public Variant(string name, string shortName, bool isReadOnly) {
            m_name = name;
            m_shortName = shortName;
            m_isReadOnly = isReadOnly;
        }

        public string Name {
            get {
                return m_name;
            }
            set {
                Assertions.Assert.IsFalse (m_isReadOnly);
                if (!m_isReadOnly) {
                    m_name = value;
                }
            }
        }

        public string ShortName {
            get {
                return m_shortName;
            }
        }
        public bool IsReadOnly {
            get {
                return m_isReadOnly;
            }
        }
    }

    [System.Serializable]
    public class VariantAxis {
        [SerializeField] private string m_name;
        [SerializeField] private List<Variant> m_variants;
        [SerializeField] private bool m_isReadOnly;

        public VariantAxis(string name, bool isReadOnly = false) {
            m_name = name;
            m_variants = new List<Variant> ();
            m_isReadOnly = isReadOnly;
        }

        public List<Variant> Variants {
            get {
                return m_variants;
            }
        }

        public string Name {
            get {
                return m_name;
            }
            set {
                Assertions.Assert.IsFalse (m_isReadOnly);
                if (!m_isReadOnly) {
                    m_name = value;
                }
            }
        }

        public bool IsReadOnly {
            get {
                return m_isReadOnly;
            }
        }

        public Variant AddVariant(string name) {
            var newV = new Variant (name, CreateShortName(m_name), m_isReadOnly);
            m_variants.Add (newV);
            return newV;
        }

        public void RemoveVariant(Variant v) {
            m_variants.Remove (v);
        }

        private string CreateShortName(string name) {

            int i = 0;
            var s = name.Substring (0, 1).ToLower();
            while( m_variants.Where(v => v.ShortName == s).Any() ) {
                s = string.Format ("{0}{1}", s, ++i);
            }

            return s;
        }
    }


    public class VariantOperation : ScriptableObject {

        [SerializeField] private string m_activeTag;
        [SerializeField] private List<VariantAxis> m_axis;
        [SerializeField] private List<ObjectVariantInfo> m_objects;

        private static readonly string kDATAPATH = "Assets/VO.asset";

        private static VariantOperation s_operation;

        public static VariantOperation GetOperation() {
            if(s_operation == null) {
                if(!Load()) {
                    // Create vanilla db
                    s_operation = ScriptableObject.CreateInstance<VariantOperation>();
                    s_operation.m_axis = CreateDefaultAxis();
                    s_operation.m_objects = new List<ObjectVariantInfo>();

                    AssetDatabase.CreateAsset(s_operation, kDATAPATH);
                }
            }

            return s_operation;
        }

        private static bool Load() {

            bool loaded = false;

            try {
                var dbPath = kDATAPATH;

                if(File.Exists(dbPath)) 
                {
                    VariantOperation op = AssetDatabase.LoadAssetAtPath<VariantOperation>(dbPath);

                    if(op != null) {
                        s_operation = op;
                        loaded = true;
                    }
                }
            } catch(System.Exception e) {
                Debug.LogException (e);
            }

            return loaded;
        }

        public static void SetDBDirty() {
            EditorUtility.SetDirty(s_operation);
        }


        public static List<VariantAxis> Axis {
            get {
                return GetOperation ().m_axis;
            }
        }

        public static List<ObjectVariantInfo> Objects {
            get {
                return GetOperation ().m_objects;
            }
        }

        private static List<VariantAxis> CreateDefaultAxis() {
            var defaultAxis = new List<VariantAxis> ();
            //TODO add lang properly
            //TODO add device profile properly
            var lang = new VariantAxis("Languages", true);
            lang.AddVariant ("Chinese");
            lang.AddVariant ("Korean");
            lang.AddVariant ("Russian");

            var dev  = new VariantAxis("Quality Settings", true);
            foreach (var name in QualitySettings.names) {
                dev.AddVariant (name);
            }

            defaultAxis.Add (lang);
            defaultAxis.Add (dev);


            return defaultAxis;
        }

        public VariantAxis CreateNewAxis(string name) {

            int i = 0;
            var newName = name;
            while (m_axis.Where (ax => ax.Name == newName).Any ()) {
                newName = string.Format ("{0} {1}", name, ++i);
            }

            var newAxis = new VariantAxis (newName, false);
            m_axis.Add (newAxis);
            SetDBDirty ();
            return newAxis;
        }

        public void RemoveVariantAxis(VariantAxis ax) {
            m_axis.Remove(ax);
            SetDBDirty ();
        }

        public void RemoveVariantFromVariantAxis(Variant v, VariantAxis ax) {
            ax.RemoveVariant (v);
            SetDBDirty ();
        }

        public ObjectVariantInfo CreateObjectVariantInfo(Object o) {
            if (!m_objects.Where (oi => oi.SrcObject == o).Any ()) {
                var info = new ObjectVariantInfo (o);
                m_objects.Add (info);
                SetDBDirty ();
                return info;
            }
            return m_objects.Find (oi => oi.SrcObject == o);
        }

        public void RemoveObjectVariantInfo(Object o) {
            var info = m_objects.Find (oi => oi.SrcObject == o);
            if (info != null) {
                m_objects.Remove (info);
                SetDBDirty ();
            }
        }

        public UnityEngine.Object[] GetSourceObjectsContainsVariantOf(string tagName) {
            return new UnityEngine.Object[]{};
        }

        public UnityEngine.Object GetRemappedObject(UnityEngine.Object src) {
            return null;
        }

        public UnityEngine.Object GetRemappedObjectForTag(UnityEngine.Object src, string tagName ) {
            return null;
        }

        public string[] GetAllTagsForObject(UnityEngine.Object src) {
            return new string[]{};
        }

        public void AddVariantMapping(string tagName, UnityEngine.Object src, UnityEngine.Object dst) {
            Debug.Log (string.Format("Add {0} => {1}: {2}", src.name, dst.name, tagName));
        }

        public void RemoveVariantMapping(string tagName, UnityEngine.Object src) {
            Debug.Log (string.Format("Remove {0} for {1}", src.name, tagName));
        }
        public void RemoveAllVariantMappingForTag(string tagName) {
            Debug.Log (string.Format("Remove ALL TAG {0}", tagName));
        }
        public void RemoveAllVariantMappingForObject(UnityEngine.Object src) {
            Debug.Log (string.Format("Remove ALL {0}", src.name));
        }
        public void RemoveVariantMapping () {
            Debug.Log (string.Format("ALL REMOVED"));
        }

        public void ApplyRemap(string tag) {
            Debug.Log (string.Format("APPLYED"));
        }
    }
}
