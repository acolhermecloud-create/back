
using API.Jobs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.Acquirers;
using Domain.Interfaces.Repository.Bank;
using Domain.Interfaces.Repository.System;
using Domain.Interfaces.Services;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using Repository;
using Repository.Acquirers;
using Repository.Bank;
using Repository.System;
using Service;
using System.Text;
using Util;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            string? envEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string settingFile = string.Empty;

            if (envEnvironment == null)
                settingFile = "appsettings.Development.json";
            else if (envEnvironment == "Development")
                settingFile = "appsettings.Development.json";
            else if (envEnvironment == "PreRelease")
                settingFile = "appsettings.PreRelease.json";
            else if (envEnvironment == "Production")
                settingFile = "appsettings.json";

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile(settingFile, optional: false, reloadOnChange: true);

            #region VARIABLES

            string sentryDsn = builder.Configuration["Sentry:Dsn"];
            string sentryEnvirovment = builder.Configuration["Sentry:Environment"];
            string dbMongoName = builder.Configuration["MongoDB:DbName"];

            #endregion

            SentrySdk.Init(options =>
            {
                // A Sentry Data Source Name (DSN) is required.
                // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
                options.Dsn = sentryDsn;
                options.Environment = sentryEnvirovment;

                // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                // This might be helpful, or might interfere with the normal operation of your application.
                // We enable it here for demonstration purposes when first trying Sentry.
                // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
                options.Debug = sentryEnvirovment == "debug";

                // This option is recommended. It enables Sentry's "Release Health" feature.
                options.AutoSessionTracking = true;

                // Enabling this option is recommended for client applications only. It ensures all threads use the same global scope.
                options.IsGlobalModeEnabled = false;

                // Example sample rate for your transactions: captures 10% of transactions
                options.TracesSampleRate = 0.1;
            });

            builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented; // Opcional: formata��o do JSON
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyResolver();
                options.SerializerSettings.Formatting = Formatting.None;
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        if (envEnvironment == "Development" || envEnvironment == "PreRelease")
                        {
                            builder.WithOrigins(
                                "http://localhost:3000", "http://localhost:3001", "http://localhost:3002",
                                "https://front-a01031611408-350ac0a9fb2c.herokuapp.com"
                            );
                        }
                        else if (envEnvironment == "Production")
                        {
                            builder.WithOrigins("https://front-a01031611408-350ac0a9fb2c.herokuapp.com");
                        }

                        builder.AllowAnyMethod().AllowAnyHeader();
                    });
            });

            #region Authentication Token

            var key = Encoding.ASCII.GetBytes(builder.Configuration["Crypt:Secret"]);

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("user", policy => policy.RequireClaim("Store", "user"));
                options.AddPolicy("admin", policy => policy.RequireClaim("Store", "admin"));
            });

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            #endregion

            #region MongoDb

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(builder.Configuration["MongoDB:Url"]);
                var client = new MongoClient(settings);
                return client;
            });

            #endregion

            builder.Services.AddStackExchangeRedisCache(o =>
            {
                o.Configuration = builder.Configuration["Redis:Server"];
            });

            #region Services

            builder.Services.AddScoped<IMailService, MailService>();
            builder.Services.AddScoped<IMigrationService, MigrationService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ISocialAuthProvider, SocialAuthProvider>();
            builder.Services.AddScoped<IUtilityService, UtilityService>();
            builder.Services.AddScoped<IS3Service, S3Service>();
            builder.Services.AddScoped<ICampaignService, CampaignService>();
            builder.Services.AddScoped<INotificationsService, NotificationsService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<ILeverageRequestService, LeverageRequestService>();
            builder.Services.AddScoped<IStoreService, StoreService>();
            builder.Services.AddScoped<ICacheService, CacheService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IFakeDataForCampaignService, FakeDataForCampaignService>();
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<ITransfeeraService, TransfeeraService>();
            builder.Services.AddScoped<ISystemService, SystemService>();
            builder.Services.AddScoped<IUtmfyService, UtmfyService>();

            builder.Services.AddScoped<IBankService, BankService>();
            builder.Services.AddHttpClient<IVenitService, VenitService>();
            #endregion

            #region Repositorys

            builder.Services.AddSingleton<IUserRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserGroupAccessRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserGroupAccessRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IGroupAccessRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new GroupAccessRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICategoryRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new CategoryRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IAddressRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new AddressRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICampaignRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new CampaignRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICampaignDonationRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new DonationRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICampaignLogsRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new CampaignLogsRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICampaignReportRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new CampaignReportRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IComplaintRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new ComplaintRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IOngRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new OngRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserPointsRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserPointsRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserNotificationsTokenRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserNotificationsTokensRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserNotificationsRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserNotificationsRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IPlanRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new PlanRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserPaymentAccountRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserPaymentAccountRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ILeverageRequestRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new LeverageRequestRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICampaignTransactionsRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new CampaignTransactionsRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IGatewayConfigurationRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new GatewayConfigurationRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IDigitalStickerRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new DigitalStickerRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserDigitalStickersRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserDigitalStickersRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserDigitalStickersUsageRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserDigitalStickersUsageRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICampaignCommentsRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new CampaignCommentsRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ICodeChallengeRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new CodeChallengeRepository(mongoClient, dbMongoName); });
            
            builder.Services.AddSingleton<IBankAccountRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new BankAccountRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IBankTransactionRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new BankTransactionRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IBaasConfigurationRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new BaasConfigurationRepository(mongoClient, dbMongoName); });
            
            builder.Services.AddSingleton<IReflowPayRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new ReflowPayRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IReflowPayV2Repository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new ReflowPayV2Repository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IBlooBankRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new BlooBankRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<ITransfeeraRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new TransfeeraRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUtmRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UtmRepository(mongoClient, dbMongoName); });

            #endregion

            #region BACKGOUND TASKS

            builder.Services.AddHostedService<MigrationJob>();
            builder.Services.AddHostedService<BankJob>();
            builder.Services.AddHostedService<CampaignJob>();

            // HINGFIRE
            builder.Services.AddHangfire(config =>
            {
                var mongoUrlBuilder = new MongoUrlBuilder(builder.Configuration["MongoDB:Url"]);
                var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());
                config.UseMongoStorage(mongoClient, dbMongoName, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy(),
                    },
                    CheckConnection = true,
                    Prefix = "FIRE_"
                });
            });

            builder.Services.Configure<BackgroundJobServerOptions>(options =>
            {
                options.WorkerCount = 20;
                options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
                options.ServerCheckInterval = TimeSpan.FromSeconds(5);
            });

            builder.Services.AddHangfireServer();

            #endregion

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthorization();

            app.UseHangfireDashboard("/trigger");

            app.MapControllers();

            app.Run();
        }
    }
}
