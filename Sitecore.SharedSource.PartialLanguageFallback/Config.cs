using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Globalization;

namespace Sitecore.SharedSource.PartialLanguageFallback
{
   public class Config
   {
      public static IEnumerable<ID> SupportedTemplateIDs
      {
         get
         {
            var templateIds = MainUtil.RemoveEmptyStrings(EnforceVersionPresenceTemplates.ToLower().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            return from templateId in templateIds where ID.IsID(templateId) select ID.Parse(templateId);
         }
      }

      public static bool VaryFallbackSettingsPerLanguage
      {
         get
         {
            return Settings.GetBoolSetting("Fallback.VaryFallbackSettingsPerLanguage", false);
         }
      }

      public static string EnforceVersionPresenceTemplates
      {
         get
         {
            return Settings.GetSetting("Fallback.EnforceVersionPresenceTemplates");
         }
      }

      public static bool ProcessSystemFields
      {
         get
         {
            return Settings.GetBoolSetting("Fallback.ProcessSystemFields", false);
         }
      }

      public static string IgnoredFields
      {
         get
         {
            return Settings.GetSetting("Fallback.IgnoredFields");
         }
      }

      public static long FallbackCacheSize
      {
         get
         {
            return GetSize("Fallback.CacheSize", "10MB");
         }
      }

      private static string MasterLanguageSetting
      {
         get
         {
            return Settings.GetSetting("Fallback.MasterLanguage");
         }
      }

      public static Language MasterLanguage
      {
         get
         {
            Language parsedLang;

            if (Language.TryParse(MasterLanguageSetting, out parsedLang))
            {
               return parsedLang;
            }

            throw new Exception("Master Language is either not defined or not in a proper format");
         }
      }

      private static long GetSize(string setting, string defaultSize)
      {
         return StringUtil.ParseSizeString(Settings.GetSetting(setting, defaultSize));
      }

      public static bool VersionPresenceEnforced
      {
         get
         {
            return Context.Site != null &&
                   Context.Site.SiteInfo.Properties["enforceVersionPresence"] != null &&
                   Context.Site.SiteInfo.Properties["enforceVersionPresence"].Equals("true", StringComparison.InvariantCultureIgnoreCase);
         }
      }

      public static bool SiteFallbackEnabled
      {
         get
         {
            return Context.Site != null &&
               //!Context.PageMode.IsPageEditor &&
                   Context.Site.SiteInfo.Properties["enableFallback"] != null &&
                   Context.Site.SiteInfo.Properties["enableFallback"].Equals("true", StringComparison.InvariantCultureIgnoreCase);
         }
      }
   }
}
