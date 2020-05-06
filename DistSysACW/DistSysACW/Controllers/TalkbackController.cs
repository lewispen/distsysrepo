using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistSysACW.Controllers
{
    public class TalkBackController : BaseController
    {
        /// <summary>
        /// Constructs a TalkBack controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public TalkBackController(Models.UserContext context) : base(context) { }

        [ActionName("Hello")]
        public string Get()
        {
            #region TASK1
            return "Hello World";
            #endregion
        }

        [ActionName("Sort")]
        public IActionResult Get([FromQuery]int[] integers)
        {
            #region TASK1
            var result = integers;
            if (result.Length == 0)
            {
                return Ok(result);
            }
            else
            {
                bool goodResult = true;
                for (int i = 0; i < result.Length; i++)
                {
                    goodResult = (IsDigitsOnly(result[i].ToString()));
                    if (goodResult == false)
                    {
                        return BadRequest();
                    }
                }
                if (goodResult == true)
                {
                    Array.Sort(result);
                    return Ok(result);
                }
            }
            return Ok(result);
            #endregion
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
