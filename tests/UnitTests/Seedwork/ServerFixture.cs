﻿using Esquio.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Xunit;

namespace UnitTests.Seedwork
{
    public class ServerFixture
    {
        public TestServer TestServer { get; set; }

        public ServerFixture()
        {
            var testServer = new TestServer();

            var host = Host.CreateDefaultBuilder()
                 .UseContentRoot(Directory.GetCurrentDirectory())
                 .ConfigureAppConfiguration((_, cfg) =>
                 {
                     cfg.AddJsonFile("appsettings.json", optional: false);
                 })
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder
                     .UseServer(testServer)
                     .UseStartup<TestStartup>();
                 }).Build();

            host.StartAsync().Wait();

            TestServer = host.GetTestServer();
        }
    }

    [CollectionDefinition(nameof(AspNetCoreServer))]
    public class AspNetCoreServer
        : IClassFixture<ServerFixture>
    {

    }

    class TestStartup
    {
        private IConfiguration _configuration;

        public TestStartup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .Services
                .AddEsquio()
                .AddConfigurationStore(_configuration, "Esquio");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting(routes =>
            {
                routes.MapEsquio("esquio");
                routes.MapControllerRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRazorPages();
            });

            app.UseAuthorization();
        }
    }

    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }

        [ActionName("ActionWithFlag")]
        [Flag(ApplicationName = "TestApp",FeatureName ="Sample1")]
        public IActionResult Sample1()
        {
            return Content("Active");
        }

        [ActionName("ActionWithFlag")]
        public IActionResult Sample11()
        {
            return Content("NonActive");
        }


        [ActionName("ActionWithFlagNoActive")]
        [Flag(ApplicationName = "TestApp", FeatureName = "Sample2")]
        public IActionResult Sample2()
        {
            return Content("Active");
        }

        [ActionName("ActionWithFlagNoActive")]
        public IActionResult Sample22()
        {
            return Content("NonActive");
        }
    }
}
