# Experimenting With MediatR Pipelines

## Introduction to MediatR

MediatR, available as a NuGet package for .NET, embodies the mediator design pattern, a strategy aimed at decoupling communication between objects. 

By fostering such decoupling, MediatR enhances flexibility and maintainability within software architectures. 

For a comprehensive understanding of this pattern, you can refer to the following resource: [Refactoring Guru - Mediator Design Pattern](https://refactoring.guru/design-patterns/mediator). 

A well-established implementation of this pattern for .NET is MediatR, whose official GitHub project can be found at the following link: [MediatR GitHub Repository](https://github.com/jbogard/MediatR).

## Fundamentals of MediatR

In essence, MediatR operates across three primary modes:

- **Request**: Involving a single receiver with a service response.
- **Notification**: Engaging multiple receivers without a service response.
- **StreamRequest**: Utilizing a single receiver for stream operations with a service response.

For the scope of this project, our focus is primarily on the Request behavior, particularly on exploring MediatR Pipelines.

## Understanding MediatR Pipelines

Within the mediator Request flow, there exists a clear distinction between a publisher and a subscriber. 
By leveraging MediatR pipelines, we can effectively intercept this flow and introduce customized logic into the process.

To implement a pipeline, one needs to inherit from the interface "IPipelineBehavior<TRequest, TResponse>". 

At this juncture, the imperative is to implement the Handle method, as demonstrated below:

```csharp
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
{
    // Pre-processing logic

    var response = await next();

    // Post-processing logic
    
    return response;
}
```

As illustrated in the provided code snippet, this approach enables the insertion of logic both before and after invoking the subsequent step in the mediator pipeline.

Furthermore, the creation of multiple pipeline behaviors, registered in sequence, facilitates the establishment of a cohesive chain of behaviors.

```csharp
   //Just register the behaviors in the order you would like them to be called.
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandAuthorizationBehavior<,>));
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

```


Another noteworthy technique employed in this project involves the customization of the default interface of MediatR's IRequest.

By inheriting the default IRequest interface and crafting our custom Interface, such as ICommand, we gain the ability to explicitly filter pipelines for specific interfaces.

Sample implementation of a custom IRequest:

```csharp
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
```

Sample instantiation of a pipeline tailored exclusively for ICommand:

```csharp
public sealed class MyPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
```

## Practical Application of MediatR

Now let's delve into a practical use case for MediatR.