using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using API.Controllers.api;
using API.Util;
using com.timmons.Stitch.Shared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PAM_Model = com.timmons.cognitive.API.Model;
using PAMAPI = com.timmons.cognitive.API.DAL;

using API.Middleware.Helpers;



using Model = API.Middleware.Model;
//Model.DTOs.

namespace API.Helpers
{

    public class UsersHelper : Lookupable<Model.DTOs.UserDTO, Guid>, Lookupable<object, String>
    {
        private static readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        IConfiguration configuration;
        IConnection con;

        public UsersHelper(IConfiguration configuration, IConnection con)
        {
            this.con = con;
            this.configuration = configuration;
        }

        //public async Task<IEnumerable<UserDTO>> Get(IEnumerable<int> pamIDs)
        //{

        //}

        public async IAsyncEnumerable<Model.DTOs.UserDTO> Get(IEnumerable<Guid> cognitoIDs)
        {
            //So here we want to trigger a background sync if the data is stale but not to keep a reference to it 
            AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();

            var request = new ListUsersRequest();
            var poolId = configuration.GetValue<string>("Authentication:Cognito:PoolID");
            request.UserPoolId = poolId;


            var allIntUsers = await PAMAPI.DynamicObjectRepo.Get(con.Wrap(), (from i in cognitoIDs select i.ToString()).ToList(), Types.User).ToListAsync();

            var um = allIntUsers.ToDictionary(usr => usr.fields.ContainsKey("CognitoID") ? usr.fields["CognitoID"].ToString() : "unknown id " + usr.id, usr => usr);

            var getUserReq = new AdminGetUserRequest
            {
                UserPoolId = poolId
            };


            foreach (var id in cognitoIDs)
            {
                var cachedCopy = cache.Get<Model.DTOs.UserDTO>(id);

                if (cachedCopy != null)
                    yield return cachedCopy;
                else
                {
                    if(id == Guid.Empty)
                    {
                        var nDTO = new Model.DTOs.UserDTO
                        {
                            id = id.ToString(),
                            email = "System",
                            last = "System",
                            first = "System",
                            enabled = true
                        };


                        cache.Set<Model.DTOs.UserDTO>(id, nDTO, DateTime.Now.AddMinutes(5));

                        continue;
                    }
                    getUserReq.Username = id.ToString();
                    var getUserResp = await client.AdminGetUserAsync(getUserReq);

                    var user = getUserResp;

                    var mappedUser = um.ContainsKey(user.Username) ? um[user.Username] : null;
                    var email = user.UserAttributes.Where(a => a.Name == "email").Select(a => a.Value).FirstOrDefault();
                    var last = user.UserAttributes.Where(a => a.Name == "custom:lastName").Select(a => a.Value).FirstOrDefault();
                    var first = user.UserAttributes.Where(a => a.Name == "custom:firstName").Select(a => a.Value).FirstOrDefault();



                    var groupRequest = new AdminListGroupsForUserRequest();
                    groupRequest.Username = user.Username;
                    groupRequest.UserPoolId = poolId;
                    var groupResponse = await client.AdminListGroupsForUserAsync(groupRequest);
                    var enabled = user.Enabled;


                    var newDTO = new Model.DTOs.UserDTO
                    {
                        id = user.UserAttributes.Where(a => a.Name == "sub").Select(a => a.Value).FirstOrDefault(),
                        email = email,
                        last = last,
                        first = first,
                        //groups = (from grp in groupResponse.Groups select grp.GroupName.Replace("_", " ")).ToList()
                        enabled = enabled
                    };


                    cache.Set<Model.DTOs.UserDTO>(id, newDTO, DateTime.Now.AddMinutes(5));

                    yield return newDTO;
                }

            }

            yield break;
        }

        internal static async Task<IEnumerable<Model.DTOs.UserDTO>> GetAllByGroup(IEnumerable<string> roles, IConfiguration configuration, IConnection con)
        {
            AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();
            var poolId = configuration.GetValue<string>("Authentication:Cognito:PoolID");

            List<Model.DTOs.UserDTO> all = new List<Model.DTOs.UserDTO>();



            foreach (var grp in roles)
            {
                var request = new ListUsersInGroupRequest
                {
                    UserPoolId = poolId,
                    GroupName = grp.Replace(" ", "_")
                };
                var resp = await client.ListUsersInGroupAsync(request);

                do
                {
                    foreach (var user in resp.Users)
                    {
                        //var mappedUser = um.ContainsKey(user.Username) ? um[user.Username] : null;
                        var email = user.Attributes.Where(a => a.Name == "email").Select(a => a.Value).FirstOrDefault();
                        var last = user.Attributes.Where(a => a.Name == "custom:lastName").Select(a => a.Value).FirstOrDefault();
                        var first = user.Attributes.Where(a => a.Name == "custom:firstName").Select(a => a.Value).FirstOrDefault();
                        var enabled = user.Enabled;

                        all.Add(new Model.DTOs.UserDTO
                        {
                            id = user.Attributes.Where(a => a.Name == "sub").Select(a => a.Value).FirstOrDefault(),
                            email = email,
                            last = last,
                            first = first,
                            //groups = (from grp in groupResponse.Groups select grp.GroupName.Replace("_", " ")).ToList(),
                            enabled = enabled
                        });
                    }

                    if (resp.NextToken != null)
                    {
                        request.NextToken = resp.NextToken;
                        resp = await client.ListUsersInGroupAsync(request);
                    }
                }
                while (resp.NextToken != null) ;

            }

            return all.Distinct();


            
        }

        public static async Task<IEnumerable<Model.DTOs.UserDTO>> GetAll(IConfiguration configuration, IConnection con)
        {

            //So here we want to trigger a background sync if the data is stale but not to keep a reference to it 

            var cachedCopy = cache.Get<List<Model.DTOs.UserDTO>>("allCognitoUsers");

            if (cachedCopy != null)
                return cachedCopy;

            AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();

            var request = new ListUsersRequest();
            var poolId = configuration.GetValue<string>("Authentication:Cognito:PoolID");
            request.UserPoolId = poolId;


            var allIntUsers = (await PAMAPI.DynamicObjectRepo.GetAll(con.Wrap(), Types.User)).ToList();

            var um = allIntUsers.ToDictionary(usr => usr.fields.ContainsKey("CognitoID") ? usr.fields["CognitoID"].ToString() : "unknown id " + usr.id, usr => usr);

            var resp = await client.ListUsersAsync(request);

            List<Model.DTOs.UserDTO> all = new List<Model.DTOs.UserDTO>();

            do
            {
                foreach (var user in resp.Users)
                {
                    //var groupRequest = new AdminListGroupsForUserRequest();
                    //groupRequest.Username = user.Username;
                    //groupRequest.UserPoolId = poolId;
                    //var groupResponse = await client.AdminListGroupsForUserAsync(groupRequest);

                    var mappedUser = um.ContainsKey(user.Username) ? um[user.Username] : null;
                    var email = user.Attributes.Where(a => a.Name == "email").Select(a => a.Value).FirstOrDefault();
                    var last = user.Attributes.Where(a => a.Name == "custom:lastName").Select(a => a.Value).FirstOrDefault();
                    var first = user.Attributes.Where(a => a.Name == "custom:firstName").Select(a => a.Value).FirstOrDefault();
                    var enabled = user.Enabled;

                    all.Add(new Model.DTOs.UserDTO
                    {
                        id = user.Attributes.Where(a => a.Name == "sub").Select(a => a.Value).FirstOrDefault(),
                        email = email,
                        last = last,
                        first = first,
                        //groups = (from grp in groupResponse.Groups select grp.GroupName.Replace("_", " ")).ToList(),
                        enabled = enabled
                    });
                }

                if (resp.PaginationToken != null)
                {
                    request.PaginationToken = resp.PaginationToken;
                    resp = await client.ListUsersAsync(request);
                }
            }
            while (resp.PaginationToken != null);


            cache.Set<List<Model.DTOs.UserDTO>>("allCognitoUsers", all, DateTime.Now.AddMinutes(5)); //Only cache for 5 minutes tops

            return all;

        }

        public async Task<Model.DTOs.UserDTO> GetSingle(Guid id)
        {
            AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();

            var request = new ListUsersRequest();
            var poolId = configuration.GetValue<string>("Authentication:Cognito:PoolID");
            request.UserPoolId = poolId;


            var allIntUsers = await PAMAPI.DynamicObjectRepo.Get(con.Wrap(), new List<String> { id.ToString() }, Types.User).ToListAsync();

            var um = allIntUsers.ToDictionary(usr => usr.fields.ContainsKey("CognitoID") ? usr.fields["CognitoID"].ToString() : "unknown id " + usr.id, usr => usr);

            var getUserReq = new AdminGetUserRequest
            {
                UserPoolId = poolId
            };


            
            var cachedCopy = cache.Get<Model.DTOs.UserDTO>(id);

            if (cachedCopy != null)
                return cachedCopy;
            else
            {
                getUserReq.Username = id.ToString();
                var getUserResp = await client.AdminGetUserAsync(getUserReq);

                var user = getUserResp;

                var mappedUser = um.ContainsKey(user.Username) ? um[user.Username] : null;
                var email = user.UserAttributes.Where(a => a.Name == "email").Select(a => a.Value).FirstOrDefault();
                var last = user.UserAttributes.Where(a => a.Name == "custom:lastName").Select(a => a.Value).FirstOrDefault();
                var first = user.UserAttributes.Where(a => a.Name == "custom:firstName").Select(a => a.Value).FirstOrDefault();



                var groupRequest = new AdminListGroupsForUserRequest();
                groupRequest.Username = user.Username;
                groupRequest.UserPoolId = poolId;
                var groupResponse = await client.AdminListGroupsForUserAsync(groupRequest);
                var enabled = user.Enabled;


                var newDTO = new Model.DTOs.UserDTO
                {
                    id = user.UserAttributes.Where(a => a.Name == "sub").Select(a => a.Value).FirstOrDefault(),
                    email = email,
                    last = last,
                    first = first,
                    //groups = (from grp in groupResponse.Groups select grp.GroupName.Replace("_", " ")).ToList(),
                    enabled = enabled
                };


                cache.Set<Model.DTOs.UserDTO>(id, newDTO, DateTime.Now.AddMinutes(5));

                return newDTO;
            }

        }

        public async IAsyncEnumerable<object> Get(IEnumerable<string> cognitoIDs)
        {
            //So here we want to trigger a background sync if the data is stale but not to keep a reference to it 
            AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();

            var request = new ListUsersRequest();
            var poolId = configuration.GetValue<string>("Authentication:Cognito:PoolID");
            request.UserPoolId = poolId;


            var allIntUsers = await PAMAPI.DynamicObjectRepo.Get(con.Wrap(), (from i in cognitoIDs select i.ToString()).ToList(), Types.User).ToListAsync();

            var um = allIntUsers.ToDictionary(usr => usr.fields.ContainsKey("CognitoID") ? usr.fields["CognitoID"].ToString() : "unknown id " + usr.id, usr => usr);

            var getUserReq = new AdminGetUserRequest
            {
                UserPoolId = poolId
            };


            foreach (var id in cognitoIDs)
            {
                var cachedCopy = cache.Get<Model.DTOs.UserDTO>(id);

                if (cachedCopy != null)
                    yield return cachedCopy;
                else
                {
                    getUserReq.Username = id;
                    var getUserResp = await client.AdminGetUserAsync(getUserReq);

                    var user = getUserResp;

                    var mappedUser = um.ContainsKey(user.Username) ? um[user.Username] : null;
                    var email = user.UserAttributes.Where(a => a.Name == "email").Select(a => a.Value).FirstOrDefault();
                    var last = user.UserAttributes.Where(a => a.Name == "custom:lastName").Select(a => a.Value).FirstOrDefault();
                    var first = user.UserAttributes.Where(a => a.Name == "custom:firstName").Select(a => a.Value).FirstOrDefault();



                    //var groupRequest = new AdminListGroupsForUserRequest();
                    //groupRequest.Username = user.Username;
                    //groupRequest.UserPoolId = poolId;
                    //var groupResponse = await client.AdminListGroupsForUserAsync(groupRequest);
                    var enabled = user.Enabled;


                    var newDTO = new Model.DTOs.UserDTO
                    {
                        id = user.UserAttributes.Where(a => a.Name == "sub").Select(a => a.Value).FirstOrDefault(),
                        email = email,
                        last = last,
                        first = first,
                        //groups = (from grp in groupResponse.Groups select grp.GroupName.Replace("_", " ")).ToList(),
                        enabled = enabled
                    };


                    cache.Set<Model.DTOs.UserDTO>(id, newDTO, DateTime.Now.AddMinutes(5));

                    yield return newDTO;
                }

            }

            yield break;
        }


        //TODO: Google Users dont have their username set to the sub and instead are some strange id
        //We need to either change the UserDTO to reflect the username as the id (And thus nixing the guid)
        //or change this to search for the user by the sub https://docs.aws.amazon.com/cognito-user-identity-pools/latest/APIReference/API_ListUsers.html
        //https://docs.aws.amazon.com/cognito-user-identity-pools/latest/APIReference/API_ListUsers.html#CognitoUserPools-ListUsers-request-Filter
        public async Task<object> GetSingle(string id)
        {
            AmazonCognitoIdentityProviderClient client = new AmazonCognitoIdentityProviderClient();

            var request = new ListUsersRequest();
            var poolId = configuration.GetValue<string>("Authentication:Cognito:PoolID");
            request.UserPoolId = poolId;


            var allIntUsers = await PAMAPI.DynamicObjectRepo.Get(con.Wrap(), new List<string> { id.ToString() }, Types.User).ToListAsync();

            var um = allIntUsers.ToDictionary(usr => usr.fields.ContainsKey("CognitoID") ? usr.fields["CognitoID"].ToString() : "unknown id " + usr.id, usr => usr);

            var getUserReq = new AdminGetUserRequest
            {
                UserPoolId = poolId
            };



            var cachedCopy = cache.Get<Model.DTOs.UserDTO>(id);

            if (cachedCopy != null)
                return cachedCopy;
            else
            {
                getUserReq.Username = id;
                AdminGetUserResponse getUserResp = null;

                var newDTO = new Model.DTOs.UserDTO
                {
                    id = id,
                    email = "Unknown"
                };

                try
                {
                    getUserResp = await client.AdminGetUserAsync(getUserReq);

                    var user = getUserResp;

                    var mappedUser = um.ContainsKey(user.Username) ? um[user.Username] : null;
                    var email = user.UserAttributes.Where(a => a.Name == "email").Select(a => a.Value).FirstOrDefault();
                    var last = user.UserAttributes.Where(a => a.Name == "custom:lastName").Select(a => a.Value).FirstOrDefault();
                    var first = user.UserAttributes.Where(a => a.Name == "custom:firstName").Select(a => a.Value).FirstOrDefault();



                    var groupRequest = new AdminListGroupsForUserRequest();
                    groupRequest.Username = user.Username;
                    groupRequest.UserPoolId = poolId;
                    var groupResponse = await client.AdminListGroupsForUserAsync(groupRequest);
                    var enabled = user.Enabled;


                    newDTO = new Model.DTOs.UserDTO
                    {
                        id = user.Username, //user.UserAttributes.Where(a => a.Name == "sub").Select(a => a.Value).FirstOrDefault(),
                        email = email,
                        last = last,
                        first = first,
                        enabled = enabled
                    };

                }
                catch
                {
                }

                


                cache.Set<Model.DTOs.UserDTO>(id, newDTO, DateTime.Now.AddMinutes(5));

                return newDTO;
            }
        }
    }
    
}
