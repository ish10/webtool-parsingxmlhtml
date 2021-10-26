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
using Microsoft.AspNetCore.Hosting;

namespace MigrationTool
{
    class Program
    {
        private static string sourcepath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"\ConsoleApp10\bin\Debug\net5.0", @"\");
        private static string componentpath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"ComponentHtml\");
        private static string mapperpath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"MapperXML\");
        private static string destinationpath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"DestinationXML\");
        static async Task Main(string[] args)
        {
            FilesMapping[] filesArray = getMappedFiles();
            foreach(FilesMapping files in filesArray)
            {
                //string path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
                //var sourcepath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"\ConsoleApp10\bin\Debug\net5.0", @"\") + "source.xml";

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


                string mapperFile = mapperpath + files.MapperXML;
                string sourceFile = sourcepath + files.SourceXML;
                string mainhtmlFile = componentpath + files.ComponentHTML;
                string match = Regex.Match(files.SourceXML, @"_source.xml|source.xml").ToString();
                string destinationFile = (match.Trim().Length > 0)
                                                            ? destinationpath + files.SourceXML.Replace(match, "_destination.xml")
                                                            : destinationpath + files.SourceXML.Replace(".xml", "_destination.xml");

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

                    //AsyncContext.Run((Action)(async () =>
                    //{

                    outputhtml = await HTMLUtilities.reading(tempHtmlList, htmlDictionary.ElementAt(j).Value, htmlids, mainhtmlFile);
                    //}));


                    var outputxml = XMLUtilities.readingxml(tempXmlList, htmlDictionary.ElementAt(j).Key, xmlids, sourceFile);
                    XmlNode userNodeparentmaster = xmlDoc.CreateElement("content");
                    XmlAttribute attribute = xmlDoc.CreateAttribute("id");
                    attribute.Value = dest;
                    userNodeparentmaster.Attributes.Append(attribute);
                    rootNode.AppendChild(userNodeparentmaster);
                    for (int htmlloop = 0; htmlloop < outputhtml.Count; htmlloop++)
                    {
                        var key = outputhtml.ElementAt(htmlloop).Key.Trim();
                        var value = outputhtml.ElementAt(htmlloop).Value;


                        //extracting maaper element in htmlfile using key 
                        XmlNodeList elementtobemapped = mapperxml.GetElementsByTagName(key);
                        if (elementtobemapped.Count > 0)
                        {
                            var innertext = elementtobemapped[0].InnerText.Trim();
                            if (innertext != null)
                            {

                                XmlNode userNode = xmlDoc.CreateElement(value.TagName.ToLower().Trim());
                                userNode.InnerText = outputxml[innertext].InnerText;
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
                HTMLUtilities.createDestHTML(mainhtmlFile, destinationFile, htmlDictionary, files.SourceXML);

            }
        }

        private static FilesMapping[] getMappedFiles()
        {
            string mappedFilesJson = extractJsonObject("mappedFiles");
            FilesMapping[] filesArray = Newtonsoft.Json.JsonConvert.DeserializeObject<FilesMapping[]>(mappedFilesJson);
            return filesArray;
        }

        private static string extractJsonObject(string jsonName)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"\") + "FilesConfig.json";
            using (StreamReader read = new StreamReader(path))
            {
                string jsonString = read.ReadToEnd();
                JObject deserializeObject = (JObject)JsonConvert.DeserializeObject(jsonString);
                foreach (var element in deserializeObject)
                {
                    if (element.Key.ToString().Trim().ToLower() == jsonName.Trim().ToLower())
                        return element.Value.ToString();
                }
                read.Close();

            }
            return null;
        }



    }
}

