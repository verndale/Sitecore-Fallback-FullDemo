using System;
using Sitecore.Caching;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.SharedSource.PartialLanguageFallback.Caching
{
   public class FallbackValuesCache : CustomCache
   {
      public FallbackValuesCache(string name, long maxSize): base(name, maxSize) { }

      public void AddFallbackValues(Item item, Field field, string value)
      {
         Assert.ArgumentNotNull(item, "item");
         Assert.ArgumentNotNull(field, "field");
         Assert.ArgumentNotNull(value, "value");
         SetString(GetKey(item, field), value);
      }

      private string GetKey(Item item, Field field)
      {
         return String.Format("{0}|{1}", item.Uri, field.ID);
      }

      public string GetFallbackValues(Item item, Field field)
      {
         Assert.ArgumentNotNull(item, "item");
         return GetString(GetKey(item, field));
      }
   }
}
