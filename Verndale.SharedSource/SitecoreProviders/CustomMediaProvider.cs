using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Links;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Web;
using Verndale.SharedSource.Helpers;

namespace Verndale.SharedSource.SitecoreProviders
{
    /// <summary>
    /// The CustomMediaProvider class implements MediaProvider
    /// It contains an override for the GetMediaUrl method
    /// The majority of the logic is exactly the same as the method in MediaProvider except it will also:
    /// 1. check UrlOptions to see if language should be embedded in the url
    /// 2. encode the url with settings from encodeNameReplacements
    /// </summary>
    public class CustomMediaProvider : MediaProvider
    {
        public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
        {
            Assert.ArgumentNotNull((object)item, "item");
            Assert.ArgumentNotNull((object)options, "options");

            string result = base.GetMediaUrl(item, options);

            // Added by Verndale, check if language should be embedded
            UrlOptions urlOptions = UrlOptions.DefaultOptions;
            urlOptions = LanguageHelper.CheckOverrideLanguageEmbedding(urlOptions);
            if (urlOptions.LanguageEmbedding == LanguageEmbedding.Always)
            {
                result = "/" + Sitecore.Context.Language.Name.ToLowerInvariant() + result;
            }

            return result;
        }
    }
}
