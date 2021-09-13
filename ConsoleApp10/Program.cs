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
            var dr = new Dictionary<string, string>();
            //for writting xml document
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("content");
            xmlDoc.AppendChild(rootNode);

            //datastructure for reading source xml
            List<XmlNode> xm = new List<XmlNode>();
            List<XmlNode> xm1 = new List<XmlNode>();

            //datastructure for html file
            List<IElement> ht = new List<IElement>();
            List<IElement> ht1 = new List<IElement>();

            //datastructure for dictionary for html element
            Dictionary<string, List<IElement>> dch = new Dictionary<string, List<IElement>>();

            //datastructure for dictionary for source xml element
            Dictionary<string, List<XmlNode>> dcx = new Dictionary<string, List<XmlNode>>();

            //preparing files to be loaded
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            string mapperFile = path + @"\mapper.xml";
            string sourceFile = path + @"\source.xml";
            string destinationFile = path + @"\destination.xml";
            string finalhtml = path + @"\finalresult.html";

            var xml = XMLUtilities.loadXML(mapperFile);
            var xml2 = XMLUtilities.loadXML(sourceFile);

            //// mapper functionality
            XmlNodeList elemList1 = xml.GetElementsByTagName("srccomponent");
            XmlNodeList elemList2 = xml.GetElementsByTagName("destcomponent");
            XMLUtilities.addingMapperToDiction(dr, elemList1, elemList2);

            XMLUtilities.writeToDestination(destinationFile, "<main>"); //build a root level element
            ////source xml 
            for (int j = 0; j < dr.Count; j++)
            {
                var src = dr.ElementAt(j).Key;
                var dest = dr.ElementAt(j).Value;
                List<IElement> outputhtml = null;

                XMLUtilities.writeToDestination(destinationFile, " <" + dest + ">"); //taking the tagname used in mapper and add as xml element wrapper

                AsyncContext.Run((Action)(async () =>
                {
                    outputhtml = await HTMLUtilities.reading(ht, dr.ElementAt(j).Value);
                }));


                var outputxml = XMLUtilities.readingxml(xm, dr.ElementAt(j).Key);

                for (int htmlloop = 0; htmlloop < outputhtml.Count; htmlloop++)
                {
                    //extract the html element(s) Id
                    var fi = outputhtml[htmlloop].Id;
                    string query = string.Format("//*[@id='{0}']", fi);

                    //extract the source elements
                    XmlElement el = (XmlElement)xml2.SelectSingleNode(query);
                    if (el != null)
                    {
                        // adding all html element
                        var kid = outputhtml[htmlloop].Children;
                        if (kid.Length > 0)
                        {
                            for (int k = 0; k < kid.Length; k++)
                            {
                                ht1.Add(kid[k]);
                            }
                            int i = 0;
                            while (i < ht1.Count)
                            {
                                var child = ht1[i].Children;
                                if (child.Length > 0)
                                {
                                    if (ht[i].TagName == "A")
                                    {
                                        for (int k = 0; k < child.Length; k++)
                                        {
                                            ht.Insert(i + 1 + k, child[k]);
                                        }

                                        i++;
                                    }

                                    for (int k = 0; k < child.Length; k++)
                                    {
                                        ht1.Insert(i + 1 + k, child[k]);
                                    }
                                    i++;
                                }
                                else
                                {
                                    i++;
                                }
                            }


                            for (int q = 0; q < ht1.Count; q++)
                            {
                                if (dch.ContainsKey(ht1[q].TagName))
                                {
                                    dch[ht1[q].TagName].Add(ht1[q]);
                                }
                                else
                                {
                                    dch[ht1[q].TagName] = new List<IElement> { ht1[q] };
                                }
                            }

                            var xmlnodes = el.ChildNodes;

                            for (int o = 0; o < xmlnodes.Count; o++)
                            {
                                xm1.Add(xmlnodes[o]);
                            }

                            int ishp = 0;
                            while (ishp < xm1.Count)
                            {
                                if (xm1[ishp].ChildNodes.Count > 0)
                                {
                                    var result4 = xm1[ishp].ChildNodes;
                                    if (result4.Count == 1 && result4[0].Name == "#text")
                                    {
                                        ishp++;
                                        continue;
                                    }
                                    for (int z = 0; z < result4.Count; z++)
                                    {
                                        xm1.Insert(ishp + 1 + z, result4[z]);
                                    }
                                    ishp++;
                                }
                                else
                                {
                                    ishp++;
                                }
                            }

                            for (int q = 0; q < ht1.Count; q++)
                            {
                                if (dcx.ContainsKey(xm1[q].Name.ToUpper()))
                                {
                                    dcx[xm1[q].Name.ToUpper()].Add(xm1[q]);
                                }
                                else
                                {
                                    dcx[xm1[q].Name.ToUpper()] = new List<XmlNode> { xm1[q] };
                                }
                            }

                            //adding
                            XmlNode userNodeparent = xmlDoc.CreateElement("content");
                            XmlAttribute attribute = xmlDoc.CreateAttribute("id");
                            attribute.Value = fi;
                            userNodeparent.Attributes.Append(attribute);
                            rootNode.AppendChild(userNodeparent);
                            for (int a = 0; a < dch.Count; a++)
                            {
                                string first = dch.ElementAt(a).Key;
                                if (dcx.ContainsKey(first))
                                {
                                    //adding the parent element in the destination xml
                                    XMLUtilities.writeToDestination(destinationFile, "     <" + first + ">");
                                    for (int b = 0; b < dch[first].Count; b++)
                                    {
                                        var test = dcx[first].ElementAt(b).HasChildNodes;
                                        if (dcx[first].ElementAt(b).HasChildNodes == true && dcx[first].ElementAt(b).ChildNodes[0].Name != "#text")
                                        {
                                            var test1 = dcx[first].ElementAt(b).ChildNodes.Count;
                                            XmlNode userNode = xmlDoc.CreateElement(first);
                                            userNodeparent.AppendChild(userNode);
                                            for (int r = 0; r < test1; r++)
                                            {

                                                string second = dch.ElementAt(a + 1).Key;
                                                XmlNode inneruserNode = xmlDoc.CreateElement(second);
                                                inneruserNode.InnerText = dcx[second].ElementAt(b).InnerText;
                                                userNode.AppendChild(inneruserNode);


                                            }

                                        }
                                        else
                                        {
                                            XmlNode userNode = xmlDoc.CreateElement(first);
                                            dch[first].ElementAt(b).InnerHtml = dcx[first].ElementAt(b).InnerText;
                                            userNode.InnerText = dcx[first].ElementAt(b).InnerText;
                                            userNodeparent.AppendChild(userNode);
                                        }
                                        // append the parent's children. Note: need to parse it more.
                                        XMLUtilities.writeToDestination(destinationFile, "         " + dch[first].ElementAt(b).InnerHtml.Trim());
                                    }
                                    XMLUtilities.writeToDestination(destinationFile, "     </" + first + ">");

                                }
                            }

                            ht1.Clear();
                            xm1.Clear();
                            dcx.Clear();
                            dch.Clear();
                        }


                        else {
                            XmlNode userNode = xmlDoc.CreateElement(el.Name);
                            userNode.InnerText = el.InnerText;
                            XmlAttribute attribute = xmlDoc.CreateAttribute("id");
                            attribute.Value = fi;
                            userNode.Attributes.Append(attribute);
                            rootNode.AppendChild(userNode);

                        }
                    }
                }
                XMLUtilities.writeToDestination(destinationFile, " </" + dest + ">");
            }
            xmlDoc.Save(@"C:\Users\ishpr\Desktop\test-doc.xml");
            XMLUtilities.writeToDestination(destinationFile, "</main>"); //close the root level element
            HTMLUtilities.writing(finalhtml, destinationFile);
        }
    }
}