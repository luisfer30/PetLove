using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<SaaSVeterinarioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SaaSVeterinario")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
