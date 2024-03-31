using CostaRicaBeaches.Api.Configurations;
using CostaRicaBeaches.Api.Interfaces;
using CostaRicaBeaches.Api.Middlewares;
using CostaRicaBeaches.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Costa Rica",
        Version = "v1"
    });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "OAuth2.0 Auth Code with Azure Entra",
        Name = "oauth2",
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration["AzureAdB2CSwagger:AuthorizationUrl"]!),
                TokenUrl = new Uri(builder.Configuration["AzureAdB2CSwagger:TokenUrl"]!),
                Scopes = new Dictionary<string, string>
                {
                    { builder.Configuration["AzureAdB2CSwagger:ApiScope"]!, "Access to the Costa Rica Beaches Chat" }
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { builder.Configuration["AzureAdB2CSwagger:ApiScope"]! }
        }
    });
});

builder.Services.Configure<AzureAISearchOptions>(builder.Configuration.GetSection("AzureAISearch"));
builder.Services.Configure<AzureOpenAIOptions>(builder.Configuration.GetSection("AzureOpenAI"));

builder.Services.AddSingleton<IAzureAISearchService, AzureAISearchService>();
builder.Services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.NameClaimType = "name";
    }, options => { builder.Configuration.Bind("AzureAdB2C", options); });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("beaches_chatbot", policy =>
        policy.RequireAuthenticatedUser());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Costa Rica v1");
    c.OAuthClientId(builder.Configuration["AzureAdB2CSwagger:OpenIdClientId"]!);
    c.OAuthUsePkce();
    c.OAuthScopeSeparator(" ");
});

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapGet("CostaRica/Beaches/Chat", async ([FromQuery] string message, [FromServices] IAzureOpenAIService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.SendMessageAsync(message, cancellationToken));
}).RequireAuthorization().RequireScope("CostaRica:Beaches:ChatBot");

await app.RunAsync();

