﻿using System;
using System.Collections.Generic;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using OneIdentity.DevOps.Logic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using OneIdentity.DevOps.ConfigDb;
using Serilog;

namespace OneIdentity.DevOps
{
    internal class SafeguardDevOpsService
    {
        private readonly IWebHost _host;
        private readonly IEnumerable<IPluginManager> _services;

        public SafeguardDevOpsService()
        {
            var webSslCert = CheckDefaultCertificate();

            if (webSslCert == null)
            {
                Log.Logger.Error("Failed to find or change the default SSL certificate.");
                Environment.Exit(1);
            }

            Log.Logger.Information($"Configuration file location: {Path.Combine(WellKnownData.ServiceDirPath, WellKnownData.AppSettings)}.json");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"{Path.Combine(WellKnownData.ServiceDirPath, WellKnownData.AppSettings)}.json",
                    optional: true, reloadOnChange: true).Build();
            var httpsPort = configuration["HttpsPort"] ?? "443";

            _host = new WebHostBuilder()
                .UseSerilog()
                .UseKestrel(options =>
                {
                    int port;
                    if (int.TryParse(httpsPort, out port) == false)
                        port = 443;
                    Log.Logger.Information($"Binding web server to port: {port}.");
                    options.ListenAnyIP(port, listenOptions =>
                        {
                            listenOptions.UseHttps(webSslCert);
                        });
                })
                .ConfigureServices(services => services.AddAutofac())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            // TODO: better way to start this service??
            _services = (IEnumerable<IPluginManager>)_host.Services.GetService(typeof(IEnumerable<IPluginManager>));
        }

        public void Start()
        {
            _services.ToList().ForEach(s => s.Run());
            _host.RunAsync();
        }

        public void Stop()
        {
            _host.StopAsync().Wait();
        }

        private X509Certificate2 CheckDefaultCertificate()
        {
            using var db = new LiteDbConfigurationRepository();

            X509Certificate2 webSslCert = db.WebSslCertificate;
            if (webSslCert != null)
                return webSslCert;

            webSslCert = CertificateHelper.CreateDefaultSSLCertificate();
            db.WebSslCertificate = webSslCert;
            Log.Logger.Information("Created and installed a default web ssl certificate.");

            // Need to make sure that we return a db instance of the certificate rather than a local instance
            //  So rather than just returning the webSslCert created above, get a new instance of the certificate
            //  from the database.
            return db.WebSslCertificate;
        }
    }
}
