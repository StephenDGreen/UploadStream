using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Upload.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Upload.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;

namespace Upload
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
                options.UseSqlServer(
                    Configuration.GetConnectionString("IdentityConnection")));
            #region snippet_AddRazorPages
            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions
                            .AddPageApplicationModelConvention("/LargeFileUpload",
                                model =>
                                {
                                    model.Filters.Add(
                                        new GenerateAntiforgeryTokenCookieAttribute());
                                    model.Filters.Add(
                                        new DisableFormValueModelBindingAttribute());
                                    model.Filters.Add(
                                        new RequestFormLimitsAttribute()
                                        {
                                            MultipartBodyLengthLimit = Configuration.GetValue<long>("LargeFileSizeLimit"),
                                            MemoryBufferThreshold = Int32.MaxValue
                                        });
                                });
                    options.Conventions
                        .AddPageApplicationModelConvention("/FileUpload",
                            model =>
                            {
                                model.Filters.Add(
                                    new GenerateAntiforgeryTokenCookieAttribute());
                                model.Filters.Add(
                                    new DisableFormValueModelBindingAttribute());
                            });
                    options.Conventions
                        .AuthorizePage("/FileUpload");
                    options.Conventions
                        .AuthorizePage("/FileList");
                    options.Conventions
                        .AuthorizePage("/LargeFileUpload");
                });
            services.AddAuthentication()
                .AddCookie()
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Tokens:Issuer"],
                        ValidAudience = Configuration["Tokens:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                    };

                });
            services.AddDbContext<UploadContext>(options => options.UseSqlServer(
                    Configuration.GetConnectionString("UploadConnection")));
            services.AddControllers();
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
