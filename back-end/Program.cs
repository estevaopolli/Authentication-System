
using System.Security.Cryptography;
using System.Threading.RateLimiting;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;

using Services.AuthService;

using Data.AppDbContext;
using Models.User;
using Models.Recover;

using Microsoft.AspNetCore.Identity.Data;

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

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 4;
        opt.Window = TimeSpan.FromSeconds(12);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});


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
app.UseRateLimiter();

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
}).RequireRateLimiting("fixed");


app.MapPost("/recover", async (RecoverUser recoverUser, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(user => user.Email == recoverUser.Email);
    if (user != null)
    {
        string resetToken = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        string resetTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(resetToken)));
        user.ResetToken = resetTokenHash;
        await db.SaveChangesAsync();

        return Results.Ok(new {message = "Email existe", resetToken = resetToken, code = "RESET_CODE_GENERATED"});
    }
    else
    {
        return Results.NotFound(new {message = "Email não existe", code = "EMAIL_NOT_FOUND"});
    }
}).RequireRateLimiting("fixed");

app.MapPost("/reset-password", async (RecoverUser recoverUser, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(user => user.Email == recoverUser.Email);

    if (string.IsNullOrEmpty(recoverUser.ResetToken))
    {
        return Results.BadRequest(new {message = "Código Inválido", code = "INVALID_RESET_TOKEN"});
    }
    if (string.IsNullOrEmpty(recoverUser.NewPassword))
    {
        return Results.BadRequest(new {message = "Senha inválida", code = "INVALID_NEW_PASSWORD"});
    }
    if (recoverUser.NewPassword != recoverUser.ConfirmNewPassword)
    {
        return Results.BadRequest(new {message = "Senha não coincidem", code = "PASSWORDS_DONT_MATCH"});
    }

    string receivedTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(recoverUser.ResetToken)));
    Console.WriteLine("degub");
    if(receivedTokenHash == user.ResetToken)
    {
        var newPasswordHasher = new PasswordHasher<RecoverUser>();
        string hash = newPasswordHasher.HashPassword(recoverUser, recoverUser.NewPassword);
        user.Password = hash;
        await db.SaveChangesAsync();
        return Results.Ok(new {message="Senha alterada com sucesso", code = "SUCCESSFUL_PASSWORD_RESET"});
    }
    else
    {
        return Results.BadRequest(new {message="Código Inválido", code = "INVALID_RESET_TOKEN"});
    }
});

app.MapGet("/profile", [Authorize(Roles = "User")]() =>
{
    return Results.Ok(new {message = "Usuário Autenticado como usuário!", code = "SUCCESSFUL_AUTHENTICATION"});
}).RequireAuthorization();

app.Run();


