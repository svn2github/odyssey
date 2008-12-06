using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TestApp
{
    public partial class DataBound : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            TreeNodeBinding b;

        }

        protected void OdcTreeView1_NodeEdited(object sender, Odyssey.Web.OdcTreeNodeEventArgs e)
        {
            object data = e.Node.DataItem;
            Trace.Warn(data.ToString());
        }

        protected void TreeView1_TreeNodeCheckChanged(object sender, TreeNodeEventArgs e)
        {
            Trace.Warn("IOK");
        }
    }
}
