using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Web.Cors;

namespace TEST_CSC_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;            
        }

        public IConfiguration Configuration { get; }
        readonly string MySpecificOrigins = "origin";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.ConfigureWritable<Transsped>(Configuration.GetSection("Transsped"));


            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.AddMvc();
            services.AddCors(options =>
            {
                options.AddPolicy(MySpecificOrigins, builder =>
                {
                    builder.WithOrigins("http://localhost:64357", "https://msign-test.transsped.ro/https://msign-test.transsped.ro/moa-id-auth/oauth2/auth").AllowAnyHeader().AllowAnyMethod();
                });
            });
            

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Transsped";
            })
            .AddCookie()
            .AddOAuth("Transsped", options =>
            {
                options.ClientId = Configuration["Transsped:ClientID"];
                options.ClientSecret = Configuration["Transsped:ClientSecret"];
                options.AuthorizationEndpoint = Configuration["Transsped:AuthURL"];
                options.TokenEndpoint = Configuration["Transsped:TokenURL"];
                options.UserInformationEndpoint = Configuration["Transsped:TokenURL"];
                options.CallbackPath = new PathString("/redirect");

                options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.TokenEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var user = JObject.Parse(await response.Content.ReadAsStringAsync());

                        context.RunClaimActions(user);
                    }
                };
            });

            services.AddSingleton<IAccessToken, MyAccessToken>();
            
 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(env.ContentRootPath)
            //    .AddJsonFile("appsettings.json",
            //                 optional: false,
            //                 reloadOnChange: true)
            //    .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
               // builder.AddUserSecrets<Startup>();
                app.UseDeveloperExceptionPage();
            }
            app.UseCors();
            app.UseMvc();
          
        }
    }
}
