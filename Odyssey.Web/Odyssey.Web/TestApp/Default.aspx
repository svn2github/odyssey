<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TestApp._Default" %>

<%@ Register Assembly="Odyssey.Web" Namespace="Odyssey.Web" TagPrefix="odc" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Styles.css" rel="Stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"/>
    <div>
        <h1>Example 1</h1>
        <h2>OdcTreeView with static OdcTreeNodes with editable text and Templates</h2>
        <h3>This example also shows how to handle commands raised within a template or context menu.</h3>
        <odc:OdcTreeView ID="OdcTreeView1" runat="server"  EnableViewState="true" 
         ClientNodeTextChanged="nodeTextChanged"
           ClientNodeExpanded = "expanded"
            ClientContextMenuOpening="myContextMenu" 
            ClientNodeCollapsed="collapsed"
             ClientNodeSelectionChanged="nodeSelected" 
            Font-Size="9" Font-Names="Arial"   EnableDragDrop="true" 
            AutoPostBack="true" DisableTextSelection="true"
            EnableClientExpand="true" AllowNodeEditing="true"
            onnodeclick="OdcTreeView1_NodeClick" oncollapsed="OdcTreeView1_Collapsed"  
             OnNodeExpanded="OdcTreeView1_Expanded"  OnNodeCheck="OdcTreeView1_NodeCheck" 
            OnNodeCommand="OdcTreeView1_Command"  OnEditNode="OdcTreeView1_DblClick" 
            OnNodePopulate="OdcTreeView1_NodePopulate" ExpandDepth="7">
<%--            <NodeTemplate>
                <asp:TextBox ID="TextBox1" runat="server" Text="<%# Container.Node.Text %>" />
        </NodeTemplate>--%>
         <NodeTemplate>
            <asp:Image runat="server" ImageUrl="~/ColorHS.png" />
            <asp:Label  isText="true" runat="server" Text="<%# Container.Node.Text %>" />
            <asp:LinkButton ID="btn" runat="server" Text="Link" OnCommand="MyCommand"  CommandName="link" CommandArgument="<%# Container.Node.Key %>" UseSubmitBehavior="false" />
        </NodeTemplate>
        <ContextMenuTemplate>
        <div style="padding:px;margin:5px;">
        <asp:LinkButton runat="server" Text="Rename" CommandName="rename" /><br />
        <asp:LinkButton runat="server" Text="Insert" CommandName="insert" /><br />
        <asp:LinkButton  runat="server" Text="Delete" CommandName="delete" />
        </div>
        </ContextMenuTemplate>
            <Nodes>
                <odc:OdcTreeNode Text="1">
                    <odc:OdcTreeNode Text="1.1" ImageUrl="~/ColorHS.png" />
                    <odc:OdcTreeNode Text="1.2" />
                </odc:OdcTreeNode>
                <odc:OdcTreeNode Text="2" />
                <odc:OdcTreeNode Text="3">
                    <odc:OdcTreeNode Text="3.1" />
                    <odc:OdcTreeNode Text="3.2"  />
                    <odc:OdcTreeNode Text="3.3">
                        <odc:OdcTreeNode Text="3.3.1"  CssClass = "myClass"/>
                        <odc:OdcTreeNode Text="3.3.2" />
                        <odc:OdcTreeNode Text="3.3.3" />
                        <odc:OdcTreeNode Text="3.3.4" ImageUrl="~/ColorHS.png"  />
                        <odc:OdcTreeNode Text="3.3.5" ShowCheckBox="true" />
                        <odc:OdcTreeNode Text="3.3.6" ShowCheckBox="true" IsChecked="true" />
                    </odc:OdcTreeNode>
                <%--    <odc:OdcTreeNode Text="3.4" />--%>
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
    <asp:Label runat="server" Text="Command:" />
    <asp:TextBox  ID="TextBox1" runat="server"  ></asp:TextBox>
        <asp:Button ID="Button1"   runat="server" Text="PostBack" CommandName="Help" OnClick="BtnClick" OnCommand="MyCommand" /><br />
        <a id="custom">-</a>
    </form>
    <script type="text/javascript">
        function expanded(node, e) {
            if (node.getText()>="4")   alert("Expanded: "+node.getText());
        }

        function collapsed(node, e) {
            if (node.getText() >= "4") alert("Collapsed: " + node.getText());
        }

        function nodeTextChanged(node, e) {
            var txt = node.getText();
            var e = document.getElementsByName("custom");
            e[0].innerHTML = "changed: "+txt;
            
        }

        function nodeSelected(node, e) {
            var value = node.get_selected();
            if (value) {
                //node.setText("OK");
                var txt = node.getText();
                var e = document.getElementsByName("custom");
                e[0].innerHTML = "selected: " + txt;
            }

        }
        function myContextMenu(tree, e) {
            var cm = e.menuElement;
            var node = e.node;
            var text = node.getText();
            if (node.isFirst()) {
                e.set_cancel(true);
                alert("no menu for the first item!");
            }
            if (text == "3.3.1") cm.style.backgroundColor = "Red"; else cm.style.backgroundColor = "";
        }

    </script>
    </body>
</html>
