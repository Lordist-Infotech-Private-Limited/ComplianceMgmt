using BoldReports.Web;
using ComplianceMgmt.Api.Exceptions;
using ComplianceMgmt.Api.Filters;
using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using ComplianceMgmt.Api.Repository;
using ComplianceMgmt.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

namespace ComplianceMgmt.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add JSON configuration file
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog(); // Replace default logger with Serilog

            Bold.Licensing.BoldLicenseProvider.RegisterLicense("hqtVyred0+U80CCsByBoE8h7o10O167TD7JGPrspwwk=");
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", policy =>
                {
                    policy.WithOrigins("https://compliancemgmt.lordist.in") // Allow only your React app
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithMethods("GET", "POST", "OPTIONS");
                });
            });

            builder.Services.AddOpenApi();

            // Add services to the container.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
                // Configure JSON serializer to avoid metadata
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;

            });

            // Add other services
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IRecordCountRepository, RecordCountRepository>();
            builder.Services.AddScoped<IServerDetailRepository, ServerDetailRepository>();
            builder.Services.AddScoped<IBorrowerDetailRepository, BorrowerDetailRepository>();
            builder.Services.AddScoped<IBorrowerLoanRepository, BorrowerLoanRepository>();
            builder.Services.AddScoped<IBorrowerMortgageRepository, BorrowerMortgageRepository>();
            builder.Services.AddScoped<IBorrowerMortgageOtherRepository, BorrowerMortgageOtherRepository>();
            builder.Services.AddScoped<ICoBorrowerDetailsRepository, CoBorrowerDetailsRepository>();
            builder.Services.AddScoped<IStatewiseLoanRepository, StatewiseLoanRepository>();
            builder.Services.AddScoped<IComplianceReportRepository, ComplianceReportRepository>();

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddSingleton<TokenService>();

            // Add config for JWT bearer token
            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Secret"])),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    ValidateLifetime = true, // Ensure token expiration is validated
                    ClockSkew = TimeSpan.Zero // Optional: Set to zero to avoid time skew (default is 5 minutes)
                };
                // Add event handler for token errors
                opt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddProblemDetails();

            // use AddMySqlDataSource to configure MySqlConnector
            builder.Services.AddTransient(x =>
              new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register ComplianceMgmtDbContext for DI
            builder.Services.AddScoped<ComplianceMgmtDbContext>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            // Define Swagger generation options and add Bearer token authentication
            builder.Services.AddSwaggerGen(c =>
            {
                //c.OperationFilter<FileUploadOperation>(); // Add this custom operation filter
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Compliance Mgmt JWT Auth Api",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer jhfdkj.jkdsakjdsa.jkdsajk\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
            });

            builder.Services.AddHttpContextAccessor();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add in-memory cache
            builder.Services.AddMemoryCache();

            ReportConfig.DefaultSettings = new ReportSettings().RegisterExtensions(["BoldReports.Data.MySQL"]);
            //builder.Services.AddHybridCache(); // Not shown: optional configuration API.

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new ApiExceptionFilter());
            });

            // Register the background service
            builder.Services.AddHostedService<MySqlKeepAliveService>();

            var app = builder.Build();

            //var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // Use a centralized exception handler
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionHandlerFeature?.Error;

                    var statusCode = exception switch
                    {
                        NotFoundException => StatusCodes.Status404NotFound,
                        ValidationException => StatusCodes.Status400BadRequest,
                        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                        MySqlException mysqlEx when mysqlEx.Message.Contains("Unable to convert MySQL date/time value to System.DateTime")
                                                => StatusCodes.Status400BadRequest,
                        DatabaseException => StatusCodes.Status500InternalServerError,
                        _ => StatusCodes.Status500InternalServerError
                    };

                    // Log the exception with Serilog
                    Log.Error(exception, "An unhandled exception occurred: {Message}", exception?.Message);

                    var problemDetails = new ProblemDetails
                    {
                        Title = exception switch
                        {
                            NotFoundException => "Resource not found.",
                            ValidationException => "Validation error occurred.",
                            UnauthorizedAccessException => "Unauthorized access.",
                            MySqlException mysqlEx when mysqlEx.Message.Contains("Unable to convert MySQL date/time value to System.DateTime")
                    => "Invalid date/time value in the database.",
                            DatabaseException => "A database error occurred.",
                            _ => "An error occurred while processing your request."
                        },
                        Status = statusCode,
                        Detail = app.Environment.IsDevelopment() ? exception?.ToString() : exception?.Message, // Include detailed information only if needed
                        Instance = context.Request.Path
                    };

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = statusCode;

                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });

            app.UseExceptionHandler();
            app.UseStatusCodePages();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();

            // Use CORS
            app.UseCors("AllowSpecificOrigin");

            // Ensure endpoints support OPTIONS requests
            app.MapMethods("/ReportViewer/PostReportAction", ["OPTIONS"], (HttpContext context) =>
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "https://compliancemgmt.lordist.in");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                context.Response.StatusCode = 204; // No content
                return Task.CompletedTask;
            });

            //app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            try
            {
                app.Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
