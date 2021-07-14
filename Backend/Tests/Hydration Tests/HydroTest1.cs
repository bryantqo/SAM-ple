using API.Middleware.Helpers;
using com.timmons.cognitive.API.Model;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Mocks;
using PAM_Model = com.timmons.cognitive.API.Model;
using Model = API.Middleware.Model;
using System.Threading.Tasks;

namespace Tests.Hydration_Tests
{
    public class HydroTest1
    {

        HydroHomie waterBottle;

        [SetUp]
        public void Setup()
        {
            var configHelperMock = new GenericConfigHelperMock();

            var testModel = FakeObjectModels.FakeModel1;


            var nameLookerUppers = new Dictionary<int, Lookupable<object, string>>();
            var idLookerUppers = new Dictionary<int, Lookupable<object, int>>();

            var mockUsersHelper = new UsersHelperMock();

            waterBottle =
                new HydroHomie()
                .WithUsersHelper(mockUsersHelper)
                .WithGenericConfigHelper(configHelperMock)
                .WithGenericConfigHelperML(configHelperMock)
                .FromModel(testModel, mockUsersHelper, idLookerUppers, nameLookerUppers, new LookyLooLookupMock());
        }

        [Test]
        public async Task HydrateConfigChoiceTest()
        {
            var testObject = FakeObjects.FakeObjectsTypeA.First();
            var outObj = await waterBottle.HydrateSingle(testObject);

            var testfld = outObj.fields.SelectToken("test").ToObject<Model.DTOs.ChoiceDTO>();

            Assert.AreEqual(1, testfld.id);
            Assert.AreEqual("Test Val 1", testfld.name);

        }

        [Test]
        public async Task HydrateObjectReferenceFieldTest()
        {
            var testObject = FakeObjects.FakeObjectsTypeA.Skip(1).First();
            var outObj = await waterBottle.HydrateSingle(testObject);

            var testfld = outObj.fields.SelectToken("test2").ToObject<Model.DTOs.ChoiceDTO>();

            Assert.AreEqual(1, testfld.id);
            Assert.AreEqual("Second Test Fake Object 1", testfld.name);

        }

        [Test]
        public async Task HydrateObjectCreatedByTest()
        {
            var testObject = FakeObjects.FakeObjectsTypeA.Skip(2).First();
            var outObj = await waterBottle.HydrateSingle(testObject);

            Assert.AreEqual("bob dole", outObj.createdBy.name);

        }

        [Test]
        public async Task HydrateObjectLastModifiedByTest()
        {
            var testObject = FakeObjects.FakeObjectsTypeA.Skip(2).First();
            var outObj = await waterBottle.HydrateSingle(testObject);

            Assert.AreEqual("Alan@Rickman.com", outObj.lastModifiedBy.email);

        }
    }
}