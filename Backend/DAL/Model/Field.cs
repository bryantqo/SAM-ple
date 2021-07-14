using System;
using System.Collections.Generic;
using System.Text;

namespace com.timmons.cognitive.API.Model
{



    public enum FieldType
    {
        None = 999,

        Text = 0,
        LongText = 1,
        Int = 2,
        Float = 3,
        Currency = 4,
        Date = 5,
        Flag = 6,
        
        Choice = 7,
        
        ObjectReference = 10,

        Spatial_Old = 11,
        Spatial = 20,

        LinkField = 100,
        ClusterField = 120

    }


    public class Field
    {
        public int id { get; set; }
        public string name;
    }

    public class RawField : Field
    {
        public Newtonsoft.Json.Linq.JToken raw { get; set; }
    }

    public class TextField : Field
    {
        public int? minLength { get; set; }
        public int? maxLength { get; set; }
    }

    public class CurrencyField : Field
    {
        public int? maxLength { get; set; }
    }

    public class LongTextField : Field
    {
        public int? minLength { get; set; }
        public int? maxLength { get; set; }
    }

    public class ChoiceField : Field
    {
        public int lookupReference { get; set; }
        public bool? multiple { get; set; }
    }

    public class ConfigChoiceField : Field
    {
        public string configLookupReference { get; set; }
        public bool? multiple { get; set; }
    }
    public class TwoLevelConfigChoiceField : Field
    {
        public string parentField { get; set; }
        public string configLookupReference { get; set; }
        public bool? multiple { get; set; }
    }

    public class ObjectReferenceField : Field
    {
        public int objectTypeId { get; set; }
        public int? syncFieldId { get; set; }
        public bool? multiple { get; set; }
    }

    public class ByIdObjectReferenceField : ObjectReferenceField
    {
        public bool useId { get; set; }
    }

    public class ByNameObjectReferenceField : ObjectReferenceField
    {
        public bool useName { get; set; }

    }

    public class ClusterField: Field
    {
        public List<Tuple<FieldType, Field>> subfields { get; set; }
    }

}
