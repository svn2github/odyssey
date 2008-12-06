<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TemplatesAndCommands.aspx.cs" Inherits="TestApp.Test" %>

<%@ Register Assembly="Odyssey.Web" Namespace="Odyssey.Web" TagPrefix="odc" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <h1>Example 3: Templates and Commands.</h1>
    <h3>This example shows how to use a EditNodeTemplate and how to perform validation.</h3>
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"/>
        <odc:OdcTreeView ID="treeView" runat="server" AutoPostBack="False" OnNodePopulate="treeView_NodePopulate"
            OnNodeCommand="treeView_Command"  OnAfterPostBack="treeView_PostBack">
            <ContextMenuTemplate>
                <asp:LinkButton Style="margin-left: 5px" ID="btnOkay" runat="server" CommandName="ok"
                    Text="Okay" /><br />
                <asp:LinkButton ID="btnCancel" runat="server" CommandName="cancel" Text="Cancel" />
            </ContextMenuTemplate>
            <EditNodeTemplate>
                <asp:TextBox runat="server" ID="tbText" Text='<%# Bind("Text") %>' />
                <asp:LinkButton style="padding-left:4px" ID="btnRename" runat="server" CommandName="rename" Text="Rename" />
                <asp:CustomValidator runat="server" ID="myValidator" ControlToValidate="tbText" OnServerValidate="Validating"  ErrorMessage="Error" Display="Dynamic" Text="Must start with !" EnableClientScript="false"/>
                <asp:LinkButton ID="btnCancel" runat="server" CommandName="cancel" Text="Cancel"  />
            </EditNodeTemplate>
            <NodeTemplate>
                <%# Container.Node.Text %><asp:LinkButton Style="padding-left: 4px" ID="btnAdd" runat="server"
                    CommandName="add" Text="Add" />
                <asp:LinkButton ID="btnRemove" runat="server" CommandName="remove" Text="Remove" />
                <asp:LinkButton ID="btnEdit" runat="server" CommandName="edit" Text="Edit" />
            </NodeTemplate>
        </odc:OdcTreeView>
    </div>
    </form>
</body>
</html>
