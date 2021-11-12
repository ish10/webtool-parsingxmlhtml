using AngleSharp.Dom;
using MigrationToolWebAPI.Pojo;
using MigrationToolWebAPI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace MigrationToolWebAPI
{
    class MainTool
    {
        private static string sourcepath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"\MigrationToolWebAPI\bin\Debug\net5.0", @"\");
        private static string componentpath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"ComponentHtml\");
        private static string mapperpath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"MapperXML\");
        private static string destinationpathJson = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"DestinationJSON\");
        private static string destinationpath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"DestinationXML\");
        private static string filesConfigPath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"\Config\") + "FilesConfig.json";
        private static string FinalPathsConfig = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"\Config\") + "FinalPaths.json";

        public static async Task toolProcessAsync()
        {
            List<tags> _tags = new List<tags>();
            List<data> _data = new List<data>();
            FilesMapping[] filesArray = getMappedFiles();
            ArrayList mappedFiles = new ArrayList();

            foreach (FilesMapping files in filesArray)
            {
                FinalPaths finalPathDest = new FinalPaths();
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
                finalPathDest.destnationXML = destinationFile;

                string destinationFileJSON = (match.Trim().Length > 0)
                                                     ? destinationpathJson + files.SourceXML.Replace(match, "_destination.json")
                                                     : destinationpathJson + files.SourceXML.Replace(".json", "_destination.json");
                finalPathDest.destnationJSON = destinationFileJSON;

                var mapperxml = XMLUtilities.loadXML(mapperFile);
                var sourcexml = XMLUtilities.loadXML(sourceFile);
                ///reading new fuctionalities

                //// mapper functionality
                XmlNodeList elemList1 = mapperxml.GetElementsByTagName("srccomponent");
                XmlNodeList elemList2 = mapperxml.GetElementsByTagName("destcomponent");
                XMLUtilities.addingMapperToDiction(htmlDictionary, elemList1, elemList2, mapperxml);
                

                if (!Directory.Exists(destinationpathJson))
                    Directory.CreateDirectory(destinationpathJson);
                using FileStream createStream1 = File.Create(destinationFileJSON);

                ////source xml 
                for (int j = 0; j < htmlDictionary.Count; j++)

                {
                    _data.Clear();
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
                                _data.Add(new data
                                {
                                    childtag = new Dictionary<string, string>
                                    {

                                        [key] = outputxml[innertext.Trim()].InnerText.Trim()


                                    }

                                }) ; 

                                XmlNode userNode = xmlDoc.CreateElement(value.TagName.ToLower().Trim());
                                userNode.InnerText = outputxml[innertext].InnerText;
                                attribute = xmlDoc.CreateAttribute("id");
                                attribute.Value = key;
                                userNode.Attributes.Append(attribute);
                                userNodeparentmaster.AppendChild(userNode);
                            }


                        }

                    }

                    _tags.Add(new tags()
                    {
                        
                        mainid = new Dictionary<string, List<data>>()
                            {
                                [dest] = clonemethod(_data)


                            }


                    })  ;
                    if (!Directory.Exists(destinationpath))
                        Directory.CreateDirectory(destinationpath);
                    xmlDoc.Save(destinationFile);
                }
                await System.Text.Json.JsonSerializer.SerializeAsync(createStream1, _tags);
                _tags.Clear();
                HTMLUtilities.createDestHTML(mainhtmlFile, destinationFile, htmlDictionary, files.SourceXML, finalPathDest);
                mappedFiles.Add(finalPathDest);
            }

            //to fill the empty strings(e.g destinationHTML, destinationJSOn, etc..) to the fileconfig.json
            try
            {
                var jsonToWrite = JsonConvert.SerializeObject(mappedFiles, Formatting.Indented);
                using (var writer = new StreamWriter(FinalPathsConfig))
                {
                    writer.WriteLine(jsonToWrite);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
            
            using (StreamReader read = new StreamReader(filesConfigPath))
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

        public static List<data> clonemethod(List<data> data)
        {

            List<data> data_1 = data.Select(book => new data
            {
                childtag = new Dictionary<string, string>
                {

                    [book.childtag.ElementAt(0).Key] = book.childtag.ElementAt(0).Value

                }


            }).ToList();
            return data_1;
        }
    }
}
