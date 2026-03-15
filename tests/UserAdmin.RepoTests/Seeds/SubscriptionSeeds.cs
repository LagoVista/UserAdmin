using LagoVista.Core;
using LagoVista.UserAdmin.Models.Orgs;
using Relational.Tests.Core.Seeds;
using Relational.Tests.Core.Utils;
using System;
using System.Collections.Generic;

namespace BillingTests.Common.Seeds
{
    public class SubscriptionSeeds
    {
        public static Subscription Primary { get; private set; }
        public static Subscription Secondary { get; private set; }
        public static Subscription SecondOrg { get; private set; }

        public static Subscription Active { get; set; }
        public static Subscription Inactive { get; set; }

        public static void Populate(int count)
        {
            All.Clear();

            if (count < 4) throw new ArgumentException("Count must be at least 4 to populate all seed data");

            for (var idx = 0; idx < count; ++idx)
            {
                var subscription = new Subscription
                {
                    Id = TestSeeds.CreateGuidString((byte)(idx + 1)),
                    Name = $"Subscription {idx + 1}",
                    Key = $"subscription{idx + 1}",
                    Description = $"Seed Subscription {idx + 1}",
                    Start = CalendarDate.Create(2026,1,1),
                    End = CalendarDate.Create(2026,8,1),
                    ActiveDate = CalendarDate.Create(2026,1,1),
                    IsActive = true,
                    Status = Subscription.Status_OK,
                    PaymentTokenStatus = Subscription.PaymentTokenStatus_Empty,
                }.StampRelational();

                if (idx == 0) Primary = subscription;
                if (idx == 1) Secondary = subscription;
                if (idx == 2) { subscription.IsActive = true; Active = subscription; }
                if (idx == 3) { subscription.IsActive = false; Inactive = subscription; }
                if(idx == count - 1)
                {
                    subscription.OwnerOrganization = OrganizationSeeds.Secondary.ToEntityHeader();
                    SecondOrg = subscription;
                }
                All.Add(subscription);
            }
        }

        public static List<Subscription> All = new();
    }
}

