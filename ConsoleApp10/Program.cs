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
            Dictionary<string, string> drchild = new Dictionary<string, string>();
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
            string mainhtmlFile = path + @"test.html";
            

            var xml = XMLUtilities.loadXML(mapperFile);
            var xml2 = XMLUtilities.loadXML(sourceFile);
            ///reading new fuctionalities
            
            //// mapper functionality
            XmlNodeList elemList1 = xml.GetElementsByTagName("srccomponent");
            XmlNodeList elemList2 = xml.GetElementsByTagName("destcomponent");
            XMLUtilities.addingMapperToDiction(dr, elemList1, elemList2,xml);

           
            ////source xml 
            for (int j = 0; j < dr.Count; j++)
            {
                var src = dr.ElementAt(j).Key;
                var dest = dr.ElementAt(j).Value;
                XmlNodeList elemList3 = xml.GetElementsByTagName(src);
                XmlNodeList elemList4 = xml.GetElementsByTagName(dest);
                var resultsrc= elemList3[0].ChildNodes;
                var resultdest = elemList4[0].ChildNodes;
                for (int y = 0; y < resultsrc.Count; y++) {

                    drchild.Add(Convert.ToString(resultdest[y].InnerText.Trim()), Convert.ToString(resultsrc[y].InnerText.Trim())) ;
                    
                
                }

                List<IElement> outputhtml = null;

              

                AsyncContext.Run((Action)(async () =>
                {
                    outputhtml = await HTMLUtilities.reading(ht, dr.ElementAt(j).Value);
                }));


                var outputxml = XMLUtilities.readingxml(xm, dr.ElementAt(j).Key);

                for (int htmlloop = 0; htmlloop < outputhtml.Count; htmlloop++)
                {
                    //extract the html element(s) Id
                    var fi = outputhtml[htmlloop].Id;

                    if (drchild.ContainsKey(fi)==true)
                    {
                        string xmlIdInput = drchild[fi];
                        string query = string.Format("//*[@id='{0}']", xmlIdInput);

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

                                        for (int b = 0; b < dch[first].Count; b++)
                                        {
                                            var test = dcx[first].ElementAt(b).HasChildNodes;
                                            if (dcx[first].ElementAt(b).HasChildNodes == true && dcx[first].ElementAt(b).ChildNodes[0].Name != "#text")
                                            {
                                                var test1 = dcx[first].ElementAt(b).ChildNodes.Count;
                                                XmlNode userNode = xmlDoc.CreateElement(first.ToLower());
                                                userNodeparent.AppendChild(userNode);
                                                for (int r = 0; r < test1; r++)
                                                {

                                                    string second = dch.ElementAt(a + 1).Key;
                                                    XmlNode inneruserNode = xmlDoc.CreateElement(second.ToLower());
                                                    inneruserNode.InnerText = dcx[second].ElementAt(b).InnerText;
                                                    userNode.AppendChild(inneruserNode);


                                                }
                                                a++;

                                            }
                                            else
                                            {
                                                XmlNode userNode = xmlDoc.CreateElement(first.ToLower());
                                                dch[first].ElementAt(b).InnerHtml = dcx[first].ElementAt(b).InnerText;
                                                userNode.InnerText = dcx[first].ElementAt(b).InnerText;
                                                userNodeparent.AppendChild(userNode);
                                            }

                                        }
                                    }
                                }

                                ht1.Clear();
                                xm1.Clear();
                                dcx.Clear();
                                dch.Clear();
                            }


                            else
                            {
                                XmlNode userNode = xmlDoc.CreateElement(el.Name);
                                userNode.InnerText = el.InnerText;
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
            HTMLUtilities.writing(mainhtmlFile, destinationFile);
        }
    }
}