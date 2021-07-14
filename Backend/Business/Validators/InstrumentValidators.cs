using com.timmons.Stitch.Shared;
using System.Collections.Generic;
using System.Linq;
using PAM_Model = com.timmons.cognitive.API.Model;

namespace API.Middleware.Validators
{
    public class InstrumentValidators
    {
        public static Validator<PAM_Model.DynamicObject> NewInstrumentValidator(IConnection con)
        {
            return new newInstrumentValidator(con);
        }

        public static Validator<PAM_Model.DynamicObject> UpdateInstrumentValidator(IConnection con)
        {
            return new updateInstrumentValidator(con);
        }
    }

    class newInstrumentValidator : Validator<PAM_Model.DynamicObject>
    {
        public newInstrumentValidator(IConnection con)
        { }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            return Validation.valid();
        }
    }

    class updateInstrumentValidator : Validator<PAM_Model.DynamicObject>
    {
        public updateInstrumentValidator(IConnection con)
        { }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            return Validation.valid();
        }
    }
}
