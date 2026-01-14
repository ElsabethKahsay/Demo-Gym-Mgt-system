using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoulFitness.Abstractions;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using SoulFitness.DataObjects.UserManagment.Services;
using SoulFitness.Utilities;

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace SoulFitness.web
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SoulFitnessConnection"),
                b => b.CommandTimeout(900).EnableRetryOnFailure().MigrationsAssembly("SoulFitness.Web")).EnableSensitiveDataLogging()
                );

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Soul Fitness API", Version = "v1" });
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<IRoleManager, RoleManager>();


            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IDbInitializer, DbInitializer>();

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddAuthentication().AddCookie(o =>
            {
                o.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Soul Fitness API v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "FileUploads")),
                RequestPath = "/FileUploads"
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            dbInitializer.Initialize();
        }
    }
}
