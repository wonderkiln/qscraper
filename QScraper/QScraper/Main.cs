using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QScraper
{
    class MainClass
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, arg) =>
            {
                Logger.Error("Unhandled Exception: {0}", arg.ExceptionObject);
            };

            // Verify and create image folders
            if(!Directory.Exists("images")) Directory.CreateDirectory("images");

            var sw = Stopwatch.StartNew();

            Logger.Info("Started generating json file");

            var wootMainTask = Task.Factory.StartNew<List<Dictionary<string, object>>>(ParseWootDeals);
            var dealListTask = Task.Factory.StartNew<List<Dictionary<string, object>>>(ParseDealList);
            var onesaleadayTask = Task.Factory.StartNew<List<Dictionary<string, object>>>(Parse1saleaday);
            var dailyStealsTask = Task.Factory.StartNew<List<Dictionary<string, object>>>(ParseDailyStealsDeals);
            var amazonGoldBoxTask = Task.Factory.StartNew<Dictionary<string, object>>(ParseAmazonGoldBox);
            var newEggShellShockersTask = Task.Factory.StartNew<List<Dictionary<string, object>>>(ParseNeweggShellShocker);
            var steepandCheapTask = Task.Factory.StartNew<Dictionary<string, object>>(ParseSteepandCheap);
            var geeksTask = Task.Factory.StartNew<Dictionary<string, object>>(ParseGeeks);
            var yugsterTask = Task.Factory.StartNew<List<Dictionary<string, object>>>(ParseYugster);

            Task.WaitAll(wootMainTask, dealListTask, dailyStealsTask, onesaleadayTask, amazonGoldBoxTask, newEggShellShockersTask, steepandCheapTask, geeksTask, yugsterTask);

            var allDeals = new Dictionary<string, object>();

            // Woot
            if (wootMainTask.Result.Count == 8)
            {
                allDeals.Add("woot.main", wootMainTask.Result[0]);
                allDeals.Add("woot.home", wootMainTask.Result[1]);
                allDeals.Add("woot.kids", wootMainTask.Result[2]);
                allDeals.Add("woot.wine", wootMainTask.Result[3]);
                allDeals.Add("woot.shirt", wootMainTask.Result[4]);
                allDeals.Add("woot.sellout", wootMainTask.Result[5]);
                allDeals.Add("woot.tech", wootMainTask.Result[6]);
                allDeals.Add("woot.sport", wootMainTask.Result[7]);
            }
            allDeals.Add("deals.list", dealListTask.Result);

            // 1 Sale A Day
            if (onesaleadayTask.Result.Count == 5)
            {
                allDeals.Add("1saleaday.main", onesaleadayTask.Result[0]);
                allDeals.Add("1saleaday.wireless", onesaleadayTask.Result[1]);
                allDeals.Add("1saleaday.watch", onesaleadayTask.Result[2]);
                allDeals.Add("1saleaday.family", onesaleadayTask.Result[3]);
                allDeals.Add("1saleaday.jewelry", onesaleadayTask.Result[4]);
            }

            // Daily Steals
            if (dailyStealsTask.Result.Count == 5)
            {
                allDeals.Add("dailysteals.main", dailyStealsTask.Result[0]);
                allDeals.Add("dailysteals.mobile", dailyStealsTask.Result[1]);
                allDeals.Add("dailysteals.home", dailyStealsTask.Result[2]);
                allDeals.Add("dailysteals.toys", dailyStealsTask.Result[3]);
                allDeals.Add("dailysteals.lastcall", dailyStealsTask.Result[4]);
            }

            // Electronics
            allDeals.Add("shellshocker.list", newEggShellShockersTask.Result);
            allDeals.Add("geeks", geeksTask.Result);

            // Others
            allDeals.Add("amazongoldbox", amazonGoldBoxTask.Result);
            allDeals.Add("steepandcheap", steepandCheapTask.Result);
            if (yugsterTask.Result.Count == 3)
            {
                allDeals.Add("yugster.tdeal", yugsterTask.Result[0]);
                allDeals.Add("yugster.yug", yugsterTask.Result[1]);
                allDeals.Add("yugster.twatch", yugsterTask.Result[2]);
            }

            ReplaceLinks(allDeals);

            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var outputFn = Path.Combine(exeDir, "data.json");

            var serialiedData = JsonConvert.SerializeObject(allDeals);

            if (!IsContentEqual(outputFn, serialiedData))
            {
                File.WriteAllText(outputFn, serialiedData);
            }

            sw.Stop();
            Logger.Info("Completed in {0} seconds", sw.Elapsed.TotalSeconds);

            // Fix for mono.
            LogManager.Configuration = null;
        }

        private static bool IsContentEqual(string filePath, string data)
        {
            if (!File.Exists(filePath)) return false;

            var fStream = new FileStream(filePath, FileMode.Open);
            var sReader = new StreamReader(fStream);
            var fData = sReader.ReadToEnd();

            sReader.Close();
            sReader.Dispose();

            fStream.Close();
            fStream.Dispose();

            return data.Equals(fData);
        }

        private static void ReplaceLinks(Dictionary<string, dynamic> allDeals)
        {
            allDeals["woot.main"]["URL"] = "http://www.tkqlhce.com/aj103r09608OVQRQPPUOQPXSVXQX";
            allDeals["woot.home"]["URL"] = "http://www.kqzyfj.com/2i108mu2-u1HOJKJIINHJIRNOIJR";
            allDeals["woot.wine"]["URL"] = "http://www.kqzyfj.com/hi117uoxuowBIDEDCCHBDCKEDHGG?url=http%3A%2F%2Fwine.woot.com";
            allDeals["woot.sellout"]["URL"] = "http://www.jdoqocy.com/pe122lnwtnvAHCDCBBGACBJEHJFE";
            allDeals["woot.shirt"]["URL"] = "http://www.kqzyfj.com/a2110y1A719PWRSRQQVPRQYTWYTX";
            allDeals["woot.kids"]["URL"] = "http://www.anrdoezrs.net/5b106js0ys-FMHIHGGLFHGOIHLKH";
            allDeals["woot.tech"]["URL"] = "http://www.kqzyfj.com/3b111qgpmgo3A56544935554946B";
            allDeals["woot.sport"]["URL"] = "http://www.tkqlhce.com/dd106r09608OVQRQPPUOQQPXRWYY";

            allDeals["1saleaday.main"]["URL"] = "http://www.tkqlhce.com/3566kjspjr6D89877C6888DBFB8";
            allDeals["1saleaday.wireless"]["URL"] = "http://www.kqzyfj.com/r779vpyvpxCJEFEDDICEDMHGLIG";
            allDeals["1saleaday.watch"]["URL"] = "http://www.jdoqocy.com/19108gv30v2IPKLKJJOIKJSNMRNK";
            allDeals["1saleaday.family"]["URL"] = "http://www.jdoqocy.com/f8116gv30v2IPKLKJJOIKJSNMRMR";
            allDeals["1saleaday.jewelry"]["URL"] = "http://www.dpbolvw.net/jm70vpyvpxCJEFEDDICEDMHGLHL";

            allDeals["dailysteals.main"]["URL"] = "http://www.anrdoezrs.net/ch103lnwtnvAHCDCBBGACCCJICFE";
            allDeals["dailysteals.mobile"]["URL"] = "http://www.jdoqocy.com/ak108hz74z6MTOPONNSMOOOVUORT";
            allDeals["dailysteals.home"]["URL"] = "http://www.tkqlhce.com/od98r09608OVQRQPPUOQQQXWQTY";
            allDeals["dailysteals.toys"]["URL"] = "http://www.jdoqocy.com/ih117xdmjdl07232116022298318";
            allDeals["dailysteals.lastcall"]["URL"] = "http://www.tkqlhce.com/gg77mu2-u1HOJKJIINHJJJQPKIQ";

            allDeals["amazongoldbox"]["URL"] = "http://www.amazon.com/gp/goldbox/?ie=UTF8&camp=1789&creative=390957&linkCode=ur2&tag=dealflux-20";
            allDeals["geeks"]["URL"] = "http://www.dpbolvw.net/q997ar-xrzELGHGFFKEGFKINMHL";

            foreach (var ss in allDeals["shellshocker.list"])
            {
                ss["URL"] = "http://www.kqzyfj.com/click-6121005-10467856";
            }
        }

        // Woot
        private static List<Dictionary<string, object>> ParseWootDeals()
        {
            var urls = new string[] {
                "http://api.woot.com/1/sales/current.rss/www.woot.com",
                "http://api.woot.com/1/sales/current.rss/home.woot.com",
                "http://api.woot.com/1/sales/current.rss/kids.woot.com",
                "http://api.woot.com/1/sales/current.rss/wine.woot.com",
                "http://api.woot.com/1/sales/current.rss/shirt.woot.com",
                "http://api.woot.com/1/sales/current.rss/sellout.woot.com",
                "http://api.woot.com/1/sales/current.rss/tech.woot.com",
                "http://api.woot.com/1/sales/current.rss/sport.woot.com"
            };

            var items = new List<Dictionary<string, object>>();
            foreach (var url in urls)
            {
                var currRet = ParseOneWootDeal(url);
                items.Add(currRet);
            }

            return items;
        }

        private static Dictionary<string, object> ParseOneWootDeal(string url)
        {
            var item = new Dictionary<string, object>() {
                {"URL", ""},
                {"name", ""},
                {"price", ""},
                {"condition", ""},
                {"photo", ""},
                {"shipping_cost", ""},
                {"woot_off", ""},
                {"sold_out", ""},
                {"sold_out_percentage", ""},
                {"description", ""}
            };

            var body = DownloadString(url);
            if (body == null)
            {
                Logger.Error("Error while downloading Woot deal");
                return item;
            }

            var xdoc = XDocument.Parse(body);
            var root = xdoc.Root.Element("channel");
            if (root == null) return item;

            try
            {
                var el = root.Element("item");
                var urlEle = el.Element("link");
                var nameEle = el.Element("title");
                var priceEle = el.Element("{http://www.woot.com/}price");
                var conditionEle = el.Element("{http://www.woot.com/}condition");
                var photoEle = el.Element("{http://www.woot.com/}standardimage");
                var shippingCostEle = el.Element("{http://www.woot.com/}shipping");
                var wootOffEle = el.Element("{http://www.woot.com/}wootoff");
                var soldOutEle = el.Element("{http://www.woot.com/}soldout");
                var soldOutPerEle = el.Element("{http://www.woot.com/}soldoutpercentage");
                var desEle = el.Element("description");

                if (urlEle != null) item["URL"] = urlEle.Value;
                if (nameEle != null) item["name"] = nameEle.Value;
                if (priceEle != null) item["price"] = priceEle.Value;
                if (conditionEle != null) item["condition"] = conditionEle.Value;
                if (photoEle != null) item["photo"] = photoEle.Value;
                if (shippingCostEle != null) item["shipping_cost"] = shippingCostEle.Value;
                if (wootOffEle != null) item["woot_off"] = wootOffEle.Value;
                if (soldOutEle != null) item["sold_out"] = soldOutEle.Value;
                if (soldOutPerEle != null) item["sold_out_percentage"] = soldOutPerEle.Value;
                if (desEle != null) item["description"] = ParseWootDescription(desEle);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when parsing Woot deal: {0}", ex.Message);
            }

            return item;
        }

        private static Dictionary<string, object> ParseWootDescription(XElement desEle)
        {
            var content = "";
            if (desEle != null) content = WebUtility.HtmlDecode(desEle.Value);

            var hDoc = new HtmlDocument();
            hDoc.LoadHtml(content);
            if (hDoc.ParseErrors != null && hDoc.ParseErrors.Count() > 0)
            {
                // Parse error.
                return null;
            }

            var textArr = hDoc.DocumentNode.InnerText
                .Split(new string[] { "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .Select(str => str.Trim());

            var intoDetail = false;
            var bodyText = new StringBuilder();
            string currDetailHeader = null;
            var currDetailText = new StringBuilder();
            var detailText = new List<object>();
            foreach (var text in textArr)
            {
                if (text.EndsWith(":"))
                {
                    if (currDetailHeader != null)
                    {
                        detailText.Add(new
                        {
                            section = currDetailHeader,
                            body = currDetailText.ToString()
                        });
                        currDetailText.Clear();
                    }
                    currDetailHeader = text;
                    intoDetail = true;
                    continue;
                }
                if (!intoDetail)
                {
                    bodyText.AppendLine(text);
                }
                else
                {
                    currDetailText.AppendLine(text);
                }
            }
            if (currDetailHeader != null)
            {
                detailText.Add(new
                {
                    section = currDetailHeader,
                    body = currDetailText.ToString()
                });
            }

            return new Dictionary<string, object>() {
                {"body", bodyText.ToString()},
                {"details", detailText}
            };
        }

        private static List<Dictionary<string, object>> ParseDealList()
        {
            var items = new List<Dictionary<string, object>>();

            var body = DownloadString("http://deals.woot.com/deals.rss/");
            if (body == null)
            {
                Logger.Error("Error while downloading deal list");
                return items;
            }

            var xdoc = XDocument.Parse(body);
            var root = xdoc.Root.Element("channel");
            if (root == null) return items;

            foreach (var item in root.Elements("item"))
            {
                try
                {
                    var titleEle = item.Element("title");
                    var linkEle = item.Element("link");
                    var thumbnail = item.Element("{http://search.yahoo.com/mrss/}group");

                    var photoEle = thumbnail != null ?
                        thumbnail.Elements("{http://search.yahoo.com/mrss/}content").FirstOrDefault(e =>
                        {
                            return e.Attribute("height").Value == "200";
                        }) : null;

                    var photoUrl = photoEle != null ? photoEle.Attribute("url") : null;

                    items.Add(new Dictionary<string, object>() {
                        {"name", titleEle != null ? titleEle.Value : ""},
                        {"URL", linkEle != null ? linkEle.Value : ""},
                        {"photo", photoUrl != null ? photoUrl.Value : ""},
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception when parsing deal list item: {0}", ex.Message);
                }
            }

            return items;
        }

        // 1 Sale A Day
        private static List<Dictionary<string, object>> Parse1saleaday()
        {
            var items = new List<Dictionary<string, object>>();

            var body = DownloadString("http://1saleaday.com/rss_feedburner.asp");
            if (body == null)
            {
                Logger.Error("Error while downloading 1 Sale A Day deals");
                return items;
            }

            var xdoc = XDocument.Parse(body);
            var root = xdoc.Root.Element("channel");
            var itemEles = root.Elements("item");
            if (itemEles == null) return items;

            foreach (var item in itemEles)
            {
                try
                {
                    var titleEle = item.Element("title");
                    var linkEle = item.Element("link");
                    var desEle = item.Element("description");
                    var desDoc = new HtmlDocument();
                    desDoc.LoadHtml(desEle.Value);
                    var pNode = desDoc.DocumentNode.SelectSingleNode("//p");
                    var firstImg = desDoc.DocumentNode.SelectSingleNode("//img");

                    var price = "";
                    var title = "";
                    if (titleEle != null)
                    {
                        var arr = titleEle.Value.Split(new string[] { "  " }, StringSplitOptions.None);
                        if (arr.Count() == 2)
                        {
                            title = arr[0];
                            price = arr[1].Trim().Trim('[', ']');
                        }
                    }
                    items.Add(new Dictionary<string, object>(){
                        {"name", title},
                        {"price", price},
                        {"URL", linkEle != null ? linkEle.Value : ""},
                        {"photo", firstImg != null ? firstImg.GetAttributeValue("src", "") : ""},
                        {"description", pNode != null ? WebUtility.HtmlDecode(pNode.InnerText) : ""}
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception when parsing 1 Sale A Day item: {0}", ex.Message);
                    return items;
                }
            }

            return items;
        }

        //Daily Steals
        private static List<Dictionary<string, object>> ParseDailyStealsDeals()
        {
            var urls = new string[][] {
                new string[] { "http://www.dailysteals.com/rss", "dailysteals.main" },
                new string[] { "http://mobile.dailysteals.com/rss", "dailysteals.mobile" },
                new string[] { "http://home.dailysteals.com/rss", "dailysteals.home" },
                new string[] { "http://toys.dailysteals.com/rss", "dailysteals.toys" },
                new string[] { "http://lastcall.dailysteals.com/rss", "dailysteals.lastcall" }
            };

            var items = new List<Dictionary<string, object>>();
            foreach (var url in urls)
            {
                var currRet = ParseOneDailyStealsDeal(url[0], url[1]);
                items.Add(currRet);
            }

            return items;
        }

        private static Dictionary<string, object> ParseOneDailyStealsDeal(string url, string name)
        {
            var item = new Dictionary<string, object>() {
                {"name", ""},
                {"URL", ""},
                {"description", ""}
            };

            var body = DownloadString(url);
            if (body == null) return item;

            /*var hDoc = new HtmlDocument();
            hDoc.OptionAutoCloseOnEnd = true;
            hDoc.LoadHtml(body);*/

            XDocument xdoc = null;
            try { xdoc = XDocument.Parse(body); }
            catch { return item; }

            var channelEle = xdoc.Root.Element("channel");
            if (channelEle == null) return item;

            try
            {
                // HTML
                /*var contentFrame = hDoc.DocumentNode.SelectSingleNode("//div[@id='content']/div[@class='mainproduct']");
                var img = contentFrame.SelectSingleNode("./a[@class='productimage']/img");
                var title = contentFrame.SelectSingleNode("./div[@class='details']/h2");
                var shipping = contentFrame.SelectSingleNode("./div[@class='details']/ul[@class='pricelist']/li[4]/div");
                var price = contentFrame.SelectSingleNode("./div[@class='details']/div[@class='yourprice']/strong");
                var desc = hDoc.DocumentNode.SelectSingleNode("//div[@id='content']/div[@class='bottom']/div[@class='description']//div[@class='text']");

                item["URL"] = url;
                if (title != null) item["name"] = title.InnerText.Trim();
                if (price != null) item["price"] = price.InnerText;
                if (img != null) item["photo"] = img.GetAttributeValue("src", "");
                if (desc != null) item["description"] = desc.InnerText.Trim();
                if (shipping != null) item["shipping_cost"] = shipping.InnerText.ToLower();*/

                var el = channelEle.Element("item");
                var nameEle = el.Element("title");
                var urlEle = el.Element("link");
                var desEle = el.Element("description");
                var tempRet = ParseOneDailyStealsHtmlDeal(urlEle.Value, name);

                if (nameEle != null) item["name"] = nameEle.Value;
                if (urlEle != null) item["URL"] = urlEle.Value;
                if (desEle != null) item["description"] = ParseWootDescription(desEle);
                foreach (var e in tempRet) item.Add(e.Key, e.Value);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when parsing DailySteals item: {0}", ex.Message);
            }

            return item;
        }

        private static Dictionary<string, object> ParseOneDailyStealsHtmlDeal(string url, string name)
        {
            var item = new Dictionary<string, object>() {
                {"price", ""},
                {"photo", ""},
            };

            var body = DownloadString(url);
            if (body == null) return item;

            var hDoc = new HtmlDocument();
            hDoc.OptionAutoCloseOnEnd = true;
            hDoc.LoadHtml(body);

            try
            {
                var mainProd = hDoc.DocumentNode.SelectSingleNode("//div[@class='mainproduct']");
                var img = mainProd.SelectSingleNode(".//a[@class='productimage']/img");
                var finalPrice = mainProd.SelectSingleNode(".//div[@class='details']/div[@class='yourprice']/strong");

                if (finalPrice != null) item["price"] = WebUtility.HtmlDecode(finalPrice.FirstChild.InnerText.Trim());
                if (img != null)
                {
                    item["photo"] = img.GetAttributeValue("src", "");

                    // Save image to file
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(item["photo"].ToString(), "images/" + name + ".jpg");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when parsing DailyStealsHtml: {0}", ex.Message);
            }

            return item;
        }

        // Amazon Gold Box
        private static Dictionary<string, object> ParseAmazonGoldBox()
        {
            var item = new Dictionary<string, object>() {
                {"name", ""},
                {"URL", ""},
                {"price", ""},
                {"photo", ""},
                {"description", ""}
            };

            var body = DownloadString("http://www.amazon.com/gp/goldbox");
            if (body == null)
            {
                Logger.Error("Download error (Amazon Goldbox)");
                return item;
            }

            var hDoc = new HtmlDocument();
            hDoc.OptionAutoCloseOnEnd = true;
            hDoc.LoadHtml(body);

            try
            {
                var contentFrame = hDoc.DocumentNode.SelectSingleNode("//div[@class='gbox-framed-contentC']");
                var title = contentFrame.SelectSingleNode(".//div[@class='gbox-dotd-container']/a");
                var img = contentFrame.SelectSingleNode(".//img[@class='gbox-img']");
                var des = contentFrame.SelectSingleNode(".//div[@class='gbox-dotd-container']/span/div");
                var listPrice = contentFrame.SelectSingleNode(".//span[@id='gbox-dotd-list-price']");
                var preProPrice = contentFrame.SelectSingleNode(".//span[@id='gbox-dotd-pre-promo-price']");
                var gbDiscount = contentFrame.SelectSingleNode(".//span[@id='gbox-dotd-discount']");
                var finalPrice = contentFrame.SelectSingleNode(".//span[@id='gbox-dotd-promo-price']");
                var rating = contentFrame.SelectSingleNode(".//span[@class='crAvgStars']/span/a/img");
                var nRates = contentFrame.SelectSingleNode(".//span[@class='crAvgStars']/a");

                var sb = new StringBuilder();
                sb.AppendLine(des.InnerText.Trim());
                if (listPrice != null)
                {
                    sb.AppendLine(string.Format("List price: {0}", WebUtility.HtmlDecode(listPrice.InnerText.Trim())));
                }
                if (preProPrice != null)
                {
                    sb.AppendLine(string.Format("Yesterday's Price: {0}", WebUtility.HtmlDecode(preProPrice.InnerText.Trim())));
                }
                if (gbDiscount != null)
                {
                    sb.AppendLine(string.Format("Today's Discount: {0}", WebUtility.HtmlDecode(gbDiscount.InnerText.Trim())));
                }
                // Fix for incorrect close tag of final price
                if (finalPrice != null)
                {
                    sb.AppendLine(string.Format("Gold Box Price: {0}", WebUtility.HtmlDecode(finalPrice.FirstChild.InnerText.Trim())));
                }
                if (rating != null && nRates != null)
                {
                    sb.AppendLine(string.Format("Rate: {0} by {1} customers.", rating.GetAttributeValue("title", ""), nRates.InnerText));
                }


                if (title != null) item["name"] = WebUtility.HtmlDecode(title.InnerText);
                if (title != null) item["URL"] = title.GetAttributeValue("href", "");
                if (finalPrice != null) item["price"] = WebUtility.HtmlDecode(finalPrice.FirstChild.InnerText.Trim());
                if (img != null) item["photo"] = img.GetAttributeValue("src", "");

                var desc = sb.ToString();
                var index = desc.IndexOf("&nbsp;");
                if (index != -1) desc = desc.Substring(0, index);
                item["description"] = desc;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when parsing Amazon Goldbox: {0}", ex.Message);
            }

            return item;
        }

        // Newegg
        private static List<Dictionary<string, object>> ParseNeweggShellShocker()
        {
            var ret = new List<Dictionary<string, object>>();

            var body = DownloadString("http://www.ows.newegg.com/Promotions.egg/ShellShockers");
            if (body == null)
            {
                Logger.Error("Error while downloading NeweggShellShockers deals");
                return ret;
            }

            JToken ssTok = JRaw.Parse(body);

            foreach (var childToken in ssTok)
            {
                try
                {
                    var num = childToken["ShellShockItemNumber"];
                    var type = childToken["ShellShockerItemType"];

                    var itemNum = "";
                    var desText = "";
                    switch (type.ToString())
                    {
                        case "0":
                            // Item.
                            itemNum = num.ToString();
                            break;
                        case "3":
                            // Item.
                            itemNum = num.ToString();
                            break;
                        case "1":
                            {
                                // Combo.
                                var comboContent = "";
                                try
                                {
                                    comboContent = DownloadString("http://www.ows.newegg.com/Combos.egg/" + num.ToString() + "/DealsDetail/");
                                    var comboJTok = JRaw.Parse(comboContent);

                                    itemNum = comboJTok["ComboProductList"][0]["ItemNumber"].ToString();
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error("Exception parsing nss combo: {0}", ex.Message);
                                    Logger.Error("Combo content: {0}", comboContent);
                                    continue;
                                }
                                break;
                            }
                        case "2":
                            {
                                // Bundle.
                                var bundleContent = "";
                                try
                                {
                                    bundleContent = DownloadString("http://www.ows.newegg.com/Combos.egg/" + num.ToString() + "/BundleDetail/");
                                    var comboJTok = JRaw.Parse(bundleContent);
                                    itemNum = comboJTok["ComboProductList"][0]["ItemNumber"].ToString();
                                    var sb = new StringBuilder();
                                    sb.AppendLine(comboJTok["Title"].ToString());
                                    sb.AppendLine(comboJTok["PromotionText"].ToString());
                                    sb.AppendLine(comboJTok["ComboPriceAfterMIRDescription"].ToString());
                                    sb.AppendLine("Include:");
                                    var iiIdx = 1;
                                    foreach (var includedItem in comboJTok["ImageList"])
                                    {
                                        sb.AppendLine(string.Format("{0}. {1}", iiIdx.ToString(), includedItem["Title"].ToString()));
                                        iiIdx++;
                                    }
                                    desText = sb.ToString();
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error("Exception parsing nss bundle: {0}", ex.Message);
                                    Logger.Error("Bundle content: {0}", bundleContent);
                                    continue;
                                }
                                break;
                            }
                        default:
                            throw new InvalidDataException();
                    }

                    var specContent = "";
                    var prodDetailsContent = "";
                    JToken specJTok = null;
                    JToken pdJTok = null;

                    try
                    {
                        specContent = DownloadString("http://www.ows.newegg.com/products.egg/" + itemNum + "/Specification");
                        specJTok = JRaw.Parse(specContent);

                        prodDetailsContent = DownloadString("http://www.ows.newegg.com/products.egg/" + itemNum + "/ProductDetails");
                        pdJTok = JRaw.Parse(prodDetailsContent);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception parsing nss item: {0}", ex.Message);
                        Logger.Error("Item spec content: {0}", specContent);
                        Logger.Error("Item prod details content: {0}", prodDetailsContent);
                        continue;
                    }

                    if (desText == "")
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine(pdJTok["Title"].ToString());
                        sb.AppendLine(string.Format("Limited {0} per customer", pdJTok["LimitQuantity"].ToString()));
                        sb.AppendLine(string.Format("Original price: {0}", pdJTok["OriginalPrice"].ToString()));
                        sb.AppendLine(string.Format("Discount: {0}", pdJTok["Discount"].ToString()));
                        sb.AppendLine(string.Format("Final price: {0}", pdJTok["FinalPrice"].ToString()));
                        sb.AppendLine(string.Format("Review: {0}/5 - {1} reviews",
                            pdJTok["ReviewSummary"]["Rating"].ToString(),
                            pdJTok["ReviewSummary"]["TotalReviews"].ToString()));
                        desText = sb.ToString();
                    }
                    var specList = new List<object>();
                    foreach (var specTok in specJTok["SpecificationGroupList"])
                    {
                        var specKVList = new List<object>();
                        foreach (var specPair in specTok["SpecificationPairList"])
                        {
                            specKVList.Add(new
                            {
                                key = specPair["Key"].ToString(),
                                value = specPair["Value"].ToString()
                            });
                        }
                        specList.Add(new
                        {
                            category = specTok["GroupName"].ToString(),
                            items = specKVList
                        });
                    }

                    ret.Add(new Dictionary<string, object>() {
                        {"name", specJTok["Title"].ToString()},
                        {"URL", "http://www.newegg.com/Product/Product.aspx?Item=" + specJTok["NeweggItemNumber"].ToString()},
                        {"photo", pdJTok["Image"]["FullPath"].ToString()},
                        {"price", pdJTok["FinalPrice"].ToString()},
                        {"shipping_cost", pdJTok["ShippingInfo"]["NormalShippingText"].ToString()},
                        {"details", new {
                            description = desText,
                            specs = specList,
                        }}
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception when parsing NeweggShellShockers: {0}", ex.Message);
                }
            }

            return ret;
        }

        // Steep and Cheap
        private static Dictionary<string, object> ParseSteepandCheap()
        {
            var item = new Dictionary<string, object>() {
                {"name", ""},
                {"URL", ""},
                {"photo", ""},
                {"price", ""},
                {"description", ""},
            };

            var url = "http://www.avantlink.com/api.php?module=DotdFeed&affiliate_id=52911&merchant_id=10268&website_id=69623&custom_tracking_code=&output=xml";
            var body = DownloadString(url);
            if (body == null)
            {
                Logger.Error("Error while downloading Steep and Cheap deals");
                return item;
            }

            var xdoc = XDocument.Parse(body);
            var root = xdoc.Root.Element("Table1");
            if (root == null) return item;

            try
            {
                var titleEle = root.Element("Product_Name");
                var descEle = root.Element("Long_Description");
                var linkEle = root.Element("Buy_URL");
                var priceEle = root.Element("Sale_Price");
                var imageEle = root.Element("Large_Image_URL");

                if (titleEle != null) item["name"] = titleEle.Value;
                if (linkEle != null) item["URL"] = linkEle.Value;
                if (imageEle != null) item["photo"] = imageEle.Value;
                if (priceEle != null) item["price"] = "$" + priceEle.Value;
                if (descEle != null) item["description"] = HtmlEntity.DeEntitize(descEle.Value);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when parsing Steep and Cheap: {0}", ex.Message);
            }

            return item;
        }

        // Geeks
        private static Dictionary<string, object> ParseGeeks()
        {
            var item = new Dictionary<string, object>() {
                {"name", ""},
                {"URL", ""},
                {"photo", ""},
                {"price", ""},
                {"description", ""},
            };

            var body = DownloadString("http://www.geeks.com/one-day-deal/");
            if (body == null)
            {
                Logger.Error("Error while downloading Geeks deals");
                return item;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            try
            {
                var el = doc.DocumentNode.SelectSingleNode("//body/div[2]/table//table[1]//td[3]");
                var nameEle = el.SelectSingleNode("./table[2]/tr[1]/td");
                var photoEle = el.SelectSingleNode("./table[2]//img");
                var priceEle = el.SelectSingleNode(".//td[@class='checkoutprice']/font");

                var li = new List<object>();

                var descEle = el.SelectNodes("./table[3]//td[2]/ul//li");
                var desc = "";
                if (descEle != null && descEle.Count > 2)
                {
                    desc = HtmlEntity.DeEntitize(descEle[2].InnerText).Trim();
                    desc = desc.Remove(desc.LastIndexOf(Environment.NewLine));
                }

                descEle = el.SelectNodes("./table[3]//td[1]//tr");
                if (descEle != null)
                {
                    foreach (var childItem in descEle)
                    {
                        var childs = childItem.SelectNodes(".//td");
                        if (childs.Count >= 2)
                        {
                            li.Add(new
                            {
                                section = HtmlEntity.DeEntitize(childs[0].InnerText).Trim(),
                                body = HtmlEntity.DeEntitize(childs[1].InnerText).Trim()
                            });
                        }
                    }
                }

                if (nameEle != null) item["name"] = HtmlEntity.DeEntitize(nameEle.InnerText).Trim();
                if (photoEle != null) item["photo"] = "http://www.geeks.com" + photoEle.GetAttributeValue("src", "");
                if (priceEle != null) item["price"] = priceEle.InnerText;
                item["description"] = new Dictionary<string, object>()
                {
                    {"body", desc}, 
                    {"details", li}
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when parsing Geeks: {0}", ex.Message);
            }

            return item;
        }

        // Yugster
        private static List<Dictionary<string, object>> ParseYugster()
        {
            string[] urls = new string[]
            {
                "http://www.yugster.com/todays-deals/daily-offer",
                "http://www.yugster.com/todays-deals/yours-until-gone",
                "http://www.yugster.com/todays-deals/daily-watch-deal"
            };

            var items = new List<Dictionary<string, object>>();
            foreach (var url in urls)
            {
                var item = ParseOneYugsterDeal(url);
                items.Add(item);
            }

            return items;
        }

        private static Dictionary<string, object> ParseOneYugsterDeal(string url)
        {
            var item = new Dictionary<string, object>() {
                {"name", ""},
                {"URL", ""},
                {"photo", ""},
                {"price", ""},
                {"description", ""},
                {"condition", ""},
                {"shipping_cost", ""}
            };

            var body = DownloadString(url);
            if (body == null)
            {
                Logger.Error("Error while downloading one Tugster deal");
                return item;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            try
            {
                var nameEle = doc.DocumentNode.SelectSingleNode("//div[@class='hl_product_details_inner']//h1");
                var photoEle = doc.DocumentNode.SelectSingleNode("//div[@id='hl_product']//img");
                var priceEle = doc.DocumentNode.SelectSingleNode("//span[@class='price']");
                var shipEle = doc.DocumentNode.SelectSingleNode("//span[@class='ships_for']");
                var condEle = doc.DocumentNode.SelectSingleNode("//span[@class='condition']");

                var desc = new Dictionary<string, object>
                {
                    {"body", ""},
                    {"details", ""}
                };

                var descEle = doc.DocumentNode.SelectSingleNode("//div[@id='description_tab']");
                if (descEle != null)
                {
                    var nodes = descEle.SelectNodes(".//p");
                    if (nodes != null)
                    {
                        var sb = new StringBuilder();
                        foreach (var descItem in nodes)
                        {
                            sb.AppendLine(HtmlEntity.DeEntitize(descItem.InnerText).Trim());
                        }
                        desc["body"] = sb.ToString();
                    }

                    nodes = descEle.SelectNodes(".//tr");
                    if (nodes != null)
                    {
                        var li = new List<Dictionary<string, object>>();
                        foreach (var descItem in nodes)
                        {
                            var sc = descItem.ChildNodes;
                            if (sc.Count >= 2)
                            {
                                li.Add(new Dictionary<string, object>()
                                {
                                    {"section", HtmlEntity.DeEntitize(sc[0].InnerText).Trim()},
                                    {"body", HtmlEntity.DeEntitize(sc[1].InnerText).Trim()}
                                });
                            }
                        }
                        desc["details"] = li;
                    }
                }

                if (nameEle != null) item["name"] = HtmlEntity.DeEntitize(nameEle.InnerText).Trim();
                if (photoEle != null) item["photo"] = "http://www.yugster.com" + photoEle.GetAttributeValue("src", "");
                if (priceEle != null) item["price"] = priceEle.InnerText;
                if (shipEle != null)
                {
                    var ship = shipEle.InnerText;
                    var index = ship.IndexOf(':');
                    if (index != -1) ship = ship.Substring(index + 2);
                    item["shipping_cost"] = ship.Trim();
                }
                if (condEle != null)
                {
                    var cond = condEle.InnerText;
                    var index = cond.IndexOf(':');
                    if (index != -1) cond = cond.Substring(index + 2);
                    item["condition"] = cond.Trim();
                }
                item["URL"] = url;
                item["description"] = desc;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception when parsing one Yugster deal: {0}", ex.Message);
            }

            return item;
        }

        // Download method
        private static string DownloadString(string url)
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.ContentEncoding, "UTF-8");
                try
                {
                    var data = client.DownloadString(url);
                    data = data.Replace((char)0x1F, ' ');
                    return data;
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("[Download error from '{0}'] {1}", url, ex.Message));
                    return null;
                }
            }
        }
    }
}
