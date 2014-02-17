using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.SecurityModel;

namespace Sitecore.SharedSource.PartialLanguageFallback.Extensions
{
    public static class LanguageExtensions
    {
        public static bool HasFallbackAssigned(this Language language, Database database)
        {
            return !String.IsNullOrEmpty(GetFallbackLanguage(language, database).Name);
        }

        public static Language GetFallbackLanguage(this Language language, Database database)
        {
            var sourceLangItem = GetLanguageDefinitionItem(language, database);
            return sourceLangItem != null ? sourceLangItem.GetFallbackLanguage() : null;
        }

        public static Item GetLanguageDefinitionItem(this Language language, Database database)
        {
            var sourceLanguageItemId = LanguageManager.GetLanguageItemId(language, database);
            return ID.IsNullOrEmpty(sourceLanguageItemId) ? null : ItemManager.GetItem(sourceLanguageItemId, Language.Parse("en"), Sitecore.Data.Version.Latest, database, SecurityCheck.Disable);
        }
    }
}
