using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SharedSource.PartialLanguageFallback.Extensions;

namespace Sitecore.SharedSource.PartialLanguageFallback.Managers
{
   public static class FallbackLanguageManager
   {
      public static Language FallbackLanguage { get { return Context.Language.GetFallbackLanguage(Context.Database); } }

      public static string ReadFallbackValue(Field field, Item item)
      {
         var fallbackItem = item.GetFallbackItem();
         return fallbackItem != null && fallbackItem.Versions.Count > 0 ? fallbackItem[field.ID] : null;
      }
   }
}
