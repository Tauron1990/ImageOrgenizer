using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageOrganizer.Web.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application;
using Tauron.Application.Common.BaseLayer.BusinessLayer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.ImageOrginazer.ViewModels;
using Tauron.Application.Implement;
using Tauron.Application.Ioc;

namespace ImageOrganizer.Web
{
    public class Startup : CommonApplication
    {
        private object _sync = new object();

        public Startup(IConfiguration configuration) 
            : base(true, null, new WebUIControllerFactory()) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            Monitor.Enter(_sync);
            Task.Run(() => OnStartup(new string[0]));

            lock (_sync)
                return new AggregateServiceProvider(services.BuildServiceProvider(), Current.Container);
        }

        public override IContainer Container { get; set; }

        protected override IWindow DoStartup(CommandLineProcessor args)
        {
            Monitor.Exit(_sync);
            return base.DoStartup(args);
        }

        protected override void Fill(IContainer container)
        {
            var resolver = new ExportResolver();

            foreach (var asm in new[]
            {
                typeof(Startup).Assembly,
                typeof(CommonApplication).Assembly,
                typeof(RuleFactory).Assembly,
                typeof(OperatorImpl).Assembly,
                typeof(DatabaseImpl).Assembly,
                typeof(MainWindowViewModel).Assembly
            })
                resolver.AddAssembly(asm);

            container.Register(resolver);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.use

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
