<%@ OutputCache Location="None" VaryByParam="none" %>

<%@ Page Language="c#" CodeBehind="UploadMedia.aspx.cs" AutoEventWireup="false" Inherits="Sitecore.Shell.Applications.Media.UploadMedia.UploadMediaPage" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" >
<html>
  <meta http-equiv="X-UA-Compatible" content="IE=5" />
  <head>
    <title>Sitecore</title>
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
    <style type="text/css">
      html, body, iframe
      {
        height:100%;
        width:100%;
        margin:0;
        padding:0;
        border:0;
      }
    
      body{
        overflow:hidden;  
      }
      
      #sitecoreattach
      {
        height:0;  
      }
    </style>
  </head>
  <body>
      <iframe id="Attach" runat="server" name="attach" scrolling="no"/>
      <iframe name="sitecoreattach" id="sitecoreattach"/>
  </body>
</html>
