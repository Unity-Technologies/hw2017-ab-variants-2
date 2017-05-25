using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace UnityEngine.Variant
{
    public class VariantTreeItem : TreeViewItem
    {
        private VariantInfo m_info;
        public VariantInfo Info
        {
            get { return m_info; }
        }
        public VariantTreeItem(VariantInfo info, int depth, Texture2D iconTexture) : base(info.nameHashCode, depth, info.Name)
        {
            m_info = info;
            icon = iconTexture;
            children = new List<TreeViewItem>();
        }

        public void Rename(string newName) {
            m_info.Name = newName;
        }
    }

    public class VariantAxisTreeItem : TreeViewItem
    {
        private VariantAxisInfo m_info;
        public VariantAxisInfo Info
        {
            get { return m_info; }
        }
        public VariantAxisTreeItem(VariantAxisInfo b, int depth, Texture2D iconTexture) : base(b.nameHashCode, depth, b.Name)
        {
            m_info = b;
            icon = iconTexture;
            children = new List<TreeViewItem>();
        }

        public void Rename(string newName) {
            m_info.Name = newName;
        }
    }

    public class RootTreeItem : TreeViewItem
    {
        private RootVariantInfo m_info;
        public RootVariantInfo Info
        {
            get { return m_info; }
        }
        public RootTreeItem(RootVariantInfo b, int depth, Texture2D iconTexture) : base("".GetHashCode(), depth, "")
        {
            m_info = b;
            icon = iconTexture;
            children = new List<TreeViewItem>();
        }
    }


    public class VariantInfo
    {
        protected VariantAxisInfo m_Parent;
        protected bool m_Dirty;
        protected Variant m_variant;

        public VariantInfo(Variant v, VariantAxisInfo parent)
        {
            m_variant = v;
            m_Parent = parent;
        }

        public VariantAxisInfo parent
        { get { return m_Parent; } }

        public Variant AssignedVariant {
            get {
                return m_variant;
            }
        }

        public string Name
        {
            get { return m_variant.Name; }
            set { m_variant.Name = value; }
        }

        public int nameHashCode
        {
            get { return m_variant.ShortName.GetHashCode(); }
        }

        public void HandleDelete()
        {
            m_Parent.HandleChildDelete(this);
        }
            
        public VariantTreeItem CreateTreeView()
        {
            return new VariantTreeItem (this, 1, Model.GetBundleIcon ());
        }
    }

    public class VariantAxisInfo
    {
        protected List<VariantInfo> m_Children;
        protected VariantAxis m_axis;

        public VariantAxis AssignedAxis {
            get {
                return m_axis;
            }
        }

        public VariantAxisInfo(VariantAxis axis)
        {
            m_axis = axis;
            m_Children = new List<VariantInfo>();

            foreach (var v in m_axis.Variants) {
                m_Children.Add (new VariantInfo(v, this));
            }
        }
        
        public VariantInfo GetChild(Variant v)
        {
            return m_Children.Find(c => c.AssignedVariant == v);
        }

        public List<VariantInfo> GetChildList()
        {
            return m_Children;
        }

        public void AddChild(Variant info)
        {
            m_Children.Add(new VariantInfo(info, this));
        }

        public string Name
        {
            get { return m_axis.Name; }
            set {
                m_axis.Name = value;
            }
        }

        public int nameHashCode
        {
            get { return m_axis.Name.GetHashCode(); }
        }

        public void HandleDelete()
        {
            VariantOperation.GetOperation().RemoveVariantAxis (m_axis);
            m_Children.Clear();
        }

        public bool HandleChildDelete(VariantInfo child)
        {
            m_Children.Remove (child);
            VariantOperation.GetOperation().RemoveVariantFromVariantAxis (child.AssignedVariant, m_axis);

            return true;
        }

        public VariantAxisTreeItem CreateTreeView()
        {
            var result = new VariantAxisTreeItem (this, 0, Model.GetFolderIcon ());
            foreach (var child in m_Children) {
                result.AddChild (child.CreateTreeView ());
            }
            return result;
        }
    }

    public class RootVariantInfo
    {
        [SerializeField] List<VariantAxisInfo> m_Children;

        public RootVariantInfo()
        {
            m_Children = new List<VariantAxisInfo> ();
            foreach (var ax in VariantOperation.Axis) {
                if (GetChild (ax) == null) {
                    AddChild (ax);
                }
            }
        }

        public VariantAxisInfo GetChild(VariantAxis ax)
        {
            return m_Children.Find(c => c.AssignedAxis == ax);
        }

        public void AddChild(VariantAxis ax)
        {
            m_Children.Add(new VariantAxisInfo (ax));
        }

        public RootTreeItem CreateTreeView()
        {
            var result = new RootTreeItem(this, -1, Model.GetFolderIcon());
            foreach (var child in m_Children)
            {
                result.AddChild(child.CreateTreeView());
            }
            return result;
        }
    }
}
