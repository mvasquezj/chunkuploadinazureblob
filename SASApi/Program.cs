using Microsoft.AspNetCore.Mvc;
using SASApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddBlobService();
builder.Services.AddSingleton<SasService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapPost("/api/sas", async ([FromBody] FileData fileData, SasService sasService) => 
    await sasService.GetContainerSasToken((fileData)))
.WithName("SAS");

app.Run();
