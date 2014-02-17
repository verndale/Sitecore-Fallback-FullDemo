<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="LanguageMigration.aspx.cs"
   Inherits="sitecore_modules.Web.LanguageTools.LanguageMigration" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Language Migration</title>
    <link href="Language.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="jquery-library.js"></script>
</head>
<body>
   <form id="mainform" runat="server" class="main_form">
   <div class="main-wrapper">
        <div class="site-head inner-header">
            <div class="wrapper">
                <div class="site-head-pad">
                    <div class="logo"></div>
                </div>
            </div>
        </div>
        <div class="site-body">
            <div class="wrapper">
                <div class="page-title-wrapper">
                    <div class="page-title">
                        <h1 class="dotted-line-left">Language Migration Tool</h1>
                    </div>
                </div>
                <div class="content-wrapper">
                    <div class="cols-wrap cols-1">
                        <asp:Label runat="server" ID="lblMessage" ForeColor="Red" Visible="False"></asp:Label>
                         <div class="col">
                            <div class="col-pad">
                                <div class="language-form language-form-validation">
                                    <div>
                                        <label for="ddlDatabase">Select Sitecore Database:</label>
                                        <asp:DropDownList ID="ddlDatabase" runat="server" CssClass="required" />
                                    </div>
                                    <div>
                                        <label for="txtItemPath">Sitecore Item/Directory Path:</label>
                                        <span class="dpPath">
                                            <input name="txtDirPath_0" id="txtDirPath_0" disabled="disabled" value="/sitecore" type="text" />
                                        </span>
                                        <span class="dpPathText">
                                            <asp:TextBox ID="txtItemPath" Width="500px" runat="server" CssClass="required" />
                                        </span>
                                    </div>
                                    <div>
                                        <label for="chkDryRun">Dry Run</label>
                                        <asp:CheckBox ID="chkDryRun" runat="server" />
                                    </div>
                                    <div>
                                        <label for=""></label>
                                        <p class="description">(will run through all of the items and output to a log what would happen to it, but won't actually execute the action)</p>
                                    </div>
                                    <div>
                                        <label for="ddlAction">Type of Action:</label>
                                        <asp:DropDownList ID="ddlAction" runat="server">
                                            <asp:ListItem value="MigrateSource">Migrate Source To Target Language</asp:ListItem>
                                            <asp:ListItem value="DeleteTarget">Remove Target Language</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label for=""></label>
                                        <p class="description">(Note: Selection 'Remove Target' will delete all the target language versions. 
                                        This will not perform any kind of migration.)</p>
                                    </div>
                                    <div>
                                        <label for="ddlSourceLanguage">Source Language:</label>
                                        <asp:DropDownList ID="ddlSourceLanguage" runat="server" />
                                    </div>
                                    <div>
                                        <label for="ddlTargetLanguage">Target Language:</label>
                                        <asp:DropDownList ID="ddlTargetLanguage" runat="server" /> 
                                         &nbsp;&nbsp;OR <!-- added '&nbsp;' to give some space before 'OR' -->
                                        <span class="all-other-languages-trigger"><!-- Added a wrapper so we can style and align this specificaly in the CSS -->
											<asp:CheckBox ID="chkAddAllOtherLanguages" runat="server" Checked="false" Text="ALL OTHER LANGUAGES" />
										</span>
                                    </div>
                                    <div>
                                        <label for="chkProcessChildren">Process children:</label>
                                        <asp:CheckBox ID="chkProcessChildren" runat="server" />
                                    </div>
                                    <div>
                                        <label for="chkProcessRecursively">Process recursively:</label>
                                        <asp:CheckBox ID="chkProcessRecursively" runat="server" />
                                    </div>
                                    <div>
                                        <label for="chkOnlyCreateIfNoVersion">Only items without language:</label>
                                        <asp:CheckBox ID="chkOnlyCreateIfNoVersion" runat="server" Checked="true" />
                                    </div>
                                    <div>
                                        <label for=""></label>
                                        <p class="description">(Only create version for items that do not yet have a version in target language)</p>
                                    </div>
                                    <div>
                                        <label for="chkOnlyVersion">Don't transfer field values:</label>
                                        <asp:CheckBox ID="chkOnlyVersion" runat="server" Checked="true" />
                                    </div>
                                    <div>
                                        <label for=""></label>
                                        <p class="description">(Only create version, don't copy field values, for use if leveraging language fallback)</p>
                                    </div>
                                    <div>
                                        <label for="chkOnlyContentFields">Transfer only content fields:</label>
                                        <asp:CheckBox ID="chkOnlyContentFields" runat="server" />
                                    </div>
                                    <div>
                                        <label for=""></label>
                                        <p class="description">(If copying field values too, only use content fields that should be translated)</p>
                                    </div>
                                    <div>
                                        <label for="chkDeleteSource">Delete source version:</label>
                                        <asp:CheckBox ID="chkDeleteSource" runat="server"  />
                                    </div>
                                    <div>
                                        <label for=""></label>
                                        <p class="description">(will delete the Source Version)</p>
                                    </div>
                                    <div>
                                        <label>&nbsp;</label>
                                        <asp:Button ID="brnProcess" OnClick="Process_Click" Text="Process" runat="server" CssClass="contact-btn" />
                                    </div>
                                </div>
                                <div class="clear"></div>
                            </div>
                            <div class="clear"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="site-foot" style="min-height: 200px">
            <div class="wrapper">
                <div class="site-foot-pad">
                    <div class="footer-logo-head">
                        <div class="footer-logo"></div>
                    </div>
                    <div class="copy-right">
                        <p>
                            &copy; Copyright
                            <%= DateTime.Now.Year %>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div> 
   </form>
   
    <script type="text/javascript">
        $(document).ready(function() {
            $("#<%= chkAddAllOtherLanguages.ClientID %>").click(function () {
                if ($('#<%= chkAddAllOtherLanguages.ClientID %>').is(':checked')) {
                    $("#<%= ddlTargetLanguage.ClientID %>").prop("disabled", true);
                    $("#<%= chkDeleteSource.ClientID %>").prop("disabled", true);
                    $("#<%= chkDeleteSource.ClientID %>").attr("checked", false);
                } else {
                    $("#<%= ddlTargetLanguage.ClientID %>").prop("disabled", false);
                    $("#<%= chkDeleteSource.ClientID %>").prop("disabled", false);
                }
            });

            $("#<%= ddlAction.ClientID %>").change(function () {
                if ($("#<%= ddlAction.ClientID %>").val() == "DeleteTarget") {
                    $("#<%= ddlSourceLanguage.ClientID %>").prop("disabled", true);
                    $("#<%= chkOnlyCreateIfNoVersion.ClientID %>").prop("disabled", true);
                    $("#<%= chkOnlyCreateIfNoVersion.ClientID %>").attr("checked", true);
                    $("#<%= chkOnlyContentFields.ClientID %>").prop("disabled", true);
                    $("#<%= chkOnlyContentFields.ClientID %>").attr("checked", false);
                    $("#<%= chkOnlyVersion.ClientID %>").prop("disabled", true);
                    $("#<%= chkOnlyVersion.ClientID %>").attr("checked", true);
                    $("#<%= chkDeleteSource.ClientID %>").prop("disabled", true);
                    $("#<%= chkDeleteSource.ClientID %>").attr("checked", false);
                    $("#<%= chkAddAllOtherLanguages.ClientID %>").prop("disabled", true);
                    $("#<%= chkAddAllOtherLanguages.ClientID %>").attr("checked", false);
                } else {
                    $("#<%= ddlSourceLanguage.ClientID %>").prop("disabled", false);
                    $("#<%= chkOnlyCreateIfNoVersion.ClientID %>").prop("disabled", false);
                    $("#<%= chkOnlyContentFields.ClientID %>").prop("disabled", false);
                    $("#<%= chkOnlyVersion.ClientID %>").prop("disabled", false);
                    $("#<%= chkDeleteSource.ClientID %>").prop("disabled", false);
                    $("#<%= chkAddAllOtherLanguages.ClientID %>").prop("disabled", false);
                }
            });

            $("#<%= chkOnlyVersion.ClientID %>").click(function () {
                if ($('#<%= chkOnlyVersion.ClientID %>').is(':checked')) {
                    $("#<%= chkOnlyContentFields.ClientID %>").prop("disabled", true);
                    $("#<%= chkOnlyContentFields.ClientID %>").attr("checked", false);
                } else {
                    $("#<%= chkOnlyContentFields.ClientID %>").prop("disabled", false);
                }
            });
        });
    </script>

</body>
</html>
