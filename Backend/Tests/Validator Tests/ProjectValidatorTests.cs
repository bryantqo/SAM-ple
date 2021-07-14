using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Mocks;
using Model = com.timmons.cognitive.API.Model;

namespace Tests.Validator_Tests
{
    public class ProjectValidatorTests
    {
        API.Middleware.Validators.Validator<Model.DynamicObject> newValidator;
        API.Middleware.Validators.Validator<Model.DynamicObject> updateValidator;

        [SetUp]
        public void Setup()
        {
            var fakeLookup = new LookyLooMock()
                .WithMockObject(new Model.DynamicObject
                {
                    id = 1,
                    name = "Test Fake Object 1",
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
                })
                .WithMockObject(new Model.DynamicObject
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
                });

            newValidator = API.Middleware.Validators.ProjectValidators.NewProjectValidator(fakeLookup);
            updateValidator = API.Middleware.Validators.ProjectValidators.UpdateProjectValidator(fakeLookup);
        }

        [Test]
        public void Test_Name_Exists()
        {
            var fakeFields = JObject.FromObject(new
            {

            });

            var testObject = new Model.DynamicObject()
            {
                name = "Test Fake Object 1",
                created = 1234,
                modified = 1234,
                createdBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                lastModifiedBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                fields = fakeFields
            };

            var result = newValidator.isValid(testObject);

            Assert.IsFalse(result.isValid);

            Assert.NotZero(result.validationMessages.Count);
            Assert.AreEqual(result.validationMessages.Count, 4);
        }

        [Test]
        public void Test_Name_Is_Same_With_Trailing_Space()
        {
            var fakeFields = JObject.FromObject(new
            {

            });

            var testObject = new Model.DynamicObject()
            {
                name = "Test Fake Object 1 ",
                created = 1234,
                modified = 1234,
                createdBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                lastModifiedBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                fields = fakeFields
            };

            var result = newValidator.isValid(testObject);

            Assert.IsFalse(result.isValid);

            Assert.NotZero(result.validationMessages.Count);
            Assert.AreEqual(result.validationMessages.Count, 4);
        }

        [Test]
        public void Test_Name_Is_Different_But_Missing_Fields()
        {
            var fakeFields = JObject.FromObject(new
            {

            });

            var testObject = new Model.DynamicObject()
            {
                name = "Test Fake Object 1.1",
                created = 1234,
                modified = 1234,
                createdBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                lastModifiedBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                fields = fakeFields
            };

            var result = newValidator.isValid(testObject);

            Assert.IsFalse(result.isValid);

            Assert.NotZero(result.validationMessages.Count);
            Assert.AreEqual(result.validationMessages.Count, 3);
        }

        [Test]
        public void Test_Update_With_Same_Name()
        {
            var fakeFields = JObject.FromObject(new
            {

            });

            var testObject = new Model.DynamicObject()
            {
                id = 1,
                name = "Test Fake Object 1",
                created = 1234,
                modified = 1234,
                createdBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                lastModifiedBy = new Model.User
                {
                    id = Guid.NewGuid()
                },
                fields = fakeFields
            };

            var result = updateValidator.isValid(testObject);

            Assert.IsTrue(result.isValid);
        }
    }
}
