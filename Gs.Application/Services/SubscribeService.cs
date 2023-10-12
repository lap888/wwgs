using System;
using CSRedis;
using Gs.Core;
using System.Linq;
using Gs.Domain.Models;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using System.Threading.Tasks;
using Gs.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Gs.Domain.Entity;
using Microsoft.Extensions.Options;

namespace Gs.Application.Services
{
    public class SubscribeService : ISubscribeService
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly Models.AppSetting AppSettings;

        public SubscribeService(IServiceProvider serviceProvider, IOptionsMonitor<Models.AppSetting> monitor)
        {
            ServiceProvider = serviceProvider;
            AppSettings = monitor.CurrentValue;
        }

        /// <summary>
        /// 消息订阅用户注册事件
        /// </summary>
        /// <param name="Msg">消息主体</param>
        /// <returns></returns>
        public async Task SubscribeMemberRegist(String Msg)
        {
            using (var service = ServiceProvider.CreateScope())
            {
                try
                {
                    var Team = service.ServiceProvider.GetRequiredService<ITeamService>();
                    SubscribeTeam info = Msg.JsonTo<SubscribeTeam>();
                    MemberRelation Relation = await Team.SetRelation(info.MemberId, info.ParentId);
                    await Team.UpdateTeamPersonnel(info.MemberId, 1);
                }
                catch (Exception ex)
                {
                    SystemLog.Debug($"消息订阅用户注册事件 发生错误\r\n消息内容===>PUBLISH Member_Regist \"{Msg}\"", ex);
                }
            }
        }

        /// <summary>
        /// 消息订阅用户认证事件
        /// </summary>
        /// <param name="Msg">消息主体</param>
        /// <returns></returns>
        public async Task SubscribeMemberCertified(String Msg)
        {
            using (var service = ServiceProvider.CreateScope())
            {
                try
                {
                    var Team = service.ServiceProvider.GetRequiredService<ITeamService>();
                    var Honor = service.ServiceProvider.GetRequiredService<IHonorService>();
                    var CottonSub = service.ServiceProvider.GetRequiredService<ICottonService>();
                    var Mining = service.ServiceProvider.GetRequiredService<IMiningService>();

                    Int32 BaseId = 1; // 赠送量化宝编号
                    SubscribeTeam info = Msg.JsonTo<SubscribeTeam>();
                    MemberRelation Relation = await Team.GetRelation(info.MemberId);
                    await Team.UpdateTeamDirectPersonnel(Relation.ParentId, 2);

                    //荣誉值
                    await Honor.ChangeAmount(Relation.ParentId, 2, HonorModifyType.MEMBER_REAL_NAME, false, info.Nick);
                    //贡献值 上级贡献
                    await CottonSub.ChangeAmount(Relation.ParentId, 50, CottonModifyType.MEMBER_REAL_NAME, false, info.Nick);
                    //自己实名贡献值
                    await CottonSub.ChangeAmount(info.MemberId, 50, CottonModifyType.REAL_NAME, false, info.Nick);
                    //更新邀请人等级
                    await Team.UpdateLevel((int)Relation.ParentId);

                    var rult = await Mining.Exchange(info.MemberId, BaseId, MiningSource.NEW_PEOPLE_GIVE); // 赠送量化宝
                }
                catch (Exception ex)
                {
                    SystemLog.Debug($"消息订阅用户认证事件 发生错误\r\n消息内容===>PUBLISH \"{Msg}\"", ex);
                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="golds"></param>
        /// <returns></returns>
        private string CaculatorGolds(decimal golds)
        {
            string UserLevel = String.Empty;

            foreach (var item in AppSettings.Levels.OrderBy(o => o.Claim))
            {
                if (golds >= item.Claim) { UserLevel = item.Level; }
            }
            return UserLevel;
        }

        /// <summary>
        /// 消息订阅量化宝开启事件
        /// </summary>
        /// <param name="Msg"></param>
        /// <returns></returns>
        public async Task SubscribeTaskAction(String Msg)
        {
            using (var service = ServiceProvider.CreateScope())
            {
                decimal ParentRate = 0.05M;     // 上级活跃度比率
                // decimal CityRate = 0.01M;       // 城市活跃度比率
                // decimal AreaRate = 0.05M;   // 区县活跃度比率
                try
                {
                    ITeamService Team = service.ServiceProvider.GetRequiredService<ITeamService>();
                    IHonorService Honor = service.ServiceProvider.GetRequiredService<IHonorService>();
                    IActiveService Active = service.ServiceProvider.GetRequiredService<IActiveService>();
                    IAdditionService Addition = service.ServiceProvider.GetRequiredService<IAdditionService>();

                    WwgsContext SqlContext = service.ServiceProvider.GetRequiredService<WwgsContext>();

                    SubscribeTeam info = Msg.JsonTo<SubscribeTeam>();

                    //==============================查找量化宝配置==============================//
                    MiningBase TaskSetting = Constants.BaseSetting.FirstOrDefault(o => o.BaseId == info.BaseId);
                    if (null == TaskSetting)
                    {
                        SystemLog.Warn($"消息订阅量化宝开启事件,未找到量化宝配置.\r\n{Msg}");
                        return;
                    }
                    //==============================查找量化宝配置==============================//
                    if (!info.RenewTask)
                    {
                        MemberRelation Relation = await Team.GetRelation(info.MemberId);
                        await Team.UpdateTeamKernel(Relation.MemberId, TaskSetting.Active);
                        // ====== 活跃度
                        await Active.ChangeAmount(Relation.MemberId, TaskSetting.Active, ActiveModifyType.EXCHANGE_MINER, false, TaskSetting.BaseName, info.RecordId);
                        // ====== 加成活跃度
                        await Addition.ChangeAmount(Relation.ParentId, TaskSetting.Active * ParentRate, AdditionModifyType.SUBORDINATE_ADDITION, false, TaskSetting.BaseName, info.RecordId);
                        // if (TaskSetting.HonorValue > 0)
                        // {
                        //     // ====== 荣誉值
                        //     await Honor.ChangeAmount(Relation.MemberId, TaskSetting.HonorValue, HonorModifyType.EXCHANGE_MINER, false, TaskSetting.BaseName);
                        // }

                        // UserLocations Location = SqlContext.UserLocations.Where(item => item.UserId == Relation.MemberId).FirstOrDefault();

                        // // ====== 城市合伙人 加成活跃度
                        // if (!String.IsNullOrWhiteSpace(Location.CityCode))
                        // {
                        //     CityMaster CityInfo = SqlContext.CityMaster.Where(item => item.CityCode == Location.CityCode && item.EndDate > DateTime.Now).FirstOrDefault();
                        //     if (CityInfo != null)
                        //     {
                        //         await Addition.ChangeAmount(CityInfo.UserId, TaskSetting.Active * CityRate, AdditionModifyType.CITY_ADDITION, false, TaskSetting.BaseName, info.RecordId);
                        //     }
                        // }

                        // // ====== 区县合伙人 加成活跃度
                        // if (!String.IsNullOrWhiteSpace(Location.AreaCode))
                        // {
                        //     CityMaster AreaInfo = SqlContext.CityMaster.Where(item => item.CityCode == Location.AreaCode && item.EndDate > DateTime.Now).FirstOrDefault();
                        //     if (AreaInfo != null)
                        //     {
                        //         await Addition.ChangeAmount(AreaInfo.UserId, TaskSetting.Active * AreaRate, AdditionModifyType.DISTRICT_ADDITION, false, TaskSetting.BaseName, info.RecordId);
                        //     }
                        // }
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.Debug($"消息订阅量化宝开启事件 发生错误\r\n消息内容===>PUBLISH Member_TaskAction \"{Msg}\"", ex);
                }

            }


        }
    }
}
