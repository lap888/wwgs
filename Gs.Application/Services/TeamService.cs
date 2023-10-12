using Dapper;
using System;
using Gs.Core;
using System.Linq;
using System.Text;
using Gs.Domain.Entity;
using Gs.Domain.Models;
using Gs.Domain.Context;
using Gs.Domain.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gs.Domain.Configs;
using Gs.Domain.Enums;
using Microsoft.Extensions.Options;
using Gs.Domain.Models.Dto;

namespace Gs.Application.Services
{
    /// <summary>
    /// 团队
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly WwgsContext context;
        private readonly Models.AppSetting AppSetting;
        public TeamService(WwgsContext mySql, IOptionsMonitor<Models.AppSetting> monitor)
        {
            this.context = mySql;
            this.AppSetting = monitor.CurrentValue;
        }



        /// <summary>
        /// 团队信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> TeamInfos(TeamInfosReqDto model)
        {
            MyResult<object> result = new MyResult<object>();

            if (model.UserId < 1) { return result.SetStatus(ErrorCode.InvalidToken, "sign error"); }

            TeamInfo myTeamInfo = await context.Dapper
                .QueryFirstOrDefaultAsync<TeamInfo>("SELECT ext.*,CONCAT( @CosUrl, u.`avatarUrl` ) AS `avatarUrl` FROM user_ext AS ext, `user` AS u WHERE u.id = ext.userId AND userId = @UserId;",
                new { Constants.CosUrl, UserId = model.UserId });
            if (myTeamInfo == null) { myTeamInfo = new TeamInfo(); }
            //总量
            var teamLevel = 0;
            teamLevel = myTeamInfo.TeamStart;
            if (myTeamInfo.TeamStart > 4)
            {
                teamLevel = 4;
            }
            //算进度
            // var teamSetting = AppSetting.TeamLevels.FirstOrDefault(t => t.TeamStart == teamLevel);
            // myTeamInfo.NeedAuthCount = teamSetting.AuthCount;
            // myTeamInfo.NeedTeamCandyH = teamSetting.TeamCount;
            // myTeamInfo.NeedLittleCandyH = teamSetting.LianMenTeamCount;
            //core
            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT u.id, u.`auditState`, u.`mobile`, CONCAT( @CosUrl, u.`avatarUrl` ) AS `avatarUrl`, u.`name`, u.`ctime`, ");
            QuerySql.Append("ue.`authCount`, ue.`bigCandyH`, ue.`teamCandyH`, ue.`teamCount`, ue.`teamStart`");
            QuerySql.Append("FROM `user` AS u LEFT JOIN user_ext ue ON u.id = ue.userId ");
            QuerySql.Append("WHERE u.`inviterMobile` = @Mobile ");

            if (model.Type == 0)
            {
                QuerySql.Append("ORDER BY ue.teamCandyH ");
                QuerySql.Append(model.Order);
            }
            else if (model.Type == 1)
            {
                QuerySql.Append("ORDER BY ue.teamCount ");
                QuerySql.Append(model.Order);
            }
            else if (model.Type == 2)
            {
                QuerySql.Append("ORDER BY u.ctime ");
                QuerySql.Append(model.Order);
            }
            else
            {
                QuerySql.Append("ORDER BY u.id ");
                QuerySql.Append(model.Order);
            }
            QuerySql.Append(" LIMIT @PageIndex,@PageSize;");

            result.RecordCount = await context.Dapper.QueryFirstOrDefaultAsync<Int32>("SELECT COUNT(id) FROM `user` WHERE `inviterMobile` = @Mobile;", new { Mobile = model.Mobile });
            result.PageCount = (result.RecordCount + model.PageSize - 1) / model.PageSize;
            IEnumerable<TeamInfosDto> teamInfoList = await context.Dapper.QueryAsync<TeamInfosDto>(QuerySql.ToString(), new { Constants.CosUrl, Mobile = model.Mobile, PageIndex = (model.PageIndex - 1) * model.PageSize, model.PageSize });

            result.Data = new { MyTeamInfo = myTeamInfo, TeamInfoList = teamInfoList };
            return result;
        }

        /// <summary>
        /// 更新团队人数
        /// </summary>
        /// <param name="Uid">用户ID</param>
        /// <param name="Number">变更人员数量(默认+1)</param>
        /// <returns></returns>
        public async Task<bool> UpdateTeamPersonnel(long Uid, int Number = 1)
        {
            if (Number == 0) { return true; }
            var TeamUsers = await this.GetRelation(Uid);
            if (String.IsNullOrWhiteSpace(TeamUsers.TopologyString)) { return true; }
            String Sql = $"UPDATE `user_ext` SET `teamCount`=`teamCount`+{Number},`updateTime`='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE (`teamCount`+{Number}>=0) AND `userId` IN ({TeamUsers.TopologyString})";
            return (await this.context.Dapper.ExecuteAsync(Sql)) > 0;
        }

        /// <summary>
        /// 更新团队活跃
        /// </summary>
        /// <param name="Uid">用户ID</param>
        /// <param name="Quantity">更新数量(默认+1)</param>
        /// <returns></returns>
        public async Task<bool> UpdateTeamKernel(long Uid, decimal Quantity = 1)
        {
            if (Quantity == 0) { return true; }
            var TeamUsers = await this.GetRelation(Uid);
            if (String.IsNullOrWhiteSpace(TeamUsers.TopologyString)) { return true; }
            String Sql = $"UPDATE `user_ext` SET `teamCandyH`=`teamCandyH`+{Quantity},`updateTime`='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE (`teamCandyH`+{Quantity}>=0) AND `userId` IN ({TeamUsers.TopologyString})";
            return (await this.context.Dapper.ExecuteAsync(Sql)) > 0;
        }

        /// <summary>
        /// 更新团队的直推人数
        /// </summary>
        /// <param name="Uid">用户ID(不传则更新所有用户)</param>
        /// <param name="Status">计入直推人数的用户状态(默认:已认证)</param>
        /// <returns></returns>
        public async Task<bool> UpdateTeamDirectPersonnel(long? Uid, int? Status = 2)
        {
            StringBuilder Sql = new StringBuilder();
            Sql.AppendLine("UPDATE `user_ext` AS `E` ");
            Sql.AppendLine("INNER JOIN (");
            Sql.AppendLine("SELECT `R`.`ParentId` AS `Uid`,COUNT(`R`.`ParentId`) AS `Total` ");
            Sql.AppendLine("FROM `user` AS `U` ");
            Sql.AppendLine("LEFT JOIN `user_relation` AS `R` ");
            Sql.AppendLine("ON `U`.`id`=`R`.`UserId` ");
            Sql.AppendLine("WHERE 1=1 ");

            //==============================插入条件==============================//
            if (null != Status) { Sql.AppendLine($"AND `U`.`auditState`={Status} "); }
            if (null != Uid) { Sql.AppendLine($" AND `R`.`ParentId`={Uid} "); }
            //==============================结束插入==============================//

            Sql.AppendLine("GROUP BY `R`.`ParentId`");
            Sql.AppendLine(") AS `T` ");
            Sql.AppendLine("ON `E`.`userId`=`T`.`Uid` ");
            Sql.AppendLine("SET ");
            Sql.AppendLine("`E`.`authCount`=`T`.`Total`,");
            Sql.AppendLine($"`E`.`updateTime`='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");

            return (await this.context.Dapper.ExecuteAsync(Sql.ToString())) > 0;
        }

        /// <summary>
        /// 获取用户拓扑关系
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        public async Task<MemberRelation> GetRelation(long Uid)
        {
            UserRelation relation = await this.context.UserRelation.FirstOrDefaultAsync(o => o.UserId == Uid);
            if (null == relation)
            {
                //==============尝试修复推荐关系==============//
                long? ParentId = await context.Dapper.QueryFirstOrDefaultAsync<long?>($"SELECT `UserId` AS `MemberId` FROM `user_relation` WHERE `UserId`=(SELECT `id` FROM `user` WHERE `mobile`=(SELECT `inviterMobile` FROM `user` WHERE `id`={Uid}))");
                if (ParentId == null) { ParentId = 0; }
                return await SetRelation(Uid, ParentId.Value);
            }
            return new MemberRelation
            {
                MemberId = relation.UserId,
                ParentId = relation.ParentId,
                RelationLevel = relation.RelationLevel,
                CreateTime = relation.CreateTime,
                TopologyString = relation.Topology,
                Topology = String.IsNullOrWhiteSpace(relation.Topology) ? new List<long>() : relation.Topology.Split(",").ToList().ConvertAll(x => x.ToLong())
            };
        }

        /// <summary>
        /// 设置用户拓扑关系
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="PUid"></param>
        /// <returns></returns>
        public async Task<MemberRelation> SetRelation(long Uid, long PUid)
        {
            UserRelation relation = await this.context.UserRelation.FirstOrDefaultAsync(o => o.UserId == Uid);
            if (null != relation)
            {
                return new MemberRelation
                {
                    MemberId = relation.UserId,
                    ParentId = relation.ParentId,
                    RelationLevel = relation.RelationLevel,
                    CreateTime = relation.CreateTime,
                    TopologyString = relation.Topology,
                    Topology = String.IsNullOrWhiteSpace(relation.Topology) ? new List<long>() : relation.Topology.Split(",").ToList().ConvertAll(x => x.ToLong())
                };
            }

            MemberRelation parentRelation = await this.GetRelation(PUid);

            parentRelation.Topology.Add(PUid);
            parentRelation.Topology.Remove(0);
            try
            {
                relation = this.context.UserRelation.Add(new UserRelation
                {
                    UserId = Uid,
                    ParentId = PUid,
                    RelationLevel = parentRelation.Topology.Count + 1,
                    CreateTime = DateTime.Now,
                    Topology = string.Join(",", parentRelation.Topology)
                }).Entity;

                if (this.context.SaveChanges() < 1) { return null; }
            }
            catch (Exception ex) { SystemLog.Debug("设置用户拓扑关系", ex); }

            return new MemberRelation
            {
                MemberId = relation.UserId,
                ParentId = relation.ParentId,
                RelationLevel = relation.RelationLevel,
                CreateTime = relation.CreateTime,
                TopologyString = relation.Topology,
                Topology = String.IsNullOrWhiteSpace(relation.Topology) ? new List<long>() : relation.Topology.Split(",").ToList().ConvertAll(x => x.ToLong())
            };
        }

        public Task<MyResult<object>> InfoForExchange(TeamInfosReqDto model)
        {
            throw new NotImplementedException();
        }

        public async Task<MyResult<object>> UpdateLevel(int userId)
        {
            MyResult result = new MyResult();
            try
            {
                var balance = context.Dapper.QueryFirstOrDefault<decimal>($"select `Balance` from `user_account_cotton` where userId={userId}");
                var level = CaculatorGolds(balance);

                String Sql = $"update `user` set `level`='{level}' where id = {userId};";
                await context.Dapper.ExecuteAsync(Sql);
                // SystemLog.Debug($"level={level}||Sql={Sql}||res={res}");
            }
            catch (System.Exception ex)
            {
                SystemLog.Debug($"ex={ex}");
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="golds"></param>
        /// <returns></returns>
        private string CaculatorGolds(decimal golds)
        {
            string UserLevel = String.Empty;

            foreach (var item in AppSetting.Levels.OrderBy(o => o.Claim))
            {
                if (golds >= item.Claim) { UserLevel = item.Level; }
            }
            return UserLevel;
        }

    }
}
