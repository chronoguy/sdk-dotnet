# Temporal .NET SDK <br /> Architecture and strategy

**.NET SDK: Архитектура и стратегия***

<div style="text-align: right"><strong>*</strong><sub><sup>this doc needs to contain some non-ASCII chars so that
the tools know to use UTF-8 without the signature</sup></sub></div>

<br />

We are currently planning to provide support for .NET developers to use Temporal. We will provide support for both:
 - **Host SDK**: write Temporal workflows using .NET and use such workflows from any language supported by Temporal;
 - **Client SDK**: use Temporal workflows (written in any supported language) from within your .NET applications.

 Here, we discuss scope for the initial version and propose the API shape.

## Supported versions

We plan to support the following .NET versions:
 - .NET Framework 4.6.2 and later
 - .NET Core 3.1 (Windows and Linux)
 - .NET 5
 - .NET 6 (Windows and Linux)

Initially, we plan to only support 64 bit applications.
However, we are aware that many customers may have the need to support 32 bit applications. In particular, Windows services are frequently run as 32 bit apps. We will prioritize adding such support based on customer feedback.

## High level architecture: Host SDK

The focus of the Host SDK is to provide an easy and reliable way to host Temporal workflow workers and temporal activity workers.

**Build on top of "Core".** We will build the .NET Host SDK on top of the [SDK Core library](https://github.com/temporalio/sdk-core) (aka "Core"). Core is a native library written in Rust that provides common abstractions for Temporal state machines, for remote GRPC invocations, and for other aspects required to build runtime-specific SDKs with consistent functionality. Currently, Core is used as the basis for the Typescript/Javascript SDK.

Where Core features are lacking, we will make case-by-case decisions to add required functionality to Core or to implement it directly in .NET.

**SDK Engine hosts workflows and activities and manages their execution.** The center of the .NET SDK is the _Engine_. The Engine keeps a list of workflows and activities hosted in a particular worker host, and a mapping of active workflow (and activity) instances and their respective local execution states. The Engine contains a message loop that polls Core for work items, such as workflow activations, activity tasks, etc. Based on that, the engine invokes and manages user-provided workflow / activity implementations.

.NET-based Temporal _activities_ are - similar to other languages supported by Temporal -  (almost) arbitrary components. There is only a small amount of runtime management involved into managing the execution of activities. In this high-level overview we therefore focus on the execution of .NET-based Temporal _workflows_.

### Workflow Restrictions

In general, .NET-based workflows provide very similar guarantees and follow very similar restrictions as other languages. A key aspect of the restrictions across all Temporal languages is that workflows must be _deterministic_. This requires special rules around any APIs that may not produce identical results if run multiple times with the same input. Most prominently this includes APIs that interact with the environment (e.g. I/O), and threading/parallelism related APIs (because most common multi-threading implementations are not deterministic in nature). Here we focus on .NET-specific nuances.

**Workflows are strictly single-threaded and asynchronous:**

**In the first version of the .NET SDK, workflows must be <u>strictly logically single-threaded</u>.**
It is not permitted to use any APIs that create or manipulate threads (implicitly or explicitly), execute thread-synchronization primitives, or perform any kind of parallel execution. Note that being _logically_ single-threaded does not mean that a given workflow is bound to a specific thread. On contrary, the Engine will suspend workflows that block waiting for Temporal events to occur, and it will resume such workflows on any (likely a different) thread when those events eventually occur. However, _logically_ workflow must be strictly single-threaded. I.e., while the execution may be performed on different underlying threads, it must never _fork_ into parallel (aka concurrent) execution sequences.

**Parallelism can be created via activities.** Of course, this restriction does _not_ apply to entities that are managed by Temporal on behalf of a workflow. E.g., workflows may use activities to perform tasks that require multi-threading. Workflows may also invoke multiple activities without waiting for previous activities to complete. This effectively creates parallelism, as activities may (and will) be executed concurrently to each other. However, actual workflow implementation is always strictly single-threaded.

**.NET asynchrony does not create concurrency.** Crucially, async/await-based asynchrony in .NET is _not_ implicitly multi-threaded. This means that invoking or awaiting [Task](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)-based async APIs does not result in logical multi-threading, except when a parallel execution thread is explicitly requested via one of the corresponding APIs. More on this topic is discussed in section [@ToDo]().

**Therefore, it is permitted to use async/await-based code in workflows. In fact, all Temporal interactions in .NET workflows are asynchronous and therefore all .NET workflows are required to be asynchronous.**

**Alternative considerations.** It must be noted that other language SDKs take a variety of approaches to the multi-threading restrictions. This is required asynchronous and parallel programming is handled differently across runtimes. For example, the Java SDK permits "pseudo-parallelism": It permits workflows to invoke a subset of threading API. The Java SDK takes over the scheduling of such threads and ensures that such scheduling is deterministic.

In .NET such approach is feasible, but we made an explicit decision not to provide such features in version 1. Two main reasons drove this decision:
 1. Because of the logically single-threaded nature of .NET async APIs, all workflow scenarios are better implemented using logically single threaded code. If, however, developers opted into _explicitly_ using multi-threaded parallelism (rather than asynchrony), then their code very likely depends on the parallelism to work as expected. A custom deterministic scheduler will invalidate developers' assumptions and lead to corruption. Such corruptions may be hard to diagnose and understand. Prohibiting multi-threading altogether leads developers into a pit of success.
 2. Adding features is easier than removing problematic features. We will listen to our user community and if, with time, we learn that there is an existing need to relax the single-threading restrictions in some particular way, we will - naturally - work to understand the underlying scenarios and to evolve the SDK accordingly. This is less risky than to provide features that are hard to use and can cause hard-to-diagnose reliability issues for some developers, and not being able to remove such features later, because other developers took a dependency on them.

**Environment interactions:**

**SDK offers deterministic alternatives to frequently used non-deterministic APIs.** Most of the non-threading related non-deterministic APIs lack determinism, because they interact with the runtime environment. Examples include time-related APIs (including random number generators seeded by time), I/O, hardware access and configuration access. The SDK provides deterministic alternatives to most frequent such APIs in a manner similar to other SDKs.

**Data resolved via .NET dependency injection APIs is deterministic, because it is persisted in workflow history and read from there when a workflow is replayed.** Among other APIs that interact with the host environment, configuration APIs deserve a particularly careful treatment. It is not practical to move all configuration access into activities. Moreover, .NET workflow host is initialized in a manner that closely follows the conventions for configuring an ASP.NET Core application host. That approach relies on the ASP.NET Core dependency injection mechanism for runtime component dependencies as well as for configuration. The SDK helps developers to avoid pitfalls around non-deterministic configuration data by making configuration access a first-class SDK feature: The SDK Engine plugs into the application host's dependencies provider (aka "service provider") and makes sure that any configuration data resolved for a particular workflow instance is automatically persisted using Temporal history marker-events. If a workflow is replayed, previously resolved configuration data is sourced from the recorded workflow history.

### Workflow Execution

. . .

### Workflow Host Configuration

The user-facing foundation of the SDK is the host. It is configured in a manner that closely follows the conventions for configuring an ASP.NET Core application host ([basic example](../src/Samples/Basic/Temporal.Samples.DesignProcess/Part1_1_TypicalWorkflow.cs#L73-L89) | [more examples](../src/Samples/Basic/Temporal.Samples.DesignProcess/Part4_1_BasicWorkflowUsage.cs#L67-L202) | [another example](../src/Samples/Basic/Temporal.Samples.DesignProcess/Part4_2_BasicWorkflowUsage_MultipleWorkers.cs#L108-L158)).

Users provide workflow implementations and activity implementations in one of two ways:
 - Pattern based: Entry points of workflow and activity implementations follow specific, well known patterns. Developers use attributes to provide metadata about such entry point to the host. The host automatically loads, and executes workflows and activities. This approach allows writing workflows (and activities) with hardly any scaffolding. 
- Interface based: Developers implement specific interfaces to create workflows (and activities), and point the host to these implementations. The host automatically loads, and executes workflows and activities. This approach requires a little more scaffolding. However, it also allows additional functionality and helps addressing some advanced scenarios (e.g. dynamic workflow APIs).




## High level architecture: Client SDK

. . .
