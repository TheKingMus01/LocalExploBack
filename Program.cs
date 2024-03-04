using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Extensions;
using OpenAI.Interfaces;
using OpenAI.Managers;
using WebApplication1.Interfaces;
using WebApplication1.Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
builder.Services.AddControllers();

builder.Services.AddScoped<IActivity, Activity>();

builder.Services.AddOpenAIService(settings => {
    settings.ApiKey = builder.Configuration["OpenAIServiceOptions:ApiKey"]; ;
});

builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IGeolocationService, GeolocationService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Use CORS policy
app.UseCors("MyPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
