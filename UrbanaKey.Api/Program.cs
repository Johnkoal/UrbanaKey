using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Api.Middleware;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Features.Units;
using UrbanaKey.Core.Features.Residents;
using UrbanaKey.Core.Features.Sanctions;
using UrbanaKey.Core.Features.Bookings;
using UrbanaKey.Core.Features.Amenities;
using UrbanaKey.Core.Features.Assemblies; // For VoteDto, CastVoteCommand
using UrbanaKey.Core.Features.Finance;
using UrbanaKey.Core.Interfaces;
using UrbanaKey.Infrastructure.Persistence;
using UrbanaKey.Infrastructure.Services;
using UrbanaKey.Infrastructure.Hubs;
using UrbanaKey.Infrastructure.Persistence.Interceptors;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc; 
using UrbanaKey.Core.Features.PQRS;
using UrbanaKey.Core.Features.Admin;
using UrbanaKey.Core.Common;
using FluentValidation;
using UrbanaKey.Api.Validators; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Persistence
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? "Server=localhost;Database=UrbanaKey;User=root;Password=password;";

builder.Services.AddDbContextPool<UrbanaKeyDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<UrbanaKeyDbContext>());

// Identity
// Identity
builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("ResidentOnly", policy => policy.RequireRole(UserRoles.Resident));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole(UserRoles.Admin, UserRoles.Guard));
});
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

// Assembly Report Endpoint
app.MapGet("/api/assemblies/{id}/report", async (Guid id, IMediator mediator) => 
{
    var pdf = await mediator.Send(new GetAssemblyPdfQuery(id));
    return Results.File(pdf, "application/pdf", $"Acta_Asamblea_{id}.pdf");
})
.RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin));

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
.WithTags("Auth")
.AddEndpointFilter(async (invocationContext, next) =>
{
    var request = invocationContext.GetArgument<OnboardingRequest>(2);
    var validator = invocationContext.HttpContext.RequestServices.GetRequiredService<OnboardingRequestValidator>();
    var validationResult = await validator.ValidateAsync(request);
    
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
    return await next(invocationContext);
});

// Units Group
var unitsGroup = app.MapGroup("/api/units")
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
    .WithTags("Units");

unitsGroup.MapGet("/", async (IMediator mediator) => 
    Results.Ok(await mediator.Send(new GetUnitsQuery())));

unitsGroup.MapPost("/", async (IMediator mediator, CreateUnitRequest request) => 
{
    var id = await mediator.Send(new CreateUnitCommand(request));
    return Results.Created($"/api/units/{id}", new { Id = id });
});

unitsGroup.MapPut("/{id}", async (Guid id, UpdateUnitRequest request, IMediator mediator) => 
{
    var result = await mediator.Send(new UpdateUnitCommand(id, request));
    return result ? Results.NoContent() : Results.NotFound();
});

unitsGroup.MapDelete("/{id}", async (Guid id, IMediator mediator) => 
{
    var result = await mediator.Send(new DeleteUnitCommand(id));
    return result ? Results.NoContent() : Results.BadRequest("No se puede eliminar la unidad (verifique si hay residentes vinculados).");
});

unitsGroup.MapPost("/import", async (IMediator mediator, IFormFile file) => 
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("File is empty.");

    using var stream = file.OpenReadStream();
    var count = await mediator.Send(new ImportUnitsCommand(stream));
    return Results.Ok(new { ImportedCount = count });
}).DisableAntiforgery();

// Resident Management Group
var residentsGroup = app.MapGroup("/api/admin/residents")
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
    .WithTags("Residents");

residentsGroup.MapPost("/link", async (IMediator mediator, LinkResidentRequest request) => 
    {
        var id = await mediator.Send(new LinkResidentCommand(request));
        return Results.Ok(new { ProfileId = id });
    });

residentsGroup.MapDelete("/{profileId}", async (Guid profileId, IMediator mediator) => 
    {
        var result = await mediator.Send(new UnlinkResidentCommand(profileId));
        return result ? Results.NoContent() : Results.NotFound();
    });

// Sanctions Group
app.MapGroup("/api/admin/sanctions")
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
    .WithTags("Sanctions")
    .MapPost("/", async (IMediator mediator, CreateSanctionRequest request) => 
    {
        var id = await mediator.Send(new CreateSanctionCommand(request));
        return Results.Created($"/api/sanctions/{id}", new { Id = id });
    });


// Assemblies Group
app.MapGroup("/api/assemblies")
    .RequireAuthorization()
    .WithTags("Assemblies")
    .MapPost("/vote", async (IMediator mediator, VoteDto voteDto) => 
    {
        await mediator.Send(new CastVoteCommand(voteDto));
        return Results.Accepted();
    });

var amenitiesGroup = app.MapGroup("/api/amenities")
    .RequireAuthorization()
    .WithTags("Amenities");

amenitiesGroup.MapPost("/", async (CreateCommonAreaRequest request, IMediator mediator) => 
    {
        var id = await mediator.Send(new CreateCommonAreaCommand(request));
        return Results.Created($"/api/amenities/{id}", new { Id = id });
    });

amenitiesGroup.MapPut("/{id}", async (Guid id, CreateCommonAreaRequest request, bool isActive, IMediator mediator) => 
    {
        var result = await mediator.Send(new UpdateCommonAreaCommand(id, request, isActive));
        return result ? Results.NoContent() : Results.NotFound();
    })
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin));

// Endpoint de Cancelación
app.MapDelete("/api/amenities/bookings/{id}", async (Guid id, IMediator mediator, ClaimsPrincipal user) => 
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var isAdmin = user.IsInRole(UserRoles.Admin);
    
    var result = await mediator.Send(new CancelBookingCommand(id, userId, isAdmin));
    return result ? Results.NoContent() : Results.BadRequest("No se pudo cancelar la reserva.");
});

// Admin Group
app.MapGroup("/api/admin")
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
    .WithTags("Admin")
    .MapPost("/users/{id}/approve", async (Guid id, IMediator mediator) => 
    {
        var success = await mediator.Send(new ApproveUserCommand(id));
        return success ? Results.Ok() : Results.BadRequest("No se pudo aprobar el usuario.");
    });

// Listado de usuarios pendientes
app.MapGroup("/api/admin/users")
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
    .WithTags("Admin Users")
    .MapGet("/pending", async (UrbanaKeyDbContext db, ITenantProvider tenantProvider) => 
    {
        var tenantId = tenantProvider.GetTenantId();
        return await db.Users
            .Where(u => u.TenantId == tenantId && !u.IsActive)
            .ToListAsync();
    });

// Admin Reports Group
app.MapGroup("/api/admin/reports")
    .RequireAuthorization("AdminOnly")
    .WithTags("Admin Reports")
    .MapGet("/audit-logs", async (IMediator mediator) => 
        Results.Ok(await mediator.Send(new GetAuditLogsQuery())));

// Finance Group
app.MapGroup("/api/admin/finance")
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
    .WithTags("Finance")
    .MapPost("/generate-invoices", async (decimal standardFee, IMediator mediator) => 
    {
        var count = await mediator.Send(new GenerateMonthlyInvoicesCommand(standardFee));
        return Results.Ok(new { InvoicesGenerated = count });
    });

// PQR Group
var pqrGroup = app.MapGroup("/api/pqr")
    .RequireAuthorization()
    .WithTags("PQR");

pqrGroup.MapGet("/", async (IMediator mediator, ClaimsPrincipal user) => 
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var results = await mediator.Send(new GetMyPqrsQuery(userId));
    return Results.Ok(results);
});

pqrGroup.MapGet("/public", async (IMediator mediator) => 
{
    return Results.Ok(await mediator.Send(new GetPublicPqrsQuery()));
});

pqrGroup.MapPost("/", async (
    IMediator mediator, 
    ClaimsPrincipal user, 
    HttpRequest request
    ) => 
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var form = await request.ReadFormAsync();
    
    var attachments = form.Files.Select(f => f.OpenReadStream()).ToList();
    
    var pqrRequest = new CreatePqrRequest(
        form["title"]!, 
        form["description"]!, 
        Guid.Parse(form["unitId"]!), 
        bool.Parse(form["isPublic"]!),
        null, // attachmentUrl
        attachments);

    var id = await mediator.Send(new CreatePqrCommand(pqrRequest, userId));
    return Results.Created($"/api/pqr/{id}", new { Id = id });
}).DisableAntiforgery(); // For file upload handling

// Nuevo endpoint para comentarios
pqrGroup.MapPost("/{id}/comments", async (Guid id, AddPqrCommentRequest request, IMediator mediator, ClaimsPrincipal user) => 
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var commentId = await mediator.Send(new AddPqrCommentCommand(request with { PqrId = id }, userId));
    return Results.Ok(new { Id = commentId });
});

// Admin PQR Group
var adminPqrGroup = app.MapGroup("/api/admin/pqr")
    .RequireAuthorization(UserRoles.Admin)
    .WithTags("Admin PQR");

adminPqrGroup.MapGet("/", async (IMediator mediator) => 
{
    return Results.Ok(await mediator.Send(new GetAllPqrsQuery()));
});

adminPqrGroup.MapPut("/{id}/status", async (IMediator mediator, Guid id, [FromBody] string status) => 
{
    var success = await mediator.Send(new UpdatePqrStatusCommand(id, status));
    return success ? Results.NoContent() : Results.NotFound();
});

app.Run();

public record OnboardingRequest(string Email, string Password, string FullName);
