using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model = com.timmons.cognitive.API.Model;

namespace API.Util
{
    public static class Types
    {
        public static readonly Model.DynamicObjectType User = new Model.DynamicObjectType() { id = 0, key = "User", singular_name = "User", plural_name = "Users" };
        public static readonly Model.DynamicObjectType Project = new Model.DynamicObjectType() { id = 1, key = "Foo", singular_name = "Foo", plural_name = "Foos" };
        
    }
}
