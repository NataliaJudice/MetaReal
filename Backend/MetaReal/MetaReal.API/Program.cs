using System.Text;


using MetaReal.API;

using MetaReal.API.Hubs;
using MetaReal.Application;
using MetaReal.Application.Interfaces;
using MetaReal.Infra.Data.Data;

using MetaReal.Infra.Data.Data.Seed;
using MetaReal.Infra.Ioc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureIoC(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MetaRealDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddHttpContextAccessor();


builder.Services.AddSignalR();
builder.Services.AddScoped<INotificador, SignalRNotificador>();

var jwtSettings = builder.Configuration.GetSection(ConfiguracaoJwt.SecaoConfig).Get<ConfiguracaoJwt>() ?? new ConfiguracaoJwt();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        //  sem esse trecho o hub sempre dava 401
        // mesmo com o token certo. só vale pras rotas /hubs, o resto continua no header normal.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// libera só a porta do vite. AllowCredentials é obrigatório senão o cookie do refresh não vai
var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());

});


builder.Services.AddControllers();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community; // sem isso ele estoura em runtime na hora de gerar o pdf

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MetaReal.API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Informe: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"

    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()

        }
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.Initialize(services);
    }

    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar as migrations/popular o banco de dados (Seed): {ex.Message}");

    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("Frontend");


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificacoesHub>("/hubs/notificacoes");

app.Run();
