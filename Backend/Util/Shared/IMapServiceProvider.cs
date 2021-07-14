using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace com.timmons.Stitch.Shared
{
    public interface IMapServiceProvider
    {
        MapService GetMapService(MapServiceConfig config);
    }

    public class MapServiceConfig
    {
        public string Key { get; set; }
        public string BaseUrl { get; set; }
        public TokenConfig Config { get; set; }
    }

    public class TokenConfig
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TokenUrl { get; set; }
        public TokenMode Mode { get; set; }
    }

    public enum TokenMode
    {
        AGS = 0,
        MapProxy = 1
    }



    public class MapService
    {
        public String Key { get; set; }
        public String BaseUrl { get; set; }
        public List<String> log { get; set; }
    }

    public class TokenRequiredMapService : MapService
    {
        public String Token { get; set; }
        public String Expires { get; set; }
    }

    public class MapServiceProvider : IMapServiceProvider
    {

        public MapService GetMapService(MapServiceConfig config)
        {

            if (config.Config != null)
            {
                switch (config.Config.Mode)
                {
                    case TokenMode.AGS:
                        {
                            return getAgsToken(config.Key, config.Config.TokenUrl, config.Config.Username, config.Config.Password, config.BaseUrl);
                        }
                    case TokenMode.MapProxy:
                        {
                            return getMapProxyToken(config.Key, config.Config.TokenUrl, config.Config.Username, config.Config.Password, config.BaseUrl);
                        }
                }
            }

            //We could get here if something in the config was invalid or unhandled
            //When there is no token config just pass the baseurl forward
            return new MapService { Key = config.Key, BaseUrl = config.BaseUrl };

        }

        private MapService getAgsToken(String key, String url, string username, string password, /*string referrer,*/ string baseURL)
        {
            List<String> log = new List<string>();

            if (url != null && username != null && password != null &&
                url.Length > 0)
            {
                log.Add(String.Format("Attempting to get a token from {0}", url));

                string postString = string.Format("password={1}&f=json&username={0}&ip=&expiration=480&encrypted=false", username, password);

                string token = null;
                string expires = null;

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postString.Length;

                try
                {
                    using (var sw = new System.IO.StreamWriter(request.GetRequestStream()))
                    {
                        log.Add("Send creds");
                        sw.Write(postString);
                        sw.Close();
                        using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                        {
                            log.Add("Read response");
                            System.IO.Stream rStream = response.GetResponseStream();
                            System.IO.StreamReader rStreamReader = new System.IO.StreamReader(rStream, System.Text.Encoding.GetEncoding("utf-8"));

                            String resp = rStreamReader.ReadToEnd();

                            log.Add("Parsing response");
                            var responseJson = JObject.Parse(resp);
                            
                            JToken tokenObj = null;
                            JToken expiresObj = null;

                            responseJson.TryGetValue("token", out tokenObj);
                            responseJson.TryGetValue("expires", out expiresObj);

                            if (tokenObj != null)
                            {
                                token = tokenObj.ToString();
                                expires = expiresObj.ToString();
                            }
                            else
                            {
                                log.Add("No token returned");
                            }

                        }
                    }
                }
                catch (System.Net.WebException swex)
                {
                    log.Add(String.Format("Encountered a web exception {0}", swex.Message));
                    return new MapService { Key = key, BaseUrl = baseURL, log = log };
                }
                catch (ArgumentException ex)
                {
                    //We hit this when the server responds with html normally meaning its broken
                    log.Add(String.Format("Unable to parse responsefor {0}", key));
                    return new MapService { Key = key, BaseUrl = baseURL, log = log };
                }

                return new TokenRequiredMapService { Key = key, Token = token, Expires = expires, BaseUrl = baseURL, log = log };
            }

            //Something was borked, return a base map service
            return new MapService { Key = key, BaseUrl = baseURL };
        }

        private MapService getMapProxyToken(String key, String url, string username, string password, /*string referrer,*/ string baseURL)
        {
            if (url != null && username != null && password != null &&
                url.Length > 0)
            {

                string postString = string.Format("login={0}&password={1}", username, password);

                string token = null;
                string expires = null;

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postString.Length;

                try
                {
                    using (var sw = new System.IO.StreamWriter(request.GetRequestStream()))
                    {
                        sw.Write(postString);
                        sw.Close();
                        using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                        {
                            var tokenHeader = response.Headers.GetValues("Set-Cookie");

                            if (tokenHeader == null)
                            {
                                return null;
                            }

                            var regex = new System.Text.RegularExpressions.Regex("(?<=\")[^\"]+(?=\")");
                            var matches = regex.Matches(tokenHeader[0]);

                            if (matches.Count > 0)
                            {
                                token = matches[0].Value;
                                expires = DateTime.Now.AddHours(1).ToLongDateString();
                            }
                        }
                    }
                }
                catch (System.Net.WebException)
                {
                    return new MapService { Key = key, BaseUrl = baseURL };
                }
                catch (ArgumentException ex)
                {
                    //We hit this when the server responds with html normally meaning its broken
                    return new MapService { Key = key, BaseUrl = baseURL };
                }

                return new TokenRequiredMapService { Key = key, Token = token, Expires = expires, BaseUrl = baseURL };
            }

            //Something was borked, return a base map service
            return new MapService { Key = key, BaseUrl = baseURL };
        }
    }
}
