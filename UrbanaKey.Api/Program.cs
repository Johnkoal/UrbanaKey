using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Api.Middleware;
using UrbanaKey.Api.Endpoints;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;
using UrbanaKey.Core.Interfaces;
using UrbanaKey.Infrastructure.Persistence;
using UrbanaKey.Infrastructure.Services;
using UrbanaKey.Infrastructure.Hubs;
using UrbanaKey.Infrastructure.Persistence.Interceptors;
using UrbanaKey.Core.Features.Assemblies; // For CastVoteHandler MediatR registration
using UrbanaKey.Api.Validators; 
using MediatR;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UrbanaKey API",
        Version = "v1",
        Description = "Backend API for UrbanaKey Residential Management System"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
builder.Services.AddOpenApi();

// Persistence
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? "Server=localhost;Database=UrbanaKey;User=root;Password=example;";

builder.Services.AddDbContext<UrbanaKeyDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<UrbanaKeyDbContext>());

// Authorization Policies
builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("ResidentOnly", policy => policy.RequireRole(UserRoles.Resident));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole(UserRoles.Admin, UserRoles.Guard));
});
builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<UrbanaKeyDbContext>();

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<AuditableEntityInterceptor>();
builder.Services.AddSingleton<IVoteChannel, VoteChannel>();
builder.Services.AddHostedService<VoteBackgroundService>();

// Storage
var useLocal = builder.Configuration.GetValue<bool>("Storage:UseLocal");

if (useLocal || builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IFileStorage, LocalStorage>();
}
else
{
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions("Storage")); 
    // Or manual config:
    builder.Services.AddSingleton(typeof(Amazon.S3.IAmazonS3), sp => 
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var s3Config = new Amazon.S3.AmazonS3Config
        {
            ServiceURL = config["Storage:ServiceUrl"]
        };
        var creds = new Amazon.Runtime.BasicAWSCredentials(config["Storage:AccessKey"], config["Storage:SecretKey"]);
        return new Amazon.S3.AmazonS3Client(creds, s3Config);
    });
    builder.Services.AddScoped<IFileStorage, CloudflareR2Storage>();
}
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<OnboardingRequestValidator>();
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<SanctionCleanupService>();
builder.Services.AddScoped<ITemplateService, FileTemplateService>();
builder.Services.AddScoped<IPdfService, QuestPdfService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CastVoteHandler).Assembly));

// SignalR with Redis
var redisConn = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConn ?? "localhost:6379");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UrbanaKey API v1");
        options.RoutePrefix = "swagger"; // swagger is default, but explicit for clarity
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthentication();
app.UseAuthorization(); 

// SignalR Hubs
app.MapHub<AssemblyHub>("/hubs/assembly");
app.MapHub<PanicHub>("/hubs/panic");

// Endpoints
app.MapGet("/", () => "UrbanaKey Backend Running");

app.MapAuthEndpoints();
app.MapUnitEndpoints();
app.MapResidentEndpoints();
app.MapSanctionEndpoints();
app.MapAssemblyEndpoints();
app.MapAmenityEndpoints();
app.MapPqrEndpoints();
app.MapAdminEndpoints();
app.MapReportEndpoints();

app.Run();

public record OnboardingRequest(string Email, string Password, string FullName);
