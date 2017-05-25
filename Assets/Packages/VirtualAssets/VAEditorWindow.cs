using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class VAEditorWindow : EditorWindow {
    
    [SerializeField] private string m_cacheDir;
    
    [MenuItem("Window/VAEditor/Open Virtual Asset Editor...", false, 11)]
    public static void Open () {
        GetWindow<VAEditorWindow>();
    }
    
    [System.Serializable]
    public class VAEntry {
        public string tag;
        public UnityEngine.Object obj;
        
        public VAEntry(string t, UnityEngine.Object o) {
            tag = t;
            obj = o;
        }
    }
    
    [SerializeField] UnityEngine.Object m_activeObject;
    [SerializeField] List<VAEntry> m_setting;
    [SerializeField] string m_activeTag;
    
    public void HandleSelectionChange ()
    {
        if (Selection.activeObject != m_activeObject) {
            if (Selection.activeObject != null) {
                if (EditorUtility.IsPersistent (Selection.activeObject)) {
                    SelectObject (Selection.activeObject);
                }
            } else {
                m_activeObject = null;
            }
        }         
    }
    
    public void OnFocus () {
        HandleSelectionChange();
    }
    
    public void OnLostFocus() {
    }
    
    private void SelectObject(UnityEngine.Object obj) {
        m_activeObject = obj;
        
        m_setting.Clear ();
        
        var tags = VirtualAssetManager.GetAllTagsForObject (obj);
        foreach (var t in tags) {
            m_setting.Add(new VAEntry(t, VirtualAssetManager.GetRemappedObjectForTag(obj, t)));
        }
    }
    
    private void ApplyVASetting () {
        
        VirtualAssetManager.RemoveAllVariantMappingForObject (m_activeObject);
        
        foreach (var t in m_setting) {
            VirtualAssetManager.AddVariantMapping (t.tag, m_activeObject, t.obj);
        }
    }
    
    public void OnProjectChange() {
        HandleSelectionChange ();
        Repaint();
    }
    
    public void OnSelectionChange ()
    {
        HandleSelectionChange();
        Repaint();
    }
    
    private void Init() {
        this.titleContent = new GUIContent("VAEditor");
        this.minSize = new Vector2(300f, 100f);
        this.maxSize = new Vector2(1000f, 400f);
        m_setting = new List<VAEntry> ();
        m_activeTag = VirtualAssetManager.activeTag;
    }
    
    public void OnEnable () {
        Init();
    }
    
    public void OnDisable() {
    }
    
    public void OnGUI () {
        
        using (new EditorGUILayout.VerticalScope()) {
            
            GUILayout.Label("VA Setting", new GUIStyle("BoldLabel"));
            GUILayout.Space(8f);
            
            if (m_activeObject == null) {
                return;
            }
            
            VAEntry removing = null;
            
            foreach (var info in m_setting) {
                using (new EditorGUILayout.HorizontalScope ()) {
                    var newTag = EditorGUILayout.TextField ("Tag", info.tag);
                    if (newTag != info.tag) {
                        info.tag = newTag;
                    }
                    if (GUILayout.Button ("-", GUILayout.Width(32f))) {
                        removing = info;
                    }
                }
                var newObj = EditorGUILayout.ObjectField ("Object", info.obj, m_activeObject.GetType (), false);
                if (newObj != info.obj) {
                    info.obj = newObj;
                }
                GUILayout.Space (8f);
            }
            
            if (removing != null) {
                m_setting.Remove (removing);
            }
            
            GUILayout.Space (8f);
            if (GUILayout.Button ("+")) {
                m_setting.Add (new VAEntry ("<new entry>", null));
            }
            
            if (GUILayout.Button ("Apply")) {
                ApplyVASetting ();
            }
            
            GUILayout.Space (20f);
            
            var newActiveTag = EditorGUILayout.TextField ("Active Tag", m_activeTag);
            if (newActiveTag != m_activeTag) {
                m_activeTag = newActiveTag;
            }
            
            if (GUILayout.Button ("Update Active Tag")) {
                VirtualAssetManager.activeTag = newActiveTag;
                VirtualAssetManager.ApplyRemap (m_activeTag);
            }
        }
    }
}
