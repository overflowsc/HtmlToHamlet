using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace HtmlToHamlet
{
    public class HamletWriter : XmlTextWriter
    {
        private static char[] assetsFolderSeparators = { '\\', '/' };
        private static string[] excludedTags = { "head", "header", "footer" };
        private static string[] contentOnlyTags = { "html", "body", "footer" };

        private int indentCount;
        private string currentElement;
        private string currentAttribute;
        private string imageFolderName;
        private string cssFolderName;
        private string javascriptFolderName;

        public HamletWriter(string filename, string imageFolderName, string cssFolderName, string javascriptFolderName)
            : base(filename, Encoding.UTF8)
        {
            Indentation = 0;
            Formatting = Formatting.None;
            currentElement = "";
            currentAttribute = "";
            indentCount = 0;
            this.imageFolderName = imageFolderName;
            this.cssFolderName = cssFolderName;
            this.javascriptFolderName = javascriptFolderName;
        }

        private string ExtractFileName(string filePath)
        {
            string[] srcPath = filePath.Split(assetsFolderSeparators);
            if (srcPath.Length == 1)
            {
                return srcPath.Last();
            }

            string fileName = srcPath.Last();
            if (fileName.Length == 0)
            {
                fileName = srcPath[srcPath.Length - 2];
            }
            return fileName;
        }

        private string ReplaceNonAlphanumeric(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            return rgx.Replace(str, "_");
        }

        private string BuildImageSrc(string currentSrc)
        {
            string fileName = ExtractFileName(currentSrc);
            fileName = imageFolderName + '_' + fileName;
            return "@{StaticR " + ReplaceNonAlphanumeric(fileName) + "}";
        }

        private string BuildCssSrc(string currentSrc)
        {
            string fileName = ExtractFileName(currentSrc);
            fileName = cssFolderName + '_' + fileName;
            return "@{StaticR " + ReplaceNonAlphanumeric(fileName) + "}";
        }

        private string BuildJavascriptSrc(string currentSrc)
        {
            string fileName = ExtractFileName(currentSrc);
            fileName = javascriptFolderName + '_' + fileName;
            return "@{StaticR " + ReplaceNonAlphanumeric(fileName) + "}";
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            // Do not remove, must not output the xml processing instruction
        }

        public override void WriteEndElement()
        {
            indentCount--;
            currentElement = "";
        }

        public override void WriteComment(string text)
        {
            // Hack, if the was a doctype declaration, the html parser did not read it correctly and have a comment node with that contant in it.
            // Just ignore it
            if (!text.Contains("CTYPE ht"))
            {
                base.WriteComment(text);
            }
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            currentElement = localName;
            base.WriteString("\r\n");
            WriteIndent();
            base.WriteStartElement(prefix, localName, ns);
            indentCount++;
        }

        public override void WriteString(string text)
        {
            string formattedText = text;
            switch (currentAttribute)
            {
                case "src":
                    switch (currentElement)
                    {
                        case "img":
                            formattedText = BuildImageSrc(text);
                            break;
                        case "script":
                            formattedText = BuildJavascriptSrc(text);
                            break;
                    }
                    break;
                case "href":
                    switch (currentElement)
                    {
                        case "link":
                            formattedText = BuildCssSrc(text);
                            break;
                    }
                    break;
            }
            base.WriteString(formattedText);
        }

        private void WriteIndent()
        {
            for (int i = 0; i < indentCount; i++)
            {
                base.WriteString("    ");
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            currentAttribute = localName;
            base.WriteStartAttribute(prefix, localName, ns);
        }


        public override void WriteEndAttribute()
        {
            currentAttribute = "";
            base.WriteEndAttribute();
        }

    }
}
