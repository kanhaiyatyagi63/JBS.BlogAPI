using AutoMapper;
using JBS.BlogAPI.Filter;
using JBS.DataLayer;
using JBS.DataLayer.Abstracts;
using JBS.Service;
using JBS.Service.Abstracts;
using JBS.Service.AutoMapperProfile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// serilog configuration
builder.Host
       .UseSerilog((hostingContext, loggerConfiguration) =>
                         loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;
services.AddControllers();

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DataConnection")));
ConfigureIdentity(builder.Services);
services.AddScoped<IUserContextService, UserContextService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
ConfigureJwtAuthentication(services, configuration);

services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

services.AddAuthorization();
services.AddHttpContextAccessor();
//mapper
services.AddAutoMapper(typeof(AutoMappingProfile));

//configure repositories services
services.ConfigureRepositoriesServices();
services.ConfigureServices();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JBS-Blog API", Version = "v1" });

    c.OperationFilter<AddAuthHeaderOperationFilter>();
    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Description = "`Token only!!!` - without `Bearer_` prefix",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Scheme = "bearer"
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

RunPendingMigration(app);

InitializeDatabase(app);

app.Run();



static void ConfigureIdentity(IServiceCollection services)
{
    services.AddIdentity<AppUser, AppRole>()
        .AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders();

    //configure identity options
    services.Configure<IdentityOptions>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    });
}

static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration Configuration)
{
    services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
            ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetValue<string>("Jwt:Secret"))),
            ValidateIssuer = true,
            ValidateAudience = false
        };
    });
}
static void RunPendingMigration(IApplicationBuilder app)
{
    try
    {
        using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        {
            var services = scope.ServiceProvider;

            var dbContext = services.GetRequiredService<DataContext>();
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                // Migrate: Migration Pending executing migration.
                dbContext.Database.Migrate();
            }
        }
    }
    catch (Exception)
    {
    }
}
static void InitializeDatabase(IApplicationBuilder app)
{
    // logger.LogInformation("Inside InitializeDatabase");
    try
    {
        using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        {
            var services = scope.ServiceProvider;

            var seedService = services.GetRequiredService<ISeedService>();

            seedService.SeedAsync().Wait();

        }
    }
    catch (Exception)
    {
        //logger.LogError(exception, "Critical: Database Migration and Seed failed.");
    }

    //logger.LogInformation("Exiting InitializeDatabase");
}