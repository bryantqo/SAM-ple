using com.timmons.Stitch.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using PAM_Model = com.timmons.cognitive.API.Model;

namespace API.Middleware.Validators
{
    public class PropertyValidators
    {
        public static Validator<PAM_Model.DynamicObject> NewPropertyValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            return new newPropertyValidator(byNameLookup);
        }

        public static Validator<PAM_Model.DynamicObject> UpdatePropertyValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            return new updatePropertyValidator(byNameLookup);
        }
    }

    class newPropertyValidator : Validator<PAM_Model.DynamicObject>
    {
        Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup;
        public newPropertyValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            this.byNameLookup = byNameLookup;
        }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            List<Validator<PAM_Model.DynamicObject>> allValidators = new List<Validator<PAM_Model.DynamicObject>>();

            allValidators.Add(new propertyNameNotBlankValidator());
            allValidators.Add(new propertyNameLengthValidator());
            allValidators.Add(new propertyTypeIsSetValidator());
            allValidators.Add(new propertyRegionIsSetValidator());
            allValidators.Add(new propertyCountyIsSetValidator());
            allValidators.Add(new propertyDateIsSetValidator());
            allValidators.Add(new propertyNameUniqueValidator(byNameLookup));

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

    class updatePropertyValidator : Validator<PAM_Model.DynamicObject>
    {
        Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup;
        public updatePropertyValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
        {
            this.byNameLookup = byNameLookup;
        }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            List<Validator<PAM_Model.DynamicObject>> allValidators = new List<Validator<PAM_Model.DynamicObject>>();

            allValidators.Add(new propertyNameNotBlankValidator());
            allValidators.Add(new propertyNameLengthValidator());
            allValidators.Add(new propertyTypeIsSetValidator());
            allValidators.Add(new propertyRegionIsSetValidator());
            allValidators.Add(new propertyCountyIsSetValidator());
            allValidators.Add(new propertyDateIsSetValidator());
            allValidators.Add(new propertyNameUniqueValidator(byNameLookup));

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

    class propertyNameNotBlankValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.name == null || obj.name.Trim().Length == 0)
            {
                return Validation.invalid("Property name must not be blank.");
            }
            return Validation.valid();
        }
    }

    class propertyNameLengthValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.name != null && obj.name.Trim().Length > 50)
            {
                return Validation.invalid("Property name must be less than 50 characters long.");
            }
            return Validation.valid();
        }
    }

    class propertyTypeIsSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields == null || !obj.fields.ContainsKey("type"))
            {
                return Validation.invalid("Property must have a type set.");
            }

            var typeField = obj.fields.SelectToken("type");

            if (typeField.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                typeField = typeField.SelectToken("id");

            if (typeField.Type != Newtonsoft.Json.Linq.JTokenType.Integer)
            {
                return Validation.invalid("Property type field must be an integer value.");
            }

            return Validation.valid();
        }
    }

    class propertyRegionIsSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if(obj.fields == null || !obj.fields.ContainsKey("region"))
            {
                return Validation.invalid("Property must have a region set.");
            }

            return Validation.valid();
        }
    }

    class propertyCountyIsSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if(obj.fields == null || !obj.fields.ContainsKey("county"))
            {
                return Validation.invalid("Property must have a county set.");
            }

            return Validation.valid();
        }
    }

    class propertyDateIsSetValidator : Validator<PAM_Model.DynamicObject>
    {
        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            if (obj.fields == null || !obj.fields.ContainsKey("establishedDate"))
            {
                return Validation.invalid("Property must have a date in MM/DD/YYYY format.");
            }

            return Validation.valid();
        }
    }

    class propertyNameUniqueValidator : Validator<PAM_Model.DynamicObject>
    {
        Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup;
        public propertyNameUniqueValidator(Helpers.Lookupable<PAM_Model.DynamicObject, string> byNameLookup)
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
                    return Validation.invalid("A property with this name already exists.");
            }

            return Validation.valid();
        }
    }
}
