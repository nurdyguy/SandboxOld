using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace Sandbox.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var timer = new List<long>();
            var list1 = new List<SortedList<int, int>>() { };
            for (int i = 0; i < 2600000; i++)
            {
                list1.Add(new SortedList<int, int>());
                list1[i].Add(i, i);
                list1[i].Add(i + 1, i + 1);
                list1[i].Add(i + 2, i + 2);
                list1[i].Add(i + 3, i + 3);
                list1[i].Add(i + 4, i + 4);
            }
            var list2 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };


            var sw = new Stopwatch();
            sw.Start();
            HttpRuntime.Cache["yourface"] = list1;
            var x = HttpRuntime.Cache["yourface"] as List<SortedList<int, int>>;
            var workingList = new List<SortedList<int, int>>(x).Where(sl => !sl.Values.Contains(20)).ToList();

            timer.Add(sw.ElapsedMilliseconds);
            sw.Restart();
            
            
            bool found = false;            
            for (int i = 0; i < list1.Count(); i++)
                for (int j = 0; j < list2.Count && !found; j++)
                {
                    if (list1[i].ContainsKey(list2[j]))
                        found = true;
                }

            timer.Add(sw.ElapsedMilliseconds);
            sw.Stop();
            return View();

        }

        [HttpPost]        
        public ActionResult Index(FormCollection content)
        {

            return View();
        }   

        public ActionResult RandomStuff()
        {
            return View();
        }
    }
}