<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NodeBindingEvent.aspx.cs"
    Inherits="TestApp.NodeBindingEvent" %>

<%@ Register Assembly="Odyssey.Web" Namespace="Odyssey.Web" TagPrefix="odc" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form2" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <div>
        <h1>Example 4</h1>
        <h2>OdcTreeView and TreeNodeBinding event</h2>
        <h3>demonstates how to use NodeBinding event to apply different templates to a node under custom conditions.</h3>
        <odc:OdcTreeView ID="OdcTreeView1" runat="server" EnableViewState="true" Font-Size="9"
            Font-Names="Arial" EnableDragDrop="true" AutoPostBack="false" DisableTextSelection="true"
            EnableClientExpand="true" AllowNodeEditing="false" ExpandDepth="7" 
            onnodebinding="OdcTreeView1_NodeBinding" 
            onnodepopulate="OdcTreeView1_NodePopulate">
            <TreeNodeBindings>
                <odc:OdcTreeNodeBinding Name="Root" ShowCheckBox="true">
                    <NodeTemplate>
                        <asp:Label ID="Label2" isText="true" runat="server" Text="<%# Container.Node.Text %>" />
                    </NodeTemplate>
                </odc:OdcTreeNodeBinding>
            </TreeNodeBindings>
            <NodeTemplate>
                <asp:Image runat="server" ImageUrl="~/ColorHS.png" />
                <asp:Label isText="true" runat="server" Text="<%# Container.Node.Text %>" />
                <asp:LinkButton ID="btn" runat="server" Text="Link"  CommandName="link"
                    CommandArgument="<%# Container.Node.Key %>" UseSubmitBehavior="false" />
            </NodeTemplate>
            <Nodes>
                <odc:OdcTreeNode Text="1" CssClass="myClass" >
                    <odc:OdcTreeNode Text="1.1" ImageUrl="~/ColorHS.png" />
                    <odc:OdcTreeNode Text="1.2" />
                </odc:OdcTreeNode>
                <odc:OdcTreeNode Text="2" />
                <odc:OdcTreeNode Text="3">
                    <odc:OdcTreeNode Text="3.1" />
                    <odc:OdcTreeNode Text="3.2" />
                    <odc:OdcTreeNode Text="3.3">
                        <odc:OdcTreeNode Text="3.3.1" />
                        <odc:OdcTreeNode Text="3.3.2" />
                        <odc:OdcTreeNode Text="3.3.3" />
                        <odc:OdcTreeNode Text="3.3.4" ImageUrl="~/ColorHS.png" />
                        <odc:OdcTreeNode Text="3.3.5" ShowCheckBox="true" />
                        <odc:OdcTreeNode Text="3.3.6" ShowCheckBox="true" IsChecked="true" />
                    </odc:OdcTreeNode>
                </odc:OdcTreeNode>
                <odc:OdcTreeNode Text="4">
                    <odc:OdcTreeNode Text="4.1" />
                    <odc:OdcTreeNode Text="4.2" PopulateOnDemand="true" IsExpanded="true" />
                    <odc:OdcTreeNode Text="4.3" />
                    <odc:OdcTreeNode Text="4.4" />
                    <odc:OdcTreeNode Text="4.5" />
                </odc:OdcTreeNode>
            </Nodes>
        </odc:OdcTreeView>
    </div>
    </form>
</body>
</html>
