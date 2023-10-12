using System;
using CSRedis;
using Gs.Application;
using Newtonsoft.Json;
using Gs.Domain.Configs;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using Gs.Application.Utils;
using Gs.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Gs.Application.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gs.Core.Extensions;
using Gs.Core.Mvc;
using System.Collections.Generic;
using Quartz.Spi;
using Quartz;
using Quartz.Impl;
using System.Threading.Tasks;

namespace Gs.WebAdmin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient("JPushSMS", client =>
            {
                String BaseStr = Security.Base64Encrypt($"{Constants.JpushKey}:{Constants.JpushSecret}");
                String AuthStr = $"Basic {BaseStr}";
                client.DefaultRequestHeaders.Add("Authorization", AuthStr);
                client.Timeout = new TimeSpan(0, 0, 0, 1, 500);
            });

            #region 
            services.Configure<Gs.Application.Models.AppSetting>(Configuration.GetSection("AppSetting"));
            #endregion

            #region 
            IConfigurationSection WeChatConf = Configuration.GetSection("WeChatConfig");
            services.Configure<Gs.Application.Models.WeChatConfig>(WeChatConf);
            Gs.Application.Models.WeChatConfig WeChatConfig = WeChatConf.Get<Gs.Application.Models.WeChatConfig>();
            services.AddHttpClient(WeChatConfig.ClientName);
            services.AddScoped<IWechatPlugin, WeChatPlugin>();
            #endregion

            #region ֧
            IConfigurationSection AlipayConf = Configuration.GetSection("AlipayConfig");
            services.Configure<Gs.Application.Models.AlipayConfig>(AlipayConf);
            Gs.Application.Models.AlipayConfig AlipayConfig = AlipayConf.Get<Gs.Application.Models.AlipayConfig>();
            services.AddHttpClient(AlipayConfig.ClientName);
            services.AddScoped<IAlipay, Alipay>();
            #endregion

            #region 
            IConfigurationSection QCloudConf = Configuration.GetSection("QCloudConfig");
            services.Configure<Gs.Application.Models.QCloudConfig>(QCloudConf);
            Gs.Application.Models.QCloudConfig QCloudConfig = QCloudConf.Get<Gs.Application.Models.QCloudConfig>();
            services.AddHttpClient(QCloudConfig.ClientName);
            services.AddScoped<IQCloudPlugin, QCloudPlugin>();
            #endregion

            #region
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

            #region 
            IConfigurationSection WePayConfig = Configuration.GetSection("WePayConfig");
            services.Configure<Gs.Application.Models.WepayConfig>(WePayConfig);
            // Gs.Application.Models.WepayConfig WePayConf = WePayConfig.Get<Gs.Application.Models.WepayConfig>();
            // services.AddHttpClient(WePayConf.ClientName, client => { client.DefaultRequestHeaders.Add("KeepAlive", "true"); });
            //services.AddHttpClient(WePayConf.CertClient)
            //    .ConfigurePrimaryHttpMessageHandler(() =>
            //    {
            //        HttpClientHandler handler = new HttpClientHandler();
            //        String certPath = Path.Combine(AppContext.BaseDirectory, "Cert", WePayConf.CertPath);
            //        handler.ClientCertificates.Add(new X509Certificate2(certPath, WePayConf.CertPass));
            //        handler.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            //        handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; };
            //        return handler;
            //    });
            services.AddScoped<IWePayPlugin, WePayPlugin>();
            #endregion
            #region
            IConfigurationSection JobsDetail = Configuration.GetSection("Jobs");
            services.Configure<List<Core.JobDetail>>(JobsDetail);
            services.AddSingleton<Core.JobScheduler>();
            services.AddTransient<Jobs.DailyCloseMiner>();
            services.AddTransient<Jobs.UpdateTeamStar>();
            services.AddTransient<Jobs.DailyUpdateDividend>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IJobFactory, Core.JobFactoryService>();
            #endregion

            #region 

            #region

            services.AddScoped<IAmbmService, AmbmService>();
            services.AddScoped<ITradeService, TradeService>();
            services.AddScoped<IActiveService, ActiveService>();
            services.AddScoped<IHonorService, HonorService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ICottonService, CottonService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IAdditionService, AdditionService>();
            services.AddScoped<IIntegralService, IntegralService>();
            services.AddScoped<IConchService, ConchService>();
            #endregion

            services.AddScoped<IUserSerivce, UserSerivce>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IMiningService, MiningService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IAliPayAction, AliPayAction>();
            services.AddScoped<IAlipay, Alipay>();

            services.AddScoped<IPaymentAction, PaymentAction>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<ICommunityService, CommunityService>();
            #endregion

            services.AddScoped<IAdminService, AdminService>();
            services.AddSingleton<ISubscribeService, SubscribeService>();
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddMvcCustomer(Constants.WEBSITE_AUTHENTICATION_SCHEME, mvcOptions =>
            {
                mvcOptions.AuthorizationSchemes = new List<MvcAuthorizeOptions>
                 {
                    new MvcAuthorizeOptions(){
                         ReturnUrlParameter="from",
                         AccessDeniedPath="/Denied",
                         AuthenticationScheme=Constants.WEBSITE_AUTHENTICATION_SCHEME,
                         LoginPath="/",
                         LogoutPath="/Logout"
                    }
                 };
            });
            services.AddOptions();
            services.AddMemoryCache();
            services.RegisterService();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            #region
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseErrorHandlerMiddleware();
            app.UseCors(t =>
            {
                t.WithMethods("POST", "PUT", "GET");
                t.WithHeaders("X-Requested-With", "Content-Type", "User-Agent");
                t.WithOrigins("*");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Down}/{action=Index}/{id?}");
            });
            #endregion

            #region
            ISubscribeService syncTeams = app.ApplicationServices.GetRequiredService<ISubscribeService>();
            CSRedisClient.SubscribeObject subScribe = null;
            CSRedisClient redis = app.ApplicationServices.GetRequiredService<CSRedisClient>();
            subScribe = redis.Subscribe(
                ("MEMBER_REGISTER", async msg => await syncTeams.SubscribeMemberRegist(msg.Body)),
                ("MEMBER_CERTIFIED", async msg => await syncTeams.SubscribeMemberCertified(msg.Body)),
                ("EXCHANGE_MINER", async msg => await syncTeams.SubscribeTaskAction(msg.Body))
            );
            #endregion


            #region
            Core.JobScheduler jobInit = app.ApplicationServices.GetRequiredService<Core.JobScheduler>();
            async Task jobStart()
            {
                await jobInit.Start();
                //============================================================//
                await jobInit.AddTask<Jobs.UpdateTeamStar>();           // 
                await jobInit.AddTask<Jobs.DailyCloseMiner>();          // 
                await jobInit.AddTask<Jobs.DailyUpdateDividend>();      // 
                //============================================================//
            }
            lifetime.ApplicationStarted.Register(jobStart().Wait);
            lifetime.ApplicationStopped.Register(() =>
            {
                jobInit.Stop();
                if (null != subScribe) { subScribe.Unsubscribe(); }
            });
            #endregion

        }
    }
}
