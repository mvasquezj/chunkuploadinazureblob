using SASApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddBlobService();
builder.Services.AddSingleton<SasService>();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.MapPost("/api/sas", async (SasService sasService) => await sasService.GetContainerSasToken("data"))
.WithName("SAS");

app.Run();
