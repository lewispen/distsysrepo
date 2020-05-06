using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreExtensions;
using System.Text;
using System.ComponentModel;

namespace DistSysACW.Controllers
{
    [ApiController]
    [Route("api/[Controller]/[Action]")]
    public class ProtectedController : BaseController
    {
        public ProtectedController(Models.UserContext context) : base(context) { }

        [ActionName("Hello")]
        [HttpGet]
        public ActionResult ProtectedGetUser()
        {
            #region TASK4
            string type = "ApiKey";
            Request.Headers.TryGetValue(type, out var value);
            try
            {
                Models.UserDatabaseAccess.AddLog("User request /Protected/Hello", value, DateTime.Now);
            }
            catch
            {

            }
            try
            {
                string vSplit = value.ToString();
                string ProtKey = Models.UserDatabaseAccess.GetTrueKey(vSplit);
                var userObject = Models.UserDatabaseAccess.GetUserObj(ProtKey);
                string result = userObject.UserName;
                return Ok("Hello " + result);
            }
            catch
            {
                return Ok("You need to do a User Post or User Set first");
            } 
            
            #endregion
        }

        [ActionName("SHA1")]
        [HttpGet]
        public ActionResult ProtectedSha1([FromQuery]string message)
        {
            #region TASK4
            string typeKey = "ApiKey";
            Request.Headers.TryGetValue(typeKey, out var value);
            try
            {
                Models.UserDatabaseAccess.AddLog("User request /Protected/SHA1", value, DateTime.Now);
            }
            catch
            {

            }
            string type = "1";
            string[] checkMessage = message.Split(" ");
            if (checkMessage.Length >= 3)
            {
                string result = Models.UserDatabaseAccess.hashConverter(type, message);
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
            
            #endregion
        }

        [ActionName("SHA256")]
        [HttpGet]
        public ActionResult ProtectedSha256([FromQuery]string message)
        {
            #region TASK4
            string typeKey = "ApiKey";
            Request.Headers.TryGetValue(typeKey, out var value);
            try
            {
                Models.UserDatabaseAccess.AddLog("User request /Protected/SHA256", value, DateTime.Now);
            }
            catch
            {

            }
            string type = "256";
            string[] checkMessage = message.Split(" ");
            if (checkMessage.Length >= 3)
            {
                string result = Models.UserDatabaseAccess.hashConverter(type, message);
                return Ok(result);
            }
            else
            {
                return BadRequest("Bad Request");
            }
            #endregion
        }



        [ActionName("GetPublicKey")]
        [HttpGet]
        public ActionResult PublicKeyRequest()
        {
            string type = "ApiKey";
            Request.Headers.TryGetValue(type, out var value);
            try
            {
                Models.UserDatabaseAccess.AddLog("User request /Protected/GetPublicKey", value, DateTime.Now);
            }
            catch
            {

            }
            var userObject = Models.UserDatabaseAccess.GetUserObj(value);
            //User with apiKey exists
            if (userObject != null)
            {
                //This only sends the Public key, changing it to true exports both public and private
                return Ok(RSAClass.RSAKeys.ToXmlStringCore22(false));
            }
            else
            {
                return BadRequest();
            }
        }

        [ActionName("Sign")]
        [HttpGet]
        public ActionResult SignKeyRequest([FromQuery]string message)
        {
            byte[] originalData = null;

            if (message == null)
            {
                return BadRequest();
            }

            originalData = Encoding.ASCII.GetBytes(message);
            //Turn back to string
            //string someString = Encoding.ASCII.GetString(bytes);
            byte[] signedData;

            string type = "ApiKey";
            Request.Headers.TryGetValue(type, out var value);
            try
            {
                Models.UserDatabaseAccess.AddLog("User request /Protected/Sign", value, DateTime.Now);
            }
            catch
            {

            }
            var userObject = Models.UserDatabaseAccess.GetUserObj(value);
            if (userObject != null)
            {
                //This only sends the Public key, changing it to true exports both public and private
                ASCIIEncoding ByteConverter = new ASCIIEncoding();
                RSAParameters key = RSAClass.RSAKeys.ExportParameters(true);

                signedData = HashAndSignBytes(originalData, key);

                var hexData = ConvertToHex(signedData);
                
                return Ok(hexData);

            }
            else
            {
                return BadRequest();
            }
        }

        public string ConvertToHex(byte[] ascii)
        {
            int x = 0;
            StringBuilder hex = new StringBuilder(ascii.Length * 2);
            foreach (byte b in ascii)
            {
                x += 1;
                hex.AppendFormat("{0:x2}", b);
                if (x != ascii.Length)
                {
                    hex.Append("-");
                }
            }
            return hex.ToString().ToUpper();
        }

        public static byte[] HashAndSignBytes(byte[] DataToSign, RSAParameters Key)
        {
            try
            {
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                return RSAalg.SignData(DataToSign, new SHA1CryptoServiceProvider());
            }
            catch
            {
                return null;
            }
        }

        internal class RSAClass
        {
            private static RSAClass RSA;
            public static RSACryptoServiceProvider RSAKeys = new RSACryptoServiceProvider();
            private RSAClass()
            {
                RSACryptoServiceProvider.UseMachineKeyStore = true;
            }

            public static RSAClass createInstance()
            {
                if (RSA == null)
                {
                    RSA = new RSAClass();
                }
                return RSA;
            }

            static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
            {
                try
                {
                    byte[] encryptedData;

                    using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                    {
                        RSA.ImportParameters(RSAKeyInfo);

                        encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                    }
                    return encryptedData;
                }
                catch(CryptographicException e)
                {
                    Console.WriteLine(e.Message);

                    return null;
                }
            }

            public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
            {
                try
                {
                    byte[] decryptedData;

                    using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                    {
                        RSA.ImportParameters(RSAKeyInfo);

                        decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                    }

                    return decryptedData;
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.ToString());

                    return null;
                }
            }

        }
    }
    
}
