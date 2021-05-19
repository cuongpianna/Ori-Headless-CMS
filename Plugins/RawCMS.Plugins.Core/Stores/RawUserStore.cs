﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RawCMS.Library.Core;
using RawCMS.Library.DataModel;
using RawCMS.Library.Service;
using RawCMS.Plugins.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RawCMS.Plugins.Core.Stores
{
    public class RawClaimsFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        public async Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            JObject userObj = JObject.FromObject(user);
            foreach (JProperty key in userObj.Properties())
            {
                if (key.HasValues && !key.Name.Contains("Password"))
                {
                    claims.Add(new Claim(key.Name, key.Value.ToString()));
                }
            }
            return claims;
        }

        public RawClaimsFactory(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            ClaimsPrincipal principal = await base.CreateAsync(user);
            return principal;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            ClaimsIdentity principal = await base.GenerateClaimsAsync(user);
            principal.AddClaims(await GetClaimsAsync(user));
            return principal;
        }
    }

    public class RawUserStore : IUserStore<IdentityUser>,

        IUserPasswordStore<IdentityUser>,
        IPasswordValidator<IdentityUser>,
        IUserClaimStore<IdentityUser>,
        IPasswordHasher<IdentityUser>,
        IProfileService
    {
        private readonly ILogger logger;
        private readonly AppEngine appEngine;
        private readonly CRUDService service;
        private const string collection = "_users";

        public RawUserStore(AppEngine appEngine, ILogger logger, CRUDService service)
        {
            this.appEngine = appEngine;
            this.logger = logger;
            this.service = service;

            InitData().Wait();
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = user.UserName.ToUpper();
            service.Insert(collection, JObject.FromObject(user));
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            service.Delete(collection, user.Id);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            JObject result = service.Get(collection, userId);
            var user = result.ToObject<IdentityUser>();
            user = await AddPasswordHash(user);
            return user;
        }

        private async Task<IdentityUser> AddPasswordHash(IdentityUser user)
        {
            DataQuery query = new DataQuery()
            {
                RawQuery = JsonConvert.SerializeObject(new { UserId = user.Id })
            };

            ItemList password = service.Query("_credentials", query);

            user.PasswordHash = password.Items.Single()["PasswordHash"].Value<string>();
            return user;
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            DataQuery query = new DataQuery()
            {
                RawQuery = JsonConvert.SerializeObject(new { NormalizedUserName = normalizedUserName })
            };

            ItemList result = service.Query(collection, query);
            if (result.TotalCount == 0)
            {
                return null;
            }

            var user = result.Items.First.ToObject<IdentityUser>();
            user = await AddPasswordHash(user);
            return user;
        }

        public async Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return user.NormalizedUserName;
        }

        public async Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return user.Id;
        }

        public async Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return user.UserName;
        }

        public async Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
        }

        public async Task InitData()
        {
            long count = 0L;
            try
            {
                count = service.Count(collection, null);
            }
            catch
            {
                //no error if missing collection
            }
            if (count == 0)
            {
                IdentityUser userToAdd = new IdentityUser()
                {
                    UserName = "admin",
                    NormalizedUserName = "admin",
                    Email = "admin@oriwave.com",
                    NormalizedEmail = "admin@oriwave.com",
                    NewPassword = "123456",//password will be hashed by service
                };

                userToAdd.Roles.Add("Admin");
                userToAdd.Roles.Add("User");
                await CreateAsync(userToAdd, CancellationToken.None);
            }
        }

        public async Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            service.Update(collection, JObject.FromObject(user), true);
            return IdentityResult.Success;
        }

        public async Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
        }

        public async Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return user.PasswordHash;
        }

        public async Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return true;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string password)
        {
            foreach (IPasswordValidator<IdentityUser> val in manager.PasswordValidators)
            {
                if (await val.ValidateAsync(manager, user, password) != IdentityResult.Success)
                {
                    return IdentityResult.Failed(new IdentityError[] {
                        new IdentityError()
                        {
                            Code="VALIDATION FAILED",
                            Description=val.ToString()
                        }
                    });
                }
            }
            return IdentityResult.Success;
        }

        public string HashPassword(IdentityUser user, string password)
        {
            return ComputePasswordHash(password);
        }

        public static string ComputePasswordHash(string password)
        {
            using (var algorithm = SHA256.Create())
            {
                // Create the at_hash using the access token returned by CreateAccessTokenAsync.
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        public PasswordVerificationResult VerifyHashedPassword(IdentityUser user, string hashedPassword, string providedPassword)
        {
            if (hashedPassword == HashPassword(user, providedPassword))
            {
                return PasswordVerificationResult.Success;
            }
            return PasswordVerificationResult.Failed;
        }

        public async Task<IList<Claim>> GetClaimsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                claims.Add(new Claim("RolesList", string.Join(",", user.Roles)));
            }

            JObject userObj = JObject.FromObject(user);
            foreach (JProperty key in userObj.Properties())
            {
                if (key.HasValues && !key.Name.Contains("Password"))//TODO: implement blacklists
                {
                    //TODO: manage metadata
                    claims.Add(new Claim(key.Name, key.Value.ToString()));
                }
            }
            return claims;
        }

        public async Task AddClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (Claim claim in claims)
            {
                //TODO:
            }
        }

        public async Task ReplaceClaimAsync(IdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            //TODO:
        }

        public async Task RemoveClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            //TODO:
        }

        public async Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            //TODO:
            return null;
        }

        public async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            ClaimsIdentity id = new ClaimsIdentity(user.UserName, ClaimTypes.NameIdentifier, ClaimTypes.Role);

            id.AddClaims(await GetClaimsAsync(user, CancellationToken.None));

            ClaimsPrincipal userprincipal = new ClaimsPrincipal();
            userprincipal.AddIdentity(id);
            return userprincipal;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            Claim userid = context.Subject.Claims.FirstOrDefault(x => x.Type == "sub");
            if (userid != null)
            {
                IdentityUser user = await FindByIdAsync(userid.Value, CancellationToken.None);
                IList<Claim> tokens = await GetClaimsAsync(user, CancellationToken.None);

                context.IssuedClaims.AddRange(tokens);
            }
            //context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
        }

        public static string NormalizeString(string value)
        {
            if (value == null) return null;
            return value.ToString().ToUpper().Trim();
        }
    }
}