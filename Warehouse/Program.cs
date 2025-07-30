using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Text;
using System.Text.Json.Serialization;
using Warehouse;
using Warehouse.Service;
//using Warehouse.Service.Whatsapp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWarehouseOrigin",
        builder =>
        {
            builder

                .WithOrigins("http://localhost:4200", "https://be-app-five.vercel.app", "http://localhost:8100",
                             "https://localhost:7089", "https://biapp.com.mx", "https://localhost:4200", "https://www.bi2.com.mx", "https://bi2.com.mx") // Reemplaza esto con el origen de tu aplicacin Angular
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Añadir HttpClient para Twilio
//builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
 /*     c.SwaggerDoc("v1.99", new OpenApiInfo { Title = "Microservicio Warehouse", Version = "v1.99 Mod. 14-11-24 11:12 20.221.74.88" });
 */

    c.SwaggerDoc("v2.26", new OpenApiInfo { Title = "Microservicio Warehouse With CORS", Version = "v2.26 Mod. 2025-07-30 17:30, BSKG, Server 66.179.240.10" }); 

  
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Ejemplo: \"Authorization: Bearer {t" +
        "oken}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<DbWarehouseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbConWarehouse")));

//WareHouse BD and Services
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();

builder.Services.AddScoped<IOcandreqService, OcandreqService>();
builder.Services.AddScoped<IInandoutService, InandoutService>();
builder.Services.AddScoped<IDetailsreqocService, DetailsreqocService>();
builder.Services.AddScoped<IDetailsinandoutService, DetailsinandoutService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();  
builder.Services.AddScoped<ISetupService, SetupService>();  
builder.Services.AddScoped<ITablesXModulesService, TablesXModulesService>();
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
builder.Services.AddScoped<IRawMaterialDetailsService, RawMaterialDetailsService>();

builder.Services.AddScoped<IPricesXProductsPresentationService, PricesXProductsPresentationService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clave JWT no est� configurada")))
        };

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v2.26/swagger.json", "Microservicio Warehouse V2.26");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();


app.UseCors("AllowWarehouseOrigin");

//app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

// Middleware de Prometheus para exponer métricas
app.UseHttpMetrics(); // Esto mide las solicitudes HTTP automáticamente

app.MapMetrics("/metricsWarehouse"); // Esto habilita el endpoint /metrics

app.Run();
