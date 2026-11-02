using Amazon;
using Amazon.S3;
using Amazon.SQS;
using System.Text;
using FirebaseAdmin;
using Amazon.Runtime;
using Newtonsoft.Json;
using SaudiPlus.Service;
using SaudiPlus.Entities;
using SaudiPlus.Helper.Aws;
using SaudiPlus.Helper.Auth;
using SaudiPlus.Helper.Email;
using System.Security.Claims;
using SaudiPlus.Helper.Bucket;
using Google.Apis.Auth.OAuth2;
using SaudiPlus.Configurations;
using SaudiPlus.Helper.Application;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Local secret files are excluded from Git, Docker builds, and publish output.
// Environment variables and command-line arguments remain deployment overrides.
builder.Configuration
    .AddJsonFile(".secrets/shared.json", optional: true, reloadOnChange: false)
    .AddJsonFile($".secrets/{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}
builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);

foreach (var key in new[] { "ConnectionStrings:DefaultConnection", "Jwt:Secret" })
{
    if (string.IsNullOrWhiteSpace(builder.Configuration[key]))
        throw new InvalidOperationException($"Required configuration '{key}' is missing. See SECURITY.md.");
}
if (!builder.Environment.IsDevelopment() && !string.IsNullOrEmpty(builder.Configuration["OtpOverride"]))
    throw new InvalidOperationException("OtpOverride is allowed only in Development.");


builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Formatting = Formatting.Indented;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver
        {
            IgnoreSerializableAttribute = true,
            IgnoreIsSpecifiedMembers = true
        };
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    }
 );

builder.Services.AddDbContext<SaudiPlusDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

builder.Services.Configure<AwsCredentials>(builder.Configuration.GetSection("AWS"));
builder.Services.Configure<AdminCredentials>(builder.Configuration.GetSection("AdminCredentials"));
builder.Services.Configure<BucketSetting>(builder.Configuration.GetSection("Bucket"));
builder.Services.Configure<GoogleAuth>(builder.Configuration.GetSection("Google"));
builder.Services.Configure<SendGridSetting>(builder.Configuration.GetSection("SendGrid"));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateTokenReplay = true,
        RequireSignedTokens = true
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            return Task.CompletedTask;
        },

        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<SaudiPlus.Entities.Identity.User>>();
            var userId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    var securityStamp = context.Principal.FindFirstValue("AspNet.Identity.SecurityStamp");

                    if (securityStamp != user.SecurityStamp)
                    {
                        context.Fail("Security stamp validation failed. User has logged in from another device.");
                    }
                }
                else
                {
                    context.Fail("User not found");
                }
            }
        },
        OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerExtension();
builder.Services.ConfigureApplicationServices();
builder.Services.ConfigureIdentityServices();
builder.Services.ConfigureLanguageService();
builder.Services.ConfigureAutoMapperServies();

builder.Services.AddAWSService<IAmazonS3>();

var awsOptions = builder.Configuration.GetAWSOptions();

if (builder.Environment.IsDevelopment())
{
    awsOptions.Credentials = new BasicAWSCredentials(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"]
    );
}

builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
builder.Services.AddAWSService<IAmazonSQS>();

builder.Services.AddHostedService<SqsNotificationProcessor>();
builder.Services.AddHostedService<RevokeOldTokensService>();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "AllowAll",
            builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    }
);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedWindowPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(30);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: cancellationToken);
        return new ValueTask();
    };
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 20 * 1024 * 1024;
});

builder.Services.AddHttpClient();
var app = builder.Build();

if (app.Environment.IsStaging() || app.Environment.IsProduction())
{
    var firebasePath = builder.Configuration["Firebase:CredentialPath"]
        ?? Path.Combine(builder.Environment.ContentRootPath, ".secrets", "firebase.json");
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebasePath),
    });
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/Mobile/swagger.json", "SaudiPlus Mobile API");
        c.SwaggerEndpoint("/swagger/Dashboard/swagger.json", "SaudiPlus Dashboard API");
        c.SwaggerEndpoint("/swagger/Business/swagger.json", "SaudiPlus Business API");
    });
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
    KnownNetworks = { },
    KnownProxies = { }
});

//app.UseHttpsRedirection();

app.UseMiddleware<LanguageMiddleware>();
app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SaudiPlusDbContext>();
    context.Database.Migrate();

    var serviceProvider = scope.ServiceProvider;

    var adminCredentials = serviceProvider.GetRequiredService<IOptions<AdminCredentials>>();
    await PolicyConfiguration.ConfigurePolicyService(serviceProvider, adminCredentials);
}

app.Run();
