using API.Middleware.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model = API.Middleware.Model;

namespace Tests.Mocks
{
    class GenericConfigHelperMock : Lookupable<object, Tuple<string, int>>, Lookupable<object, Tuple<string, IEnumerable<int>>>
    {

        Dictionary<string, Dictionary<int, Model.DTOs.ChoiceDTO>> singleLevelValues = new Dictionary<string, Dictionary<int, Model.DTOs.ChoiceDTO>>();
        Dictionary<string, Dictionary<IEnumerable<int>, Model.DTOs.ChoiceDTO>> multipleLevelValues = new Dictionary<string, Dictionary<IEnumerable<int>, Model.DTOs.ChoiceDTO>>();

        public GenericConfigHelperMock()
        {
            var testLookupValues = new Dictionary<int, Model.DTOs.ChoiceDTO>();
            testLookupValues.Add(1, new Model.DTOs.ChoiceDTO { id = 1, name = "Test Val 1" });
            testLookupValues.Add(2, new Model.DTOs.ChoiceDTO { id = 2, name = "Test Val 2" });
            testLookupValues.Add(3, new Model.DTOs.ChoiceDTO { id = 3, name = "Test Val 3" });

            var testLookupValues2 = new Dictionary<int, Model.DTOs.ChoiceDTO>();
            testLookupValues2.Add(1, new Model.DTOs.ChoiceDTO { id = 1, name = "SECOND Test Val 1" });
            testLookupValues2.Add(2, new Model.DTOs.ChoiceDTO { id = 2, name = "SECOND Test Val 2" });
            testLookupValues2.Add(3, new Model.DTOs.ChoiceDTO { id = 3, name = "SECOND Test Val 3" });

            singleLevelValues.Add("TESTLOOKUP", testLookupValues);
            singleLevelValues.Add("TESTLOOKUP2", testLookupValues);

            var multiLookup1 = new Dictionary<IEnumerable<int>, Model.DTOs.ChoiceDTO>();
            multiLookup1.Add(new List<int> { 1, 1 }, new Model.DTOs.ChoiceDTO { id = 1, name = "Multi Test 1" });
            multiLookup1.Add(new List<int> { 1, 2 }, new Model.DTOs.ChoiceDTO { id = 1, name = "Multi Test 2" });
            multiLookup1.Add(new List<int> { 1, 3 }, new Model.DTOs.ChoiceDTO { id = 1, name = "Multi Test 3" });

            var multiLookup2 = new Dictionary<IEnumerable<int>, Model.DTOs.ChoiceDTO>();
            multiLookup2.Add(new List<int> { 2, 1 }, new Model.DTOs.ChoiceDTO { id = 1, name = "SECOND Multi Test 1" });
            multiLookup2.Add(new List<int> { 2, 2 }, new Model.DTOs.ChoiceDTO { id = 1, name = "SECOND Multi Test 2" });
            multiLookup2.Add(new List<int> { 2, 3 }, new Model.DTOs.ChoiceDTO { id = 1, name = "SECOND Multi Test 3" });


            multipleLevelValues.Add("TESTLOOKUP3", multiLookup1);
            multipleLevelValues.Add("TESTLOOKUP4", multiLookup2);

        }

        async IAsyncEnumerable<object> Lookupable<object, Tuple<string, int>>.Get(IEnumerable<Tuple<string, int>> lookup)
        {
            foreach (var luid in lookup)
            {
                var lookupKey = luid.Item1;
                var id = luid.Item2;

                
                //Value isnt cached

                var newDTO = new Model.DTOs.ChoiceDTO
                {
                    id = id,
                    name = "Unknown"
                };


                
                yield return newDTO;


            }

            yield break;
        }

        async IAsyncEnumerable<object> Lookupable<object, Tuple<string, IEnumerable<int>>>.Get(IEnumerable<Tuple<string, IEnumerable<int>>> lookup)
        {
            foreach (var luid in lookup)
            {
                var lookupKey = luid.Item1;
                var idPath = luid.Item2;

                var id = idPath.LastOrDefault();

                


                //Cache wasnt set
                var newDTO = new Model.DTOs.ChoiceDTO
                {
                    id = id,
                    name = "Unknown"
                };


                

                yield return newDTO;

            }

            yield break;
        }

        async Task<object> Lookupable<object, Tuple<string, int>>.GetSingle(Tuple<string, int> lookup)
        {

            var lookupKey = lookup.Item1;
            var id = lookup.Item2;

            


            //The value wasnt cached, get a new version
            var newDTO = new Model.DTOs.ChoiceDTO
            {
                id = id,
                name = "Unknown"
            };


            if(singleLevelValues.ContainsKey(lookupKey))
            {
                var lu = singleLevelValues[lookupKey];
                if (lu.ContainsKey(id))
                    newDTO = lu[id];
            }

            


            return newDTO;

        }

        async Task<object> Lookupable<object, Tuple<string, IEnumerable<int>>>.GetSingle(Tuple<string, IEnumerable<int>> lookup)
        {

            var lookupKey = lookup.Item1;
            var idPath = lookup.Item2;

            var id = idPath.LastOrDefault();

            



            //The value wasnt cached, get a new version
            var newDTO = new Model.DTOs.ChoiceDTO
            {
                id = id,
                name = "Unknown"
            };


            


            return newDTO;


        }
    }
}
