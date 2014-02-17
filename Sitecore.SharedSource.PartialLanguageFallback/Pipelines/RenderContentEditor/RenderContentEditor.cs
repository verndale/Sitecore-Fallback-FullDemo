using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Resources;
using Sitecore.SharedSource.PartialLanguageFallback.Extensions;
using Sitecore.Shell;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.GetContentEditorSkin;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Xml;

namespace Sitecore.SharedSource.PartialLanguageFallback.Pipelines.RenderContentEditor
{
    public class CustomRenderSkinedContentEditor
    {
        // Fields
        private RenderContentEditorArgs _args;
        private Editor.Field _currentField;
        private Editor.Section _currentSection;
        private Item _item;
        private Editor.Sections _sections;
        private HtmlTextWriter _text;
        // Methods
        private void AddText(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            this.Text.WriteBeginTag(element.LocalName);
            if (element.Attributes != null)
            {
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    this.Text.WriteAttribute(attribute.LocalName, attribute.Value);
                }
            }
            if (element.HasChildNodes)
            {
                this.Text.Write('>');
                foreach (XmlNode node in element.ChildNodes)
                {
                    this.RenderElement(node);
                }
                this.Text.WriteEndTag(element.LocalName);
            }
            else
            {
                this.Text.Write(" />");
            }
        }

        public void Process(RenderContentEditorArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.Item != null)
            {
                GetContentEditorSkinArgs args2 = new GetContentEditorSkinArgs(args.Item, args.Sections);
                using (new LongRunningOperationWatcher(Settings.Profiling.RenderFieldThreshold, "GetContentEditorSkin pipeline", new string[0]))
                {
                    CorePipeline.Run("getContentEditorSkin", args2);
                }

                string skin = args2.Skin;
                if (!string.IsNullOrEmpty(skin))
                {
                    XmlDocument document = XmlUtil.LoadXml(skin);
                    if (document.DocumentElement != null)
                    {
                        this._args = args;
                        this._item = args.Item;
                        this._sections = args.Sections;
                        this.Render(document);
                        args.AbortPipeline();
                    }
                }
            }
        }

        public void Render(XmlDocument skin)
        {
            Assert.ArgumentNotNull(skin, "skin");
            this._text = new HtmlTextWriter(new StringWriter());
            this.RenderElement(skin.DocumentElement);
            this.RenderText();
        }

        private void RenderButtons(Editor.Field field)
        {
            Assert.ArgumentNotNull(field, "field");
            bool readOnly = this.Args.ReadOnly;
            Field itemField = field.ItemField;
            Item fieldType = this.Args.EditorFormatter.GetFieldType(itemField);
            if (fieldType != null)
            {
                if (!itemField.CanWrite)
                {
                    readOnly = true;
                }
                if (!UserOptions.View.UseSmartTags)
                {
                    this.Args.EditorFormatter.RenderMenuButtons(this.Args.Parent, field, fieldType, readOnly);
                }
            }
        }

        private void RenderButtons(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            string attribute = XmlUtil.GetAttribute("id", element);
            if (!string.IsNullOrEmpty(attribute))
            {
                Editor.Field fieldByID = this.Sections.GetFieldByID(attribute);
                if (fieldByID != null)
                {
                    this.RenderButtons(fieldByID);
                }
            }
        }

        private void RenderChildElements(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            foreach (XmlNode node in element.ChildNodes)
            {
                this.RenderElement(node);
            }
        }

        private void RenderElement(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            if (element.NodeType != XmlNodeType.Comment)
            {
                if (element.NodeType == XmlNodeType.Text)
                {
                    this.Text.Write(element.Value);
                }
                else if (element.NodeType == XmlNodeType.CDATA)
                {
                    this.Text.Write(element.Value);
                }
                else
                {
                    if (element.NamespaceURI == "http://www.sitecore.net/skin")
                    {
                        this.RenderText();
                        switch (element.LocalName)
                        {
                            case "label":
                                this.RenderLabel(element);
                                return;

                            case "input":
                                this.RenderInput(element);
                                return;

                            case "fields":
                                this.RenderFields();
                                return;

                            case "section":
                                this.RenderSection(element);
                                return;

                            case "sectionpanel":
                                this.RenderSectionPanel(element);
                                return;

                            case "sections":
                                this.RenderSections();
                                return;

                            case "buttons":
                                this.RenderButtons(element);
                                return;

                            case "marker":
                                this.RenderMarker(element);
                                return;
                        }
                        throw new Exception("Unknown element: " + element.Name);
                    }
                    this.AddText(element);
                }
            }
        }

        private void RenderFields()
        {
            if (this._currentSection != null)
            {
                this.RenderFields(this._currentSection);
            }
            else
            {
                this.RenderSections();
            }
        }

        private void RenderFields(Editor.Section section)
        {
            Assert.ArgumentNotNull(section, "section");
            foreach (Editor.Field field in section.Fields)
            {
                this.RenderButtons(field);
                this.RenderLabel(field);
                this.RenderInput(field);
            }
        }

        private void RenderInput(Editor.Field field)
        {
            Assert.ArgumentNotNull(field, "field");
            bool readOnly = this.Args.ReadOnly;
            Field itemField = field.ItemField;
            Item fieldType = this.Args.EditorFormatter.GetFieldType(itemField);
            if (fieldType != null)
            {
                if (!itemField.CanWrite)
                {
                    readOnly = true;
                }
                this.Args.EditorFormatter.RenderField(this.Args.Parent, field, fieldType, readOnly);
            }
        }

        private void RenderInput(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            string attribute = XmlUtil.GetAttribute("section", element);
            if (!string.IsNullOrEmpty(attribute))
            {
                Editor.Section sectionByName = this.Sections.GetSectionByName(attribute);
                if (sectionByName != null)
                {
                    this.RenderFields(sectionByName);
                }
            }
            else
            {
                string str2 = XmlUtil.GetAttribute("id", element);
                if (!string.IsNullOrEmpty(str2))
                {
                    Editor.Field fieldByID = this.Sections.GetFieldByID(str2);
                    if (fieldByID != null)
                    {
                        this.RenderInput(fieldByID);
                    }
                }
            }
        }

        private void RenderLabel(Editor.Field field)
        {
            Assert.ArgumentNotNull(field, "field");
            Field itemField = field.ItemField;
            Item fieldType = Args.EditorFormatter.GetFieldType(itemField);
            if (fieldType != null)
            {
                bool readOnly = Args.ReadOnly;
                if (!itemField.CanWrite)
                {
                    readOnly = true;
                }
                RenderLabel(Args.Parent, field, fieldType, readOnly);
            }
        }

        public void RenderLabel(Control parent, Editor.Field field, Item fieldType, bool readOnly)
        {
            Assert.ArgumentNotNull(parent, "parent");
            Assert.ArgumentNotNull(field, "field");
            Assert.ArgumentNotNull(fieldType, "fieldType");

            Field itemField = Args.Item.Fields[field.ItemField.ID];

            Language language = Args.Item.Language;

            Assert.IsNotNull(language, "language");
            if (itemField.Language != language)
            {
                Item item = ItemManager.GetItem(field.ItemField.Item.ID, language, Sitecore.Data.Version.Latest, field.ItemField.Item.Database);
                if (item != null)
                {
                    itemField = item.Fields[itemField.ID];
                }
            }
            string name = itemField.Name;
            if (!string.IsNullOrEmpty(itemField.DisplayName))
            {
                name = itemField.DisplayName;
            }
            name = Translate.Text(name);
            string toolTip = itemField.ToolTip;
            if (!string.IsNullOrEmpty(toolTip))
            {
                toolTip = Translate.Text(toolTip);
                if (toolTip.EndsWith("."))
                {
                    toolTip = StringUtil.Left(toolTip, toolTip.Length - 1);
                }
                name = name + " - " + toolTip;
            }
            name = HttpUtility.HtmlEncode(name);
            bool flag = false;
            StringBuilder builder = new StringBuilder(200);
            if (Args.IsAdministrator && (itemField.Unversioned || itemField.Shared))
            {
                builder.Append("<span class=\"scEditorFieldLabelAdministrator\"> [");
                if (itemField.Unversioned)
                {
                    builder.Append(Translate.Text("unversioned"));
                    flag = true;
                }
                if (itemField.Shared)
                {
                    if (flag)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(Translate.Text("shared"));
                    flag = true;
                }
            }



            if (itemField.InheritsValueFromOtherItem)
            {
                if (flag)
                {
                    builder.Append(", ");
                }
                else
                {
                    builder.Append("<span class=\"scEditorFieldLabelAdministrator\"> [");
                }
                builder.Append(Translate.Text("original value"));
                flag = true;
            }
            if (itemField.ContainsFallbackValue(Args.Item.Language))
            {
                if (flag)
                {
                    builder.Append(", ");
                }
                else
                {
                    builder.Append("<span class=\"scEditorFieldLabelAdministrator\"> [");
                }
                builder.Append(Translate.Text("fallback value"));
            }
            else if (itemField.ContainsStandardValue)
            {
                if (flag)
                {
                    builder.Append(", ");
                }
                else
                {
                    builder.Append("<span class=\"scEditorFieldLabelAdministrator\"> [");
                }
                builder.Append(Translate.Text("standard value"));
            }

            if (builder.Length > 0)
            {
                builder.Append("]</span>");
            }
            name = name + builder.ToString() + ":";
            if (readOnly)
            {
                name = "<span class=\"scEditorFieldLabelDisabled\">" + name + "</span>";
            }
            string helpLink = itemField.HelpLink;
            if (helpLink.Length > 0)
            {
                name = "<a class=\"scEditorFieldLabelLink\" href=\"" + helpLink + "\" target=\"__help\">" + name + "</a>";
            }
            string str4 = string.Empty;
            if (itemField.Description.Length > 0)
            {
                str4 = " title=\"" + itemField.Description + "\"";
            }
            string str5 = "scEditorFieldLabel";
            if ((UserOptions.View.UseSmartTags && !readOnly) && !UserOptions.ContentEditor.ShowRawValues)
            {
                Item item2 = fieldType.Children["Menu"];
                if (item2 != null)
                {
                    ChildList children = item2.Children;
                    int count = children.Count;
                    if (count > 0)
                    {
                        string text = children[0]["Display Name"];
                        name = (string.Concat(new object[] { "<span id=\"SmartTag_", field.ControlID, "\" onmouseover='javascript:scContent.smartTag(this, event, \"", field.ControlID, "\", ", StringUtil.EscapeJavascriptString(text), ",\"", count, "\")'>" }) + Images.GetImage("Images/SmartTag.png", 11, 11, "middle", "0px 4px 0px 0px")) + "</span>" + name;
                    }
                }
            }
            name = "<div class=\"" + str5 + "\"" + str4 + ">" + name + "</div>";
            this.AddLiteralControl(parent, name);
        }

        public void AddLiteralControl(Control parent, string text)
        {
            Assert.ArgumentNotNull(parent, "parent");
            Assert.ArgumentNotNull(text, "text");
            if (parent.Controls.Count > 0)
            {
                var control = parent.Controls[parent.Controls.Count - 1];
                var control2 = control as LiteralControl;
                if (control2 != null)
                {
                    control2.Text = control2.Text + text;
                    return;
                }
            }
            Context.ClientPage.AddControl(parent, new LiteralControl(text));
        }

        private void RenderLabel(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            string attribute = XmlUtil.GetAttribute("id", element);
            if (!string.IsNullOrEmpty(attribute))
            {
                Editor.Field fieldByID = this.Sections.GetFieldByID(attribute);
                if (fieldByID != null)
                {
                    this.RenderLabel(fieldByID);
                }
            }
        }

        private void RenderMarker(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            string attribute = XmlUtil.GetAttribute("id", element);
            if (!string.IsNullOrEmpty(attribute))
            {
                Editor.Field fieldByID = this.Sections.GetFieldByID(attribute);
                if (fieldByID != null)
                {
                    Editor.Field field2 = this._currentField;
                    this._currentField = fieldByID;
                    if (!UserOptions.View.UseSmartTags)
                    {
                        this.Args.EditorFormatter.RenderMarkerBegin(this.Args.Parent, fieldByID.ControlID);
                    }
                    this.RenderChildElements(element);
                    if (!UserOptions.View.UseSmartTags)
                    {
                        this.Args.EditorFormatter.RenderMarkerEnd(this.Args.Parent);
                    }
                    else
                    {
                        this.Args.EditorFormatter.AddLiteralControl(this.Args.Parent, "<div class=\"scEditorFieldSpacer\">" + Images.GetSpacer(1, 1) + "</div>");
                    }
                    this._currentField = field2;
                }
            }
        }

        private void RenderSection(Editor.Section section)
        {
            Assert.ArgumentNotNull(section, "section");
            Editor.Section section2 = this._currentSection;
            this._currentSection = section;
            bool renderCollapsedSections = UserOptions.ContentEditor.RenderCollapsedSections;
            this.Args.EditorFormatter.RenderSectionBegin(this.Args.Parent, section.ControlID, section.Name, section.DisplayName, section.Icon, section.IsSectionCollapsed, renderCollapsedSections);
            if (renderCollapsedSections)
            {
                this.RenderFields(section);
            }
            this.Args.EditorFormatter.RenderSectionEnd(this.Args.Parent, renderCollapsedSections, section.IsSectionCollapsed);
            this._currentSection = section2;
        }

        private void RenderSection(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            string attribute = XmlUtil.GetAttribute("name", element);
            if (!string.IsNullOrEmpty(attribute))
            {
                Editor.Section sectionByName = this.Sections.GetSectionByName(attribute);
                if (sectionByName != null)
                {
                    Editor.Section section2 = this._currentSection;
                    this._currentSection = sectionByName;
                    bool renderCollapsedSections = UserOptions.ContentEditor.RenderCollapsedSections;
                    this.Args.EditorFormatter.RenderSectionBegin(this.Args.Parent, sectionByName.ControlID, sectionByName.Name, sectionByName.DisplayName, sectionByName.Icon, sectionByName.IsSectionCollapsed, renderCollapsedSections);
                    this.RenderChildElements(element);
                    this.Args.EditorFormatter.RenderSectionEnd(this.Args.Parent, renderCollapsedSections, sectionByName.IsSectionCollapsed);
                    this._currentSection = section2;
                }
            }
        }

        private void RenderSectionPanel(XmlNode element)
        {
            Assert.ArgumentNotNull(element, "element");
            string attribute = XmlUtil.GetAttribute("name", element);
            if (!string.IsNullOrEmpty(attribute))
            {
                Editor.Section sectionByName = this.Sections.GetSectionByName(attribute);
                if (sectionByName != null)
                {
                    Editor.Section section2 = this._currentSection;
                    this._currentSection = sectionByName;
                    bool renderCollapsedSections = UserOptions.ContentEditor.RenderCollapsedSections;
                    this.Args.EditorFormatter.RenderSectionBegin(this.Args.Parent, sectionByName.ControlID, sectionByName.Name, sectionByName.DisplayName, sectionByName.Icon, sectionByName.IsSectionCollapsed, renderCollapsedSections);
                    this.RenderChildElements(element);
                    this.Args.EditorFormatter.RenderSectionEnd(this.Args.Parent, renderCollapsedSections, sectionByName.IsSectionCollapsed);
                    this._currentSection = section2;
                }
            }
        }

        private void RenderSections()
        {
            foreach (Editor.Section section in this.Sections)
            {
                this.RenderSection(section);
            }
        }

        private void RenderText()
        {
            string str = this._text.InnerWriter.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                this.Args.Parent.Controls.Add(new LiteralControl(str));
                StringWriter innerWriter = this.Text.InnerWriter as StringWriter;
                Assert.IsNotNull(innerWriter, "Internal error");
                innerWriter.GetStringBuilder().Length = 0;
            }
        }

        // Properties
        public RenderContentEditorArgs Args
        {
            get
            {
                return this._args;
            }
        }

        public Editor.Section CurrentSection
        {
            get
            {
                return this._currentSection;
            }
            set
            {
                this._currentSection = value;
            }
        }

        public Item Item
        {
            get
            {
                return this._item;
            }
        }

        public Editor.Sections Sections
        {
            get
            {
                return this._sections;
            }
        }

        public HtmlTextWriter Text
        {
            get
            {
                return this._text;
            }
        }
    }
}