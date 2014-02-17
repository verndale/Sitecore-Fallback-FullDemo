using System;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;
using Verndale.SharedSource.Helpers;

namespace Verndale.SharedSource.SitecoreProcessors
{
    public class ItemLanguageValidation : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            try
            {
                // VERY SIMILAR LOGIC LIVES IN RegionValidationModule.cs, IN THAT CASE CHECKS IF ITEM IS VALID FOR THE COUNTRY STORED IN SESSION
                // Redirects to page not found if no item version for current language and enforceVersionPresence set on site
                // and the current item is not the page not found item
                if (Sitecore.Context.Item != null && !LanguageHelper.HasContextLanguage(Sitecore.Context.Item) 
                    && Sitecore.Context.Item.ID.ToString() != PageNotFoundGUID)
                {
                    Item pageNotFoundItem = SitecoreHelper.ItemMethods.GetItemFromGUID(PageNotFoundGUID);
                    if (pageNotFoundItem != null)
                    {
                        var url = SitecoreHelper.ItemRenderMethods.GetItemUrl(pageNotFoundItem);
                        if (!String.IsNullOrEmpty(url))
                            Sitecore.Web.WebUtil.Redirect(url);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Verndale.SharedSource.SitecoreProcessors.ItemLanguageValidation. Details: " + ex.Message, this);
            }
        }

        public static string PageNotFoundGUID
        {
            get
            {
                return Sitecore.Context.Site.SiteInfo.Properties["pageNotFoundGUID"];
            }
        }
    }
}
