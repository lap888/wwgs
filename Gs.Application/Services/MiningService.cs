using System;
using System.Linq;
using Gs.Domain.Models;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gs.Core;
using Gs.Domain.Enums;
using CSRedis;
using System.Linq.Expressions;
using Gs.Domain.Entity;
using Gs.Domain.Models.Admin;
using Dapper;
using Gs.Core.Utils;
using System.Data;
using System.Text;
using Gs.Core.Extensions;

namespace Gs.Application.Services
{
    /// <summary>
    /// 量化宝服务
    /// </summary>
    public class MiningService : IMiningService
    {
        private readonly WwgsContext context;
        private readonly CSRedisClient RedisCache;
        private readonly ICottonService CottonSub;
        private readonly IConchService ConchSub;
        private readonly IAdditionService AdditionSub;
        public MiningService(WwgsContext mySql, IConchService conch, CSRedisClient cSRedis, IHonorService honor, ICottonService cotton, IAdditionService addition)
        {
            context = mySql;
            CottonSub = cotton;
            RedisCache = cSRedis;
            AdditionSub = addition;
            ConchSub = conch;
        }

        /// <summary>
        /// 量化宝商店
        /// </summary>
        /// <returns></returns>
        public async Task<MyResult<List<MiningBase>>> Store()
        {
            MyResult<List<MiningBase>> result = new MyResult<List<MiningBase>>();
            return await Task.Run(() =>
            {
                result.Data = Constants.BaseSetting.Where(item => item.StoreShow == true).ToList();
                return result;
            });
        }

        /// <summary>
        /// 我的量化宝
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public async Task<MyResult<List<MiningInfo>>> MyBase(int UserId, MiningStatus Status)
        {
            MyResult<List<MiningInfo>> result = new MyResult<List<MiningInfo>>();
            result.Data = new List<MiningInfo>();
            try
            {
                var UserMinings = await context.UserMining.Where(item => item.UserId == UserId && item.State == Status).ToListAsync();
                result.Data = UserMinings.Join(Constants.BaseSetting, user => user.BaseId, mining => mining.BaseId, (user, mining) => new MiningInfo()
                {
                    BaseId = user.BaseId,
                    BaseName = mining.BaseName,
                    IsRenew = mining.IsRenew,
                    Active = mining.Active,
                    MaxHave = mining.MaxHave,
                    UnitPrice = mining.UnitPrice,
                    DayOut = mining.DayOut,
                    TotalOut = mining.TotalOut,
                    BeginDate = user.BeginDate,
                    ExpiryDate = user.ExpiryDate,
                    HonorValue = mining.HonorValue,
                    TaskDuration = user.Duration,
                    Colors = mining.Colors,
                    Remark = mining.Remark,
                }).ToList();
            }
            catch (Exception ex)
            {
                SystemLog.Debug("我的量化宝", ex);
            }
            return result;
        }

        /// <summary>
        /// 兑换量化宝
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="BaseId"></param>
        /// <param name="Source"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Exchange(long UserId, int BaseId, MiningSource Source = MiningSource.EXCHANGE_MINER)
        {
            MyResult<object> result = new MyResult<object>();
            var Mining = Constants.BaseSetting.FirstOrDefault(item => item.BaseId == BaseId);
            if (Mining == null) { return result.SetStatus(ErrorCode.InvalidData, "量化宝信息有误"); }
            var CottonAccount = await ConchSub.Info(UserId);
            if (Source != MiningSource.SYSTEM_AWARD && Source != MiningSource.NEW_PEOPLE_GIVE)
            {
                if (CottonAccount == null || CottonAccount.Usable < Mining.UnitPrice) { return result.SetStatus(ErrorCode.InvalidData, "账户余额不足"); }
            }

            CSRedisClientLock CacheLock = null;
            try
            {
                CacheLock = RedisCache.Lock($"EXCHANGE_MINE:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "操作太快了"); }

                var MinerCunt = await context.UserMining.Where(item => item.UserId == UserId && item.BaseId == BaseId && item.State == MiningStatus.EFFECTIVE).CountAsync();

                if (Mining.MaxHave <= MinerCunt) { return result.SetStatus(ErrorCode.InvalidData, "量化宝已超出上限"); }

                if (Source != MiningSource.SYSTEM_AWARD && Source != MiningSource.NEW_PEOPLE_GIVE)
                {
                    if (!await ConchSub.ChangeAmount(UserId, -Mining.UnitPrice, ConchModifyType.EXCHANGE_MINER, false, Mining.BaseName))
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "兑换失败");
                    }
                }

                var Now = DateTime.Now;
                var MiningEntity = new Domain.Entity.UserMining()
                {
                    UserId = UserId,
                    BaseId = BaseId,
                    BeginDate = Now.Date,
                    Duration = Mining.TaskDuration,
                    ExpiryDate = Now.AddDays(Mining.TaskExpires).Date,
                    CreateTime = Now,
                    UpTime = Now,
                    Remark = String.Empty,
                    Source = Source,
                    State = MiningStatus.EFFECTIVE,
                };
                context.UserMining.Add(MiningEntity);
                if (context.SaveChanges() < 1)
                {
                    SystemLog.Warn($"兑换量化宝:{UserId}:添加量化宝:{Mining.ToJson()}");
                }
                var Message = new SubscribeTeam() { RecordId = MiningEntity.Id.ToString(), BaseId = BaseId, MemberId = UserId, RenewTask = false };
                if (RedisCache.Publish("EXCHANGE_MINER", Message.ToJson()) < 1)
                {
                    SystemLog.Warn($"兑换量化宝:{UserId}:消息订阅:{Mining.ToJson()}");
                }
                result.Data = MiningEntity.Id.ToString();
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("兑换量化宝", ex);
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
            return result.SetStatus(ErrorCode.InvalidData, "兑换失败");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> DigMine(long UserId)
        {
            MyResult<Object> result = new MyResult<Object>();
            CSRedisClientLock CacheLock = null;
            Int32 DigMinerCunt = 1;
            try
            {
                CacheLock = RedisCache.Lock($"DIG_MINE:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "操作太快了"); }

                var UserMinings = await context.UserMining.Where(item => item.UserId == UserId && item.State == MiningStatus.EFFECTIVE).ToListAsync();
                var Minings = UserMinings.Join(Constants.BaseSetting, user => user.BaseId, mining => mining.BaseId, (user, mining) => new MiningInfo()
                {
                    BaseId = user.BaseId,
                    BaseName = mining.BaseName,
                    IsRenew = mining.IsRenew,
                    Active = mining.Active,
                    MaxHave = mining.MaxHave,
                    UnitPrice = mining.UnitPrice,
                    DayOut = mining.DayOut,
                    TotalOut = mining.TotalOut,
                    BeginDate = user.BeginDate,
                    ExpiryDate = user.ExpiryDate,
                    HonorValue = mining.HonorValue,
                    Duration = user.Duration,
                    TaskDuration = user.Duration,
                    Colors = mining.Colors,
                    Remark = mining.Remark,
                }).ToList();
                if (Minings.Count < 1) { return result.SetStatus(ErrorCode.InvalidData, "你没有量化宝哦"); }

                var DigMinerRecord = await context.UserDigMinerRecord.Where(item => item.UserId == UserId && item.CreateDate.Date == DateTime.Now.Date)
                    .FirstOrDefaultAsync();
                if (DigMinerRecord == null)
                {
                    DigMinerRecord = new UserDigMinerRecord()
                    {
                        UserId = UserId,
                        Schedule = DigMinerCunt,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        Source = 0,
                        Remark = String.Empty,
                    };
                    context.UserDigMinerRecord.Add(DigMinerRecord);
                    if (context.SaveChanges() < 1)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "请稍后重试...");
                    }
                }
                if (DigMinerRecord.Schedule < 1) { return result.SetStatus(ErrorCode.InvalidData, "今天量化宝已完成"); }

                // var Addition = await AdditionSub.Info(UserId);
                // Addition = Addition ?? new AccountInfo();
                // decimal AddOut = Addition.Balance * 10 / 30.0000M;

                decimal MyOut = Minings.Where(item => item.ExpiryDate > DateTime.Now && item.Duration > 0).Sum(item => item.DayOut);

                //下级直推
                var mSql = $"select id,baseId from `user_mining` where userId in (select id from user where `inviterMobile` in (select mobile from user where id={UserId}) and `auditState`=2)";
                var myInviterMinings = await context.Dapper.QueryAsync<UserMining>(mSql);
                //活跃加成
                // var myInviterMinings = await context.UserMining.Where(item => item.UserId == UserId && item.State == MiningStatus.EFFECTIVE).ToListAsync();
                decimal inviterCOut = 0;    //我的任务产量
                decimal TotalOut = Math.Round(MyOut / DigMinerCunt, 4);
                myInviterMinings.ToList().ForEach(minnings =>
                {
                    var tmp = Constants.BaseSetting.FirstOrDefault(item => item.BaseId == minnings.BaseId).DayOut;
                    tmp = tmp * 0.05M;
                    inviterCOut += tmp;
                    // if (minnings.BaseId == 1)
                    // {
                    //     tmp = tmp * 0.1M;
                    //     inviterCOut += tmp;
                    // }
                    // else
                    // {
                    //     tmp = tmp * 0.05M;
                    //     inviterCOut += tmp;
                    // }
                });
                inviterCOut = Math.Round(inviterCOut, 4);
                if (await ConchSub.ChangeAmount(UserId, TotalOut, ConchModifyType.DIG_MINER, false, TotalOut.ToString()))
                {
                    await ConchSub.ChangeAmount(UserId, inviterCOut, ConchModifyType.Inviter_DIG_MINER, false, inviterCOut.ToString());
                    if (DigMinerRecord.Schedule == DigMinerCunt)
                    {
                        for (int i = 0; i < UserMinings.Count; i++)
                        {
                            UserMinings[i].Duration = UserMinings[i].Duration - 1;
                        }
                        context.UserMining.UpdateRange(UserMinings);
                    }
                    DigMinerRecord.Schedule = DigMinerRecord.Schedule - 1;
                    if (context.SaveChanges() < 1) { SystemLog.Warn("减少次数失败"); }
                    return result;
                }
            }
            catch (Exception ex)
            {
                SystemLog.Debug("失败", ex);
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
            return result.SetStatus(ErrorCode.InvalidData, "量化宝失败");
        }


        #region 后台管理
        /// <summary>
        /// 量化宝列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<MiningInfo>>> BaseList(QueryMining query)
        {
            MyResult<List<MiningInfo>> result = new MyResult<List<MiningInfo>>();
            result.Data = new List<MiningInfo>();
            try
            {
                Expression<Func<UserMining, bool>> where = null;
                if (query.State != null) { where = where.AndAlso(item => item.State == query.State); }
                if (!string.IsNullOrWhiteSpace(query.Mobile))
                {
                    Int64 UserId = context.UserEntity.Where(item => item.Mobile == query.Mobile).Select(item => item.Id).FirstOrDefault();
                    where = where.AndAlso(item => item.UserId == UserId);
                }
                if (query.Source != null) { where = where.AndAlso(item => item.Source == query.Source); }
                if (query.BaseId > 0) { where = where.AndAlso(item => item.BaseId == query.BaseId); }

                if (where == null) { where = where.AndAlso(item => item.Id > 0); }
                var UserMinings = (await context.UserMining.Where(where).Join(context.UserEntity, mining => mining.UserId, user => user.Id, (mining, user) => new MiningInfo()
                {
                    Id = mining.Id,
                    Nick = user.Name,
                    Mobile = user.Mobile,
                    BaseId = mining.BaseId,
                    BeginDate = mining.BeginDate,
                    Duration = mining.Duration,
                    ExpiryDate = mining.ExpiryDate,
                    TaskDuration = mining.Duration,
                    CreateTime = mining.CreateTime,
                    State = mining.State,
                    Source = mining.Source,
                }).ToListAsync()).AsQueryable().Pages(query.PageIndex, query.PageSize, out int count, out int pageCount);
                result.PageCount = pageCount;
                result.RecordCount = count;

                result.Data = UserMinings.Join(Constants.BaseSetting, data => data.BaseId, mining => mining.BaseId, (data, mining) => new MiningInfo()
                {
                    Id = data.Id,
                    Nick = data.Nick,
                    Mobile = data.Mobile,
                    BaseId = data.BaseId,
                    BaseName = mining.BaseName,
                    IsRenew = mining.IsRenew,
                    Active = mining.Active,
                    MaxHave = mining.MaxHave,
                    UnitPrice = mining.UnitPrice,
                    DayOut = mining.DayOut,
                    TotalOut = mining.TotalOut,
                    BeginDate = data.BeginDate,
                    Duration = data.Duration,
                    ExpiryDate = data.ExpiryDate,
                    HonorValue = mining.HonorValue,
                    TaskDuration = data.TaskDuration,
                    CreateTime = data.CreateTime,
                    Colors = mining.Colors,
                    State = data.State,
                    Source = data.Source,
                    Remark = mining.Remark,
                }).ToList();

            }
            catch (Exception ex)
            {
                SystemLog.Debug("我的量化宝", ex);
            }
            return result;
        }

        /// <summary>
        /// 量化宝延期
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Extension(MiningInfo info)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                Int32 Days = info.Duration;
                var model = await context.UserMining.Where(item => item.Id == info.Id).FirstOrDefaultAsync();
                if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "量化宝不存在"); }
                model.ExpiryDate = model.ExpiryDate.AddDays(Days);
                model.Duration = model.Duration + Days;
                context.UserMining.Update(model);
                if (context.SaveChanges() > 0) { return result; }
            }
            catch (Exception ex) { SystemLog.Debug("量化宝延期", ex); }
            return result.SetStatus(ErrorCode.InvalidData, "延期失败");
        }

        /// <summary>
        /// 关闭量化宝
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Colse(MiningInfo info)
        {
            MyResult<Object> result = new MyResult<Object>();
            try
            {
                Int32 Days = info.Duration;
                var model = await context.UserMining.Where(item => item.Id == info.Id).FirstOrDefaultAsync();
                if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "量化宝不存在"); }
                model.State = MiningStatus.NO_EFFECTIVE;
                context.UserMining.Update(model);
                if (context.SaveChanges() > 0) { return result; }
            }
            catch (Exception ex) { SystemLog.Debug("关闭量化宝", ex); }
            return result.SetStatus(ErrorCode.InvalidData, "关闭量化宝失败");

        }

        /// <summary>
        /// 赠送量化宝
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Grant(QueryMining info)
        {
            MyResult<object> result = new MyResult<object>();
            var Mining = Constants.BaseSetting.FirstOrDefault(item => item.BaseId == info.BaseId);
            if (Mining == null) { return result.SetStatus(ErrorCode.InvalidData, "量化宝信息有误"); }
            CSRedisClientLock CacheLock = null;
            try
            {
                CacheLock = RedisCache.Lock($"GRANT_MINE:{info.BaseId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "操作太快了"); }
                var UserInfo = await context.UserEntity.Where(item => item.Mobile == info.Mobile).FirstOrDefaultAsync();

                var Now = DateTime.Now;
                var MiningEntity = new Domain.Entity.UserMining()
                {
                    UserId = UserInfo.Id,
                    BaseId = info.BaseId,
                    BeginDate = Now.Date,
                    Duration = Mining.TaskDuration,
                    ExpiryDate = Now.AddDays(Mining.TaskExpires).Date,
                    CreateTime = Now,
                    UpTime = Now,
                    Remark = String.Empty,
                    Source = MiningSource.SYSTEM_AWARD,
                    State = MiningStatus.EFFECTIVE,
                };
                context.UserMining.Add(MiningEntity);
                if (context.SaveChanges() < 1)
                {
                    SystemLog.Warn($"赠送量化宝:{UserInfo.Id}:添加量化宝:{Mining.ToJson()}");
                }
                result.Data = MiningEntity.Id.ToString();
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.Debug("兑换量化宝", ex);
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
            return result.SetStatus(ErrorCode.InvalidData, "兑换失败");

        }
        #endregion

    }
}
