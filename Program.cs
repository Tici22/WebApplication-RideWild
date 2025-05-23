using Adventure19.Models;
using Microsoft.EntityFrameworkCore;
using Adventure19.Controllers;
using Adventure19.AuthModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Adventure19.Services;
using Adventure19.Util;
using Microsoft.AspNetCore.Diagnostics;

namespace Adventure19
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 1. Configurazione Serilog da appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            //Log.Logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(configuration)
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .CreateLogger();

            // 2. Inizializza builder
            var builder = WebApplication.CreateBuilder(args);

            // 3. Usa Serilog come logger
            //builder.Host.UseSerilog();

            // 4. CORS
            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("CORSPolicy",
                   builder => builder
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials()
                   .SetIsOriginAllowed((host) => true));
            });

            // 5. Controller e JSON
            builder.Services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

            // 6. Connessioni DB
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDbConnection")));

            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

            // 7. JWT e servizi
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            builder.Services.AddAuthorization();

            // 🔹 8. Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddOpenApi();

            // 9. EmailService
            builder.Services.AddSingleton<EmailService>();

            // 10. Costruzione app
            var app = builder.Build();


            //app.UseSerilogRequestLogging();
            if (app.Environment.IsDevelopment())
                app.UseExceptionHandler("/error");
            else
                app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            //IMPORTANTE !!!! LASCIARE QUESTO PEZZO COMMENTATO PERCHÉ ALTRIMENTI INTERFERISCE CON LA CETRALIZZAZIONE LATO BACK END
            // 11. Middleware per loggare errori non gestiti globalmente

            //app.UseExceptionHandler(errorApp =>
            //{
            //    errorApp.Run(async context =>
            //    {
            //        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            //        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            //        logger.LogError(exceptionHandlerPathFeature?.Error, "Errore non gestito");

            //        context.Response.StatusCode = 500;
            //        await context.Response.WriteAsync("Errore interno del server");
            //    });
            //});

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CORSPolicy");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // 12. Avvio app
            app.Run();

            // 13. Chiude e flush dei log (buona prassi)
            //Log.CloseAndFlush();
        }
    }
}