using System;
using System.Collections.Generic;
using System.Text;

namespace Xpera.domain.entities
{
    // The three plans a store can subscribe to. Each plan includes everything in the plan before it.
    // A = Micro (Tenant A), B = Small (Tenant B), C = Medium (Tenant C).
    public class Plan
    {
        public string PlanCode { get; set; } = string.Empty;

        public string PlanName { get; set; } = string.Empty;

        public decimal MonthlyFee { get; set; }

        public string Includes { get; set; } = string.Empty;

    }
}
