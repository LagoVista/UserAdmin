using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Managers
{
    public class DefaultRoleList : IDefaultRoleList
    {
        public const string ORG_ADMIN = "orgadmin";
        public const string OWNER = "owner";
        public const string EXECUTIVE = "executive";
        public const string IOT_APP_BUILDER = "iotappbuilder";
        public const string IOT_APP_PUBLISHER = "iotapppublisher";
        public const string DEVICE_MANAGER = "devicemanager";
        public const string APP_BUILDER = "dashboardbuilder";
        public const string APP_CONSUMER = "dashboardconsumer";
        public const string REPORT_BUILDER = "reportbuilder";
        public const string REPORT_VIEWER = "reportviewer";
        public const string PROJECT_SYS_ADMIN = "projectsysadmin";
        public const string PROJECT_MANAGER = "projectmanager";
        public const string PROJECT_CONTRIBUTOR = "projectcontributor";
        public const string CONTENT_BUILDER = "contentcreator";
        public const string CONTENT_VIEWER = "contentviewer";
        public const string ACCOUNTING = "accounting";
        public const string DEVOPS = "devops";
        public const string BOOKKEEPING = "bookkeeping";
        public const string USER_ADMIN = "useradmin";
        public const string SECURITY_ADMIN = "securityadmin";
        public const string TIME_BILLING = "timebilling";
        public const string EXPENSE_BILLING = "expensebilling";
        public const string HUMAN_RESOURCES = "humanresources";
        public const string FSL_ADMIN = "fsladmin";
        public const string FSL_MANAGER = "fslmanager";
        public const string FSL_TKT_HANDLER = "fslhandler";
        public const string MARKETING_MANAGER = "marketing_manager";
        public const string MARKETING = "marketing";


        public IEnumerable<Role> GetStandardRoles()
        {
            return new List<Role>()
            {
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B01", IsPublic = true, IsSystemRole = true, Name = "Owner", AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER}, Key=DefaultRoleList.OWNER, Description="User that initially created the organization or has ultiamte rights to everything within the portal.." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B02", IsPublic = true, IsSystemRole = true, Name = "Executive",AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER}, Key=DefaultRoleList.EXECUTIVE, Description="User that initially created the organization or has ultiamte rights to everything within the portal.." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B03", IsPublic = true, IsSystemRole = true, Name = "Organization Admin",AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER, DefaultRoleList.USER_ADMIN}, Key=DefaultRoleList.ORG_ADMIN, Description="User will full rights to everything within an organization." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B04", IsPublic = true, IsSystemRole = true, Name = "User Admin",AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER, DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN}, Key=DefaultRoleList.USER_ADMIN, Description="User will full rights to everything within an organization." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B05", IsPublic = true, IsSystemRole = true, Name = "Security Admin",AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER, DefaultRoleList.ORG_ADMIN}, Key=DefaultRoleList.SECURITY_ADMIN, Description="User will full rights to everything within an organization." },
               
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B06", IsPublic = true, IsSystemRole = true, Name = "Dev Ops",AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER, DefaultRoleList.ORG_ADMIN}, Key=DefaultRoleList.DEVOPS, Description="User will full rights to everything within an organization." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B07", IsPublic = true, IsSystemRole = true, Name = "IoT App Builder",AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN},  Key=DefaultRoleList.IOT_APP_BUILDER, Description="Can create and manage assets ossociated with an IoT application." },
                new Role() { Id = "ACDC1BADF00D1CAFEF12CE0FF55F2B08", IsPublic = true, IsSystemRole = true, Name = "IoT App Publisher",AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN}, Key = DefaultRoleList.IOT_APP_PUBLISHER, Description = "Can allocate resource for publish IoT applications." }, 
                new Role() { Id = "ACDC1BADF00D1CAFEF12CE0FF55F2B09", IsPublic = true, IsSystemRole = true, Name = "Device Manager", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN}, Key = DefaultRoleList.DEVICE_MANAGER, Description = "Can Create and Manage devices." },

                new Role() { Id = "ACDC1BADF00D1CAFEF12CE0FF55F2B10", IsPublic = true, IsSystemRole = true, Name = "Dashboard App Builder", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN}, Key = DefaultRoleList.APP_BUILDER, Description = "Can create dashboards to view data generated by IoT devices." },
                new Role() { Id = "ACDC1BADF00D1CAFEF12CE0FF55F2B11", IsPublic = true, IsSystemRole = true, Name = "Dashboard App Viewer", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN, DefaultRoleList.APP_BUILDER},  Key = DefaultRoleList.APP_CONSUMER, Description = "Can view data and dashboards generated by IoT devices." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B12", IsPublic = true, IsSystemRole = true, Name = "Project System Admin", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN },  Key=DefaultRoleList.PROJECT_SYS_ADMIN, Description="Can change how project related task and task status are defined." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B13", IsPublic = true, IsSystemRole = true, Name = "Project Manager", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN}, Key=DefaultRoleList.PROJECT_MANAGER, Description="Can manage projects, create tasks and task assignments." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B14", IsPublic = true, IsSystemRole = true, Name = "Project Contributor", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN}, Key=DefaultRoleList.PROJECT_CONTRIBUTOR, Description="Can change the status on tasks" },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B15", IsPublic = true, IsSystemRole = true, Name = "Report Builder", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN},  Key=DefaultRoleList.REPORT_BUILDER, Description="Can create and publish reports used to view data produced from devices." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B16", IsPublic = true, IsSystemRole = true, Name = "Report Viewer",  AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN,  DefaultRoleList.REPORT_BUILDER}, Key=DefaultRoleList.REPORT_VIEWER, Description="Can view data produced from devices." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B17", IsPublic = true, IsSystemRole = true, Name = "Content Builder",  AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN }, Key=DefaultRoleList.CONTENT_BUILDER, Description="Can create new content within the system." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B18", IsPublic = true, IsSystemRole = true, Name = "Content Viewer", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.USER_ADMIN },  Key=DefaultRoleList.CONTENT_VIEWER, Description="Can view content created within the system." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B19", IsPublic = true, IsSystemRole = true, Name = "Accounting", AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER},  Key=DefaultRoleList.ACCOUNTING, Description="Process payroll and maintain a list of accounts." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B20", IsPublic = true, IsSystemRole = true, Name = "Book Keeping", AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER, DefaultRoleList.ACCOUNTING},  Key=DefaultRoleList.BOOKKEEPING, Description="Can enter and maintain data used within accounts." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B21", IsPublic = true, IsSystemRole = true, Name = "User Admin",  AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER},  Key=DefaultRoleList.USER_ADMIN, Description="User Admin can create new accounts." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B22", IsPublic = true, IsSystemRole = true, Name = "Time Billing", AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER, DefaultRoleList.USER_ADMIN},  Key=DefaultRoleList.TIME_BILLING, Description="Can create and manage assets ossociated with an IoT application." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B23", IsPublic = true, IsSystemRole = true, Name = "Expense Billing",  AuthorizedGranterRoles = new List<string> {DefaultRoleList.OWNER, DefaultRoleList.USER_ADMIN},  Key=DefaultRoleList.EXPENSE_BILLING, Description="Can create and manage assets ossociated with an IoT application." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B24", IsPublic = true, IsSystemRole = true, Name = "Field Serivce Lite - System Admin", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN}, Key=DefaultRoleList.FSL_ADMIN, Description="The Field Serivce Light Admin can configure settings associated with Field Serivce Light." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B25", IsPublic = true, IsSystemRole = true, Name = "Field Serivce Lite - Manager", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN, DefaultRoleList.FSL_ADMIN}, Key=DefaultRoleList.FSL_MANAGER, Description="The Field Serivce Light Manager is responsible for maintaining and assigning service tickets." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B26", IsPublic = true, IsSystemRole = true, Name = "Field Serivce Lite - Ticket Handler", AuthorizedGranterRoles = new List<string> {DefaultRoleList.FSL_MANAGER, DefaultRoleList.ORG_ADMIN}, Key=DefaultRoleList.FSL_TKT_HANDLER, Description="The Field Serivce Light ticket handler is responsible for the execution of service tickets." },

                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B27", IsPublic = true, IsSystemRole = true, Name = "Marketing", AuthorizedGranterRoles = new List<string> {DefaultRoleList.MARKETING_MANAGER, DefaultRoleList.ORG_ADMIN},  Key=DefaultRoleList.MARKETING, Description="Marketing role is responsible for content and execution of marketing strategies." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B28", IsPublic = true, IsSystemRole = true, Name = "Marketing Manager", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN},  Key=DefaultRoleList.MARKETING_MANAGER, Description="Marketing Manager is responsible for all public facing communications for the organization and it's products." },
                new Role() { Id=  "ACDC1BADF00D1CAFEF12CE0FF55F2B29", IsPublic = true, IsSystemRole = true, Name = "Human Resources", AuthorizedGranterRoles = new List<string> {DefaultRoleList.ORG_ADMIN}, Key=DefaultRoleList.HUMAN_RESOURCES, Description="Human resources is responsible for maintaining user records and other related information." },
              };

    }
}
}
