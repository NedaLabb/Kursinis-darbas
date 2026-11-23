using GymOccupancy.Data;
using Microsoft.EntityFrameworkCore;
using GymOccupancy.Models;
using GymOccupancy.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("GymDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Nerasta GymDb prisijungimo eilutė.");
}

builder.Services.AddDbContext<GymDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<OccupancyReader>();
builder.Services.AddScoped<EventProcessor>();

builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", options =>{
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHostedService<RabbitMqReceiver>();
builder.Services.AddScoped<OccupancyAnalytics>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GymDbContext>();

    await db.Database.MigrateAsync();
    await TestAccounts.CreateAsync(db);

    if (!await db.Clubs.AnyAsync())
    {
        db.Clubs.AddRange(
            new Club
            {
                ClubId = "K001",
                CompanyId = "I001",
                Name = "Centro klubas"
            },
            new Club
            {
                ClubId = "K002",
                CompanyId = "I001",
                Name = "Parko klubas"
            },
            new Club
            {
                ClubId = "K003",
                CompanyId = "I002",
                Name = "Šiaurės klubas"
            }
        );

        await db.SaveChangesAsync();
    }
}

app.MapGet("/api/health", () => Results.Ok("API veikia."));

app.MapGet("/api/admin/clubs", async (HttpContext context, GymDbContext db) =>{
    if (!context.User.IsInRole("Admin"))
    {
        return Results.StatusCode(403);
    }

    var companyId = context.User.FindFirst("CompanyId")?.Value;

    if (string.IsNullOrEmpty(companyId))
    {
        return Results.StatusCode(403);
    }

    var clubs = await db.Clubs.Where(club => club.CompanyId == companyId).ToListAsync();

    return Results.Ok(clubs);
})
.RequireAuthorization();

app.MapPost("/api/login", async (LoginRequest request, LoginService loginService, HttpContext context) =>{
    var account = await loginService.CheckLoginAsync(request.Username, request.Password);

    if (account is null)
    {
        return Results.Unauthorized();
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
        new Claim(ClaimTypes.Name, account.Username),
        new Claim(ClaimTypes.Role, account.Role)
    };

    if (account.CompanyId is not null)
    {
        claims.Add(new Claim("CompanyId", account.CompanyId));
    }

    var identity = new ClaimsIdentity(claims, "Cookies");
    var user = new ClaimsPrincipal(identity);

    await context.SignInAsync("Cookies", user);

    return Results.Ok(new
    {
        account.Username,
        account.Role,
        account.CompanyId
    });
});

app.MapGet("/api/visitor/clubs", async (HttpContext context, GymDbContext db) =>{
    if (!context.User.IsInRole("Visitor"))
    {
        return Results.StatusCode(403);
    }

    var clubs = await db.Clubs.OrderBy(club => club.Name).Select(club => new{
            club.ClubId,
            club.Name
        })
        .ToListAsync();

    return Results.Ok(clubs);
})
.RequireAuthorization();

app.MapGet("/api/visitor/clubs/{clubId}/occupancy", async (string clubId, HttpContext context, OccupancyReader reader) =>{
    
    if (!context.User.IsInRole("Visitor"))
    {
        return Results.StatusCode(403);
    }

    var count = await reader.GetCurrentCountAsync(clubId);

    if (count is null)
    {
        return Results.NotFound("Klubas nerastas.");
    }

    return Results.Ok(new { clubId, count });
})
.RequireAuthorization();

app.MapGet("/api/visitor/clubs/{clubId}/analytics",
    async (string clubId, DateTimeOffset from, DateTimeOffset to, HttpContext context, OccupancyReader reader, OccupancyAnalytics analytics) =>{
        
        if (!context.User.IsInRole("Visitor"))
        {
            return Results.StatusCode(403);
        }

        if (from >= to)
        {
            return Results.BadRequest("Laikotarpio pradžia turi būti ankstesnė už pabaigą.");
        }

        var count = await reader.GetCurrentCountAsync(clubId);

        if (count == null)
        {
            return Results.NotFound("Klubas nerastas.");
        }

        var history = await reader.GetHistoryAsync(clubId);
        var hours = analytics.CalculateWeeklyAverages(history, from, to);

        WeeklyOccupancy? quietestHour = null;

        foreach (var hour in hours)
        {
            if (quietestHour == null ||
                hour.AveragePeople < quietestHour.AveragePeople)
            {
                quietestHour = hour;
            }
        }

        return Results.Ok(new
        {
            hours,
            quietestHour
        });
})
.RequireAuthorization();

app.MapGet("/api/admin/clubs/{clubId}/analytics",
    async (string clubId, DateTimeOffset from, DateTimeOffset to, HttpContext context, GymDbContext db, OccupancyReader reader, OccupancyAnalytics analytics) =>{
        
        if (!context.User.IsInRole("Admin"))
        {
            return Results.StatusCode(403);
        }

        var companyId = context.User.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(companyId))
        {
            return Results.StatusCode(403);
        }

        var clubExists = await db.Clubs.AnyAsync(club => club.ClubId == clubId && club.CompanyId == companyId);

        if (!clubExists)
        {
            return Results.NotFound("Klubas nerastas arba nepriklauso jūsų įmonei.");
        }

        if (from >= to)
        {
            return Results.BadRequest("Laikotarpio pradžia turi būti ankstesnė už pabaigą.");
        }

        var history = await reader.GetHistoryAsync(clubId);
        var average = analytics.CalculateAverage(history, from, to);

        return Results.Ok(new
        {
            average
        });
})
.RequireAuthorization();

app.Run();