using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Odyssey.Web
{
    public class OdcTreeNodeCheckEventArgs:OdcTreeNodeEventArgs
    {
        public bool IsChecked { get; private set; }

        public OdcTreeNodeCheckEventArgs(OdcTreeNode node, bool isChecked)
            : base(node)
        {
            this.IsChecked = isChecked;
        }
    }

    public delegate void OdcTreeNodeCheckEventHandler(object sender, OdcTreeNodeCheckEventArgs e);
}
