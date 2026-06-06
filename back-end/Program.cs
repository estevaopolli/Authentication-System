using System.Net.Mail;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Data.AppDbContext;
using Models.User;
using Services.AuthService;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddTransient<AuthService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var secretKey = builder.Configuration["Jwt:Secret"];
    var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = jwtKey,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
}
);
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontEnd", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("PermitirFrontEnd");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/signup", async (User u, AppDbContext db, AuthService service) =>
{
    var passwordHasher = new PasswordHasher<User>();
    string hash = passwordHasher.HashPassword(u, u.Password);
    

    if (string.IsNullOrEmpty(u.Username))
    {
        return Results.BadRequest(new {message = "Usuário inválido", code = "INVALID_USERNAME"});
    }
    if (string.IsNullOrEmpty(u.Email))
    {
        return Results.BadRequest(new {message = "E-mail inválido", code = "INVALID_EMAIL"});
    }

    if (string.IsNullOrEmpty(u.Password))
    {
        return Results.BadRequest(new {message = "Senha inválida", code = "INVALID_PASSWORD"});
    }
    else
    {
        u.Password = hash;
    }

    bool emailExists = await db.Users.AnyAsync(user => user.Email == u.Email);
    bool userExists = await db.Users.AnyAsync(user => user.Username == u.Username);

    if (userExists)
    {
        return Results.BadRequest(new {message = "Usuário já existe", code = "USER_ALREADY_EXISTS"});
    }
    else if (emailExists)
    {
        return Results.BadRequest(new {message = "Email já existe", code = "EMAIL_ALREADY_EXISTS"});
    }
    else
    {
        var jwtToken = service.Generate(u);
        db.Users.Add(u);

        await db.SaveChangesAsync();
        return Results.Ok(new {message="Usuário cadastrado com sucesso", code="SUCCESSFUL_REGISTRATION", token = jwtToken});
    }
});

app.MapPost("/login", async (User u, AppDbContext db, AuthService service) =>
{
    var passwordHasher = new PasswordHasher<User>();
    var findingUser = await db.Users.FirstOrDefaultAsync(user => user.Email == u.Email);
    if(findingUser != null)
    {
        var hashedPassword = findingUser.Password;
        var passwordVerify = passwordHasher.VerifyHashedPassword(u, hashedPassword, u.Password);

        if (passwordVerify == PasswordVerificationResult.Success)
        {
            var jwtToken = service.Generate(u);

            return Results.Ok(new {message = "Usuário validado com sucesso", code = "SUCCESSFUL_VALIDATION", token = jwtToken});
        }
    }
    return Results.BadRequest(new {message = "E-mail ou Senha incorretos", code = "INCORRECT_CREDENTIALS"});
});

app.MapGet("/profile", [Authorize(Roles = "User")]() =>
{
    return Results.Ok(new {message = "Usuário Autenticado!"});
}).RequireAuthorization();

app.Run();


