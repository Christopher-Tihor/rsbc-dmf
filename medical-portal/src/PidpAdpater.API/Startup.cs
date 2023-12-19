﻿using FluentValidation.AspNetCore;
using pdipadapter.Infrastructure;
using pdipadapter.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Serilog;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using pdipadapter.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using pdipadapter.Core.Http;
using MediatR;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using pdipadapter.Common;
using pdipadapter.Infrastructure.HttpClients;
using pdipadapter.Helpers.Mapping;
using PidpAdapter.API.Infrastructure;
using MediatR.Registration;
using Mapster;
using static MedicalPortal.API.Features.Users.Commands.CreateUser;
using pdipadapter.Infrastructure.Services;

namespace pdipadapter;
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => this.Configuration = configuration;
    public void ConfigureServices(IServiceCollection services)
    {
        var config = this.InitializeConfiguration(services);
       // var jsonSerializerOptions = this.Configuration.GenerateJsonSerializerOptions();
        services
          .AddAutoMapper(typeof(Startup))
          .AddHttpClients(config)
          .AddKeycloakAuth(config)
          .AddScoped<IPidpAdapterAuthorizationService, PidpAdapterAuthorizationService>()
          .AddSingleton<IClock>(NodaTime.SystemClock.Instance).AddSingleton<Microsoft.Extensions.Logging.ILogger>(svc => svc.GetRequiredService<ILogger<CommandHandler>>());

        services.AddMapster(options =>
        {
            options.Default.IgnoreNonMapped(true);
            options.Default.IgnoreNullValues(true);
            options.AllowImplicitDestinationInheritance = true;
            options.AllowImplicitSourceInheritance = true;
            options.Default.UseDestinationValue(member =>
                member.SetterModifier == AccessModifier.None &&
                member.Type.IsGenericType &&
                member.Type.GetGenericTypeDefinition() == typeof(ICollection<>));
        });


        services.AddControllers().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

        //services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("Administrator", policy => policy.Requirements.Add(new RealmAccessRoleRequirement("administrator")));
        //    options.AddPolicy(Infrastructure.Auth.Policies.MedicalPractitioner, policy => policy
        //    .RequireAuthenticatedUser()
        //    .RequireRole(Roles.Practitoner, Roles.Moa));
        //    options.AddPolicy(Infrastructure.Auth.Policies.DmftEnroledUser, policy => policy
        //    .RequireAuthenticatedUser()
        //    .RequireRole(Roles.DfmtEnroledRole));
        //});

        services.AddControllers(options => options.Conventions.Add(new RouteTokenTransformerConvention(new KabobCaseParameterTransformer())))
            .AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<Startup>())
            .AddJsonOptions(options => options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb))
            .AddHybridModelBinder();
        services.AddHttpClient();

        //services.AddDbContext<JumDbContext>(options => options
        //    .UseSqlServer(config.ConnectionStrings.JumDatabase, sql => sql.UseNodaTime())
        //    .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: false));

        var serviceConfig = new MediatRServiceConfiguration();
        ServiceRegistrar.AddRequiredServices(services, serviceConfig);

        services.Scan(scan => scan
        .AddTypes(typeof(IRequestHandler<>))
        .AsSelf()
        .WithScopedLifetime());

        //services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
        services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
       // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        //services.AddFluentValidation(new[] { typeof(UpdatePlayerCommandHandler).GetTypeInfo().Assembly });
        //services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
        //services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddSingleton<ProblemDetailsFactory, UserManagerProblemDetailsFactory>();

        services.AddSingleton<IAuthorizationHandler, RealmAccessRoleHandler>();
        services.AddTransient<IClaimsTransformation, KeycloakClaimTransformer>();
        services.AddHttpContextAccessor();
        services.AddTransient<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
        services.AddScoped<IProxyRequestClient, ProxyRequestClient>();
        //services.AddScoped<IOpenIdConnectRequestClient, OpenIdConnectRequestClient>();

       // services.AddMediatR(typeof(MedicalPortal.API.Features.Users.Commands.CreateUser).GetTypeInfo().Assembly);
        //services.AddDynamics(this.Configuration);
        services.AddDistributedMemoryCache();
        services.AddCmsAdapterGrpcService(config);

        //services.AddGrpc(o => o.Interceptors.Add<UserManagerProblemDetailsFactory>());

        services.AddHealthChecks()
            .AddCheck("liveliness", () => HealthCheckResult.Healthy())
            .AddSqlServer(config.ConnectionStrings.JumDatabase, tags: new[] { "services" });

        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new HeaderApiVersionReader("api-version");
        });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pidp Adapter API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            options.CustomSchemaIds(x => x.FullName);
        });

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .WithOrigins("http://localhost:8089", "http://localhost:9092", "http://localhost:4200", "https://medical-portal-pidp-0137d5-dev.apps.silver.devops.gov.bc.ca") //use config later
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        }); 

        services.AddFluentValidationRulesToSwagger();

    }
    private PdipadapterConfiguration InitializeConfiguration(IServiceCollection services)
    {
        var config = new PdipadapterConfiguration();
        this.Configuration.Bind(config);
        services.AddSingleton(config);

        Log.Logger.Information("### App Version:{0} ###", Assembly.GetExecutingAssembly().GetName().Version);
        Log.Logger.Information("### Pidp Adapter Configuration:{0} ###", JsonSerializer.Serialize(config));

        return config;
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
         
        }
        //app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseExceptionHandler("/error");
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pidp Api Adapter"));

        app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userId = httpContext.User.GetUserId();
            if (!userId.Equals(Guid.Empty))
            {
                diagnosticContext.Set("User", userId);
            }
        });
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            // endpoints.MapHealthChecks("/health");
        });

    }
}
