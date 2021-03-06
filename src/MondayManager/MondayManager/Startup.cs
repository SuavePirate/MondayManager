using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MondayManager.Providers;
using MondayManager.Services;

using GraphQL.Client.Serializer.Newtonsoft;
using Voicify.Sdk.Webhooks.Services.Definitions;
using Voicify.Sdk.Webhooks.Services;

namespace MondayManager
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
            services.AddMvc().AddNewtonsoftJson();
            services.AddControllers();

            services.AddScoped<IMondayDataProvider, MondayDataProvider>();
            services.AddScoped<IMondayResponseService, MondayResponseService>();
            services.AddScoped<IDataTraversalService, DataTraversalService>();
            services.AddScoped<IEnhancedLanguageService, EnhancedLanguageService>();
            services.AddScoped<IPhraseParserService, PhraseParserService>();
            services.AddScoped((s) => new GraphQLHttpClient("https://api.monday.com/v2", new NewtonsoftJsonSerializer()));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
