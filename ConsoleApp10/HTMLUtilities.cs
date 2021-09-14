using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using System.IO;
using System.Net.Http;
using System.Xml;
using AngleSharp.Dom;
using Nito.AsyncEx;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Reflection;

namespace ConsoleApp10
{
    class HTMLUtilities
    {
        internal static async Task<List<IElement>> reading(List<IElement> ht, string id)
        {
            var path = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"XML\");
            var html = File.ReadAllText(path + @"\test.html");

            var config = Configuration.Default;
            using var context = BrowsingContext.New(config);
            using var doc = await context.OpenAsync(req => req.Content(html));

            var lis = doc.GetElementById(id);

            var lisi = lis.Children;
            foreach (var li in lisi)
            {
                ht.Add(li);
            }

            /*int i = 0;
            while (i < ht.Count)
            {

                var child = ht[i].Children;
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
                    else
                    {
                        for (int k = 0; k < child.Length; k++)
                        {
                            ht.Insert(i + 1 + k, child[k]);

                        }

                        i++;
                    }
                }
                else
                {
                    i++;
                }


            }*/

            return ht;
        }

        internal static void writing(string htmlpath, string xmlpath)
        {
            var destXml = XMLUtilities.loadXML(xmlpath);
            XMLUtilities.readFromDestination(destXml, htmlpath);
        }
    }

    }

