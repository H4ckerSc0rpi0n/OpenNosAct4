﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class AccountDAO : MappingBaseDAO<Account, AccountDTO>, IAccountDAO
    {
        #region Methods

        public DeleteResult Delete(long accountId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Account Account = context.Account.FirstOrDefault(c => c.AccountId.Equals(accountId));

                    if (Account != null)
                    {
                        context.Account.Remove(Account);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ACCOUNT_ERROR"), accountId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref AccountDTO account)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long accountId = account.AccountId;
                    Account entity = context.Account.FirstOrDefault(c => c.AccountId.Equals(accountId));

                    if (entity == null)
                    {
                        account = Insert(account, context);
                        return SaveResult.Inserted;
                    }
                    account = Update(entity, account, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), account.AccountId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public AccountDTO LoadById(long accountId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Account account = context.Account.FirstOrDefault(a => a.AccountId.Equals(accountId));
                    if (account != null)
                    {
                        return _mapper.Map<AccountDTO>(account);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public AccountDTO LoadByName(string name)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Account Account = context.Account.FirstOrDefault(a => a.Name.Equals(name));
                    if (Account != null)
                    {
                        return _mapper.Map<AccountDTO>(Account);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public void LogIn(string name)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void UpdateLastSessionAndIp(string name, int session, string ip)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Account Account = context.Account.FirstOrDefault(a => a.Name.Equals(name));
                    if (Account != null)
                    {
                        Account.LastSession = session;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void WriteGeneralLog(long accountId, string ipAddress, long? CharacterId, string logType, string logData)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    GeneralLog log = new GeneralLog
                    {
                        AccountId = accountId,
                        IpAddress = ipAddress,
                        Timestamp = DateTime.Now,
                        LogType = logType,
                        LogData = logData,
                        CharacterId = CharacterId
                    };

                    context.GeneralLog.Add(log);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private AccountDTO Insert(AccountDTO account, OpenNosContext context)
        {
            if (account.LastCompliment == DateTime.MinValue)
            {
                account.LastCompliment = DateTime.Now;
            }

            Account entity = _mapper.Map<Account>(account);
            entity.LastCompliment = DateTime.Now.AddDays(-1);
            context.Account.Add(entity);
            context.SaveChanges();
            return _mapper.Map<AccountDTO>(entity);
        }

        private AccountDTO Update(Account entity, AccountDTO account, OpenNosContext context)
        {
            if (entity != null)
            {
                // The Mapper breaks context.SaveChanges(), so we need to "map" the data by hand...
                // entity = _mapper.Map<Account>(account);
                entity.Authority = account.Authority;
                entity.LastCompliment = account.LastCompliment;
                // entity.LastSession = account.LastSession;
                entity.Name = account.Name;
                entity.Password = account.Password;
                context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
            }
            return _mapper.Map<AccountDTO>(entity);
        }

        #endregion
    }
}