using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.SharedSource.PartialLanguageFallback.Extensions;

namespace Verndale.SharedSource.Helpers
{
    public class LanguageHelper
    {
        public static bool DisableFallbackCheck
        {
            get
            {
                // if the PartialLanguageFallback config file is not turned on or configured, 
                // then the system should consider fallback enabled, so it doesn't try to 404 redirect
                if (Sitecore.Context.Site == null || Sitecore.Context.Site.SiteInfo.Properties["enableFallback"] == null)
                    return true;

                return false;
            }
        }

        public static bool EnforceVersionPresence
        {
            get
            {
                return Sitecore.Context.Site != null &&
                       Sitecore.Context.Site.SiteInfo.Properties["enforceVersionPresence"] != null &&
                       Sitecore.Context.Site.SiteInfo.Properties["enforceVersionPresence"].Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public static bool EnableFallback
        {
            get
            {
                return Sitecore.Context.Site != null &&
                       Sitecore.Context.Site.SiteInfo.Properties["enableFallback"] != null &&
                       Sitecore.Context.Site.SiteInfo.Properties["enableFallback"].Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public static IEnumerable<ID> SupportedTemplateIDs
        {
            get
            {
                var templateIds = MainUtil.RemoveEmptyStrings(EnforceVersionPresenceTemplates.ToLower().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                return from templateId in templateIds where ID.IsID(templateId) select ID.Parse(templateId);
            }
        }

        public static string EnforceVersionPresenceTemplates
        {
            get
            {
                return Settings.GetSetting("Fallback.EnforceVersionPresenceTemplates");
            }
        }

        public static bool IsOfSupportedTemplate(Item item)
        {
            var template = TemplateManager.GetTemplate(item.TemplateID, item.Database);
            return SupportedTemplateIDs.Any(template.DescendsFromOrEquals);
        }

        protected bool EmptyVersion(Item item)
        {
            return item == null || item.Versions.Count == 0;
        }

        public static string PageNotFoundGUID
        {
            get
            {
                if (Sitecore.Context.Site == null ||
                    Sitecore.Context.Site.SiteInfo.Properties["pageNotFoundGUID"] == null)
                    return "";
                return Sitecore.Context.Site.SiteInfo.Properties["pageNotFoundGUID"];
            }
        }

        public static bool HasContextLanguage(Item item)
        {
            // get the latest version and count, count should be 0 if it doesn't exist in the current language
            Item latestVersion = item.Versions.GetLatestVersion();
            var hasVersionInCurrentLanguage = (latestVersion != null) && (latestVersion.Versions.Count > 0);

            // if fallback config is not enabled, return true
            // if it has a version in the current language, then return true
            // if it DOESNT have a version in the current language and a version MUST exist (in the least, a blank version in the language) 
            // because this item's template (or base templates) is in the list of supported templates, return false
            // if it DOESNT have a version in the current language and fallback is NOT enabled, return false
            if (DisableFallbackCheck)
                return true;
            if (hasVersionInCurrentLanguage)
                return true;
            else if (!hasVersionInCurrentLanguage && EnforceVersionPresence && IsOfSupportedTemplate(item))
                return false;
            else if (!EnableFallback)
                return false;

            // Note, the following only applies if enforceVersionPresence is false, 
            // because if it is true, then straight up if the language version doesn't exist, it should be treated like it doesn't exist

            // But even if we aren't enforcing version presence, we don't want it to return a blank item, 
            // and by default sitecore would do so if the language version doesn't exist AND the language doesn't fall back.

            // so if we have gotten this far, we know it doesn't have a version in the current language
            // we have already checked if EnableFallback isn't turned on, and we are returning false in that case, 
            // because if it can't fallback, then without a language version it will definitely return blank.

            // but there is the case that if this item is falling back, then eventually it might get to a language version that does exist
            // so we run the following code, which only is necessary if fallback is enabled AND if the current language has been configured to fallback
            // we can then check that fallback language version, and so on, recursively

            // assumes at this point that version presence is NOT enforced AND it doesn't have a version in the current language
            // it should be the case that fallback is enabled, in order to get here, but let's check just in case
            if (EnableFallback)
            {
                try
                {
                    // check if the current language falls back and get the version of the item it falls back to
                    bool currentLanguageFallsBack = Sitecore.Context.Language.HasFallbackAssigned(Sitecore.Context.Database);
                    var fallbackItem = Sitecore.SharedSource.PartialLanguageFallback.Extensions.ItemExtensions.GetFallbackItem(item);

                    // if the current language doesn't fall back at all and we already know if we have gotten this far, 
                    // there isn't a version in the current language, so return false
                    if (!currentLanguageFallsBack)
                        return false;
                    else
                    {
                        // if there is no latest version for this item in the current language (which we know is the case otherwise we wouldn't have gotten this far), '
                        // check to see if it falls back to another language version and if so then return the latest version check on that
                        // don't want to say there is no version for this context if the language it falls back to does have a version, because it will load that one.
                        // recursively will do this in case one language falls back to another which falls back to another

                        if (fallbackItem != null)
                            return HasContextLanguage(fallbackItem);
                    }

                }
                catch
                {

                }
            }

            return true;
        }

        public static List<Item> RemoveItemsWithoutLanguageVersion(List<Item> items)
        {
            var filteredItems = new List<Item>();
            foreach (Item item in items)
            {
                if (HasContextLanguage(item) && IsValidForCountry(item))
                    filteredItems.Add(item);
            }

            return filteredItems;
        }

        public static bool IsValidForCountry(Item item)
        {
            // default is to return true, 
            // if session is null or country has not yet be set into session or the item is null 
            // or doesn't have any countries set in the Limit to Countries field, then should be valid
            // only is not valid if country code set in session is not among countries explicitly selected for the current item.
            if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["Country Code"] != null && item != null)
            {
                var countries = SitecoreHelper.ItemRenderMethods.GetMultilistValueByFieldName("Limit To Countries", item);
                // only try to match if the current item has a value set in a Limit To Countries field
                if (countries != null && countries.Count > 0)
                {
                    var matchingCountry =
                        countries.Where(
                            x => x.Fields["Code"].ToString() == HttpContext.Current.Session["Country Code"].ToString());

                    return matchingCountry.Any();
                }
            }

            return true;
        }

        public static void CreateVersionInEachLanguage(Item item)
        {
            IEnumerable<Language> languages = LanguageManager.GetLanguages(item.Database).Where(a => a != item.Language);

            foreach (Language language in languages)
            {
                Item localizedItem = item.Database.GetItem(item.ID, language);

                //if Versions.Count == 0 then no entries exist in the given language
                if (localizedItem.Versions.Count == 0)
                {
                    localizedItem.Editing.BeginEdit();
                    localizedItem.Versions.AddVersion();
                    localizedItem.Editing.EndEdit();
                }
            }
        }

        public static UrlOptions CheckOverrideLanguageEmbedding(UrlOptions urlOptions)
        {
            var thisSite = Sitecore.Context.Site;

            if (urlOptions.Site != null)
                thisSite = urlOptions.Site;

            if (!String.IsNullOrEmpty(thisSite.SiteInfo.Properties["languageEmbedding"]))
            {
                if (thisSite.SiteInfo.Properties["languageEmbedding"].ToLower() == "never")
                    urlOptions.LanguageEmbedding = LanguageEmbedding.Never;
                else if (thisSite.SiteInfo.Properties["languageEmbedding"].ToLower() == "always")
                    urlOptions.LanguageEmbedding = LanguageEmbedding.Always;
                else if (thisSite.SiteInfo.Properties["languageEmbedding"].ToLower() == "asneeded")
                    urlOptions.LanguageEmbedding = LanguageEmbedding.AsNeeded;
            }

            return urlOptions;
        }
    }
}
