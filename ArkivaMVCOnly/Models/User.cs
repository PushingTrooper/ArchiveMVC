namespace ArkivaMVCOnly
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using ArkivaMVCOnly.Models;

    public class User : DbContext
    {
        public User()
            : base("name=test")
        {
        }

        public virtual DbSet<TJ_Users> TJ_Users { get; set; }
        public virtual DbSet<TJ_Dokument> TJ_Dokument { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TJ_Users>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Users>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.emri_dokumentit)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.emri_subjektit)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.nr_inspektimi)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.lloji)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.fusha_indeksimit)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.vend_zyra)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.vend_nr_kutise)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.vend_rafti)
                .IsUnicode(false);

            modelBuilder.Entity<TJ_Dokument>()
                .Property(e => e.CreatedBy)
                .IsUnicode(false);
        }
    }
}
