// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 20996ac5afa40f0a9a6866d23f9e5f19d990acbd95cc2a08c6922dea096d930c
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LagoVista.UserAdmin.Tests
{
    public class TestBase
    {
        protected const string TEST_ORG_ID1 = "1ORG71BA323448A69497918C9E226C71";
        protected const string TEST_ORG_ID2 = "2ORG71BA323448A69497918C9E226C72";
        protected const string TEST_ORG_ID3 = "3ORG71BA323448A69497918C9E226C73";
        protected const string TEST_USER_ID1 = "1USERBEA25F149359F5E5A0ACE20329C";
        protected const string TEST_USER_ID2 = "2USERBEA25F149359F5E5A0ACE20329C";
        protected const string TEST_USER_ID3 = "3USERBEA25F149359F5E5A0ACE20329C";

        protected EntityHeader OrgEH1
        {
            get { return EntityHeader.Create(TEST_ORG_ID1, "TESTORG"); }
        }

        protected EntityHeader UserEH1
        {
            get { return EntityHeader.Create(TEST_USER_ID1, "TESTUSER"); }
        }

        protected EntityHeader OrgEH2
        {
            get { return EntityHeader.Create(TEST_ORG_ID2, "TESTORG"); }
        }

        protected EntityHeader UserEH2
        {
            get { return EntityHeader.Create(TEST_USER_ID2, "TESTUSER"); }
        }

        protected EntityHeader OrgEH3
        {
            get { return EntityHeader.Create(TEST_ORG_ID3, "TESTORG"); }
        }

        protected EntityHeader UserEH3
        {
            get { return EntityHeader.Create(TEST_USER_ID3, "TESTUSER"); }
        }

        protected void AssertValid(InvokeResult result)
        {
            Assert.IsTrue(result.Successful, result.ErrorMessage);
        }

        protected void AssertInValid(InvokeResult result, int errorCount, params string[] expectedErrorCodes)
        {
            Assert.IsFalse(result.Successful, "Expected to have an invalid result, but result was  valid.");
            Assert.AreEqual(errorCount, result.Errors.Count, "Error count did not match.");
            foreach (var errorCode in expectedErrorCodes)
            {
                Assert.AreEqual(1, result.Errors.Where(err => err.ErrorCode == errorCode).Count(), $"Did not find expected error code {errorCode}");
            }

            Console.WriteLine("Errors (Expected)");
            Console.WriteLine("==========================");
            foreach (var err in result.Errors)
            {
                Console.WriteLine($"\t{err.ErrorCode} - {err.Message}");
            }
        }


    }
}
