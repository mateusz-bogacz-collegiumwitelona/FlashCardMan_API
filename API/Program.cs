using BackgroundWorks;
using Data.Context;
using Data.Interfaces;
using Data.Models;
using Data.Repositories;
using Data.Seed;
using Events;
using Events.Emails;
using Events.Emails.Helpers;
using Events.Emails.Interfaces;
using Events.Event;
using Events.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services.Interfaces;
using Services.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//register identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});


var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.UTF8.GetBytes(jwtSettings["KEY"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["ISSUER"],
        ValidAudience = jwtSettings["AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero 
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            Console.WriteLine($"DEBUG: OnMessageReceived start. Cookies: {string.Join(", ", context.Request.Cookies.Keys)}");

            if (context.Request.Cookies.ContainsKey("jwt"))
            {
                context.Token = context.Request.Cookies["jwt"];
                Console.WriteLine($"DEBUG: SUCCESS - Token read from ‘jwt’ cookie.");
            }
            else if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    Console.WriteLine($"DEBUG: Token read from Authorization header.");
                }
            }
            else
            {
                Console.WriteLine($"DEBUG: Token not found in ‘jwt’ cookie or header.");
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"DEBUG: Token successfully verified. User: {context.Principal.Identity.Name}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT AUTHORIZATION ERROR: {context.Exception.Message}");
            if (context.Exception.InnerException != null)
            {
                Console.WriteLine($"INTERNAL ERROR: {context.Exception.InnerException.Message}");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(op =>
{
    op.AddPolicy("AllowClient", p =>
    {
        p.WithOrigins("http://localhost:4000", "https://localhost")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});


builder.Services.AddControllers(op =>
{
    var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

    op.Filters.Add(new AuthorizeFilter(policy));
})
.ConfigureApiBehaviorOptions(op =>
{
    op.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
        .Where(e => e.Value.Errors.Count > 0)
        .SelectMany(e => e.Value.Errors)
        .Select(e => e.ErrorMessage)
        .ToList();

        var response = new
        {
            success = false,
            message = "Validation error",
            errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "FlashCardMan API",
        Description = "API to stupid clon of Anki",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});


// add db context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//register repo
builder.Services.AddScoped<IDeckRepository, DeckRepository>();
builder.Services.AddScoped<IFlashCardRepo, FlashCardRepo>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

//register services
builder.Services.AddScoped<IDeckServices, DeckServices>();
builder.Services.AddScoped<IFlashCardService, FlashCardService>();
builder.Services.AddScoped<ILoginRegisterServices, LoginRegisterServices>();

//register queue
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();

//register helpers
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<EmailBodys>();
builder.Services.AddHttpContextAccessor();

//register background services
builder.Services.AddHostedService<EmailBackgroundWorker>();

//register dispachers
builder.Services.AddTransient<IEventDispatcher, EventDispatcher>();

//register observers
builder.Services.AddTransient<IEventHandler<UserEvent>, SendRegistrationEmailHandler>();
builder.Services.AddTransient<IEventHandler<UserEvent>, SendResetPasswordEmailHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FlashCardMan API v1");
    });
}

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManage = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await dbContext.Database.MigrateAsync();
        var seedData = new SeedData(dbContext, userManager, roleManage);
        await seedData.SeedAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during migration/seeding: {ex.Message} | {ex.InnerException}");
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
