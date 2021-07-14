using API.Middleware.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model = API.Middleware.Model;

namespace Tests.Mocks
{
    public class UsersHelperMock : Lookupable<Model.DTOs.UserDTO, Guid>
    {
        Dictionary<Guid, Model.DTOs.UserDTO> MockRepo = new Dictionary<Guid, Model.DTOs.UserDTO>();
        
        public static Guid testUser1ID = Guid.NewGuid();
        public static Guid testUser2ID = Guid.NewGuid();
        public static Guid testUser3ID = Guid.NewGuid();

        Model.DTOs.UserDTO testUser1;
        Model.DTOs.UserDTO testUser2;
        Model.DTOs.UserDTO testUser3;

        public UsersHelperMock()
        {
            testUser1 = new Model.DTOs.UserDTO
            {
                id = testUser1ID.ToString(),
                email = "bob@dole.com",
                first = "bob",
                last = "dole",
                enabled = true
            };

            testUser2 = new Model.DTOs.UserDTO
            {
                id = testUser2ID.ToString(),
                email = "Alan@Rickman.com",
                first = "Severus",
                last = "Snape",
                enabled = true
            };

            testUser3 = new Model.DTOs.UserDTO
            {
                id = testUser3ID.ToString(),
                email = "nick@frost",
                first = "Nick",
                last = "Frost",
                enabled = true
            };

            MockRepo.Add(testUser1ID, testUser1);
            MockRepo.Add(testUser2ID, testUser2);
            MockRepo.Add(testUser3ID, testUser3);
        }

        async IAsyncEnumerable<Model.DTOs.UserDTO> Lookupable<Model.DTOs.UserDTO, Guid>.Get(IEnumerable<Guid> lookup)
        {
            foreach(var id in lookup)
            {
                if (MockRepo.ContainsKey(id))
                    yield return MockRepo[id];
            }

            yield break;
        }

        async Task<Model.DTOs.UserDTO> Lookupable<Model.DTOs.UserDTO, Guid>.GetSingle(Guid lookup)
        {
            if (MockRepo.ContainsKey(lookup))
                return MockRepo[lookup];

            return null;
        }
    }
}
