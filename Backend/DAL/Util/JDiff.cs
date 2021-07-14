using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.timmons.cognitive.API.Util
{
    public static class JDiff
    {
        /// <summary>
        /// Deep compare two NewtonSoft JObjects. If they don't match, returns text diffs
        /// </summary>
        /// <param name="source">The expected results</param>
        /// <param name="target">The actual results</param>
        /// <returns>Text string</returns>

        public static List<StringBuilder> Diff(this JObject source, JObject target, List<string> path = null)
        {
            if(path == null)
            {
                path = new List<string>();
            }

            List<StringBuilder> returnString = new List<StringBuilder>();


            foreach (KeyValuePair<string, JToken> sourcePair in source)
            {
                try
                {
                    var thisPath = new List<String>();
                    thisPath.AddRange(path);
                    thisPath.Add(sourcePair.Key);
                    String pathStr = "[" + String.Join("][", thisPath) + "]";

                    if (sourcePair.Value.Type == JTokenType.Object)
                    {
                        if (target.GetValue(sourcePair.Key) == null)
                        {
                            returnString.Add(new StringBuilder().Append("Added " + pathStr + " with value " + sourcePair.Value));
                        }
                        else if (target.GetValue(sourcePair.Key).Type != JTokenType.Object)
                        {
                            returnString.Add(new StringBuilder().Append(pathStr + ": "
                                                    + target.Property(sourcePair.Key).Value + " changed to "
                                                    + sourcePair.Value));
                        }
                        else
                        {
                            returnString.AddRange(Diff(sourcePair.Value.ToObject<JObject>(),
                                target.GetValue(sourcePair.Key).ToObject<JObject>(), thisPath));
                        }
                    }
                    else if (sourcePair.Value.Type == JTokenType.Array)
                    {
                        if (target.GetValue(sourcePair.Key) == null)
                        {
                            returnString.Add(new StringBuilder().Append("Added " + pathStr + " with value " + sourcePair.Value
                                                + Environment.NewLine));
                        }
                        else
                        {
                            returnString.AddRange(Diff(sourcePair.Value.ToObject<JArray>(),
                                target.GetValue(sourcePair.Key).ToObject<JArray>(), thisPath));
                        }
                    }
                    else
                    {
                        JToken expected = sourcePair.Value;
                        var actual = target.SelectToken(sourcePair.Key);
                        if (actual == null)
                        {
                            returnString.Add(new StringBuilder().Append(("Added " + pathStr + " with value " + sourcePair.Value)));
                        }
                        else
                        {
                            if (!JToken.DeepEquals(expected, actual))
                            {
                                returnString.Add(new StringBuilder().Append(pathStr + ": "
                                                    + target.Property(sourcePair.Key).Value + " changed to "
                                                    + sourcePair.Value));
                            }
                        }
                    }
                }
                catch { }
            }
            return returnString;
        }

        /// <summary>
        /// Deep compare two NewtonSoft JArrays. If they don't match, returns text diffs
        /// </summary>
        /// <param name="source">The expected results</param>
        /// <param name="target">The actual results</param>
        /// <param name="arrayName">The name of the array to use in the text diff</param>
        /// <returns>Text string</returns>

        public static List<StringBuilder> Diff(this JArray source, JArray target, List<string> path = null)
        {
            if (path == null)
            {
                path = new List<string>();
            }

            var returnString = new List<StringBuilder>();
            for (var index = 0; index < source.Count; index++)
            {
                var thisPath = new List<String>();
                thisPath.AddRange(path);
                thisPath.Add(String.Format("{0}",index));
                String pathStr = "[" + String.Join("][", thisPath) + "]";

                var expected = source[index];
                if (expected.Type == JTokenType.Object)
                {
                    var actual = (index >= target.Count) ? new JObject() : target[index];
                    returnString.AddRange(Diff(expected.ToObject<JObject>(),
                        actual.ToObject<JObject>(), thisPath));
                }
                else
                {

                    var actual = (index >= target.Count) ? "" : target[index];
                    if (!JToken.DeepEquals(expected, actual))
                    {
                        returnString.Add(new StringBuilder().Append(pathStr + ": " + actual
                                         + " changed to " + expected ));
                    }
                }
            }
            return returnString;
        }
    }
}
