using CSRedis;
using Gs.Core;
using System;
using System.IO;
using Gs.Application;
using Newtonsoft.Json;
using Gs.Domain.Context;
using Gs.Domain.Configs;
using System.Reflection;
using Gs.Core.Extensions;
using Gs.Domain.Repository;
using Gs.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Gs.Application.Middleware;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace Gs.WebApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 锟剿凤拷锟斤拷锟斤拷锟斤拷锟斤拷时锟斤拷锟矫★拷使锟矫此凤拷锟斤拷锟缴斤拷锟斤拷锟斤拷锟斤拷锟接碉拷锟斤拷锟斤拷锟叫★拷
        /// </summary>
        /// <param name="services"></param>
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

            #region 系统锟斤拷锟斤拷注锟斤拷
            services.Configure<Application.Models.AppSetting>(Configuration.GetSection("AppSetting"));
            services.Configure<Application.Models.TicketConfig>(Configuration.GetSection("TicketConfig"));
            #endregion

            #region 注锟斤拷微锟斤拷锟斤拷锟斤拷
            IConfigurationSection WeChatConf = Configuration.GetSection("WeChatConfig");
            services.Configure<Application.Models.WeChatConfig>(WeChatConf);
            Application.Models.WeChatConfig WeChatConfig = WeChatConf.Get<Application.Models.WeChatConfig>();
            services.AddHttpClient(WeChatConfig.ClientName);
            services.AddScoped<IWechatPlugin, WeChatPlugin>();
            #endregion

            #region 支锟斤拷锟斤拷锟斤拷锟斤拷
            IConfigurationSection AlipayConf = Configuration.GetSection("AlipayConfig");
            services.Configure<Application.Models.AlipayConfig>(AlipayConf);
            Application.Models.AlipayConfig AlipayConfig = AlipayConf.Get<Application.Models.AlipayConfig>();
            services.AddHttpClient(AlipayConfig.ClientName);
            services.AddScoped<IAlipay, Alipay>();
            #endregion

            #region 实锟斤拷锟斤拷证锟斤拷锟斤拷
            IConfigurationSection RealVerifyConf = Configuration.GetSection("RealVerifyConfig");
            services.Configure<Application.Models.RealVerifyConfig>(RealVerifyConf);
            Application.Models.RealVerifyConfig RealVerifyConfig = RealVerifyConf.Get<Application.Models.RealVerifyConfig>();
            services.AddHttpClient(RealVerifyConfig.ClientName);
            services.AddScoped<IRealVerify, RealVerify>();
            #endregion

            #region 锟斤拷讯锟斤拷锟斤拷锟斤拷
            IConfigurationSection QCloudConf = Configuration.GetSection("QCloudConfig");
            services.Configure<Application.Models.QCloudConfig>(QCloudConf);
            Application.Models.QCloudConfig QCloudConfig = QCloudConf.Get<Application.Models.QCloudConfig>();
            services.AddHttpClient(QCloudConfig.ClientName);
            services.AddScoped<IQCloudPlugin, QCloudPlugin>();
            #endregion

            #region 注锟斤拷锟斤拷锟捷匡拷
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

            #region 微锟斤拷支锟斤拷注锟斤拷
            IConfigurationSection WePayConfig = Configuration.GetSection("WePayConfig");
            services.Configure<Application.Models.WepayConfig>(WePayConfig);
            Application.Models.WepayConfig WePayConf = WePayConfig.Get<Application.Models.WepayConfig>();
            services.AddHttpClient(WePayConf.ClientName, client => { client.DefaultRequestHeaders.Add("KeepAlive", "true"); });
            // services.AddHttpClient(WePayConf.CertClient)
            //     .ConfigurePrimaryHttpMessageHandler(() =>
            //     {
            //         HttpClientHandler handler = new HttpClientHandler();
            //         String certPath = Path.Combine(AppContext.BaseDirectory, "Cert", WePayConf.CertPath);
            //         handler.ClientCertificates.Add(new X509Certificate2(certPath, WePayConf.CertPass));
            //         handler.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            //         handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; };
            //         return handler;
            //     });
            services.AddScoped<IWePayPlugin, WePayPlugin>();
            #endregion

            #region 
            #region 
            services.AddScoped<IActiveService, ActiveService>();
            services.AddScoped<IHonorService, HonorService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ICottonService, CottonService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IAdditionService, AdditionService>();
            services.AddScoped<IIntegralService, IntegralService>();
            services.AddScoped<IConchService, ConchService>();
            #endregion

            services.AddScoped<IAmbmService, AmbmService>();
            services.AddScoped<IUserSerivce, UserSerivce>();
            services.AddScoped<IMiningService, MiningService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<ICommunityService, CommunityService>();

            services.AddScoped<IAliPayAction, AliPayAction>();
            services.AddScoped<IPaymentAction, PaymentAction>();
            services.AddScoped<IAddressService, AddressService>();

            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<ICoinService, CoinService>();
            services.AddScoped<ITradeService, TradeService>();



            #endregion

            //
            services.AddResponseCompression();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(setupAction =>
            {
                setupAction.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                setupAction.SerializerSettings.DateFormatString = "yyyy/MM/d HH:mm:ss";
                setupAction.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                setupAction.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddMemoryCache();
#if DEBUG
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    name: "v1",
                    info: new OpenApiInfo
                    {
                        Title = "WWGS-API",
                        Description = "API DOC",
                        Version = "v1.0.0"
                    }
                );
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
            });
#endif
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); //session锟斤拷锟斤拷时锟斤拷
                options.Cookie.HttpOnly = true;//锟斤拷为httponly
            });
            services.RegisterService();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            #region 
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }
            else { app.UseHsts(); }
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>() { { ".apk", "application/vnd.android.package-archive" } })
            });
            app.UseErrorHandlerMiddleware();
            app.UseCors(t =>
            {
                t.WithMethods("POST", "PUT", "GET");
                t.WithHeaders("X-Requested-With", "Content-Type", "User-Agent", "token");
                t.WithOrigins("*");
            });
#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    url: "/swagger/v1/swagger.json",
                    name: "v1.0.0"
                );
            });
#endif
            app.UseResponseCompression();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            #endregion

        }
    }
}
