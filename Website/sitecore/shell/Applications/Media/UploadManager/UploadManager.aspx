<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UploadManager.aspx.cs" Inherits="Sitecore.Shell.Applications.Media.UploadManager.UploadManagerPage" %>
<%@ Register Assembly="Sitecore.Kernel" Namespace="Sitecore.Web.UI.HtmlControls" TagPrefix="sc" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<meta http-equiv="X-UA-Compatible" content="IE=5"/>
<head runat="server">
  <title>Sitecore</title>
  <sc:Head runat="server" />
</head>
  <frameset rows="50%,*,0">
    <frame id="Upload" runat="server" name="Upload" scrolling="no" marginwidth="0" marginheight="0"/>
    <frame id="Result" runat="server" name="Result" marginwidth="0" marginheight="0" />
    <frame id="SitecoreUpload" marginheight="0" marginwidth="0" name="SitecoreUpload" frameborder="0" noresize="noresize" />
  </frameset>
</html>
