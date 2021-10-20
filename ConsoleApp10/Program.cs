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
using System.Net.Http.Headers;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;
using Aspose.Cells;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ConsoleApp10
{
    class Program
    {

        
        static async Task Main(string[] args)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
          
            //Dictionary<string, string>
            //datastructure for mapper
            var htmlDictionary = new Dictionary<string, string>();
            Dictionary<string, string> mapperIds = new Dictionary<string, string>();
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
            // for keeping track of all html element and ids
            Dictionary<string, IElement> htmlids = new Dictionary<string, IElement>();


            //datastructure for dictionary for source xml element
            Dictionary<string, List<XmlNode>> finalXmlDictionary = new Dictionary<string, List<XmlNode>>();
            // for keeping track of all xml element and ids
            Dictionary<string, XmlNode> xmlids = new Dictionary<string, XmlNode>();

            //preparing files to be loaded
            path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            string mapperFile = path + @"\mapper.xml";
            string sourceFile = @"D:\VS_project\Migration\webtool-parsingxmlhtml\\source2.xml";
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

                tempHtmlList.Clear();
                tempXmlList.Clear();
                htmlids.Clear();
                xmlids.Clear();

                var src = htmlDictionary.ElementAt(j).Key;
                var dest = htmlDictionary.ElementAt(j).Value;
                XmlNodeList elemList3 = mapperxml.GetElementsByTagName(src);
                XmlNodeList elemList4 = mapperxml.GetElementsByTagName(dest);
                //var resultsrc = elemList3[0].ChildNodes;
                //var resultdest = elemList4[0].ChildNodes;
                mapperIds.Clear();
                


                Dictionary<string, IElement> outputhtml = null;

                AsyncContext.Run((Action)(async () =>
                {

                    outputhtml = await HTMLUtilities.reading(tempHtmlList, htmlDictionary.ElementAt(j).Value, htmlids);
                }));


                var outputxml = XMLUtilities.readingxml(tempXmlList, htmlDictionary.ElementAt(j).Key, xmlids);
                XmlNode userNodeparentmaster = xmlDoc.CreateElement("content");
                XmlAttribute attribute = xmlDoc.CreateAttribute("id");
                attribute.Value = dest;
                userNodeparentmaster.Attributes.Append(attribute);
                rootNode.AppendChild(userNodeparentmaster);
                for (int htmlloop = 0; htmlloop < outputhtml.Count; htmlloop++)
                {
                    var key = outputhtml.ElementAt(htmlloop).Key;
                    var value = outputhtml.ElementAt(htmlloop).Value;


                    //extracting maaper element in htmlfile using key 
                    XmlNodeList elementtobemapped = mapperxml.GetElementsByTagName(key);
                    if (elementtobemapped.Count > 0)
                    {
                        var innertext = elementtobemapped[0].InnerText.ToLower().Trim();
                        if (innertext!= null)
                        {
                            
                            XmlNode userNode = xmlDoc.CreateElement(value.TagName.ToLower());
                            userNode.InnerText = outputxml[innertext.Trim()].InnerText;
                            attribute = xmlDoc.CreateAttribute("id");
                            attribute.Value = key;
                            userNode.Attributes.Append(attribute);
                            userNodeparentmaster.AppendChild(userNode);
                        }

                        if (outputxml.ContainsKey(innertext))
                        {

                            var content = outputxml[innertext].InnerText;
                        }
                    }

                }
                xmlDoc.Save(destinationFile);
            }
            HTMLUtilities.createDestHTML(mainhtmlFile, destinationFile, htmlDictionary);
        }

       

    }
}

