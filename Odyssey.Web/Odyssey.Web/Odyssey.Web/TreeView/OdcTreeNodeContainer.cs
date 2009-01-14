﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;

namespace Odyssey.Web
{
    /// <summary>
    /// A container for the NodeTemplate and EditNodeTemplate for a OdcTreeNode.
    /// </summary>
    public class OdcTreeNodeContainer:WebControl,IDataItemContainer,INamingContainer
    {
        public OdcTreeNodeContainer(OdcTreeNode node, int dataIndex)
        {
            this.dataItem = node.DataItem;
            this.dataItemIndex = dataIndex;
            this.displayIndex = dataIndex;
            this.node = node;
        }


        internal OdcTreeNode node;
        private object dataItem;
        private int dataItemIndex;
        private int displayIndex;

        public string SubClass { get; set; }

        public OdcTreeNode Node
        {
            get { return node; }
        }

        #region IDataItemContainer Members

        /// <summary>
        /// Gets the data item associated with this node if data bound, otherwise null.
        /// </summary>
        public object DataItem
        {
            get { return node.DataItem; }
        }

        /// <summary>
        /// Gets the index of the associated data item if data bound, otherwise 0.
        /// </summary>
        public int DataItemIndex
        {
            get { return dataItemIndex; }
        }


        /// <summary>
        /// When implemented, gets the position of the data item as displayed in a control.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An Integer representing the position of the data item as displayed in a control.
        /// </returns>
        public int DisplayIndex
        {
            get { return displayIndex; }
        }

        #endregion


        protected override void RenderContents(HtmlTextWriter writer)
        {
            EnsureChildControls();
            base.RenderContents(writer);
        }


        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Span;
            }
        }
    }
}