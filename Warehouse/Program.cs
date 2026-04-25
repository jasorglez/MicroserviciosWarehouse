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
    x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

   c.SwaggerDoc("v5.32", new OpenApiInfo { Title = "Microservicio Warehouse", Version = "v5.32 Molienda + DetailsMolienda CRUD 2026-04-24" });
  
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

builder.Services.AddScoped<IItemCommentsService, ItemCommentsService>();
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
builder.Services.AddScoped<IPrefixSetupService, PrefixSetupService>();

// MateriaByCatalog service registration
builder.Services.AddScoped<IMateriaByCatalogService, MateriaByCatalogService>();
builder.Services.AddScoped<ISucursalByMaterialProveedorService, SucursalByMaterialProveedorService>();


builder.Services.AddScoped<IPricesXProductsPresentationService, PricesXProductsPresentationService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IMaterialXModuloService, MaterialXModuloService>();
builder.Services.AddScoped<IAutorizacionMontoService, AutorizacionMontoService>();
builder.Services.AddScoped<IMoliendaService, MoliendaService>();
builder.Services.AddScoped<IDetailsMoliendaService, DetailsMoliendaService>();

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
        c.SwaggerEndpoint("/swagger/v5.31/swagger.json", "Microservicio Warehouse V5.31");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowWarehouseOrigin");

//app.UseAuthorization();
app.UseAuthorization();

// ── Migración automática: typeoc / datepostpone en detailsreqoc ───────────────
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DbWarehouseContext>();
    await db.Database.ExecuteSqlRawAsync(@"
        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                       WHERE TABLE_NAME='detailsreqoc' AND COLUMN_NAME='typeoc')
            ALTER TABLE detailsreqoc ADD typeoc VARCHAR(50) NULL;

        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                       WHERE TABLE_NAME='detailsreqoc' AND COLUMN_NAME='datepostpone')
            ALTER TABLE detailsreqoc ADD datepostpone DATE NULL;

    ");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Auto-migration detailsreqoc falló — corre el ALTER TABLE manualmente.");
}

app.MapControllers();

// Middleware de Prometheus para exponer métricas
app.UseHttpMetrics(); // Esto mide las solicitudes HTTP automáticamente

app.MapMetrics("/metricsWarehouse"); // Esto habilita el endpoint /metrics

app.Run();
