using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Odyssey.Web.TreeView
{
    public class  OdcTreeNodeBindingEventArgs:OdcTreeNodeEventArgs
    {
        public OdcTreeNodeBindingEventArgs(OdcTreeNode node, OdcTreeNodeBinding binding, object dataItem, OdcTreeNodeBindings bindings)
            : base(node)
        {
            Binding = binding;
            DataItem = dataItem;
            this.Bindings = bindings;
        }

        /// <summary>
        /// Gets or sets the binding to use for this node.
        /// </summary>
        public OdcTreeNodeBinding Binding { get; set; }

        /// <summary>
        /// Gets or sets the data item;
        /// </summary>
        public object DataItem { get; set; }

        /// <summary>
        /// Gets a collection of OdcTreeNodeBindings which are possible to use.
        /// </summary>
        public OdcTreeNodeBindings Bindings { get; private set; }
    }
}
