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
using System.Text.RegularExpressions;

namespace MigrationTool
{
    class HTMLUtilities
    {
        
        internal static async Task<Dictionary<string, IElement>> reading(List<IElement> tempHtmlList, string id, Dictionary<string, IElement> htmlids, string componentHTML)
        {
            
            var html = File.ReadAllText(componentHTML);

            var config = Configuration.Default;
            using var context = BrowsingContext.New(config);
            using var doc = await context.OpenAsync(req => req.Content(html));

            var lis = doc.GetElementById(id);

            var lisi = lis.Children;
            foreach (var li in lisi)
            {
                tempHtmlList.Add(li);
              
            }

            int i = 0;
            while (i < tempHtmlList.Count)
            {

                var child = tempHtmlList[i].Children;
                if (child.Length > 0)
                {
                    if (tempHtmlList[i].TagName == "A")
                    {
                        for (int k = 0; k < child.Length; k++)
                        {
                            tempHtmlList.Insert(i + 1 + k, child[k]);

                        }

                        i++;
                    }
                    else
                    {
                        for (int k = 0; k < child.Length; k++)
                        {
                            tempHtmlList.Insert(i + 1 + k, child[k]);

                        }

                        i++;
                    }
                }
                else
                {
                    i++;
                }


            }
            
            // adding all elements and  id to dictinary
            foreach (var element in tempHtmlList) {
                htmlids.Add(element.Id.Trim(), element);
            }
            return htmlids;
        }

        internal static void createDestHTML(string mainhtmlFile, string xmlpath, Dictionary<string, string> dr, string SourceXml)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"DestionationPages\");
            
            //extracting components.html
            HtmlDocument doc = new HtmlDocument();
            doc.Load(mainhtmlFile);//components.html
            var childnodes = doc.DocumentNode.ChildNodes;
            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");

            //creating destination.html and copy the component.html content
            string match = Regex.Match(SourceXml, @"_source.xml|source.xml").ToString();
            string filepath = (match.Trim().Length > 0)
                               ? path + SourceXml.Replace(match, "_destionationPage.html")
                               : path + SourceXml.Replace(".xml", "_destionationPage.html");
            HtmlDocument document = new HtmlDocument();

            //copying all in component.html without filtering
            using (FileStream fs = File.Create(filepath)) {
                foreach (var child in childnodes)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(child.OuterHtml);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                }
                fs.Close();
            }

            //extracting body in destinationPage.html to remove current children
            document.Load(filepath);
            HtmlNode destBodyNode = document.DocumentNode.SelectSingleNode("/html/body");
            destBodyNode.RemoveAllChildren();
            document.Save(filepath);

            //compare the component main children ids with the mapper ids value in destcomponent Tag
            foreach (var child in bodyNode.ChildNodes)
            {
               if(!checkHTMLNode(child))
               {
                    string childName = child.Name.Trim().ToLower();
                    if (childName.Equals("script") || childName.Equals("style"))
                    {
                        destBodyNode.AppendChild(child);
                    } else if(dr.Values.Contains(child.Id.Trim()))
                    {
                        destBodyNode.AppendChild(child);
                    }
               }
            }
            document.Save(filepath);

            //copy content from destination.xml to destinationPage.html
            writing(filepath, xmlpath);

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

        //internal static int getHtmlAttributeIndex(HtmlNode node)
        //{
        //    int index = -1;
        //    //searching for id for current node attributes
        //    for (int loop = 0; loop < node.Attributes.Count; loop++)
        //    {
        //        if (node.Attributes[loop].Name.Trim().ToLower() == "id")
        //        {
        //            index = loop;
        //            break;
        //        }
        //    }
        //    return index;
        //}
        private static bool checkHTMLNode(HtmlNode child)
        {
            Regex rg = new Regex(@"#[a-zA-Z]*");
            return (!rg.IsMatch(child.Name.Trim().ToLower())) ? false : true;
        }
    }

}

