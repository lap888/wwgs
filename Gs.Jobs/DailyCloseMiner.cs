using Dapper;
using Gs.Core;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Enums;
using Gs.Domain.Models;
using Gs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Gs.Jobs
{
    /// <summary>
    /// 每日关停量化宝
    /// </summary>
    public class DailyCloseMiner : IJob
    {
        private readonly IServiceProvider ServiceProvider;
        public DailyCloseMiner(IServiceProvider service)
        {
            this.ServiceProvider = service;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            using (var service = this.ServiceProvider.CreateScope())
            {
                try
                {
                    decimal ParentRate = 0.05M;     // 上级活跃度比率
                    // decimal CityRate = 0.01M;       // 城市活跃度比率
                    // decimal AreaRate = 0.05M;       // 区县活跃度比率

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    WwgsContext SqlContext = service.ServiceProvider.GetRequiredService<WwgsContext>();
                    ITeamService Team = service.ServiceProvider.GetRequiredService<ITeamService>();
                    IAdditionService Addition = service.ServiceProvider.GetRequiredService<IAdditionService>();

                    List<Domain.Entity.UserMining> UserMinings = await SqlContext.UserMining
                        .Where(item => item.State == MiningStatus.EFFECTIVE && (item.Duration < 1 || item.ExpiryDate < DateTime.Now))
                        .ToListAsync();

                    List<MiningInfo> Minings = UserMinings.Join(Constants.BaseSetting, user => user.BaseId, mining => mining.BaseId, (user, mining) => new MiningInfo()
                    {
                        Id = user.Id,
                        UserId = user.UserId,
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
                        CreateTime = user.CreateTime,
                        Colors = mining.Colors,
                        Remark = mining.Remark,
                    }).ToList();

                    foreach (var item in Minings)
                    {
                        await SqlContext.Dapper.ExecuteAsync("UPDATE `user_mining` SET `State` = 0 WHERE `Id` = @RecordId;", new { RecordId = item.Id });

                        MemberRelation Relation = await Team.GetRelation(item.UserId);
                        await Team.UpdateTeamKernel(Relation.MemberId, -item.Active);

                        //上级活跃度加成 扣除
                        await Addition.ChangeAmount(Relation.ParentId, -(item.Active * ParentRate), AdditionModifyType.SUBORDINATE_ADDITION_EXPIRED, false, item.BaseName);

                        // UserLocations Location = SqlContext.UserLocations.Where(d => d.UserId == Relation.MemberId).FirstOrDefault();

                        // // ====== 城市合伙人 加成活跃度 扣除
                        // if (!String.IsNullOrWhiteSpace(Location.CityCode))
                        // {
                        //     CityMaster CityInfo = SqlContext.CityMaster.Where(d => d.CityCode == Location.CityCode && d.StartDate < item.CreateTime).FirstOrDefault();
                        //     if (CityInfo != null)
                        //     {
                        //         await Addition.ChangeAmount(CityInfo.UserId, -(item.Active * CityRate), AdditionModifyType.CITY_ADDITION_EXPIRED, false, item.BaseName, item.Id.ToString());
                        //     }
                        // }

                        // // ====== 区县合伙人 加成活跃度 扣除
                        // if (!String.IsNullOrWhiteSpace(Location.AreaCode))
                        // {
                        //     CityMaster AreaInfo = SqlContext.CityMaster.Where(d => d.CityCode == Location.AreaCode && d.StartDate < item.CreateTime).FirstOrDefault();
                        //     if (AreaInfo != null)
                        //     {
                        //         await Addition.ChangeAmount(AreaInfo.UserId, -(item.Active * AreaRate), AdditionModifyType.DISTRICT_ADDITION_EXPIRED, false, item.BaseName, item.Id.ToString());
                        //     }
                        // }
                    }

                    stopwatch.Stop();
                    SystemLog.Jobs($"每日关停量化宝 执行完成,执行时间:{stopwatch.Elapsed.TotalSeconds}秒");
                }
                catch (Exception ex)
                {
                    SystemLog.Jobs("每日关停量化宝 发生错误", ex);
                }
            }
        }
    }
}
