using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Net;
using Wesley.Crawler.SimpleCrawler.Models;

namespace Wesley.Crawler.SimpleCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //测试代理IP是否生效：http://1212.ip138.com/ic.asp

            //测试当前爬虫的User-Agent：http://www.whatismyuseragent.net

            //1.抓取城市
            CityCrawler();

            //2.抓取酒店
            //HotelCrawler();

            //3.并发抓取示例
            //ConcurrentCrawler();

            Console.ReadKey();
        }


        /// <summary>
        /// 抓取城市列表
        /// </summary>
        public static void CityCrawler() {
            
            var cityUrl = "http://hotels.ctrip.com/citylist";//定义爬虫入口URL
            var cityList = new List<City>();//定义泛型列表存放城市名称及对应的酒店URL
            var cityCrawler = new SimpleCrawler();//调用刚才写的爬虫程序
            cityCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };
            cityCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);
            };
            cityCrawler.OnCompleted += (s, e) =>
            {
                //使用正则表达式清洗网页源代码中的数据
                var links = Regex.Matches(e.PageSource, @"<a[^>]+href=""*(?<href>/hotel/[^>\s]+)""\s*[^>]*>(?<text>(?!.*img).*?)</a>", RegexOptions.IgnoreCase);
                foreach (Match match in links)
                {
                    var city = new City
                    {
                        CityName = match.Groups["text"].Value,
                        Uri = new Uri("http://hotels.ctrip.com" + match.Groups["href"].Value
                    )
                    };
                    if (!cityList.Contains(city)) cityList.Add(city);//将数据加入到泛型列表
                    Console.WriteLine(city.CityName + "|" + city.Uri);//将城市名称及URL显示到控制台
                }
                Console.WriteLine("===============================================");
                Console.WriteLine("爬虫抓取任务完成！合计 " + links.Count + " 个城市。");
                Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                Console.WriteLine("线程：" + e.ThreadId);
                Console.WriteLine("地址：" + e.Uri.ToString());
            };
            cityCrawler.Start(new Uri(cityUrl)).Wait();//没被封锁就别使用代理：60.221.50.118:8090
        }



        /// <summary>
        /// 抓取酒店列表
        /// </summary>
        public static void HotelCrawler() {
            var hotelUrl = "http://hotels.ctrip.com/hotel/zunyi558";
            var hotelList = new List<Hotel>();
            var hotelCrawler = new SimpleCrawler();
            hotelCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };
            hotelCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);
            };
            hotelCrawler.OnCompleted += (s, e) =>
            {
                var links = Regex.Matches(e.PageSource, @"""><a[^>]+href=""*(?<href>/hotel/[^>\s]+)""\s*data-dopost[^>]*><span[^>]+>.*?</span>(?<text>.*?)</a>", RegexOptions.IgnoreCase);
                foreach (Match match in links)
                {
                    var hotel = new Hotel
                    {
                        HotelName = match.Groups["text"].Value,
                        Uri = new Uri("http://hotels.ctrip.com" + match.Groups["href"].Value
                    )
                    };
                    if (!hotelList.Contains(hotel)) hotelList.Add(hotel);//将数据加入到泛型列表
                    Console.WriteLine(hotel.HotelName + "|" + hotel.Uri);//将酒店名称及详细页URL显示到控制台
                }

                Console.WriteLine();
                Console.WriteLine("===============================================");
                Console.WriteLine("爬虫抓取任务完成！合计 " + links.Count + " 个酒店。");
                Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                Console.WriteLine("线程：" + e.ThreadId);
                Console.WriteLine("地址：" + e.Uri.ToString());
            };
            hotelCrawler.Start(new Uri(hotelUrl)).Wait();//没被封锁就别使用代理：60.221.50.118:8090
        }


        /// <summary>
        /// 并发抓取示例
        /// </summary>
        public static void ConcurrentCrawler() {
            var hotelList = new List<Hotel>() {
                new Hotel { HotelName="遵义浙商酒店", Uri=new Uri("http://hotels.ctrip.com/hotel/4983680.html?isFull=F") },
                new Hotel { HotelName="遵义森林大酒店", Uri=new Uri("http://hotels.ctrip.com/hotel/1665124.html?isFull=F") },
            };
            var hotelCrawler = new SimpleCrawler();
            hotelCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };
            hotelCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);
            };
            hotelCrawler.OnCompleted += (s, e) =>
            {
                Console.WriteLine();
                Console.WriteLine("===============================================");
                Console.WriteLine("爬虫抓取任务完成！");
                Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                Console.WriteLine("线程：" + e.ThreadId);
                Console.WriteLine("地址：" + e.Uri.ToString());
            };
            Parallel.For(0, 2, (i) =>
            {
                var hotel = hotelList[i];
                hotelCrawler.Start(hotel.Uri);
            });
        }
    }








   



}


