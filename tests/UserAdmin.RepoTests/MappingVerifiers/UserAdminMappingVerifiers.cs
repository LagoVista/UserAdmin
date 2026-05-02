using LagoVista.Core.AutoMapper;
using LagoVista.Models;
using LagoVista.Relational;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAdmin.RepoTest.MappingVerifiers
{

        [TestFixture]
        public class UserAdminMappingVerifiers
        {
            public void VerifyMapping<TSource, TDestination>() where TSource : class where TDestination : class
            {
                try
                {
                    MappingVerifier.Verify<TSource, TDestination>(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.Replace("\\n", Environment.NewLine));
                    Assert.Fail();
                }
            }

            [Test]
            public void OrganizationToDto() => VerifyMapping<Organization, OrganizationDTO>();

        //    we will never load this, it's only used for saving, to bring data from the db on views.
        //    [Test]
        //    public void OrganizationFromDto() => VerifyMapping<OrganizationDTO, Organization>();
    }
}
