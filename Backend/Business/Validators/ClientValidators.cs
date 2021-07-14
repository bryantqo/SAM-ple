using com.timmons.Stitch.Shared;

using PAM_Model = com.timmons.cognitive.API.Model;

namespace API.Middleware.Validators
{
    public class ClientValidators
    {
        public static Validator<PAM_Model.DynamicObject> NewPropertyValidator(IConnection con)
        {
            return new newClientValidator(con);
        }

        public static Validator<PAM_Model.DynamicObject> UpdatePropertyValidator(IConnection con)
        {
            return new updateClientValidator(con);
        }
    }

    class newClientValidator : Validator<PAM_Model.DynamicObject>
    {
        public newClientValidator(IConnection con)
        { }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            return Validation.valid();
        }
    }

    class updateClientValidator : Validator<PAM_Model.DynamicObject>
    {
        public updateClientValidator(IConnection con)
        { }

        public Validation isValid(PAM_Model.DynamicObject obj)
        {
            return Validation.valid();
        }
    }
}
