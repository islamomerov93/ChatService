using System.Text;

using ChatService;
using ChatService.Data;
using ChatService.Data.Models;
using ChatService.Hub;
using ChatService.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args: args);

builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddAuthentication(configureOptions: o =>
//    {
//        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//    }).AddJwtBearer(configureOptions: o =>
//    {
//        o.RequireHttpsMetadata = true;
//        o.SaveToken = true;
//        o.TokenValidationParameters = new TokenValidationParameters
//                                          {
//                                              ValidateIssuer = false,
//                                              ValidateIssuerSigningKey = true,
//                                              IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.ASCII.GetBytes(s: builder.Configuration[key: "JwtOptions:SecurityKey"])),
//                                              ValidateAudience = false,
//                                              ValidateLifetime = true,
//                                              ClockSkew = TimeSpan.Zero
//                                          };
//    });

builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            configurePolicy: policy =>
                {
                    policy.WithOrigins("http://localhost:3000");
                    policy.AllowCredentials();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
    });

builder.Services.AddDbContext<ApplicationDbContext>(optionsAction: options =>
    options.UseSqlServer(connectionString: builder.Configuration.GetConnectionString(name: "DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(setupAction: opt =>
        {
            opt.Password.RequiredLength = 8;
            opt.Password.RequireDigit = true;
            opt.Password.RequireUppercase = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireNonAlphanumeric = true;
            opt.User.RequireUniqueEmail = true;
        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IDictionary<string, UserConnection>>
    (opts => new Dictionary<string, UserConnection>());


builder.Services.AddTransient<IChatRepository, ChatRepository>();

WebApplication app = builder.Build();

app.UseCors();

//app.UseAuthentication();

//app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chat");

app.Run();