using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using WealthTools.Authentication.Common.TSA;
using WealthTools.Common;
using WealthTools.Common.Cache;
using WealthTools.Common.DatabaseConnection;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.Encryption;
using WealthTools.Common.Logger;
using WealthTools.Common.Logger.Interfaces;
using WealthTools.Common.Models;
using WealthTools.Common.Utils.WebAPI;
using WealthTools.Middleware.Usage;


namespace WealthTools.WebAPI.Logger
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BaseApiStartupSetup.AddCommonServices(services);
            BaseApiStartupSetup.AddSwagger(services, "v1.0", "WealthTools.WebAPI.Logger", "WealthTools.WebAPI.Logger");
            BaseApiStartupSetup.AddAuthorization(services, Configuration);

            BaseApiStartupSetup.AddHealthCheck(services, Configuration,
                                new List<HealthCheck>() { HealthCheck.OracleConnection });

            // Add Project Specific Packages
            services.AddTransient<IUsageLogRepository, UsageLogRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IWMLogger logger, IHttpContextAccessor accessor, IServiceProvider serviceProvider)
        {
            BaseApiStartupSetup.UseHealthChecks(app);
            BaseApiStartupSetup.UseExceptionHandler(app, env, logger, "WealthTools.WebAPI.Logger");
            BaseApiStartupSetup.UseCors(app);
            BaseApiStartupSetup.UseSwagger(app, "../swagger/v1.0/swagger.json", "WealthTools.WebAPI.Logger v1.0");
            app.UseAuthentication();
            //app.UseWealthToolsUsage();
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
