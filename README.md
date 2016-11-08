LiquidState
====

Efficient state machines for .NET with both synchronous and asynchronous support.
Heavily inspired by the excellent state machine library [**Stateless**](https://github.com/nblumhardt/stateless) by
**Nicholas Blumhardt.**

[![NuGet badge](https://buildstats.info/nuget/LiquidState)](https://www.nuget.org/packages/LiquidState)

Installation
----

**NuGet:**

> Install-Package LiquidState

**Supported Platforms:**
> .NETPlatform 1.0 (Formerly PCL259 profile - Supports .NETCore, .NETDesktop, Xamarin and Mono) 

Highlights
----

- Zero heap allocations during the machine execution - GC friendly and high-performance. (Awaitable machines still incur the async/await
costs).
- Fully supports async/await methods everywhere => `OnEntry`, `OnExit`, during trigger, and even trigger conditions.
- Builds a linked object graph internally during configuration making it a much faster and more efficient implementation than regular dictionary based implementations.
- Both synchronous, and asynchronous machines with full support for `async-await`.
- `MoveToState`, to move freely between states, without triggers.
- `PermitDynamic` to support selection of states dynamically on-the-fly.
- `Diagnostics` in-built to check for validity of triggers, and currently available triggers.

[**Release Notes**](https://github.com/prasannavl/LiquidState/blob/master/ReleaseNotes.md)

How To Use
---

You only ever create machines with the `StateMachineFactory` static class. This is the factory for both configurations and the machines. The different types of machines given above are automatically chosen based on the parameters specified from the factory.

**Step 1:** Create a configuration:

```c#
var config = StateMachineFactory.CreateConfiguration<State, Trigger>();
```

or for awaitable, or async machine:

```c#
var config = StateMachineFactory.CreateAwaitableConfiguration<State, Trigger>();
```

**Step 2:** Setup the machine configurations using the fluent API.

```c#
    config.ForState(State.Off)
        .OnEntry(() => Console.WriteLine("OnEntry of Off"))
        .OnExit(() => Console.WriteLine("OnExit of Off"))
        .PermitReentry(Trigger.TurnOn)
        .Permit(Trigger.Ring, State.Ringing,
                () => { Console.WriteLine("Attempting to ring"); })
        .Permit(Trigger.Connect, State.Connected,
                () => { Console.WriteLine("Connecting"); });

    var connectTriggerWithParameter =
                config.SetTriggerParameter<string>(Trigger.Connect);

    config.ForState(State.Ringing)
        .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
        .OnExit(() => Console.WriteLine("OnExit of Ringing"))
        .Permit(connectTriggerWithParameter, State.Connected,
                name => { 
                 Console.WriteLine("Attempting to connect to {0}", name);
                })
        .Permit(Trigger.Talk, State.Talking,
                () => { Console.WriteLine("Attempting to talk"); });
```

**Step 3:** Create the machine with the configuration:

```c#
var machine = StateMachineFactory.Create(State.Ringing, config);
```

**Step 4:** Use them!

* Using triggers:

>
```c#
machine.Fire(Trigger.On);
```
or 
```
await machine.FireAsync(Trigger.On);
```

* Using direct states:

>
```c#
machine.MoveToState(State.Ringing);
``` 
or its async variant.


* To use parameterized triggers, have a look at the example below.

Examples
---

A synchronous machine example:

```c#
    var config = StateMachineFactory.CreateConfiguration<State, Trigger>();

    config.ForState(State.Off)
        .OnEntry(() => Console.WriteLine("OnEntry of Off"))
        .OnExit(() => Console.WriteLine("OnExit of Off"))
        .PermitReentry(Trigger.TurnOn)
        .Permit(Trigger.Ring, State.Ringing,
                () => { Console.WriteLine("Attempting to ring"); })
        .Permit(Trigger.Connect, State.Connected,
                () => { Console.WriteLine("Connecting"); });

    var connectTriggerWithParameter =
                config.SetTriggerParameter<string>(Trigger.Connect);

    config.ForState(State.Ringing)
        .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
        .OnExit(() => Console.WriteLine("OnExit of Ringing"))
        .Permit(connectTriggerWithParameter, State.Connected,
                name => { Console.WriteLine("Attempting to connect to {0}", name); })
        .Permit(Trigger.Talk, State.Talking,
                () => { Console.WriteLine("Attempting to talk"); });

    config.ForState(State.Connected)
        .OnEntry(() => Console.WriteLine("AOnEntry of Connected"))
        .OnExit(() => Console.WriteLine("AOnExit of Connected"))
        .PermitReentry(Trigger.Connect)
        .Permit(Trigger.Talk, State.Talking,
            () => { Console.WriteLine("Attempting to talk"); })
        .Permit(Trigger.TurnOn, State.Off,
            () => { Console.WriteLine("Turning off"); });


    config.ForState(State.Talking)
        .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
        .OnExit(() => Console.WriteLine("OnExit of Talking"))
        .Permit(Trigger.TurnOn, State.Off,
            () => { Console.WriteLine("Turning off"); })
        .Permit(Trigger.Ring, State.Ringing,
            () => { Console.WriteLine("Attempting to ring"); });

    var machine = StateMachineFactory.Create(State.Ringing, config);

    machine.Fire(Trigger.Talk);
    machine.Fire(Trigger.Ring);
    machine.Fire(connectTriggerWithParameter, "John Doe");
```

Now, let's take the same dumb, and terrible example, but now do it **asynchronously**!
(Mix and match synchronous code when you don't need asynchrony to avoid the costs.)

```c#
    // Note the "CreateAwaitableConfiguration"
    var config = StateMachineFactory.CreateAwaitableConfiguration<State, Trigger>();

    config.ForState(State.Off)
        .OnEntry(async () => Console.WriteLine("OnEntry of Off"))
        .OnExit(async () => Console.WriteLine("OnExit of Off"))
        .PermitReentry(Trigger.TurnOn)
        .Permit(Trigger.Ring, State.Ringing,
            async () => { Console.WriteLine("Attempting to ring"); })
        .Permit(Trigger.Connect, State.Connected,
            async () => { Console.WriteLine("Connecting"); });

    var connectTriggerWithParameter =
                config.SetTriggerParameter<string>(Trigger.Connect);

    config.ForState(State.Ringing)
        .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
        .OnExit(() => Console.WriteLine("OnExit of Ringing"))
        .Permit(connectTriggerWithParameter, State.Connected,
                name => { Console.WriteLine("Attempting to connect to {0}", name); })
        .Permit(Trigger.Talk, State.Talking,
                () => { Console.WriteLine("Attempting to talk"); });

    config.ForState(State.Connected)
        .OnEntry(async () => Console.WriteLine("AOnEntry of Connected"))
        .OnExit(async () => Console.WriteLine("AOnExit of Connected"))
        .PermitReentry(Trigger.Connect)
        .Permit(Trigger.Talk, State.Talking,
            async () => { Console.WriteLine("Attempting to talk"); })
        .Permit(Trigger.TurnOn, State.Off,
            async () => { Console.WriteLine("Turning off"); });

    config.ForState(State.Talking)
        .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
        .OnExit(() => Console.WriteLine("OnExit of Talking"))
        .Permit(Trigger.TurnOn, State.Off,
            () => { Console.WriteLine("Turning off"); })
        .Permit(Trigger.Ring, State.Ringing,
            () => { Console.WriteLine("Attempting to ring"); });

    var machine = StateMachineFactory.Create(State.Ringing, config);

    await machine.FireAsync(Trigger.Talk);
    await machine.FireAsync(Trigger.Ring);
    await machine.FireAsync(connectTriggerWithParameter, "John Doe");
```

Core APIs
---

**IStateMachineCore:**

```c#
public interface IStateMachineCore<TState, TTrigger>
{
    TState CurrentState { get; }
    bool IsEnabled { get; }
    void Pause();
    void Resume();

    event Action<TriggerStateEventArgs<TState, TTrigger>> UnhandledTrigger;
    event Action<TransitionEventArgs<TState, TTrigger>> InvalidState;
    event Action<TransitionEventArgs<TState, TTrigger>> TransitionStarted;
    event Action<TransitionExecutedEventArgs<TState, TTrigger>>
                                                       TransitionExecuted;
}
```

**Synchronous:**

```c#
public interface IStateMachine<TState, TTrigger> 
        : IStateMachineCore<TState, TTrigger>
{
    IStateMachineDiagnostics<TState, TTrigger> Diagnostics { get; }

    void Fire<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, 
            TArgument argument);
    void Fire(TTrigger trigger);
    void MoveToState(TState state, 
            StateTransitionOption option = StateTransitionOption.Default);
}
```

**Awaitable:**

```c#
public interface IAwaitableStateMachine<TState, TTrigger> 
        : IStateMachineCore<TState, TTrigger>
{
    IAwaitableStateMachineDiagnostics<TState, TTrigger> Diagnostics { get; }

    Task FireAsync<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument);
    Task FireAsync(TTrigger trigger);
    Task MoveToStateAsync(TState state, 
            StateTransitionOption option = StateTransitionOption.Default);
}
```

In-built Machines
---

* **Common Roots:**
    - **AbstractStateMachineCore** - All machines derive from this. This mostly just provide the common boiler plates.

* **Synchronous:**
    - **RawStateMachine** - Direct, fully functional raw state machine. No protective abstractions or overheads. Typically only used as a base for other machines.
    - **GuardedStateMachine** - Lock-free protection over raw state machine. Minimal statemachine for independent usage.
    - **BlockingStateMachine** - Synchronized using Monitor and blocks until all of the requests are completed one by one. Order is not guaranteed, when parallel triggers are fired due to the nature of locks.

* **Awaitable:**
    - **RawAwaitableStateMachine** - Direct, fully functional raw state machine. No protective abstractions or overheads. Typically only used as a base for other machines.
    - **GuardedAwaitableStateMachine** - Lock-free protection over raw state machine. Minimal statemachine for independent usage.
    - **ScheduledAwaitableStateMachine** - Schedules the machine implementation to an external TaskScheduler. Thread-safety, order, and synchronization are the responsibility of the scheduler.
    - **QueuedAwaitableStateMachine** - A lock-free implementation of a fully asynchronous queued statemachine. Order is guaranteed.

Notes:

- Awaitable state machines are a superset of asynchronous machines. All async machines are awaitable, but the opposite `may or may not` be true.
- Most of the above machines come with both their own sealed classes as well as `Base` classes, for extending them.

Dynamic Triggers
---

A simple implementation of the dynamic trigger is a part of the sample.
For more information or if you want to understand in detail the choices leading up to the design, please have a look at:
https://github.com/prasannavl/LiquidState/pull/20, and https://github.com/prasannavl/LiquidState/pull/7

Support
----

Please use the GitHub issue tracker [here](https://github.com/prasannavl/LiquidState/issues) if you'd like to report problems or discuss features. As always, do a preliminary search in the issue tracker before opening new ones - (*Tip:* include pull requests, closed, and open issues: *[Exhaustive search](https://github.com/prasannavl/LiquidState/issues?q=)* ).


Credits
---
Thanks to [JetBrains](https://www.jetbrains.com) for the OSS license of Resharper Ultimate.

Proudly developed using:

<a href="https://www.jetbrains.com/resharper/
"><img src="https://blog.jetbrains.com/wp-content/uploads/2014/04/logo_resharper.gif" alt="Resharper logo" width="100" /></a>


Contributions
----

If this project has helped you, the best way to `say thanks` is to contribute back :)

**Note:** If your contribution introduces new API surface, features, or includes either conceptual or implementational changes, it is **highly recommended** that an issue is opened first (which will be labelled accordingly), so that design notes or at-least a quick review on the direction is discussed, before any large effort is made. There's nothing I hate more than good contributions that go unmerged. :)
