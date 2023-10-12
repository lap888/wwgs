using Gs.Core;
using Gs.Domain.Context;
using Gs.Domain.Entity;
using Gs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Jobs
{
    /// <summary>
    /// 社区营业额
    /// </summary>
    public class MonthTurnoverDividend : IJob
    {
        private readonly IServiceProvider ServiceProvider;
        public MonthTurnoverDividend(IServiceProvider service)
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
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    WwgsContext SqlContext = service.ServiceProvider.GetRequiredService<WwgsContext>();
                    IWalletService WalletSub = service.ServiceProvider.GetRequiredService<IWalletService>();

                    DateTime Now = DateTime.Now;
                    List<TurnoverModel> Turnoveres = await SqlContext.StoreOrder.Where(item => item.State == Domain.Models.Store.OrderStatus.COMPLETED)
                        .Join(SqlContext.CommunityCenter, order => order.StoreId, community => community.Id, (order, community) => new TurnoverModel()
                        {
                            StoreId = order.StoreId,
                            CityCode = community.CityCode,
                            AreaCode = community.AreaCode,
                            Turnover = order.ServicePrice

                        }).ToListAsync();

                    #region 写入社区 营业额

                    List<TurnoverModel> CommunityTurnover = Turnoveres.GroupBy(item => item.StoreId).Select(item => new TurnoverModel()
                    {
                        StoreId = item.Key,
                        Turnover = item.Sum(o => o.Turnover)
                    }).ToList();

                    foreach (var item in CommunityTurnover)
                    {
                        var entity = new CommunityTurnover()
                        {
                            StoreId = item.StoreId,
                            Turnover = item.Turnover,
                            Date = Now.AddMonths(-1),
                            CreateTime = DateTime.Now,
                            Remark = string.Empty,
                        };
                        SqlContext.Add(entity);
                        if (SqlContext.SaveChanges() < 1) { SystemLog.Warn($"社区营业额=》{entity.ToJson()}"); }
                    }
                    #endregion

                    #region 区县 营业额奖励

                    #endregion

                    #region 城市 营业额奖励

                    #endregion

                    stopwatch.Stop();
                    SystemLog.Jobs($"社区营业额 执行完成,执行时间:{stopwatch.Elapsed.TotalSeconds}秒");
                }
                catch (Exception ex)
                {
                    SystemLog.Jobs("社区营业额 发生错误", ex);
                }
            }
        }


    }

    public class TurnoverModel
    {
        public Int64 StoreId { get; set; }
        public String CityCode { get; set; }
        public String AreaCode { get; set; }
        public Decimal Turnover { get; set; }
    }

}
