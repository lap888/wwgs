﻿using Dapper;
using Gs.Core;
using Gs.Domain.Context;
using Gs.Domain.Models;
using Gs.Domain.Repository;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Jobs
{
    public class DailyUpdateDividend : IJob
    {
        private readonly IServiceProvider ServiceProvider;

        public DailyUpdateDividend(IServiceProvider service)
        {
            this.ServiceProvider = service;
        }
        public async Task Execute1(IJobExecutionContext context)
        {
            using (var service = this.ServiceProvider.CreateScope())
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    WwgsContext SqlContext = service.ServiceProvider.GetRequiredService<WwgsContext>();
                    ITeamService Team = service.ServiceProvider.GetRequiredService<ITeamService>();
                    IConchService ConchSub = service.ServiceProvider.GetRequiredService<IConchService>();
                    IMiningService MiningSub = service.ServiceProvider.GetRequiredService<IMiningService>();

                    String TableName = $"member_dividend_{DateTime.Now.ToString("yyyyMMdd")}";

                    StringBuilder Sql = new StringBuilder();
                    //==============================创建基础数据表==============================//
                    Sql.AppendLine("DROP TABLE IF EXISTS `member_ext_tmp`;");
                    Sql.AppendLine("CREATE TABLE `member_ext_tmp` (");
                    Sql.AppendLine("  `Id` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `UserID` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `ParentId` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `teamStart` int(11) NOT NULL,");
                    Sql.AppendLine("  `teamCount` int(11) NOT NULL,");
                    Sql.AppendLine("  `authCount` int(11) NOT NULL,");
                    Sql.AppendLine("  `teamCandyH` int(11) NOT NULL,");
                    Sql.AppendLine("  `bigCandyH` int(11) NOT NULL,");
                    Sql.AppendLine("  `littleCandyH` int(11) NOT NULL,");
                    Sql.AppendLine("  PRIMARY KEY (`UserID`),");
                    Sql.AppendLine("	KEY `FK_Id` (`Id`) USING BTREE,");
                    Sql.AppendLine("  KEY `FK_ParentId` (`ParentId`) USING BTREE,");
                    Sql.AppendLine("  KEY `FK_teamCandyH` (`teamCandyH`) USING BTREE");
                    Sql.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                    Sql.AppendLine("");
                    //==============================拷贝基础数据==============================//
                    Sql.AppendLine("TRUNCATE TABLE member_ext_tmp;");
                    Sql.AppendLine("INSERT INTO member_ext_tmp SELECT ");
                    Sql.AppendLine("A.Id,A.UserID,R.ParentId,0 AS teamStart,A.teamCount,A.authCount,A.teamCandyH,0 AS bigCandyH,0 AS littleCandyH FROM (");
                    Sql.AppendLine("SELECT (@i:=@i+1) AS Id,ext.userId AS UserID,ext.authCount,ext.teamCount,ext.teamCandyH FROM user_ext AS ext,(SELECT @i:=0) AS Ids ORDER BY ext.teamCandyH DESC) AS A");
                    Sql.AppendLine("INNER JOIN (SELECT UserId,ParentId FROM user_relation) AS R ON A.userId=R.UserId");
                    Sql.AppendLine("ORDER BY A.Id;");
                    Sql.AppendLine("");
                    //==============================创建分红基础表==============================//
                    Sql.AppendLine($"DROP TABLE IF EXISTS `{TableName}`;");
                    Sql.AppendLine($"CREATE TABLE `{TableName}` (");
                    Sql.AppendLine("  `UserId` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `teamStart` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `teamCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `bigCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `littleCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  PRIMARY KEY (`UserId`)");
                    Sql.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                    //==============================写入分红数据==============================//
                    Sql.AppendLine($"TRUNCATE TABLE `{TableName}`;");
                    Sql.AppendLine($"INSERT INTO `{TableName}` SELECT ");
                    Sql.AppendLine("Tmp.UserId,");
                    Sql.AppendLine("(CASE");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 20000 AND Tmp.littleCandyH >= 5000 THEN 4");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 8000 AND Tmp.littleCandyH >= 2000 THEN 3");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 2000 AND Tmp.littleCandyH >= 400 THEN 2");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 500 THEN 1");
                    Sql.AppendLine("  ELSE 0 ");
                    Sql.AppendLine("END)AS teamStart, ");
                    Sql.AppendLine("Tmp.teamCandyH, ");
                    Sql.AppendLine("Tmp.bigCandyH AS bigCandyH, ");
                    Sql.AppendLine("Tmp.littleCandyH AS littleCandyH ");
                    Sql.AppendLine("FROM (");
                    Sql.AppendLine("SELECT ");
                    Sql.AppendLine("A.UserID,A.authCount,A.teamCandyH,");
                    Sql.AppendLine("IFNULL(B.BigCandyH,0) AS bigCandyH,");
                    Sql.AppendLine("IF(A.teamCandyH-IFNULL(B.BigCandyH,0)<0,0,A.teamCandyH-IFNULL(B.BigCandyH,0)) AS littleCandyH");
                    Sql.AppendLine("FROM member_ext_tmp AS A LEFT JOIN (");
                    Sql.AppendLine("SELECT A.ParentId AS UserID,SUM(A.teamCandyH) AS BigCandyH FROM (SELECT * FROM member_ext_tmp WHERE teamCandyH>0) AS A");
                    Sql.AppendLine("  WHERE (");
                    Sql.AppendLine("    SELECT COUNT(1) FROM (SELECT * FROM member_ext_tmp WHERE teamCandyH>0) AS B");
                    Sql.AppendLine("    WHERE B.ParentId=A.ParentId AND B.Id<=A.Id");
                    Sql.AppendLine("   )<2 ");
                    Sql.AppendLine(" GROUP BY A.ParentId) AS B ON A.UserID=B.UserID");
                    Sql.AppendLine(")AS Tmp;");
                    Sql.AppendLine("");
                    //==============================删除基础数据表==============================//
                    Sql.AppendLine("DROP TABLE IF EXISTS `member_ext_tmp`;");
                    //==============================更新数据==============================//
                    Sql.AppendLine("");
                    //==============================执行SQL语句==============================//
                    var SqlString = Sql.ToString();
                    SqlContext.Dapper.Execute(SqlString, null, null, 1200);

                    //==============================修改正式表内SQL语句==============================//
                    SqlContext.Dapper.Execute($"UPDATE user_ext AS E INNER JOIN `{TableName}` AS T ON E.userId=T.UserID SET E.bigCandyH = T.bigCandyH, E.littleCandyH = T.littleCandyH, E.teamStart = T.teamStart, E.updateTime = NOW();", null, null, 1200);

                    //==============================取出分红基本数据==============================//
                    decimal TradeAmount = SqlContext.Dapper.QueryFirstOrDefault<decimal>($"select IFNULL(sum(`Incurred`),0) sumTotal from `user_account_cotton_coin_record` where `AccountId`=2 and TO_DAYS(`ModifyTime`)=TO_DAYS(now())");
                    List<TeamInfosDto> StartUsers = SqlContext.Dapper.Query<TeamInfosDto>($"SELECT ue.*, u.`Mobile` FROM `user_ext` AS ue LEFT JOIN `user` AS u ON ue.userId = u.id WHERE ue.`teamStart` >= 1 ORDER BY ue.`teamStart` DESC;").ToList();

                    Dictionary<int, decimal[]> StarAmounts = new Dictionary<int, decimal[]>
                    {
                        {1,new decimal[]{ 0.05M, 1 } },     //环保达人 10%
                        {2,new decimal[]{ 0.17M, 1 } },     //环保新星 17%
                        {3,new decimal[]{ 0.15M, 1 } },     //环保使者 15%
                        {4,new decimal[]{ 0.08M, 1 } },     //环保天使 8%
                    };

                    #region 写入每日分红记录
                    String TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
                    await SqlContext.Dapper.ExecuteAsync($"INSERT INTO `everyday_dividend` VALUES ('{TodayDate}', {TradeAmount}, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)");
                    #endregion

                    //==============================构建分红SQL语句==============================//
                    for (int i = 1; i <= StarAmounts.Count; i++)
                    {
                        // List<TeamInfosDto> TotalUser = new List<TeamInfosDto>();
                        List<TeamInfosDto> CurrentUser = StartUsers.Where(o => o.TeamStart == i).ToList();    //取出当前星级用户
                        int PeopleCount = CurrentUser.Count + (int)StarAmounts[i][1];      //今日享受分红的人数
                        decimal ThisStarAmount = TradeAmount * StarAmounts[i][0] / PeopleCount; //计算当前星级每人分红金额

                        await SqlContext.Dapper.ExecuteAsync($"UPDATE `everyday_dividend` SET `Star{i}`={ThisStarAmount}, `People{i}`={PeopleCount} WHERE (`DividendDate`='{TodayDate}')");

                        List<long> AllUserIds = CurrentUser.Select(o => o.UserId).ToList();
                        if (AllUserIds.Count == 0) { continue; }

                        //==============================执行分红操作==============================//
                        String TalentName = String.Empty;
                        switch (StarAmounts[i][0])
                        {
                            case 1:
                                TalentName = "一星达人";
                                break;
                            case 2:
                                TalentName = "二星达人";
                                break;
                            case 3:
                                TalentName = "三星达人";
                                break;
                            case 4:
                                TalentName = "四星达人";
                                break;
                            default:
                                break;
                        }

                        foreach (var item in AllUserIds)
                        {
                            await ConchSub.ChangeAmount(item, ThisStarAmount, Domain.Enums.ConchModifyType.TALENT_BONUS, false, TalentName);
                        }

                        for (int ii = i; ii >= 1; ii--)
                        {
                            if (AllUserIds.Count == 0) { continue; }

                            var iii = -1;
                            switch (ii)
                            {
                                case 4: iii = 6; break;
                                case 3: iii = 5; break;
                                case 2: iii = 3; break;
                                case 1: iii = 2; break;
                            }

                            if (iii <= 0) { return; }
                            //==============================取需要给予量化宝的用户ID==============================//
                            List<long> GiveUserIds = SqlContext.Dapper.Query<long>($"SELECT UserId FROM user_mining WHERE BaseId IN ({ii}, {iii}) AND Source = 3 AND UserId IN ({ string.Join(",", AllUserIds)});").ToList();
                            List<long> NoGiveTaskUserIds = AllUserIds.Where(o => !GiveUserIds.Contains(o)).ToList();

                            foreach (var item in NoGiveTaskUserIds)
                            {
                                var res = await MiningSub.Exchange(item, iii, Domain.Enums.MiningSource.SYSTEM_AWARD);
                                if (res.Code != 200)
                                {
                                    SystemLog.Jobs("发送量化宝发生错误" + res.Message);
                                }
                            }
                        }
                    }
                    stopwatch.Stop();
                    SystemLog.Jobs($"每日分红 执行完成,执行时间:{stopwatch.Elapsed.TotalSeconds}秒");
                }
                catch (Exception ex)
                {
                    SystemLog.Jobs("每日分红 发生错误", ex);
                }
            }
        }


        /// <summary>
        /// 分红 批量执行
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
                    ITeamService Team = service.ServiceProvider.GetRequiredService<ITeamService>();
                    IConchService ConchSub = service.ServiceProvider.GetRequiredService<IConchService>();
                    IMiningService Settings = service.ServiceProvider.GetRequiredService<IMiningService>();

                    String TableName = $"wwgs_member_dividend_{DateTime.Now.ToString("yyyyMMdd")}";

                    StringBuilder Sql = new StringBuilder();
                    //==============================创建基础数据表==============================//
                    Sql.AppendLine("DROP TABLE IF EXISTS `wwgs_member_ext_tmp`;");
                    Sql.AppendLine("CREATE TABLE `wwgs_member_ext_tmp` (");
                    Sql.AppendLine("  `Id` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `UserID` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `ParentId` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `teamStart` int(11) NOT NULL,");
                    Sql.AppendLine("  `teamCount` int(11) NOT NULL,");
                    Sql.AppendLine("  `authCount` int(11) NOT NULL,");
                    Sql.AppendLine("  `teamCandyH` int(11) NOT NULL,");
                    Sql.AppendLine("  `bigCandyH` int(11) NOT NULL,");
                    Sql.AppendLine("  `littleCandyH` int(11) NOT NULL,");
                    Sql.AppendLine("  PRIMARY KEY (`UserID`),");
                    Sql.AppendLine("	KEY `FK_Id` (`Id`) USING BTREE,");
                    Sql.AppendLine("  KEY `FK_ParentId` (`ParentId`) USING BTREE,");
                    Sql.AppendLine("  KEY `FK_teamCandyH` (`teamCandyH`) USING BTREE");
                    Sql.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                    Sql.AppendLine("");
                    //==============================拷贝基础数据==============================//
                    Sql.AppendLine("TRUNCATE TABLE wwgs_member_ext_tmp;");
                    Sql.AppendLine("INSERT INTO wwgs_member_ext_tmp SELECT ");
                    Sql.AppendLine("A.Id,A.UserID,R.ParentId,0 AS teamStart,A.teamCount,A.authCount,A.teamCandyH,0 AS bigCandyH,0 AS littleCandyH FROM (");
                    Sql.AppendLine("SELECT (@i:=@i+1) AS Id,ext.userId AS UserID,ext.authCount,ext.teamCount,ext.teamCandyH FROM user_ext AS ext,(SELECT @i:=0) AS Ids ORDER BY ext.teamCandyH DESC) AS A");
                    Sql.AppendLine("INNER JOIN (SELECT UserId,ParentId FROM user_relation) AS R ON A.userId=R.UserId");
                    Sql.AppendLine("ORDER BY A.Id;");
                    Sql.AppendLine("");
                    //==============================创建分红基础表==============================//
                    Sql.AppendLine($"DROP TABLE IF EXISTS `{TableName}`;");
                    Sql.AppendLine($"CREATE TABLE `{TableName}` (");
                    Sql.AppendLine("  `UserId` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `teamStart` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `teamCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `bigCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `littleCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  PRIMARY KEY (`UserId`)");
                    Sql.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                    //==============================写入分红数据==============================//
                    Sql.AppendLine($"TRUNCATE TABLE `{TableName}`;");
                    Sql.AppendLine($"INSERT INTO `{TableName}` SELECT ");
                    Sql.AppendLine("Tmp.UserId,");
                    Sql.AppendLine("(CASE");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 20000 AND Tmp.littleCandyH >= 5000 THEN 4");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 8000 AND Tmp.littleCandyH >= 2000 THEN 3");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 2000 AND Tmp.littleCandyH >= 400 THEN 2");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 500 THEN 1");
                    Sql.AppendLine("  ELSE 0 ");
                    Sql.AppendLine("END)AS teamStart,");
                    Sql.AppendLine("Tmp.teamCandyH,");
                    Sql.AppendLine("IF(Tmp.bigCandyH<Tmp.littleCandyH,Tmp.littleCandyH,Tmp.bigCandyH) AS bigCandyH,");
                    Sql.AppendLine("IF(Tmp.bigCandyH>Tmp.littleCandyH,Tmp.littleCandyH,Tmp.bigCandyH) AS littleCandyH");
                    Sql.AppendLine("FROM (");
                    Sql.AppendLine("SELECT ");
                    Sql.AppendLine("A.UserID,A.authCount,A.teamCandyH,");
                    Sql.AppendLine("IFNULL(B.BigCandyH,0) AS bigCandyH,");
                    Sql.AppendLine("IF(A.teamCandyH-IFNULL(B.BigCandyH,0)<0,0,A.teamCandyH-IFNULL(B.BigCandyH,0)) AS littleCandyH");
                    Sql.AppendLine("FROM wwgs_member_ext_tmp AS A LEFT JOIN (");
                    Sql.AppendLine("SELECT A.ParentId AS UserID,SUM(A.teamCandyH) AS BigCandyH FROM (SELECT * FROM wwgs_member_ext_tmp WHERE teamCandyH>0) AS A");
                    Sql.AppendLine("  WHERE (");
                    Sql.AppendLine("    SELECT COUNT(1) FROM (SELECT * FROM wwgs_member_ext_tmp WHERE teamCandyH>0) AS B");
                    Sql.AppendLine("    WHERE B.ParentId=A.ParentId AND B.Id<=A.Id");
                    Sql.AppendLine("   )<2 ");
                    Sql.AppendLine(" GROUP BY A.ParentId) AS B ON A.UserID=B.UserID");
                    Sql.AppendLine(")AS Tmp;");
                    Sql.AppendLine("");
                    //==============================删除基础数据表==============================//
                    Sql.AppendLine("DROP TABLE IF EXISTS `wwgs_member_ext_tmp`;");
                    //==============================更新数据==============================//
                    Sql.AppendLine("");
                    //==============================执行SQL语句==============================//
                    var SqlString = Sql.ToString();
                    SqlContext.Dapper.Execute(SqlString, null, null, 1200);


                    //==============================修改正式表内SQL语句==============================//
                    SqlContext.Dapper.Execute($"UPDATE user_ext AS E INNER JOIN `{TableName}` AS T ON E.userId=T.UserID SET E.bigCandyH=T.bigCandyH,E.littleCandyH=T.littleCandyH,E.teamStart=T.teamStart,E.updateTime=NOW();", null, null, 1200);

                    //==============================取出分红基本数据==============================//
                    decimal TradeAmount = SqlContext.Dapper.QueryFirstOrDefault<decimal>($"select IFNULL(sum(`Incurred`),0) sumTotal from `user_account_cotton_coin_record` where `AccountId`=2 and TO_DAYS(now())-TO_DAYS(`ModifyTime`)=1");

                    List<TeamInfosDto> StartUsers = SqlContext.Dapper.Query<TeamInfosDto>($"SELECT ue.*, u.`AccountId` FROM `user_ext` AS ue LEFT JOIN `user_account_cotton_coin` AS u ON ue.userId = u.UserId WHERE ue.`teamStart` >= 1 ORDER BY ue.`teamStart` DESC;").ToList();

                    Dictionary<int, decimal[]> StarAmounts = new Dictionary<int, decimal[]>
                    {
                        {1,new decimal[]{ 0.10M, 1 } },     //1星10%----注入40个分享排行榜
                        {2,new decimal[]{ 0.20M, 1 } },     //2星20%----注入9个分享排行榜----注入7个守榜
                        {3,new decimal[]{ 0.15M, 1 } },     //3星15%----注入1个分享排行榜----注入2个守榜
                        {4,new decimal[]{ 0.05M, 1 } },     //4星10%----注入1个守榜
                    };

                    #region 写入每日分红记录
                    String TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
                    await SqlContext.Dapper.ExecuteAsync($"INSERT INTO `everyday_dividend` VALUES ('{TodayDate}', {TradeAmount}, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)");
                    #endregion

                    //==============================构建分红SQL语句==============================//
                    for (int i = 1; i <= StarAmounts.Count; i++)
                    {
                        List<TeamInfosDto> TotalUser = new List<TeamInfosDto>();
                        List<TeamInfosDto> CurrentUser = StartUsers.Where(o => o.TeamStart == i).ToList();    //取出当前星级用户
                        int PeopleCount = CurrentUser.Count + (int)StarAmounts[i][1];      //今日享受分红的人数
                        decimal ThisStarAmount = TradeAmount * StarAmounts[i][0] / PeopleCount; //计算当前星级每人分红金额
                        ThisStarAmount = Math.Round(ThisStarAmount, 4);
                        await SqlContext.Dapper.ExecuteAsync($"UPDATE `everyday_dividend` SET `Star{i}`={ThisStarAmount}, `People{i}`={PeopleCount} WHERE (`DividendDate`='{TodayDate}')");

                        List<long> AllAccountIds = CurrentUser.Select(o => o.AccountId).ToList();

                        if (AllAccountIds.Count == 0) { continue; }

                        //==============================构建分红SQL语句==============================//
                        StringBuilder amountSql = new StringBuilder("INSERT INTO `user_account_cotton_coin_record` ( `AccountId`, `PreChange`, `Incurred`, `PostChange`, `ModifyType`, `ModifyDesc`, `ModifyTime` ) values ");
                        string PostChangeSql = string.Empty;
                        foreach (var item in AllAccountIds)
                        {
                            PostChangeSql = $"select IFNULL((SELECT `PostChange` FROM `user_account_cotton_coin_record` WHERE `AccountId`={item} ORDER BY `RecordId` DESC LIMIT 1),0)";
                            decimal PostChange = SqlContext.Dapper.QueryFirstOrDefault<decimal>(PostChangeSql);
                            decimal _postChange = Math.Round(PostChange, 4);
                            decimal tp = ThisStarAmount + _postChange;
                            Core.SystemLog.Debug($"PostChange{PostChange}||_postChange{_postChange}||tp={tp}");
                            amountSql.Append($"\r\n({item},{_postChange},{ThisStarAmount},{tp},8,'{i}星达人','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'),");
                        }

                        String AmountSqlString = amountSql.ToString().TrimEnd(',');
                        Core.SystemLog.Debug($"执行分红sql={AmountSqlString}");

                        //==============================执行分红操作==============================//

                        string AmountGiveSqlString = $"UPDATE `user_account_cotton_coin` SET `Balance`=`Balance`+{ThisStarAmount},`ModifyTime`=NOW() WHERE `AccountId` in ({string.Join(",", AllAccountIds)})";
                        using (IDbConnection db = SqlContext.DapperConnection)
                        {
                            db.Open();
                            IDbTransaction Tran = db.BeginTransaction();
                            try
                            {
                                if (ThisStarAmount > 0)
                                {
                                    db.Execute(AmountGiveSqlString, null, Tran);
                                    db.Execute(AmountSqlString, null, Tran);
                                    Tran.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                Core.SystemLog.Jobs($"每日分红发生错误,分红语句:{AmountGiveSqlString}\r\n\r\n记录语句:{AmountSqlString}", ex);
                                Tran.Rollback();
                            }
                            finally { if (db.State == ConnectionState.Open) { db.Close(); } }
                        }
                        var CurrentUserId = CurrentUser.Select(t => t.UserId).ToList();
                        //赠送达人矿机
                        for (int ii = i; ii >= 1; ii--)
                        {
                            if (CurrentUser.Count == 0) { continue; }

                            var iii = -1;
                            switch (ii)
                            {
                                case 4: iii = 6; break;
                                case 3: iii = 5; break;
                                case 2: iii = 3; break;
                                case 1: iii = 2; break;
                            }

                            if (iii <= 0) { return; }
                            //==============================取需要给予任务的用户ID==============================//
                            List<long> GiveUserIds = SqlContext.Dapper.Query<long>($"SELECT UserId FROM user_mining WHERE BaseId IN ({iii}) AND Source = 3 AND UserId IN ({ string.Join(",", CurrentUserId)});").ToList();
                            List<long> NoGiveTaskUserIds = CurrentUserId.Where(o => !GiveUserIds.Contains(o)).ToList();
                            //==============================构建任务赠送SQL语句==============================//
                            var effectiveBiginTime = DateTime.Now.Date.AddDays(1).ToLocalTime().ToString("yyyy-MM-dd");
                            var effectiveEndTime = DateTime.Now.Date.AddDays(31).ToLocalTime().ToString("yyyy-MM-dd");
                            StringBuilder StarTaskSql = new StringBuilder("insert into user_mining (userId, BaseId, BeginDate, Duration, source,ExpiryDate) values ");


                            foreach (var item in NoGiveTaskUserIds)
                            {
                                StarTaskSql.Append($"\r\n({item}, {iii},'{effectiveBiginTime}', 30,3,'{effectiveEndTime}'),");
                            }
                            String TaskSqlString = StarTaskSql.ToString().TrimEnd(',');
                            if (NoGiveTaskUserIds.Count > 0)
                            {
                                using (IDbConnection db = SqlContext.DapperConnection)
                                {
                                    db.Open();
                                    IDbTransaction Tran = db.BeginTransaction();
                                    try
                                    {
                                        db.Execute(TaskSqlString, null, Tran);
                                        Tran.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.SystemLog.Jobs($"每日赠送任务发生错误:赠送语句:{TaskSqlString}", ex);
                                        Tran.Rollback();
                                    }
                                    finally { if (db.State == ConnectionState.Open) { db.Close(); } }
                                }
                            }
                        }
                    }
                    stopwatch.Stop();
                    Core.SystemLog.Jobs($"每日分红 执行完成,执行时间:{stopwatch.Elapsed.TotalSeconds}秒");
                }
                catch (Exception ex)
                {
                    Core.SystemLog.Jobs("每日分红 发生错误", ex);
                }
            }
        }

        /// <summary>
        /// 团队用户信息表
        /// </summary>
        public class TeamInfosDto
        {
            /// <summary>
            /// 
            /// </summary>
            public long Id { get; set; }
            /// <summary>
            /// 用户ID
            /// </summary>
            public long UserId { get; set; }

            
            /// <summary>
            /// 账户ID
            /// </summary>
            /// <value></value>

            public long AccountId { get; set; }
            /// <summary>
            /// 直推等级
            /// </summary>
            /// <value></value>
            public int AuthCount { get; set; } = 0;
            /// <summary>
            /// 大区果核
            /// </summary>
            /// <value></value>
            public int BigCandyH { get; set; } = 0;
            /// <summary>
            /// 小区果核
            /// </summary>
            /// <value></value>
            public int LittleCandyH { get; set; } = 0;
            /// <summary>
            /// 团队果核
            /// </summary>
            /// <value></value>
            public int TeamCandyH { get; set; } = 0;
            /// <summary>
            /// 团队人数
            /// </summary>
            /// <value></value>
            public int TeamCount { get; set; } = 0;
            /// <summary>
            /// 团队等级
            /// </summary>
            /// <value></value>
            public int TeamStart { get; set; } = 0;
            /// <summary>
            /// 用户手机号
            /// </summary>
            public string Mobile { get; set; }
        }
    }
}
