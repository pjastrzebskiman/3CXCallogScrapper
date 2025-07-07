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
                // Klucz główny: tylko MainCallHistoryId
                entity.HasKey(e => e.MainCallHistoryId);
                entity.Property(e => e.SegmentId); // już nie klucz
                entity.Property(e => e.MainCallHistoryId).HasMaxLength(64);
                entity.Property(e => e.CallHistoryId).HasMaxLength(64);
                entity.Property(e => e.CdrId).HasMaxLength(64);
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
                entity.Property(e => e.Direction).HasMaxLength(32);
                entity.Property(e => e.CallType).HasMaxLength(64);
                entity.Property(e => e.Status).HasMaxLength(32);
                entity.HasIndex(e => e.StartTime);
            });
        }
    }
}
