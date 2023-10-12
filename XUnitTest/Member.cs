using Gs.Domain.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTest
{
    public class Member
    {
        private readonly IServiceProvider ServiceProvider;
        public readonly IConfiguration Configuration;

        public Member()
        {
            CommServiceProvider commService = new CommServiceProvider();
            Configuration = commService.GetConfiguration();
            ServiceProvider = commService.GetServiceProvider();
        }

        [Fact]
        public async Task Register()
        {
            var UserSub = ServiceProvider.GetRequiredService<IUserSerivce>();
            var RedisCache = ServiceProvider.GetRequiredService<CSRedis.CSRedisClient>();

            Int64 Mobile = 19866668889;

            for (int i = 0; i < 100; i++)
            {
                //await Task.Delay(1000 * 5);
                var res= await UserSub.SignUp(new Gs.Domain.Models.Dto.SignUpDto()
                {
                    Mobile = Mobile.ToString(),
                    InvitationCode = (13866668890).ToString(),
                    NickName = Mobile.ToString(),
                    Password = "123456",
                    VerifyCode = "9527"
                });
                Mobile++;
                Console.WriteLine(res.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Exchange()
        {
            var UserSub = ServiceProvider.GetRequiredService<IUserSerivce>();
            var MiningSub = ServiceProvider.GetRequiredService<IMiningService>();
            var RedisCache = ServiceProvider.GetRequiredService<CSRedis.CSRedisClient>();

            var UserId = 10000;

            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(1000 * 3);
                RedisCache.Publish("MEMBER_CERTIFIED", JsonConvert.SerializeObject(new { MemberId = UserId, Nick = UserId.ToString() }));
                UserId++;
            }


        }

        [Fact]
        public async Task Publish()
        {
            var Team = ServiceProvider.GetRequiredService<ITeamService>();
            await Team.SetRelation(10006, 9999);
            var RedisCache = ServiceProvider.GetRequiredService<CSRedis.CSRedisClient>();

            RedisCache.Publish("MEMBER_REGISTER", JsonConvert.SerializeObject(new { MemberId = 10006, ParentId = 9999 }));
        }
    }
}
