
using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using ComplianceMgmt.Api.Repository;
using ComplianceMgmt.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using System.Text;
using System.Text.Json.Serialization;

namespace ComplianceMgmt.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Bold.Licensing.BoldLicenseProvider.RegisterLicense("hqtVyred0+U80CCsByBoE8h7o10O167TD7JGPrspwwk=");
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:3000") // Allow only your React app
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithMethods("GET", "POST", "OPTIONS")
                          .AllowCredentials(); // Use this only if credentials are required
                });
            });

            builder.Services.AddOpenApi();

            var options = new WebApplicationOptions
            {
                WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources") // Set your custom path here
            };

            builder = WebApplication.CreateBuilder(options);  // Use the options here

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

            // Register DbContextFactory only
            builder.Services.AddTransient(x =>
              new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register SamadhaanDapperDbContext for DI
            builder.Services.AddScoped<ComplianceMgmtDbContext>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            // Register services
            builder.Services.AddMemoryCache();

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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();


            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    context.Response.StatusCode = 204; // No Content
                    return;
                }
                await next();
            });


            // app.UseCors(cors => cors
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .SetIsOriginAllowed(origin => true)
            //    .AllowCredentials()
            //);
            // Use CORS
            app.UseCors("AllowReactApp");
            //app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
