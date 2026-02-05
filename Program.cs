using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SGCP.Context;
using SGCP.Handlers;
using SGCP.Handlers;
using SGCP.Helper;
using SGCP.IService;
using SGCP.IServices;
using SGCP.Service;
using SGCP.Services;
using SGCP.Services.Notifications;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var servers = new List<string>
{
    "http://localhost:5001",
    "http://localhost:5002",
    "http://localhost:5003"
};


builder.Services.AddSingleton<IRoundRobinDispatcherService>(new RoundRobinDispatcherService(servers));
builder.Services.AddTransient<RoundRobinHandler>();

// 4. ????? ??? HttpClient ?????? ?????? ??? Handler ????
builder.Services.AddHttpClient("RoundRobinClient")
    .AddHttpMessageHandler<RoundRobinHandler>();

//var firebaseCredentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "D:\\SGCP\\SGCP\\SGCP\\wwwroot\\sgcp-6564a-firebase-adminsdk-fbsvc-ca4801bfd8json");
//FirebaseApp.Create(new AppOptions()
//{
//    Credential = GoogleCredential.FromFile(firebaseCredentialsPath),
//});
builder.Services.AddControllers();  
builder.Services.AddTransient<Seed>();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGovernmentService, GovernmentService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IComplaintTypeService, ComplaintTypeService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IComplaintHistoryService, ComplaintHistoryService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditAspectService, AuditAspectService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IComplaintHistoryReportService, ComplaintHistoryReportService>();


builder.Services.Configure<VerifywayOptions>(builder.Configuration.GetSection("Verifyway"));

builder.Services.AddHttpClient<IVerifywayService, VerifywayService>((sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<VerifywayOptions>>().Value;
  http.BaseAddress = new Uri(opt.BaseUrl);
  http.DefaultRequestHeaders.Accept.Clear();
  http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
  http.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", opt.ApiKey);
});

builder.Services.AddMemoryCache();


builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });


builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SGCP",
        Version = "v0.1",
        Description = "Full-featured Swagger API documentation",
        Contact = new OpenApiContact
        {
            Name = "Suliman Armoush",
            Email = "suliman221232@gmail.com"
        }
    });

    options.AddSecurityDefinition("Token", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: `{token}`"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme
            {
              Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Token"
              }
            },
            Array.Empty<string>()
          }
        });
});

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll",
      builder =>
      {
        builder
              .AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
      });
});


FirebaseApp.Create(new AppOptions()
{
  Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sgcp-2026-firebase-adminsdk-fbsvc-b754bd79d2.json")),
});

var app = builder.Build();
QuestPDF.Settings.License = LicenseType.Community;

app.UseHttpsRedirection();
app.UseStaticFiles();

if (args.Length == 1 && args[0].ToLower() == "seeddata")
    SeedData(app);

void SeedData(IHost app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

    using (var scope = scopedFactory.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<Seed>();
        service.SeedDataContext();
    }
}






    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger";
    });


app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
