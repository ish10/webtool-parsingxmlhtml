
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

namespace ConsoleApp10
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dictionary<string, string>
//datastructure for mapper
            var dr = new Dictionary<string, string>();
            //datastructure for reading source xml
            List<XmlNode> xm = new List<XmlNode>();
            List<XmlNode> xm1 = new List<XmlNode>();
            //datastructure for html file
            List<IElement> ht = new List<IElement>();
            List<IElement> ht1 = new List<IElement>();
            //datastructure for dictionary for html element
            Dictionary<string, List<IElement>> dch= new Dictionary<string, List<IElement>>();

            //datastructure for dictionary for xml element
            Dictionary<string, List<XmlNode>> dcx = new Dictionary<string, List<XmlNode>>();
            XmlDocument xml = new XmlDocument();
            xml.Load("C:\\Users\\ishpr\\Desktop\\mapper.xml"); // suppose that myXmlString contains "<Names>...</Names>"
            //loding source xml file for 2nd time
            XmlDocument xml2 = new XmlDocument();
            xml2.Load("C:\\Users\\ishpr\\Desktop\\example2.xml");
            // mapper functionality
            XmlNodeList elemList1 = xml.GetElementsByTagName("srccomponent");
            XmlNodeList elemList2 = xml.GetElementsByTagName("destcomponent");
            var result1 =elemList1[0].ChildNodes;
            var result2 = elemList2[0].ChildNodes;


            for (int i = 0; i < result1.Count; i++) {
                dr.Add(result1[i].InnerText, result2[i].InnerText);
            }


            //sourece xml 
            for (int j = 0; j < dr.Count; j++) {
                var src = dr.ElementAt(j).Key;
                var dest = dr.ElementAt(j).Value;
                List<IElement> outputhtml=null;
                AsyncContext.Run((Action)(async () =>
                {
                     outputhtml = await reading(ht, dr.ElementAt(j).Value);
                }));
                
               
              var outputxml = readingxml(xm, dr.ElementAt(j).Key);
              

                    for (int htmlloop = 0; htmlloop < outputhtml.Count; htmlloop++) {

                   var fi= outputhtml[htmlloop].Id;
                    string query = string.Format("//*[@id='{0}']", fi);
                    XmlElement el = (XmlElement)xml2.SelectSingleNode(query);

                    if (el!=null) {
                       var kid= outputhtml[htmlloop].Children;
                        if (kid.Length > 0) {

                            for (int k = 0; k < kid.Length; k++) {

                                ht1.Add(kid[k]);
                            
                            }

                          
                        




                            int i = 0;
                            while (i < ht1.Count) {

                                var child = ht1[i].Children;
                                if (child.Length > 0) {
                                    //if (ht[i].TagName == "A")
                                    //{
                                    //    for (int k = 0; k < child.Length; k++)
                                    //    {
                                    //        ht.Insert(i+1+k,child[k]);

                                    //    }

                                    //    i++;
                                    //}
                                    
                                    for (int k = 0; k < child.Length; k++) {
                                        ht1.Insert(i+1+k,child[k]);

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

                            for (int o = 0; o < xmlnodes.Count; o++) {

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
                            Console.WriteLine(dcx);
                            Console.WriteLine(dch);
                            XmlDocument xmlDoc = new XmlDocument();
                            XmlNode rootNode = xmlDoc.CreateElement("content");
                            xmlDoc.AppendChild(rootNode);

                            for (int a = 0; a < dch.Count; a++) {

                                string first = dch.ElementAt(a).Key;
                                if (dcx.ContainsKey(first)) {
                                    XmlNode mynode = xmlDoc.CreateElement("content");

                                    for (int b = 0; b < dch[first].Count; b++)
                                    {
                                        if (b<dcx[first].Count) { 

                                        dch[first].ElementAt(b).InnerHtml = dcx[first].ElementAt(b).InnerText;
                                        if (a==0 && b == 0)
                                        {

                                            rootNode.AppendChild(mynode);
                                            XmlNode userNode = xmlDoc.CreateElement(first);
                                            userNode.InnerText = dcx[first].ElementAt(b).InnerText;
                                            mynode.AppendChild(userNode);

                                        }
                                        else
                                        {
                                            XmlNode userNode = xmlDoc.CreateElement(first);
                                            mynode.InnerText = dcx[first].ElementAt(b).InnerText;
                                            mynode.AppendChild(userNode);


                                        }

                                            



                                    }
                                    }
                                    
                                    
                                   
                                
                                
                                }
                            }
                            xmlDoc.Save("C:\\Users\\ishpr\\Desktop\\test-doc.xml");

                        }

                    }
                    ht1.Clear();
                    xm1.Clear();
                    dch.Clear();
                    dcx.Clear();

                }
               
              
               
            }
         
          

           
        }

        static List<XmlNode> readingxml(List<XmlNode> xm, string id )
        {

            XmlDocument xml1 = new XmlDocument();
            xml1.Load("C:\\Users\\ishpr\\Desktop\\example2.xml");
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
        static async Task<List<IElement>> reading(List<IElement> ht, string id) {

            var html = File.ReadAllText(@"C:\Users\ishpr\Desktop\test.html");

            var config = Configuration.Default;
            using var context = BrowsingContext.New(config);
            using var doc = await context.OpenAsync(req => req.Content(html));
            
            var lis = doc.GetElementById(id);

            var lisi = lis.Children;
            foreach (var li in lisi)
            {
                ht.Add(li);
            }

            //int i = 0;
            //while (i < ht.Count) {
               
            //    var child = ht[i].Children;
            //    if (child.Length > 0) {
            //        if (ht[i].TagName == "A")
            //        {
            //            for (int k = 0; k < child.Length; k++)
            //            {
            //                ht.Insert(i+1+k,child[k]);

            //            }

            //            i++;
            //        }
            //        else { 
            //        for (int k = 0; k < child.Length; k++) {
            //            ht.Insert(i+1+k,child[k]);
                        
            //        }

            //        i++;
            //        }
            //    }
            //    else
            //    {
            //        i++;
            //    }


            //}

            return ht;
        }
    }
}
