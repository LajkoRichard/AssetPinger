using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AssetPinger
{
    public partial class AssetDBContext : DbContext
    {
        public AssetDBContext()
        {
        }

        public AssetDBContext(DbContextOptions<AssetDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Assets> Assets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlite(@"Data Source=I:\COMMON\Monitoring\SDIHU Asset Explorer\AssetDB.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Assets>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Ip).HasColumnName("IP");

                entity.Property(e => e.LastActiveTime)
                    .IsRequired()
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.Mac).HasColumnName("MAC");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
