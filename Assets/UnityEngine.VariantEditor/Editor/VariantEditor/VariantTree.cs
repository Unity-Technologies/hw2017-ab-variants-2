using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System;


namespace UnityEngine.Variant
{
    internal class VariantTree : TreeView
    { 
        VariantManageTab m_Controller;
        private bool m_ContextOnItem = false;
//        List<UnityEngine.Object> m_EmptyObjectList = new List<Object>();

        public VariantTree(TreeViewState state, VariantManageTab ctrl) : base(state)
        {
            Model.Rebuild();
            m_Controller = ctrl;
            showBorder = true;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item.displayName.Length > 0;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
        }

        protected override void RenameEnded(RenameEndedArgs args)
        { 
            base.RenameEnded(args);
            if (args.newName.Length > 0 && args.newName != args.originalName)
            {
                args.acceptedRename = true;

                var renamedItem = FindItem(args.itemID, rootItem);
                if (renamedItem is VariantAxisTreeItem) {
                    VariantAxisTreeItem axt = renamedItem as VariantAxisTreeItem;
                    axt.Rename (args.newName);
                    ReloadAndSelect(axt.Info.nameHashCode, false);
                }
                if (renamedItem is VariantTreeItem) {
                    VariantTreeItem vt = renamedItem as VariantTreeItem;
                    vt.Rename (args.newName);
                    ReloadAndSelect(vt.Info.nameHashCode, false);
                }
            }
            else
            {
                args.acceptedRename = false;
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            Model.Refresh();
            var root = Model.CreateRootTreeView();
            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 1) {
                var selected = FindItem(selectedIds[0], rootItem);

                if (selected is VariantAxisTreeItem)
                {
                    VariantAxisTreeItem item = selected as VariantAxisTreeItem;
                    item.Info.AssignedAxis.ResetSelection ();
                }
                if (selected is VariantTreeItem) {
                    VariantTreeItem item = selected as VariantTreeItem;
                    item.Info.parent.AssignedAxis.SelectVariant (item.Info.AssignedVariant);
                }
            }
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }


        protected override void ContextClicked()
        {
            if (m_ContextOnItem)
            {
                m_ContextOnItem = false;
                return;
            }

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add new variant group"), false, AddNewVariantAxis, null); 
            menu.AddItem(new GUIContent("Reload all data"), false, ForceReloadData, null);
            menu.ShowAsContext();
        }

        protected override void ContextClickedItem(int id)
        {
            m_ContextOnItem = true;
            List<TreeViewItem> selectedNodes = new List<TreeViewItem>();
            foreach (var nodeID in GetSelection())
            {
                selectedNodes.Add(FindItem(nodeID, rootItem));
            }

            if(selectedNodes.Count == 1)
            {
                GenericMenu menu = new GenericMenu();
                if (selectedNodes[0] is VariantAxisTreeItem)
                {
                    VariantAxisTreeItem item = selectedNodes [0] as VariantAxisTreeItem;
                    if (!item.Info.AssignedAxis.IsReadOnly) {
                        menu.AddItem (new GUIContent ("Add new variant"), false, AddNewVariant, selectedNodes);
                        menu.AddItem(new GUIContent("Rename"), false, RenameItem, selectedNodes);
                        menu.AddItem(new GUIContent("Delete " + selectedNodes [0].displayName), false, DeleteVariant, selectedNodes);
                    }
                }
                if (selectedNodes [0] is VariantTreeItem) {
                    VariantTreeItem item = selectedNodes [0] as VariantTreeItem;
                    if (!item.Info.parent.AssignedAxis.IsReadOnly) {
                        menu.AddItem(new GUIContent("Rename"), false, RenameItem, selectedNodes);
                        menu.AddItem(new GUIContent("Delete " + selectedNodes [0].displayName), false, DeleteVariant, selectedNodes);
                    }
                }
                menu.ShowAsContext();
            }
        }

        void ForceReloadData(object context)
        {
            Model.ForceReloadData(this);
        }

        void RenameItem(object b)
        {
            var selectedNodes = b as List<TreeViewItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                if (selectedNodes[0] is VariantAxisTreeItem)
                {
                    VariantAxisTreeItem item = selectedNodes [0] as VariantAxisTreeItem;
                    if (!item.Info.AssignedAxis.IsReadOnly) {
                        BeginRename(FindItem(item.Info.nameHashCode, rootItem), 0.1f);
                    }
                }
                else if(selectedNodes[0] is VariantTreeItem)
                {
                    VariantTreeItem item = selectedNodes [0] as VariantTreeItem;
                    if (!item.Info.parent.AssignedAxis.IsReadOnly) {
                        BeginRename (FindItem (item.Info.nameHashCode, rootItem), 0.1f);
                    }
                }
            }
        }
        void AddNewVariantAxis(object b)
        {
            Model.AddNewAxis ();
            Reload ();
        }

        void AddNewVariant(object b)
        {
            var selectedNodes = b as List<TreeViewItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                if (selectedNodes[0] is VariantAxisTreeItem)
                {
                    VariantAxisTreeItem item = selectedNodes [0] as VariantAxisTreeItem;
                    item.Info.AssignedAxis.AddVariant ("New Variant Item");
                    ForceReloadData (this);
                    //ReloadAndSelect(item.Info.nameHashCode, true);
                }
            }
        }

        void DeleteVariant(object b)
        {
            var selectedNodes = b as List<TreeViewItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                foreach (var i in selectedNodes) {
                    if (i is VariantAxisTreeItem)
                    {
                        VariantAxisTreeItem item = i as VariantAxisTreeItem;
                        VariantOperation.GetOperation().RemoveVariantAxis ( item.Info.AssignedAxis );
                    }
                    else if(i is VariantTreeItem)
                    {
                        VariantTreeItem item = i as VariantTreeItem;
                        VariantAxisTreeItem p = i.parent as VariantAxisTreeItem;
                        VariantOperation.GetOperation().RemoveVariantFromVariantAxis ( item.Info.AssignedVariant, p.Info.AssignedAxis );
                    }
                }
            }
            ForceReloadData (this);
        }

        protected override void KeyEvent()
        {
            if (Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                List<TreeViewItem> selectedNodes = new List<TreeViewItem>();
                foreach (var nodeID in GetSelection())
                {
                    selectedNodes.Add(FindItem(nodeID, rootItem));
                }
                DeleteVariant(selectedNodes);
            }
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        internal void Refresh()
        {
            var selection = GetSelection();
            Reload();
            SelectionChanged(selection);
        }

        private void ReloadAndSelect(int hashCode, bool rename)
        {
            var selection = new List<int>();
            selection.Add(hashCode);
            ReloadAndSelect(selection);
            if(rename)
            {
                BeginRename(FindItem(hashCode, rootItem), 0.25f);
            }
        }
        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes, TreeViewSelectionOptions.RevealAndFrame);
            SelectionChanged(hashCodes);
        }
    }
}
