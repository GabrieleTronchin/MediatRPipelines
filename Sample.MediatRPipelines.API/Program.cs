using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.MediatRPipelines.API.Models;
using Sample.MediatRPipelines.Domain;
using Sample.MediatRPipelines.Domain.Commands.SampleCommand;
using Sample.MediatRPipelines.Domain.Commands.SampleRequest;
using Sample.MediatRPipelines.Persistence;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPersistenceLayer();

builder.Services.AddMediatorSample();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/SampleCommand", ([FromBody] SampleBody sampleBody, IMediator mediator) =>
{
    return mediator.Send(new SampleCommand() { Id = Guid.NewGuid(), Description = sampleBody.Description, EventTime = DateTime.UtcNow });
})
.WithName("SampleCommand")
.WithOpenApi();

app.MapPost("/SampleRequest", ([FromBody] SampleBody sampleBody, IMediator mediator) =>
{
    return mediator.Send(new SampleRequest() { Id = Guid.NewGuid(), Description = sampleBody.Description, EventTime = DateTime.UtcNow });
})
.WithName("SampleRequest")
.WithOpenApi();

app.MapGet("/SampleEntity", (IMediator mediator) =>
{
    return mediator.Send(new SampleEntityQuery());
})
.WithName("SampleEntity")
.WithOpenApi();


app.MapPost("/AddSampleEntity", ([FromBody] SampleBody sampleBody, IMediator mediator) =>
{
    return mediator.Send(new AddSampleEntityCommand() { Id = Guid.NewGuid(), Description = sampleBody.Description, EventTime = DateTime.UtcNow });
})
.WithName("AddSampleRequest")
.WithOpenApi();


app.Run();