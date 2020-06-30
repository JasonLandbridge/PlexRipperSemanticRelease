﻿using Autofac;
using AutofacSerilogIntegration;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;
using PlexRipper.Application.Config;
using PlexRipper.WebAPI.Config;
using System.Linq;
using System.Reflection;

namespace PlexRipper.WebAPI
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {

            // TODO Make sure to configure this correctly when setting up security
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // app.UseHttpsRedirection();

            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // serve Swagger UI

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // app.UseAuthorization();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Fluent Validator
            services.AddMvc().AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssemblyContaining<ApplicationModule>();
                fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
            });
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // General
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddCors();

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "PlexRipper API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            // Autofac
            services.AddOptions();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            ContainerConfig.ConfigureContainer(builder);
            builder.RegisterLogger(autowireProperties: true);

        }

    }
}
