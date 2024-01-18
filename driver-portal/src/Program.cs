using HealthChecks.UI.Client;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pssg.DocumentStorageAdapter.Helpers;
using Rsbc.Dmf.DriverPortal.Api;
using Rsbc.Dmf.DriverPortal.Api.Services;
using System.Net;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost
    .UseUrls()
    .UseKestrel(options =>
    {
        options.Listen(IPAddress.Any, 8080);
    });

// add services to DI container
var services = builder.Services;
var env = builder.Environment;

services
    .AddAuthentication("token")
    //JWT tokens handling
    .AddJwtBearer("token", options =>
    {
        // uncomment for testing http://localhost
        //options.RequireHttpsMetadata = false;
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        builder.Configuration.GetSection("auth:token").Bind(options);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };

        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        // if token does not contain a dot, it is a reference token, forward to introspection auth scheme
        options.ForwardDefaultSelector = ctx =>
        {
            var authHeader = (string)ctx.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return null;
            return authHeader.Substring("Bearer ".Length).Trim().Contains(".") ? null : "introspection";
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var userService = ctx.HttpContext.RequestServices.GetRequiredService<IUserService>();
                ctx.Principal = await userService.Login(ctx.Principal);
                ctx.Success();
            }
        };
    })
    //reference tokens handling
    .AddOAuth2Introspection("introspection", options =>
    {
        options.EnableCaching = true;
        options.CacheDuration = TimeSpan.FromMinutes(1);
        builder.Configuration.GetSection("auth:introspection").Bind(options);
        options.Events = new OAuth2IntrospectionEvents
        {
            OnTokenValidated = async ctx =>
            {
                var userService = ctx.HttpContext.RequestServices.GetRequiredService<IUserService>();
                ctx.Principal = await userService.Login(ctx.Principal);
                ctx.Success();
            },
            OnUpdateClientAssertion =
            async ctx =>
            {
                await Task.CompletedTask;
            }
        };
    });

services.AddAuthorization(options =>
            {
                
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    "OIDC");
                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser().AddAuthenticationSchemes("token").RequireClaim("scope", "driver-portal-api");
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

services.AddCors();
services.AddControllersWithViews().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    // ignore omitted parameters on models to enable optional params (e.g. User update)
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Driver Portal API",
        Description = "An ASP.NET Core Web API for managing Driver Portal items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});

// Add Document Storage Adapter
services.AddDocumentStorageClient(builder.Configuration);

// Add Case Management System (CMS) Adapter 
services.AddCaseManagementAdapterClient(builder.Configuration);

// Health Checks
services.AddHealthChecks()
    .AddCheck("driver-portal", () => HealthCheckResult.Healthy("OK"));

services.AddHttpClient();
services.AddHttpContextAccessor();
services.AddTransient<IUserService, UserService>();
// NOTE temporary logger code, replace after adding logger e.g. Serilog/Splunk
using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());
services.AddAutoMapperSingleton(loggerFactory);

var app = builder.Build();

app.UseHealthChecks("/hc/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecks("/hc/live", new HealthCheckOptions
{
    // Exclude all checks and return a 200-Ok.
    Predicate = _ => false
});

// configure HTTP request pipeline


// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHsts();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}")
    .RequireAuthorization(); 

app.MapFallbackToFile("index.html");
app.MapSwagger();

app.Run();