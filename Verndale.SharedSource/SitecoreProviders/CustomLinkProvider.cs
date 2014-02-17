using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Configuration;
using Sitecore.Links;
using Verndale.SharedSource.Helpers;

namespace Verndale.SharedSource.SitecoreProviders
{
    public class CustomLinkProvider : LinkProvider
    {
        public override string GetItemUrl(Item item, UrlOptions urlOptions)
        {
            var holdUrlOptions = urlOptions;
            try
            {
                urlOptions = LanguageHelper.CheckOverrideLanguageEmbedding(urlOptions);

                urlOptions.SiteResolving = Settings.Rendering.SiteResolving;
                return base.GetItemUrl(item, urlOptions);

            }
            catch
            {
                urlOptions.SiteResolving = Settings.Rendering.SiteResolving;

                return base.GetItemUrl(item, holdUrlOptions);
            }
        }
    }
}
