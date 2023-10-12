using Gs.WebApi;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace yoyoApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// ���
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                    WebHost.CreateDefaultBuilder(args)
                    .UseKestrel(options =>
                    {
                        options.Limits.MaxRequestBodySize = null;
                    })
                    // .UseUrls("http://192.168.5.7:5001")
                    .UseStartup<Startup>();
    }
}
