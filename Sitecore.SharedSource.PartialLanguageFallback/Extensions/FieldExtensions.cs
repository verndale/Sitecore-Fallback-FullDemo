using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.StringExtensions;

namespace Sitecore.SharedSource.PartialLanguageFallback.Extensions
{
   public static class FieldExtensions
   {
      public static Item InnerItem(this Field field)
      {
         using (new SecurityDisabler())
         {
            return field.Database.GetItem(field.ID, field.Language);
         }
      }

      public static bool HasInnerFieldValue(this Field field, string fieldToken, string val)
      {
         var innerItem = InnerItem(field);

         if (innerItem == null) return false;

         return innerItem[fieldToken] == val;
      }

      public static bool IsOfType<T>(this Field field)
      {
         return FieldTypeManager.GetField(field) is T;
      }

      public static bool ContainsFallbackValue(this Field field)
      {
         return ContainsFallbackValue(field, Context.Language);
      }

      public static bool ContainsFallbackValue(this Field field, Language language)
      {
         if (language == null) return false;

         if (!LanguageManager.IsLanguageNameDefined(field.Database, language.Name)) return false;

         if (!field.GetValue(false, false).IsNullOrEmpty()) return false;

         if (language.Name.Equals(Config.MasterLanguage)) return false;

         if (!Config.SiteFallbackEnabled) return false;

         if (!field.ValidForFallback()) return false;

         if (!FallbackEnabled(field)) return false;

         var item = field.Item;
         var database = item.Database;

         if (language.HasFallbackAssigned(database))
         {
            var fallbackLanguage = language.GetFallbackLanguage(database);

            var fallbackItem = database.GetItem(item.ID, fallbackLanguage);

            if (fallbackItem == null || fallbackItem.Versions.Count == 0) return false;

            return fallbackItem.ContainsField(field.ID) && fallbackItem[field.ID].Equals(item[field.ID]);
         }

         return false;
      }

      public static bool FallbackEnabled(this Field field)
      {
         Assert.IsNotNull(field, "field cannot be null");

         var item = field.Item;
         Assert.IsNotNull(item, "item cannot be null");

         // if the fallback settings does not need to vary for each language, we need to read this setting from the master lang ver
         if(!Config.VaryFallbackSettingsPerLanguage)
         {
            var masterItem = field.Item.GetMasterItem();

            if(masterItem == null)
            {
               return false;
            }

            field = masterItem.Fields[field.ID];
         }

         return field.HasInnerFieldValue(Constants.FieldIds.EnableFallbackIfConfigured, "1");
      }

      public static bool ValidForFallback(this Field field)
      {
         // ignoring __Source field that is a part of Item Cloning functionality
         // if this field is processed, the SV provider goes into infinite loop
         if (field.ID.Equals(FieldIDs.Source))
         {
            //LogUtil.Instance.Debug("ValidForFallback returns false. Fields is 'Source'. Field: {0}", field.Name);
            return false;
         }

         if (Config.IgnoredFields.Contains(field.ID.ToString()))
         {
            //LogUtil.Instance.Debug("ValidForFallback returns false. This field is setup to be ignored in configuration. Field: {0}", field.Name);
            return false;
         }

         // if configuration is set to ignore system fields - return false
         if (!Config.ProcessSystemFields && field.IsSystem())
         {
            //LogUtil.Instance.Debug("ValidForFallback returns false. System fields are setup not to fallback. Field: {0}", field.Name);
            return false;
         }

         // preserve for backwards compatibility
         if (ID.Parse(Constants.FieldIds.AlwaysEnforceFallback).Equals(field.ID))
         {
            //LogUtil.Instance.Debug("ValidForFallback returns false. Fields is 'AlwaysEnforceFallback'. Field: {0}", field.Name);
            return false;
         }

         if (ID.Parse(Constants.FieldIds.EnableFallbackIfConfigured).Equals(field.ID))
         {
            //LogUtil.Instance.Debug("ValidForFallback returns false. Fields is 'EnableFallbackIfConfigured'. Field: {0}", field.Name);
            return false;
         }

         //LogUtil.Instance.Debug("ValidForFallback returns true. Field is OK to fallback. Field: {0}", field.Name);
         return true;
      }

      public static bool IsSystem(this Field field)
      {
         return field.Name.StartsWith("__");
      }
   }
}
