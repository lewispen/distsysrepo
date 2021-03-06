﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistSysACW.Controllers
{
    [ApiController]
    [Route("api/[Controller]/[Action]")]
    public class UserController : BaseController
    {
        public UserController(Models.UserContext context) : base(context) { }

        [ActionName("New")]
        [HttpGet]
        public ActionResult GetUser([FromQuery]string username)
        {
            #region TASK4
            string type = "Get";
            string result = Models.UserDatabaseAccess.NewUser(username, type);
            return Ok(result);
            #endregion
        }

        [ActionName("New")]
        [HttpPost]
        public ActionResult PostUser([FromBody]string username)
        {
            #region TASK4
            string type = "Post";
            var result = Models.UserDatabaseAccess.NewUser(username, type);
            if (result == "400")
            {
                return BadRequest("Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json");
            }
            else if (result == "403")
            {
                return StatusCode(403, "Oops. This username is already in use. Please try again with a new username.");
            }
            else
            {
                return Ok(result);
            }
            #endregion
        }

        [ActionName("RemoveUser")]
        [HttpDelete]
        [Authorize(Roles = "admin,user")]
        public ActionResult RemoveUser([FromQuery]string username)
        {
            #region TASK4
            string type = "ApiKey";
            Request.Headers.TryGetValue(type, out var value);

            string vSplit = value.ToString();
            string apiKey = Models.UserDatabaseAccess.GetTrueKey(vSplit);
            bool result = Models.UserDatabaseAccess.RemoveUser(username, apiKey);
            if (result == true)
            {
                return Ok(result);
            }
            //False
            else
            {
                return Ok(result);
            }
            #endregion
        }

        [ActionName("ChangeRole")]
        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult PostChangeRole([FromBody]Models.User userInfo)
        {
            #region TASK4
            string[] roles = new string[] { "admin", "user" };
            int i = roles.Length, z = 0, isBadRequest = 0;
            while(true)
            {
                try
                {
                    //If the role does exist
                    string checkRole = userInfo.Role.ToLower();
                    if (checkRole == roles[z])
                    {
                        string result = Models.UserDatabaseAccess.ChangeRole(userInfo);
                        if (result == "200")
                        {
                            return Ok("DONE");
                        }
                        else if (result == "400")
                        {
                            return BadRequest("NOT DONE: Username does not exist");
                        }
                        else
                        {
                            return BadRequest("NOT DONE: An error occured");
                        }
                    }
                    //Enters if the role does not exist
                    else if (userInfo.Role != roles[z] && isBadRequest == 2)
                    {
                        isBadRequest += 1;
                        //If the number of bad roles is equal to the total number of roles the role doesn't exist
                        if (isBadRequest == roles.Length)
                        {
                            return BadRequest("NOT DONE: Role does not exist");
                        }
                    }
                    else
                    {
                        z += 1;
                    }
                }
                catch
                {
                    return BadRequest("NOT DONE: An error occured");
                }
            }
            #endregion
        }
    }
}
