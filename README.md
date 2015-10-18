LiquidState
====

Efficient state machines for .NET with both synchronous and asynchronous support.
Heavily inspired by the excellent state machine library [**Stateless**](https://github.com/nblumhardt/stateless) by
**Nicholas Blumhardt.**

[![Build status](https://ci.appveyor.com/api/projects/status/6a1pmx2o3jaje60m/branch/master?svg=true)](https://ci.appveyor.com/project/prasannavl/liquidstate/branch/master) [![NuGet downloads](http://img.shields.io/nuget/dt/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState)
[![NuGet stable version](http://img.shields.io/nuget/v/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState) [![NuGet pre version](http://img.shields.io/nuget/vpre/LiquidState.svg?style=flat)](https://www.nuget.org/packages/LiquidState)

Installation
----

**NuGet:**

> Install-Package LiquidState

**Supported Platforms:**
> PCL profile 259: Supports all platforms including Xamarin.iOS and Xamarin.Android.

Highlights
----

- Fully supports async/await methods everywhere => `OnEntry`, `OnExit`, during trigger, and even trigger conditions.
- Builds a linked object graph internally during configuration making it a much faster and more efficient implementation than regular dictionary based implementations.
- Both synchronous, and asynchronous machines with full support for `async-await`.
- `MoveToState`, to move freely between states, without triggers.
- `PermitDynamic` to support selection of states dynamically on-the-fly.
- `Diagnostics` in-built to check for validity of triggers, and currently available triggers.

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
// Note the "CreateAsyncConfiguration"
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


Contributions
----

If this project has helped you, the best way to `say thanks` is to contribute back :)

Any commits into the `master` branch will be automatically built and deployed as `pre-release` nuget packages.

**Note:** If your contribution introduces new API surface, features, or includes either conceptual or implementational changes, it is **highly recommended** that an issue is opened first (which will be labelled accordingly), so that design notes or at-least a quick review on the direction is discussed, before any large effort is made. There's nothing I hate more than good contributions that go unmerged. :)


Release Notes
---

######v1.1.0

- Added removable invalid trigger event handler by default
- Added `Ignore` and `IgnoreIf` to configurations

######v1.2.0

- Added generic parameterized triggers

######v1.3.0-beta

- Added QueuedAsyncStateMachine with customizable synchronization context, and queued Fire semantics.

######v.2.0.0-beta

- Changed AsyncStateMachine to AwaitableStateMachine
- Changed QueuedAsyncStateMachine to AsyncStateMachine
- AwaitableStateMachine are logically synchronous but accepts Task and async methods and can be awaited.
- AsyncStateMachine is fully asynchronous and dispatched onto the instantiated synchronization context, and is thread safe.
- AsyncStateMachines are queued by default.
- All except AsyncStateMachines will throw InvalidOperationException if attempted to Fire while a transition is in progress.

######v.2.1.0-beta

- Critical Fix: All the state machines never reset the IsRunning value on error or unhandled state, leading to the machine being dormant
- More robust error handling
- Removed StateMachine.ReConfigure. Retain the configuration, and reconfigure it any time to modify a live state machine.
- FluidStateMachine added

######v.2.1.1-beta

- Improve fluid flow dynamics for FluidStateMachine

######v.2.1.2-beta

- Allow null states in FluidStateMachine
- Allow zero configuration start for FluidStateMachine

######v.2.1.4-beta

- Remove FluidStateMachine in favor of SimpleStateMachine (not available yet)

######v.2.1.5-beta

- Fix: Async state machine queues attempts to execute concurrently (throwing an exception preventing it) when it enters a queue, and not awaited

######v.2.1.6-beta

- Remove dispatcher on AsyncStateMachine. Dispatching and synchronization be easily handled within the delegates itself, and not by the machine.

######v.3.0.0-beta

- Complete re-write of all the machines, with Interlocked routines. All three machines are lock-free and thread-safe.
- Add MoveToState(TState, StateTransitionOption) to all three state machines to move freely between states.

######v.3.0.1-beta

- Fix: MoveToState on AsyncStateMachine was broken due to wrong internal method being called.

######v.3.0.2-beta

- Minor changes and fixes

######v.3.0.3-beta

- Cleanup release

######v.3.0.4-beta

- Internal implementation changes.
- Remove Stop method from all machine, as the complexity it bring about is simply unnecessary since the functionality can easily be implemented with an additional state.

######v.3.0.5

- v3.0.5, is now transitioning to its own Immutable Collections, and as a side effect, it is also out of beta, and is  now fully stable.

######v.3.0.6

- Drop dependency on System.Collections.Immutable

######v.3.1.0

- Version bump, with removal of all dependencies.
- Fix: Code contracts rewrite was missing on Release builds.

######v.3.2.0

- Breaking change: The configuration API has been renamed from
  **x.Configure(TState)** to **x.ForState(TState)**
  (Since, its only small literal change more in line with the semantics involved, the major version number has been retained)

######v.3.2.1

- Minor perf improvements
- Enforce strict semantic versioning

######v.4.0

- Rename StateMachine to StateMachineFactory to be more approriate.
- Add a new BlockingStateMachine for a synchronous machine that processes sequentially by blocking, enabled with a blocking parameter on creation.
- StateMachineFactory now returns IStateMachine instead of the concrete class for synchronous state machines also.

######v.4.1

- Added new AwaitableStateMachineWithScheduler which is just a simple AwaitableStateMachine that accepts a TaskScheduler to directly execute them without the queuing, or thread-safety overheads.

######v.4.1.1

- Fix CanHandleTrigger()

######v.5.0.0

- Add `Task<bool> CanHandleTriggerAsync(TTrigger trigger);` to IAwaitableStateMachine
- Make CanHandleTrigger check for predicates, if present.

Breaking changes:

- Removed `bool CanHandleTrigger(TTrigger trigger)` from it IAwaitableStateMachine. Has been replaced by the async version.

######v6.0.1

- Complete rewrite, removed a lot of code duplication, and restructured.

Breaking changes: 

- Everthing other than the MoveToState*, Fire* and configuration methods. 

######v7.0.0

- New `Diagnostics` API, for examination of current state, and possible trigger that could be handled.
- `PermitDynamic` now takes a func that returns `DynamicState<TState>` struct, instead of `TState` - `DynamicState.Create` and `DynamicState.NoTransition` helpers can be used to easily create them. 
- Numerous internal changes, and refactoring.
- Bugfix: Machine continues with execution on InvalidState.

Breaking changes:

- `CurrentPermittedStates` moved into `Diagnostics`.

######v7.2.0

- Introduce `Transition<TState, TTrigger>` object
- All state machine configurations now take the transition object. (Extensions methods provide act as overloads without the transition objects for convenience)
- New factory methods to be able to create custom state machines through the factory.

######v8.0

- Major cleanup
- Add proper runtime null checks on required public API surface.
- Enable usage of `null` as a value for a reference based `TState`.
- All internal APIs that take both `TState` and `TTrigger` as generic parameters are now consistent, and always take them in that order. Generally shouldn't be any breaking change unless you ended up using internal or obscure APIs.

