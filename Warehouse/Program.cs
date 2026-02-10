using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Warehouse;
using Warehouse.Service;
using Warehouse.Service.Delison;
//using Warehouse.Service.Whatsapp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWarehouseOrigin",
        builder =>
        {
            builder

                .WithOrigins("https://localhost", "http://localhost:4200", 
                             "http://localhost:5173",
                             "https://be-app-five.vercel.app", "http://localhost:8100",
                             "https://localhost:7089", "https://biapp.com.mx", 
                             "https://localhost:4200", "https://www.bi2.com.mx",
                             "https://smp-git-main-jasorglezs-projects.vercel.app",
                             "https://smp-beta.vercel.app",
                             "https://bi2.com.mx",
                             "https://pruebas.bi2.mx",
                             "https://clinica-pruebas.bi2.mx",
                             "https://www.hco-siaf.com") 
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Añadir HttpClient para Twilio
//builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x => 
{
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

   c.SwaggerDoc("v5.10", new OpenApiInfo { Title = "Microservicio Warehouse", Version = "v5.10 Mod. 2025-02-09 18:15, BSK, Server 76.13.28.145 12:24" }); 
  
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
builder.Services.AddScoped<IMaterialxFinalProductService, MaterialxFinalProductService>();
builder.Services.AddScoped<IFinalProductService, FinalProductService>();
builder.Services.AddScoped<IProviderTypeService, ProviderTypeService>();

builder.Services.AddScoped<IOcandreqService, OcandreqService>();
builder.Services.AddScoped<IInandoutService, InandoutService>();
builder.Services.AddScoped<IDetailsreqocService, DetailsreqocService>();
builder.Services.AddScoped<IDetailsinandoutService, DetailsinandoutService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();  
builder.Services.AddScoped<ISetupService, SetupService>();  
builder.Services.AddScoped<ITablesXModulesService, TablesXModulesService>();
builder.Services.AddScoped<IParameterByMaterialDescriptionService, ParameterByMaterialDescriptionService>();
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
builder.Services.AddScoped<IRawMaterialDetailsService, RawMaterialDetailsService>();
builder.Services.AddScoped<IProveedorXTablaService, ProveedorXTablaService>();
builder.Services.AddScoped<IFamilySubFamilyDelisonService, FamilySubFamilyDelisonService>();
builder.Services.AddScoped<IMaterialDelisonService, MaterialDelisonService>();
builder.Services.AddScoped<ISubfamilyxProviderService, SubfamilyxProviderService>();
builder.Services.AddScoped<ITypexPrefixesService, TypexPrefixesService>();

// MateriaByCatalog service registration
builder.Services.AddScoped<IMateriaByCatalogService, MateriaByCatalogService>();
builder.Services.AddScoped<ISucursalByMaterialProveedorService, SucursalByMaterialProveedorService>();


builder.Services.AddScoped<IPricesXProductsPresentationService, PricesXProductsPresentationService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();

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
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clave JWT no esta configurada"))),
            // AGREGAR ESTAS LÍNEAS:
            ClockSkew = TimeSpan.FromMinutes(5), // Tolerancia de 5 minutos
            RequireExpirationTime = true
        };

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v5.10/swagger.json", "Microservicio Warehouse V5.10");
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
