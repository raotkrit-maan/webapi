using Microsoft.EntityFrameworkCore;
using webapi.Data.DbContexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Scan(scan=>scan.FromAssemblyOf<Program>()
                    .AddClasses(classes=>classes.InNamespaces("webapi.Services"))
                    .AsMatchingInterface()
                    .WithScopedLifetime());


  builder.Services.AddCors(options =>
{
    options.AddPolicy("private_policy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://127.0.0.1:5173",
                "http://<<ALLOWED_IP>>:3000",
                "https://<<ALLOWED_DOMAIN>>"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // ถ้าคุณใช้ cookie/credentials ค่อยเปิดอันนี้:
        // .AllowCredentials();
    });

    options.AddPolicy("public_policy", policy =>
    {
        policy
            .AllowAnyOrigin()
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
app.UseCors("public_policy");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
