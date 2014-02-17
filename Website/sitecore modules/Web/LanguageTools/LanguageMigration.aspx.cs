using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Jobs;
using Sitecore.SecurityModel;
using Sitecore.StringExtensions;
using Version = Sitecore.Data.Version;
using Verndale.SharedSource.Helpers;

namespace sitecore_modules.Web.LanguageTools
{
    public partial class LanguageMigration : System.Web.UI.Page
    {
        protected Item RootItem
        {
            get
            {
                // append the path entered by the user to /sitecore and get the item to start the migration with
                var root = txtItemPath.Text;
                return root.IsNullOrEmpty() ? null : CurrentDatabase.GetItem("/sitecore" + root);
            }
        }

        protected bool DryRun
        {
            get { return chkDryRun.Checked; }
        }

        protected bool DeleteSource
        {
            get { return chkDeleteSource.Checked; }
        }
        protected string Action
        {
            get { return ddlAction.SelectedValue; }
        }

        protected bool ProcessChildren
        {
            get { return chkProcessChildren.Checked; }
        }

        protected bool ProcessRecursively
        {
            get { return chkProcessRecursively.Checked; }
        }

        protected bool TransferOnlyContentFields
        {
            get { return chkOnlyContentFields.Checked; }
        }

        protected bool TransferOnlyVersion
        {
            get { return chkOnlyVersion.Checked; }
        }

        protected bool OnlyCreateIfNoVersion
        {
            get { return chkOnlyCreateIfNoVersion.Checked; }
        }

        protected bool AddAllOtherLanguages
        {
            get { return chkAddAllOtherLanguages.Checked; }
        }

        protected Database CurrentDatabase
        {
            // sets to either the selected database or 'master' if none
            get { return Factory.GetDatabase(ddlDatabase.SelectedValue.IsNullOrEmpty() ? "master" : ddlDatabase.SelectedValue); }
        }

        protected Language SourceLanguage
        {
            get
            {
                var selectedLanguage = ddlSourceLanguage.SelectedValue;
                Language sourceLanguage;

                if (!selectedLanguage.IsNullOrEmpty() &&
                    Language.TryParse(selectedLanguage, out sourceLanguage))
                {
                    return sourceLanguage;
                }

                return LanguageManager.DefaultLanguage;
            }
        }

        protected Language TargetLanguage
        {
            get
            {
                var selectedLanguage = ddlTargetLanguage.SelectedValue;
                Language targetLanguage;

                if (!selectedLanguage.IsNullOrEmpty() &&
                    Language.TryParse(selectedLanguage, out targetLanguage))
                {
                    return targetLanguage;
                }

                return LanguageManager.DefaultLanguage;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetupLists();
            }

            DefaultFields();
        }

        protected void SetupLists()
        {
            // bind all sitecore databases, default selection to master
            foreach (var db in Factory.GetDatabaseNames())
            {
                ddlDatabase.Items.Add(new ListItem(db) { Selected = db.Equals("master", StringComparison.InvariantCultureIgnoreCase) });
            }

            // bind all languages setup in sitecore, default to en
            foreach (var language in LanguageManager.GetLanguages(CurrentDatabase))
            {
                ddlSourceLanguage.Items.Add(new ListItem(language.Name) { Selected = language.Name.Equals("en", StringComparison.InvariantCultureIgnoreCase) });
                ddlTargetLanguage.Items.Add(new ListItem(language.Name) { Selected = language.Name.Equals("en", StringComparison.InvariantCultureIgnoreCase) });
            }

        }

        protected void DefaultFields()
        {
            // there is some logic as to certain fields being enabled or checked based on selections in other fields

            // if not processing children items, then not processing recursively either
            if (!chkProcessChildren.Checked)
                chkProcessRecursively.Checked = false;

            // if deleting target version, no point in having Source Language, and several of the checkboxes enabled, they don't apply
            if (ddlAction.SelectedValue == "DeleteTarget")
            {
                ddlSourceLanguage.Enabled = false;
                chkOnlyCreateIfNoVersion.Checked = true;
                chkOnlyCreateIfNoVersion.Enabled = false;
                chkOnlyContentFields.Checked = false;
                chkOnlyContentFields.Enabled = false;
                chkOnlyVersion.Checked = true;
                chkOnlyVersion.Enabled = false;
                chkDeleteSource.Checked = false;
                chkDeleteSource.Enabled = false;
                chkAddAllOtherLanguages.Checked = false;
                chkAddAllOtherLanguages.Enabled = false;
            }

            // if adding all other languages, target language is unnecessary 
            // and deleting source version wouldn't make sense since it is adding all languages
            if (chkAddAllOtherLanguages.Checked)
            {
                ddlTargetLanguage.Enabled = false;
                chkDeleteSource.Checked = false;
                chkDeleteSource.Enabled = false;
            }

            // if only adding the lbank version and no fields, then specifying only content fields doesn't apply
            if (chkOnlyVersion.Checked)
            {
                chkOnlyContentFields.Checked = false;
                chkOnlyContentFields.Enabled = false;
            }

            // deleting the source language is a big deal, warn the user and make them confirm
            chkDeleteSource.Attributes.Add("onclick", "return confirm('Are you sure you want to delete the source version?!');");
        }

        protected void Process_Click(object sender, EventArgs e)
        {
            // validate the mandatory fields are set before running the migration
            if (CheckIncomingParameters())
            {
                RunMigrationJob();
            }
        }

        protected bool CheckIncomingParameters()
        {
            // check that the root item is specified and exists
            // make sure the target language is different from the source language (as long as not adding all other languages)
            lblMessage.Visible = false;

            if (String.IsNullOrEmpty(txtItemPath.Text))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Please enter a path.";
                return false;
            }
            var rootItem = RootItem;
            if (rootItem == null)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Cannot find root item: /sitecore" + txtItemPath.Text;
                return false;
            }
            

            if ((Action != "DeleteTarget") && !AddAllOtherLanguages && SourceLanguage.Equals(TargetLanguage))
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Source and Target languages should not be identical";
                return false;
            }

            return true;
        }

        // starts the migration process
        protected Handle RunMigrationJob()
        {
            var manager = new LanguageMigrationManager(RootItem, CurrentDatabase, SourceLanguage, TargetLanguage, DryRun,
                                                       DeleteSource, Action, ProcessChildren, ProcessRecursively,
                                                       TransferOnlyContentFields, TransferOnlyVersion, OnlyCreateIfNoVersion, AddAllOtherLanguages);

            var options = new JobOptions("Migration", "LanguageMigration", "shell", manager, "RunMigration")
            {
                WriteToLog = true,
                AtomicExecution = true,
            };

            var job = new Job(options);
            JobManager.Start(job);

            return job.Handle;
        }
    }

    // this class is used for the migration process
    public class LanguageMigrationManager
    {
        protected Item RootItem
        {
            get;
            set;
        }

        protected Database SourceDatabase
        {
            get;
            set;
        }

        protected Language SourceLanguage
        {
            get;
            set;
        }

        protected Language TargetLanguage { get; set; }

        protected bool ProcessChildren { get; set; }

        protected bool ProcessRecursively { get; set; }

        protected bool DeleteSource { get; set; }
        protected string Action { get; set; }

        protected bool DryRun { get; set; }

        protected bool TransferOnlyContentFields { get; set; }

        protected bool TransferOnlyVersion { get; set; }
        protected bool OnlyCreateIfNoVersion { get; set; }
        protected bool AddAllOtherLanguages { get; set; }

        public LanguageMigrationManager(Item rootItem, Database database, Language sourceLanguage, Language targetLanguage, 
            bool dryRun, bool deleteSource, string action, bool processChildren, bool processRecursively, 
            bool transferOnlyContentFields, bool transferOnlyVersion, bool onlyCreateIfNoVersion, bool addAllOtherLanguages)
        {
            RootItem = rootItem;
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
            SourceDatabase = database;
            DryRun = dryRun;
            DeleteSource = deleteSource;
            Action = action;
            ProcessChildren = processChildren;
            ProcessRecursively = processRecursively;
            TransferOnlyContentFields = transferOnlyContentFields;
            TransferOnlyVersion = transferOnlyVersion;
            OnlyCreateIfNoVersion = onlyCreateIfNoVersion;
            AddAllOtherLanguages = addAllOtherLanguages;
        }

        public virtual void RunMigration()
        {
            Assert.IsNotNull(RootItem, "rootitem cannot be null");
            Migrate(RootItem);
        }

        protected virtual void Migrate(Item item)
        {
            // perform the migration for the specific item
            // then if user selected to process children, loop through the children
            // if processing recursively, then run migrate method for each child, otherwise, just perform migration on child item
            PerformMigration(item);

            if (ProcessChildren)
            {
                foreach (Item child in item.Children)
                {
                    if (ProcessRecursively)
                        Migrate(child);
                    else
                    {
                        PerformMigration(child);
                    }
                }
            }
        }

        protected void PerformMigration(Item item)
        {
            // get the item in the target language (should exist, but may not have any versions in that language)
            var targetLangItem = ItemManager.GetItem(item.ID, TargetLanguage, Version.Latest, item.Database);

            if (targetLangItem == null)
            {
                Error.LogError("LanguageMigrationJob. Cannot find target item in target language");
                return;
            }

            // If delete target action is selected (Delete all the target language versions without doing any migration.)
            // as long as there is at least one version in that language, 
            // Call DeleteTargetVersions which will loop through and delete all version in that language of the item
            if (Action == "DeleteTarget" && targetLangItem != null)
            {
                if ((targetLangItem.Versions.Count >= 1))
                    DeleteTargetVersions(targetLangItem);
            }
            else
            {
                // otherwise the action is MigrateSource
                // make sure there is at least once version in the source language of the item
                var sourceLangItem = ItemManager.GetItem(item.ID, SourceLanguage, Version.Latest, item.Database);
                if (sourceLangItem.Versions.Count < 1)
                {
                    Error.LogError("LanguageMigrationJob. Cannot find any source language versions to migrate");
                    return;
                }

                // loop through all versions in the language
                // if dry run was checked, simply log the action that it would have taken
                // otherwise, call CopyVersion method which will create the version in the target language
                foreach (var version in sourceLangItem.Versions.GetVersions(false))
                {
                    if (DryRun)
                    {
                        Log.Info(
                           "LanguageMigrationJob. The version will be migrated: {0} {1} {2}".FormatWith(version.Paths.FullPath,
                                                                                                        version.Version.Number.
                                                                                                           ToString(),
                                                                                                        version.Language.Name), this);
                    }
                    else
                    {
                        Log.Info(
                           "LanguageMigrationJob. Migrating version: {0} {1} {2}".FormatWith(version.Paths.FullPath,
                                                                              version.Version.Number.ToString(),
                                                                              version.Language.Name), this);

                        CopyVersion(version, targetLangItem);
                    }
                }
            }
        }

        protected void DeleteTargetVersions(Item targetLangItem)
        {
            // loop through all versions in the language
            // if dry run was checked, simply log the action that it would have taken
            // otherwise, call ItemManager.RemoveVersion method which will remove the version in the target language
            foreach (var version in targetLangItem.Versions.GetVersions(false))
            {
                if (DryRun)
                {
                    Log.Info(
                       "LanguageMigrationJob. The target language version will be deleted: {0} {1} {2}".FormatWith(version.Paths.FullPath,
                                                                                                    version.Version.Number.
                                                                                                       ToString(),
                                                                                                    version.Language.Name), this);
                }
                else
                {
                    Log.Info(
                       "LanguageMigrationJob. Target language version deleted: {0} {1} {2}".FormatWith(version.Paths.FullPath,
                                                                          version.Version.Number.ToString(),
                                                                          version.Language.Name), this);

                    using (new EditContext(version))
                    {
                        ItemManager.RemoveVersion(version);
                    }
                }
            }

        }

        protected void CopyVersion(Item sourceVersion, Item targetLangItem)
        {
            // if the checkbox to add all other language versions was checked,
            // call the CreateVersionInEachLanguage LanguageHelper method, it will create versions in all languages

            // otherwise, if the OnlyCreateIfNoVersion checkbox was checked 
            // (meaning if it already exists in a language, don't just create another version of it)
            // OR if there is no version yet, then add a version in the target language
            // and finally, only if user specified they want the field values copied via the chkOnlyVersion checkbox, call CopyFields method 
            if (AddAllOtherLanguages)
            {
                LanguageHelper.CreateVersionInEachLanguage(sourceVersion);
            }
            else if (!OnlyCreateIfNoVersion || targetLangItem.Versions.Count == 0)
            {
                using (new EditContext(targetLangItem))
                {
                    var newVer = targetLangItem.Versions.AddVersion();
                    if (!TransferOnlyVersion)
                        CopyFields(newVer, sourceVersion);
                }
            }

            // if delete source checkbox was checked (in a case where content is being transferred from one language to another)
            // then call ItemManager.RemoveVersion
            if (DeleteSource)
            {
                using (new EditContext(sourceVersion))
                {
                    ItemManager.RemoveVersion(sourceVersion);
                }
            }
        }

        protected void CopyFields(Item newVer, Item sourceVersion)
        {
            newVer.Fields.ReadAll();
            newVer.RuntimeSettings.ReadOnlyStatistics = true;

            // if user wants the field values copied over, then loop through all of the fields
            // if user wants all fields copied or the field is a Content Field (ShouldBeTranslated), then call CopyField
            using (new EditContext(newVer))
            {
                foreach (Field field in newVer.Fields)
                {
                    if (!TransferOnlyContentFields || field.ShouldBeTranslated)
                    {
                        CopyField(sourceVersion, field);
                    }
                }
            }
        }

        protected void CopyField(Item sourceVersion, Field field)
        {
            // copy the field value

            var sourceFieldValue = sourceVersion[field.ID];

            if (sourceFieldValue.IsNullOrEmpty()) return;

            field.Value = sourceFieldValue;
        }
    }
}