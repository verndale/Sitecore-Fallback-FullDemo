using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Data.Items;
using Sitecore.StringExtensions;
using Sitecore.SharedSource.PartialLanguageFallback.Extensions;
using Verndale.SharedSource.Helpers;

namespace sitecore_modules.Web.LanguageTools
{
    public partial class Language_Fallback_Report : System.Web.UI.Page
    {
        protected Item RootItem
        {
            get
            {
                // append the path entered by the user to /sitecore and get the item to start the report from
                var root = txtItemPath.Text;
                return root.IsNullOrEmpty() ? null : CurrentDatabase.GetItem("/sitecore" + root);
            }
        }

        protected Database CurrentDatabase
        {
            // sets to either the selected database or 'master' if none
            get { return Factory.GetDatabase(ddlDatabase.SelectedValue.IsNullOrEmpty() ? "master" : ddlDatabase.SelectedValue); }
        }

        private static String _fieldToCheckForReporting;
        private static String FieldToCheckForReporting
        {
            get
            {
                // setting of the fields to check for Fallback, in the Sitecore.SharedSource.PartialLanguageFallback.config
                if (_fieldToCheckForReporting == null)
                    _fieldToCheckForReporting = Settings.GetSetting("Fallback.FieldToCheckForReporting");

                return _fieldToCheckForReporting;
            }
        }

        protected bool HideFilteredContent
        {
            get { return chkHideFilteredContent.Checked; }
        }

        protected Language FilteredLanguage
        {
            get
            {
                var selectedLanguage = ddlFilteredLanguage.SelectedValue;
                Language filteredLanguage;

                if (!selectedLanguage.IsNullOrEmpty() &&
                    Language.TryParse(selectedLanguage, out filteredLanguage))
                {
                    return filteredLanguage;
                }

                return LanguageManager.GetLanguage(Settings.DefaultLanguage);
            }
        }

        protected string FilterType
        {
            get { return ddlFilterType.SelectedValue; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetupLists();
                // output the list of fields that will be used to test fallback, so the user knows what they are
                // they are delimited by comma, so replace by comma space to make more user friendly
                ltlFieldsToCheckForReporting.Text = FieldToCheckForReporting.Replace("," , ", ");
            }

            langWrapper.Visible = false;
        }

        // This function will bind "Database" and "Language" dropdowns...
        protected void SetupLists()
        {
            // Binding Database names here, default master
            foreach (var db in Factory.GetDatabaseNames())
            {
                ddlDatabase.Items.Add(new ListItem(db)
                {
                    Selected = db.Equals("master", StringComparison.InvariantCultureIgnoreCase)
                });
            }

            // Binding Languages here, default to en
            foreach (var language in LanguageManager.GetLanguages(CurrentDatabase))
            {
                ddlFilteredLanguage.Items.Add(new ListItem(language.Name)
                {
                    Selected = language.Name.Equals("en", StringComparison.InvariantCultureIgnoreCase)
                });
            }
        }

        public void btnRun_Click(Object sender, EventArgs e)
        {
            // validate the mandatory fields are set before running the report
            if (CheckIncomingParameters())
            {
                ajaxloader.Attributes.Add("style", "display:");
                langWrapper.Visible = true;

                try
                {
                    // the results will be appended together using a stringbuilder
                    // this is because of the recursive nature of the sitecore tree
                    var displayResults = new StringBuilder();
                    displayResults.AppendLine("<ul class=\"fallback-report\">");
                        
                    bool doesQualify = false;
                    // get the results from GetResultDisplay, pass in the RootItem to start the report
                    // the out variable is more useful for the recursive calls within the method, 
                    // but basically it specifies if the item being passed in matches the filter criteria
                    var results = GetResultDisplay(RootItem, out doesQualify);
                    displayResults.Append(results.ToString());
                    displayResults.AppendLine("</ul>");
                        
                    // output the stringbuilder results to a literal
                    ltlReportResult.Text = displayResults.ToString();
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    langWrapper.Visible = false;
                    lblMessage.Text = "An error occured, Please check with a system administrator or see log for more details.";
                    Sitecore.Diagnostics.Log.Error("Error in Generating Report", ex, this);
                }

                ajaxloader.Attributes.Add("style", "display:none");
            }
        }

        protected bool CheckIncomingParameters()
        {
            // check that the user has entered something into the path textbox
            // check that the root item is specified and exists
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
            
            return true;
        }

        // this is the main method that gets the results of the report
        // it is called recursively as we iterate through the tree
        // it checks if the current item matches the filtered criteria and will output the item style based on that
        // additionally it will output the language versions for the item and then check its children
        // it is important to pass back whether the current item qualifies or not for the filters
        // this is because if the user has selected to hide items that don't qualify, 
        // we don't want to hide a parent item where a child DOES qualify
        private StringBuilder GetResultDisplay(Item startItem, out bool doesQualify)
        {
            var theseResults = new StringBuilder();

            // GetLanguages gets back a string of language version that are applicable to the item
            var languageDisplay = GetLanguages(startItem);

            // get the version of the item for the filtered language
            // if the version has a count of 0 versions, then consider it null
            var thisLanguageVersion = startItem.Versions.GetLatestVersion(FilteredLanguage);
            if (thisLanguageVersion != null && thisLanguageVersion.Versions.Count == 0)
                thisLanguageVersion = null;

            // if the item is not null for the filtered language, call the CheckIfFallingBack method
            // this method will check certain pre-defined fields on the item to see if values are set for them (FieldToCheckForReporting)
            // if they are all null and the language is set to fallback, then this item, in the filtered language, is falling back
            // it also gives back the language that it is falling back to
            Language languageFallingBackTo = null;
            var allSpecifiedFieldsAreFallingBack = false;
            if (thisLanguageVersion != null)
            {
                languageFallingBackTo = thisLanguageVersion.Language;
                allSpecifiedFieldsAreFallingBack = CheckIfFallingBack(thisLanguageVersion, out languageFallingBackTo);
            }
            
            var thisName = startItem.Name;
            doesQualify = false;

            // 1. if filter type is 'WithVersion', then qualify those that DO have a version in this language
            // 2. if filter type is 'WithoutVersion', then qualify those that DON'T have a version in this language
            // 3. if filter type is 'ExplicitContent', then qualify those where the content for certain specific fields 
            // was found to HAVE any overrides (any explicitly set content) in the selected language (and obviously must have a languageversion)
            // 4. if filter type is 'FallbackContent', then qualify those where the content for certain specific fields 
            // was found to NOT HAVE at least one override (any explicitly set content) in the selected languaged (or missing the language version entirely)
            if ((FilterType == "WithVersion" && thisLanguageVersion != null)
                || (FilterType == "WithoutVersion" && thisLanguageVersion == null)
                || (FilterType == "ExplicitContent" && thisLanguageVersion != null && !allSpecifiedFieldsAreFallingBack)
                || (FilterType == "FallbackContent" && (thisLanguageVersion == null || allSpecifiedFieldsAreFallingBack)))
            {
                doesQualify = true;
            }
            
            // loop through all of the children items of the current item (this is where the recursive call is)
            // will get back the entire display of descendents, including their list of languages
            // it will also return whether any of the children items qualify for the filter parameters
            // the display results of the children are in a separate stringbuilder for now 
            // and will be a applied to the current stringbuilder below
            var childrenItems = startItem.Children;
            var childResults = new StringBuilder();
            var atLeastOneChildQualifies = false;
            foreach (Item childItem in childrenItems)
            {
                var childQualifies = false;
                var thisChildResults = GetResultDisplay(childItem, out childQualifies);
                if (childQualifies)
                    atLeastOneChildQualifies = true;
                childResults.Append(thisChildResults.ToString());
            }

            // if the user has selected to hide filtered content 
            // and neither the current item nor any of its descendents qualify for the filters
            // then set a variable that will be used to set a class that will hide this item and its children
            var hideItemClass = "";
            if (HideFilteredContent && !doesQualify && !atLeastOneChildQualifies)
            {
                hideItemClass = " HideItem";
            }
            // if the item doesn't qualify for the filters, then make its font gray
            if (!doesQualify)
            {
                thisName = "<font color='gray'>" + thisName + "</font>";
            }

            // if there are no children for the current item, then set a variable that will set the toggle button to hide
            var hideToggleCss = "";
            if (!childrenItems.Any())
                hideToggleCss = " ToggleHidden";

            // start appending the output of the current item, include the css for hiding the item and hiding the toggle
            theseResults.AppendLine("<li class=\"fsr-child-root " + hideItemClass + "\">");
            theseResults.AppendLine("<div class=\"legend-root " + hideToggleCss + "\">");
            if (childrenItems.Any())
                theseResults.AppendLine("<span class=\"toggle-btn\" title=\"Expand\"></span>");
            theseResults.AppendLine(thisName + ":");
            theseResults.AppendLine("</div>");
            // output the language display for the item, which is also toggleable
            theseResults.AppendLine("<span class=\"toggle-language-btn\" title=\"Expand\"></span><div class='language-list'>" + languageDisplay + "</div>");
            // output the child display
            if (childrenItems.Any())
            {
                theseResults.AppendLine("<ul class=\"fallback-report content\">");
                theseResults.Append(childResults);
                theseResults.AppendLine("</ul>");
            }
            else
            {
                theseResults.AppendLine("<ul class=\"content\"> </ul>");
            }
            theseResults.AppendLine("</li>");

            return theseResults;
        }

        // Check if there are any values configured for this item in "Fallback.FieldToCheckForReporting", a Sitecore Setting in Fallback Config 
        // contains a comma delimited string of all the Fields that needs to be check to see if any of the field has it's own value.
        // and if any of those fields has its own value, then language item is not falling back...
        // this method will take in the language version of the item we are checking 
        // and will return out the language it is ultimately falling back to, if applicable
        private bool CheckIfFallingBack(Item thisLanguageVersion, out Language languageFallingBackTo)
        {
            bool contentIsFallingBack = true;
            languageFallingBackTo = null;
            if (thisLanguageVersion != null && thisLanguageVersion.Language != null)
            {
                // default the languageFallingBackTo to this versions language
                languageFallingBackTo = thisLanguageVersion.Language;
                // get the fields to check, splitting the values on the comma
                var fallbackFieldsList = FieldToCheckForReporting.Split(',');
                // loop through each field and check if the current item language version has a value for the field
                // if ANY do, then the content is not considered falling back
                // NOTE, of course this means some fields could be falling back and others not, 
                // but the point of this report is to get an idea of where we have set content explicitly and what items haven't been translated yet
                // so we are assuming if any of the important specified fields do have explicit content set, then it is no longer falling back.
                foreach (var field in fallbackFieldsList)
                {
                    if (thisLanguageVersion.Fields[field] != null)
                    {
                        var hasContentInField = thisLanguageVersion.Fields[field].HasValue;
                        if (hasContentInField)
                        {
                            contentIsFallingBack = false;
                            break;
                        }
                    }
                }

                // if the content is falling back (fields are all null), check if the language has fallback set
                // if fallback is not assigned, then clearly it can't be falling back, even if no values have been set yet for the fields
                // if it does, then get what language it is set to fall back to and call this method recursively for the item in that language
                // we are going to keep going back until we find content, and that will be the language we end up returning, 
                // so we know ultimately what language is being displayed
                if (contentIsFallingBack)
                {
                    var hasFallbackAssigned = false;
                    try
                    {
                        hasFallbackAssigned = thisLanguageVersion.Language.HasFallbackAssigned(CurrentDatabase);
                    }
                    catch (Exception ex)
                    {
                        Sitecore.Diagnostics.Log.Error("Error checking for fallback of item '" + thisLanguageVersion.Name + "'", ex, this);
                    }
                    if (hasFallbackAssigned)
                    {
                        var fallbackLanguage = thisLanguageVersion.Language.GetFallbackLanguage(CurrentDatabase);
                        var fallbackLanguageVersion = thisLanguageVersion.Versions.GetLatestVersion(fallbackLanguage);
                        CheckIfFallingBack(fallbackLanguageVersion, out languageFallingBackTo);
                    }
                    else
                        contentIsFallingBack = false;
                }
            }
            return contentIsFallingBack;
        }

        

        // This function will return all the languages configured for the particular item being displayed
        private string GetLanguages(Item item)
        {
            var languageStr = new StringBuilder();

            // item.Versions.GetVersions(true) will return all the language versions configured for the particular item
            // further limit these where language isn't null and the name isn't blank
            // language will be null for invariant versions
            // loop through all language versions for the current item
            var versions = item.Versions.GetVersions(true);
            var versionList = versions.Where(x => x.Language != null && !String.IsNullOrEmpty(x.Language.Name)).ToList();
            foreach (var languageVersion in versionList)
            {
                // again, just make sure not outputting for a null language
                if (languageVersion.Language != null && !String.IsNullOrEmpty(languageVersion.Language.Name))
                {
                    // we are outputting the languaged delimited by comma
                    // so obviously if we are on the last one, don't output the comma
                    var commaStr = ",";
                    if (languageVersion == versionList.Last())
                        commaStr = "";

                    // use the CheckIfFallingBack method again here to figured out if the language is falling back or not
                    // and if so, what language it is falling back to
                    var languageFallingBackTo = languageVersion.Language;
                    var allSpecifiedFieldsAreFallingBack = CheckIfFallingBack(languageVersion, out languageFallingBackTo);

                    // we are going to output the display of each language in the list color-coded based on fallback and explicit content
                    // if any of the fields has explicit content, it is red and not falling back 
                    // (again, high level, there COULD be fields that are falling back, just not all of the specified ones to check)
                    if (!allSpecifiedFieldsAreFallingBack)
                        languageStr.AppendLine("<div class='legend red'><span>" + languageVersion.Language.Name + "</span>" +
                                           commaStr + "</div>");
                    else
                    {
                        // if the language it is falling back to IS the filtered language, then output the language in blue
                        // otherwise if there is fallback, but to a different language, display in green
                        var colorCss = "green";
                        if (languageFallingBackTo == FilteredLanguage)
                            colorCss = "blue";
                        languageStr.AppendLine("<div class='legend " + colorCss + "'><span>" + languageVersion.Language.Name);
                        
                        // if the language is falling back to a different language, then show an arrow and the name of that other language 
                        if (languageFallingBackTo != languageVersion.Language)
                        {
                            languageStr.AppendLine("<span class='arrow'></span>");
                            languageStr.AppendLine(languageFallingBackTo.Name);
                        }
                        languageStr.AppendLine("</span>" + commaStr + "</div>");
                    }
                }
            }

            var languages = languageStr.ToString();
            return languages;
        }

        
    }
}