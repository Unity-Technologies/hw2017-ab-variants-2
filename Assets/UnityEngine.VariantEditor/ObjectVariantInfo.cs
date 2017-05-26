using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using System.Text;

namespace UnityEngine.Variant
{
    [Serializable]
    public class AxisSelection
    {
        public string axName;
        public string vShortName;

        public AxisSelection() {
        }
        public AxisSelection(VariantAxis ax, Variant v) {
            axName = ax.Name;
            vShortName = v.ShortName;
        }
    }

    [Serializable]
    public class ObjectVariantDestinationInfo
    {
        [SerializeField] private UnityEngine.Object m_dstObj;
        [SerializeField] private List<AxisSelection> m_selections;

        public ObjectVariantDestinationInfo() {
            m_dstObj = null;
            m_selections = new List<AxisSelection> ();
        }

        public UnityEngine.Object DstObject {
            get {
                return m_dstObj;
            }
            set {
                m_dstObj = value;
            }
        }

        public List<AxisSelection> Selections {
            get {
                return m_selections;
            }
        }

        public string CreateTagFromSelection() {

            if (m_selections.Count == 0) {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder ();

            string dv = "";
            foreach (var ax in VariantOperation.Axis) {
                sb.Append (dv);
                var sel = m_selections.Find (s => s.axName == ax.Name);
                if (sel == null) {
                    sb.Append ("*");
                } else {
                    sb.Append (sel.vShortName);
                }
                dv = "-";
            }

            return sb.ToString ();
        }
    }


    [Serializable]
    public class ObjectVariantInfo
    {
        [SerializeField] private UnityEngine.Object m_obj;
        [SerializeField] private List<ObjectVariantDestinationInfo> m_destinations;
        [SerializeField] private List<ObjectVariantDestinationInfo> m_removing;

        private static int s_windowId = 1000;

        public static readonly float kWIDTH  = 120f;
        public static readonly float kHEIGHT = 200f;

        private Rect m_baseRect;
        private int m_index;

        public ObjectVariantInfo(UnityEngine.Object o)
        {
            m_obj = o;
            m_destinations = new List<ObjectVariantDestinationInfo> ();
            m_removing = new List<ObjectVariantDestinationInfo> ();
        }

        public UnityEngine.Object SrcObject {
            get {
                return m_obj;
            }
        }

        public static float Height {
            get {
                return kHEIGHT + VariantOperation.Axis.Count * 20f;
            }
        }

        public void SetRemapConfig(ObjectVariantDestinationInfo info) {
            if (info.DstObject != null) {
                VariantOperation.GetOperation ().AddVariantMapping (info.CreateTagFromSelection (), m_obj, info.DstObject);
            } else {
                RemoveRemapConfig (info);
            }
        }

        public void SetAllRemapConfig() {
            foreach (var d in m_destinations) {
                SetRemapConfig (d);
            }
        }

        public void RemoveRemapConfig(ObjectVariantDestinationInfo info) {
            VariantOperation.GetOperation ().RemoveVariantMapping (info.CreateTagFromSelection(), m_obj);
        }

        public void RemoveRemapConfig() {
            VariantOperation.GetOperation ().RemoveAllVariantMappingForObject (m_obj);
        }


        public void DrawObject (int index, Rect parentRect) {
            m_index = index;

            m_baseRect = new Rect(20f, 20f + (Height + 20f) * index, kWIDTH * (m_destinations.Count + 1), Height);

            GUIStyle s = GUI.skin.FindStyle("flow node 0");
            GUI.Window(index, m_baseRect, DrawThisObject, string.Empty,  s);
        }

        private void DrawThisObject(int id) {

            Rect content = new Rect (10, 10, kWIDTH-20, Height -20);

            GUILayout.BeginArea (content);
            DrawContent(content);
            GUILayout.EndArea ();

            int i = 1;
            foreach (var dst in m_destinations) {
                Rect r = new Rect(kWIDTH * i + 10, 10, content.width, content.height);
                GUILayout.BeginArea (r);
                DrawDstContent (content, dst, i++);
                GUILayout.EndArea ();
            }

            if (m_removing.Count > 0) {
                m_removing.ForEach (obj => RemoveRemapConfig(obj));
                m_removing.ForEach (obj => m_destinations.Remove(obj));
                m_removing.Clear ();
            }
        }

        private void DrawContent (Rect r) {
            var tex = AssetPreview.GetAssetPreview (m_obj);

            GUILayout.Label (tex, GUILayout.Width (100f), GUILayout.Height (100f));
            GUILayout.Label (m_obj.name, "BoldLabel", GUILayout.Width(100f));

            GUILayout.Space (8f);

            if (GUILayout.Button ("Add Variant")) {
                m_destinations.Add (new ObjectVariantDestinationInfo());
            }

            if (GUILayout.Button ("Delete Config")) {
                VariantOperation.GetOperation().RemoveObjectVariantInfo (m_obj);
            }
        }

        private void DrawDstContent (Rect r, ObjectVariantDestinationInfo dstInfo, int index) {
            var tex = AssetPreview.GetAssetPreview (dstInfo.DstObject);

            GUILayout.Label (tex, GUILayout.Width (100f), GUILayout.Height (100f));

            string name = "<null>";

            if (dstInfo.DstObject != null) {
                name = dstInfo.DstObject.name;
            } 
            GUILayout.Label (name, "BoldLabel", GUILayout.Width (100f));
            var newObj = EditorGUILayout.ObjectField (dstInfo.DstObject, m_obj.GetType (), false, GUILayout.Width (100f));
            if (newObj != dstInfo.DstObject) {
                dstInfo.DstObject = newObj;
                SetRemapConfig (dstInfo);
                VariantOperation.SetDBDirty ();
            }

            GUILayout.Space (8f);

            foreach (var ax in VariantOperation.Axis) {

                var items = ax.Variants.Select (v => v.Name).ToList ();
                items.Insert (0, "<Any>");
                var array = items.ToArray ();

                int popIndex = 0;
                AxisSelection s = dstInfo.Selections.Find(x => x.axName == ax.Name);
                if (s != null) {
                    popIndex = ax.Variants.FindIndex (v => v.ShortName == s.vShortName) + 1;
                }

                var newIndex = EditorGUILayout.Popup(popIndex, array, GUILayout.Width(100f));
                if(newIndex != popIndex) {
                    if(newIndex == 0) {
                        dstInfo.Selections.RemoveAll (sel => sel.axName == ax.Name);
                    }
                    else  {
                        if (s == null) {
                            s = new AxisSelection ();
                            s.axName = ax.Name;
                            dstInfo.Selections.Add (s);
                        }
                        s.vShortName = (newIndex == 0)? "" : ax.Variants.ElementAt (newIndex -1).ShortName;
                    }
                    SetRemapConfig (dstInfo);
                    VariantOperation.SetDBDirty ();
                }
                GUILayout.Space (2f);
            }

            GUILayout.Space (8f);

            if (GUILayout.Button ("Delete Variant")) {
                m_removing.Add (dstInfo);
            }
        }

    }

}
