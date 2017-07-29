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
        public DbSet<RDBMSOrg> Org { get; set; }
        public DbSet<RDBMSAppUser> AppUser { get; set; }
        public DbSet<Subscription> Subscription { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //  NuvIoTDev1234

            base.OnModelCreating(modelBuilder);
        }
    }
}
