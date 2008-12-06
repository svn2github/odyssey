using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Diagnostics;

namespace Odyssey.Web
{
    [Serializable]
    public class OdcTreeNodeCollection : IStateManager, IList<OdcTreeNode>, ICollection
    {
        #region IStateManager Members

        public bool IsTrackingViewState
        {
            get;
            internal set;
        }

        private int? level = null;

        public int Depth
        {
            get
            {
                if (level==null)
                {
                    int depth = 0;
                    OdcTreeNode col = Owner as OdcTreeNode;
                    while (col != null)
                    {
                        depth++;
                        col = col.owner as OdcTreeNode;
                    }
                    level = depth;

                }
                return level.Value;
            }
        }

        public void LoadViewState(object state)
        {
            object[] bags = (object[])state;
            Dictionary<int, OdcTreeNode> sorted = this.ToDictionary(x => x.Key);
            int index = 0;
            foreach (OdcTreeNode.TreeNodeState bag in bags)
            {
                OdcTreeNode node;
                if (sorted.ContainsKey(bag.key))
                {
                    node = sorted[bag.key];
                }
                else
                {
                    // this node was added during PopulateOnDemand or delayed loading:
                    node = new OdcTreeNode();
                    node.Key = bag.key;
                    Insert(index, node);
                }
                index++;
                node.LoadViewState(bag);
            }
        }

        public object SaveViewState()
        {
            object[] bags = new object[Count];
            int index = 0;
            foreach (OdcTreeNode node in this)
            {
                bags[index++] = node.SaveViewState();
            }
            return bags;
        }

        public void TrackViewState()
        {
            IsTrackingViewState = true;
        }

        #endregion

        public OdcTreeNodeCollection(OdcTreeNode owner)
            : base()
        {
            this.Owner = owner;
        }

        public OdcTreeNodeCollection(OdcTreeView owner)
            : base()
        {
            this.Owner = owner;
        }

        private List<OdcTreeNode> list = new List<OdcTreeNode>();
        private OdcTreeView treeView;

        public OdcTreeView TreeView
        {
            get
            {
                if (treeView == null)
                {
                    treeView = Owner as OdcTreeView;
                    if (treeView == null)
                    {
                        treeView = (Owner as OdcTreeNode).TreeView;
                    }
                }
                return treeView;
            }
        }


        /// <summary>
        /// Gets the Owner which is eighter OdcTreeView or OdcTreeNode.S
        /// </summary>
        public object Owner { get; private set; }


        #region IList<OdcTreeNode> Members

        public int IndexOf(OdcTreeNode item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, OdcTreeNode item)
        {
            RegisterNode(item);
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            OdcTreeNode node = this[index];
            UnRegisterNode(node);
            list.RemoveAt(index);
        }

        public OdcTreeNode this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        #endregion

        #region ICollection<OdcTreeNode> Members

        public void Add(OdcTreeNode item)
        {
            RegisterNode(item);
            list.Add(item);
        }

        /// <summary>
        /// Prepare the node with additional data required for the TreeView.
        /// </summary>
        /// <param name="item">The node to prepare</param>
        protected virtual void RegisterNode(OdcTreeNode node)
        {

            node.owner = Owner;
            if (TreeView != null)
            {
                TreeView.RegisterNode(node);
                foreach (OdcTreeNode sub in node.ChildNodes)
                {
                    node.ChildNodes.RegisterNode(sub);
                }
            }
        }

        protected virtual void UnRegisterNode(OdcTreeNode node)
        {
            node.owner = null;
            TreeView.UnRegisterNode(node);
        }

        public void Clear()
        {
            foreach (OdcTreeNode node in this)
            {
                UnRegisterNode(node);
            }
            list.Clear();
        }

        public bool Contains(OdcTreeNode item)
        {
            return list.Contains(item);
        }

        public void CopyTo(OdcTreeNode[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(OdcTreeNode item)
        {
            if (item != null) UnRegisterNode(item);
            return list.Remove(item);
        }

        #endregion

        #region IEnumerable<OdcTreeNode> Members

        public IEnumerator<OdcTreeNode> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            list.CopyTo(array as OdcTreeNode[], index);
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get { return list; }
        }

        #endregion

    }
}
