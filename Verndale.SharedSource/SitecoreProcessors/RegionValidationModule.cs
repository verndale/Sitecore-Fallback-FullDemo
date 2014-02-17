using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Verndale.SharedSource.Helpers;

namespace Verndale.SharedSource.SitecoreProcessors
{
    public class RegionValidationModule : IHttpModule
    {
        // registered in both the system.webServer and httpModules parts of the web.config, initializes on application load
        // note, this must be handled in this manner because it checks a value in session and therefore the session must be instanciated
        // so it must run AFTER System.Web.SessionState.SessionStateModule
        // unlike the ItemLanguageValidation which can be run in the httpRequestBegin pipeline because it isn't using session 
        public void Init(HttpApplication context)
        {
            context.PostAcquireRequestState += RequestHandler;
        }

        public void Dispose()
        {
            //
        }

        public void RequestHandler(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;

            if (app.Context.Handler is IRequiresSessionState)
            {
                try
                {
                    // VERY SIMILAR LOGIC LIVES IN ItemLanguageValidation.cs, IN THAT CASE CHECKS IF ITEM EXISTS IN CURRENT LANGUAGE
                    // Redirects to page not found if item not valid for current country
                    // and the current item is not the page not found item
                    if (Sitecore.Context.Item != null && !LanguageHelper.IsValidForCountry(Sitecore.Context.Item) 
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
                    Log.Error("Verndale.SharedSource.SitecoreProcessors.RegionValidationModule. Details: " + ex.Message, this);
                }
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
