using ExemploLogProviderCSharp.Providers.Logger;
using ExemploLogProviderCSharp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
builder.Services.AddScoped<TesteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Map("/", () => "ExemploLogProviderCSharp - Home");

app.MapControllers();

app.Run();
