using com.timmons.Stitch.Shared;
using System.Collections.Generic;
using System.Linq;
using PAM_Model = com.timmons.cognitive.API.Model;

namespace API.Middleware.Validators
{
    public class AmendmentValidators
    {
        public static Validator<PAM_Model.DynamicObject> NewAmendmentValidator(IConnection con)
        {
            return new newAmendmentValidator(con);
        }

        public static Validator<PAM_Model.DynamicObject> UpdateAmendmentValidator(IConnection con)
        {
            return new updateAmendmentValidator(con);
        }
    }

    class newAmendmentValidator : Validator<PAM_Model.DynamicObject>
    {
        public newAmendmentValidator(IConnection con)
        { }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            return Validation.valid();
        }
    }

    class updateAmendmentValidator : Validator<PAM_Model.DynamicObject>
    {
        public updateAmendmentValidator(IConnection con)
        { }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            return Validation.valid();
        }
    }
}
