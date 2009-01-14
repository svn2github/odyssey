﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Collections.Specialized;
using System.Reflection;
using Odyssey.Web.TreeView;

#region changelog
// Version 0.1..2
// - redesigned CreateChildControls and InstantiateNodes:
//   previousely, it was not possible to add controls like DropDownList to a Template and keep track of it's state since
//   the template was always recreated after a PostBack.
// - using yield return instead of populating a list. for GetAllNodes().
#endregion
/// TODO: Multiselect (one selected and many marked )
/// TODO: Drag&Drop
/// TODO: Designer
/// TODO: Is it possible to use Index instead of DataPath for saving IsChecked and IsExpanded in data bound nodes?
#region WebResources
[assembly: WebResource(Odyssey.Web.OdcTreeView.CssStyles, "text/css", PerformSubstitution = true)]
[assembly: WebResource(Odyssey.Web.OdcTreeView.RootLineImage, "image/gif")]
[assembly: WebResource(Odyssey.Web.OdcTreeView.LineImage, "image/gif")]
[assembly: WebResource(Odyssey.Web.OdcTreeView.LastLineImage, "image/gif")]
[assembly: WebResource(Odyssey.Web.OdcTreeView.MidLineImage, "image/gif")]
[assembly: WebResource(Odyssey.Web.OdcTreeView.PlusImage, "image/png")]
[assembly: WebResource(Odyssey.Web.OdcTreeView.MinusImage, "image/png")]
#endregion
namespace Odyssey.Web
{
    /// <summary>
    /// An ajax enabled TreeView control with templates and context menu. 
    /// This control requires a ScriptManager.
    /// (c) Copyright 2008 by Thomas Gerber
    /// </summary>
    [ToolboxData("<{0}:OdcTreeView runat=server></{0}:OdcTreeView>")]
    [ParseChildren(true)]
    public class OdcTreeView : HierarchicalDataBoundControl, IScriptControl, IPostBackEventHandler, IPostBackDataHandler, INamingContainer
    {
        internal const string CssStyles = "Odyssey.Web.TreeView.styles.TreeView.css";
        internal const string RootLineImage = "Odyssey.Web.TreeView.root.gif";
        internal const string LineImage = "Odyssey.Web.TreeView.images.line.gif";
        internal const string LastLineImage = "Odyssey.Web.TreeView.images.last.gif";
        internal const string MidLineImage = "Odyssey.Web.TreeView.images.mid.gif";
        internal const string PlusImage = "Odyssey.Web.TreeView.images.plus.png";
        internal const string MinusImage = "Odyssey.Web.TreeView.images.minus.png";

        /// <summary>
        /// Initializes a new instance of the <see cref="OdcTreeView"/> class.
        /// </summary>
        public OdcTreeView()
        {
            Nodes = new OdcTreeNodeCollection(this);
            AutoPostBack = false;
            DisableTextSelection = true;
            EditNodeKey = -1;
            ExpandDepth = 1;
        }

        void RegisterStyleSheet()
        {
            string csslink = "<link href='" + Page.ClientScript.GetWebResourceUrl(this.GetType(), CssStyles) + "' rel='stylesheet' type='text/css' />";
            LiteralControl include = new LiteralControl(csslink);
            this.Page.Header.Controls.Add(include);
        }

        /// <summary>
        /// Sets the initialized state of the data-bound control before the control is loaded.
        /// </summary>
        /// <param name="sender">The <see cref="T:System.Web.UI.Page"/> that raised the event.</param>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnPagePreLoad(object sender, EventArgs e)
        {
            Debug.WriteLine("PagePreLoad");
            if (Page.IsPostBack) EnsureHiddenFieldsLoaded();
            base.OnPagePreLoad(sender, e);
        }

        protected override void OnInit(EventArgs e)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Init");
            EnableDblClick = DblClick != null;
            base.OnInit(e);
            RegisterStyleSheet();
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            Debug.WriteLine("PreRender");

            if (this.IsDataBound)
            {
                ReIndexNodes();
            }

            // although previousely executed with CreateChildControl, the template might have changed due to postback event fired after the controls where created,
            // therefore, remove the instantiated control for each node and reinstantiate again:
            InstantiateContextMenu();
            InstantiateNodes(true);

            // perform validation now, when all templates are assigned and population on demand is finished:
            Page.Validate();

            ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            ScriptManager.RegisterHiddenField(this, hiddenFieldName, "");
            Page.RegisterRequiresPostBack(this);
            base.OnPreRender(e);
        }

        private void ReIndexNodes()
        {
            ReIndexNodes(Nodes, 1);
        }

        private int ReIndexNodes(OdcTreeNodeCollection nodes, int index)
        {
            foreach (OdcTreeNode node in nodes)
            {
                node.Key = index++;
                node.Container.ID = "K" + node.Key.ToString();
                if (node.ChildNodes.Count > 0)
                {
                    index = ReIndexNodes(node.ChildNodes, index);
                }
            }
            return index;
        }

        /// <summary>
        /// A helper class to examine the hidden field.
        /// </summary>
        private struct HiddenPair
        {
            public string name;
            public string value;

            public static HiddenPair ParsePair(string property)
            {
                string[] split = property.Split(':');
                HiddenPair pair = new HiddenPair { name = split[0], value = split[1] };
                return pair;
            }
        };

        private bool hiddenFieldsLoaded = false;

        /// <summary>
        /// Ensures the HiddenFileld values to be loaded.
        /// </summary>
        public void EnsureHiddenFieldsLoaded()
        {
            if (!hiddenFieldsLoaded)
            {
                hiddenFieldsLoaded = true;
                LoadHiddenFieldValues();
            }
        }

        /// <summary>
        /// Load the values of all nodes that where changed on client side (javascript):
        /// </summary>
        private void LoadHiddenFieldValues()
        {
            string value = Page.Request.Form[hiddenFieldName] as string;
            Debug.WriteLine("Hidden: " + value);
            if (!string.IsNullOrEmpty(value))
            {
                string[] properties = value.Split(';');
                foreach (string property in properties)
                {
                    HiddenPair pair = HiddenPair.ParsePair(property);
                    switch (pair.name)
                    {
                        case "exp": SetExpandedNodesFromClient(pair.value); break;
                        case "sel": SetSelectedNodesFromClient(pair.value); break;
                        case "cb": SetCheckedNodesFromClient(pair.value); break;
                        case "sn": SetSelectedNodeFromClient(pair.value); break;
                        case "txt": SetNodeTextFromClient(pair.value); break;

                    }
                }
            }
        }

        private void SetNodeTextFromClient(string commaDelimitedIndexes)
        {
            string[] split = commaDelimitedIndexes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in split)
            {
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    int index = int.Parse(keyValue[0]);
                    if (nodesByKey.ContainsKey(index))
                    {
                        OnNodeEdited(nodesByKey[index], keyValue[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when a treeNode is edited.
        /// </summary>
        protected virtual void OnNodeEdited(OdcTreeNode node, string newText)
        {
            node.Text = newText;
            if (NodeEdited != null) NodeEdited(this, new OdcTreeNodeEventArgs(node));
        }

        /// <summary>
        /// Occurs when a treeNode is edited.
        /// </summary>
        public event OdcTreeNodeEventHandler NodeEdited;

        private void SetSelectedNodeFromClient(string value)
        {
            int index = int.Parse(value);

            OdcTreeNode node = GetNodeByIndex(index);
            SelectedNodekey = node != null ? index : -1;
        }

        private void SetCheckedNodesFromClient(string commaDelimitedIndexes)
        {
            string[] split = commaDelimitedIndexes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<int, bool> nodes = (from value in split select int.Parse(value)).ToDictionary(x => x, x => true);

            foreach (OdcTreeNode node in nodesByKey.Values)
            {
                bool isChecked = nodes.ContainsKey(node.Key);
                if (isChecked != node.IsChecked)
                {
                    node.IsChecked = isChecked;
                    OnNodeCheckedChanged(node);
                }
            }
        }

        protected virtual void OnNodeCheckedChanged(OdcTreeNode node)
        {
            if (NodeCheck != null) NodeCheck(this, new OdcTreeNodeCheckEventArgs(node, node.IsChecked));
        }



        private List<OdcTreeNode> selectedNodes = new List<OdcTreeNode>();

        /// <summary>
        /// Gets a collection of all selected nodes when MultiSelect is set to true, otherwhise it contains either the SelectedNode or null.
        /// </summary>
        public IEnumerable<OdcTreeNode> SelectedNodes
        {
            get { return selectedNodes; }
        }

        private void SetSelectedNodesFromClient(string commaDelimitedIndexes)
        {
            string[] split = commaDelimitedIndexes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<int, bool> nodes = (from value in split select int.Parse(value)).ToDictionary(x => x, x => true);

            foreach (OdcTreeNode node in nodesByKey.Values)
            {
                if (node.IsSelected)
                {
                    selectedNodes.Add(node);
                }
            }
        }

        private void SetExpandedNodesFromClient(string commaDelimitedIndexes)
        {
            string[] split = commaDelimitedIndexes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<int, bool> nodes = (from value in split select int.Parse(value)).ToDictionary(x => x, x => true);

            foreach (OdcTreeNode node in nodesByKey.Values)
            {
                node.IsExpanded = nodes.ContainsKey(node.Key);
            }
        }

        private OdcTreeViewContextMenuContainer contextMenu;

        /// <summary>
        /// Gets the container control for the Context Menu.
        /// </summary>
        public WebControl ContextMenu
        {
            get
            {
                EnsureChildControls();
                return contextMenu;
            }
        }


        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        /// <remarks>
        /// Version 0.1.0.2: Changed the order of method calls to enable controls in a template that require viewstate.
        /// </remarks>
        protected override void CreateChildControls()
        {
            Debug.WriteLine("CreateChildControls");

            if (NodeTemplate == null) NodeTemplate = new DefaultTemplate(false);
            if (EditNodeTemplate == null) EditNodeTemplate = new DefaultTemplate(true);

            // IMPORTANT: to not the order of the following two line since it will affect event bubbling:  
            EnsureNodesPopulated();
            InitContextMenu();
            PerformSelect();
            DataBind();
            base.CreateChildControls();
            EnsureHiddenFieldsLoaded();

            // the templates for each node must be instantiated to register an event fired by a template control:
            InstantiateNodes(false);
            InstantiateContextMenu();

        }


        bool selected = false;

        /// <summary>
        /// Retrieves data from the associated data source.
        /// </summary>
        protected override void PerformSelect()
        {
            if (!selected)
            {
                selected = true;
                if (IsDataBound)
                {
                    Debug.WriteLine("PerformSelect");
                    BindDataSource(Nodes, "");
                }
            }

            base.PerformSelect();
        }

        private void BindDataSource(OdcTreeNodeCollection nodes, string viewPath)
        {

            HierarchicalDataSourceView view = GetData(viewPath);
            IHierarchicalEnumerable he = view.Select();

            // pre-sort the available node bindings by the level:
            OdcTreeNodeBinding binding = GetTreeNodeBindingForLevel(nodes.Depth);
            foreach (object e in he)
            {
                IHierarchyData data = he.GetHierarchyData(e);

                OdcTreeNode node = new OdcTreeNode(this);
                node.DataPath = data.Path;
                node.DataItem = e;
                node.Value = null;
                node.IsDataBound = true;
                node.Text = data.Item.ToString();
                nodes.Add(node);

                if (binding != null) AttachAndBindNode(node, binding);

                if (!node.IsExpanded.HasValue) node.IsExpanded = node.Depth < ExpandDepth;
                PrepareNode(node);

                if (data.HasChildren)
                {
                    if (node.IsNodeExpanded())
                    {
                        BindDataSource(node.ChildNodes, data.Path);
                    }
                    else
                    {
                        node.PopulateOnDemand = true;
                    }
                }
            }
        }

        /// <summary>
        /// Attach and bind all values from OdcTreeNode to the OdcTreeNode 
        /// </summary>
        /// <param name="node">The OdcTreeNode to attach.</param>
        /// <param name="binding">The OdcTreeNodeBinding with information.</param>
        private void AttachAndBindNode(OdcTreeNode node, OdcTreeNodeBinding binding)
        {
            object dataItem = node.DataItem;
            binding = OnNodeBinding(node, binding, dataItem);
            if (binding.ShowCheckBox.HasValue) node.ShowCheckBox = binding.ShowCheckBox;
            if (binding.IsChecked.HasValue) node.IsChecked = binding.IsChecked.Value;
            if (!string.IsNullOrEmpty(binding.ImageUrl)) node.ImageUrl = binding.ImageUrl;

            if (!string.IsNullOrEmpty(binding.ImageUrlField)) node.ImageUrl = (string)DataBinder.Eval(dataItem, binding.ImageUrlField);
            if (!string.IsNullOrEmpty(binding.ShowCheckBoxField)) node.ShowCheckBox = (bool?)DataBinder.Eval(dataItem, binding.ShowCheckBoxField);
            if (!string.IsNullOrEmpty(binding.TextField)) node.Text = DataBinder.Eval(dataItem, binding.TextField) as string;
            if (!string.IsNullOrEmpty(binding.ValueField)) node.Value = DataBinder.Eval(dataItem, binding.ValueField);
            if (!string.IsNullOrEmpty(binding.CheckedField)) node.IsChecked = (bool)DataBinder.Eval(dataItem, binding.CheckedField);
        }

        /// <summary>
        /// Returns a collection of TreeNodeBindings for the specified level.
        /// </summary>
        /// <param name="level">The level in the hierarchy for which to get the bindings.</param>
        /// <returns>All available TreeNodeBindings sorted descending by the level, which can be less or equal to the parameter level.</returns>
        /// <remarks>
        /// I'm using the two parte mechanism of GetTreeNodeBindingForLevel and GetTreeNodeBindingByMember since it would cause a better performance
        /// for data bound nodes, since GetTreeNodeBindingForLevel is performed only once for a level and not all nodes of the level separately.
        /// </remarks>
        private OdcTreeNodeBinding GetTreeNodeBindingForLevel(int level)
        {
            var result = from b in TreeNodeBindings
                         where b.Level <= level
                         orderby b.Level descending
                         select b;

            return result.FirstOrDefault();
        }


        /// <summary>
        /// Populates the child nodes of a node from a hierarchical data source.
        /// </summary>
        /// <param name="node">The node for which to populate the child nodes.</param>
        private void PopulateNodeFromDataSource(OdcTreeNode node)
        {
            node.PopulateOnDemand = false;
            HierarchicalDataSourceView view = GetData(node.DataPath);
            IHierarchicalEnumerable he = view.Select();

            IHierarchyData data = he.GetHierarchyData(node.DataItem);
            BindDataSource(node.ChildNodes, data.Path);
        }


        /// <summary>
        /// Initialize the context menu if available.
        /// </summary>
        private void InitContextMenu()
        {
            if (ContextMenuTemplate != null)
            {
                contextMenu = new OdcTreeViewContextMenuContainer();
                contextMenu.ID = "ContextMenu";
                contextMenu.EnableViewState = false;
                Controls.Add(contextMenu);
            }
        }

        /// <summary>
        /// Instantiate all nodes of the tree, which means to assign a template to each node.
        /// </summary>
        /// <param name="bind">Specifies whether to bind the nodes.</param>
        /// <remarks>
        /// Version 0.1.0.2:
        /// Changed the order of method calls to enable controls in a template that require viewstate.
        /// </remarks>
        private void InstantiateNodes(bool bind)
        {
            Debug.WriteLine("InstantiateNodes");
            foreach (KeyValuePair<int, OdcTreeNode> pair in nodesByKey)
            {
                OdcTreeNode node = pair.Value;

                InstantiateNode(node);
                if (bind) node.Container.DataBind();
            }
        }

        /// <summary>
        /// Instantiates a single node.
        /// </summary>
        /// <param name="node">The node to instantiate</param>
        /// <remarks>
        /// Version 0.1.0.2:
        /// The template is now no longer always recreated, to preserve the viewstate of embedded control in a template, like DropDownList.
        /// Now the template is only recreated if it has changed after it was first created with CreateChildControls and maybe changed
        /// due to a method call triggered by a PostBack event.
        /// </remarks>
        private void InstantiateNode(OdcTreeNode node)
        {
            ITemplate template = node.EditMode ? EditNodeTemplate : NodeTemplate;
            OdcTreeNodeBinding binding = OnNodeBinding(node, null, node);
            if (binding != null)
            {
                AttachAndBindNode(node, binding);
                if (binding.NodeTemplate != null) template = binding.NodeTemplate;
            }
            if (template != node.Template)
            {
                OdcTreeNodeContainer container = node.Container;
                node.Template = template;
                container.Controls.Clear();
                template.InstantiateIn(container);
            }
        }



        /// <summary>
        /// Instantiate a template to the context menu if available.
        /// </summary>
        private void InstantiateContextMenu()
        {

            if (ContextMenuTemplate != null && contextMenu != null)
            {
                contextMenu.Controls.Clear();
                ContextMenuTemplate.InstantiateIn(contextMenu);
                contextMenu.DataBind();
            }
        }

        private bool nodesInited = false;

        /// <summary>
        /// Ensure that the nodes are populated.
        /// </summary>
        protected void EnsureNodesPopulated()
        {
            if (!nodesInited)
            {
                nodesInited = true;
                PopulateNodes();
            }
        }

        /// <summary>
        /// Populates all nodes and which are marked with PopulateOnDemand set to true.
        /// </summary>
        private void PopulateNodes()
        {
            Debug.WriteLine("PopulateNodes");
            this.PopulateNode(Nodes);
        }

        private bool? isDataBound;
        /// <summary>
        /// Gets whether tree is data bound.
        /// </summary>
        public bool IsDataBound
        {
            get
            {
                if (!isDataBound.HasValue)
                {
                    isDataBound = IsBoundUsingDataSourceID;
                }
                return isDataBound.Value;
            }
        }

        /// <summary>
        /// Gets or sets the object from which the data-bound control retrieves its list of data items.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An object that represents the data source from which the data-bound control retrieves its data. The default is null.
        /// </returns>
        public override object DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                isDataBound = true;
                base.DataSource = value;
            }
        }

        #region state management

        [Serializable]
        struct TreeViewStateBag
        {
            object nodes;
            int editNodeIndex;
            int selectedNodeIndex;
            checkedNodeBag[] checkedNodes;

            public TreeViewStateBag(OdcTreeView view)
            {
                editNodeIndex = view.EditNodeKey;
                selectedNodeIndex = view.SelectedNodekey;
                nodes = null;
                checkedNodes = null;
                if (!view.IsDataBound)
                {
                    nodes = view.Nodes.SaveViewState();
                }
                else
                {
                    nodes = view.expandedNodes.ToArray();
                }
                checkedNodes = view.GetCheckedNodes();
            }

            internal void Load(OdcTreeView view)
            {
                string[] expandedNodes = nodes as string[];
                if (expandedNodes != null)
                {
                    view.expandedNodes.Clear();
                    foreach (string s in expandedNodes)
                    {
                        view.expandedNodes.Add(s);
                    }
                }
                else
                {
                    if (nodes != null) view.Nodes.LoadViewState(nodes);
                }
                view.SetCheckedNodes(checkedNodes);
                view.SelectedNodekey = selectedNodeIndex;
                view.EditNodeKey = editNodeIndex;
            }
        }

        HashSet<string> expandedNodes = new HashSet<string>();


        protected override object SaveViewState()
        {
            if (this.IsViewStateEnabled)
            {
                Debug.WriteLine("Save ViewState!");
                TreeViewStateBag state = new TreeViewStateBag(this);
                return state;
            }
            else return null;
        }

        protected override void LoadViewState(object savedState)
        {
            //    PerformSelect();
            Debug.WriteLine("Load ViewState");
            TreeViewStateBag state = (TreeViewStateBag)savedState;
            state.Load(this);
        }
        #endregion

        [Browsable(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [MergableProperty(false)]
        public OdcTreeNodeCollection Nodes { get; private set; }

        /// <summary>
        /// Gets a collection of script descriptors that represent ECMAScript (JavaScript) client components.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerable"/> collection of <see cref="T:System.Web.UI.ScriptDescriptor"/> objects.
        /// </returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Odyssey.Web.TreeViewControl", this.ClientID);
            descriptor.AddProperty("hiddenFieldName", hiddenFieldName);

            if (EnableDragDrop) descriptor.AddProperty("enableDragDrop", EnableDragDrop);
            if (EnableDblClick) descriptor.AddProperty("dblClickEnabled", EnableDblClick);
            if (MultiSelect) descriptor.AddProperty("multiSelect", MultiSelect);
            if (AutoPostBack) descriptor.AddProperty("autoPostBack", AutoPostBack);
            if (AllowNodeEditing) descriptor.AddProperty("allowNodeEditing", AllowNodeEditing);
            if (!DisableTextSelection) descriptor.AddProperty("disableTextSelection", DisableTextSelection);
            if (EditNodeKey >= 0) descriptor.AddProperty("editNodeIndex", EditNodeKey);
            if (!string.IsNullOrEmpty(ClientContextMenuOpening)) descriptor.AddEvent("contextMenu", ClientContextMenuOpening);
            if (!string.IsNullOrEmpty(ClientNodeSelectionChanged)) descriptor.AddEvent("nodeSelectionChanged", ClientNodeSelectionChanged);
            if (!string.IsNullOrEmpty(ClientNodeExpanded)) descriptor.AddEvent("nodeExpanded", ClientNodeExpanded);
            if (!string.IsNullOrEmpty(ClientNodeCollapsed)) descriptor.AddEvent("nodeCollapsed", ClientNodeCollapsed);
            if (!string.IsNullOrEmpty(ClientNodeCheckedChanged)) descriptor.AddEvent("checkedChanged", ClientNodeCheckedChanged);
            if (!string.IsNullOrEmpty(ClientNodeTextChanged)) descriptor.AddEvent("nodeTextChanged", ClientNodeTextChanged);
            if (!string.IsNullOrEmpty(ClientEditModeChanged)) descriptor.AddEvent("nodeEditModeChanged", ClientEditModeChanged);
            if (CaptureAllClicks) descriptor.AddProperty("captureAllClicks", CaptureAllClicks);
            yield return descriptor;

        }

        /// <summary>
        /// Gets a collection of <see cref="T:System.Web.UI.ScriptReference"/> objects that define script resources that the control requires.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerable"/> collection of <see cref="T:System.Web.UI.ScriptReference"/> objects.
        /// </returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("Odyssey.Web.TreeView.client.TreeViewControl.js", this.GetType().Assembly.FullName);
            yield return new ScriptReference("Odyssey.Web.TreeView.client.TreeNode.js", this.GetType().Assembly.FullName);
        }

        /// <summary>
        /// Gets the name of the hidden field to exchange data between client and server that have changed on client side.
        /// </summary>
        private string hiddenFieldName
        {
            get
            {
                return this.ClientID + "$clientData";
            }
        }


        private Dictionary<int, OdcTreeNode> nodesByKey = new Dictionary<int, OdcTreeNode>();
        private Dictionary<int, OdcTreeNodeContainer> templateItemsByKey = new Dictionary<int, OdcTreeNodeContainer>();

        /// <summary>
        /// Ensures that a node and all child nodes are populated if marked with PopulateOnDemand set to true.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="populate"></param>
        private void PopulateNode(OdcTreeNodeCollection nodes)
        {
            int max = nodes.Count - 1;
            for (int i = 0; i <= max; i++)
            {
                OdcTreeNode node = nodes[i];
                if (node.PopulateOnDemand && node.IsNodeExpanded()) OnNodePopulate(node);
                PopulateNode(node.ChildNodes);
            }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> value that corresponds to this Web server control. This property is used primarily by control developers.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// One of the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> enumeration values.
        /// </returns>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }


        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute("uniqueId", this.UniqueID);
            writer.AddAttribute("class", ClassName);
            base.RenderBeginTag(writer);
        }

        const string defaultClassName = "odcTreeView";
        private string className = defaultClassName;

        /// <summary>
        /// Gets or sets the class name that is used to attach the style of the tree view.
        /// </summary>
        [Description("Gets or sets the class name that is used to attach the style of the tree view.")]
        [DefaultValue(defaultClassName)]
        public string ClassName
        {
            get { return className; }
            set { className = value; }
        }


        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (!this.DesignMode)
            {
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            }

            base.Render(writer);
        }


        /// <summary>
        /// Outputs the content of a server control's children to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the rendered content.</param>
        protected override void RenderChildren(HtmlTextWriter writer)
        {
            if (!this.DesignMode)
            {
                RenderNodes(writer, Nodes, "", true);
                if (contextMenu != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, "contextMenu");
                    contextMenu.RenderControl(writer);
                }
            }
            else
            {
                RenderDesignAppearance(writer);
            }
        }

        /// <summary>
        /// Renders the shape of the control in design mode.
        /// </summary>
        /// <param name="writer"></param>
        private void RenderDesignAppearance(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "solid");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "black");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "2px");
            writer.AddStyleAttribute("height", "40px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write("OdcTreeView");
            writer.WriteBreak();
            string id = this.ID ?? "";
            writer.Write(id);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets or sets the template for the context menu.
        /// </summary>
        [TemplateContainer(typeof(OdcTreeViewContextMenuContainer))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate ContextMenuTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for a OdcTreeNode. If set to null, a default template is used.
        /// </summary>
        [TemplateContainer(typeof(OdcTreeNodeContainer), BindingDirection.TwoWay)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public IBindableTemplate NodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for a OdcTreeNode  in EditMode. If set to null, a default template is used.
        /// </summary>
        [TemplateContainer(typeof(OdcTreeNodeContainer), BindingDirection.TwoWay)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public IBindableTemplate EditNodeTemplate { get; set; }

        /// <summary>
        /// Renders all OdcTreeNodes.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter class</param>
        /// <param name="nodes">a collection of OdcTreeNodes.</param>
        /// <param name="css">The css style to apply.</param>
        protected void RenderNodes(HtmlTextWriter writer, OdcTreeNodeCollection nodes, string css, bool isRoot)
        {
            css = string.IsNullOrEmpty(css) ? "ul" : "ul " + css;
            writer.AddAttribute(HtmlTextWriterAttribute.Class, css);
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            foreach (OdcTreeNode node in nodes)
            {
                isRoot = false;
                OdcTreeNodeContainer item = node.Container;
                writer.AddAttribute(HtmlTextWriterAttribute.Class, isRoot ? "Root" : node.IsLast ? "Last" : "Mid");
                writer.AddAttribute("key", node.Key.ToString());
                if (node.CanEdit == false) writer.AddAttribute("canEdit", "false");
                if (node.PopulateOnDemand) writer.AddAttribute("populate", "true");
                writer.RenderBeginTag(HtmlTextWriterTag.Li);

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "div");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                if (node.HasToggleButton())
                {
                    bool expanded = node.IsExpanded ?? true;
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, expanded ? "Collapse" : "Expand");
                    writer.AddAttribute("event", expanded ? "collapse" : "expand");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.RenderEndTag();  // span
                }

                writer.AddAttribute("event", "click");
                string spanCss = "span";
                if (node.IsSelected) spanCss = spanCss + " Sel";

                if (!string.IsNullOrEmpty(node.CssClass))
                {
                    spanCss += " " + node.CssClass;
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Class, spanCss);
                item.RenderControl(writer);
                writer.RenderEndTag();  // div
                if (node.PopulateOnDemand || node.ChildNodes.Count > 0)
                {
                    string subCss = !node.IsLast ? "Ln" : "";
                    if (!node.IsNodeExpanded())
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "collapsed");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                    }
                    RenderNodes(writer, node.ChildNodes, subCss, false);
                }
                writer.RenderEndTag(); // li
            }
            writer.RenderEndTag();  // ul
        }

        #region IPostBackEventHandler Members

        /// <summary>
        ///  Handle the __postBack events from the javascript client.
        /// </summary>
        /// <param name="eventArgument">The name of the postBack event to be handled.</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            //BindValues();  // Note: do not bind the modified data, but cancel it. only alow binding caused by a postback event from another control at OnBubbleEvent.
            string[] para = eventArgument.Split(':');
            if (para.Length >= 2)
            {
                string command = para[0];
                int index = int.Parse(para[1]);
                OdcTreeNode node = GetNodeByIndex(index);

                OnBeforePostBack(node);

                switch (command)
                {
                    case "click": OnNodeClick(node); break;
                    case "expand": OnNodeExpanded(node); break;
                    case "collapse": OnNodeCollapsed(node); break;
                    case "check": OnCheck(node, para[2]); break;
                    case "dblclk": OnDblClick(node); break;
                    case "edit": OnEditNode(node, para[2]); break;
                    case "cancelEdit": OnCancelEditMode(node); break;
                }

                OnAfterPostBack(node);
            }
        }

        #endregion

        /// <summary>
        /// Raises after a postback event has occured.
        /// </summary>
        protected virtual void OnAfterPostBack(OdcTreeNode node)
        {
            if (AfterPostBack != null)
            {
                AfterPostBack(this, new OdcTreeNodeEventArgs(node));
            }
        }

        /// <summary>
        /// Raises after a postback event has occured.
        /// </summary>
        public event OdcTreeNodeEventHandler AfterPostBack;

        /// <summary>
        /// Raises when before a postback event has occures.
        /// </summary>
        protected virtual void OnBeforePostBack(OdcTreeNode node)
        {
            if (BeforePostBack != null)
            {
                BeforePostBack(this, new OdcTreeNodeEventArgs(node));
            }
        }

        /// <summary>
        /// Raises when before a postback event has occures.
        /// </summary>
        public event OdcTreeNodeEventHandler BeforePostBack;


        /// <summary>
        /// Occurs when the IsChecked state of a node has changed.
        /// </summary>
        protected virtual void OnCheck(OdcTreeNode node, string value)
        {
            bool isChecked = value == "true";
            node.IsChecked = isChecked;

            if (NodeCheck != null)
            {
                NodeCheck(this, new OdcTreeNodeCheckEventArgs(node, isChecked));
            }
        }


        /// <summary>
        /// Gets the node by it's Node.Index value.
        /// </summary>
        /// <param name="index">The Node.Index value.</param>
        /// <returns>OdcTreeNode otherwise null.</returns>
        public OdcTreeNode GetNodeByIndex(int index)
        {
            return nodesByKey.ContainsKey(index) ? nodesByKey[index] : null;
        }

        /// <summary>
        /// Determines whether the event for the server control is passed up the page's UI server control hierarchy.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        /// <returns>
        /// true if the event has been canceled; otherwise, false. The default is false.
        /// </returns>
        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            CommandEventArgs commandEvent = args as CommandEventArgs;
            if (commandEvent != null)
            {
                if (!commandEvent.CommandName.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                {
                    // upate the values from the templaty only, if no cancel event is fired:
                    ExtractValuesFromNodeTemplates();
                }
                EnsureHiddenFieldsLoaded();
                EnsureNodesPopulated();
                Debug.WriteLine("Bubbling");
                OnNodeCommand(source, commandEvent);
                return false;
            }
            return base.OnBubbleEvent(source, args);
        }

        /// <summary>
        /// Extracts the values that where changed in each node's template
        /// </summary>
        private void ExtractValuesFromNodeTemplates()
        {
            foreach (OdcTreeNode node in nodesByKey.Values)
            {
                ExtractValueFromNodeTemplate(node);
            }
        }


        /// <summary>
        /// Extracts the values that where changed in a node's template.
        /// </summary>
        /// <param name="node">the node to update.</param>
        private void ExtractValueFromNodeTemplate(OdcTreeNode node)
        {
            OdcTreeNodeContainer container = node.Container;
            IOrderedDictionary d = node.Key == EditNodeKey ? EditNodeTemplate.ExtractValues(container) : NodeTemplate.ExtractValues(container);

            Object dataItem = container.DataItem;
            if (dataItem != null)
            {
                if (dataItem == node)
                {
                    // when the data item is the node, then directly modify the values instead of using slower reflection:
                    string newText = node.Text;
                    if (d.Contains("Text")) newText = d["Text"] as string;
                    if (d.Contains("Value")) node.Value = d["Value"];
                    if (newText != node.Text) OnNodeEdited(node, newText);
                }
                else
                {
                    // since the data item is an unknown class, reflection is necessary to modify the updated values from the template to the item:
                    Type type = dataItem.GetType();
                    foreach (string key in d.Keys)
                    {
                        PropertyInfo info = type.GetProperty(key);
                        if (info != null)
                        {
                            info.SetValue(dataItem, d[key], null);
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Occurs when a CommandEvent has bubbled.
        /// </summary>
        protected void OnNodeCommand(Object source, CommandEventArgs commanEvent)
        {

            OdcTreeNode node = this.SelectedNode;
            if (NodeCommand != null && node != null) NodeCommand(this, new OdcTreeViewCommandEventArgs(source, commanEvent, node));
        }

        private int editNodeIndex = -1;

        /// <summary>
        /// Gets or sets the key index of the node in edit mode.
        /// </summary>
        public int EditNodeKey
        {
            get { return editNodeIndex; }
            set { editNodeIndex = value; }
        }

        private int selectedNodeIndex = -1;

        /// <summary>
        /// Gets or sets the key index of the selected node.
        /// </summary>
        public int SelectedNodekey
        {
            get { return selectedNodeIndex; }
            set
            {
                if (selectedNodeIndex != value)
                {
                    selectedNodeIndex = value;
                    this.TrackViewState();
                }
            }
        }

        /// <summary>
        /// Gets the selected node, otherwise null.
        /// </summary>       
        public OdcTreeNode SelectedNode
        {
            get
            {
                int idx = selectedNodeIndex;
                return idx >= 0 ? GetNodeByIndex(idx) : null;
            }
        }


        protected virtual void OnCancelEditMode(OdcTreeNode node)
        {
            if (CancelEditMode != null) CancelEditMode(this, new OdcTreeNodeEventArgs(node));
        }

        /// <summary>
        /// Occurs when the EditNode property has changed.
        /// </summary>
        public event OdcTreeNodeEventHandler CancelEditMode;



        /// <summary>
        /// Occurs when a CommandEvent has bubbled.
        /// </summary>
        public event OdcTreeViewCommandEventHandler NodeCommand;


        protected void OnDblClick(OdcTreeNode node)
        {
            if (DblClick != null) DblClick(this, new OdcTreeNodeEventArgs(node));
        }

        /// <summary>
        /// Occurs when a node was double clicked.
        /// </summary>
        public event OdcTreeNodeEventHandler DblClick;

        /// <summary>
        /// Occurs wehen the client notifies the server to set a node to edit mode.
        /// </summary>
        protected void OnEditNode(OdcTreeNode node, string para)
        {
            bool editMode = para == "true";
            if (EditNode != null && editMode) EditNode(this, new OdcTreeNodeEventArgs(node));
        }

        /// <summary>
        /// Occurs wehen the client notifies the server to set a node to edit mode.
        /// </summary>
        public event OdcTreeNodeEventHandler EditNode;

        /// <summary>
        /// Occurs when a node was clicked.
        /// </summary>
        protected virtual void OnNodeClick(OdcTreeNode node)
        {
            ChangeSelection(node);
            if (NodeClick != null) NodeClick(this, new OdcTreeNodeEventArgs(node));
        }

        private void ChangeSelection(OdcTreeNode node)
        {
            SelectedNodekey = node != null ? node.Key : -1;
            var selectedNodes = from n in nodesByKey where n.Value.IsSelected == true select n.Value;
            OdcTreeNode selectedNode = selectedNodes.FirstOrDefault();
            TrackViewState();
        }

        /// <summary>
        /// Occurs when a node was collapsed.
        /// </summary>
        protected virtual void OnNodeCollapsed(OdcTreeNode node)
        {
            node.IsExpanded = false;
            TrackViewState();
            if (NodeCollapsed != null) NodeCollapsed(this, new OdcTreeNodeEventArgs(node));
        }

        /// <summary>
        /// Occurs when a node was expanded.
        /// </summary>
        protected virtual void OnNodeExpanded(OdcTreeNode node)
        {
            node.IsExpanded = true;
            TrackViewState();
            if (NodeExpanded != null) NodeExpanded(this, new OdcTreeNodeEventArgs(node));
            if (node.PopulateOnDemand) OnNodePopulate(node);
        }

        /// <summary>
        /// Occurs when to populate the ChildNodes of a node.
        /// </summary>
        /// <param name="node">The node for which to populate the ChildNodes.</param>
        protected internal virtual void OnNodePopulate(OdcTreeNode node)
        {
            Debug.WriteLine("Populating");
            if (node.IsDataBound)
            {
                PopulateNodeFromDataSource(node);
            }
            else
            {
                if (NodePopulate != null)
                {
                    NodePopulate(this, new OdcTreeNodeEventArgs(node));
                }
            }
        }


        /// <summary>
        /// Occurs when to populate the ChildNodes of a node.
        /// </summary>
        /// <param name="node"></param>
        public event OdcTreeNodeEventHandler NodePopulate;

        /// <summary>
        /// Occurs when a node was clicked.
        /// </summary>
        public event OdcTreeNodeEventHandler NodeClick;

        /// <summary>
        /// Occurs when a node was expanded
        /// </summary>
        public event OdcTreeNodeEventHandler NodeExpanded;

        /// <summary>
        /// Occurs when a node was collapsed
        /// </summary>
        public event OdcTreeNodeEventHandler NodeCollapsed;

        /// <summary>
        /// Occurs when the IsChecked state of a node has changed.
        /// </summary>
        public event OdcTreeNodeCheckEventHandler NodeCheck;


        #region IPostBackDataHandler Members

        /// <summary>
        /// When implemented by a class, processes postback data for an ASP.NET server control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control.</param>
        /// <param name="postCollection">The collection of all incoming name values.</param>
        /// <returns>
        /// true if the server control's state changes as a result of the postback; otherwise, false.
        /// </returns>
        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            return true;
        }

        public void RaisePostDataChangedEvent()
        {

        }

        #endregion


        /// <summary>
        /// Gets or sets whether to allow selection of multible nodes.
        /// </summary>
        /// <remarks>This property is not supported yet.</remarks>
        [DefaultValue(false)]
        public bool MultiSelect { get; set; }

        /// <summary>
        /// Gets or set whether the TreeView uses client side (javascript) selection with no postback.
        /// If set to true, every click causes a postback to the server, otherwise a postback is not necassarily performed, if a node is
        /// selected, expanded or collapsed unless it is necassary to update data from the server.
        /// </summary>
        [DefaultValue(false)]
        public bool AutoPostBack { get; set; }

        /// <summary>
        /// Secifies wether the client should recognize double clicks and inform the server if so.
        /// </summary>
        internal bool EnableDblClick { get; set; }

        /// <summary>
        /// Gets or sets whether drag & drop with nodes is enabled on client side (via javascript).
        /// </summary>
        [DefaultValue(false)]
        public bool EnableDragDrop { get; set; }

        /// <summary>
        /// Gets all nested nodes from this TreeView.
        /// </summary>
        /// <returns>A collection of all nested nodes.</returns>
        /// <remarks>
        /// Version 0.1.0.2:
        /// changed from creating a List<OdcTreeNode/> to using yield return to gather the data.
        /// </remarks>
        public IEnumerable<OdcTreeNode> GetAllNodes()
        {
            return IterateSubNodes(Nodes);
        }

        private IEnumerable<OdcTreeNode> IterateSubNodes(OdcTreeNodeCollection nodes)
        {
            foreach (OdcTreeNode node in nodes)
            {
                yield return node;
                foreach (OdcTreeNode sub in IterateSubNodes(node.ChildNodes))
                {
                    yield return sub;
                }
            }
        }


        /// <summary>
        /// Adds a  OdcTreeNode and all nested sub-nodes to a collection
        /// </summary>
        /// <param name="odcTreeNodeCollection">The collection into which to add the nodes.</param>
        /// <param name="nodes">The nodes to add itself and it's nested sub-nodes.</param>
        private void AddSubNodes(OdcTreeNodeCollection odcTreeNodeCollection, List<OdcTreeNode> nodes)
        {
            foreach (OdcTreeNode node in odcTreeNodeCollection)
            {
                nodes.Add(node);
                AddSubNodes(node.ChildNodes, nodes);
            }
        }

        /// <summary>
        /// Gets or sets whether to allow node text editing on client side.
        /// </summary>
        /// <remarks>
        /// In combination with NodeTemplate, you must specify an element with isText="true" that contains the text to be edited as innerHTML.
        /// </remarks>
        /// <example>
        /// <asp:Label isText="true" runat="server" text="edit me"/><asp:LinkButton runat="server" Text="update"/>
        /// </example>
        [DefaultValue(false)]
        [Description("Gets or sets whether to allow node text editing on client side.")]
        public bool AllowNodeEditing { get; set; }

        /// <summary>
        /// Gets or sets whether theclient disabled selection of text while moving the mouse with pressed buttons.
        /// </summary>
        [DefaultValue(true)]
        [Description("Gets or sets whether the client disabled selection of text while moving the mouse with pressed buttons.")]
        public bool DisableTextSelection { get; set; }

        /// <summary>
        /// The internal counter for unique Index properties associated with this TreeView.
        /// </summary>
        private int nodeKey = 1;

        /// <summary>
        /// Prepare the node with additional data required for the TreeView.
        /// Particullarly, this checks wether an index is applied to the node and updates the internal index generator, otherwise it
        /// creates an unique index for this node.
        /// </summary>
        /// <param name="item">The node to prepare.</param>
        protected virtual void PrepareNode(OdcTreeNode node)
        {
            node.TreeView = this;
            if (node.Key <= 0)
            {
                node.Key = nodeKey++;
            }
            else
            {
                nodeKey = Math.Max(nodeKey, node.Key + 1);
            }
            if (expandedNodes.Contains(node.DataPath)) node.IsExpanded = true;
            if (checkedNodes != null && node.ShowCheckBox.HasValue && node.ShowCheckBox.Value == true)
            {
                string key = node.DataPath;
                if (checkedNodes.ContainsKey(key)) node.IsChecked = checkedNodes[key];
            }
        }

        /// <summary>
        /// Registers a node to the treeview.
        /// </summary>
        /// <param name="node"></param>
        internal protected virtual void RegisterNode(OdcTreeNode node)
        {
            PrepareNode(node);

            if (!nodesByKey.ContainsKey(node.Key)) nodesByKey.Add(node.Key, node);
            if (node.Container == null)
            {
                OdcTreeNodeContainer container = new OdcTreeNodeContainer(node, node.Key);

                // the container must have an id that can be reproduced on postbacks, otherwise controls in the template that cause postbacks will not be recognized:
                container.ID = "K" + node.Key.ToString();

                container.EnableViewState = true; // 0.1.2: this is necassary when adding controls in a Template that require viewstates.
                node.Container = container;
                templateItemsByKey.Add(node.Key, container);
                Controls.Add(container);
                if (node.IsSelected) SelectedNodekey = node.Key;
            }
        }

        /// <summary>
        /// Unregisters a node from the treeview.
        /// </summary>
        /// <param name="node">The node to unregister.</param>
        internal protected virtual void UnRegisterNode(OdcTreeNode node)
        {
            nodesByKey.Remove(node.Key);
        }

        [DefaultValue(1)]
        public int ExpandDepth { get; set; }

        /// <summary>
        /// used to update the expandedNodes HashSet which is used for ViewState.
        /// </summary>
        /// <param name="node"></param>
        internal void NotifyExpandedOrCollapsed(OdcTreeNode node)
        {
            string dataPath = node.DataPath;
            if (dataPath != null)
            {
                if (node.IsNodeExpanded())
                {
                    if (!expandedNodes.Contains(dataPath)) expandedNodes.Add(dataPath);
                }
                else expandedNodes.Remove(dataPath);
            }
        }

        private OdcTreeNodeBindings treeNodeBindings = new OdcTreeNodeBindings();

        /// <summary>
        /// Gets a collection of TreeNodeBindings that specify how to bind a node depending on the level and member.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public OdcTreeNodeBindings TreeNodeBindings
        {
            get { return treeNodeBindings; }
        }


        /// <summary>
        /// Occurs when a OdcTreeNodeBinding must be selected.
        /// </summary>
        /// <returns>The OdcTreeNodeBinding to use for this node.</returns>
        protected OdcTreeNodeBinding OnNodeBinding(OdcTreeNode node, OdcTreeNodeBinding binding, object dataItem)
        {
            //Trace.WriteLine("Bind " + node.Text);
            if (NodeBinding != null)
            {
                OdcTreeNodeBindingEventArgs e = new OdcTreeNodeBindingEventArgs(node, binding, dataItem, TreeNodeBindings);
                NodeBinding(this, e);
                return e.Binding;
            }
            return binding;
        }

        /// <summary>
        /// Occurs when a OdcTreeNodeBinding must be selected.
        /// </summary>
        public event EventHandler<OdcTreeNodeBindingEventArgs> NodeBinding;

        private bool IsNodeBindingEventAttached()
        {
            return NodeBinding != null;
        }


        /// <summary>
        /// A helper class to save the IsChecked state for data bound nodes in ViewState.
        /// </summary>
        [Serializable]
        internal class checkedNodeBag
        {
            public string dataPath;
            public bool IsChecked;
        }

        /// <summary>
        /// Uses to save the IsChecked state of data bound nodes in ViewState.
        /// </summary>
        internal checkedNodeBag[] GetCheckedNodes()
        {
            if (IsDataBound)
            {
                var r = from n in this.GetAllNodes()
                        where n.ShowCheckBox.HasValue && n.ShowCheckBox.Value == true
                        select
                            new checkedNodeBag { dataPath = n.DataPath, IsChecked = n.IsChecked };
                return r.ToArray();
            }
            return null;
        }

        internal string[] GetExpandedNodes()
        {
            var nodes = from n in this.GetAllNodes()
                        where n.IsExpanded == true
                        select n.DataPath;

            return nodes.ToArray();
        }

        /// <summary>
        /// Used to apply the IsChecked state of data bound nodes with values that come from ViewState.
        /// </summary>
        Dictionary<string, bool> checkedNodes;

        /// <summary>
        /// Used to apply the IsChecked state of data bound nodes with values that come from ViewState.
        /// </summary>
        internal void SetCheckedNodes(checkedNodeBag[] checkedNodes)
        {
            if (checkedNodes != null)
            {
                this.checkedNodes = new Dictionary<string, bool>();
                foreach (checkedNodeBag s in checkedNodes) this.checkedNodes.Add(s.dataPath, s.IsChecked);
            }
        }

        /// <summary>
        /// Set to true to get a NodeClick event even when the node is already selected.
        /// </summary>
        [Description("Set to true to get a NodeClick event even when the node is already selected.")]
        public bool CaptureAllClicks { get; set; }


        /// <summary>
        /// Gets or sets the client side event function that is raised before the context menu pops up.
        /// </summary>
        ///
        [Category("ClientScript")]
        public string ClientContextMenuOpening { get; set; }

        [Category("ClientScript")]
        public string ClientNodeSelectionChanged { get; set; }

        [Category("ClientScript")]
        public string ClientNodeExpanded { get; set; }

        [Category("ClientScript")]
        public string ClientNodeCollapsed { get; set; }

        [Category("ClientScript")]
        public string ClientNodeCheckedChanged { get; set; }

        [Category("ClientScript")]
        public string ClientNodeTextChanged { get; set; }

        [Category("ClientScript")]
        public string ClientEditModeChanged { get; set; }
    }
}