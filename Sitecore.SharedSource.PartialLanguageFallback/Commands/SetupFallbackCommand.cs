using System;
using System.Collections.Specialized;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.StringExtensions;

namespace Sitecore.SharedSource.PartialLanguageFallback.Commands
{
   [Serializable]
   public class SetupFallbackCommand : Command
   {
      protected string FieldId { get; set; }

      protected string FieldValue { get; set; }

      protected bool FieldParametersDefined
      {
         get { return !FieldId.IsNullOrEmpty() && !FieldValue.IsNullOrEmpty(); }
      }

      public override void Execute(CommandContext context)
      {
         Assert.ArgumentNotNull(context, "context");
         if (context.Items.Length == 1)
         {
            var item = context.Items[0];
            var parameters = new NameValueCollection();
            parameters["id"] = item.ID.ToString();
            parameters["language"] = item.Language.ToString();
            parameters["version"] = item.Version.ToString();

            FieldId = context.Parameters["fieldid"];
            FieldValue = context.Parameters["fieldvalue"];

            if (FieldParametersDefined)
            {
               Context.ClientPage.Start(this, "Run", parameters);
            }
         }
      }

      public override CommandState QueryState(CommandContext context)
      {
         Assert.ArgumentNotNull(context, "context");
         if (context.Items.Length != 1)
         {
            return CommandState.Hidden;
         }

         var item = context.Items[0];

         if (item == null)
            return CommandState.Hidden;

         if (!Context.IsAdministrator)
            return CommandState.Hidden;

         if (item.TemplateID == TemplateIDs.TemplateFolder ||
            item.TemplateID == TemplateIDs.Template ||
            item.TemplateID == TemplateIDs.TemplateSection ||
            item.TemplateID == TemplateIDs.TemplateField)
         {
            return CommandState.Enabled;
         }

         return CommandState.Hidden;
      }

      protected void Run(ClientPipelineArgs args)
      {
         Assert.ArgumentNotNull(args, "args");
         var id = args.Parameters["id"];
         var lang = args.Parameters["language"];
         var ver = args.Parameters["version"];
         var item = Context.ContentDatabase.Items[id, Language.Parse(lang), Data.Version.Parse(ver)];

         if (Context.IsAdministrator || (item.Access.CanWrite() && (item.Locking.CanLock() || item.Locking.HasLock())))
         {
            if (item == null)
            {
               SheerResponse.Alert("Item not found.", new string[0]);
            }
            else if (SheerResponse.CheckModified())
            {
               SetupFallback(item);
            }
         }
      }

      protected virtual void SetupFallback(Item item)
      {
         if (item.TemplateID.Equals(TemplateIDs.TemplateFolder)) { SetupTemplateFolderFallback(item); return; }

         if (item.TemplateID.Equals(TemplateIDs.Template)) { SetupTemplateFallback(item); return; }

         if (item.TemplateID.Equals(TemplateIDs.TemplateSection)) { SetupSectionFallback(item); return; }

         if (item.TemplateID.Equals(TemplateIDs.TemplateField)) { SetupFieldFallback(item); return; }
      }

      protected virtual void SetupTemplateFolderFallback(Item templateFolderItem)
      {
         if (templateFolderItem == null) return;

         foreach (Item child in templateFolderItem.Children)
         {
            if (child.TemplateID.Equals(TemplateIDs.Template))
            {
               SetupTemplateFallback(child);
            }
            if (child.TemplateID.Equals(TemplateIDs.TemplateFolder))
            {
               SetupTemplateFolderFallback(child);
            }
         }
      }

      protected virtual void SetupTemplateFallback(Item templateItem)
      {
         var template = TemplateManager.GetTemplate(templateItem.ID, templateItem.Database);

         if (template != null)
         {
            foreach (var field in template.GetFields(false))
            {
               SetupFieldFallback(field, templateItem.Language, templateItem.Database);
            }
         }
      }

      protected virtual void SetupSectionFallback(Item item)
      {
         var template = TemplateManager.GetTemplate(item.ParentID, item.Database);

         if (template != null)
         {
            var section = template.GetSection(item.ID);

            if (section != null)
            {
               foreach (var field in section.GetFields())
               {
                  SetupFieldFallback(field, item.Language, item.Database);
               }
            }
         }
      }

      protected virtual void SetupFieldFallback(TemplateField field, Language language, Database database)
      {
         var fieldItem = database.GetItem(field.ID, language);

         if (fieldItem.Versions.Count == 0)
            fieldItem = fieldItem.Versions.AddVersion();

         SetupFieldFallback(fieldItem);
      }

      protected virtual void SetupFieldFallback(Item fieldItem)
      {
         if (FieldParametersDefined && fieldItem != null && fieldItem.Fields[FieldId] != null)
         {
            using (new SecurityDisabler())
            {
               fieldItem.Editing.BeginEdit();
               fieldItem.Fields[FieldId].Value = FieldValue;
               //SetupValidators(fieldItem);
               fieldItem.Editing.EndEdit();
            }
         }
      }
   }
}
