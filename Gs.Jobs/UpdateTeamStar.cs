using Dapper;
using Gs.Core;
using Gs.Domain.Context;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Gs.Jobs
{
    /// <summary>
    /// 更新星级
    /// </summary>
    public class UpdateTeamStar : IJob
    {
        private readonly IServiceProvider ServiceProvider;
        public UpdateTeamStar(IServiceProvider service)
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

                    String TableName = $"member_star_now";

                    StringBuilder Sql = new StringBuilder();
                    //============================== 创建基础数据表 ==============================//
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
                    //============================== 拷贝基础数据 ==============================//
                    Sql.AppendLine("TRUNCATE TABLE member_ext_tmp;");
                    Sql.AppendLine("INSERT INTO member_ext_tmp SELECT ");
                    Sql.AppendLine("A.Id,A.UserID,R.ParentId,0 AS teamStart,A.teamCount,A.authCount,A.teamCandyH,0 AS bigCandyH,0 AS littleCandyH FROM (");
                    Sql.AppendLine("SELECT (@i:=@i+1) AS Id,ext.userId AS UserID,ext.authCount,ext.teamCount,ext.teamCandyH FROM user_ext AS ext,(SELECT @i:=0) AS Ids ORDER BY ext.teamCandyH DESC) AS A");
                    Sql.AppendLine("INNER JOIN (SELECT UserId,ParentId FROM user_relation) AS R ON A.userId=R.UserId");
                    Sql.AppendLine("ORDER BY A.Id;");
                    Sql.AppendLine("");
                    //============================== 创建分红基础表 ==============================//
                    Sql.AppendLine($"DROP TABLE IF EXISTS `{TableName}`;");
                    Sql.AppendLine($"CREATE TABLE `{TableName}` (");
                    Sql.AppendLine("  `UserId` bigint(20) NOT NULL,");
                    Sql.AppendLine("  `teamStart` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `teamCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `bigCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  `littleCandyH` int(11) NOT NULL DEFAULT '0',");
                    Sql.AppendLine("  PRIMARY KEY (`UserId`)");
                    Sql.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                    //============================== 写入分红数据 ==============================//
                    Sql.AppendLine($"TRUNCATE TABLE `{TableName}`;");
                    Sql.AppendLine($"INSERT INTO `{TableName}` SELECT ");
                    Sql.AppendLine("Tmp.UserId,");
                    Sql.AppendLine("(CASE");
                    // Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.littleCandyH >= 200000 THEN 5");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 20000 AND Tmp.littleCandyH >= 5000 THEN 4");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 8000 AND Tmp.littleCandyH >= 2000 THEN 3");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 2000 AND Tmp.littleCandyH >= 400 THEN 2");
                    Sql.AppendLine("  WHEN Tmp.authCount >= 20 AND Tmp.teamCandyH >= 500 THEN 1");
                    Sql.AppendLine("  ELSE 0 ");
                    Sql.AppendLine("END)AS teamStart,");
                    Sql.AppendLine("Tmp.teamCandyH,");
                    Sql.AppendLine("Tmp.bigCandyH AS bigCandyH,");
                    Sql.AppendLine("Tmp.littleCandyH AS littleCandyH");
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
                    //============================== 删除基础数据表 ==============================//
                    Sql.AppendLine("DROP TABLE IF EXISTS `member_ext_tmp`;");

                    //============================== 执行SQL语句 ==============================//
                    var SqlString = Sql.ToString();
                    await SqlContext.Dapper.ExecuteAsync(SqlString, null, null, 1200);

                    //============================== 修改正式表内SQL语句 ==============================//
                    SqlContext.Dapper.Execute($"UPDATE user_ext AS E INNER JOIN `{TableName}` AS T ON E.userId = T.UserID SET E.bigCandyH = T.bigCandyH, E.littleCandyH = T.littleCandyH, E.teamStart = T.teamStart, E.updateTime = NOW();", null, null, 1200);

                    stopwatch.Stop();
                    SystemLog.Jobs($"定时更新大区小区及星级 执行完成,执行时间:{stopwatch.Elapsed.TotalSeconds}秒");
                }
                catch (Exception ex)
                {
                    SystemLog.Jobs("定时更新大区小区及星级 发生错误", ex);
                }
            }
        }
    }
}
