using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Configuration;
using Verndale.SharedSource.Helpers;

namespace Verndale.SharedSource.SitecoreProcessors
{
    // this is called in Sitecore.SharedSource.PartialLanguageFallback.config, on the item:created event
    public class CreateVersionInAllLanguagesOnCreate
    {
        #region properties
        private static String _pathsToCheckForLanguageVersions;
        private static String PathsToCheckForLanguageVersions
        {
            get
            {
                if (_pathsToCheckForLanguageVersions == null)
                    _pathsToCheckForLanguageVersions = Settings.GetSetting("Fallback.PathsToCheckForLanguageVersions");

                return _pathsToCheckForLanguageVersions;
            }
        }
        #endregion

        protected void OnItemCreated(object sender, EventArgs args)
        {
            if (args == null)
                return;

            if (String.IsNullOrEmpty(PathsToCheckForLanguageVersions))
                return;

            var createdArgs = Event.ExtractParameter(args, 0) as ItemCreatedEventArgs;
            if (createdArgs == null)
                return;

            var item = createdArgs.Item;

            var pathsList = PathsToCheckForLanguageVersions.Split('|');

            foreach (var path in pathsList)
            {
                if (item.Paths.FullPath.ToLower().Contains(path.ToLower()))
                {
                    LanguageHelper.CreateVersionInEachLanguage(item);
                    break;
                }
            }
        }
    }
}
