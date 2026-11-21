using System;
using System.Collections.Generic;
using System.Text;

namespace Xpera.domain.entities
{
    // UC-14: which plan a company pays for. The active subscription decides the company's plan,
    // and the plan decides which menus the store sees.
    public class Subscription
    {
        public int SubscriptionId { get; set; }

        public int CompanyId { get; set; }

        public string PlanCode { get; set; } = string.Empty;

        public decimal MonthlyFee { get; set; }

        public DateTime StartedOn { get; set; } = DateTime.Today;

        public DateTime RenewsOn { get; set; } = DateTime.Today.AddMonths(1);

        public bool IsActive { get; set; } = true;

        public Company? Company { get; set; }

        public Plan? Plan { get; set; }

    }
}
