using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Repos.RDBMS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class UserAdminDataContext : DbContext
    {
        public UserAdminDataContext(DbContextOptions<UserAdminDataContext> contextOptions) : base(contextOptions)
        {

        }

        public DbSet<RDBMSOrg> Org { get; set; }
        public DbSet<RDBMSAppUser> AppUser { get; set; }
        public DbSet<Subscription> Subscription { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //  NuvIoTDev1234



            base.OnModelCreating(modelBuilder);

            modelBuilder.LowerCaseNames();
        }
    }


    public static class EFExtensions
    {
        public static void LowerCaseNames(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Replace table names
                entity.SetTableName(entity.GetTableName().ToLower());

                // Replace column names            
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToLower());
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToLower());
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName());
                }

            }
        }
    }
}
