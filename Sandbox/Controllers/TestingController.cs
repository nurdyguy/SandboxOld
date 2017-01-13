using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using Sandbox.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Sandbox.Controllers
{
    public class TestingController : BaseController
    {
        private readonly ISpClient spClient;
        private readonly IRnClient rnClient;

        public TestingController(ISpClient spClient, IRnClient rnClient)
        {
            this.spClient = spClient;
            this.rnClient = rnClient;
        }

        [Route("asdf")]
        public ActionResult Index()
        {
            Parallel.For(1, 1000000, i => GetCombFromID(8843, 3));
            return View();
        }

        //[Route("asdf")]
        public ActionResult RouteTest()
        {
            return View();
        }

        public JsonResult GetPermList(List<int> used)
        {
            if (used == null)
                used = new List<int>();

            if (HttpRuntime.Cache["FullList_52c5"] as List<List<int>> == null)
                HttpRuntime.Cache["FullList_52c5"] = GenFullList(new List<int>(), 5);

            var timer = new List<long>();
            var sw = new Stopwatch();
            sw.Start();
            
            var fullList = GenFullList(used, 5);         
                        
            timer.Add(sw.ElapsedMilliseconds);
            sw.Restart();

            fullList = GenListFromCache(used);
            var x = EncodeCombs(fullList);
            timer.Add(sw.ElapsedMilliseconds);
            sw.Stop();
            var result = new { timer, fullList.Count };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GenHandList()
        {
            var hands = GenFullList(new List<int>(0), 5);
            var encodedHands = new Dictionary<int, List<int>>();
            hands.ForEach(h => encodedHands.Add(GetCombID(h), h));
            var result = new { encodedHands };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ApiTest()
        {
            var result = await spClient.GetResult<ApiTestModel>("v1/artists/671sBQXQM2vHSu0AGvpfDs/top-tracks?country=us");

            var request = new 
            {
                ContentID = 173106,
                LastWatchedOn =	1480804060,
                LastPosition = 110,
                IsCompleted = false
            };  
            var result2 = await rnClient.PostResult<ApiTestModel>("api/atwork/content/recentlywatched/add/173106", request, 4);


            return View();
        }
        
        private List<int> GetNextList(List<int> lastList)
        {
            var sl = new SortedList<int, int>();
            var nextList = new List<int>(lastList);
            for (int i = 0; i < 52; i++)
                sl.Add(i, i);
            
            List<int> cIndexes = new List<int>();
            nextList.ForEach(c => cIndexes.Add(0));
            var length = nextList.Count;

            for (int i = length - 1; i >= 0; i--)
            {
                cIndexes[i] = sl.IndexOfKey(nextList[i]);

                if (cIndexes[i] == sl.Count - 2 - (length - i - 1))
                {
                    nextList[i] = sl[sl.Keys[cIndexes[i] + 1]];
                    return nextList;
                }
                if (cIndexes[i] < sl.Count - 2 - (length - i - 1))
                {
                    nextList[i] = sl[sl.Keys[cIndexes[i] + 1]];
                    // loop forward and assign tail
                    for (int j = i + 1; j < length; j++)
                    {
                        nextList[j] = sl[sl.Keys[cIndexes[i] + 1 + (j - i)]];
                    }
                    return nextList;
                }

            }
            return null;
        }
        
        private List<int> GetNextList(List<int> lastList, SortedList<int, int> unused)
        {
            var sl = new SortedList<int, int>(unused);
            var nextList = new List<int>(lastList);

            List<int> cIndexes = new List<int>();
            nextList.ForEach(c => cIndexes.Add(0));
            var length = nextList.Count;

            for (int i = length - 1; i >= 0; i--)
            {
                cIndexes[i] = sl.IndexOfKey(nextList[i]);

                if (cIndexes[i] == sl.Count - 2 - (length - i - 1))
                {
                    nextList[i] = sl[sl.Keys[cIndexes[i] + 1]];
                    return nextList;
                }
                if (cIndexes[i] < sl.Count - 2 - (length - i - 1))
                {
                    nextList[i] = sl[sl.Keys[cIndexes[i] + 1]];
                    // loop forward and assign tail
                    for (int j = i + 1; j < length; j++)
                    {
                        nextList[j] = sl[sl.Keys[cIndexes[i] + 1 + (j - i)]];
                    }
                    return nextList;
                }

            }
            return null;
        }
        
        private List<List<int>> GenFullList(List<int> omit, int size)
        {
            var unused = new SortedList<int, int>(52);
            var fullList = new List<List<int>>();

            for (int i = 0; i < 52; i++)
                if(!omit.Contains(i))
                    unused.Add(i, i);

            var initList = new List<int>();
            for (var i = 0; i < size; i++)
                initList.Add(i);

            var nextList = initList;
            while (nextList != null)
            {
                fullList.Add(nextList);
                nextList = GetNextList(nextList, unused);
            }

            return fullList;
        } 

        private List<List<int>> GenListFromCache(List<int> omit)
        {
            var fullList = HttpRuntime.Cache["FullList_52c5"] as List<List<int>>;
            var result = new List<List<int>>();

            if (fullList == null)
                return result;
            
            bool found = false;
            for (int i = 0; i < fullList.Count(); i++)
            {
                found = false;
                for (int j = 0; j < omit.Count && !found; j++)
                    if (fullList[i].Contains(omit[j]))
                        found = true;

                if (!found)
                    result.Add(fullList[i]);
            }

            return result;
        }

        private List<int> EncodeCombs(List<List<int>> combs, int max = 51)
        {
            if (!combs.Any())
                return null;
                        
            var l = new List<int>();
            //var cMax = nCr(max + 1, combs.First().Count);
            //Parallel.ForEach(combs, c =>
            combs.ForEach(c =>
            {
                l.Add(GetCombID(c, max));
            });
            
            return l;
        }

        private List<List<int>> DecodeCombs(List<int> codes, int combLength = 2, int max = 51 )
        {
            if (!codes.Any())
                return null;
            var combs = new List<List<int>>();
            codes.ForEach(c =>
            {
                combs.Add(GetCombFromID(c, combLength, max));
            });
            return combs;
        }

        public int GetCombID(List<int> comb, int max = 51)
        {
            UInt64 id = nCr(max + 1, comb.Count);            
            for (int i = 0; i < comb.Count; i++)
                id -= nCr(max - comb[i], comb.Count - i);
            return (int)id;
        }

        public List<int> GetCombFromID(int id, int combLength = 2,  int max = 51)
        {
            List<int> comb = new List<int>(combLength);
            var tId = nCr(max + 1, combLength) - (UInt64)id;
            for(int i = combLength; i > 0; i--)
            {
                UInt64 tVal = 0;
                bool done = false;
                int pos = 0;
                while(!done)
                {
                    var t = nCr(max - pos, i);
                    if (t <= tId)
                    {
                        tVal = t;
                        done = true;
                    }
                    pos++;
                }
                tId -= tVal;
                comb.Add(pos - 1);
            }
            
            return comb;
        }

        private UInt64 nCr(int n, int r)
        {
            if (r > n)
                return 0;
            if (n == 0 || n == r)
                return 1;
            if(n - r < r)
                return partFactorial(n, r) / partFactorial(n-r, 1);
            else
                return partFactorial(n, n-r) / partFactorial(r, 1);
        }

        private UInt64 partFactorial(int x, int y = 1)
        {
            if (x == 0 || x == y)
                return 1;
            if (y > x)
                return 0;
            UInt64 result = 1;
            for (var i = y + 1; i <= x; i++)
                result *= (UInt64)i;
            return result;
        }
    }
}