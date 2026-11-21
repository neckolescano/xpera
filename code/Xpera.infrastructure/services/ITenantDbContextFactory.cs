using System;
using System.Collections.Generic;
using System.Text;
using Xpera.infrastructure.data;

namespace Xpera.infrastructure.services
{
    public interface ITenantDbContextFactory
    {
        Task<TenantCrmDbContext> CreateAsync(int companyId);
    }
}
