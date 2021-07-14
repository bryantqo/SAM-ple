using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Util
{
    public static class JDiff
    {
        // TODO: This may be better https://github.com/wbish/jsondiffpatch.net
        /// <summary>
        /// Deep compare two NewtonSoft JObjects. If they don't match, returns text diffs
        /// </summary>
        /// <param name="source">The expected results</param>
        /// <param name="target">The actual results</param>
        /// <returns>Text string</returns>

        public static JObject Diff(this JObject source, JObject target, List<string> path = null)
        {
            JObject diff = new JObject();

            if(target == null)
            {
                diff = source;
                return diff;
            }

            if (path == null)
            {
                path = new List<string>();
            }

            foreach(var pair in source)
            {
                if(!target.ContainsKey(pair.Key))
                {
                    // Item doesnt exist in the target
                    diff[pair.Key] = source[pair.Key];
                }
                else
                {
                    if (pair.Value.Type == JTokenType.Null && target[pair.Key].Type != JTokenType.Null)
                    {
                        diff[pair.Key] = pair.Value;
                    }
                    else if (pair.Value.Type == JTokenType.Object)
                    {
                        var subDiff = (pair.Value as JObject).Diff(target[pair.Key] as JObject);
                        if (subDiff.Count > 0)
                        {
                            diff[pair.Key] = new JObject();
                            foreach (var merge in subDiff)
                            {
                                diff[pair.Key][merge.Key] = merge.Value;
                            }
                        }
                    }
                    else if (pair.Value.Type == JTokenType.Array)
                    {
                        var subDiff = (pair.Value as JArray).Diff(target[pair.Key] as JArray);
                        diff[pair.Key] = subDiff;
                    }
                    else
                    {
                        if (!pair.Value.Equals(target[pair.Key]))
                        {
                            diff[pair.Key] = pair.Value;
                        }
                    }
                }
            }

            return diff;
        }

        /// <summary>
        /// Deep compare two NewtonSoft JArrays. If they don't match, returns text diffs
        /// </summary>
        /// <param name="source">The expected results</param>
        /// <param name="target">The actual results</param>
        /// <param name="arrayName">The name of the array to use in the text diff</param>
        /// <returns>Text string</returns>

        public static JArray Diff(this JArray source, JArray target, List<string> path = null)
        {
            JArray diff = new JArray();

            if(source == null && target != null)
            {
                diff = source;
                return diff;
            }

            if(target == null)
            {
                diff = source;
                return diff;
            }

            if (source.Count == target.Count)
            {
                // Count is the same, make sure items are equal
                for (var i = 0; i < source.Count; i++)
                {
                    var itm = source[i];
                    var tar = target[i];

                    if(itm.Type == JTokenType.Object)
                    {
                        var subDiff = (itm as JObject).Diff(tar as JObject);
                        if (subDiff.Count > 0)
                        {
                            diff = source;
                            return diff;
                        }
                    }
                    else if (itm.Type == JTokenType.Array)
                    {
                        var subDiff = (itm as JArray).Diff(tar as JArray);
                        if (subDiff.Count > 0)
                        {
                            diff = source;
                            return diff;
                        }
                    }
                    else
                    {
                        if (!itm.Equals(tar))
                        {
                            diff = source;
                            return diff;
                        }
                    }
                }
            }
            else
            {
                // Dont even bother with the remaining items
                diff = source;
            }

            return diff;
        }
    }
}
