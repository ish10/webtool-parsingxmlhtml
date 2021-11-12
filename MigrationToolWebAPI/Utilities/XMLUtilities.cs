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
using System.Diagnostics;
using System.Reflection;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace MigrationToolWebAPI
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
                htmlDictionary.Add(result1[i].InnerText.Trim().ToLower(), result2[i].InnerText.Trim());
            }

            
        }
        // TO READ EACH MAIN COMPONENT CHILDREN 
        internal static Dictionary<string, XmlNode> readingxml(List<XmlNode> xm, string id, Dictionary<string, XmlNode> xmlids, string sourcepath)
        {
            
            XmlDocument xmldoc = XMLUtilities.loadXML(sourcepath);
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

                foreach (var tag in xm)
                {

                    xmlids.Add(tag.Attributes["id"].Value.Trim(), tag);
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

            //extracting destinationPage.html
            HtmlDocument doc = new HtmlDocument();
            doc.Load(htmlpath);
            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");

            //actual comparing and appending
            foreach (HtmlNode nNode in bodyNode.Descendants())
            {
                bool isValidElement = checkHTMLNode(nNode);
                //if not #text nor script
                if (!isValidElement)
                {
                    string idValue = nNode.Id.Trim();
                    for (int loop = 0; loop < totalMainChildren; loop++)
                    {
                        xmlnode = extractElements(mainChildren[loop].ChildNodes, idValue); // search if html id found in destination.xml
                        if (xmlnode != null)
                        {
                            if (!nNode.Name.Trim().ToLower().Equals("div") && !xmlnode.Name.Trim().ToLower().Equals("content"))
                            {
                                if (nNode.ChildNodes.Count <= 1 && xmlnode.ChildNodes.Count <= 1)
                                {
                                    nNode.InnerHtml = xmlnode.InnerText;
                                }
                                //else if (nNode.ChildNodes.Count > 1)
                                //{
                                //    parseHTMLXML(nNode.ChildNodes, xmlnode);
                                //}
                                
                            }
                            
                        }
                    }
                    //xmlnode = extractElements(mainChildren, idValue); // search if html id found in destination.xml
                    
                }
            }
            doc.Save(htmlpath);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            try
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = htmlpath;
                process.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }



        }
        //private static void parseHTMLXML(HtmlNodeCollection collection, XmlNodeList mainChildren)
        //{
        //    foreach (HtmlNode nNode in collection)
        //    {
        //        if (!nNode.Name.Trim().ToLower().Equals("#text"))
        //        {
        //            foreach (XmlElement xml in mainChildren)
        //            {
        //                int htmlElementID = HTMLUtilities.getHtmlAttributeIndex(nNode);
        //                int xmlElementId = getXmlAttributeIndex(xml);
        //                if (xmlElementId > -1 && htmlElementID > -1)
        //                {
        //                    if (nNode.Attributes[htmlElementID].Value.Trim().ToLower() == xml.Attributes[xmlElementId].Value.Trim().ToLower())
        //                    {
        //                        if (nNode.ChildNodes.Count > 1)
        //                        {
        //                            parseHTMLXML(nNode.ChildNodes, xml.ChildNodes);
        //                        }
        //                        else if (nNode.ChildNodes.Count <= 1 && xml.ChildNodes.Count <= 1)
        //                        {
        //                            nNode.InnerHtml = xml.InnerText;
        //                        }
        //                    }
        //                } else if (nNode.Name.Trim().ToLower().Equals(xml.Name.Trim().ToLower()))
        //                {
        //                    if (nNode.ChildNodes.Count > 1)
        //                    {
        //                        parseHTMLXML(nNode.ChildNodes, xml.ChildNodes);
        //                    }
        //                    else if (nNode.ChildNodes.Count <= 1 && xml.ChildNodes.Count <= 1)
        //                    {
        //                        nNode.InnerHtml = xml.InnerText;
        //                    }
        //                }
        //            }
        //        }

        //    }
        //}
        private static int getXmlAttributeIndex(XmlElement node)
        {
            int index = -1;
            //searching for id for current node attributes
            for (int loop = 0; loop < node.Attributes.Count; loop++)
            {
                if (node.Attributes[loop].Name.Trim().ToLower() == "id")
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
                if (child.Attributes[index].Value.Trim() == idValue)
                {
                    return child;
                }
            }
            return null;
        }
        
        private static bool checkHTMLNode(HtmlNode child)
        {
            Regex rg = new Regex(@"#[a-zA-Z]*|script|style");
            return (!rg.IsMatch(child.Name.Trim().ToLower())) ? false : true;
        }
    }
}
