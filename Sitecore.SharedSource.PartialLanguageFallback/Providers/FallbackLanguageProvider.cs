using System.Collections.Generic;
using System.Collections.Specialized;
using Sitecore.Configuration;
using Sitecore.Data.Engines.DataCommands;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Eventing;
using Sitecore.Eventing.Remote;
using Sitecore.SharedSource.PartialLanguageFallback.Caching;
using Sitecore.SharedSource.PartialLanguageFallback.Extensions;
using Sitecore.SharedSource.PartialLanguageFallback.Managers;

namespace Sitecore.SharedSource.PartialLanguageFallback.Providers
{
   public class FallbackLanguageProvider : StandardValuesProvider
   {
      #region Fields

      private Dictionary<string, FallbackValuesCache> _fallbackValuesCaches;
      private Dictionary<string, FallbackValuesCache> _fallbackIgnoreCaches;

      #endregion

      #region Event Handlers

      private void DataEngine_RemoveVersion(object sender, ExecutedEventArgs<RemoveVersionCommand> e)
      {
         ClearFallbackCaches(e.Command.Item.Uri, e.Command.Database);
      }

      private void DataEngine_DeletedItem(object sender, ExecutedEventArgs<DeleteItemCommand> e)
      {
         ClearFallbackCaches(e.Command.Item.Uri, e.Command.Database);
      }

      private void DataEngine_SavedItem(object sender, ExecutedEventArgs<SaveItemCommand> e)
      {
         ClearFallbackCaches(e.Command.Item.Uri, e.Command.Database);
      }

      private void ClearFallbackCaches(ItemUri itemUri, Database database)
      {
         var cache = _fallbackValuesCaches[database.Name];
         var ignore_cache = _fallbackIgnoreCaches[database.Name];

         //<!--CHANGED BY VERNDALE-->
         // this is called by InitializeEventHandlers, EventManager.Subscribe<PublishEndRemoteEvent>, where it passes in a null ItemUri
         // Therefore on CD servers, where PublishEndRemoteEvent would be called, there would have been null errors when trying to use itemUri
         // must add null check and if null, whole fallback cache should be cleared

         if (cache != null)
         {
             // Added a null check on itemUri
            if (itemUri == null)
                cache.Clear();
            else
                cache.RemoveKeysContaining(itemUri.ItemID.ToString());
         }
         if (ignore_cache != null)
         {
            // Added a null check on itemUri
            if (itemUri == null)
                ignore_cache.Clear();
            else
                ignore_cache.RemoveKeysContaining(itemUri.ItemID.ToString());
         }
      }

      #endregion

      #region Initializers

      private void InitializeEventHandlers(Database database)
      {
         var dataEngine = database.Engines.DataEngine;
         dataEngine.DeletedItem += DataEngine_DeletedItem;
         dataEngine.SavedItem += DataEngine_SavedItem;
         dataEngine.RemovedVersion += DataEngine_RemoveVersion;
         EventManager.Subscribe<PublishEndRemoteEvent>(@event => ClearFallbackCaches(null, database));
      }

      #endregion

      #region Properties

      public string SupportedDatabases { get; set; }

      #endregion

      #region Overrides

      public override void Initialize(string name, NameValueCollection config)
      {
         //LogUtil.Instance.Debug("base.Initialize");
         base.Initialize(name, config);

         _fallbackValuesCaches = new Dictionary<string, FallbackValuesCache>();
         _fallbackIgnoreCaches = new Dictionary<string, FallbackValuesCache>();

         //LogUtil.Instance.Debug("Settings.ConfigurationIsSet");
         if (!Settings.ConfigurationIsSet) return;

         //LogUtil.Instance.Debug("Instatiating fallback caches");
         foreach (var dbName in SupportedDatabases.Split(new[] { '|', ' ', ',' }))
         {
            var database = Factory.GetDatabase(dbName);

            if (database == null) continue;

            //LogUtil.Instance.Debug("Instatiating fallback caches for database: {0}", database.Name);
            _fallbackValuesCaches.Add(database.Name,
                                      new FallbackValuesCache(database.Name + "[fallbackValues]", Config.FallbackCacheSize));
            _fallbackIgnoreCaches.Add(database.Name,
                      new FallbackValuesCache(database.Name + "[fallbackIgnoreFields]", Config.FallbackCacheSize));

            //LogUtil.Instance.Debug("Instatiating event handlers for database: {0}", database.Name);
            InitializeEventHandlers(database);
         }
      }

      public override string GetStandardValue(Field field)
      {
         Assert.ArgumentNotNull(field, "field");
         var item = field.Item;
         Assert.ArgumentNotNull(item, "item");

         //LogUtil.Instance.Debug("DatabaseCheck. Item: {0} Field: {1}", field.Item.ID, field.Name);
         if (!DatabaseCheck(item))
         {
            //LogUtil.Instance.Debug("DatabaseCheck failed");
            return base.GetStandardValue(field);
         }
         //LogUtil.Instance.Debug("DatabaseCheck successful");

         if (!field.ValidForFallback())
         {
            //LogUtil.Instance.Debug("Returning base.GetStandardValue. Item: {0} Field: {1}", field.Item.ID, field.Name);
            return base.GetStandardValue(field);
         }

         var siteFallbackEnabled = Config.SiteFallbackEnabled;

         if (!siteFallbackEnabled)
         {
            //LogUtil.Instance.Debug("Fallback is not enabled for site {0}".FormatWith(Context.Site.Name));
            return base.GetStandardValue(field);
         }

         string returnvalue;
         if (TryCacheFirst(item, field, out returnvalue)) return returnvalue;

         if (field.FallbackEnabled())
         {
            //LogUtil.Instance.Debug("GetFallbackValue. Item: {0} Field: {1}", field.Item.ID, field.Name);
            var fallbackValue = GetFallbackValue(field, item);

            return fallbackValue ?? GetStandardValueAndCacheIgnored(item, field);
         }

         //LogUtil.Instance.Debug("Returning base.GetStandardValue. Item: {0} Field: {1}", field.Item.ID, field.Name);
         return GetStandardValueAndCacheIgnored(item, field);
      }

      #endregion

      #region Processing Methods

      protected virtual string GetFallbackValue(Field field, Item item)
      {
         var fallbackValuesFromCache = GetFallbackValuesFromCache(item, field);

         if (fallbackValuesFromCache == null)
         {
            fallbackValuesFromCache = FallbackLanguageManager.ReadFallbackValue(field, item);
            AddFallbackValuesToCache(item, field, fallbackValuesFromCache);
         }

         return fallbackValuesFromCache;
      }

      #endregion

      #region Validation

      protected virtual bool DatabaseCheck(Item item)
      {
         if (item == null) return false;

         var result = SupportedDatabases.Contains(item.Database.Name.ToLowerInvariant());

         return result;
      }

      #endregion

      #region Caching Methods

      protected virtual void AddFallbackValuesToCache(Item item, Field field, string value)
      {
         if (value != null)
         {
            _fallbackValuesCaches[item.Database.Name].AddFallbackValues(item, field, value);
         }
      }

      protected virtual string GetFallbackValuesFromCache(Item item, Field field)
      {
         var cache = _fallbackValuesCaches[item.Database.Name];
         return cache != null ? cache.GetFallbackValues(item, field) : null;
      }

      protected virtual string GetStandardValueAndCacheIgnored(Item item, Field field)
      {
         var standardvalue = base.GetStandardValue(field) ?? string.Empty;
         _fallbackIgnoreCaches[item.Database.Name].AddFallbackValues(item, field, standardvalue);
         return standardvalue;
      }

      private bool TryCacheFirst(Item item, Field field, out string returnvalue)
      {
         returnvalue = _fallbackIgnoreCaches[item.Database.Name].GetFallbackValues(item, field);
         if (returnvalue == null)
         {
            returnvalue = _fallbackValuesCaches[item.Database.Name].GetFallbackValues(item, field);
            return returnvalue != null;
         }
         return true;
      }
      #endregion
   }
}
