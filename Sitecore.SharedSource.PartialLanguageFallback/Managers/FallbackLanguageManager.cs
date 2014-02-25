using System;
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
            
            //<!--CHANGED BY VERNDALE-->
            // removed the check for Versions.Count here, which had been preventing it from checking for a value in another language 
            // necessary in the scenario where this language also falls back (in the scenario of chained fallback, eg: es-US -> en-US -> en) 
            // and this particular language version had not yet been added to the item
            // note that merely calling fallbackItem[field.ID] will check this method (ReadFallbackValue) recursively 
            // because it will trigger the GetStandardValue method in the FallbackLanguageProvider
            
            //return fallbackItem != null && fallbackItem.Versions.Count > 0 ? fallbackItem[field.ID] : null;
            return fallbackItem != null ? fallbackItem[field.ID] : null;
        }
    }
}
