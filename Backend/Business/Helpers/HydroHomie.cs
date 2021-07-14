using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PAM_Model = com.timmons.cognitive.API.Model;


namespace API.Middleware.Helpers
{

    //This class provides extensions to use the HydroHomie class easier
    public static class HydroHomieExt
    {
        public static HydroHomie WithUsersHelper(this HydroHomie me, Lookupable<Model.DTOs.UserDTO, Guid> helper)
        {
            me.setUsersHelper(helper);
            return me;
        }
        public static HydroHomie WithGenericConfigHelper(this HydroHomie me, Lookupable<object, Tuple<string, int>> helper)
        {
            me.setGenericConfigHelper(helper);
            return me;
        }
        public static HydroHomie WithGenericConfigHelperML(this HydroHomie me, Lookupable<object, Tuple<string, IEnumerable<int>>> helper)
        {
            me.setGenericConfigHelperML(helper);
            return me;
        }

        public static HydroHomie WithTwoLevelConfigLookup(this HydroHomie me, string parent, string fieldName, string lookup)
        {
            me.addConfigLookup(fieldName, parent, lookup);
            return me;
        }

        public static HydroHomie WithLookup(this HydroHomie me, string fieldName, Lookupable<object, int> lookup, PAM_Model.Field parentField = null)
        {
            me.addLookup(fieldName, lookup, parentField);
            return me;
        }

        public static HydroHomie WithLookup(this HydroHomie me, string fieldName, Lookupable<object, string> lookup)
        {
            me.addLookup(fieldName, lookup);
            return me;
        }

        public static HydroHomie WithConfigLookup(this HydroHomie me, string fieldName, string lookup)
        {
            me.addConfigLookup(fieldName, lookup);
            return me;
        }


        public static HydroHomie FromModel(this HydroHomie me, PAM_Model.DynamicObjectModelNew model, 
            Lookupable<Model.DTOs.UserDTO, Guid> usersHelper, Dictionary<int, Lookupable<object, int>> internalIntObjectHelpers, 
            Dictionary<int, Lookupable<object, string>> internalStringObjectHelpers, Lookupable<DynamicObjectLookup, int> LookyLooLookup)
        {
            me
            .WithUsersHelper(usersHelper);

            foreach(var (fieldType, field) in model.fields)
            {
                me.FromSingleField(fieldType, field, internalIntObjectHelpers, internalStringObjectHelpers, LookyLooLookup);
            }

            return me;
        }

        public static HydroHomie FromSingleField(this HydroHomie me, PAM_Model.FieldType fieldType,
            PAM_Model.Field field, Dictionary<int, Lookupable<object, int>> internalIntObjectHelpers,
            Dictionary<int, Lookupable<object, string>> internalStringObjectHelpers, Lookupable<DynamicObjectLookup, int> LookyLooLookup, IEnumerable<string> parentPath = null)
        {
            if (parentPath == null)
                parentPath = new List<string>();

            switch (fieldType)
            {
                case PAM_Model.FieldType.ObjectReference:
                    {
                        if (field is PAM_Model.ByIdObjectReferenceField)
                        {
                            var fld = field as PAM_Model.ByIdObjectReferenceField;

                            if (internalIntObjectHelpers.ContainsKey(fld.objectTypeId))
                            {
                                var refer = internalIntObjectHelpers[fld.objectTypeId];
                                me.WithLookup(String.Join(".", (parentPath).Append(field.name)), refer);
                            }

                            var myLLT = LookyLooLookup.GetSingle(fld.objectTypeId);
                            myLLT.Wait();
                            var myLL = myLLT.Result;

                            Lookupable<object, int> lu = myLL;

                            if (lu != null)
                            {
                                me.WithLookup(String.Join(".", (parentPath).Append(field.name)), lu);
                            }
                        }
                        else if (field is PAM_Model.ByNameObjectReferenceField)
                        {
                            var fld = field as PAM_Model.ByNameObjectReferenceField;

                            if (internalStringObjectHelpers.ContainsKey(fld.objectTypeId))
                            {
                                var refer = internalStringObjectHelpers[fld.objectTypeId];
                                me.WithLookup(String.Join(".", (parentPath).Append(field.name)), refer);
                            }
                            else
                            {

                                var myLLT = LookyLooLookup.GetSingle(fld.objectTypeId);
                                myLLT.Wait();
                                var myLL = myLLT.Result;


                                Lookupable<object, string> lu = myLL;

                                if (lu != null)
                                {


                                    me.WithLookup(String.Join(".", (parentPath).Append(field.name)), lu);
                                }
                            }
                        }
                    }; break;
                case PAM_Model.FieldType.Choice:
                    {
                        if (field is PAM_Model.ConfigChoiceField)
                        {
                            var fld = field as PAM_Model.ConfigChoiceField;

                            me.addConfigLookup(String.Join(".", (parentPath).Append(field.name)), fld.configLookupReference);
                        }
                        else if (field is PAM_Model.TwoLevelConfigChoiceField)
                        {
                            var fld = field as PAM_Model.TwoLevelConfigChoiceField;

                            //TODO: Change this to handle nested (cluster) fields
                            me.WithTwoLevelConfigLookup(fld.parentField, fld.name, fld.configLookupReference);
                        }
                        else if (field is PAM_Model.ChoiceField)
                        {
                            var fld = field as PAM_Model.ChoiceField;

                            //TODO
                            //me.addChoiceLookup(fld.name, fld.lookupReference);
                        }
                    }; break;
                case PAM_Model.FieldType.ClusterField:
                    {
                        //TODO: Calculate a path
                        //TODO: Figure out how to handle nested fields
                        //We only need one level right now

                        foreach(var (type,fld) in (field as PAM_Model.ClusterField).subfields)
                        {
                            //I think I am thinking about this wrongly maybe. We need to figure out
                            //the fields type inside of the clusterfild but we dont keep track of type?
                            //Also if we just add the field to the model from the beginning
                            //Wont it just work
                            //Assuming we add in a path of some sort
                            //Big brain thinks Tuple<FieldType,Field>
                            me.FromSingleField(type, fld, internalIntObjectHelpers, internalStringObjectHelpers, LookyLooLookup, parentPath.Append(field.name));
                        }
                    }; break;
            }

            return me;
        }
    }

    //This class streamlines dynamic field hydration driven by configuration (soon)
    public class HydroHomie
    {
        private Lookupable<Model.DTOs.UserDTO, Guid> usersHelper;
        private Lookupable<object, Tuple<string, int>> genericConfigLookupHelperLU;
        private Lookupable<object, Tuple<string, IEnumerable<int>>> genericConfigHelperLU_ML;

        private Dictionary<string, List<Lookupable<object, int>>> relationalByIntLookups = new Dictionary<string, List<Lookupable<object, int>>>();
        private Dictionary<string, Lookupable<object, string>> relationalByStringLookups = new Dictionary<string, Lookupable<object, string>>();
        private Dictionary<string, string> configLookups = new Dictionary<string, string>();
        private Dictionary<string, Tuple<string, string>> twoLevelConfigLookups = new Dictionary<string, Tuple<string, string>>();

        private Dictionary<String, LookupType> lookups = new Dictionary<string, LookupType>();

        private enum LookupType
        {
            LookupByInt,
            LookupByString,
            LookupByConfig,
            LookupByConfigTwoLevel
        }

        public void setUsersHelper(Lookupable<Model.DTOs.UserDTO, Guid> usersHelper)
        {
            this.usersHelper = usersHelper;
        }
        public void setGenericConfigHelper(Lookupable<object, Tuple<string, int>> genericConfigLookupHelper)
        {
            this.genericConfigLookupHelperLU = genericConfigLookupHelper;
        }
        public void setGenericConfigHelperML(Lookupable<object, Tuple<string, IEnumerable<int>>> genericConfigLookupHelper)
        {
            this.genericConfigHelperLU_ML = genericConfigLookupHelper;
        }

        public void addLookup(string fieldName, Lookupable<object, int> lookup, PAM_Model.Field parent = null)
        {
            if (!relationalByIntLookups.ContainsKey(fieldName))
                relationalByIntLookups[fieldName] = new List<Lookupable<object, int>>();

            relationalByIntLookups[fieldName].Add(lookup);
            lookups[fieldName] = LookupType.LookupByInt;
        }

        public void addLookup(string fieldName, Lookupable<object, string> lookup)
        {
            relationalByStringLookups[fieldName] = lookup;
            lookups[fieldName] = LookupType.LookupByString;
        }

        public void addConfigLookup(string fieldName, string lookup)
        {
            configLookups[fieldName] = lookup;
            lookups[fieldName] = LookupType.LookupByConfig;
        }

        public void addConfigLookup(string fieldName, string parent, string config)
        {
            twoLevelConfigLookups[fieldName] = new Tuple<string, string>(parent, config);
            lookups[fieldName] = LookupType.LookupByConfigTwoLevel;
        }

        public async Task<PAM_Model.DynamicObject> HydrateSingle(PAM_Model.DynamicObject project)
        {
            if (project == null)
                return null;

            await project.HydrateCreateMod(usersHelper);

            //Dont try to hydrate wehn we aint got no data
            if (project.fields == null || project.fields.Type == JTokenType.Null)
                return project;

            List<String> fieldsToHydrate = lookups.Keys.ToList();

            foreach (var field in fieldsToHydrate)
            {
                if (lookups.ContainsKey(field))
                {
                    switch (lookups[field])
                    {
                        case LookupType.LookupByInt:
                            {
                                await project.HydrateRelationalIntField(field, relationalByIntLookups[field]);
                            }; break;
                        case LookupType.LookupByString:
                            {
                                await project.HydrateRelationalStringField(field, relationalByStringLookups[field]);
                            }; break;
                        case LookupType.LookupByConfig:
                            {
                                var t = configLookups[field];
                                await project.HydrateChoiceConfigLookup(field, t, genericConfigLookupHelperLU);
                            }; break;
                        case LookupType.LookupByConfigTwoLevel:
                            {
                                var t = twoLevelConfigLookups[field];
                                await project.HydrateTwoLevelChoiceConfigLookup(t.Item1, field, t.Item2, genericConfigHelperLU_ML);
                            }; break;
                    }
                }
            }

            return project;
        }

        public PAM_Model.DynamicObject DehydrateSingle(PAM_Model.DynamicObject project)
        {
            project.DehydrateCreateMod(usersHelper);

            List<String> fieldsToHydrate = lookups.Keys.ToList();

            foreach (var field in fieldsToHydrate)
            {
                if (lookups.ContainsKey(field))
                {
                    switch (lookups[field])
                    {
                        case LookupType.LookupByInt:
                            {
                                project.DehydrateRelationalIntField(field);
                            }; break;
                        case LookupType.LookupByString:
                            {
                                project.DehydrateRelationalStringField(field);
                            }; break;
                        case LookupType.LookupByConfig:
                            {
                                var t = configLookups[field];
                                project.DehydrateChoiceConfigLookup(field);
                            }; break;
                        case LookupType.LookupByConfigTwoLevel:
                            {
                                var t = twoLevelConfigLookups[field];
                                project.DehydrateTwoLevelChoiceConfigLookup(t.Item1, field);
                            }; break;
                    }
                }
            }

            return project;
        }


        public bool ValidRelationalIntField(String field, PAM_Model.DynamicObject obj)
        {
            try
            {
                var tok = obj.fields.SelectToken(field);

                if (tok == null)
                    return true;

                if (tok.Type == JTokenType.Null)
                    return true;

                if (tok.Type == JTokenType.Array)
                {
                    List<object> newValues = new List<object>();
                    foreach (var tok2 in tok)
                    {
                        int? id2 = null;

                        if (tok2.Type == JTokenType.Integer)
                            id2 = tok2.Value<int>();
                        else if (tok2.Type == JTokenType.Object)
                            id2 = tok2.Value<int>("id");

                        if (id2 == null)
                            return false;

                    }
                }
                else
                {
                    int? id = null;

                    if (tok.Type == JTokenType.Integer)
                        id = tok.Value<int>();
                    else if (tok.Type == JTokenType.Object)
                        id = tok.Value<int>("id");

                    if (id == null)
                        return false;
                }
            }
            catch 
            {
                return false;
            }

            return true;
        }

        public bool ValidRelationalStringField(String field, PAM_Model.DynamicObject obj)
        {


            try
            {
                var tok = obj.fields.SelectToken(field);

                if (tok == null)
                    return true;

                if (tok.Type == JTokenType.Null)
                    return true;

                if (tok.Type == JTokenType.Array)
                {
                    List<object> newValues = new List<object>();
                    foreach (var tok2 in tok)
                    {
                        string id2 = null;

                        if (tok2.Type == JTokenType.String)
                            id2 = tok2.Value<string>();
                        else if (tok2.Type == JTokenType.Object)
                            id2 = tok2.Value<string>("id");

                        if (id2 == null)
                            return false;

                    }
                }
                else
                {
                    string id = null;

                    if (tok.Type == JTokenType.String)
                        id = tok.Value<string>();
                    else if (tok.Type == JTokenType.Object)
                        id = tok.Value<string>("id");

                    if (id == null)
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }


        public IEnumerable<Tuple<string,bool>> HoldsWater(PAM_Model.DynamicObject project)
        {
            
            List<String> fieldsToHydrate = lookups.Keys.ToList();

            foreach (var field in fieldsToHydrate)
            {
                if (lookups.ContainsKey(field))
                {
                    switch (lookups[field])
                    {
                        case LookupType.LookupByInt:
                            {
                                if (!ValidRelationalIntField(field, project))
                                    yield return new Tuple<string,bool> ( field, false);
                            }; break;
                        case LookupType.LookupByString:
                            {
                                if (!ValidRelationalStringField(field, project))
                                    yield return new Tuple<string, bool>(field, false);

                            }; break;
                        case LookupType.LookupByConfig:
                            {
                                if (!ValidRelationalIntField(field, project))
                                    yield return new Tuple<string, bool>(field, false);

                            }; break;
                        case LookupType.LookupByConfigTwoLevel:
                            {
                                if (!ValidRelationalIntField(field, project))
                                    yield return new Tuple<string, bool>(field, false);

                            }; break;
                    }
                }
            }

            yield break;

        }

        public async IAsyncEnumerable<PAM_Model.DynamicObject> HydrateAll(IEnumerable<PAM_Model.DynamicObject> all)
        {
            await foreach (var project in all.HydrateCreateMod(usersHelper))
            {

                List<String> fieldsToHydrate = lookups.Keys.ToList();

                foreach (var field in fieldsToHydrate)
                {
                    if (lookups.ContainsKey(field))
                    {
                        switch (lookups[field])
                        {
                            case LookupType.LookupByInt:
                                {
                                    await project.HydrateRelationalIntField(field, relationalByIntLookups[field]);
                                }; break;
                            case LookupType.LookupByString:
                                {
                                    await project.HydrateRelationalStringField(field, relationalByStringLookups[field]);
                                }; break;
                            case LookupType.LookupByConfig:
                                {
                                    var t = configLookups[field];
                                    await project.HydrateChoiceConfigLookup(field, t, genericConfigLookupHelperLU);
                                }; break;
                            case LookupType.LookupByConfigTwoLevel:
                                {
                                    var t = twoLevelConfigLookups[field];
                                    await project.HydrateTwoLevelChoiceConfigLookup(t.Item1, field, t.Item2, genericConfigHelperLU_ML);
                                }; break;
                        }
                    }
                }

                yield return project;
            }

            yield break;
        }
    }

}
