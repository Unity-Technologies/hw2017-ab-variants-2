using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace UnityEngine.Variant
{

    public class Model
    {
        const string k_NewVariantBaseName = "newVariant";
        public static /*const*/ Color k_LightGrey = Color.grey * 1.5f;

        private static RootVariantInfo m_RootLevelVariant = new RootVariantInfo();

        static private Texture2D m_folderIcon = null;
        static private Texture2D m_bundleIcon = null;
        static private Texture2D m_sceneIcon = null;

        public static void ForceReloadData(TreeView tree)
        {
            Rebuild();
            tree.Reload();
        }

        public static void Rebuild()
        {
            m_RootLevelVariant = new RootVariantInfo();
            Refresh();
        }

        public static RootTreeItem CreateRootTreeView() {
            return m_RootLevelVariant.CreateTreeView ();
        }

        public static void Refresh()
        {
            var axis = VariantOperation.Axis;
            if(axis != null)
            {
                foreach (var ax in axis) {
                    if (m_RootLevelVariant.GetChild (ax) == null) {
                        m_RootLevelVariant.AddChild (ax);
                    }
                }
            }
        }

        public static void AddNewAxis() {
            var newAxis = VariantOperation.GetOperation().CreateNewAxis ("New Variant Group");
            m_RootLevelVariant.AddChild (newAxis);
        }

        static public Texture2D GetFolderIcon()
        {
            if (m_folderIcon == null)
                FindBundleIcons();
            return m_folderIcon;
        }
        static public Texture2D GetBundleIcon()
        {
            if (m_bundleIcon == null)
                FindBundleIcons();
            return m_bundleIcon;
        }
        static public Texture2D GetSceneIcon()
        {
            if (m_sceneIcon == null)
                FindBundleIcons();
            return m_sceneIcon;
        }
        static private void FindBundleIcons()
        {
            m_folderIcon = EditorGUIUtility.FindTexture("Folder Icon");
            string[] icons = AssetDatabase.FindAssets("ABundleBrowserIconY1756");
            foreach (string i in icons)
            {
                string name = AssetDatabase.GUIDToAssetPath(i);
                if (name.Contains("ABundleBrowserIconY1756Basic.png"))
                    m_bundleIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(name, typeof(Texture2D));
                else if (name.Contains("ABundleBrowserIconY1756Scene.png"))
                    m_sceneIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(name, typeof(Texture2D));
            }
        }
    }
}
