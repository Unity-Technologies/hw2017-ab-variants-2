using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using System.IO;
using System.Text;

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

    public enum AxisType {
        Language,
        QualitySetting,
        Other
    }

    [System.Serializable]
    public class VariantAxis {
        [SerializeField] private string m_name;
        [SerializeField] private List<Variant> m_variants;
        [SerializeField] private AxisType m_type;
        private Variant m_currentSelection = null;

        public VariantAxis(string name, AxisType atype) {
            m_name = name;
            m_variants = new List<Variant> ();
            m_type = atype;
            m_currentSelection = null;
        }

        public void ResetSelection() {
            m_currentSelection = null;
            VariantOperation.GetOperation ().ApplyRemap ();
        }

        public void SelectVariant(Variant v) {
            m_currentSelection = v;
            VariantOperation.GetOperation ().ApplyRemap ();
        }

        public Variant CurrentSelection {
            get {
                return m_currentSelection;
            }
        }

        public List<Variant> Variants {
            get {
                if(m_type == AxisType.Language) {
                    UpdateForLanguage ();
                }
                else if(m_type == AxisType.QualitySetting) {
                    UpdateForQualitySettings ();
                }
                return m_variants;
            }
        }

        public string Name {
            get {
                return m_name;
            }
            set {
                Assertions.Assert.IsFalse (IsReadOnly);
                if (!IsReadOnly) {
                    m_name = value;
                }
            }
        }

        public bool IsReadOnly {
            get {
                return m_type != AxisType.Other;
            }
        }

        public Variant AddVariant(string name) {
            var newV = new Variant (name, CreateShortName(m_name), IsReadOnly);
            m_variants.Add (newV);
            return newV;
        }

        public void RemoveVariant(Variant v) {
            m_variants.Remove (v);
        }

        public void UpdateForLanguage() {
            var languages = Application.supporingLanguages;
            var removingItem = new List<Variant> ();
//            foreach (var v in m_variants) {
//                if (System.Array.Find (languages, l => l == v.Name) == null) 
//                {
//                    removingItem.Add (v);
//                }
//            }
//            removingItem.ForEach (r => RemoveVariant(r));

            foreach (var l in languages) {
                var variant = m_variants.Find (v => v.Name == l.ToString());
                if (variant == null) {
                    AddVariant (l.ToString());
                }
            }
        }

        public void UpdateForQualitySettings() {
            var names = QualitySettings.names;
            var removingItem = new List<Variant> ();
//            foreach (var v in m_variants) {
//                if (System.Array.Find (name, n => n == v.Name) == null) 
//                {
//                    removingItem.Add (v);
//                }
//            }
//            removingItem.ForEach (r => RemoveVariant(r));

            foreach (var l in names) {
                var variant = m_variants.Find (v => v.Name == l);
                if (variant == null) {
                    AddVariant (l);
                }
            }
        }

        private string CreateShortName(string name) {

            int i = 0;
            var l = name.Substring (0, 1).ToLower();
            var s = l;
            while( m_variants.Where(v => v.ShortName == s).Any() ) {
                s = string.Format ("{0}{1}", l, ++i);
            }

            return s;
        }
    }


    public class VariantOperation : ScriptableObject {

        [SerializeField] private string m_activeTag;
        [SerializeField] private List<VariantAxis> m_axis;
        [SerializeField] private List<ObjectVariantInfo> m_objects;

        private static readonly string kDATAPATH = "Assets/Resources/VO.asset";

        private static VariantOperation s_operation;

        public static VariantOperation GetOperation() {
            if(s_operation == null) {
                if(!Load()) {
                    #if UNITY_EDITOR
                    // Create vanilla db
                    s_operation = ScriptableObject.CreateInstance<VariantOperation>();
                    s_operation.m_axis = CreateDefaultAxis();
                    s_operation.m_objects = new List<ObjectVariantInfo>();

                    AssetDatabase.CreateAsset(s_operation, kDATAPATH);
                    #else
                    Debug.LogError("Failed To Load from disk...");
                    #endif
                }
            }

            return s_operation;
        }

        private static bool Load() {

            bool loaded = false;

            try {
//                var dbPath = kDATAPATH;
//
//                if(File.Exists(dbPath)) 
//                {
//                    VariantOperation op = AssetDatabase.LoadAssetAtPath<VariantOperation>(dbPath);
//
//                    if(op != null) {
//                        s_operation = op;
//                        loaded = true;
//                    }
//                }
                VariantOperation op = Resources.Load<VariantOperation>("VO");
                if(op != null) {
                    s_operation = op;
                    loaded = true;
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
            var lang = new VariantAxis("Languages", AxisType.Language);

            var dev  = new VariantAxis("Quality Settings", AxisType.QualitySetting);

            defaultAxis.Add (lang);
            defaultAxis.Add (dev);


            return defaultAxis;
        }

        void OnApplicationLanguageChanged()
        {
            Debug.Log ("[VO]Language changed to:" + Application.currentLanguage);
        }

        public VariantAxis CreateNewAxis(string name) {

            int i = 0;
            var newName = name;
            while (m_axis.Where (ax => ax.Name == newName).Any ()) {
                newName = string.Format ("{0} {1}", name, ++i);
            }

            var newAxis = new VariantAxis (newName, AxisType.Other);
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

//        public UnityEngine.Object[] GetSourceObjectsContainsVariantOf(string tagName) {
//            return new UnityEngine.Object[]{};
//        }

//        public UnityEngine.Object GetRemappedObject(UnityEngine.Object src) {
//            return null;
//        }
//
//        public UnityEngine.Object GetRemappedObjectForTag(UnityEngine.Object src, string tagName ) {
//            return null;
//        }
//
//        public string[] GetAllTagsForObject(UnityEngine.Object src) {
//            return new string[]{};
//        }

        public void AddVariantMapping(string tagName, UnityEngine.Object src, UnityEngine.Object dst) {
            VirtualAssetManager.AddVariantMapping (tagName, src, dst);
            Debug.Log (string.Format("Add {0} => {1}: {2}", src.name, dst.name, tagName));
        }

        public void RemoveVariantMapping(string tagName, UnityEngine.Object src) {
            VirtualAssetManager.RemoveVariantMapping (tagName, src);
            Debug.Log (string.Format("Remove {0} for {1}", src.name, tagName));
        }
        public void RemoveAllVariantMappingForTag(string tagName) {
            VirtualAssetManager.RemoveAllVariantMappingForTag (tagName);
            Debug.Log (string.Format("Remove ALL TAG {0}", tagName));
        }
        public void RemoveAllVariantMappingForObject(UnityEngine.Object src) {
            VirtualAssetManager.RemoveAllVariantMappingForObject (src);
            Debug.Log (string.Format("Remove ALL {0}", src.name));
        }
        public void RemoveVariantMapping () {
            VirtualAssetManager.RemoveVariantAllMapping ();
            Debug.Log (string.Format("ALL REMOVED"));
        }

        private string CreateCurrentTag() {

            if (m_axis.Find (ax => ax.CurrentSelection != null) == null) {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder ();

            string dv = "";
            foreach (var ax in VariantOperation.Axis) {
                sb.Append (dv);
                var sel = ax.CurrentSelection;
                if (sel == null) {
                    sb.Append ("*");
                } else {
                    sb.Append (sel.ShortName);
                }
                dv = "-";
            }

            return sb.ToString ();
        }

        public void SelectLanguage(SystemLanguage l) {

            var langAxis = m_axis.Find (ax => ax.Name == "Languages");
            var selected = langAxis.Variants.Find (v=> v.Name == l.ToString());
            langAxis.SelectVariant(selected);
            ApplyRemap();
        }

        public void ApplyRemap() {
            string tag = CreateCurrentTag ();
            VirtualAssetManager.activeTag = tag;
            VirtualAssetManager.ApplyRemap (tag);
            Debug.Log (string.Format("Remap => {0}", tag));
        }

        public void ResetRemap() {
            VirtualAssetManager.ApplyRemap (string.Empty);
        }
    }
}
