using AngleSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Diagnostics;
using System.Reflection;

namespace ConsoleApp10
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dictionary<string, string>
            //datastructure for mapper
            var htmlDictionary = new Dictionary<string, string>();
            Dictionary<string, string> mapperIds= new Dictionary<string, string>();
            //for writting xml document
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("content");
            xmlDoc.AppendChild(rootNode);

            //datastructure for reading source xml
            List<XmlNode> tempXmlList = new List<XmlNode>();
            List<XmlNode> xmlList = new List<XmlNode>();

            //datastructure for html file
            List<IElement> tempHtmlList = new List<IElement>();
            List<IElement> htmlList = new List<IElement>();

            //datastructure for dictionary for html element
            Dictionary<string, List<IElement>> finalHtmlDictionary = new Dictionary<string, List<IElement>>();
            Dictionary<string, List<IElement>> tokeepcount = new Dictionary<string, List<IElement>>();

            //datastructure for dictionary for source xml element
            Dictionary<string, List<XmlNode>> finalXmlDictionary = new Dictionary<string, List<XmlNode>>();

            //preparing files to be loaded
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            string mapperFile = path + @"\mapper.xml";
            string sourceFile = path + @"\source.xml";
            string destinationFile = path + @"\destination.xml";
            string mainhtmlFile = path + @"test.html";
            

            var mapperxml = XMLUtilities.loadXML(mapperFile);
            var sourcexml = XMLUtilities.loadXML(sourceFile);
            ///reading new fuctionalities
            
            //// mapper functionality
            XmlNodeList elemList1 = mapperxml.GetElementsByTagName("srccomponent");
            XmlNodeList elemList2 = mapperxml.GetElementsByTagName("destcomponent");
            XMLUtilities.addingMapperToDiction(htmlDictionary, elemList1, elemList2, mapperxml);

           
            ////source xml 
            for (int j = 0; j < htmlDictionary.Count; j++)
            {
                var src = htmlDictionary.ElementAt(j).Key;
                var dest = htmlDictionary.ElementAt(j).Value;
                XmlNodeList elemList3 = mapperxml.GetElementsByTagName(src);
                XmlNodeList elemList4 = mapperxml.GetElementsByTagName(dest);
                var resultsrc= elemList3[0].ChildNodes;
                var resultdest = elemList4[0].ChildNodes;
                for (int y = 0; y < resultsrc.Count; y++) {

                    mapperIds.Add(Convert.ToString(resultdest[y].InnerText.Trim()), Convert.ToString(resultsrc[y].InnerText.Trim())) ;
                    
                
                }

                List<IElement> outputhtml = null;

              

                AsyncContext.Run((Action)(async () =>
                {
                    outputhtml = await HTMLUtilities.reading(tempHtmlList, htmlDictionary.ElementAt(j).Value);
                }));


                var outputxml = XMLUtilities.readingxml(tempXmlList, htmlDictionary.ElementAt(j).Key);

                for (int htmlloop = 0; htmlloop < outputhtml.Count; htmlloop++)
                {
                    //extract the html element(s) Id
                    var fi = outputhtml[htmlloop].Id;

                    if (mapperIds.ContainsKey(fi)==true)
                    {
                        string xmlIdInput = mapperIds[fi];
                        string query = string.Format("//*[@id='{0}']", xmlIdInput);

                        //extract the source elements
                        XmlElement element = (XmlElement)sourcexml.SelectSingleNode(query);
                        if (element != null)
                        {
                            // adding all html element
                            var kid = outputhtml[htmlloop].Children;
                            if (kid.Length > 0)
                            {
                                for (int k = 0; k < kid.Length; k++)
                                {
                                    htmlList.Add(kid[k]);
                                }
                                int i = 0;
                                while (i < htmlList.Count)
                                {
                                    var child = htmlList[i].Children;
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

                                        for (int k = 0; k < child.Length; k++)
                                        {
                                            htmlList.Insert(i + 1 + k, child[k]);
                                        }
                                        i++;
                                    }
                                    else
                                    {
                                        i++;
                                    }
                                }


                                for (int q = 0; q < htmlList.Count; q++)
                                {
                                    if (finalHtmlDictionary.ContainsKey(htmlList[q].TagName))
                                    {
                                        finalHtmlDictionary[htmlList[q].TagName].Add(htmlList[q]);
                                    }
                                    else
                                    {
                                        finalHtmlDictionary[htmlList[q].TagName] = new List<IElement> { htmlList[q] };
                                    }
                                }

                                var xmlnodes = element.ChildNodes;

                                for (int o = 0; o < xmlnodes.Count; o++)
                                {
                                    xmlList.Add(xmlnodes[o]);
                                }

                                int loop = 0;
                                while (loop < xmlList.Count)
                                {
                                    if (xmlList[loop].ChildNodes.Count > 0)
                                    {
                                        var result4 = xmlList[loop].ChildNodes;
                                        if (result4.Count == 1 && result4[0].Name == "#text")
                                        {
                                            loop++;
                                            continue;
                                        }
                                        for (int z = 0; z < result4.Count; z++)
                                        {
                                            xmlList.Insert(loop + 1 + z, result4[z]);
                                        }
                                        loop++;
                                    }
                                    else
                                    {
                                        loop++;
                                    }
                                }

                                for (int q = 0; q < xmlList.Count; q++)
                                {
                                    if (finalXmlDictionary.ContainsKey(xmlList[q].Name.ToUpper()))
                                    {
                                        finalXmlDictionary[xmlList[q].Name.ToUpper()].Add(xmlList[q]);
                                    }
                                    else
                                    {
                                        finalXmlDictionary[xmlList[q].Name.ToUpper()] = new List<XmlNode> { xmlList[q] };
                                    }
                                }

                                //adding
                                XmlNode userNodeparent = xmlDoc.CreateElement("content");
                                XmlAttribute attribute = xmlDoc.CreateAttribute("id");
                                attribute.Value = fi;
                                userNodeparent.Attributes.Append(attribute);
                                rootNode.AppendChild(userNodeparent);
                                for (int a = 0; a < finalHtmlDictionary.Count; a++)
                                {
                                    string first = finalHtmlDictionary.ElementAt(a).Key; //e.g A
                                    if (finalXmlDictionary.ContainsKey(first))
                                    {
                                        //adding the parent element in the destination xml

                                        for (int b = 0; b < finalHtmlDictionary[first].Count; b++)
                                        {
                                            var test = finalXmlDictionary[first].ElementAt(b).HasChildNodes;
                                            if (finalXmlDictionary[first].ElementAt(b).HasChildNodes == true && finalXmlDictionary[first].ElementAt(b).ChildNodes[0].Name != "#text")
                                            {
                                                
                                                if (finalXmlDictionary[first].ElementAt(b).ChildNodes.Count == finalHtmlDictionary[first].ElementAt(b).Children.Length) {
                                                    var test1 = finalHtmlDictionary[first].ElementAt(b).Children.Length;
                                                    var innerchild = finalHtmlDictionary[first].ElementAt(b).Children;
                                                    XmlNode userNode = xmlDoc.CreateElement(first.ToLower());
                                                    userNodeparent.AppendChild(userNode);
                                                    for (int r = 0; r < test1; r++)
                                                {

                                                        // string second = finalHtmlDictionary.ElementAt(a + 1 + r).Key;//change
                                                        string second = innerchild[r].TagName;


                                                        if (tokeepcount.ContainsKey(innerchild[r].TagName))
                                                        {
                                                            tokeepcount[ innerchild[r].TagName].Add(innerchild[r]);
                                                        }
                                                        else
                                                        {
                                                            tokeepcount[innerchild[r].TagName] = new List<IElement> { innerchild[r] };
                                                        }
                                                        //use the tag in test.html to be added to destination
                                                        XmlNode inneruserNode = xmlDoc.CreateElement(second.ToLower());

                                                        //if finalHtmlDictionary and finalXmlDictionary have the same key for example h3 then create xml node
                                                        if (finalXmlDictionary.ContainsKey(second))
                                                    {
                                                        inneruserNode.InnerText = finalXmlDictionary[second].ElementAt(b).InnerText;
                                                        userNode.AppendChild(inneruserNode);
                                                    }
                                                        /*
                                                         * if not, for example finalHtmlDictionary anchor has h3 chils
                                                         * and finalXmlDictionary anchor has a h2 or button child, 
                                                         * then check mapper parents elemet
                                                         */
                                                        else
                                                        {
                                                        
                                                        XmlNodeList parents = mapperxml.GetElementsByTagName("parents");//in mapper
                                                        var children = parents[0].ChildNodes.Count;
                                                        var parentChildren = parents[0].ChildNodes;
                                                        for (int index = 0; index < children; index++)
                                                        {
                                                            if (parentChildren[index].Name == first.ToUpper()) //<A>
                                                            {
                                                                int count = parentChildren[index].ChildNodes.Count;
                                                                var nestedParents = parentChildren[index].ChildNodes; //<heading>
                                                                for (int c = 0; c < count; c++)
                                                                {
                                                                    if (finalXmlDictionary.ContainsKey(nestedParents[c].InnerText.ToUpper()))
                                                                    {
                                                                        inneruserNode.InnerText = finalXmlDictionary[nestedParents[c].InnerText.ToUpper()].ElementAt(b).InnerText;
                                                                        userNode.AppendChild(inneruserNode);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                  
                                                }
                                                    a= a + tokeepcount.Count;
                                                    tokeepcount.Clear();


                                            } 

                                            }
                                            else
                                            {
                                                XmlNode userNode = xmlDoc.CreateElement(first.ToLower());
                                                finalHtmlDictionary[first].ElementAt(b).InnerHtml = finalXmlDictionary[first].ElementAt(b).InnerText;
                                                userNode.InnerText = finalXmlDictionary[first].ElementAt(b).InnerText;
                                                userNodeparent.AppendChild(userNode);
                                            }

                                        }
                                    }
                                }

                                htmlList.Clear();
                                xmlList.Clear();
                                finalXmlDictionary.Clear();
                                finalHtmlDictionary.Clear();
                            }


                            else
                            {
                                XmlNode userNode = xmlDoc.CreateElement(element.Name);
                                userNode.InnerText = element.InnerText;
                                XmlAttribute attribute = xmlDoc.CreateAttribute("id");
                                attribute.Value = fi;
                                userNode.Attributes.Append(attribute);
                                rootNode.AppendChild(userNode);

                            }
                        }

                    }



                }
            }
            xmlDoc.Save(destinationFile);
            HTMLUtilities.createDestHTML(mainhtmlFile, destinationFile, htmlDictionary);
        }
    }
}