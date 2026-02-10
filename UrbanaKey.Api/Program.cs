using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Api.Middleware;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Features.Assemblies; // For VoteDto, CastVoteCommand
using UrbanaKey.Core.Interfaces;
using UrbanaKey.Infrastructure.Persistence;
using UrbanaKey.Infrastructure.Services;
using UrbanaKey.Infrastructure.Hubs;
using UrbanaKey.Infrastructure.Persistence.Interceptors;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Persistence
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? "Server=localhost;Database=UrbanaKey;User=root;Password=password;";

builder.Services.AddDbContextPool<UrbanaKeyDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Identity
builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<UrbanaKeyDbContext>();

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<AuditableEntityInterceptor>();
builder.Services.AddSingleton<IVoteChannel, VoteChannel>();
builder.Services.AddHostedService<VoteBackgroundService>();

// Storage
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions("Storage")); // Maps to ServiceUrl etc if standard format
// Or manual config:
builder.Services.AddSingleton<Amazon.S3.IAmazonS3>(sp => 
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
}

app.UseHttpsRedirection();

// Tenant Middleware
app.UseMiddleware<TenantResolverMiddleware>();

app.UseAuthentication();
app.UseAuthorization(); 

// SignalR Hubs
app.MapHub<PanicHub>("/hubs/panic");

// Endpoints
app.MapGet("/", () => "UrbanaKey Backend Running");

// Auth Group
app.MapGroup("/api/auth")
    .MapIdentityApi<User>()
    .WithTags("Auth");

// Custom Auth Endpoint Example
app.MapPost("/api/auth/onboarding", async (UserManager<User> userManager, ITenantProvider tenantProvider, OnboardingRequest request) => 
{
    var user = new User 
    { 
        UserName = request.Email, 
        Email = request.Email, 
        FullName = request.FullName,
        TenantId = tenantProvider.GetTenantId(),
        IsActive = false 
    };
    
    var result = await userManager.CreateAsync(user, request.Password);
    
    if (result.Succeeded)
        return Results.Ok(new { Message = "Request submitted for approval" });
        
    return Results.BadRequest(result.Errors);
})
.WithTags("Auth");

// Units Group
app.MapGroup("/api/units")
    .RequireAuthorization()
    .WithTags("Units")
    .MapGet("/", async (UrbanaKeyDbContext db) => await db.Units.ToListAsync());

// Assemblies Group
app.MapGroup("/api/assemblies")
    .RequireAuthorization()
    .WithTags("Assemblies")
    .MapPost("/vote", async (IMediator mediator, VoteDto voteDto) => 
    {
        await mediator.Send(new CastVoteCommand(voteDto));
        return Results.Accepted();
    });

// PQR Group
app.MapGroup("/api/pqr")
    .RequireAuthorization()
    .WithTags("PQR")
    .MapGet("/", () => Results.Ok("PQR List"));

app.Run();

public record OnboardingRequest(string Email, string Password, string FullName);
