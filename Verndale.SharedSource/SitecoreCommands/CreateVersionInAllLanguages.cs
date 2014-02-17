using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Text;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Verndale.SharedSource.Helpers;

namespace Verndale.SharedSource.SitecoreCommands
{
    public class CreateVersionInAllLanguages : Command
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");

            if (context.Items.Length == 1)
            {
                NameValueCollection parameters = new NameValueCollection();
                parameters["items"] = base.SerializeItems(context.Items);
                Context.ClientPage.Start(this, "Run", parameters);
            }
        }

        protected void Run(ClientPipelineArgs args)
        {
            Item item = base.DeserializeItems(args.Parameters["items"])[0];
            if (item == null)
            {
                return;
            }

            if (SheerResponse.CheckModified())
            {
                //prompt user they want to add versions
                if (args.IsPostBack)
                {
                    if (args.Result == "yes")
                    {
                        LanguageHelper.CreateVersionInEachLanguage(item);

                        Sitecore.Web.UI.HtmlControls.DataContext contentEditorDataContext = Sitecore.Context.ClientPage.FindControl("ContentEditorDataContext") as Sitecore.Web.UI.HtmlControls.DataContext;
                        contentEditorDataContext.SetFolder(item.Uri);
                    }
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("Create version for each language?");
                    SheerResponse.Confirm(builder.ToString());
                    args.WaitForPostBack();
                }
            }
        }
    }
}
