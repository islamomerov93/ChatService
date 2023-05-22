using System.Text;
using System.Text.Json.Serialization;

using ChatService;
using ChatService.Hub;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Reset.Application.Interfaces.Repositories;
using Reset.Infrastructure.Extensions.ServiceCollectionExtensions;
using Reset.Infrastructure.InterfaceImplementations.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args: args);

builder.Services.AddCorsPolicyExtension(builder.Configuration.GetSection("AllowedOrigins").Value.Split(","));

builder.Services.AddDbContextExtension(builder.Configuration);

builder.Services.AddIdentityExtension();

builder.Services.AddAuthentication(configureOptions: o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(configureOptions: o =>
    {
        o.RequireHttpsMetadata = true;
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.ASCII.GetBytes(s: builder.Configuration[key: "JwtOptions:SecurityKey"])),
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Headers["Authorization"];
                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        accessToken = context.Request.Query["access_token"];
                    }

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/chat")))
                    {
                        // Read the token out of the query string
                        context.Request.Headers["Authorization"] = "Bearer " + accessToken;
                    }
                    return Task.CompletedTask;
                }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IDictionary<string, UserConnection>>
    (opts => new Dictionary<string, UserConnection>());

builder.Services.AddSingleton<IDictionary<string, RequestChatUserConnection>>
    (opts => new Dictionary<string, RequestChatUserConnection>());


builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddSignalR().AddJsonProtocol(o =>
    {
        o.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


using (IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope())
{
    IServiceProvider scopeServiceProvider = scope.ServiceProvider;
    
    ILogger logger = scopeServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(categoryName: "Data Seeding context");
        logger.LogInformation(message: "Seeding Default Data Started");

        IChatRepository _repo = scopeServiceProvider.GetRequiredService<IChatRepository> ();

        _repo.CreateGeneralChat();

        logger.LogInformation(message: "Finished Seeding Default Data");
        logger.LogInformation(message: "Application Starting");
}



WebApplication app = builder.Build();

app.UseCors();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<ChatHub>("/chat");

app.Run();