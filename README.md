# Experimenting With MediatR Pipelines

## About MediatR

MediatR, available as a NuGet package for .NET, embodies the mediator design pattern. This pattern serves to decouple communication between objects, enhancing flexibility and maintainability in software architectures. For a comprehensive understanding of this pattern, you can refer to the following resource: [Refactoring Guru - Mediator Design Pattern](https://refactoring.guru/design-patterns/mediator). A well-established implementation of this pattern for .NET is MediatR. You can find the official GitHub project for MediatR at the following link: [MediatR GitHub Repository](https://github.com/jbogard/MediatR).

## Basic MediatR Info

In simple terms, MediatR operates in three modes:

- **Request**: Involves a single receiver with a service response.
- **Notification**: Engages multiple receivers with no service response.
- **StreamRequest**: Utilizes a single receiver for stream operations with a service response.

For this project, our focus lies on the Request behavior, particularly on MediatR Pipelines.

## MediatR Pipelines

In the mediator Request flow, there exists a publisher and a subscriber. Utilizing MediatR pipelines allows us to intercept this flow and introduce logic in the middle of the process.

To implement a pipeline, we need to inherit from the interface "IPipelineBehavior<TRequest, TResponse>". At this point, we must implement the Handle method. Here's a sample:

```csharp
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
{
    // Pre-processing logic

    var response = await next();

    // Post-processing logic
    
    return response;
}
```

As depicted in the code, we can introduce logic before or after calling the next step in the mediator pipeline.

We can create multiple pipeline behavior, ...TODO

## MediatR Use Case