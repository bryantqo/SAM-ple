using com.timmons.Stitch.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PAM_Model = com.timmons.cognitive.API.Model;
using PAM_API = com.timmons.cognitive.API.DAL;

namespace API.Middleware.Validators
{
    public class ProjectValidators
    {
        public static Validator<PAM_Model.DynamicObject> NewProjectValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            return new newProjectValidator(byNameLookup);
        }

        public static Validator<PAM_Model.DynamicObject> UpdateProjectValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            return new updateProjectValidator(byNameLookup);
        }
    }

    class newProjectValidator : Validator<PAM_Model.DynamicObject>
    {
        Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup;

        public newProjectValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            this.byNameLookup = byNameLookup;
        }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            List<Validator<PAM_Model.DynamicObject>> allValidators = new List<Validator<PAM_Model.DynamicObject>>();

            allValidators.Add(new projectNameNotBlankValidator());
            allValidators.Add(new projectNameLengthValidator());
            allValidators.Add(new projectTypeIsSetValidator());
            allValidators.Add(new projectChecklistTypeIsSetValidator());
            allValidators.Add(new projectPropertyIsSetValidator());
            allValidators.Add(new projectNameUniqueValidator(byNameLookup));

            var allResults =
                (from i in allValidators
                 select i.isValid(obj)).ToList();

            var anyInvalid = (from i in allResults where !i.isValid select i).Any();
            var allMessages = (from i in allResults select i.validationMessages).SelectMany(a => a).ToList();

            if (anyInvalid)
                return Validation.invalid(allMessages);

            return Validation.valid(allMessages);
        }
    }

    class updateProjectValidator : Validator<PAM_Model.DynamicObject>
    {
        Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup;

        public updateProjectValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            this.byNameLookup = byNameLookup;
        }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            List<Validator<PAM_Model.DynamicObject>> allValidators = new List<Validator<PAM_Model.DynamicObject>>();

            allValidators.Add(new projectNameLengthValidator());
            allValidators.Add(new projectTypeIsNotSetValidator());
            allValidators.Add(new projectSubtypeIsNotSetValidator());
            allValidators.Add(new projectChecklistTypeIsNotSetValidator());
            allValidators.Add(new projectPropertyIsNotSetValidator());
            allValidators.Add(new projectNumberIsNotSetValidator());
            allValidators.Add(new projectNameUniqueValidator(byNameLookup));

            var allResults =
                (from i in allValidators
                 select i.isValid(obj)).ToList();

            var anyInvalid = (from i in allResults where !i.isValid select i).Any();
            var allMessages = (from i in allResults select i.validationMessages).SelectMany(a => a).ToList();

            if (anyInvalid)
                return Validation.invalid(allMessages);

            return Validation.valid(allMessages);
        }
    }

    class projectNameNotBlankValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.name == null || obj.name.Trim().Length == 0)
            {
                return Validation.invalid("Project name must not be blank.");
            }
            return Validation.valid();
        }
    }

    class projectNameLengthValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.name != null && obj.name.Trim().Length > 50)
            {
                return Validation.invalid("Project name must be less than 50 characters long.");
            }
            return Validation.valid();
        }
    }

    class projectTypeIsSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields == null || !obj.fields.ContainsKey("type"))
            {
                return Validation.invalid("Project must have a type set.");
            }


            var typeField = obj.fields.SelectToken("type");

            if (typeField.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                typeField = typeField.SelectToken("id");

            if (typeField.Type != Newtonsoft.Json.Linq.JTokenType.Integer)
            {
                return Validation.invalid("Project type field must be an integer value.");
            }

            //TODO: Validate that its a valid value

            return Validation.valid();
        }
    }

    class projectTypeIsNotSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields != null && obj.fields.ContainsKey("type"))
            {
                return Validation.invalid("Project must not have a type set for update.");
            }

            //TODO: Validate that its a valid value

            return Validation.valid();
        }
    }

    class projectSubtypeIsNotSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields != null && obj.fields.ContainsKey("subtype"))
            {
                return Validation.invalid("Project must not have a subtype set for update.");
            }

            //TODO: Validate that its a valid value

            return Validation.valid();
        }
    }

    class projectChecklistTypeIsNotSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields != null && obj.fields.ContainsKey("checklistType"))
            {
                return Validation.invalid("Project must not have a checklist type set for update.");
            }

            //TODO: Validate that its a valid value

            return Validation.valid();
        }
    }

    class projectPropertyIsNotSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields != null && obj.fields.ContainsKey("property"))
            {
                return Validation.invalid("Project must not have a property set for update.");
            }

            //TODO: Validate that its a valid value

            return Validation.valid();
        }
    }

    class projectNumberIsNotSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields != null && obj.fields.ContainsKey("projectNumber"))
            {
                return Validation.invalid("Project must not have a projectNumber set for update.");
            }

            //TODO: Validate that its a valid value

            return Validation.valid();
        }
    }


    class projectChecklistTypeIsSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields == null || !obj.fields.ContainsKey("checklistType"))
            {
                return Validation.invalid("Project must have a checklist type set.");

            }

            var typeField = obj.fields.SelectToken("checklistType");

            if (typeField.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                typeField = typeField.SelectToken("id");

            if (typeField.Type != Newtonsoft.Json.Linq.JTokenType.Integer)
            {
                return Validation.invalid("Project checklist type field must be an integer value.");
            }

            //TODO: Validate that its a valid value

            return Validation.valid();
        }
    }

    class projectPropertyIsSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields == null || !obj.fields.ContainsKey("property") || obj.fields.GetValue("property").Type == Newtonsoft.Json.Linq.JTokenType.Null)
            {
                return Validation.invalid("Project must have a property.");
            }

            var propertyValue = obj.fields.Value<Newtonsoft.Json.Linq.JObject>("property");

            if (!propertyValue.ContainsKey("id") || propertyValue.GetValue("id").Type != Newtonsoft.Json.Linq.JTokenType.Integer)
            {
                return Validation.invalid("Property field must have an id field set to an integer value.");
            }

            //Lastly validate that the id from above is valid


            return Validation.valid();
        }
    }

    class projectNameUniqueValidator : Validator<PAM_Model.DynamicObject>
    {
        Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup;
        public projectNameUniqueValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            this.byNameLookup = byNameLookup;
        }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.name != null)
            {
                var results = new List<PAM_Model.DynamicObject>();

                var t = byNameLookup.Get(new List<string> { obj.name.Trim() });
                var e = t.GetAsyncEnumerator();

                bool moveNext = true;
                try
                {
                    while (moveNext)
                    {
                        var task = e.MoveNextAsync().AsTask(); 
                        task.Wait();
                        if (task.Result)
                            results.Add(e.Current);
                        else
                            moveNext = false;
                    }
                }
                finally
                {
                    var disp = e.DisposeAsync().AsTask();
                    disp.Wait();
                    disp.Dispose();
                }

                results = results.Where(a => a.id != obj.id).ToList();

                if (results.Any())
                    return Validation.invalid("A project with this name already exists.");
            }

            return Validation.valid();
        }
    }
}
