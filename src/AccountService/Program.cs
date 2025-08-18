using AccountService.Behaviors;
using AccountService.Data;
using AccountService.Exceptions;
using AccountService.Filters;
using AccountService.Results;
using AccountService.Services.Interfaces;
using AccountService.Services.Methods;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire.PostgreSql;

ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => { options.Filters.Add(new AuthorizeFilter()); });
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options
        .UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection"))));

builder.Services.AddHangfireServer();
builder.Services.AddDbContext<AccountServiceContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = "http://keycloak:8080/realms/accountservice";
    options.Audience = "myclient";
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "http://keycloak:8080/realms/accountservice",
        ValidateAudience = true,
        ValidateLifetime = true
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
});
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "AccountService",
            Version = "v1",
            Description = "API для управления банковскими счетами и переводами"
        });
    c.EnableAnnotations();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
        c.SchemaFilter<EnumTypesSchemaFilter>(xmlPath);
    }

    c.SchemaFilter<ResultSchemaFilter>();
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddSingleton<IAccountStorageService, AccountStorageService>();
builder.Services.AddSingleton<IClientVerificationService, ClientVerificationService>();
builder.Services.AddSingleton<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ValidationFilter>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API AccountService v1");
        c.RoutePrefix = "swagger";
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountServiceContext>();
    db.Database.Migrate();
}
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is ConcurrencyException)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                Type = "ConcurrencyConflict",
                Title = "Concurrency Error",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            }));
        }
    });
});

RecurringJob.AddOrUpdate<InterestAccrualService>(
    "daily-interest-accrual",
    service => service.AccrueInterestForAllAccounts(),
    Cron.Daily(3, 0),
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc
    });

app.UseRouting();
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();


public class EnumTypesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
            schema.Description = string.Join(", ",
                Enum.GetNames(context.Type)
                    .Select(name => $"{name} = {Convert.ToInt64(Enum.Parse(context.Type, name))}"));
    }
}

public class ResultSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType) return;
        if (context.Type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = context.Type.GetGenericArguments()[0];
            schema.Properties["value"] = new OpenApiSchema
            { Reference = new OpenApiReference { Id = valueType.Name, Type = ReferenceType.Schema } };
        }
    }
}