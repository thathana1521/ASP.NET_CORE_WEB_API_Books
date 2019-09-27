using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Services;
using BookApiProject.Services.Interfaces;
using BookApiProject.Services.Repositories;
using BookApiProject.Services.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using SwaggerOptions = BookApiProject.Services.Swagger.SwaggerOptions;

namespace BookApiProject
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(o=>o.SerializerSettings.ReferenceLoopHandling = 
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            

            var connectionString = Configuration["connectionStrings:bookDbConnectionString"];
            services.AddDbContext<BookDbContext>(c =>c.UseSqlServer(connectionString));

            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IReviewerRepository, ReviewerRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IBookRepository,BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();

            //add swagger
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Book API", Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, BookDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var swaggerOptions = new SwaggerOptions();
            Configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);

            app.UseSwagger(option => { option.RouteTemplate = swaggerOptions.JsonRoute; });
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint(swaggerOptions.UiEndpoint, swaggerOptions.Description);
            });

            /*app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });*/

            //Just for the first time
            //context.SeedDataContext();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            
            app.UseMvc();
        }
    }
}
