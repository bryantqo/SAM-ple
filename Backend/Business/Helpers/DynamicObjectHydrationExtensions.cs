
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PAM_Model = com.timmons.cognitive.API.Model;

namespace API.Middleware.Helpers
{
    /*
     * This class provides extension methods for the dynamic object for hydration purpouses
     * For example say you have an object with a choice field
     *  When stored in the database the field only has the id as a reference to the choice
     *  When it comes back out of the dtaabase we would want to know what that choice is named
     *  Instead of storing the name value with each object we only store the reference so that if the 
     *  choice name changes it is automatically reflected and we dont have to go update every item in the database
     */
    public static class DynamicObjectHydrationExtensions
    {
        /*
         * The created by and last modified by columns are UUID type. We want to have some other info about the user (usually) such as the email and name
         * This function populates that info regardless of the source. (I.E. WRAPs vs Cognito)
         */
        public static async IAsyncEnumerable<PAM_Model.DynamicObject> HydrateCreateMod(this IEnumerable<PAM_Model.DynamicObject> allProjects,
            Lookupable<Model.DTOs.UserDTO, Guid> usersHelper)
        {
            var allProjUsers = from i in allProjects select i.createdBy.id;
            allProjUsers = allProjUsers.Concat(from i in allProjects select i.lastModifiedBy.id);
            allProjUsers = allProjUsers.Distinct();

            var allUsers = new Dictionary<String, Model.DTOs.UserDTO>();

            await foreach (var user in usersHelper.Get(allProjUsers))
            {
                allUsers[user.id] = user;
            }

            foreach (var project in allProjects)
            {
                if (allUsers.ContainsKey(project.createdBy.id.ToString()))
                {
                    var user = allUsers[project.createdBy.id.ToString()];

                    project.createdBy.email = user.email;

                    if (user.first != null)
                        project.createdBy.firstName = user.first;

                    if (user.last != null)
                        project.createdBy.lastName = user.last;
                }

                if (allUsers.ContainsKey(project.lastModifiedBy.id.ToString()))
                {
                    var user = allUsers[project.lastModifiedBy.id.ToString()];

                    project.lastModifiedBy.email = user.email;

                    if (user.first != null)
                        project.lastModifiedBy.firstName = user.first;

                    if (user.last != null)
                        project.lastModifiedBy.lastName = user.last;
                }

                yield return project;
            }

            yield break;
        }

        public static async Task<PAM_Model.DynamicObject> HydrateCreateMod(this PAM_Model.DynamicObject project, Lookupable<Model.DTOs.UserDTO, Guid> usersHelper)
        {
            try
            {
                var allProjects = new List<PAM_Model.DynamicObject> { project };
                var allProjUsers = from i in allProjects where i.createdBy.id != Guid.Empty select i.createdBy.id;
                allProjUsers = allProjUsers.Concat(from i in allProjects where i.lastModifiedBy.id != Guid.Empty select i.lastModifiedBy.id);
                allProjUsers = allProjUsers.Distinct();

                var allUsers = new Dictionary<String, Model.DTOs.UserDTO>();

                await foreach (var user in usersHelper.Get(allProjUsers))
                {
                    allUsers[user.id] = user;
                }


                if (allUsers.ContainsKey(project.createdBy.id.ToString()))
                {
                    var user = allUsers[project.createdBy.id.ToString()];

                    project.createdBy.email = user.email;

                    if (user.first != null)
                        project.createdBy.firstName = user.first;

                    if (user.last != null)
                        project.createdBy.lastName = user.last;
                }

                if (allUsers.ContainsKey(project.lastModifiedBy.id.ToString()))
                {
                    var user = allUsers[project.lastModifiedBy.id.ToString()];

                    project.lastModifiedBy.email = user.email;

                    if (user.first != null)
                        project.lastModifiedBy.firstName = user.first;

                    if (user.last != null)
                        project.lastModifiedBy.lastName = user.last;
                }
            }
            catch { }
            return project;

        }

        /*
         * This method replaces the field value of a choice field with the long form of a choice
         * The database only keeps a reference to id and we need to tag the name on there so we reduce the number of requests made
         */
        public static async Task<PAM_Model.DynamicObject> HydrateChoice(this PAM_Model.DynamicObject obj, string topLevelKey, Lookupable<Model.DTOs.ChoiceDTO, int> map)
        {
            int? id = null;

            try
            {
                id = obj.fields.Value<int>(topLevelKey);
            }
            catch { }

            if (id == null)
            {
                try
                {
                    id = obj.fields.SelectToken(topLevelKey).Value<int>("id");
                }
                catch { }
            }

            if (id != null)
            {
                Model.DTOs.ChoiceDTO r = null;
                await foreach (var i in map.Get(new List<int> { id.GetValueOrDefault() }))
                {
                    r = i;
                }

                var luv = r;
                obj.fields[topLevelKey] = JObject.FromObject(luv);
            }

            return obj;
        }


        public static async Task<JToken> HydrateRelationalIntField(JToken cur, IEnumerable<string> path,
            List<Lookupable<object, int>> objectLookups)
        {
            if (path.Count() == 0)
            {
                //We should be at our token
                //Lets try to hydrate it
                if (cur.Type == JTokenType.Null)
                    return cur;

                if (cur.Type == JTokenType.Array)
                {
                    var asArr = cur as JArray;

                    var resArr = new JArray();

                    for (var i = 0; i < asArr.Count(); i++)
                    {
                        asArr[i] = await HydrateRelationalIntField(asArr[i], path, objectLookups);

                        if (asArr[i].Type != JTokenType.Null)
                            resArr.Add(asArr[i]);
                    }

                    return resArr;
                }

                if (cur.Type == JTokenType.Object)
                {
                    var asObj = cur as JObject;

                    if (asObj.ContainsKey("id"))
                    {
                        var id = asObj.Value<int?>("id");

                        if (id != null)
                        {

                            object lookup = null;

                            foreach (var objectLookup in objectLookups)
                            {
                                lookup = await objectLookup.GetSingle(id.GetValueOrDefault());

                                if (lookup != null)
                                    break;
                            }


                            cur = null;
                            if (lookup != null)
                                cur = JObject.FromObject(lookup);

                            return cur;

                        }
                    }
                }

                if (cur.Type == JTokenType.Integer)
                {
                    var id = cur.Value<int?>();

                    if (id != null)
                    {

                        object lookup = null;

                        foreach (var objectLookup in objectLookups)
                        {
                            lookup = await objectLookup.GetSingle(id.GetValueOrDefault());

                            if (lookup != null)
                                break;
                        }


                        cur = null;
                        if (lookup != null)
                            cur = JObject.FromObject(lookup);

                        return cur;

                    }
                }

            }

            var sel = path.First();
            var rest = path.Skip(1);
            if (cur.Type == JTokenType.Array)
            {
                var asArr = cur as JArray;
                for (var i = 0; i < asArr.Count; i++)
                {
                    asArr[i] = await HydrateRelationalIntField(asArr[i], path, objectLookups);
                }

                return cur;
            }

            if (cur.Type == JTokenType.Object)
            {
                var asObj = cur as JObject;

                if (asObj.ContainsKey(sel))
                {
                    asObj[sel] = await HydrateRelationalIntField(asObj[sel], rest, objectLookups);

                }

                return asObj;
            }


            return cur;
        }

        /*
            * This method takes a field that is deemed to be a relational int aka by id for most cases and populates it with the rest of the data
            * Deemed nescessary for the transaction
        */
        public static async Task<PAM_Model.DynamicObject> HydrateRelationalIntField(this PAM_Model.DynamicObject obj, string pathToRelational,
        List<Lookupable<object, int>> objectLookups)
        {
                obj.fields = await HydrateRelationalIntField(obj.fields, pathToRelational.Split("."), objectLookups) as JObject;
                return obj;
            
        }


        /*
         * This method takes a field that is deemed to be a relational string aka by name for most cases and populates it with the rest of the data
         * Deemed nescessary for the transaction
         */
        public static async Task<PAM_Model.DynamicObject> HydrateRelationalStringField(this PAM_Model.DynamicObject obj, string pathToRelational,
            Lookupable<object, string> objectLookup)
        {
            if (!obj.fields.ContainsKey(pathToRelational))
                return obj;

            try
            {
                var tok = obj.fields.SelectToken(pathToRelational);

                if (tok.Type == JTokenType.Array)
                {
                    List<object> newValues = new List<object>();
                    foreach (var tok2 in tok)
                    {
                        if (tok2 == null || tok2.Type == JTokenType.Null)
                            continue;

                        String id2 = null;

                        if (tok2.Type == JTokenType.String)
                            id2 = tok2.Value<string>();
                        else if (tok2.Type == JTokenType.Object)
                            id2 = tok2.Value<string>("id");

                        var lookup = await objectLookup.GetSingle(id2);
                        if (lookup != null)
                            newValues.Add(lookup);
                    }

                    obj.fields[pathToRelational] = JArray.FromObject(newValues);
                }
                else
                {
                    String id = null;

                    if (tok.Type == JTokenType.String)
                        id = tok.Value<string>();
                    else if (tok.Type == JTokenType.Object)
                        id = tok.Value<string>("id");

                    if (id == null)
                        return obj;
                    var lookup = await objectLookup.GetSingle(id);
                    obj.fields[pathToRelational] = JObject.FromObject(lookup);
                }
            }
            catch { }
            return obj;
        }


        public static async Task<JToken> HydrateChoiceConfigLookup(JToken cur, IEnumerable<string> path,
            string lookupKey, Lookupable<object, Tuple<string, int>> objectLookups)
        {
            if (path.Count() == 0)
            {
                //We should be at our token
                //Lets try to hydrate it

                if (cur.Type == JTokenType.Array)
                {
                    var asArr = cur as JArray;

                    for (var i = 0; i < asArr.Count(); i++)
                    {
                        asArr[i] = await HydrateChoiceConfigLookup(asArr[i], path, lookupKey, objectLookups);
                    }

                    return asArr;
                }

                if (cur.Type == JTokenType.Object)
                {
                    var asObj = cur as JObject;

                    if (asObj.ContainsKey("id"))
                    {
                        var id = asObj.Value<int?>("id");

                        if (id != null)
                        {
                            var r = await objectLookups.GetSingle(new Tuple<string, int>(lookupKey, id.GetValueOrDefault()));
                            cur = JObject.FromObject(r);
                        }
                        return cur;

                    }
                }


                if (cur.Type == JTokenType.Integer)
                {
                    var id = cur.Value<int?>();

                    if (id != null)
                    {
                        var r = await objectLookups.GetSingle(new Tuple<string, int>(lookupKey, id.GetValueOrDefault()));
                        cur = JObject.FromObject(r);
                    }
                    return cur;
                }

                return cur;

            }

            var sel = path.First();
            var rest = path.Skip(1);
            if (cur.Type == JTokenType.Array)
            {
                var asArr = cur as JArray;
                for (var i = 0; i < asArr.Count; i++)
                {
                    asArr[i] = await HydrateChoiceConfigLookup(asArr[i], path, lookupKey, objectLookups);
                }

                return asArr;
            }

            if (cur.Type == JTokenType.Object)
            {
                var asObj = cur as JObject;

                if (asObj.ContainsKey(sel))
                {
                    asObj[sel] = await HydrateChoiceConfigLookup(asObj[sel], rest, lookupKey, objectLookups);

                }

                return asObj;
            }


            return cur;

        }

        /*
         * In ULTRA we started hacking in choice fields by throwing them in the config
         * This was to ease development time and to have one location to get the app config as well as the choice config
         */
        public static async Task<PAM_Model.DynamicObject> HydrateChoiceConfigLookup(this PAM_Model.DynamicObject obj, string topLevelKey,
            string lookupKey, Lookupable<object, Tuple<string, int>> map)
        {
                obj.fields = await HydrateChoiceConfigLookup(obj.fields, topLevelKey.Split("."), lookupKey, map) as JObject;
                return obj;
        }

        /*
         * In ULTRA we started hacking in two level choice fields by throwing them in the config
         * This was to ease development time and to have one location to get the app config as well as the choice config
         * 
         * 
         * Side Note: I hate the concept of choices being dependent on other choices. It leads to bad UX but some cant get
         * The idea of a multi picker to stick
         * ie instead of
         *      Field 1:
         *          Choice 1
         *          Choice 2
         *          Choice 3
         *      Field 2:
         *          Choice A
         *          Choice B
         *          Choice C
         * Have this
         *      Unified Field:
         *          Choice 1
         *          Choice 1 - Choice A
         *          Choice 2 - Choice A
         *          Choice 2 - Choice B
         *          Choice 3 - Choice A
         *          Choice 3 - Choice C
         *          
         * Then you only have to select a single item, dont need additional validation, and dont have to add in the dumb 
         * logic to change values in another field.
         * 
         * Make fields pure again!
         * 
         * But who am I, some sort of front end engineer LOL. Out of here ᕕ( ᐛ )ᕗ
         */
        public static async Task<PAM_Model.DynamicObject> HydrateTwoLevelChoiceConfigLookup(this PAM_Model.DynamicObject obj, string firstKey, string topLevelKey,
            string lookupKey, Lookupable<object, Tuple<string, IEnumerable<int>>> map)
        {


            int? firstID = null;

            var tok = obj.fields.SelectToken(firstKey);

            if (tok == null || tok.Type == JTokenType.Null)
            {
                return obj;
            }
            else if (tok.Type == JTokenType.Integer)
            {
                firstID = tok.Value<int>();
            }
            else if (tok.Type == JTokenType.Object)
            {
                firstID = tok.Value<int>("id");
            }


            int? secondID = null;

            tok = obj.fields.SelectToken(topLevelKey);

            if (tok == null || tok.Type == JTokenType.Null)
            {
                return obj;
            }
            else if (tok.Type == JTokenType.Integer)
            {
                secondID = tok.Value<int>();
            }
            else if (tok.Type == JTokenType.Object)
            {
                secondID = tok.Value<int>("id");
            }




            if (firstID != null && secondID != null)
            {
                object r = null;

                r = await map.GetSingle(new Tuple<string, IEnumerable<int>>(lookupKey, new List<int> { firstID.GetValueOrDefault(), secondID.GetValueOrDefault() }));

                obj.fields[topLevelKey] = JObject.FromObject(r);
            }

            return obj;
        }
    }


    public static class ProjectDehydration
    {
        public static IEnumerable<PAM_Model.DynamicObject> DehydrateCreateMod(this IEnumerable<PAM_Model.DynamicObject> allProjects, Lookupable<Model.DTOs.UserDTO, Guid> usersHelper)
        {

            foreach (var project in allProjects)
            {
                project.createdBy = new PAM_Model.User { id = project.createdBy.id };
                project.lastModifiedBy = new PAM_Model.User { id = project.lastModifiedBy.id };

                yield return project;
            }

            yield break;
        }

        public static PAM_Model.DynamicObject DehydrateCreateMod(this PAM_Model.DynamicObject project, Lookupable<Model.DTOs.UserDTO, Guid> usersHelper)
        {
            //project.createdBy = new Model.User { id = project.createdBy.id };
            //project.lastModifiedBy = new Model.User { id = project.lastModifiedBy.id };

            return project;
        }

        public static PAM_Model.DynamicObject DehydrateChoice(this PAM_Model.DynamicObject obj, string topLevelKey, Lookupable<Model.DTOs.ChoiceDTO, int> map)
        {
            Model.DTOs.IDKeyed val = null;

            try
            {
                val = obj.fields.Value<Model.DTOs.IDKeyed>(topLevelKey);
            }
            catch
            {
                try
                {
                    val = new Model.DTOs.IDKeyed { id = obj.fields.Value<int>(topLevelKey) };
                }
                catch
                { }
            }



            if (val != null)
            {
                obj.fields[topLevelKey] = JObject.FromObject(val);
            }

            return obj;
        }

        public static JToken DehydrateRelationalIntField(JToken tok, IEnumerable<string> path)
        {
            if (path.Count() == 0)
            {
                //At our target

                if (tok == null || tok.Type == JTokenType.Null)
                    return tok;

                if (tok.Type == JTokenType.Array)
                {
                    var asArray = tok as JArray;

                    for (var i = 0; i < asArray.Count(); i++)
                    {
                        asArray[i] = DehydrateRelationalIntField(asArray[i], path);
                    }

                    return asArray;
                }

                int? id = null;

                if (tok.Type == JTokenType.Integer)
                    id = tok.Value<int>();
                else if (tok.Type == JTokenType.Object)
                    id = tok.Value<int>("id");

                if (id != null)
                {
                    tok = JObject.FromObject(new { id = id.GetValueOrDefault() });
                }



                return tok;
            }

            
            if (tok.Type == JTokenType.Array)
            {
                var asArray = tok as JArray;
                for(var i = 0; i < asArray.Count(); i++)
                {
                    asArray[i] = DehydrateRelationalIntField(asArray[i], path);
                }

                return asArray;
            }

            if (tok.Type == JTokenType.Object)
            {
                var cur = path.First();
                var rest = path.Skip(1);

                var asObj = tok as JObject;

                if (asObj.ContainsKey(cur))
                {
                    asObj[cur] = DehydrateRelationalIntField(asObj[cur], rest);
                }
            }

            return tok;
        }

        public static PAM_Model.DynamicObject DehydrateRelationalIntField(this PAM_Model.DynamicObject obj, string pathToRelational)
        {
            obj.fields = DehydrateRelationalIntField(obj.fields, pathToRelational.Split(".")) as JObject;
            return obj;
        }

        public static PAM_Model.DynamicObject DehydrateRelationalStringField(this PAM_Model.DynamicObject obj, string pathToRelational)
        {
            if (!obj.fields.ContainsKey(pathToRelational))
                return obj;

            try
            {
                var tok = obj.fields.SelectToken(pathToRelational);

                if (tok.Type == JTokenType.Array)
                {
                    List<object> newValues = new List<object>();
                    foreach (var tok2 in tok)
                    {
                        String id2 = null;

                        if (tok2.Type == JTokenType.String)
                            id2 = tok2.Value<string>();
                        else if (tok2.Type == JTokenType.Object)
                            id2 = tok2.Value<string>("id");

                        newValues.Add(new { id = id2 });
                    }

                    obj.fields[pathToRelational] = JArray.FromObject(newValues);
                }
                else
                {
                    String id = null;

                    if (tok.Type == JTokenType.String)
                        id = tok.Value<string>();
                    else if (tok.Type == JTokenType.Object)
                        id = tok.Value<string>("id");

                    obj.fields[pathToRelational] = JObject.FromObject(new { id = id });
                }
            }
            catch { }
            return obj;
        }


        public static JToken DehydrateChoiceConfigLookup(JToken tok, IEnumerable<string> path)
        {
            if(path.Count() == 0)
            {
                //At our target

                if (tok == null || tok.Type == JTokenType.Null)
                    return tok;

                if (tok.Type == JTokenType.Array)
                {
                    var asArray = tok as JArray;

                    for(var i = 0; i < asArray.Count(); i++)
                    {
                        asArray[i] = DehydrateChoiceConfigLookup(asArray[i], path);
                    }

                    return asArray;
                }

                int? id = null;

                if (tok.Type == JTokenType.Integer)
                    id = tok.Value<int>();
                else if (tok.Type == JTokenType.Object)
                    id = tok.Value<int>("id");

                if (id != null)
                {
                    tok = JObject.FromObject(new { id = id.GetValueOrDefault() });
                }

                

                return tok;
            }

            var cur = path.First();
            var rest = path.Skip(1);


            if (tok.Type == JTokenType.Array)
            {
                var asArray = tok as JArray;
                for (var i = 0; i < asArray.Count(); i++)
                {
                    asArray[i] = DehydrateChoiceConfigLookup(asArray[i], path);
                }

                return asArray;
            }

            if (tok.Type == JTokenType.Object)
            {
                var asObj = tok as JObject;

                if(asObj.ContainsKey(cur))
                {
                    asObj[cur] = DehydrateChoiceConfigLookup(asObj[cur], rest);
                }
            }

            return tok;
        }

        public static PAM_Model.DynamicObject DehydrateChoiceConfigLookup(this PAM_Model.DynamicObject obj, string topLevelKey)
        {
            obj.fields = DehydrateChoiceConfigLookup(obj.fields, topLevelKey.Split(".")) as JObject;
            return obj;
        }

        public static PAM_Model.DynamicObject DehydrateTwoLevelChoiceConfigLookup(this PAM_Model.DynamicObject obj, string firstKey, string topLevelKey)
        {
            var tok = obj.fields.SelectToken(topLevelKey);

            if (tok == null || tok.Type == JTokenType.Null)
                return obj;

            int? id = null;

            if (tok.Type == JTokenType.Integer)
                id = tok.Value<int>();
            else if (tok.Type == JTokenType.Object)
                id = tok.Value<int>("id");

            if (id != null)
            {
                obj.fields[topLevelKey] = JObject.FromObject(new { id = id.GetValueOrDefault() });
            }

            return obj;
        }

        public static PAM_Model.DynamicObject HydrateSystemFields(this PAM_Model.DynamicObject newObject, PAM_Model.DynamicObjectModelNew model)
        {
            var sys = (from fld in model.fields where fld.Item2.id < 0 select fld).ToList();

            foreach (var fld in sys)
            {
                if (newObject.fields.ContainsKey(fld.Item2.name))
                {
                    if (fld.Item2.id == -1) // Name field
                        newObject.name = newObject.fields[fld.Item2.name].ToString();
                }
            }

            return newObject;
        }

        public static Tuple<PAM_Model.DynamicObject, PAM_Model.DynamicObject> SplitGeometry(this PAM_Model.DynamicObject newObject, PAM_Model.DynamicObjectModelNew model)
        {
            var geomFields = (from i in model.fields where i.Item1 == PAM_Model.FieldType.Spatial select i).ToList();

            PAM_Model.DynamicObject spatial = new PAM_Model.DynamicObject { id = newObject.id, name = newObject.name, created = newObject.created, createdBy = newObject.createdBy, deleted = newObject.deleted, lastModifiedBy = newObject.lastModifiedBy, modified = newObject.modified, type = newObject.type, fields = new JObject() };

            foreach (var spfd in geomFields)
            {
                if (newObject.fields.ContainsKey(spfd.Item2.name))
                {
                    spatial.fields[spfd.Item2.name] = newObject.fields[spfd.Item2.name];
                    newObject.fields.Remove(spfd.Item2.name);
                }
            }

            return new Tuple<PAM_Model.DynamicObject, PAM_Model.DynamicObject>(newObject, spatial);
        }
    }


}
