using _3CXCallogScrapper.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3CXCallogScrapper
{
    public class CallLogDbContext : DbContext
    {
        public CallLogDbContext(DbContextOptions<CallLogDbContext> options) : base(options)
        {
        }

        public DbSet<CallLogEntry> CallLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CallLogEntry>(entity =>
            {
                entity.HasKey(e => e.SegmentId);
                entity.Property(e => e.SegmentId).ValueGeneratedNever();
                entity.Property(e => e.StartTime).HasColumnType("timestamp with time zone");
                entity.Property(e => e.RingingDuration).HasMaxLength(50);
                entity.Property(e => e.TalkingDuration).HasMaxLength(50);
                entity.Property(e => e.RecordingUrl).HasMaxLength(512);
                entity.Property(e => e.SourceDn).HasMaxLength(128);
                entity.Property(e => e.SourceCallerId).HasMaxLength(128);
                entity.Property(e => e.SourceDisplayName).HasMaxLength(256);
                entity.Property(e => e.DestinationDn).HasMaxLength(128);
                entity.Property(e => e.DestinationCallerId).HasMaxLength(128);
                entity.Property(e => e.DestinationDisplayName).HasMaxLength(256);
                entity.Property(e => e.ActionDnDn).HasMaxLength(128);
                entity.Property(e => e.ActionDnCallerId).HasMaxLength(128);
                entity.Property(e => e.ActionDnDisplayName).HasMaxLength(256);
                entity.Property(e => e.Reason).HasMaxLength(512);
            });
        }
    }
}
