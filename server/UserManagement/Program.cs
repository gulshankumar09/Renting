using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Shared.Common.Extensions;
using UserManagement.Data;
using UserManagement.Models.Entities;
using UserManagement.Services;
using UserManagement.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
});

// Configure file upload options
builder.Services.Configure<FormOptions>(options =>
{
    // Set the limit to max file size
    options.MultipartBodyLengthLimit = builder.Configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 10 * 1024 * 1024);
});

// Add DbContext
builder.Services.AddDbContext<UserManagementDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(60);
        });
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<UserManagementDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = builder.Configuration.GetValue<bool>("Jwt:ValidateIssuer"),
        ValidateAudience = builder.Configuration.GetValue<bool>("Jwt:ValidateAudience"),
        ValidateLifetime = builder.Configuration.GetValue<bool>("Jwt:ValidateLifetime"),
        ValidateIssuerSigningKey = builder.Configuration.GetValue<bool>("Jwt:ValidateIssuerSigningKey"),
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))),
        ClockSkew = TimeSpan.Zero
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddUserManagementServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Create document storage directory if it doesn't exist
var documentsPath = builder.Configuration["FileStorage:BasePath"] ?? "UserFiles";
if (!Directory.Exists(documentsPath))
{
    Directory.CreateDirectory(documentsPath);
}

// Initialize database
await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
