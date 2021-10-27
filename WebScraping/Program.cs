using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ScrapySharp.Extensions;


namespace WebScraping
{
    class Program
    {
        private static List<HtmlNode> foundElements = new List<HtmlNode>();
        static async Task Main(string[] args)
        {
            
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"\WebScraping\bin\Debug\net5.0", @"\");
            string fileName = "";
            Dictionary<string, int> sourceFiles = new Dictionary<string, int>();
            try
            {
                //extract all webpages we want to migrate and loop over them
                string websitesJson = extractJsonObject("websites");
                PageFeature[] websitesArray = Newtonsoft.Json.JsonConvert.DeserializeObject<PageFeature[]>(websitesJson);
                foreach (PageFeature page in websitesArray)
                {
                    //make each source file name unique according to the given name or by incrementing if found before
                    if(page.Name.Trim() == "")
                    {
                        fileName = Regex.Match(page.URL, @"\b(\.\w+\.)\b").ToString().Replace(".", "");
                        if (sourceFiles.ContainsKey(fileName))
                        {
                            ++sourceFiles[fileName];
                            fileName = $"{fileName}_{sourceFiles[fileName]}_source.xml";
                        }
                        else
                        {
                            sourceFiles.Add(fileName, 0);
                            fileName += "_source.xml";
                        }
                    }
                    else if (sourceFiles.ContainsKey(page.Name))
                    {
                        ++sourceFiles[page.Name];
                        fileName = $"{page.Name}_{sourceFiles[page.Name]}_source.xml";
                    } else {
                        fileName = $"{page.Name}_source.xml";
                        sourceFiles.Add(page.Name, 0);
                    }
                    
                    string fullPath = path + fileName;
                    await getFromWebsite(fullPath, page);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static async Task getFromWebsite(string path, PageFeature currentPage)
        {
            try
            {
                // Read HTML page From Web
                HtmlWeb web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(currentPage.URL);

                //extracting and parsing the website
                HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");
                string filterXPath = currentPage.MainContainer;
                string filterAttribute = currentPage.FilteredAttribute;
                var parentNodes = bodyNode.SelectNodes(filterXPath);
                Dictionary<string, int> componentsIdFound = new Dictionary<string, int>();
                Regex rg = new Regex(@"#[a-zA-Z]*|script");
                string attributeReg = @"[^\w\d-_:,]";
                string componentId = "";


                using (XmlWriter writer = XmlWriter.Create(path))
                {
                    //xml version element
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Root");
                    foreach (var child in parentNodes)
                    {
                        if (!rg.IsMatch(child.Name.Trim().ToLower()))
                        {
                            var div = child.DescendantsAndSelf();
                            try
                            {
                                foreach (HtmlNode component in div)
                                {
                                    if (!rg.IsMatch(component.Name.Trim().ToLower()) && !component.GetAttributeValue(filterAttribute).Equals(""))
                                    {
                                        componentId = Regex.Replace(component.GetAttributeValue(filterAttribute).Trim().Replace(" ", string.Empty).ToLower(),
                                                                    attributeReg,
                                                                    ".");
                                        componentId = componentId.Split(".")[0];

                                        if (componentsIdFound.ContainsKey(componentId))
                                        {
                                            int value = componentsIdFound[componentId]; //the value of how many time this component id is found before
                                            componentsIdFound[componentId] += 1; // increment;
                                            componentId += $"-{value}";
                                        }
                                        else
                                        {
                                            componentsIdFound.Add(componentId, 1);
                                        }
                                        writer.WriteStartElement("Component");
                                        writer.WriteAttributeString("id", componentId);
                                        IEnumerable<HtmlNode> conatiner = component.Descendants();
                                        createSourceElement(conatiner, writer, componentId);
                                        writer.WriteEndElement();
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }

                    writer.WriteEndElement();
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                throw new Exception(e.Message);
            }
        }

        private static void createSourceElement(IEnumerable<HtmlNode> conatiner, XmlWriter writer, string parentId)
        {
            Dictionary<string, int> levelChildren = new Dictionary<string, int>();
            Regex rg = new Regex(@"#[a-zA-Z]*|script");
            
 
            foreach (var innerTag in conatiner)
            {
                if (!rg.IsMatch(innerTag.Name.Trim().ToLower()))
                {
                    string idValue = "";

                    if (!foundElements.Contains(innerTag))
                    {
                        foundElements.Add(innerTag);

                        if (innerTag.Name.Trim().ToLower().Equals("div") || innerTag.Name.Trim().ToLower().Equals("section") || innerTag.Name.Trim().ToLower().Equals("article"))
                        {
                            var childrenList = innerTag.ChildNodes.ToList();
                            bool conatinDiv = false;
                            foreach (var child in childrenList)
                            {
                                conatinDiv = child.Name.Trim().ToLower().Equals("div") || child.Name.Trim().ToLower().Equals("section") || child.Name.Trim().ToLower().Equals("article") ? true : false;
                                if (conatinDiv) break;
                            }
                            //The container doesn't contain more containers and not empty.
                            if (!conatinDiv && childrenList.Count > 0 && checkContainer(childrenList))
                            {
                                if (levelChildren.ContainsKey("content"))
                                {
                                    idValue = $"{parentId}_content-{levelChildren["content"]}";
                                    levelChildren["content"]++;
                                }
                                else
                                {
                                    idValue = $"{parentId}_content";
                                    levelChildren.Add("content", 1);

                                }
                                writer.WriteStartElement("content");
                                writer.WriteAttributeString("id", idValue);
                                addRequiredAttributes(writer, innerTag);
                                createSourceElement(innerTag.Descendants(), writer, idValue);
                                writer.WriteEndElement();

                            }
                        }

                        else if (!innerTag.Name.Trim().ToLower().Equals("div"))
                        {
                            if (levelChildren.ContainsKey(innerTag.Name.ToUpper()))
                            {
                                idValue = $"{parentId}_{innerTag.Name.ToUpper()}-{levelChildren[innerTag.Name.ToUpper()]}";
                                levelChildren[innerTag.Name.ToUpper()]++;
                            }
                            else
                            {
                                idValue = $"{parentId}_{innerTag.Name.ToUpper()}";
                                levelChildren.Add(innerTag.Name.ToUpper(), 1);
                            }

                            writer.WriteStartElement(innerTag.Name.Trim().ToLower());
                            writer.WriteAttributeString("id", idValue);
                            addRequiredAttributes(writer, innerTag);

                            if (innerTag.ChildNodes.Count > 1 || (innerTag.ChildNodes.Count == 1 && !innerTag.FirstChild.Name.Equals("#text")))
                            {
                                createSourceElement(innerTag.Descendants(), writer, idValue);
                            }
                            else
                            {
                                writer.WriteString(innerTag.InnerHtml);
                            }
                            writer.WriteEndElement();
                        }
                    }
                }

            }
        }

        private static string extractJsonObject(string jsonName)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\") + "WebsiteConfig.json";
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

        private static bool checkContainer(List<HtmlNode> childrenlist)
        {
            Regex rg = new Regex(@"#[a-zA-Z]*|script|style");
            return (childrenlist.Count == 1 && !rg.IsMatch(childrenlist[0].Name.Trim().ToLower())) ? false : true;
        }

        private static void addRequiredAttributes(XmlWriter writer, HtmlNode htmlNode)
        {
            //required attributes to be added in the source xml elements if found
            string[] neededAttributes = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(extractJsonObject("attributesList"));

            if (htmlNode.HasAttributes)
            {
                //extracting the current html element's attributes
                var htmlAttributes = htmlNode.GetAttributes();
                foreach(HtmlAttribute nodeAttr in htmlAttributes)
                {
                    if (neededAttributes.Contains(nodeAttr.Name.Trim()))
                    {
                        writer.WriteAttributeString(nodeAttr.Name.Trim(), nodeAttr.Value);
                    }
                }
            }

        }
    }
}
