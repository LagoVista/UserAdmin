using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class AppUserLoginTypeFormFieldTests
    {
        [TestMethod]
        public void LoginTypePicker_UsesExistingEnum_AndUpdatesLoginType()
        {
            var user = new AppUser();
            Assert.AreEqual(LoginTypes.AppUser, user.LoginType);
            Assert.AreEqual(AppUser.LoginType_AppUser, user.LoginTypeSelection.Id);

            user.LoginTypeSelection = EntityHeader<LoginTypes>.Create(LoginTypes.Kiosk);
            Assert.AreEqual(LoginTypes.Kiosk, user.LoginType);
            Assert.AreEqual(AppUser.LoginType_Kiosk, user.LoginTypeSelection.Id);
            Assert.AreEqual("Kiosk", user.LoginTypeName);

            user.LoginTypeSelection = null;
            Assert.AreEqual(LoginTypes.Kiosk, user.LoginType);
        }

        [TestMethod]
        public void LoginTypePicker_IsIncludedInForm_AndHasAllEnumOptions()
        {
            Assert.IsTrue(new AppUser().GetFormFieldsCol2().Contains(nameof(AppUser.LoginTypeSelection)));

            var property = typeof(AppUser).GetProperty(nameof(AppUser.LoginTypeSelection));
            var attribute = (FormFieldAttribute)Attribute.GetCustomAttribute(property, typeof(FormFieldAttribute));
            Assert.IsNotNull(attribute);

            var field = FormField.Create("loginTypeSelection", attribute, property);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    AppUser.LoginType_DeviceOwner,
                    AppUser.LoginType_AppUser,
                    AppUser.LoginType_Kiosk,
                    AppUser.LoginType_AppEndUser
                },
                field.Options.Select(item => item.Id).ToArray());
            Assert.IsFalse(field.IsUserEditable);
        }

        [TestMethod]
        public void LoginTypePicker_DeserializesThroughExistingLoginType()
        {
            var user = new AppUser();
            JsonConvert.PopulateObject(
                "{\"loginTypeSelection\":{\"id\":\"appenduser\"}}", user);
            Assert.AreEqual(LoginTypes.AppEndUser, user.LoginType);
            Assert.AreEqual(LoginTypes.AppEndUser.ToString(), user.LoginTypeName);

            var loaded = JsonConvert.DeserializeObject<AppUser>("{\"loginType\":1}");
            Assert.AreEqual(LoginTypes.AppUser, loaded.LoginType);
            Assert.AreEqual(AppUser.LoginType_AppUser, loaded.LoginTypeSelection.Id);
        }
    }
}
