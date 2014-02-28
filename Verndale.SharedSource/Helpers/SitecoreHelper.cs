using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Xml.XPath;

using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Web.UI.WebControls;
using Sitecore.Collections;
using Sitecore.Xml.Xsl;
using Sitecore.Security.Accounts;
using Sitecore.SecurityModel;
using Sitecore.Search;
using Sitecore.Layouts;

namespace Verndale.SharedSource.Helpers
{
    public class SitecoreHelper
    {
        public class ItemMethods
        {
            /// <summary>
            /// Use this method when getting the item at a specific path, for a specific database
            /// </summary>
            /// <param name="path">string</param>
            /// <returns>Item </returns>
            public static Item GetItemByPath(string path, Database database)
            {
                return database.GetItem(path);
            }

            /// <summary>
            /// Use this method when getting the item at a specific path, for the current context database
            /// </summary>
            /// <param name="path">string</param>
            /// <returns>Item </returns>
            public static Item GetItemByPath(string path)
            {
                return Sitecore.Context.Database.GetItem(path);
            }

            /// <summary>
            /// Gets the item by path in master.
            /// </summary>
            /// <param name="path">The path.</param>
            /// <returns></returns>
            public static Item GetItemByPathInMaster(string path)
            {
                // get Master database
                Database masterDB = Factory.GetDatabase("master");
                return masterDB.GetItem(path);
            }

            /// <summary>
            ///Use this method when getting the item for a specific GUID.  Can pass in a specific database.
            /// Only use this method in special circumstances when you need to access master directly (eg, working with finding where clone came from).
            /// </summary>
            /// <param name="id">string</param>
            /// <returns>Item</returns>
            public static Item GetItemFromGUID(string guid, Database database)
            {
                return database.GetItem(new ID(guid));
            }

            /// <summary>
            ///Use this method when getting the item for a specific GUID.  Will use the current context database (web).
            /// </summary>
            /// <param name="id">string</param>
            /// <returns>Item</returns>
            public static Item GetItemFromGUID(string guid)
            {
                Item item = null;
                if (!string.IsNullOrEmpty(guid))
                {
                    Sitecore.Data.ID ItemID = ID.Parse(guid);
                    item = Sitecore.Context.Database.GetItem(ItemID);
                }
                return item;
            }

            /// <summary>
            /// This method will return the first ancestor item of the passed in item that has a matching template name or base template name
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="templatename">String</param>
            /// <returns>Item</returns>
            public static Item GetFirstAncestorByTemplate(Item item, String templatename)
            {
                if (item == null || templatename.Length == 0) return null;

                if (item.TemplateName.ToLower() == templatename.ToLower() || item.Template.BaseTemplates.Any(t => t.Name.ToLower() == templatename.ToLower()))
                {
                    return item;
                }
                else
                    return GetFirstAncestorByTemplate(item.Parent, templatename);
            }

            /// <summary>
            /// Searches for the first item that uses or inherits from the given template ID (in string form), starting from the given item and recursing through its ancestors.
            /// </summary>
            /// <param name="item">The item to start the search from.</param>
            /// <param name="templateId">The string form of the ID of the template that the returned item must use or inherit.</param>
            /// <returns>The first matching item, if one can be found; otherwise, null.</returns>
            public static Item GetAncestorOrSelfByTemplateId(Item item, string templateId)
            {
                if (item == null || string.IsNullOrEmpty(templateId))
                {
                    return null;
                }

                ID parsedTemplateId;
                return ID.TryParse(templateId, out parsedTemplateId)
                           ? GetAncestorOrSelfByTemplateId(item, parsedTemplateId)
                           : null;
            }

            /// <summary>
            /// Searches for the first item that uses or inherits from the given template ID, starting from the given item and recursing through its ancestors.
            /// </summary>
            /// <param name="item">The item to start the search from.</param>
            /// <param name="templateId">The ID of the template that the returned item must use or inherit.</param>
            /// <returns>The first matching item, if one can be found; otherwise, null.</returns>
            public static Item GetAncestorOrSelfByTemplateId(Item item, ID templateId)
            {
                if (item == null || templateId == ID.Null || templateId == ID.Undefined)
                {
                    return null;
                }

                var templateItem = item.Database.GetItem(templateId);
                return templateItem == null ? null : GetAncestorOrSelfByTemplateItem(item, templateItem);
            }

            /// <summary>
            /// Searches for the first item that uses or inherits from the given template item, starting from the given item and recursing through its ancestors.
            /// </summary>
            /// <param name="item">The item to start the search from.</param>
            /// <param name="templateItem">The template item that the returned item must use or inherit.</param>
            /// <returns>The first matching item, if one can be found; otherwise, null.</returns>
            public static Item GetAncestorOrSelfByTemplateItem(Item item, TemplateItem templateItem)
            {
                if (item == null || templateItem == null)
                {
                    return null;
                }

                if (item.Template == templateItem || IsTemplateAncestor(item.Template, templateItem))
                {
                    return item;
                }

                return GetAncestorOrSelfByTemplateItem(item.Parent, templateItem);
            }

            /// <summary>
            /// This method will search recursively up through the parent's of an item looking for another item
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="ancestor">Item</param>
            /// <returns>Bool</returns>
            public static Boolean IsAncestor(Item item, Item ancestor)
            {
                if (item == null || ancestor == null)
                {
                    return false;
                }

                if (item.ID == ancestor.ID) return true;

                return IsAncestor(item.Parent, ancestor);
            }

            /// <summary>
            /// This looks at each base template of an item and checks if any ancestors of those base templates matches the other passed in item
            /// </summary>
            /// <param name="item">TemplateItem</param>
            /// <param name="ancestor">TemplateItem</param>
            /// <returns>Bool</returns>
            public static Boolean IsTemplateAncestor(TemplateItem item, TemplateItem ancestor)
            {
                if (item == null || ancestor == null)
                {
                    return false;
                }

                if (item.ID == ancestor.ID) return true;

                foreach (TemplateItem basetemplate in item.BaseTemplates)
                {
                    if (IsTemplateAncestor(basetemplate, ancestor))
                        return true;
                    if (IsAncestor(basetemplate, ancestor))
                        return true;
                }

                return false;
            }

        }

        public class ItemRenderMethods
        {
            /// <summary>
            /// Gets the URL of an item to be used when linking through to an item
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>String</returns>
            public static string GetItemUrl(Item item)
            {
                if (item == null)
                {
                    return string.Empty;
                }

                if (!LanguageHelper.HasContextLanguage(item) || !LanguageHelper.IsValidForCountry(item))
                {
                    return string.Empty;
                }

                string redirectUrl;
                var hasRedirectUrl =
                    GetGeneralLinkURL(item.Fields["Redirect Url"], out redirectUrl) &&
                    !String.IsNullOrEmpty(redirectUrl);

                return hasRedirectUrl ? redirectUrl : LinkManager.GetItemUrl(item);
            }

            /// <summary>
            /// Gets the URL of an item to be used when linking through to an item.
            /// Overloads GetItemUrl(item)
            /// </summary>
            /// <param name="itemId">The item ID.</param>
            /// <returns>String</returns>
            public static string GetItemUrl(string itemId)
            {
                Item item = ItemMethods.GetItemFromGUID(itemId);
                if (item == null) return string.Empty;
                return GetItemUrl(item);
            }

            // This function will render hyperlink from a Sitecore item.  The text of the hyperlink is "Page Title" by default.
            //Field name to overwrite hyperlink text
            public static void RenderItemHyperLink(ref HyperLink hl, Item sitecoreItem, string fieldName, bool useRenderer, bool openNewWindow, string overrideURL)
            {
                if (!String.IsNullOrEmpty(fieldName))
                    hl.Text = GetRawValueByFieldName(fieldName, sitecoreItem, useRenderer);

                RenderLinkFieldHyperLink(hl, sitecoreItem, "Redirect Link", "");

                if (!String.IsNullOrEmpty(overrideURL))
                    hl.NavigateUrl = overrideURL;
                else if (String.IsNullOrEmpty(hl.NavigateUrl))
                    hl.NavigateUrl = GetItemUrl(sitecoreItem);

                if (openNewWindow)
                    hl.Target = "_blank";

                var isDisabledNavigationLink = GetCheckBoxValueByFieldName("Disabled Navigation Link", sitecoreItem, false);
                hl.Enabled = !isDisabledNavigationLink;
            }

            /// <summary>
            /// i. Will set hyperlink for you, passin referencee to the hyperlink control, the sitecore item, the name of the field, whether to useRenderer, whether to openNewWindow when clicked, and an overrideURL (like in the case of linking to a pdf instead o fa  news item directly).
            ///ii. This will first set the hyperlink text to the value of the field passed in, or the Headline if nothing is passed in.  If still not set, will try Title.
            ///iii. If overrideURL is set, will set NavigateURL to that, otherwise uses GetItemURL.  Will set Target to _blank if openNewWindow is true.
            /// </summary>
            /// <param name="hyperLink">HyperLink</param>
            /// <param name="item">Item</param>
            /// <param name="fieldName">string</param>
            /// <param name="defaultTextIfBlank">string</param>
            public static void RenderLinkFieldHyperLink(HyperLink hyperLink, Item item, string fieldName, string defaultTextIfBlank)
            {
                if (hyperLink == null || item == null || string.IsNullOrEmpty(fieldName))
                {
                    return;
                }

                var url = "";
                var linkText = "";
                var linkTarget = "";
                GetLinkFieldUrl(item, fieldName, out url, out linkText, out linkTarget);

                hyperLink.NavigateUrl = url;
                if (!string.IsNullOrEmpty(linkText))
                    hyperLink.Text = linkText;
                else if (!String.IsNullOrEmpty(defaultTextIfBlank))
                    hyperLink.Text = defaultTextIfBlank;
                hyperLink.Target = linkTarget;

            }

            /// <summary>
            /// Gets the link field URL.
            /// </summary>
            /// <param name="sitecoreItem">The sitecore item.</param>
            /// <param name="fieldName">Name of the field.</param>
            /// <param name="url">The URL.</param>
            /// <param name="linkText">The link text.</param>
            /// <param name="linkTarget">The link target.</param>
            public static void GetLinkFieldUrl(Item sitecoreItem, string fieldName, out string url, out string linkText, out string linkTarget)
            {
                url = "";
                linkText = "";
                linkTarget = "";
                var linkField = (Sitecore.Data.Fields.LinkField)sitecoreItem.Fields[fieldName];
                if (linkField != null)
                {
                    if (!GetGeneralLinkURL(linkField, out url))
                    {
                        return;
                    }
                    linkText = linkField.Text;
                    linkTarget = linkField.Target;
                    if (!string.IsNullOrEmpty(linkField.QueryString))
                        url += "?" + linkField.QueryString;
                    if (!string.IsNullOrEmpty(linkField.Anchor))
                        url += "#" + linkField.Anchor;
                }
            }

            public static bool HasLinkFieldUrl(Item sitecoreItem, string fieldName)
            {
                if (sitecoreItem == null || string.IsNullOrWhiteSpace(fieldName))
                    return false;
                string url;
                string linkText;
                string linkTarget;
                GetLinkFieldUrl(sitecoreItem, fieldName, out url, out linkText, out linkTarget);
                return !string.IsNullOrWhiteSpace(url);
            }

            /// <summary>
            /// Tries to get the URL for the link field on the item.
            /// </summary>
            /// <param name="item">The item containing the link field.</param>
            /// <param name="fieldName">The name of the link field on the item.</param>
            /// <param name="url">The URL contained in the link field.</param>
            /// <returns>True if able to return a URL; otherwise, false.</returns>
            public static bool GetGeneralLinkURL(Item item, string fieldName, out string url)
            {
                url = string.Empty;

                try
                {
                    if (item == null || string.IsNullOrEmpty(fieldName))
                    {
                        return false;
                    }

                    var rawLinkField = item.Fields[fieldName];
                    var linkFieldIsValidAndHasContent = rawLinkField != null &&
                                                        !string.IsNullOrEmpty(rawLinkField.Value) &&
                                                        FieldTypeManager.GetField(rawLinkField) is LinkField;
                    if (!linkFieldIsValidAndHasContent)
                    {
                        return false;
                    }

                    LinkField linkField = rawLinkField;
                    return GetGeneralLinkURL(linkField, out url);
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Tries to get the URL for a LinkField.
            /// </summary>
            /// <param name="linkField">The LinkField to retrieve the URL from.</param>
            /// <param name="url">The URL for the LinkField.</param>
            /// <returns>True if able to return a URL, otherwise false.</returns>
            public static Boolean GetGeneralLinkURL(LinkField linkField, out string url)
            {
                url = string.Empty;

                try
                {
                    if (linkField == null)
                        return false;

                    return linkField.LinkType == "internal"
                               ? TryGetUrlForInternalLinkField(linkField, out url)
                               : TryGetUrlForNonInternalLinkField(linkField, out url);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// Tries to get the URL for a LinkField whose type is "internal", handling item redirection.
            /// </summary>
            /// <param name="linkField">The LinkField to get the URL from.</param>
            /// <param name="url">The URL for the LinkField.</param>
            /// <returns>True if able to return a URL, otherwise false.</returns>
            private static bool TryGetUrlForInternalLinkField(LinkField linkField, out string url)
            {
                url = string.Empty;

                try
                {
                    if (linkField == null || linkField.TargetItem == null)
                    {
                        return false;
                    }

                    // Create a stack of internal link fields that will be used for processing redirects.
                    var internalLinkFieldStack = new Stack<LinkField>();
                    internalLinkFieldStack.Push(linkField);
                    while (internalLinkFieldStack.Count < 3)
                    {
                        var thisInternalLinkField = internalLinkFieldStack.Peek();
                        var thisItem = thisInternalLinkField.TargetItem;
                        var rawRedirectLinkField = thisItem.Fields["Redirect Url"];
                        var hasValidAndFilledRedirectLinkField = rawRedirectLinkField != null &&
                                                                 !String.IsNullOrEmpty(rawRedirectLinkField.Value) &&
                                                                 FieldTypeManager.GetField(rawRedirectLinkField) is
                                                                 LinkField;
                        if (hasValidAndFilledRedirectLinkField)
                        {
                            LinkField redirectLinkField = rawRedirectLinkField;
                            if (redirectLinkField.LinkType == "internal")
                            {
                                var redirectItem = redirectLinkField.TargetItem;
                                if (internalLinkFieldStack.Any(x => x.TargetItem.ID == redirectItem.ID))
                                {
                                    // We've hit an endless loop of redirection!
                                    Log.Error(
                                        "SitecoreHelper.ItemRenderMethods.TryGetUrlForInternalLinkField: The following item(s) are part of an endless loop of redirection: " +
                                        String.Join(", ",
                                                    internalLinkFieldStack.Select(x => x.TargetItem.ID.ToString()).Reverse()
                                                        .ToArray()), typeof(ItemRenderMethods));
                                    return false;
                                }
                                else
                                {
                                    // Add the redirectLinkField to the internalLinkFieldStack and continue processing redirects.
                                    internalLinkFieldStack.Push(redirectLinkField);
                                }
                            }
                            else
                            {
                                // The redirectLinkField is a non-internal type, so return the URL for the redirectLinkField.
                                return TryGetUrlForNonInternalLinkField(redirectLinkField, out url);
                            }
                        }
                        else
                        {
                            // No more redirection, so return this item.
                            url = LinkManager.GetItemUrl(thisItem);
                            if (!string.IsNullOrEmpty(thisInternalLinkField.QueryString))
                                url += "?" + thisInternalLinkField.QueryString;
                            if (!string.IsNullOrEmpty(thisInternalLinkField.Anchor))
                                url += "#" + thisInternalLinkField.Anchor;
                            return true;
                        }
                    }

                    // We've hit the maximum number of redirects and will not attempt to process anymore.
                    Log.Error(
                        "SitecoreHelper.ItemRenderMethods.TryGetUrlForInternalLinkField: Hit the maximum number of redirects while processing the following chain of items: " +
                        String.Join(", ",
                                    internalLinkFieldStack.Select(x => x.TargetItem.ID.ToString()).Reverse().ToArray()),
                        typeof(ItemRenderMethods));
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// Tries to get the URL for a LinkField whose type is not "internal".
            /// </summary>
            /// <param name="linkField">The LinkField to get the URL from.</param>
            /// <param name="url">The URL for the LinkField.</param>
            /// <returns>True if able to return a URL, otherwise false.</returns>
            private static bool TryGetUrlForNonInternalLinkField(LinkField linkField, out string url)
            {
                url = string.Empty;

                try
                {
                    if (linkField == null)
                    {
                        return false;
                    }

                    switch (linkField.LinkType)
                    {
                        case "external":
                        case "mailto":
                        case "javascript":
                            url = linkField.Url;
                            return true;
                        case "anchor":
                            url = "#" + linkField.Anchor;
                            return true;
                        case "media":
                            if (linkField.TargetItem == null)
                                return false;
                            var mediaItem = new MediaItem(linkField.TargetItem);
                            return GetMediaURL(mediaItem, out url);
                        case "":
                            return true;
                        default:
                            return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// Tries to get the URL for the MediaItem referenced in the given file field on the given item.
            /// </summary>
            /// <param name="fieldName">The name of the field that references the MediaItem. Note, this field name must reference a FileField.</param>
            /// <param name="item">The item that contains the given field.</param>
            /// <param name="url">The URL for the MediaItem.</param>
            /// <returns>True if a URL could be returned for the MediaItem; otherwise, false.</returns>
            public static Boolean GetMediaFileFriendlyURL(string fieldName, Item item, out String url)
            {
                url = "";
                if (string.IsNullOrEmpty(fieldName) || item == null)
                {
                    return false;
                }

                var rawField = item.Fields[fieldName];
                var fieldIsValidAndHasContent = rawField != null && !string.IsNullOrEmpty(rawField.Value) &&
                                                FieldTypeManager.GetField(rawField) is FileField;
                if (!fieldIsValidAndHasContent)
                {
                    return false;
                }

                FileField fileField = rawField;
                if (fileField.MediaItem == null)
                {
                    return false;
                }

                return GetMediaURL(fileField.MediaItem, out url);
            }

            /// <summary>
            /// Tries to get the URL for the MediaItem referenced in the given image field on the given item.
            /// </summary>
            /// <param name="fieldName">The name of the field that references the MediaItem. Note, this field name must reference an ImageField.</param>
            /// <param name="item">The item that contains the given field.</param>
            /// <param name="url">The URL for the MediaItem.</param>
            /// <returns>True if a URL could be returned for the MediaItem; otherwise, false.</returns>
            public static Boolean GetMediaImageFriendlyURL(string fieldName, Item item, out String url)
            {
                url = "";

                if (string.IsNullOrEmpty(fieldName) || item == null)
                {
                    return false;
                }

                var rawField = item.Fields[fieldName];
                var fieldIsValidAndHasContent = rawField != null && !string.IsNullOrEmpty(rawField.Value) &&
                                                FieldTypeManager.GetField(rawField) is Sitecore.Data.Fields.ImageField;
                if (!fieldIsValidAndHasContent)
                {
                    return false;
                }

                Sitecore.Data.Fields.ImageField imageField = rawField;
                if (imageField.MediaItem == null)
                {
                    return false;
                }

                return GetMediaURL(imageField.MediaItem, out url);
            }

            /// <summary>
            /// Tries to get the URL for the given MediaItem.
            /// </summary>
            /// <param name="mediaItem">The MediaItem to get the URL for.</param>
            /// <param name="url">The URL for the MediaItem.</param>
            /// <returns>True if a URL could be returned for the MediaItem; otherwise, false.</returns>
            public static Boolean GetMediaURL(MediaItem mediaItem, out String url)
            {
                url = string.Empty;

                if (mediaItem == null)
                {
                    return false;
                }
                else
                {
                    url = Sitecore.StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(mediaItem, new MediaUrlOptions { IncludeExtension = true }));

                    // removing the logic from here that potentially embeds the language
                    // instead, this is now set in a CustomMediaProvider GetMediaUrl override method
                    // this media provider should be set in a config file to be used as the media provider instead of the default sitecore one

                    //// check if language should be embedded, and if so, embed the context language
                    //UrlOptions urlOptions = UrlOptions.DefaultOptions;
                    //urlOptions = LanguageHelper.CheckOverrideLanguageEmbedding(urlOptions);
                    //if (urlOptions.LanguageEmbedding == LanguageEmbedding.Always)
                    //    url = "/" + Sitecore.Context.Language.Name + url;

                    return true;
                }
            }

            /// <summary>
            /// Takes in a fieldname and an item and checks the Size field, returning the value as a megabyte value
            /// </summary>
            /// <param name="fieldName">name of the field of the media item</param>
            /// <param name="item">the item that contains the field</param>
            /// <returns>returns the size value as a string in megabytes/gigabytes/kilobytes/bytes, appended with 'mb/gb/kb/b'</returns>
            public static string GetMediaSize(string fieldName, Item item)
            {
                if (item == null || string.IsNullOrEmpty(fieldName))
                {
                    return string.Empty;
                }

                Sitecore.Data.Fields.ImageField image = item.Fields[fieldName];
                if (image == null || image.MediaItem == null)
                {
                    return string.Empty;
                }

                return GetMediaSize(image.MediaItem);
            }

            /// <summary>
            /// Takes in a media file item, takes the size in bytes from the 'Size' field, returning the value as a megabyte value
            /// </summary>
            /// <param name="mediaItem">the media item - from the media library</param>
            /// <returns>returns the size value as a string in megabytes/gigabytes/kilobytes/bytes, appended with 'mb/gb/kb/b'</returns>
            public static string GetMediaSize(Item mediaItem)
            {
                var rawSizeInBytes = GetRawValueByFieldName("Size", mediaItem, false);
                decimal sizeInBytes;
                if (string.IsNullOrEmpty(rawSizeInBytes) || !decimal.TryParse(rawSizeInBytes, out sizeInBytes))
                {
                    return string.Empty;
                }

                const decimal bytesInKilobyte = 1024;
                const decimal bytesInMegabyte = bytesInKilobyte * 1024;
                const decimal bytesInGigabyte = bytesInMegabyte * 1024;
                if (sizeInBytes >= bytesInGigabyte)
                {
                    return string.Format("{0:#.#}gb", sizeInBytes / bytesInGigabyte);
                }

                if (sizeInBytes >= bytesInMegabyte)
                {
                    return string.Format("{0:#.#}mb", sizeInBytes / bytesInMegabyte);
                }

                if (sizeInBytes >= bytesInKilobyte)
                {
                    return string.Format("{0:#.#}kb", sizeInBytes / bytesInKilobyte);
                }

                return string.Format(string.Format("{0}b", sizeInBytes));
            }

            /// <summary>
            /// Will return the Path to the datasource item of the sublayout.  Can then use GetItemByPath to get the item to display the details of the source.
            /// </summary>
            /// <param name="thisSublayout">Sublayout</param>
            /// <returns>string</returns>
            //string dataSource = SitecoreHelper.GetSublayoutDataSource(Parent as Sublayout);
            //Item calloutItem = SitecoreHelper.GetItemByPath(dataSource);
            public static string GetSublayoutDataSource(Sublayout thisSublayout)
            {
                string ds = "";
                if (thisSublayout != null)
                {
                    ds = thisSublayout.DataSource;
                }
                return ds;
            }

            /// <summary>
            /// Will return the value of a parameter set in a sublayout in the presentation of an item
            /// </summary>
            /// <param name="key">parameter key</param>
            /// <param name="thisSublayout">Sublayout</param>
            /// <returns>the parameter value set in the presentation</returns>
            public static string GetSublayoutParameter(string key, Sublayout thisSublayout)
            {
                string value = String.Empty;

                string rawParameters = thisSublayout.Parameters; // Attributes["sc_parameters"]
                var parameters = Sitecore.Web.WebUtil.ParseUrlParameters(rawParameters);
                if (parameters[key] != null)
                {
                    value = parameters[key];
                }

                return value;
            }

            /// <summary>
            /// Will return a DateTime value for an string formatted as an ISODate (which is how inherent date fields are stored in sitecore items, eg, created timestamp)
            /// </summary>
            /// <param name="dateStr">string</param>
            /// <returns>DateTime</returns>
            public static DateTime GetDateFromSitecoreIsoDate(string dateStr)
            {
                try
                {
                    var thisDate = Sitecore.DateUtil.IsoDateToDateTime(dateStr);
                    return thisDate;
                }
                catch
                {
                    return new DateTime();
                }

            }

            /// <summary>
            /// Tries to get the date and time from the given item and field name.
            /// </summary>
            /// <param name="item">The item to get the date and time from.</param>
            /// <param name="fieldName">The name of the field containing the date and time.</param>
            /// <param name="dateTime">The date and time stored in the field.</param>
            /// <returns>True if the date and time could be retrieved from the field; otherwise, false.</returns>
            public static bool TryGetDateTimeByFieldName(Item item, string fieldName, out DateTime dateTime)
            {
                dateTime = DateTime.MinValue;

                try
                {
                    var rawDateField = item.Fields[fieldName];
                    var dateFieldIsValidAndHasContent = rawDateField != null &&
                                                        !string.IsNullOrEmpty(rawDateField.Value) &&
                                                        FieldTypeManager.GetField(rawDateField) is DateField;
                    if (!dateFieldIsValidAndHasContent)
                    {
                        return false;
                    }

                    DateField dateField = rawDateField;
                    dateTime = dateField.DateTime.Date;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Pass in an item and a field and will use the GUID stored in that field (assuming it is a field that relates to another item) to get the related or referenced item
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="field">String</param>
            /// <returns>Item , Null Value Exception</returns>
            public static Item GetReferenceField(Item item, String field)
            {
                if (item == null || field.Length == 0)
                    return null;

                ReferenceField refField;

                try
                {
                    refField = item.Fields[field];
                    String test = item.Fields[field].Value;
                    if (test.Length > 0)
                    {

                    }
                }
                catch
                {
                    return null;
                }

                if (refField != null)
                {
                    if (refField.TargetItem != null)
                    {
                        return refField.TargetItem;
                    }
                }


                return null;
            }

            /// <summary>
            /// Pass in an item Id and a field and will use the GUID stored in that field (assuming it is a field that relates to another item) to get the related or referenced item
            /// </summary>
            /// <see cref="GetReferenceField(Item, String)"/>
            /// <param name="item">Item</param>
            /// <param name="field">String</param>
            /// <returns>Item , Null Value Exception</returns>

            public static Item GetReferenceField(string itemId, string field)
            {
                var item = ItemMethods.GetItemFromGUID(itemId);
                return item == null ? null : GetReferenceField(item, field);
            }

            /// <summary>
            /// Use this method for getting any needed values from an item.  Pass in the field name, item, and whether to use FieldRenderer (useRenderer) when getting the value
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="itemPath">string</param>
            /// <param name="useRenderer">bool</param>
            /// <returns>string</returns>
            public static string GetRawValueByFieldName(string fieldName, string itemPath, bool useRenderer)
            {
                Item item = Sitecore.Context.Database.GetItem(itemPath);

                return GetRawValueByFieldName(fieldName, item, useRenderer);
            }

            /// <summary>
            /// Use this method for getting any needed values from an item.  
            /// Pass in the field name, item, and whether to use FieldRenderer (useRenderer) when getting the value
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="item">Item</param>
            /// <param name="useRenderer">bool</param>
            /// <returns>string</returns>
            public static string GetRawValueByFieldName(string fieldName, Item item, bool useRenderer)
            {
                string fieldValue = string.Empty;
                if (item != null)
                {
                    try
                    {
                        if (useRenderer)
                            fieldValue = FieldRenderer.Render(item, fieldName);
                        else
                            fieldValue = item.Fields[fieldName].Value;
                    }
                    catch
                    {

                    }
                }
                return fieldValue;
            }

            /// <summary>
            /// By default the return value is false, can optionally pass in what the default value should be.  
            /// This tries to cast the passed in field as a checkbox field for the item and return whether it is checked or not.
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="item">Item</param>
            /// <returns>bool</returns>
            public static bool GetCheckBoxValueByFieldName(string fieldName, Item item)
            {
                return GetCheckBoxValueByFieldName(fieldName, item, false);
            }

            /// <summary>
            /// By default the return value is false, can optionally pass in what the default value should be.  
            /// This tries to cast the passed in field as a checkbox field for the item and return whether it is checked or not.
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="item">Item</param>
            /// <param name="defaultValue">bool</param>
            /// <returns>bool</returns>
            public static bool GetCheckBoxValueByFieldName(string fieldName, Item item, bool defaultValue)
            {
                bool isChecked = defaultValue;

                if (item != null)
                {
                    try
                    {
                        var chkField = (CheckboxField)item.Fields[fieldName];
                        isChecked = chkField.Checked;
                    }
                    catch
                    {

                    }
                }

                return isChecked;
            }

            /// <summary>
            /// For the passed in item, tries to catch the field for the passed in string to a MultilistField and then uses GetItems to return the list of related items
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="item">Item</param>
            /// <returns>Generics List of Items</returns>
            public static List<Item> GetMultilistValueByFieldName(string fieldName, Item item)
            {
                List<Item> relatedItems = null;

                if (item != null)
                {
                    try
                    {
                        var relatedField = (MultilistField)item.Fields[fieldName];
                        relatedItems = relatedField.GetItems().ToList();
                        relatedItems = LanguageHelper.RemoveItemsWithoutLanguageVersion(relatedItems);
                    }
                    catch
                    {

                    }
                }

                return relatedItems;
            }

            /// <summary>
            /// Checks if an item field has content.
            /// Returns false if the field is null, 
            /// empty or contains only white space.
            /// </summary>
            /// <param name="item">The item to check.</param>
            /// <param name="field">The fieldname of the field.</param>
            /// <returns></returns>
            public static bool FieldHasContent(Item item, string field)
            {
                var fieldContent = SitecoreHelper.ItemRenderMethods.GetRawValueByFieldName(field, item, useRenderer: false);
                return !string.IsNullOrWhiteSpace(fieldContent);
            }

        }

    }
}
