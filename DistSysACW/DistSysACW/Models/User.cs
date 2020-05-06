using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DistSysACW.Models
{
    public class User
    {
        #region Task2
        public User() { }

        [Key]
        public string ApiKey { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public ICollection<Log> Logs { get; set; }

        #endregion
    }

    public class Log
    {
        public Log() { }

        [Key]
        public int LogId { get; set; }

        public string LogString { get; set; }

        public DateTime LogDateTime { get; set; }

        public string UserApiKey { get; set; }

    }

    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion

    public static class UserDatabaseAccess
    {
        #region Task3 

        public static string NewUser(string username, string type)
        {
            using (var ctx = new UserContext())
            {
                //User wants to add to database
                if (type == "Post")
                {
                    string apiKey;
                    if (username.Length < 1)
                    {
                        return new string("400");
                        
                    }
                    //If Database has no entries
                    if (!ctx.Users.Any())
                    {
                        apiKey = MakeApi();
                        User user = new User() { ApiKey = apiKey, UserName = username, Role = "admin" };
                        ctx.Users.Add(user);
                        ctx.SaveChanges();
                        return apiKey;
                    }
                    //If Database has any entries
                    else
                    {
                        if (ctx.Users.Where(u => u.UserName == username).Any())
                        {
                            return new string("403");
                        }
                        else
                        {
                            apiKey = MakeApi();
                            User user = new User() { ApiKey = apiKey, UserName = username, Role = "user" };
                            ctx.Users.Add(user);
                            ctx.SaveChanges();
                            return apiKey;
                        }
                    }
                }
                //User tried to Get when they should have posted
                else if (type == "Get")
                {
                    if (ctx.Users.Where(u => u.UserName == username).Any())
                    {
                        return new string("True - User Does Exist! Did you mean to do a post to create a new user?");
                    }
                    else
                    {
                        return new string("False - User Does Not Exist! Did you mean to do a POST to create a new user?");
                    }
                }
                return null;
            }
        }

        public static string MakeApi()
        {
            Guid obj = Guid.NewGuid();
            string apiKey = obj.ToString();
            return apiKey;
        }

        public static bool CheckApi(string apiKey)
        {
            using (var ctx = new UserContext())
            {
                if (ctx.Users.Where(u => u.ApiKey == apiKey).Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool CheckUser(string apiKey, string username)
        {
            using (var ctx = new UserContext())
            {
                
                //ctx.SaveChanges();
                var apiCheck = ctx.Users.Where(i => i.ApiKey == apiKey);
                var userCheck = ctx.Users.Where(u => u.UserName == username);

                if (apiCheck != null && userCheck != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static User GetUserObj(string apiKey)
        {
            User user = new User();
            using (var ctx = new UserContext())
            {
                var apiCheck = ctx.Users.Where(i => i.ApiKey == apiKey);
                if (apiCheck != null)
                {
                    user = ctx.Users.Find(apiKey);
                    return user;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool RemoveUser(string username, string apiKey)
        {
            using (var ctx = new UserContext())
            {
                try
                {
                    User checkUser = new User();

                    User endUser = new User();

                    checkUser = ctx.Users.Find(apiKey);

                    endUser = ctx.Users.Where(b => b.UserName == username).FirstOrDefault();

                    try
                    {
                        AddLog("User request /User/Delete", apiKey, DateTime.Now);
                    }
                    catch
                    {
                    
                    }
                    //Assign two instances of the user class to compare them
                     
                    if (checkUser.UserName == endUser.UserName && checkUser.ApiKey == endUser.ApiKey)
                    {
                        ctx.Remove(endUser);
                        ctx.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string ChangeRole(User user)
        {
            using (var ctx = new UserContext())
            {
                try
                {
                    User userchange = new User();
                    
                    userchange = ctx.Users.Where(b => b.UserName == user.UserName).FirstOrDefault();

                    try
                    {
                        AddLog("User request /User/Role", userchange.ApiKey, DateTime.Now);
                    }
                    catch
                    {

                    }

                    if (userchange == null)
                    {
                        return "400";
                    }

                    userchange.Role = user.Role.ToLower();
                    ctx.Update(userchange);
                    ctx.SaveChanges();
                    return "200";
                }
                catch
                {
                    return "N400";
                }
            }
        }

        public static User Authorization(string apiKey)
        {
            User user = new User();

            using (var ctx = new UserContext())
            {
                var apiCheck = ctx.Users.Where(i => i.ApiKey == apiKey);
                if (apiCheck != null)
                {
                    user = ctx.Users.Where(b => b.ApiKey == apiKey).FirstOrDefault();
                    return user;
                }
                else
                {
                    return null;
                }
            }
        }

        public static string hashConverter(string type, string message)
        {
            if (type == "1")
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                byte[] hashData = new SHA1Managed().ComputeHash(data);

                var hash = string.Empty;

                foreach (var b in hashData)
                {
                    hash += b.ToString("X2");
                }

                return hash;
            }
            else if (type == "256")
            {
                SHA256 sha256 = SHA256Managed.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                byte[] hash = sha256.ComputeHash(bytes);
                string hashresult = GetStringFromHash(hash);
                return hashresult;
            }
            else
            {
                return null;
            }
        }
        public static string GetTrueKey(string key)
        {
            string vSplit = key.ToString();
            string[] split = vSplit.Split(',');
            var authorizeKey = split[split.Length - 1];
            string authorize = authorizeKey.Trim();
            return authorize;
        }
        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public static void AddLog(string LogMessage, string apiKey, DateTime time)
        {
            using (UserContext ctx = new UserContext())
            {
                User userchange = new User();
                userchange = ctx.Users.Where(i => i.ApiKey == apiKey).FirstOrDefault();
                Log checkLog = new Log() { LogString = LogMessage, LogDateTime = time, UserApiKey = apiKey };
                ctx.Logs.Add(checkLog);
                ctx.SaveChanges();
            }
        }
        #endregion
    }
}