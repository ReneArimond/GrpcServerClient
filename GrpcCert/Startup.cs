using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GrpcCert.Services;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Authentication.Certificate;

namespace GrpcCert
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /** <summary>starts up the gRPC service</summary>
         */
        public void ConfigureServices(IServiceCollection services)
        {
            var cert = new X509Certificate2("GrpcServer.pfx", "P@55w0rd");
            services.AddGrpc();
            services.Configure<KestrelServerOptions>(kestrelOptions =>
            {
                kestrelOptions.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.ClientCertificateMode = Microsoft.AspNetCore.Server.Kestrel.Https.ClientCertificateMode.RequireCertificate;
                    httpsOptions.ServerCertificate = cert;
                   // httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                });
            });
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(authOptions =>
                {
                    authOptions.AllowedCertificateTypes = CertificateTypes.Chained;
                    authOptions.ValidateCertificateUse = true;
                    authOptions.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
                    authOptions.CustomTrustStore = new X509Certificate2Collection(cert);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TestService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
