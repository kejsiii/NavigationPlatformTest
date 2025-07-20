using Application.Interfaces;
using Application.MapperProfile;
using Application.Services;
using Application.Services.Messaging;
using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using Domain.Interfaces;
using DTO.WebApiDTO.Journey;
using DTO.WebApiDTO.User;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Presentation.Middlewares;
using Presentation.ProfileMapper;
using Presentation.Utilities;
using Presentation.Validators;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace Presentation
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            services.AddDistributedMemoryCache();
            services.AddControllers();

            // Configure DBContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Navigation Platform API",
                    Version = "v1",
                    Description = "API documentation for Navigation Platform project"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                              Enter 'Bearer' [space] and then your token in the text input below.
                              \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<UserProfileApi>();
                cfg.AddProfile<JourneyProfile>();
                cfg.AddProfile<JourneyProfileApi>();
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    RoleClaimType = ClaimTypes.Role
                };
            });



            services.AddAuthorization();

            services.AddMvc();
            services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Include,
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'"
            };
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>()); 
            #region Error Handling Configuration

            //services.AddProblemDetails().AddProblemDetailsMappingOptions();
            services.AddProblemDetailsMappingOptions();

            #endregion

            #region Validators

            services.AddScoped<IValidator<RegisterRequestDtoApi>, RegisterRequestDtoApiValidator>();
            services.AddScoped<IValidator<LoginRequestDtoApi>, LoginRequestDtoApiValidator>();
            services.AddScoped<IValidator<JourneyShareRequestDtoApi>, JourneyShareRequestDtoApiValidator>();
            services.AddScoped<IValidator<AddJourneyRequestDtoApi>, AddJourneyRequestDtoApiValidator>();
            services.AddScoped<IValidator<JourneyFilterRequestDtoApi>, JourneyFilterRequestDtoApiValidator>();
            services.AddScoped<IValidator<MonthlyRouteDistanceDtoApi>, MonthlyRouteDistanceDtoApiValidator>();
            #endregion

            #region RabbitMQ 
            services.Configure<RabbitMqOptions>(_configuration.GetSection("RabbitMq"));
            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            services.AddHostedService<DailyGoalWorker>();
            #endregion
            InitializeServices(services);
        }

        private static void InitializeServices(IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserStatusChangeRepository, UserStatusChangeRepository>();
            services.AddScoped<IJourneyShareRepository, JourneyShareRepository>();
            services.AddScoped<IJourneyRepository, JourneyRepository>();
            services.AddScoped<IJourneyPublicLinkRepository, JourneyPublicLinkRepository>();
            services.AddScoped<IDailyGoalBadgeRepository, DailyGoalBadgeRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // Services
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IJWTUtilities, JWTUtilities>();
            services.AddScoped<IJourneyServices, JourneyServices>();
            services.AddScoped<IJwtBlacklistServices, JwtBlacklistService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger(c =>
            {
                c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
                var jwrSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Enter your JWT Access Token",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Navigation Platform API v1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseProblemDetails();
            app.UseRouting();
            app.UseMiddleware<JwtBlacklistMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}