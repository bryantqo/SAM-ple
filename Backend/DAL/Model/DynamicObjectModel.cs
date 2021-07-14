using System;
using System.Collections.Generic;
using System.Text;

namespace com.timmons.cognitive.API.Model
{

    public class DynamicObjectModelNew : DynamicObjectType
    {
        public List<Tuple<FieldType, Field>> fields = new List<Tuple<FieldType, Field>>();
    }
    public static class DynamicObjectModelNewExt
    {
        public static DynamicObjectModelNew withField(this DynamicObjectModelNew me, FieldType fieldType, Field field)
        {
            me.fields.Add(new Tuple<FieldType,Field>(fieldType, field));

            return me;
        }

        public static DynamicObjectModelNew withTextField(this DynamicObjectModelNew me, TextField field)
        {
            return me.withField(FieldType.Text, field);
        }

        public static DynamicObjectModelNew withCurrencyField(this DynamicObjectModelNew me, CurrencyField field)
        {
            return me.withField(FieldType.Currency, field);
        }

        public static DynamicObjectModelNew withLongTextField(this DynamicObjectModelNew me, LongTextField field)
        {
            return me.withField(FieldType.LongText, field);
        }

        public static DynamicObjectModelNew withObjectReferenceField(this DynamicObjectModelNew me, ObjectReferenceField field)
        {
            return me.withField(FieldType.ObjectReference, field);
        }

        public static DynamicObjectModelNew withChoiceField(this DynamicObjectModelNew me, ChoiceField field)
        {
            return me.withField(FieldType.Choice, field);
        }

        public static DynamicObjectModelNew withChoiceField(this DynamicObjectModelNew me, ConfigChoiceField field)
        {
            return me.withField(FieldType.Choice, field);
        }

        public static DynamicObjectModelNew withChoiceField(this DynamicObjectModelNew me, TwoLevelConfigChoiceField field)
        {
            return me.withField(FieldType.Choice, field);
        }

        public static DynamicObjectModelNew withRawField(this DynamicObjectModelNew me, RawField field)
        {
            return me.withField(FieldType.None, field);
        }
    }

}
