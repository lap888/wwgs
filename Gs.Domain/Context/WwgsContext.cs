using System;
using System.Data;
using Gs.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Gs.Domain.Context
{
    public partial class WwgsContext : DbContext
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private readonly String ConnectionString;
        /// <summary>
        /// 数据库
        /// </summary>
        private IDbConnection DbConnection;

        /// <summary>
        /// 数据库上下文
        /// </summary>
        /// <param name="options"></param>
        public WwgsContext(DbContextOptions<WwgsContext> options) : base(options)
        {
            try
            {
                this.ConnectionString = options.GetExtension<Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal.MySqlOptionsExtension>().ConnectionString;
            }
            catch (Exception)
            {
                this.ConnectionString = String.Empty;
            }
        }


        /// <summary>
        /// 启用Dapper
        /// </summary>
        public IDbConnection Dapper
        {
            get
            {
                if (null == DbConnection) { DbConnection = this.DapperConnection; }
                return DbConnection;
            }
        }
        /// <summary>
        /// 获取一个新的Dapper数据库连接
        /// </summary>
        public IDbConnection DapperConnection { get => new MySql.Data.MySqlClient.MySqlConnection(this.ConnectionString); }

        public virtual DbSet<AuthenticationInfos> AuthenticationInfos { get; set; }
        public virtual DbSet<CityMaster> CityMaster { get; set; }
        public virtual DbSet<CommunityBackOrder> CommunityBackOrder { get; set; }
        public virtual DbSet<CommunityCenter> CommunityCenter { get; set; }
        public virtual DbSet<CommunityTurnover> CommunityTurnover { get; set; }
        public virtual DbSet<FaceInitRecord> FaceInitRecord { get; set; }
        public virtual DbSet<LoginHistory> LoginHistory { get; set; }
        public virtual DbSet<UserMining> UserMining { get; set; }
        public virtual DbSet<NoticeInfos> NoticeInfos { get; set; }
        public virtual DbSet<OrderGames> OrderGames { get; set; }
        public virtual DbSet<PhoneAttribution> PhoneAttribution { get; set; }
        public virtual DbSet<Pictures> Pictures { get; set; }
        public virtual DbSet<StoreItem> StoreItem { get; set; }
        public virtual DbSet<StoreCategory> StoreCategory { get; set; }
        public virtual DbSet<StoreOrder> StoreOrder { get; set; }
        public virtual DbSet<SysBanner> SysBanner { get; set; }
        public virtual DbSet<SysClientVersions> SysClientVersions { get; set; }
        public virtual DbSet<UserEntity> UserEntity { get; set; }
        public virtual DbSet<UserFeedback> UserFeedback { get; set; }
        public virtual DbSet<UserAddress> UserAddress { get; set; }
        public virtual DbSet<UserAccountTicket> UserAccountTicket { get; set; }
        public virtual DbSet<UserAccountTicketRecord> UserAccountTicketRecord { get; set; }
        public virtual DbSet<UserAccountWallet> UserAccountWallet { get; set; }
        public virtual DbSet<UserAccountWalletRecord> UserAccountWalletRecord { get; set; }
        public virtual DbSet<UserExt> UserExt { get; set; }
        public virtual DbSet<UserLocations> UserLocations { get; set; }
        public virtual DbSet<UserVcodes> UserVcodes { get; set; }
        public virtual DbSet<UserWithdrawHistory> UserWithdrawHistory { get; set; }
        public virtual DbSet<EverydayDividend> EverydayDividend { get; set; }
        public virtual DbSet<UserRelation> UserRelation { get; set; }
        public virtual DbSet<PayRecord> PayRecord { get; set; }
        public virtual DbSet<RechargeOrder> RechargeOrder { get; set; }
        public virtual DbSet<SystemCopywriting> SystemCopywriting { get; set; }
        public virtual DbSet<SystemUser> SystemUser { get; set; }
        public virtual DbSet<SystemActions> SystemActions { get; set; }
        public virtual DbSet<SystemRolePermission> SystemRolePermission { get; set; }
        public virtual DbSet<SystemRoles> SystemRoles { get; set; }
        public virtual DbSet<SystemTask> SystemTask { get; set; }
        public virtual DbSet<UserDigMinerRecord> UserDigMinerRecord { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<AuthenticationInfos>(entity =>
            {
                entity.ToTable("authentication_infos");

                entity.HasIndex(e => e.CertifyId)
                    .HasName("UK_certifyId")
                    .IsUnique();

                entity.HasIndex(e => e.IdNum)
                    .HasName("UNIQUE_IDNUM")
                    .IsUnique();

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AuthType)
                    .HasColumnName("authType")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CertifyId)
                    .HasColumnName("certifyId")
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FailReason)
                    .HasColumnName("failReason")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.IdNum)
                    .HasColumnName("idNum")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Pic)
                    .HasColumnName("pic")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Pic1)
                    .HasColumnName("pic1")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Pic2)
                    .HasColumnName("pic2")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.TrueName)
                    .HasColumnName("trueName")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<CityMaster>(entity =>
            {
                entity.HasKey(e => e.CityId);

                entity.ToTable("city_master");

                entity.HasIndex(e => e.CityCode)
                    .HasName("FK_CityCode")
                    .IsUnique();

                entity.Property(e => e.CityId).HasColumnType("int(11)");

                entity.Property(e => e.CityCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.AreaCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.AreaName)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.WeChat)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<CommunityBackOrder>(entity =>
            {
                entity.ToTable("community_back_order");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Condition).HasColumnType("int(3)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.ItemBrand)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.ItemCunt).HasColumnType("int(6)");

                entity.Property(e => e.ItemGrade).HasColumnType("int(3)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.RepoType).HasColumnType("int(3)");

                entity.Property(e => e.ShipMethod).HasColumnType("int(3)");

                entity.Property(e => e.State).HasColumnType("int(3)");

                entity.Property(e => e.StoreId).HasColumnType("bigint(20)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");

                entity.Property(e => e.AssessIntegral).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<CommunityCenter>(entity =>
            {
                entity.ToTable("community_center");

                entity.Property(e => e.Id).HasColumnType("bigint(11)");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.AreaCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CityCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Company)
                    .IsRequired()
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ContactTel)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Contacts)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Describe)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Doorhead)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.IsDel)
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Lat).HasColumnType("decimal(18,8)");

                entity.Property(e => e.Lng).HasColumnType("decimal(18,8)");

                entity.Property(e => e.Qq)
                    .IsRequired()
                    .HasColumnName("QQ")
                    .HasColumnType("varchar(16)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.WeChat)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Website)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<CommunityTurnover>(entity =>
            {
                entity.ToTable("community_turnover");

                entity.HasIndex(e => new { e.Date, e.StoreId })
                    .HasName("UNIQUE_STORE_DATE")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.StoreId).HasColumnType("bigint(20)");

                entity.Property(e => e.Turnover).HasColumnType("decimal(18,4)");
            });

            modelBuilder.Entity<FaceInitRecord>(entity =>
            {
                entity.ToTable("face_init_record");

                entity.HasIndex(e => new { e.CertifyId, e.IdcardNum })
                    .HasName("UNIQUE_CID")
                    .IsUnique();

                entity.HasIndex(e => new { e.IdcardNum, e.TrueName })
                    .HasName("NORMAL_IDCARD");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Alipay)
                    .HasColumnType("varchar(128)");

                entity.Property(e => e.CertifyId)
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.CertifyUrl)
                    .HasColumnType("varchar(1024)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.IdcardNum)
                    .HasColumnName("IDCardNum")
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.IsUsed)
                    .HasColumnType("int(2)");

                entity.Property(e => e.TrueName)
                    .HasColumnType("varchar(32)");
            });

            modelBuilder.Entity<LoginHistory>(entity =>
            {
                entity.ToTable("login_history");

                entity.HasIndex(e => e.Mobile)
                    .HasName("FK_mobile");

                entity.HasIndex(e => new { e.UniqueId, e.Mobile })
                    .HasName("NORMAL_UNIQUEID_MOBILE");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AppVersion)
                    .HasColumnName("appVersion")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Ctime)
                    .HasColumnName("ctime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DeviceName)
                    .HasColumnName("deviceName")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.SystemName)
                    .HasColumnName("systemName")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.SystemVersion)
                    .HasColumnName("systemVersion")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UnLockCount)
                    .HasColumnName("unLockCount")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UniqueId)
                    .HasColumnName("uniqueId")
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Utime)
                    .HasColumnName("utime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<UserMining>(entity =>
            {
                entity.ToTable("user_mining");

                entity.HasIndex(e => e.BeginDate)
                    .HasName("BeginDate");

                entity.HasIndex(e => e.ExpiryDate)
                    .HasName("ExpiryDate");

                entity.HasIndex(e => e.Source)
                    .HasName("Source");

                entity.HasIndex(e => e.UserId)
                    .HasName("UserId");

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.BaseId)
                    .HasColumnName("BaseId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BeginDate)
                    .HasColumnName("BeginDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Source)
                    .HasColumnName("Duration")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ExpiryDate)
                    .HasColumnName("ExpiryDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("int(3)");

                entity.Property(e => e.State)
                    .HasColumnName("State")
                    .HasColumnType("int(3)");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("CreateTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpTime)
                    .HasColumnName("UpTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Remark)
                    .HasColumnName("Remark")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<NoticeInfos>(entity =>
            {
                entity.ToTable("notice_infos");

                entity.HasIndex(e => e.Type)
                    .HasName("FK_type");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_userId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CeratedAt)
                    .HasColumnName("ceratedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasColumnType("text");

                entity.Property(e => e.IsRead).HasColumnName("isRead");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.IsDel).HasColumnType("bit(1)");

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<OrderGames>(entity =>
            {
                entity.ToTable("order_games");

                entity.HasIndex(e => e.CreatedAt)
                    .HasName("FK_createdAt");

                entity.HasIndex(e => e.OrderId)
                    .HasName("UNIQUE_ORDERID")
                    .IsUnique();

                entity.HasIndex(e => e.UpdatedAt)
                    .HasName("FK_updatedAt");

                entity.HasIndex(e => new { e.OrderId, e.UserId })
                    .HasName("FK_orderId_userId");

                entity.HasIndex(e => new { e.GameAppid, e.UserId, e.Status })
                    .HasName("FK_gameAppid_userId_status");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Candy)
                    .HasColumnName("candy")
                    .HasColumnType("decimal(18,8)");

                entity.Property(e => e.CandyAmount)
                    .HasColumnName("candyAmount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.GameAppid)
                    .HasColumnName("gameAppid")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.OrderAmount)
                    .HasColumnName("orderAmount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.OrderId)
                    .HasColumnName("orderId")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.RealAmount)
                    .HasColumnName("realAmount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Uuid)
                    .HasColumnName("uuid")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<PhoneAttribution>(entity =>
            {
                entity.ToTable("phone_attribution");

                entity.HasIndex(e => e.Phone)
                    .HasName("UNIQUE_PHONE")
                    .IsUnique();

                entity.HasIndex(e => e.Prefix)
                    .HasName("NORMAL_PREFIX");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AreaCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ChinaCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasColumnType("varchar(128)");

                entity.Property(e => e.Isp)
                    .IsRequired()
                    .HasColumnName("ISP")
                    .HasColumnType("varchar(16)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasColumnType("varchar(8)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.Prefix)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.Province)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<Pictures>(entity =>
            {
                entity.ToTable("pictures");

                entity.HasIndex(e => new { e.ImageableType, e.ImageableId })
                    .HasName("index_pictures_on_imageable_type_and_imageable_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Height)
                    .HasColumnName("height")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'100'");

                entity.Property(e => e.ImageableId)
                    .HasColumnName("imageableId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ImageableType)
                    .HasColumnName("imageableType")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Size)
                    .HasColumnName("size")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Width)
                    .HasColumnName("width")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'100'");
            });


            modelBuilder.Entity<StoreItem>(entity =>
            {
                entity.ToTable("store_item");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CateId)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Deleted).HasColumnType("bit(1)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnType("text(0)");

                entity.Property(e => e.Images)
                    .IsRequired()
                    .HasColumnType("varchar(4000)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Keywords)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.MetaTitle)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OldPrice)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.PointsPrice)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.Published).HasColumnType("bit(1)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.ServicePrice)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.Sku)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Stock)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<StoreCategory>(entity =>
            {
                entity.ToTable("store_item_category");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Icon)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.Sort).HasColumnType("int(11)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<StoreOrder>(entity =>
            {
                entity.ToTable("store_order");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Contacts)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ContactTel)
                    .IsRequired()
                    .HasColumnType("varchar(16)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.ExpressNum)
                    .IsRequired()
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ItemId).HasColumnType("bigint(20)");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ItemPic)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PayIntegral).HasColumnType("decimal(10,2)");

                entity.Property(e => e.PayNo)
                    .IsRequired()
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.ServicePrice).HasColumnType("decimal(10,2)");

                entity.Property(e => e.ShippingAddress)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.State).HasColumnType("int(11)");

                entity.Property(e => e.StoreId).HasColumnType("bigint(20)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<SysBanner>(entity =>
            {
                entity.ToTable("sys_banner");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(11)");

                entity.Property(e => e.CityCode)
                    .HasColumnName("cityCode")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ImageUrl)
                    .HasColumnName("imageUrl")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Params)
                    .HasColumnName("params")
                    .HasColumnType("text");

                entity.Property(e => e.Queue)
                    .HasColumnName("queue")
                    .HasColumnType("int(3)");

                entity.Property(e => e.Source)
                    .HasColumnName("source")
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.IsDel)
                    .HasColumnName("isDel")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("int(3)");
            });

            modelBuilder.Entity<SysClientVersions>(entity =>
            {
                entity.ToTable("sys_client_versions");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.CurrentVersion)
                    .HasColumnName("currentVersion")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.DeviceSystem)
                    .HasColumnName("deviceSystem")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.DownloadUrl)
                    .HasColumnName("downloadUrl")
                    .HasColumnType("text");

                entity.Property(e => e.IsHotReload).HasColumnName("isHotReload");

                entity.Property(e => e.IsNecessary)
                    .IsRequired()
                    .HasColumnName("isNecessary")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.IsSilent).HasColumnName("isSilent");

                entity.Property(e => e.Production).HasColumnName("production");

                entity.Property(e => e.UpdateContent)
                    .HasColumnName("updateContent")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.AuditState)
                    .HasName("audit_state");

                entity.HasIndex(e => e.Ctime)
                    .HasName("ctime");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.InviterMobile)
                    .HasName("FK_inviter_mobile");

                entity.HasIndex(e => e.Mobile)
                    .HasName("UK_mobile")
                    .IsUnique();

                entity.HasIndex(e => e.Rcode)
                    .HasName("rcode_2");

                entity.HasIndex(e => new { e.Alipay, e.AuditState })
                    .HasName("NORMAL_ALIPAY_AUDITSTATE");

                entity.HasIndex(e => new { e.InviterMobile, e.Mobile, e.Status })
                    .HasName("inviter_mobile");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Alipay)
                    .HasColumnName("alipay")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.AlipayUid)
                    .HasColumnName("alipayUid")
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.AuditState)
                    .HasColumnName("auditState")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AvatarUrl)
                    .IsRequired()
                    .HasColumnName("avatarUrl")
                    .HasColumnType("varchar(225)")
                    .HasDefaultValueSql("'images/avatar/default/1.png'");

                entity.Property(e => e.Ctime)
                    .HasColumnName("ctime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Golds)
                    .HasColumnName("golds")
                    .HasColumnType("decimal(18,8)")
                    .HasDefaultValueSql("'0.00000000'");

                entity.Property(e => e.InviterMobile)
                    .HasColumnName("inviterMobile")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasColumnType("varchar(80)");

                entity.Property(e => e.PasswordSalt)
                    .HasColumnName("passwordSalt")
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.Rcode)
                    .HasColumnName("rcode")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TradePwd)
                    .HasColumnName("tradePwd")
                    .HasColumnType("varchar(80)");

                entity.Property(e => e.Utime)
                    .HasColumnName("utime")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Uuid)
                    .HasColumnName("uuid")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<UserFeedback>(entity =>
            {
                entity.ToTable("user_feedback");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Images)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Remark).HasColumnType("varchar(255)");

                entity.Property(e => e.State)
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<UserAccountTicket>(entity =>
            {
                entity.HasKey(e => e.AccountId)
                    .HasName("PRIMARY");

                entity.ToTable("user_account_ticket");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId")
                    .IsUnique();

                entity.Property(e => e.AccountId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Balance)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.Expenses)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.Frozen)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.Revenue)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.State)
                    .HasColumnType("int(2)");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserAccountTicketRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId)
                    .HasName("PRIMARY");

                entity.ToTable("user_account_ticket_record");

                entity.HasIndex(e => e.AccountId)
                    .HasName("FK_AccountId");

                entity.HasIndex(e => e.ModifyTime)
                    .HasName("FK_ModifyTime");

                entity.HasIndex(e => e.ModifyType)
                    .HasName("FK_ModifyType");

                entity.HasIndex(e => new { e.AccountId, e.ModifyType })
                    .HasName("NORMAL_AID_MT");

                entity.HasIndex(e => new { e.ModifyType, e.AccountId })
                    .HasName("NORMAL_MT_AID");

                entity.Property(e => e.RecordId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.AccountId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Incurred)
                    .HasColumnType("decimal(12,5)");

                entity.Property(e => e.ModifyDesc)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.ModifyType)
                    .HasColumnType("int(11)");

                entity.Property(e => e.PostChange)
                    .HasColumnType("decimal(12,5)");

                entity.Property(e => e.PreChange)
                    .HasColumnType("decimal(12,5)");
            });

            modelBuilder.Entity<UserAccountWallet>(entity =>
            {
                entity.HasKey(e => e.AccountId)
                    .HasName("PRIMARY");

                entity.ToTable("user_account_wallet");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId");

                entity.Property(e => e.AccountId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Balance)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.Expenses)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.Frozen)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.Revenue)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserAccountWalletRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId)
                    .HasName("PRIMARY");

                entity.ToTable("user_account_wallet_record");

                entity.HasIndex(e => e.AccountId)
                    .HasName("FK_AccountId");

                entity.HasIndex(e => e.ModifyTime)
                    .HasName("FK_ModifyTime");

                entity.HasIndex(e => e.ModifyType)
                    .HasName("FK_ModifyType");

                entity.HasIndex(e => new { e.AccountId, e.ModifyType })
                    .HasName("NORMAL_AID_MT");

                entity.HasIndex(e => new { e.ModifyType, e.AccountId })
                    .HasName("NORMAL_MT_AID");

                entity.Property(e => e.RecordId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.AccountId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Incurred)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.ModifyDesc)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ModifyTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.ModifyType)
                    .HasColumnType("int(11)");

                entity.Property(e => e.PostChange)
                    .HasColumnType("decimal(18,5)");

                entity.Property(e => e.PreChange)
                    .HasColumnType("decimal(18,5)");
            });

            modelBuilder.Entity<UserExt>(entity =>
            {
                entity.ToTable("user_ext");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20) unsigned");

                entity.Property(e => e.AuthCount)
                    .HasColumnName("authCount")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BigCandyH)
                    .HasColumnName("bigCandyH")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("createTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.LittleCandyH)
                    .HasColumnName("littleCandyH")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamCandyH)
                    .HasColumnName("teamCandyH")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamCount)
                    .HasColumnName("teamCount")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamStart)
                    .HasColumnName("teamStart")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("updateTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserLocations>(entity =>
            {
                entity.ToTable("user_locations");

                entity.HasIndex(e => e.CityCode)
                    .HasName("FK_cityCode");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Area)
                    .HasColumnName("area")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.AreaCode)
                    .HasColumnName("areaCode")
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CityCode)
                    .HasColumnName("cityCode")
                    .HasColumnType("varchar(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("decimal(10,6)");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("decimal(10,6)");

                entity.Property(e => e.Province)
                    .HasColumnName("province")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ProvinceCode)
                    .HasColumnName("provinceCode")
                    .HasColumnType("varchar(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserVcodes>(entity =>
            {
                entity.ToTable("user_vcodes");

                entity.HasIndex(e => e.Mobile)
                    .HasName("NORMAL_MOBILE");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("datetime");

                entity.Property(e => e.Mobile)
                    .HasColumnName("mobile")
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.MsgId)
                    .HasColumnName("msgId")
                    .HasColumnType("varchar(64)");
            });

            modelBuilder.Entity<UserWithdrawHistory>(entity =>
            {
                entity.ToTable("user_withdraw_history");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(10,4)");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FailReason)
                    .HasColumnName("failReason")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.OrderCode)
                    .HasColumnName("orderCode")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.WithdrawTo)
                    .HasColumnName("withdrawTo")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.WithdrawType)
                    .HasColumnName("withdrawType")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<EverydayDividend>(entity =>
            {
                entity.HasKey(e => e.DividendDate)
                    .HasName("PRIMARY");

                entity.ToTable("everyday_dividend");

                entity.Property(e => e.DividendDate).HasColumnType("date");

                entity.Property(e => e.CandyFee).HasColumnType("decimal(18,6)");

                entity.Property(e => e.People1).HasColumnType("int(11)");

                entity.Property(e => e.People2).HasColumnType("int(11)");

                entity.Property(e => e.People3).HasColumnType("int(11)");

                entity.Property(e => e.People4).HasColumnType("int(11)");

                entity.Property(e => e.People5).HasColumnType("int(11)");

                entity.Property(e => e.Star1).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star2).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star3).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star4).HasColumnType("decimal(18,6)");

                entity.Property(e => e.Star5).HasColumnType("decimal(18,6)");
            });

            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.ToTable("user_address");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Area)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.IsDefault).HasColumnType("int(11)");

                entity.Property(e => e.IsDel).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasColumnType("varchar(11)");

                entity.Property(e => e.PostCode)
                    .IsRequired()
                    .HasColumnType("varchar(8)");

                entity.Property(e => e.Province)
                    .IsRequired()
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<UserRelation>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("user_relation");

                entity.HasIndex(e => e.ParentId)
                    .HasName("FK_ParentId");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.ParentId)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.RelationLevel)
                    .HasColumnType("int(11)");

                entity.Property(e => e.Topology)
                    .HasColumnType("text");
            });

            modelBuilder.Entity<PayRecord>(entity =>
            {
                entity.HasKey(e => e.PayId)
                    .HasName("PRIMARY");

                entity.ToTable("pay_record");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_UserId");

                entity.Property(e => e.PayId).HasColumnType("bigint(20)");

                entity.Property(e => e.ActionType)
                    .HasColumnType("int(8)");

                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");

                entity.Property(e => e.Channel).HasColumnType("int(8)");

                entity.Property(e => e.ChannelUid)
                    .IsRequired()
                    .HasColumnName("ChannelUID")
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Currency).HasColumnType("int(8)");

                entity.Property(e => e.Custom)
                    .IsRequired()
                    .HasColumnType("varchar(128)");

                entity.Property(e => e.Fee).HasColumnType("decimal(10,2)");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.PayStatus)
                    .HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<RechargeOrder>(entity =>
            {
                entity.ToTable("recharge_order");

                entity.HasIndex(e => e.ChannelNo)
                    .HasName("ChannelNo");

                entity.HasIndex(e => e.OrderNo)
                    .HasName("OrderNo");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Account)
                    .IsRequired()
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.BuyNum)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.ChannelNo)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.FaceValue)
                    .IsRequired()
                    .HasColumnType("varchar(64)");

                entity.Property(e => e.OrderNo)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OrderType)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.PayCandy)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.PayPeel)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PurchasePrice)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Remark)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.State)
                    .HasColumnType("int(11)");

                entity.Property(e => e.UpdateTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<SystemUser>(entity =>
            {
                entity.ToTable("system_user");

                entity.HasIndex(e => e.RoleId)
                    .HasName("rId");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(36)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.AccountStatus)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.AccountType)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.FullName)
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Gender)
                    .HasColumnType("int(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.IdCard)
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastLoginIp)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastLoginTime).HasColumnType("datetime");

                entity.Property(e => e.LoginName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasColumnType("varchar(20)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'123456'");

                entity.Property(e => e.RoleId)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.SourceType)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AdminUser)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("rId");
            });

            modelBuilder.Entity<SystemActions>(entity =>
            {
                entity.HasKey(e => e.ActionId)
                    .HasName("PRIMARY");

                entity.ToTable("system_actions");

                entity.Property(e => e.ActionId)
                    .HasColumnType("varchar(36)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ActionDescription)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Orders).HasColumnType("int(11)");

                entity.Property(e => e.ParentAction).HasColumnType("varchar(255)");
                entity.Property(e => e.Icon).HasColumnType("varchar(100)");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<SystemRolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ActionId })
                    .HasName("PRIMARY");

                entity.ToTable("system_role_permission");

                entity.Property(e => e.RoleId)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.ActionId).HasColumnType("varchar(36)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<SystemRoles>(entity =>
            {
                entity.ToTable("system_roles");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<SystemCopywriting>(entity =>
            {
                entity.ToTable("system_copywriting");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Key)
                    .HasColumnName("key")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnName("text")
                    .HasColumnType("text");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasColumnType("varchar(512)");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasColumnType("varchar(32)");
            });

            modelBuilder.Entity<SystemTask>(entity =>
            {
                entity.ToTable("system_task");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Aims).HasColumnType("int(11)");

                entity.Property(e => e.Devote)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Reward).HasColumnType("decimal(18,2)");

                entity.Property(e => e.Sort).HasColumnType("int(11)");

                entity.Property(e => e.Status).HasColumnType("int(11)");

                entity.Property(e => e.TaskDesc)
                    .IsRequired()
                    .HasColumnType("varchar(1024)");

                entity.Property(e => e.TaskTitle)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.TaskType)
                    .HasColumnType("int(11)");

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasColumnType("varchar(8)");
            });

            modelBuilder.Entity<UserDigMinerRecord>(entity =>
            {
                entity.ToTable("user_digminer_record");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("date");

                entity.Property(e => e.EndTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.Remark)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Schedule)
                    .HasColumnType("decimal(11,2)");

                entity.Property(e => e.Source)
                    .HasColumnType("int(3)");

                entity.Property(e => e.StartTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
