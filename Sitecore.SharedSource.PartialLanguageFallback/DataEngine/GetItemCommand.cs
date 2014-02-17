using System;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;

namespace Sitecore.SharedSource.PartialLanguageFallback.DataEngine
{
   public class GetItemCommand : Sitecore.Data.Engines.DataCommands.GetItemCommand
   {
      protected override Sitecore.Data.Engines.DataCommands.GetItemCommand CreateInstance()
      {
         return new GetItemCommand();
      }

      protected override Item DoExecute()
      {
         var item = base.DoExecute();

         if (item != null &&
            !String.IsNullOrEmpty(item.Language.Name) &&
             Config.VersionPresenceEnforced &&
             !item.Name.StartsWith("__") && 
             IsOfSupportedTemplate(item))

            return EmptyVersion(item) ? null : item;

         return item;
      }

      protected virtual bool IsOfSupportedTemplate(Item item)
      {
         var template = TemplateManager.GetTemplate(item.TemplateID, item.Database);
         return Config.SupportedTemplateIDs.Any(template.DescendsFromOrEquals);
      }

      protected bool EmptyVersion(Item item)
      {
         return item == null || item.Versions.Count == 0;
      }
   }
}
