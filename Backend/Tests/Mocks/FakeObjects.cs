using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model = com.timmons.cognitive.API.Model;

namespace Tests.Mocks
{
    public class FakeObjects
    {

        public static IEnumerable<Model.DynamicObject> FakeObjectsTypeA = new List<Model.DynamicObject> {
                new Model.DynamicObject
                {
                    id = 1,
                    name = "Test Fake Object 1",
                    fields = JObject.FromObject(new
                    {
                        test = new
                        {
                            id = 1
                        }
                    }),
                    created = 1234,
                    modified = 1234,
                    createdBy = new Model.User
                    {
                        id = UsersHelperMock.testUser1ID
                    },
                    lastModifiedBy = new Model.User
                    {
                        id = UsersHelperMock.testUser1ID
                    }
                },
                new Model.DynamicObject
                {
                    id = 2,
                    name = "Test Fake Object 2",
                    fields = JObject.FromObject(new
                    {
                        test2 = new
                        {
                            id = 1
                        }
                    }),
                    created = 1234,
                    modified = 1234,
                    createdBy = new Model.User
                    {
                        id = UsersHelperMock.testUser1ID
                    },
                    lastModifiedBy = new Model.User
                    {
                        id = UsersHelperMock.testUser2ID
                    }
                },
                new Model.DynamicObject
                {
                    id = 2,
                    name = "Test Fake Object 3",
                    fields = null,
                    created = 1234,
                    modified = 1234,
                    createdBy = new Model.User
                    {
                        id = UsersHelperMock.testUser1ID
                    },
                    lastModifiedBy = new Model.User
                    {
                        id = UsersHelperMock.testUser2ID
                    }
                }
            };

        public static IEnumerable<Model.DynamicObject> FakeObjectsTypeB = new List<Model.DynamicObject> {
                new Model.DynamicObject
                {
                    id = 1,
                    name = "Second Test Fake Object 1",
                    fields = null,
                    created = 1234,
                    modified = 1234,
                    createdBy = new Model.User
                    {
                        id = UsersHelperMock.testUser3ID
                    },
                    lastModifiedBy = new Model.User
                    {
                        id = Guid.NewGuid()
                    }
                },
                new Model.DynamicObject
                {
                    id = 2,
                    name = "Test Fake Object 2",
                    fields = null,
                    created = 1234,
                    modified = 1234,
                    createdBy = new Model.User
                    {
                        id = Guid.NewGuid()
                    },
                    lastModifiedBy = new Model.User
                    {
                        id = Guid.NewGuid()
                    }
                }
            };
    }
    
}
