using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Odyssey.Web;

namespace TestApp
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Page.IsPostBack)
            //{
            //    var nodes = OdcTreeView1.GetAllNodes();
            //    foreach (var n in nodes)
            //    {
            //        n.EditMode = false;
            //    }
          //      OdcTreeView1.AutoPostBack = false;
          //  }
        }

    
        public void MyCommand(object sender, CommandEventArgs e)
        {
            Trace.Warn(e.CommandName + ": " + e.CommandArgument.ToString());
        }

        public void BtnClick(object sender, EventArgs e)
        {
            Trace.Warn("OK");
        }

        protected void OdcTreeView1_NodeClick(object sender, Odyssey.Web.OdcTreeNodeEventArgs e)
        {
            TextBox1.Text = e.Node.Text;
        }

        protected void OdcTreeView1_Collapsed(object sender, Odyssey.Web.OdcTreeNodeEventArgs e)
        {
            TextBox1.Text = "collapsed:" + e.Node.Text;
        }

        protected void OdcTreeView1_Expanded(object sender, Odyssey.Web.OdcTreeNodeEventArgs e)
        {
            TextBox1.Text = "expanded" + e.Node.Text;
        }

        protected void OdcTreeView1_NodeCheck(object sender, Odyssey.Web.OdcTreeNodeCheckEventArgs e)
        {
            TextBox1.Text = "checked: " + e.Node.Text + ": " + e.IsChecked.ToString();
        }


        protected void OdcTreeView1_Command(object sender, OdcTreeViewCommandEventArgs e)
        {
            TextBox1.Text = "command: " + e.CommandName + ", " + e.Node.Text;
            if (e.CommandName == "edit")
            {
             //   e.Node.EditMode = true;
                OdcTreeView1.AutoPostBack = true;
            }
            if (e.CommandName == "delete")
            {
                OdcTreeNode parent = e.Node.Parent;
                if (parent != null) parent.ChildNodes.Remove(e.Node);
            }
        }

        protected void OdcTreeView1_DblClick(object sender, OdcTreeNodeEventArgs e)
        {
       //     e.Node.EditMode = true;
         //   OdcTreeView1.AutoPostBack = true;

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
                sub.PopulateOnDemand = i==3;
                node.ChildNodes.Add(sub);
            }
        }
    }
}
