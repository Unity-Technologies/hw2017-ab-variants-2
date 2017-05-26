using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UnityEngine.Variant
{

    public class VariantEditor : EditorWindow
    {
        [SerializeField]
        public VariantManageTab m_ManageTab;

        private Texture2D m_RefreshTexture;

        const float k_ToolbarPadding = 15;
        const float k_MenubarPadding = 16;

        [MenuItem("Window/Variant/Open Editor...", priority = 48)]
        static void ShowWindow()
        {
            var window = GetWindow<VariantEditor>();
            window.titleContent = new GUIContent("Variants");
            window.Show();
        }
        private void OnEnable()
        {
            Rect subPos = GetSubWindowArea();
            if(m_ManageTab == null)
                m_ManageTab = new VariantManageTab();
            m_ManageTab.OnEnable(subPos, this);

            m_RefreshTexture = EditorGUIUtility.FindTexture("Refresh");
        }

        private Rect GetSubWindowArea()
        {
            Rect subPos = new Rect(0, k_MenubarPadding, position.width, position.height - k_MenubarPadding);
            return subPos;
        }

        private void Update()
        {
            m_ManageTab.Update();
        }

        private void OnGUI()
        {
            DrawToolBar();
            m_ManageTab.OnGUI(GetSubWindowArea());
        }

        void DrawToolBar()
        {
            using (new EditorGUILayout.HorizontalScope (EditorStyles.toolbar)) {
                GUILayout.Space(k_ToolbarPadding);
                if (GUILayout.Button(m_RefreshTexture, EditorStyles.toolbarButton)) {
                    m_ManageTab.ForceReloadData ();
                }
                if (GUILayout.Button("Add New Axis", EditorStyles.toolbarButton)) {
                    m_ManageTab.AddNewAxis ();
                }
                if (GUILayout.Button("Apply All Again", EditorStyles.toolbarButton)) {
                    ApplyAll ();
                }
                if (GUILayout.Button("Reset ll", EditorStyles.toolbarButton)) {
                    ApplyAll ();
                }
                GUILayout.FlexibleSpace();
            }

        }
        void ApplyAll () {
            VariantOperation.GetOperation ().ResetRemap ();

            VariantOperation.GetOperation ().RemoveVariantMapping ();

            foreach (var objInfo in VariantOperation.Objects) {
                objInfo.SetAllRemapConfig ();
            }

            VariantOperation.GetOperation ().ApplyRemap();
        }

        void ResetAll() {
            VariantOperation.GetOperation ().ResetRemap ();
        }
    }
}