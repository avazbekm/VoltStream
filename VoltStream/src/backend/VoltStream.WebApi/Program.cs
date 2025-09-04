using VoltStream.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Service registrations
builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseInfrastructure(); // HTTPS, CORS, Auth
app.UseOpenApiDocumentation(); // Scalar UI

app.MapControllers();

app.Run();
