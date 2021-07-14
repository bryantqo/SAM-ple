using com.timmons.cognitive.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAM_Model = com.timmons.cognitive.API.Model;

namespace Tests.Mocks
{
    public class FakeObjectModels
    {
        public static PAM_Model.DynamicObjectModelNew FakeModel1 = (
            new PAM_Model.DynamicObjectModelNew
            {
                id = 1,
                singular_name = "test",
                plural_name = "tests",
                display = true,
                display_order = 10,
                key = "test",
                fields = new System.Collections.Generic.List<Tuple<PAM_Model.FieldType, PAM_Model.Field>>()
            })
        .withChoiceField(new ConfigChoiceField
        {
            id = 1,
            name = "test",
            configLookupReference = "TESTLOOKUP"
        })
        .withObjectReferenceField(new ByIdObjectReferenceField
        {
            id = 2,
            name = "test2",
            multiple = true,
            objectTypeId = 2,
            useId = true
        });
    }
}
