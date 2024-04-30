using MediatR;
using Sample.MediatRPipelines.Domain;
using Sample.MediatRPipelines.Domain.SampleCommand;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatRDomainSample();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/SampleRequest", (IMediator mediator) =>
{
    //TODO Add bogus
    mediator.Send(new SampleCommand() { Id = Guid.NewGuid(), Description = "Sample Description", EventTime = DateTime.UtcNow });
})
.WithName("SampleRequest")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
