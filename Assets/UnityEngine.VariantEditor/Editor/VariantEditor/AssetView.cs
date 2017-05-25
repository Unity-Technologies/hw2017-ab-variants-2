using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System;
using System.IO;


namespace UnityEngine.Variant
{
    internal class AssetView
    {
        private GraphBackground background = new GraphBackground();
        private Vector2 scrollPos = new Vector2(1500,0);
        private Vector2 spacerRectRightBottom;

        public AssetView()
        {
        }

        public void OnGUI(Rect rect, EditorWindow w)
        {
            background.Draw(rect, scrollPos);

            using (var scrollScope = new EditorGUILayout.ScrollViewScope (scrollPos)) {
                scrollPos = scrollScope.scrollPosition;

                w.BeginWindows ();

                var objs = VariantOperation.Objects;
                int i = 0;
                foreach (var o in objs) {
                    o.DrawObject (i++, rect);
                }

                w.EndWindows ();

                if (objs.Any()) {
                    spacerRectRightBottom.x = 600f;
                    spacerRectRightBottom.y = i * (ObjectVariantInfo.Height + 20f)+ 40f;
                    GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Width(spacerRectRightBottom.x), GUILayout.Height(spacerRectRightBottom.y));
                }
            }
            HandleDragAndDropGUI (rect, w);
        }

        public void UpdateViewWithTag(string variantTag) {
        }

        public void Update() {
        }

        private void HandleDragAndDropGUI (Rect dragdropArea, EditorWindow w)
        {
            Event evt = Event.current;

            switch (evt.type) {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dragdropArea.Contains (evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag ();

                    foreach (Object obj in DragAndDrop.objectReferences) {
                        VariantOperation.GetOperation().CreateObjectVariantInfo (obj);
                    }
                }
                break;
            }
        }
    }
}
