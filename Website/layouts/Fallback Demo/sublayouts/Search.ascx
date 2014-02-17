<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Search.ascx.cs" Inherits="FallbackDemo.layouts.Fallback_Demo.sublayouts.Search" %>

<%=Sitecore.Globalization.Translate.Text("Search_Keyword")%>: <asp:TextBox runat="server" ID="txtKeyword" />
<asp:Button runat="server" ID="btnSearch" OnClick="btnSearch_Click" />

<asp:Panel runat="server" id="pnlResults" Visible="false">
    <br /><br />
    <b><%=Sitecore.Globalization.Translate.Text("Results")%>:</b>
    <br />
    <asp:Repeater id="rptResults" runat="server" OnItemDataBound="rptResults_ItemDataBound">
        <ItemTemplate>
            <asp:HyperLink runat="server" ID="hlResult" />
        </ItemTemplate>
    </asp:Repeater>
</asp:Panel>