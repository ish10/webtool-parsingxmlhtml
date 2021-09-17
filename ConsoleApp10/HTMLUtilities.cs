using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using System.IO;
using System.Net.Http;
using System.Xml;
using AngleSharp.Dom;
using Nito.AsyncEx;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.Html;

namespace ConsoleApp10
{
    class HTMLUtilities
    {
        internal static async Task<List<IElement>> reading(List<IElement> tempHtmlList, string id)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            var html = File.ReadAllText(path + @"\test.html");

            var config = Configuration.Default;
            using var context = BrowsingContext.New(config);
            using var doc = await context.OpenAsync(req => req.Content(html));

            var lis = doc.GetElementById(id);

            var lisi = lis.Children;
            foreach (var li in lisi)
            {
                tempHtmlList.Add(li);
            }

            return tempHtmlList;
        }

        internal static void createDestHTML(string mainhtmlFile, string xmlpath, Dictionary<string, string> dr)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            //html main tags
            string mainTags = "<!DOCTYPE html> \n <html lang=\'en\' xmlns=\'http://www.w3.org/1999/xhtml\'> \n \t<head>\n\t\t<meta charset=\'utf - 8\'>\n\t\t<title>\n\t\t</title>\n\t</head>\n\t<body>\n\t</body>\n</html>";

            //extracting test.html
            HtmlDocument doc = new HtmlDocument();
            doc.Load(mainhtmlFile);//test.html
            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");
            for (int child = 0; child < bodyNode.ChildNodes.Count; child++)
            {
                int idIndex = getHtmlAttributeIndex(bodyNode.ChildNodes[child]);
                //if it not #text
                if (idIndex > -1)
                {
                    //extracting the id value 
                    string idAttr = bodyNode.ChildNodes[child].Attributes[idIndex].Value;
                    //search if found in mapper dictionary
                    bool InMapper = findInMapper(idAttr, dr);
                    //found
                    if (InMapper)
                    {
                        string filepath = path + idAttr + ".html";
                        HtmlDocument document = new HtmlDocument();

                        try
                        {
                            // Create the file, or overwrite if the file exists
                            FileStream fs = File.Create(filepath);
                            byte[] bytes = Encoding.UTF8.GetBytes(mainTags);
                            fs.Write(bytes, 0, bytes.Length);
                            fs.Flush();
                            fs.Close();
                            document.Load(filepath);
                            HtmlNode body = document.DocumentNode.SelectSingleNode("/html/body");
                            body.AppendChild(bodyNode.ChildNodes[child]);
                            document.Save(filepath);
                            HTMLUtilities.writing(filepath, xmlpath);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }

                }
            }
        }

        private static bool findInMapper(string idAttr, Dictionary<string, string> htmlDictionary)
        {
            foreach (var value in htmlDictionary.Values)
            {
                if (value == idAttr)
                {
                    return true;
                }
            }
            return false;
        }

        internal static void writing(string htmlpath, string xmlpath)
        {
            var destXml = XMLUtilities.loadXML(xmlpath);
            XMLUtilities.readFromDestination(destXml, htmlpath);
        }

        internal static int getHtmlAttributeIndex(HtmlNode node)
        {
            int index = -1;
            //searching for id for current node attributes
            for (int loop = 0; loop < node.Attributes.Count; loop++)
            {
                if (node.Attributes[loop].Name.ToLower() == "id")
                {
                    index = loop;
                    break;
                }
            }
            return index;
        }
    }

}

