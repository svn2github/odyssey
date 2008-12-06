using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Odyssey.Web
{
    public class OdcTreeNodeEventArgs:EventArgs
    {
        public OdcTreeNodeEventArgs(OdcTreeNode node)
            : base()
        {
            this.Node = node;
        }

        public OdcTreeNode Node { get; private set; }
    }

    public delegate void OdcTreeNodeEventHandler(object sender, OdcTreeNodeEventArgs e);
}
