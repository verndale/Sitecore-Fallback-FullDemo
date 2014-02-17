using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore.Data.Items;
using Verndale.SharedSource.Helpers;
using ADCSearchHelper = scSearchContrib.Searcher.Utilities.SearchHelper;

namespace FallbackDemo.layouts.Fallback_Demo.sublayouts
{
    public partial class Search : System.Web.UI.UserControl
    {
        private string ContextLanguage = Sitecore.Context.Language.ToString();
        private string TemplateIds = ""; //pipe delimited guids
        private string LocationId = "{EE3EC8F6-72C0-403C-91D4-E8DD9ACAD8A3}"; //Fallback Demo site Home item guid
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                btnSearch.Text = Sitecore.Globalization.Translate.Text("Search");
        }

        public void btnSearch_Click(Object sender, System.EventArgs e)
        {
            pnlResults.Visible = false;
            var keyword = txtKeyword.Text;

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = SearchHelper.FormatKeywordWithSpacesForSearch(keyword);
                var skinnyItems = SearchHelper.GetItems("advanced", ContextLanguage, TemplateIds, LocationId, keyword);
                var resultItems = ADCSearchHelper.GetItemListFromInformationCollection(skinnyItems);
                pnlResults.Visible = resultItems.Count > 0;
                rptResults.DataSource = resultItems;
                rptResults.DataBind();
            }
        }

        public void rptResults_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                HyperLink hlResult = (HyperLink)e.Item.FindControl("hlResult");
                Item item = (Item) e.Item.DataItem;

                SitecoreHelper.ItemRenderMethods.RenderItemHyperLink(ref hlResult, item, "Headline", false, false, "");
            }
        }
    }
}