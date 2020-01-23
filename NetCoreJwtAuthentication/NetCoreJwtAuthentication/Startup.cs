using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreJwtAuthentication.Helpers;
using NetCoreJwtAuthentication.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System;
using NetCoreJwtAuthentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace NetCoreJwtAuthentication
{
    public class Startup
    {
        //public IConfigurationRoot Configuration { get; set; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration/*IHostingEnvironment env*/)
        {
            Configuration = configuration;

            //var builder = new ConfigurationBuilder()
            // .SetBasePath(env.ContentRootPath)
            // .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            //Configuration = builder.Build();

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors();
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //// ===== Add our DbContext ========
            //services.AddDbContext<ApplicationDbContext>();

            //// ===== Add Identity ========
            //services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();
            //OR
            //services.AddDbContext<ApplicationDbContext>
            //  (options => options
            //      .UseSqlServer("Data Source=DESKTOP-KR0EJIO;Initial Catalog=WebBoardAuth;Persist Security Info=True;User ID=sa;Password=123456789"));

            //Enable the use of an[Authorize("Bearer")] attribute on methods and classes to protect.
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                 .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌)
                  .RequireAuthenticatedUser().Build());
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader();
            }));

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            AppSettings appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.JwtKey);


            // ===== Add Jwt Authentication ========
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims


            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtAudience"],
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuer = false,
                        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key/*Encoding.UTF8.GetBytes(Configuration["JwtKey"])*/),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                    //cfg.Events = new JwtBearerEvents()
                    //{
                    //    OnTokenValidated = (context) => {
                    //        //context.Principal.Identity is ClaimsIdentity
                    //        //So casting it to ClaimsIdentity provides all generated claims
                    //        //And for an extra token validation they might be usefull
                    //        var name = context.Principal.Identity.Name;

                    //        if (string.IsNullOrEmpty(name))
                    //        {
                    //            context.Fail("Unauthorized. Please re-login");
                    //        }
                    //        return Task.CompletedTask;
                    //    }
                    //};

                    //cfg.EventsType = typeof(UserValidation);

                    cfg.Events = new JwtBearerEvents()
                    {
                        OnTokenValidated = (context) =>
                        {
                            ClaimsPrincipal userPrincipal = context.Principal;
                            //if (userPrincipal.HasClaim(c => c.Type == "name"))
                            //{
                            //    var username = userPrincipal.Claims.First(c => c.Type == "name").Value;
                            //    if (string.IsNullOrEmpty(username))
                            //    {
                            //        context.Fail("Unauthorized. Please re-login");
                            //    }
                            //}

                            //List<Claim> claims = userPrincipal.Claims.ToList();

                            //Claim claim = claims.ElementAt(0);
                            //string username = claim.Value;

                            //IEnumerable<string> IENumUsername = claims.Where(e => e.Type == "unique_name").Select(e => e.Value);
                            //string username_ = IENumUsername.ElementAt(0);

                            string username__ = userPrincipal.Claims.First(c => c.Type == "unique_name").Value;

                            if (string.IsNullOrEmpty(username__))
                            {
                                context.Fail("Unauthorized. Please re-login");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add our Config object so it can be injected
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // ===== Add MVC ========
            services.AddMvc();

            services.AddSingleton<IConfiguration>(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env/*,ApplicationDbContext dbContext*/)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseCors("AllowAll");
            // global cors policy
            //app.UseCors(x => x
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader());

            //app.UseCors(
            //     options => options.WithOrigins("http://localhost:8080").AllowAnyMethod()
            // );

            app.UseCors("ApiCorsPolicy");

            // ===== Use Authentication ======
            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 &&
                   !Path.HasExtension(context.Request.Path.Value) &&
                   !context.Request.Path.Value.StartsWith("/api/"))
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });

            app.UseMvcWithDefaultRoute();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc();

            // ===== Create tables ======
            // dbContext.Database.EnsureCreated();

        }


        public class UserValidation : JwtBearerEvents
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Token { get; set; }

            public override async Task TokenValidated(TokenValidatedContext context)
            {
                try
                {

                    ClaimsPrincipal userPrincipal = context.Principal;

                    //this.Id = userPrincipal.Claims.First(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                    //if (userPrincipal.HasClaim(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))
                    //{
                    //    this.UserEmail = userPrincipal.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
                    //}

                    if (userPrincipal.HasClaim(c => c.Type == "name"))
                    {
                        this.Username = userPrincipal.Claims.First(c => c.Type == "name").Value;
                    }
                    if (this.Username != null)
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception("TokenValidate Failed");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }


}




