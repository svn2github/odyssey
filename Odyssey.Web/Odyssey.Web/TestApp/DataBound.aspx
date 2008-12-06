<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DataBound.aspx.cs" Inherits="TestApp.DataBound"
    Trace="false" %>

<%@ Register Assembly="Odyssey.Web" Namespace="Odyssey.Web" TagPrefix="odc" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <h1>
        Example 2: Databinding</h1>
    <h2>
        This example shows how to use a HierarchicalDataSource with the OdcTreeView.</h2>
    <h3>
        It also demonstates the usage of OdcTreeNodeBindings.</h3>
    <asp:XmlDataSource ID="XmlDataSource1" runat="server" DataFile="~/Xml.xml"></asp:XmlDataSource>
    <br />
    <div >
        <odc:OdcTreeView ID="OdcTreeView1" runat="server" DataSourceID="XmlDataSource1" AllowNodeEditing="True"
            OnNodeEdited="OdcTreeView1_NodeEdited">
            <TreeNodeBindings>
                <odc:OdcTreeNodeBinding Level="0" TextField="Title">
                    <NodeTemplate>
                        <asp:Button runat="server" Text="this is the root" UseSubmitBehavior="False" />
                    </NodeTemplate>
                </odc:OdcTreeNodeBinding>
                <odc:OdcTreeNodeBinding Level="1" TextField="Title" />
                <odc:OdcTreeNodeBinding Level="2" TextField="Title" ShowCheckBox="true" IsChecked="true" />
                <odc:OdcTreeNodeBinding Level="3" TextField="Value" ShowCheckBox="true" IsChecked="false" />
            </TreeNodeBindings>
            <%--            <NodeTemplate>
                <asp:Label ID="label" runat="server" Text='<%# Eval("Title") %>' />
                <asp:Button ID="btn" runat="server" Text="Select" CommandName="select" UseSubmitBehavior="false" />
                <asp:LinkButton ID="link" runat="server" Text="Link" CommandName="cancel" />
            </NodeTemplate> --%>
        </odc:OdcTreeView>
    </div>
    </form>
</body>
</html>
