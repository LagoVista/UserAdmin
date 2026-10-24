using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Repos.Orgs;
using NUnit.Framework;
using System;
using System.Reflection;

namespace LagoVista.UserAdmin.Tests
{
    [TestFixture]
    public class OrganizationStorageOwnerTests
    {
        private static void Normalize(Organization organization)
        {
            var method = typeof(OrganizationRepo).GetMethod(
                "EnsureOrganizationStorageOwner",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, new object[] { organization });
        }

        [Test]
        public void MissingOwnerDefaultsToOrganizationItself()
        {
            var org = new Organization { Id = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Name = "Test Organization" };
            org.OwnerOrganization = null;

            Normalize(org);

            Assert.That(org.OwnerOrganization.Id.ToString(), Is.EqualTo(org.Id.ToString()));
            Assert.That(org.OwnerOrganization.Text, Is.EqualTo(org.Name));
        }

        [Test]
        public void ExplicitOwnerIsPreserved()
        {
            var org = new Organization { Id = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Name = "Test Organization" };
            org.OwnerOrganization = EntityHeader.Create("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "Parent");

            Normalize(org);

            Assert.That(org.OwnerOrganization.Id.ToString(), Is.EqualTo("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"));
            Assert.That(org.OwnerOrganization.Text, Is.EqualTo("Parent"));
        }

        [Test]
        public void BlankOwnerIdDefaultsToSelf()
        {
            var org = new Organization { Id = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Name = "Test Organization" };
            org.OwnerOrganization = new EntityHeader { Id = "", Text = "Unassigned" };

            Normalize(org);

            Assert.That(org.OwnerOrganization.Id.ToString(), Is.EqualTo(org.Id.ToString()));
        }

        [Test]
        public void InvalidOrganizationIdIsRejected()
        {
            var org = new Organization { Id = default, Name = "Invalid" };
            org.OwnerOrganization = null;

            var exception = Assert.Throws<TargetInvocationException>(() => Normalize(org));
            Assert.That(exception.InnerException, Is.TypeOf<ArgumentNullException>());
        }
    }
}
