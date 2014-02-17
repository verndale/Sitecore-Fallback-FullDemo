using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Data.Managers;
using Verndale.SharedSource.Helpers;

namespace FallbackDemo.layouts.Fallback_Demo.layouts
{
    public partial class Basic_Layout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                var homeItem = SitecoreHelper.ItemMethods.GetItemFromGUID("{EE3EC8F6-72C0-403C-91D4-E8DD9ACAD8A3}");
                if (homeItem != null)
                {
                    SitecoreHelper.ItemRenderMethods.RenderItemHyperLink(ref hlHome, homeItem, "Headline", false, false,
                        "");
                    hlHome.Text = homeItem.Name;
                }

                var searchItem = SitecoreHelper.ItemMethods.GetItemFromGUID("{89A70218-67AF-46EA-95EE-B9E1640BC072}");
                if (searchItem != null)
                {
                    SitecoreHelper.ItemRenderMethods.RenderItemHyperLink(ref hlSearch, searchItem, "Headline", false,
                        false, "");
                    hlSearch.Text = searchItem.Name;
                }

                var languages = Sitecore.Context.Database.GetLanguages().OrderBy(x => x.Name);
                ddlLanguage.DataTextField = "Name";
                ddlLanguage.DataValueField = "Name";
                ddlLanguage.DataSource = languages;
                ddlLanguage.DataBind();

                ddlLanguage.SelectedValue = Sitecore.Context.Language.ToString();

                var logoItem = SitecoreHelper.ItemMethods.GetItemFromGUID("{BB0078B4-0A3E-430E-8FC6-548300D7FA46}");
                if (logoItem != null)
                {
                    var logoUrl = "";
                    SitecoreHelper.ItemRenderMethods.GetMediaURL(logoItem, out logoUrl);
                    imgLogo.ImageUrl = logoUrl;
                }
            }
        }

        public void ddlLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            var languageName = ddlLanguage.SelectedValue;
            Language language = LanguageManager.GetLanguage(languageName);
            if (language != null)
            {
                UrlOptions urlOptions = UrlOptions.DefaultOptions;
                urlOptions.Language = language;
                var homeItem = SitecoreHelper.ItemMethods.GetItemFromGUID("{EE3EC8F6-72C0-403C-91D4-E8DD9ACAD8A3}");
                if (homeItem != null)
                {
                    var url = LinkManager.GetItemUrl(homeItem, urlOptions);
                    Response.Redirect(url);
                }
            }
        }
    }
}