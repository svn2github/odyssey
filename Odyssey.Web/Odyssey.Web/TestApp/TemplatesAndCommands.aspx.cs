using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Odyssey.Web;

namespace TestApp
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) BuildRootNodes();

        }

        public void Validating(object source, ServerValidateEventArgs args)
        {
            args.IsValid = args.Value.StartsWith("!");
        }

        private void BuildRootNodes()
        {
            for (int i = 1; i < 6; i++)
            {
                string text = "Node " + i.ToString();
                OdcTreeNode node = new OdcTreeNode();
                node.Text = text;
                node.IsExpanded = false;
                if (i == 2) node.PopulateOnDemand = true;
                treeView.Nodes.Add(node);
            }
        }

        protected void treeView_NodePopulate(object sender, OdcTreeNodeEventArgs e)
        {
            OdcTreeNode parent = e.Node;
            string text = parent.Text + ".";
            parent.PopulateOnDemand = false;

            for (int i = 1; i < 5; i++)
            {
                OdcTreeNode node = new OdcTreeNode();
                node.Text = text + i.ToString();
                node.IsExpanded = false;
                if (i == 3) node.PopulateOnDemand = true;
                parent.ChildNodes.Add(node);
            }
        }

        protected void treeView_Command(object sender, OdcTreeViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "add": AddNode(e.Node); break;
                case "remove": RemoveNode(e.Node); break;
                case "edit": EditNode(e.Node); break;
                case "cancel": CancelNode(e.Node); break;
                case "rename": RenameNode(e.Node); break;
            }
        }

        private void RenameNode(OdcTreeNode odcTreeNode)
        {
            //TextBox tb = odcTreeNode.Container.FindControl("tbText") as TextBox;
            //odcTreeNode.Text = tb.Text;
            if (Page.IsValid) treeView.EditNodeKey = -1;
        }

        private void CancelNode(OdcTreeNode odcTreeNode)
        {
            treeView.EditNodeKey = -1;
        }

        private void EditNode(OdcTreeNode node)
        {
            treeView.EditNodeKey = node.Key;
        }

        private void RemoveNode(OdcTreeNode node)
        {
            OdcTreeNode parent = node.Parent;
            if (parent != null)
            {
                parent.ChildNodes.Remove(node);
            }
            else
            {
                treeView.Nodes.Remove(node);
            }
        }

        private void AddNode(OdcTreeNode node)
        {
            OdcTreeNode newNode = new OdcTreeNode();
            newNode.Text = "new";
            node.ChildNodes.Add(newNode);
        }

        protected void treeView_PostBack(object sender, OdcTreeNodeEventArgs e)
        {
            // Page.Validate();
            // if (Page.IsValid)
            {
                treeView.EditNodeKey = -1;
            }
        }
    }
}
