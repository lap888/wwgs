using Gs.Domain.Models;
using System.Collections.Generic;

namespace Gs.Domain.Configs
{
    public class Constants
    {
        /// <summary>
        /// jpushkey
        /// </summary>
        public const string JpushKey = "0c163ba8dc28b889ef18ebea";

        /// <summary>
        /// jpushSecret
        /// </summary>
        public const string JpushSecret = "2ea9059043d6988a681c4f91";

        /// <summary>
        /// 默认头像图片相对服务器路径
        /// </summary>
        public const string DefaultHeadPicture = "/images/head.png";

        /// <summary>
        /// 腾讯云COS 访问地址
        /// </summary>
        public const string CosUrl = "https://file.yoyoba.cn/";

        /// <summary>
        /// sign token key
        /// </summary>
        public static string SignKey = "gengshengwanwu";

        /// <summary>
        /// 公司明
        /// </summary>
        public static string Company = "新视讯";

        /// <summary>
        /// 网站授权协议
        /// </summary>
        public const string WEBSITE_AUTHENTICATION_SCHEME = "Web";

        /// <summary>
        /// 上次登录路径
        /// </summary>
        public const string LAST_LOGIN_PATH = "LAST_LOGIN_PATH";
        public const string ShowAllDataCookie = "ShowAllData";
        /// <summary>
        /// 验证码图片
        /// </summary>
        public const string WEBSITE_VERIFICATION_CODE = "ValidateCode";

        /// <summary>
        /// 
        /// </summary>
        public const string UPLOAD_TEMP_PATH = "Upload_Temp";
        public const string BANNER_PATH = "Banner";
        public const string TRADE_PATH = "Trade";
        public const string APPEALS_PATH = "Appeals";
        public const string USER_PIC = "UserPic";
        public const string SCENIC_PATH = "scenic";
        /// <summary>
        /// access_token
        /// </summary>
        public const string WxAccessToken = "access_token";

        public static List<MiningBase> BaseSetting = new List<MiningBase>
        {
            // new MiningBase{
            //     BaseId = 1,
            //     BaseName = "粉色量化宝",
            //     IsRenew = false,
            //     StoreShow = true,
            //     IsExchange = false,
            //     UnitPrice = 0,
            //     DayOut = 0.50M,
            //     TotalOut = 15,
            //     TaskDuration = 30,
            //     TaskExpires = 30,
            //     HonorValue = 0,
            //     Active = 1,
            //     ActiveDuration = 30,
            //     MaxHave = 1,
            //     Colors = "#FF69B4",
            //     Remark = "",
            // },
            new MiningBase{
                BaseId = 2,
                BaseName = "一级量化宝",
                IsRenew = false,
                StoreShow = true,
                IsExchange = true,
                UnitPrice = 10,
                DayOut = 0.4333M,
                TotalOut = 13,
                TaskDuration = 30,
                TaskExpires = 30,
                HonorValue = 50,
                Active = 1,
                ActiveDuration = 30,
                MaxHave = 1,
                Colors = "#FF0000",
                Remark = "",
            },
            new MiningBase{
                BaseId = 3,
                BaseName = "二级量化宝",
                IsRenew = false,
                StoreShow = true,
                IsExchange = true,
                UnitPrice = 100,
                DayOut = 4.3666M,
                TotalOut = 131,
                TaskDuration = 30,
                TaskExpires = 30,
                HonorValue = 45,
                Active = 10,
                ActiveDuration = 30,
                MaxHave = 1,
                Colors = "#FFA500",
                Remark = "",
            },
            new MiningBase{
                BaseId = 4,
                BaseName = "三级量化宝",
                IsRenew = false,
                StoreShow = true,
                IsExchange = true,
                UnitPrice = 500,
                DayOut = 22M,
                TotalOut = 660,
                TaskDuration = 30,
                TaskExpires = 30,
                HonorValue = 40,
                Active = 50,
                ActiveDuration = 30,
                MaxHave = 1,
                Colors = "#1E90FF",
                Remark = "",
            },
            new MiningBase{
                BaseId = 5,
                BaseName = "四级量化宝",
                IsRenew = false,
                StoreShow = true,
                IsExchange = true,
                UnitPrice = 1000,
                DayOut = 44.3333M,
                TotalOut = 1330,
                TaskDuration = 30,
                TaskExpires = 30,
                HonorValue = 35,
                Active = 100,
                ActiveDuration = 30,
                MaxHave = 1,
                Colors = "#008000",
                Remark = "",
            },
            new MiningBase{
                BaseId = 6,
                BaseName = "五级量化宝",
                IsRenew = false,
                StoreShow = true,
                IsExchange = true,
                UnitPrice = 5000,
                DayOut = 223.3333M,
                TotalOut = 6700,
                TaskDuration = 30,
                TaskExpires = 30,
                HonorValue = 30,
                Active = 500,
                ActiveDuration = 30,
                MaxHave = 1,
                Colors = "#00FFFF",
                Remark = "",
            },
            new MiningBase{
                BaseId = 7,
                BaseName = "六级量化宝",
                IsRenew = false,
                StoreShow = true,
                IsExchange = true,
                UnitPrice = 10000,
                DayOut = 450M,
                TotalOut = 13500,
                TaskDuration = 30,
                TaskExpires = 30,
                HonorValue = 25,
                Active = 1000,
                ActiveDuration = 30,
                MaxHave = 1,
                Colors = "#800080",
                Remark = "",
            },

        };
    }
}