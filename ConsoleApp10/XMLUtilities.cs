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

        internal static void addingMapperToDiction(Dictionary<string, string> dr, XmlNodeList elemList1, XmlNodeList elemList2)
        {
            var result1 = elemList1[0].ChildNodes;
            var result2 = elemList2[0].ChildNodes;
            for (int i = 0; i < result1.Count; i++)
            {
                dr.Add(result1[i].InnerText, result2[i].InnerText);
            }
        }

        internal static List<XmlNode> readingxml(List<XmlNode> xm, string id)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            XmlDocument xml1 = XMLUtilities.loadXML(path + @"\source.xml");
            //string id = "gamecarousel";
            string query = string.Format("//*[@id='{0}']", id);
            XmlElement el = (XmlElement)xml1.SelectSingleNode(query);
            var result3 = el.ChildNodes;

            int ish = 0;
            while (ish < result3.Count)
            {

                xm.Add(result3[ish]);
                ish++;

            }
            //int ishp = 0;
            //while (ishp < xm.Count)
            //{
            //    if (xm[ishp].ChildNodes.Count > 0)
            //    {
            //        if (xm[ishp].Name == "a")
            //        {
            //            var result5 = xm[ishp].ChildNodes;
            //            if (result5.Count == 1 && result5[0].Name == "#text")
            //            {
            //                ishp++;
            //                continue;
            //            }

            //            for (int z = 0; z < result5.Count; z++)
            //            {
            //                xm.Insert(ishp +1+z, result5[z]);

            //            }
            //            ishp++;
            //        }

            //        else
            //        {
            //            var result4 = xm[ishp].ChildNodes;
            //            if (result4.Count == 1 && result4[0].Name == "#text")
            //            {
            //                ishp++;
            //                continue;
            //            }
            //            for (int z = 0; z < result4.Count; z++)
            //            {
            //                xm.Insert(ishp+1+z,result4[z]);

            //            }

            //            ishp++;
            //        }
            //    }
            //    else
            //    {



            //        ishp++;
            //    }


            //}
            return xm;
        }

        internal static void writeToDestination(string path, string data)
        {
            using (FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                try
                {
                    // Encapsulate the filestream object in a StreamWriter instance.
                    StreamWriter fileWriter = new StreamWriter(file);
                    // Write the current date time to the file
                    fileWriter.WriteLine(data);
                    fileWriter.Flush();
                    fileWriter.Close();
                }
                catch (IOException ioe)
                {
                    Console.WriteLine(ioe);
                }
            };
        }

        internal static List<string> readFromDestination(XmlDocument xml)
        {
            XmlElement el = (XmlElement)xml.DocumentElement; //root
            XmlNodeList mainChildren = el.ChildNodes;
            int totalMainChildren = el.ChildNodes.Count;
            List<string> xmlContent = new List<string>();
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < totalMainChildren; index++)
            {
                sb.Append("\n<div>\n");
                    extractElements(sb, mainChildren[index].ChildNodes);
                sb.Append("\n</div>\n");
                xmlContent.Add(sb.ToString());
                sb.Clear();
            }
            return xmlContent;
        }
        private static void extractElements(StringBuilder sb, XmlNodeList mainChildren)
        {
           foreach (XmlElement child in mainChildren) {
              int childInner = child.ChildNodes.Count;//span
              if (child.GetType().ToString() == "System.Xml.XmlElement") 
              {
                sb.Append("<" + child.Name.ToLower().Trim() + ">\n");
                //recursive
                //for (int inner = 0; inner < childInner; inner++)
                //{
                        if (child.ChildNodes.Count > 1)
                        {
                            extractElements(sb, child.ChildNodes);
                        } else
                        {

                            sb.Append(child.InnerXml.ToLower().Trim()+"\n");
                        }
                //}
                sb.Append("</" + child.Name.ToLower().Trim() + ">\n");
              } else
              {
                sb.Append(child.ToString().ToLower()+"\n");
               }
           }
        }
    }
}
