using HogWildSystem.BLL;
using HogWildSystem.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem
{
    public static class HogWildExtension
    {
        public static void HogWildDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<HogWildContext>(options);

            //Add services
            services.AddScoped<WorkingVersionService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return context == null ?
                    throw new InvalidOperationException("HogWildContext is not registered.")
                    : new WorkingVersionService(context);
            });
            services.AddScoped<CustomerService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return context == null ?
                    throw new InvalidOperationException("HogWildContext is not registered.")
                    : new CustomerService(context);
            });
            services.AddScoped<LookupService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return context == null ?
                    throw new InvalidOperationException("HogWildContext is not registered.")
                    : new LookupService(context);
            });
            services.AddScoped<InvoiceService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return context == null ?
                    throw new InvalidOperationException("HogWildContext is not registered.")
                    : new InvoiceService(context);
            });
        }
    }
}
