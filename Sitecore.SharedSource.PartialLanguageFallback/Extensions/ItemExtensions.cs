using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.StringExtensions;
using Version = Sitecore.Data.Version;

namespace Sitecore.SharedSource.PartialLanguageFallback.Extensions
{
   public static class ItemExtensions
   {
      public static bool ContainsField(this Item item, string fieldToken)
      {
         Assert.ArgumentNotNull(fieldToken, "fieldToken");
         return item.Fields[fieldToken] != null;
      }

      public static bool ContainsField(this Item item, ID fieldId)
      {
         Assert.IsFalse(fieldId.IsNull, "fieldId");
         return item.Fields[fieldId] != null;
      }

      public static Language GetFallbackLanguage(this Item langItem)
      {
         Assert.IsNotNull(langItem, "langItem cannot be null");

         var fallbackLangName = langItem[Constants.FieldIds.FallbackLanguage];
         Language fallbackLang;
         return Language.TryParse(fallbackLangName, out fallbackLang) ? fallbackLang : null;
      }

      public static Item GetFallbackItem(this Item item)
      {
         Assert.IsNotNull(item, "item cannot be null");

         var fallbackLang = item.Language.GetFallbackLanguage(item.Database);

         if (fallbackLang != null &&
            !fallbackLang.Name.IsNullOrEmpty() &&
            !fallbackLang.Equals(item.Language))
         {
            return item.Database.GetItem(item.ID, fallbackLang, Version.Latest);
         }

         return null;
      }

      public static Item GetMasterItem(this Item origItem)
      {
         Assert.IsNotNull(origItem, "origItem cannot be null");
         return origItem.Database.GetItem(origItem.ID, Config.MasterLanguage, Version.Latest);
      }
   }
}
