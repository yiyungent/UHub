using System;
using Microsoft.EntityFrameworkCore;
using UHubApi.Db.Models;

namespace UHubApi.Db
{
    public class UHubApiDbContext : DbContext
    {

        #region Ctor
        public UHubApiDbContext(DbContextOptions<UHubApiDbContext> options)
            : base(options)
        {
            // 让Entity Framework启动不再效验 __MigrationHistory 表
            // 发现每次效验/查询，都要去创建 __MigrationHistory 表，而 此表 的 ContextKey字段varchar(300) 超过限制导致
            // 解决：Specified key was too long; max key length is 767 bytes
            //Database.SetInitializer<RemDbContext>(null);

            //this.Configuration.AutoDetectChangesEnabled = true;//对多对多，一对多进行curd操作时需要为true

            ////this.Configuration.LazyLoadingEnabled = false;

            //// 记录 EF 生成的 SQL
            //Database.Log = (str) =>
            //{
            //    System.Diagnostics.Debug.WriteLine(str);
            //};
        }
        #endregion


        #region Tables

        public virtual DbSet<AppUser> AppUser { get; set; }

        #endregion

    }
}
