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
            try
            {
                string url = extractJsonObject("url");
                await getFromWebsite(url, path + "source2.xml");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static async Task getFromWebsite(string url, string path)
        {
            try
            {
                // Read HTML page From Web
                HtmlWeb web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(url);

                //extracting and parsing the website
                HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");
                string filterXPath = extractJsonObject("mainContainer");
                string filterAttribute = extractJsonObject("filteredAttribute");
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

                            if (innerTag.Name.Trim().ToLower().Equals("img"))
                            {
                                writer.WriteAttributeString("src", innerTag.GetAttributeValue("src").Replace(" ", string.Empty).ToLower());
                                writer.WriteAttributeString("alt", innerTag.GetAttributeValue("alt").Replace(" ", string.Empty).ToLower());
                            }
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
            Regex rg = new Regex(@"#[a-zA-Z]*|script");
            return (childrenlist.Count == 1 && !rg.IsMatch(childrenlist[0].Name.Trim().ToLower())) ? false : true;
        }
    }
}
