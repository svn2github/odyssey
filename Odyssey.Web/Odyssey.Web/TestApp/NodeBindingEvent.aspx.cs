using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Odyssey.Web;

namespace TestApp
{
    public partial class NodeBindingEvent : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Determine what OdcTreeNodeBinding to apply for for a node.
        /// </summary>
        protected void OdcTreeView1_NodeBinding(object sender, Odyssey.Web.TreeView.OdcTreeNodeBindingEventArgs e)
        {
            OdcTreeNode node = e.Node;

            // if the node has child nodes, apply a different NodeBinding that contains a different node template to the node:
            if (node.HasChildNodes) e.Binding = e.Bindings.GetNamedBinding("Root");
        }

        protected void OdcTreeView1_NodePopulate(object sender, OdcTreeNodeEventArgs e)
        {
            OdcTreeNode node = e.Node;
            node.PopulateOnDemand = false;
            for (int i = 1; i < 6; i++)
            {
                OdcTreeNode sub = new OdcTreeNode();
                sub.Text = node.Text + "." + i.ToString();
                sub.IsExpanded = false;
                sub.PopulateOnDemand = i == 3;
                node.ChildNodes.Add(sub);
            }
        }
    }
}
