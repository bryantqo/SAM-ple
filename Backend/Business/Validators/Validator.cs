using System.Collections.Generic;

namespace API.Middleware.Validators
{
    public class Validation
    {
        public bool isValid;
        public List<string> validationMessages;
        public string field { get; set; }


        public static Validation invalid(string why)
        {
            return invalid(new List<string> { why });
        }

        public static Validation invalid(List<string> why)
        {
            return new Validation
            {
                isValid = false,
                validationMessages = why
            };
        }

        public static Validation valid()
        {
            return new Validation
            {
                isValid = true,
                validationMessages = new List<string>()
            };
        }

        public static Validation valid(string why)
        {
            return valid(new List<string> { why });

        }

        public static Validation valid(List<string> why)
        {
            return new Validation
            {
                isValid = true,
                validationMessages = why
            };
        }
    }

    public interface Validator<E>
    {
        Validation isValid(E obj);
    }
}
