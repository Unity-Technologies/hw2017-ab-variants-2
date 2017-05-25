using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;


namespace UnityEngine.Variant
{
    [System.Serializable]
    public class VariantManageTab 
    {
        [SerializeField]
        TreeViewState m_BundleTreeState;

        Rect m_Position;

        VariantTree m_BundleTree;
        AssetView m_AssetView;
        bool m_ResizingHorizontalSplitter = false;
        Rect m_HorizontalSplitterRect;
        [SerializeField]
        float m_HorizontalSplitterPercent;
        const float k_SplitterWidth = 3f;
        const float k_BundleTreeMenu = 20f;
        private static float m_UpdateDelay = 0f;

        EditorWindow m_Parent = null;

        public VariantManageTab()
        {
            m_HorizontalSplitterPercent = 0.4f;
        }

        public void OnEnable(Rect pos, EditorWindow parent)
        {
            m_Parent = parent;
            m_Position = pos;
            m_HorizontalSplitterRect = new Rect(
                (int)(m_Position.x + m_Position.width * m_HorizontalSplitterPercent),
                m_Position.y,
                k_SplitterWidth,
                m_Position.height);
        }

        public void Update()
        {
            if(Time.realtimeSinceStartup - m_UpdateDelay > 0.1f)
            {
                m_UpdateDelay = Time.realtimeSinceStartup;

                if (m_AssetView != null) {
                    m_AssetView.Update ();
                }
            }
        }

        public void AddNewAxis() {
            Model.AddNewAxis ();
            Model.ForceReloadData(m_BundleTree);
            m_Parent.Repaint();
        }

        public void ForceReloadData()
        {
            Model.ForceReloadData(m_BundleTree);
            m_Parent.Repaint();
        }

        public void OnGUI(Rect pos)
        {
            m_Position = pos;

            if(m_BundleTree == null)
            {
                m_AssetView = new AssetView();

                if (m_BundleTreeState == null) {
                    m_BundleTreeState = new TreeViewState ();
                }
                m_BundleTree = new VariantTree(m_BundleTreeState, this);
                m_BundleTree.Refresh();
                m_Parent.Repaint();
            }
            
            HandleHorizontalResize();

            //Left half
            var bundleTreeRect = new Rect(
                m_Position.x,
                m_Position.y,
                m_HorizontalSplitterRect.x,
                m_Position.height);
            
            m_BundleTree.OnGUI(bundleTreeRect);

            //Right half.
            float panelLeft = m_HorizontalSplitterRect.x + k_SplitterWidth;
            float panelWidth = m_Position.width - panelLeft;
            float panelHeight = m_Position.height;

            Rect assetViewArea = new Rect (panelLeft, m_Position.y, panelWidth, panelHeight);
            GUILayout.BeginArea (assetViewArea);

            m_AssetView.OnGUI(new Rect (0, 0, panelWidth, panelHeight), m_Parent);

            GUILayout.EndArea ();

            if (m_ResizingHorizontalSplitter)
                m_Parent.Repaint();
        }

        private void HandleHorizontalResize()
        {
            //m_horizontalSplitterRect.x = Mathf.Clamp(m_horizontalSplitterRect.x, position.width * .1f, (position.width - kSplitterWidth) * .9f);
            m_HorizontalSplitterRect.x = (int)(m_Position.width * m_HorizontalSplitterPercent);
            m_HorizontalSplitterRect.height = m_Position.height;

            EditorGUIUtility.AddCursorRect(m_HorizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.mouseDown && m_HorizontalSplitterRect.Contains(Event.current.mousePosition))
                m_ResizingHorizontalSplitter = true;

            if (m_ResizingHorizontalSplitter)
            {
                //m_horizontalSplitterRect.x = Mathf.Clamp(Event.current.mousePosition.x, position.width * .1f, (position.width - kSplitterWidth) * .9f);
                m_HorizontalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / m_Position.width, 0.1f, 0.9f);
                m_HorizontalSplitterRect.x = (int)(m_Position.width * m_HorizontalSplitterPercent);
            }

            if (Event.current.type == EventType.MouseUp)
                m_ResizingHorizontalSplitter = false;
        }
    }
}