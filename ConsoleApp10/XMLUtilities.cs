using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp;
using System.IO;
using System.Net.Http;
using AngleSharp.Dom;
using Nito.AsyncEx;
using System.Diagnostics;
using System.Reflection;
using HtmlAgilityPack;

namespace ConsoleApp10
{
    class XMLUtilities
    {
       
        internal static XmlDocument loadXML(string path)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            return xml;
        }
        // TO READ STARTING MATCHING COMPONENT
        internal static void addingMapperToDiction(Dictionary<string, string> htmlDictionary, XmlNodeList elemList1, XmlNodeList elemList2, XmlDocument xml)
        {
            var result1 = elemList1[0].ChildNodes;
            var result2 = elemList2[0].ChildNodes;
            for (int i = 0; i < result1.Count; i++)
            {
                htmlDictionary.Add(result1[i].InnerText, result2[i].InnerText);
            }

            
        }
        // TO READ EACH MAIN COMPONENT CHILDREN 
        internal static Dictionary<string, XmlNode> readingxml(List<XmlNode> xm, string id, Dictionary<string, XmlNode> xmlids)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            XmlDocument xmldoc = XMLUtilities.loadXML(path + @"\source2.xml");
            //string id = "gamecarousel";
            string query = string.Format("//*[@id='{0}']", id);
            XmlElement element = (XmlElement)xmldoc.SelectSingleNode(query);
            var result3 = element.ChildNodes;

            int loop = 0;
            while (loop < result3.Count)
            {

                xm.Add(result3[loop]);
                loop++;

            }
            int ishp = 0;
            while (ishp < xm.Count)
            {
                if (xm[ishp].ChildNodes.Count > 0)
                {
                    if (xm[ishp].Name == "a")
                    {
                        var result5 = xm[ishp].ChildNodes;
                        if (result5.Count == 1 && result5[0].Name == "#text")
                        {
                            ishp++;
                            continue;
                        }

                        for (int z = 0; z < result5.Count; z++)
                        {
                            xm.Insert(ishp + 1 + z, result5[z]);

                        }
                        ishp++;
                    }

                    else
                    {
                        var result4 = xm[ishp].ChildNodes;
                        if (result4.Count == 1 && result4[0].Name == "#text")
                        {
                            ishp++;
                            continue;
                        }
                        for (int z = 0; z < result4.Count; z++)
                        {
                            xm.Insert(ishp + 1 + z, result4[z]);

                        }

                        ishp++;
                    }
                }
                else
                {



                    ishp++;
                }


            }

            foreach (var tag in xm) {

                xmlids.Add(tag.Attributes["id"].Value.Trim(),tag);
            }

           return xmlids;
        }

        internal static void readFromDestination(XmlDocument xml, string htmlpath)
        {
            //extracting destination.xml
            XmlElement element = (XmlElement)xml.DocumentElement; //root
            XmlNodeList mainChildren = element.ChildNodes;
            int totalMainChildren = element.ChildNodes.Count;
            XmlElement xmlnode;

            //extracting test.html
            HtmlDocument doc = new HtmlDocument();
            doc.Load(htmlpath);
            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");

            //actual comparing and appending
            foreach (HtmlNode nNode in bodyNode.Descendants())
            {
                int index = HTMLUtilities.getHtmlAttributeIndex(nNode);
                if (index >= 0)
                {
                    string idValue = nNode.Attributes[index].Value;
                    xmlnode = extractElements(mainChildren, idValue); // parent's children => will change this part
                    if (xmlnode != null)
                    {
                        if (nNode.ChildNodes.Count < 1 && xmlnode.ChildNodes.Count <= 1)
                        {
                            nNode.InnerHtml = xmlnode.InnerText;
                        }
                        else if (nNode.ChildNodes.Count >= 1 && xmlnode.ChildNodes.Count >= 1)
                        {
                            parseHTMLXML(nNode.ChildNodes, xmlnode.ChildNodes);
                        }
                    }
                }
            }
            doc.Save(htmlpath);

        }
        private static void parseHTMLXML(HtmlNodeCollection collection, XmlNodeList mainChildren)
        {
            foreach (HtmlNode nNode in collection)
            {
                if (!nNode.Name.Trim().ToLower().Equals("#text"))
                {
                    foreach (XmlElement xml in mainChildren)
                    {
                        int htmlElementID = HTMLUtilities.getHtmlAttributeIndex(nNode);
                        int xmlElementId = getXmlAttributeIndex(xml);
                        if (xmlElementId > -1 && htmlElementID > -1)
                        {
                            if (nNode.Attributes[htmlElementID].Value == xml.Attributes[xmlElementId].Value)
                            {
                                if (nNode.ChildNodes.Count > 1)
                                {
                                    parseHTMLXML(nNode.ChildNodes, xml.ChildNodes);
                                }
                                else if (nNode.ChildNodes.Count <= 1 && xml.ChildNodes.Count <= 1)
                                {
                                    nNode.InnerHtml = xml.InnerText;
                                }
                            }
                        } else if (nNode.Name.Trim().ToLower().Equals(xml.Name.Trim().ToLower()))
                        {
                            if (nNode.ChildNodes.Count > 1)
                            {
                                parseHTMLXML(nNode.ChildNodes, xml.ChildNodes);
                            }
                            else if (nNode.ChildNodes.Count <= 1 && xml.ChildNodes.Count <= 1)
                            {
                                nNode.InnerHtml = xml.InnerText;
                            }
                        }
                    }
                }

            }
        }
        private static int getXmlAttributeIndex(XmlElement node)
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
        private static XmlElement extractElements(XmlNodeList mainChildren, string idValue)
        {

            foreach (XmlElement child in mainChildren)
            {
                //getting the id value for each child
                int index = getXmlAttributeIndex(child);
                if (child.Attributes[index].Value == idValue)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
