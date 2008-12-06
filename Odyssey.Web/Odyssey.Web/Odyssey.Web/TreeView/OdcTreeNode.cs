using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Drawing;

namespace Odyssey.Web
{
    [Serializable]
    [DefaultProperty("Text")]
    [ParseChildren(true, "ChildNodes")]
    public class OdcTreeNode : IStateManager, ICloneable
    {
        internal OdcTreeNode(OdcTreeView treeView)
            : base()
        {
            this.TreeView = treeView;
            Init();
        }


        public OdcTreeNode()
            : base()
        {
            Init();
        }

        public OdcTreeNode(string text)
            : base()
        {
            Init();
            this.Text = text;
        }

        public OdcTreeNode(string text, object value)
            : base()
        {
            Init();
            this.Text = text;
            this.Value = value;
        }

        public OdcTreeNode(string text, object value, bool isChecked)
            : base()
        {
            Init();
            this.Text = text;
            this.Value = value;
            this.IsChecked = isChecked;
            this.ShowCheckBox = true;
        }

        private void Init()
        {
            ChildNodes = new OdcTreeNodeCollection(this);
            DataItem = this;
            CanEdit = true;
        }

        #region IStateManager Members

        public bool IsTrackingViewState
        {
            get;
            private set;
        }

        public void LoadViewState(object stateObject)
        {
            TreeNodeState state = (TreeNodeState)stateObject;
            state.Load(this);
        }

        public object SaveViewState()
        {
            TreeNodeState state = TreeNodeState.Create(this);
            return state;
        }

        public void TrackViewState()
        {
            IsTrackingViewState = true;
            ChildNodes = new OdcTreeNodeCollection(this);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        [Serializable]
        internal struct TreeNodeState
        {
            string text;
            object value;
            string imageUrl;
            object childNodes;
            bool isChecked;
            bool? showCheckBox;
            bool editMode;
            bool populate;
            string cssClass;

            /// <summary>
            /// this value is most important since it is intended to identify the node for viewstate.
            /// </summary>
            public int key;
            bool? isExpanded;

            internal static TreeNodeState Create(OdcTreeNode node)
            {

                TreeNodeState state = new TreeNodeState();
                state.text = node.Text;
                state.value = node.Value;
                state.imageUrl = node.ImageUrl;
                state.key = node.Key;
                state.isExpanded = node.IsExpanded;
                state.childNodes = node.ChildNodes.SaveViewState();
                state.isChecked = node.IsChecked;
                state.showCheckBox = node.ShowCheckBox;
                state.editMode = node.EditMode;
                state.populate = node.PopulateOnDemand;
                state.cssClass = node.CssClass;
                return state;
            }

            internal void Load(OdcTreeNode node)
            {
                node.ShowCheckBox = showCheckBox;
                node.IsChecked = isChecked;
                node.Text = text;
                node.Value = value;
                node.ImageUrl = imageUrl;
                node.Key = key;
                node.IsExpanded = isExpanded;
                node.PopulateOnDemand = populate;
                node.ChildNodes.LoadViewState(childNodes);
                node.CssClass = cssClass;
            }
        }

        /// <summary>
        /// Gets a collection of all child nodes.
        /// </summary>
        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [MergableProperty(false)]
        public OdcTreeNodeCollection ChildNodes { get; private set; }

        /// <summary>
        /// Gets the TreeView that owns this node.
        /// </summary>
        [Browsable(false)]
        public OdcTreeView TreeView { get; internal set; }

        /// <summary>
        /// Gets  whether this node is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                OdcTreeView view = TreeView;
                return view != null ? view.SelectedNodekey == Key : false;
            }
        }

        /// <summary>
        /// Gets whether this node is the first in the same hierarchy.
        /// </summary>
        public bool IsFirst
        {
            get
            {
                OdcTreeNodeCollection c = Collection;
                return c != null && c.Count > 0 ? c[0] == this : false;
            }
        }

        /// <summary>
        /// Gets whether this node is the last in the same hierarchy.
        /// </summary>
        public bool IsLast
        {
            get
            {
                OdcTreeNodeCollection c = Collection;
                return c != null && c.Count > 0 ? c[c.Count - 1] == this : false;
            }
        }

        /// <summary>
        /// This value is a unique identifier.
        /// </summary>
        [Browsable(false)]
        public int Key { get; internal set; }
        public bool IsDataBound { get; internal set; }

        private bool? isExpanded;

        private bool populateOnDemand;

        public bool PopulateOnDemand
        {
            get { return populateOnDemand; }
            set
            {
                if (populateOnDemand != value)
                {
                    populateOnDemand = value;
                }
            }
        }

        public bool HasChildNodes
        {
            get { return ChildNodes.Count > 0 || PopulateOnDemand; }
        }


        /// <summary>
        /// Gets or sets whether this node is expanded.
        /// </summary>
        public bool? IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    if (TreeView!=null) TreeView.NotifyExpandedOrCollapsed(this);
                }
            }
        }


        internal object owner;

        internal OdcTreeNodeCollection Collection
        {
            get
            {
                OdcTreeNode node = owner as OdcTreeNode;
                if (node != null) return node.ChildNodes;
                OdcTreeView view = owner as OdcTreeView;
                return view != null ? view.Nodes : null;
            }
        }


        /// <summary>
        /// Gets the parent OdcTreeNode otherwise null.
        /// </summary>
        [Browsable(false)]
        public OdcTreeNode Parent
        {
            get
            {
                return owner as OdcTreeNode;
            }
        }


        /// <summary>
        /// Gets or sets the text for the OdcTreeNode.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets any serializable object to the node.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets the depth of this node in the hierarchy.
        /// </summary>
        public int Depth
        {
            get
            {
                if (owner == null) return -1;
                int depth = 0;
                OdcTreeNode node = Parent;
                while (node != null)
                {
                    depth++;
                    node = node.Parent;
                }
                return depth;
            }
        }

        /// <summary>
        /// Gets the data item assoicated with this node otherwise null.
        /// </summary>
        public object DataItem { get; internal set; }

        /// <summary>
        /// Gets the data path for the node if databound.
        /// </summary>
        public string DataPath { get; internal set; }

        /// <summary>
        /// Gets or sets an url for the image of this node.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets whether this node is checked.
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets whether this node offers a checkbox.
        /// </summary>
        public bool? ShowCheckBox { get; set; }
        public OdcTreeNodeContainer Container { get; internal set; }

        /// <summary>
        /// Gets whether this node is truly expanded.
        /// </summary>
        /// <returns></returns>
        internal bool IsNodeExpanded()
        {
            return IsExpanded ?? true;
        }

        /// <summary>
        /// Gets whether this node offers an expand or collapse button.
        /// </summary>
        /// <returns></returns>
        internal bool HasToggleButton()
        {
            return PopulateOnDemand || ChildNodes.Count > 0;
        }

        /// <summary>
        /// Gets whether this node is in edit mode.
        /// </summary>
        [DefaultValue(false)]
        [Browsable(false)]
        public bool EditMode
        {
            get
            {
                if (!CanEdit) return false;
                OdcTreeView view = this.TreeView;
                if (view != null)
                {
                    return view.EditNodeKey == Key;
                }
                else return false;
            }
        }


        /// <summary>
        /// Gets or sets the css class to use for this node.
        /// </summary>
        [DefaultValue("")]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets whether the node can be edited.
        /// </summary>
        [DefaultValue(true)]
        public bool CanEdit { get; set; }

    }

}
