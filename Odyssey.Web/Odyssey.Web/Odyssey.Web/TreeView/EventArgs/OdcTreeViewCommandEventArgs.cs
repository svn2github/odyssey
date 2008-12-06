using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Odyssey.Web
{
    public class OdcTreeViewCommandEventArgs:CommandEventArgs
    {
        public OdcTreeNode Node { get; private set; }
        public object Source { get; private set; }

        public OdcTreeViewCommandEventArgs(Object source, CommandEventArgs e, OdcTreeNode node)
            : base(e)
        {
            this.Node = node;
            this.Source = source;
        }
    }

    public delegate void OdcTreeViewCommandEventHandler(object sender, OdcTreeViewCommandEventArgs e);
}
