<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Language Fallback Report.aspx.cs"
    Inherits="sitecore_modules.Web.LanguageTools.Language_Fallback_Report" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Language Fallback Tool</title>
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
                <!-- page-title -->
                <div class="page-title-wrapper">
                    <div class="page-title">
                        <h1 class="dotted-line-left">
                            Language Fallback Reporting Tool</h1>
                    </div>
                </div>
                <!-- /page-title -->
                <div class="content-wrapper">
                    <!-- cols-1 -->
                    <div class="cols-wrap cols-1">
                        <!-- col-middle -->
                        <asp:Label runat="server" ID="lblMessage" ForeColor="Red" Visible="False"></asp:Label>
                        <div class="col">
                            <div class="col-pad">
                                <div class="language-form language-form-validation">
                                    <div>
                                        <label for=""></label>
                                        <p class="description">Fields To Check for Fallback: <asp:Literal runat="server" ID="ltlFieldsToCheckForReporting" /></p>
                                        <br/>NOTE: When deciding if an item is falling back, the report checks these fields.  
                                        If ANY of them have content explicitly set into a language, it is considered NOT falling back 
                                        even though technically-speaking some or many other field might in fact be falling back.  
                                        The point of this is to get an idea of what items have not yet be overridden with a translation.
                                    </div>
                                    <div>
                                        <label for="ddlDatabase">Select Sitecore Database:</label>
                                        <asp:DropDownList runat="server" ID="ddlDatabase" CssClass="required" />
                                    </div>
                                    <div>
                                        <label for="txtDirPath">Sitecore Item/Directory Path:</label>
                                        <span class="dpPath">
                                            <input name="txtDirPath_0" id="txtDirPath_0" disabled="disabled" value="/sitecore" type="text" />
                                        </span>
                                        <span class="dpPathText">
                                            <asp:TextBox runat="server" ID="txtItemPath" />
                                        </span>
                                    </div>
                                    <div>
                                        <label for="ddlFilteredLanguage">Select Language:</label>
                                        <asp:DropDownList runat="server" ID="ddlFilteredLanguage" CssClass="required" />
                                        <asp:DropDownList runat="server" ID="ddlFilterType">
                                            <asp:ListItem Value="WithVersion">Items With Version In This Language</asp:ListItem>
                                            <asp:ListItem Value="WithoutVersion">Items WITHOUT Version In This Language</asp:ListItem>
                                            <asp:ListItem Value="ExplicitContent">Items With Content In This Language (explicit)</asp:ListItem>
                                            <asp:ListItem Value="FallbackContent">Items WITHOUT Content In This Language (fallback only)</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label for="ddlFilteredLanguage">Hide Filtered Content:</label>
                                        <asp:CheckBox runat="server" ID="chkHideFilteredContent"/>
                                    </div>
                                    <div>
                                        <label for=""></label>
                                        <p class="description">(Otherwise, will include the content, but gray it out.  Will still include unqualified parent items if their children qualify to show.)</p>
                                    </div>
                                    <div>
                                        <label>&nbsp;</label>
                                        <asp:Button runat="server" ID="btnRun" Text="Run Report" OnClick="btnRun_Click" CssClass="contact-btn" />
                                        <span class="ajax-loader" id="ajaxloader" runat="server">
                                            <img src="images/ajax-loader.gif" alt="Loader" />
                                        </span>
                                    </div>
                                </div>
                                <div class="clear"></div>
                                <div class="language-search-results-wrap" id="langWrapper" runat="server">
                                    <div class="language-search-results-pad">
                                        <h3>Fallback Report Results</h3>
                                        <%-- Print Button --%>
                                        <div class="icon-print-page" onclick="window.print();">
                                            &nbsp;Print</div>
                                        <div class="icon-expand-all">
                                            &nbsp;Expand All</div>
                                        <%-- Print Button --%>
                                        <div class="blueLegend">
                                        </div>
                                        <div style="float: left">
                                            : Is falling back to language in dropdown<br />
                                        </div>
                                        <div class="greenLegend">
                                        </div>
                                        <div style="float: left">
                                            : Is falling back, but not to language in dropdown [multilevel fallback]<br />
                                        </div>
                                        <div class="redLegend">
                                        </div>
                                        <div style="float: left">
                                            : Not falling back [has its own content]<br />
                                        </div>
                                        <div class="clear"></div>
                                        
                                        <asp:Literal runat="server" ID="ltlReportResult" />
                                        
                                    </div>
                                </div>
                            </div>
                            <!-- /col-middle -->
                            <div class="clear">
                            </div>
                        </div>
                        <!-- /cols-1 -->
                    </div>
                </div>
                <!-- /content-wrapper -->
            </div>
        </div>
        <!-- /site-body -->
        <%-- site-foot --%>
        <div class="site-foot" style="min-height: 200px">
            <div class="wrapper">
                <div class="site-foot-pad">
                    <div class="footer-logo-head">
                        <div class="footer-logo">
                            
                        </div>
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
        <%-- /site-foot --%>
    </div>
    </form>
    <script type="text/javascript"> 
        //Language Fallback Code
        jQuery(".fsr-child-root > ul").hide();
        jQuery(".fsr-child-root > div.language-list").hide();
        jQuery(".toggle-btn").attr("title", "Expand");
        jQuery(".toggle-language-btn").attr("title", "Expand Languages");
        jQuery("#<%=ajaxloader.ClientID %>").hide();

        jQuery(".contact-btn").click(function() {
            jQuery("#<%=ajaxloader.ClientID %>").show();
        });

        jQuery(".toggle-btn").click(function () {

            if (!jQuery(this).hasClass("expanded")) {
                jQuery(jQuery(this).parent().parent().find("ul")[0]).show();
                jQuery(this).addClass("expanded").removeClass("collapsed").attr("title", "Collapse");
            } else {
                jQuery(jQuery(this).parent().parent().find("ul")[0]).hide();
                jQuery(this).removeClass("expanded").addClass("collapsed").attr("title", "Expand");
            }
        });

        jQuery(".toggle-language-btn").click(function () {

            if (!jQuery(this).hasClass("expanded-language")) {
                jQuery(jQuery(this).parent().find("div.language-list")[0]).show();
                jQuery(this).addClass("expanded-language").removeClass("collapsed").attr("title", "Collapse Languages");
            } else {
                jQuery(jQuery(this).parent().find("div.language-list")[0]).hide();
                jQuery(this).removeClass("expanded-language").addClass("collapsed").attr("title", "Expand Languages");
            }
        });

        jQuery(".icon-expand-all").click(function () {
            if (!jQuery(this).hasClass("expandedAll")) {
                jQuery(".fallback-report > li").find("ul").show();
                jQuery(".fallback-report > li").find("div.language-list").show();
                jQuery(this).addClass("expandedAll").removeClass("collapsed").attr("title", "Collapse").html("&nbsp;Collapse All");
                jQuery(".fallback-report > li").find(".toggle-btn").addClass("expanded").removeClass("collapsed").attr("title", "Collapse");
                jQuery(".fallback-report > li").find(".toggle-language-btn").addClass("expanded-language").removeClass("collapsed").attr("title", "Collapse Languages");
            } else {
                jQuery(".fallback-report > li").find("ul").hide();
                jQuery(".fallback-report > li").find("div.language-list").hide();
                jQuery(this).removeClass("expandedAll").addClass("collapsed").attr("title", "Expand").html("&nbsp;Expand All");
                jQuery(".fallback-report > li").find(".toggle-btn").removeClass("expanded").addClass("collapsed").attr("title", "Expand");
                jQuery(".fallback-report > li").find(".toggle-language-btn").removeClass("expanded-language").addClass("collapsed").attr("title", "Expand Languages");
            }
        });
    </script>
</body>
</html>

