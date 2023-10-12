using System;
using Quartz;
using CSRedis;
using Quartz.Spi;
using Quartz.Impl;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using Gs.Application.Services;
using Gs.Application;
using Gs.Application.Utils;
using Gs.Domain.Configs;

namespace XUnitTest
{
    public class CommServiceProvider
    {
        private IServiceProvider ServiceProvider;
        private IConfiguration Configuration;
        public IConfiguration GetConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath("D:\\source\\repos\\wwgs\\Gs.WebApi")
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            return Configuration;
        }

        public IServiceProvider GetServiceProvider()
        {
            GetConfiguration();
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddHttpClient("JPushSMS", client =>
            {
                String BaseStr = Security.Base64Encrypt($"{Constants.JpushKey}:{Constants.JpushSecret}");
                String AuthStr = $"Basic {BaseStr}";
                client.DefaultRequestHeaders.Add("Authorization", AuthStr);
                client.Timeout = new TimeSpan(0, 0, 0, 1, 500);
            });

            #region 系统配置注入
            services.Configure<Gs.Application.Models.AppSetting>(Configuration.GetSection("AppSetting"));
            services.Configure<Gs.Application.Models.TicketConfig>(Configuration.GetSection("TicketConfig"));
            #endregion

            #region 注入数据库
            services.AddSingleton(o => new CSRedisClient(Configuration.GetConnectionString("RedisConnection")));
            services.AddDbContextPool<WwgsContext>((serviceProvider, option) =>
            {
                option.UseMySql(Configuration.GetConnectionString("ServiceConStr"), myop =>
                {
                    myop.ServerVersion(new Version(5, 7, 18), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql)
                        .UnicodeCharSet(Pomelo.EntityFrameworkCore.MySql.Infrastructure.CharSet.Utf8mb4);
                });
            });
            #endregion

            #region 系统服务
            #region 系统 账户
            services.AddScoped<IActiveService, ActiveService>();
            services.AddScoped<IHonorService, HonorService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ICottonService, CottonService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IAdditionService, AdditionService>();
            services.AddScoped<IIntegralService, IntegralService>();
            #endregion

            services.AddScoped<IUserSerivce, UserSerivce>();
            services.AddScoped<IMiningService, MiningService>();
            services.AddScoped<ITeamService, TeamService>();

            services.AddScoped<IAliPayAction, AliPayAction>();
            services.AddScoped<IPaymentAction, PaymentAction>();
            services.AddScoped<IAddressService, AddressService>();

            services.AddSingleton<ISubscribeService, SubscribeService>();
            #endregion

            #region 注入微信配置
            IConfigurationSection WeChatConf = Configuration.GetSection("WeChatConfig");
            services.Configure<Gs.Application.Models.WeChatConfig>(WeChatConf);
            Gs.Application.Models.WeChatConfig WeChatConfig = WeChatConf.Get<Gs.Application.Models.WeChatConfig>();
            services.AddHttpClient(WeChatConfig.ClientName);
            services.AddScoped<IWechatPlugin, WeChatPlugin>();
            #endregion

            #region 支付宝配置
            IConfigurationSection AlipayConf = Configuration.GetSection("AlipayConfig");
            services.Configure<Gs.Application.Models.AlipayConfig>(AlipayConf);
            Gs.Application.Models.AlipayConfig AlipayConfig = AlipayConf.Get<Gs.Application.Models.AlipayConfig>();
            services.AddHttpClient(AlipayConfig.ClientName);
            services.AddScoped<IAlipay, Alipay>();
            #endregion

            #region 实名认证配置
            IConfigurationSection RealVerifyConf = Configuration.GetSection("RealVerifyConfig");
            services.Configure<Gs.Application.Models.RealVerifyConfig>(RealVerifyConf);
            Gs.Application.Models.RealVerifyConfig RealVerifyConfig = RealVerifyConf.Get<Gs.Application.Models.RealVerifyConfig>();
            services.AddHttpClient(RealVerifyConfig.ClientName);
            services.AddScoped<IRealVerify, RealVerify>();
            #endregion

            #region 腾讯云配置
            IConfigurationSection QCloudConf = Configuration.GetSection("QCloudConfig");
            services.Configure<Gs.Application.Models.QCloudConfig>(QCloudConf);
            Gs.Application.Models.QCloudConfig QCloudConfig = QCloudConf.Get<Gs.Application.Models.QCloudConfig>();
            services.AddHttpClient(QCloudConfig.ClientName);
            services.AddScoped<IQCloudPlugin, QCloudPlugin>();
            #endregion

            #region 定时量化宝注入
            IConfigurationSection JobsDetail = Configuration.GetSection("Jobs");
            services.Configure<List<Gs.Core.JobDetail>>(JobsDetail);
            services.AddSingleton<Gs.Core.JobScheduler>();
            //==============================量化宝组==============================//
            //services.AddTransient<Yoyo.Jobs.DailyCloseTask>();           //每日关闭过期量化宝
            //==============================量化宝组==============================//
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IJobFactory, Gs.Core.JobFactoryService>();
            #endregion


            ServiceProvider = services.BuildServiceProvider();

            #region 消息订阅
            ISubscribeService syncTeams = ServiceProvider.GetRequiredService<ISubscribeService>();
            CSRedisClient.SubscribeObject subScribe = null;
            CSRedisClient redis = ServiceProvider.GetRequiredService<CSRedisClient>();
            subScribe = redis.Subscribe(
                ("MEMBER_REGISTER", async msg => await syncTeams.SubscribeMemberRegist(msg.Body)),
                ("MEMBER_CERTIFIED", async msg => await syncTeams.SubscribeMemberCertified(msg.Body)),
                ("EXCHANGE_MINER", async msg => await syncTeams.SubscribeTaskAction(msg.Body))
            );
            #endregion

            #region 定时量化宝启动
            Gs.Core.JobScheduler jobInit = ServiceProvider.GetRequiredService<Gs.Core.JobScheduler>();
            async Task jobStart()
            {
                await jobInit.Start();
                //==============================量化宝==============================//
                //await jobInit.AddTask<Yoyo.Jobs.DailyCloseTask>();           //每日关闭过期量化宝
                //==============================量化宝==============================//
            }
            jobStart().Wait();
            #endregion

            return ServiceProvider;
        }
    }
}
