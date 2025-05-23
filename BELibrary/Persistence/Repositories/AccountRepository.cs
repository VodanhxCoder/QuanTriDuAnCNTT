﻿using BELibrary.Core.Entity.Repositories;
using BELibrary.Core.Utils;
using BELibrary.DbContext;
using BELibrary.Entity;
using System.Linq;

namespace BELibrary.Persistence.Repositories
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(HospitalManagementDbContext context)
            : base(context)
        {
        }

        public Account ValidBEAccount(string username, string password)
        {
            var db = HospitalManagementDbContext;

            var passwordFactory = password + VariableExtensions.KeyCrypto;
            var passwordCrypto = CryptorEngine.Encrypt(passwordFactory, true);

            var account =
                  db.Accounts.FirstOrDefault(x =>
                  (x.Role == RoleKey.Admin || x.Role == RoleKey.Doctor)
                  && x.UserName.ToLower() == username.ToLower()
                  && x.Password == passwordCrypto);

            return account;
        }

        public Account GetAccountByUsername(string username)
        {
            //need implement ,bad
            var db = HospitalManagementDbContext;
            var account =
                  db.Accounts.FirstOrDefault(x =>
                      (x.Role == RoleKey.Admin || x.Role == RoleKey.Doctor)
                  && x.UserName.ToLower() == username.ToLower());

            return account;
        }

        public Account GetAccountFeByUsername(string username)
        {
            var db = HospitalManagementDbContext;
            var account =
                db.Accounts.FirstOrDefault(x =>
                    x.Role == RoleKey.Patient
                    && x.UserName.ToLower() == username.ToLower());

            return account;
        }

        public Account ValidFeAccount(string username, string password)
        {
            var db = HospitalManagementDbContext;

            var passwordFactory = password + VariableExtensions.KeyCryptorClient;
            var passwordCrypto = CryptorEngine.Encrypt(passwordFactory, true);

            var account =
                db.Accounts.FirstOrDefault(x =>
                    x.Role == RoleKey.Patient
                    && x.UserName.ToLower() == username.ToLower()
                    && x.Password == passwordCrypto);

            return account;
        }

        public HospitalManagementDbContext HospitalManagementDbContext => Context;
    }
}