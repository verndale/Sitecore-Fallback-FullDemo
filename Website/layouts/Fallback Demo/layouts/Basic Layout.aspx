<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Basic Layout.aspx.cs" Inherits="FallbackDemo.layouts.Fallback_Demo.layouts.Basic_Layout" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Fallback Demo</title>
</head>
<body>
    <form id="mainform" runat="server">
        <asp:Image runat="server" id="imgLogo"/>
        <br /><br />
        <asp:HyperLink runat="server" ID="hlHome" /> | <asp:HyperLink runat="server" ID="hlSearch" /> | <asp:DropDownList runat="server" ID="ddlLanguage" AutoPostBack="true" OnSelectedIndexChanged="ddlLanguage_SelectedIndexChanged"/>
        
        <div>
            <sc:Placeholder runat="server" ID="phCenter" Key="Center"></sc:Placeholder>
        </div>
        <br /><br />
        <%=Sitecore.Globalization.Translate.Text("Copyright").Replace("{0}", System.DateTime.Now.Year.ToString())%>
         | Current Date: <%= System.DateTime.Now.Date.ToShortDateString()%>
         | Sample Currency: <%= String.Format("{0:c}", 10) %>
    </form>
</body>
</html>
