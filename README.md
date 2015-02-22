LiquidState
===========

Efficient state machines for .NET with both synchronous and asynchronous support.  
Heavily inspired by the excellent state machine library [**Stateless**](https://github.com/nblumhardt/stateless) by 
**Nicholas Blumhardt.**

**NuGet:** 

> Install-Package LiquidState
 


- **v3 is now a full rewrite with lock-free algorithms**, and fully thread-safe.
  
Supported Platforms:
> PCL profile 259: Supports all platforms including Xamarin.iOS and Xamarin.Android. 


**Available State Machines:**

1. **StateMachine** - Fully synchronous, and thread-safe.
2. **AwaitableStateMachine** - Logically synchronous, but accepts Task and async methods and can be awaited. Thread-safe.
3. **AsyncStateMachine** - Fully asynchronous, thread-safe and is queued by default. 

**Note:** When AsyncStateMachine or AwaitableStateMachines are used with synchronous (non-task returning) methods, it is almost as fast as the synchronous StateMachine (the penalty is really negligible anyway, unless you're running a 10 millions state changes per second).

######Why LiquidState:

- Fully supports async/await methods everywhere => `OnEntry`, `OnExit`, during trigger, and even trigger conditions.
- Builds a linked object graph internally during configuration making it a much faster and more efficient implementation than regular dictionary based implementations.
- MoveToState, to move freely between states, without triggers.

**Banchmarks**

Comparing with Sync Machine of Stateless for 10 million state changes:

```
Count: 10000000
Synchronous StateMachines - Stateless => Time taken: 00:00:21.5919263

Count: 10000000
Synchronous StateMachines - LiquidState => Time taken: 00:00:02.2374253

Count: 10000000
Synchronous StateMachines - LiquidState (Task/Async Awaitable) =>
Time taken: 00:00:13.9579566

Count: 10000000
Asynchronous StateMachines - LiquidState => Time taken: 00:00:19.4116743

```

Benchmarking code, and libraries at: [https://github.com/prasannavl/Benchmarks](https://github.com/prasannavl/Benchmarks)


**APIs:**

**IStateMachine:**
 
```c#
    public interface IStateMachine<TState, TTrigger>
    {
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        TState CurrentState { get; }
        bool IsEnabled { get; }
        bool IsInTransition { get; }
        bool CanHandleTrigger(TTrigger trigger);
        bool CanTransitionTo(TState state);
        void Fire<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, 
            TArgument argument);
        void Fire(TTrigger trigger);
        void MoveToState(TState state, 
            StateTransitionOption option = StateTransitionOption.Default);
        void Pause();
        void Resume();
        event Action<TTrigger, TState> UnhandledTriggerExecuted;
        event Action<TState, TState> StateChanged;
    }
```

**IAwaitableStateMachine:**

```c#
 public interface IAwaitableStateMachine<TState, TTrigger>
    {
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        TState CurrentState { get; }
        bool IsEnabled { get; }
        bool IsInTransition { get; }
        bool CanHandleTrigger(TTrigger trigger);
        bool CanTransitionTo(TState state);

        Task FireAsync<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument);

        Task FireAsync(TTrigger trigger);
        Task MoveToState(TState state, 
            StateTransitionOption option = StateTransitionOption.Default);
        void Pause();
        void Resume();

        event Action<TTrigger, TState> UnhandledTriggerExecuted;
        event Action<TState, TState> StateChanged;
    }
```

**How To Use:**

You only ever interact with the `StateMachine` static class. This is the factory for both configurations and the machines. 

**Step 1:** Create a configuration:

```c#
var config = StateMachine.CreateConfiguration<State, Trigger>();
```

or for awaitable, or async machine:

```c#
var config = StateMachine.CreateAwaitableConfiguration<State, Trigger>();
```

**Step 2:** Setup the machine configurations using the fluent API.

```
    config.Configure(State.Off)
        .OnEntry(() => Console.WriteLine("OnEntry of Off"))
        .OnExit(() => Console.WriteLine("OnExit of Off"))
        .PermitReentry(Trigger.TurnOn)
        .Permit(Trigger.Ring, State.Ringing, 
                () => { Console.WriteLine("Attempting to ring"); })
        .Permit(Trigger.Connect, State.Connected, 
                () => { Console.WriteLine("Connecting"); });
                
    var connectTriggerWithParameter = 
                config.SetTriggerParameter<string>(Trigger.Connect);

    config.Configure(State.Ringing)
        .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
        .OnExit(() => Console.WriteLine("OnExit of Ringing"))
        .Permit(connectTriggerWithParameter, State.Connected,
                name => { Console.WriteLine("Attempting to connect to {0}", name); })
        .Permit(Trigger.Talk, State.Talking, 
                () => { Console.WriteLine("Attempting to talk"); });
```

**Step 3:** Create the machine with the configuration:

```c#
var machine = StateMachine.Create(State.Ringing, config);
```

**Full examples:** 

A synchronous machine example:

```c#
    var config = StateMachine.CreateConfiguration<State, Trigger>();

    config.Configure(State.Off)
        .OnEntry(() => Console.WriteLine("OnEntry of Off"))
        .OnExit(() => Console.WriteLine("OnExit of Off"))
        .PermitReentry(Trigger.TurnOn)
        .Permit(Trigger.Ring, State.Ringing, 
                () => { Console.WriteLine("Attempting to ring"); })
        .Permit(Trigger.Connect, State.Connected, 
                () => { Console.WriteLine("Connecting"); });

    var connectTriggerWithParameter = 
                config.SetTriggerParameter<string>(Trigger.Connect);

    config.Configure(State.Ringing)
        .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
        .OnExit(() => Console.WriteLine("OnExit of Ringing"))
        .Permit(connectTriggerWithParameter, State.Connected,
                name => { Console.WriteLine("Attempting to connect to {0}", name); })
        .Permit(Trigger.Talk, State.Talking, 
                () => { Console.WriteLine("Attempting to talk"); });

    config.Configure(State.Connected)
        .OnEntry(() => Console.WriteLine("AOnEntry of Connected"))
        .OnExit(() => Console.WriteLine("AOnExit of Connected"))
        .PermitReentry(Trigger.Connect)
        .Permit(Trigger.Talk, State.Talking, 
              () => { Console.WriteLine("Attempting to talk"); })
        .Permit(Trigger.TurnOn, State.Off, 
              () => { Console.WriteLine("Turning off"); });


    config.Configure(State.Talking)
        .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
        .OnExit(() => Console.WriteLine("OnExit of Talking"))
        .Permit(Trigger.TurnOn, State.Off, 
              () => { Console.WriteLine("Turning off"); })
        .Permit(Trigger.Ring, State.Ringing, 
              () => { Console.WriteLine("Attempting to ring"); });

    var machine = StateMachine.Create(State.Ringing, config);

    machine.Fire(Trigger.Talk);
    machine.Fire(Trigger.Ring);
    machine.Fire(connectTriggerWithParameter, "John Doe");
```

Now, let's take the same dumb, and terrible example, but now do it **asynchronously**!  
(Mix and match synchronous code when you don't need asynchrony to avoid the costs.)

```c#
    // Note the "CreateAsyncConfiguration"
    var config = StateMachine.CreateAwaitableConfiguration<State, Trigger>();

    config.Configure(State.Off)
        .OnEntry(async () => Console.WriteLine("OnEntry of Off"))
        .OnExit(async () => Console.WriteLine("OnExit of Off"))
        .PermitReentry(Trigger.TurnOn)
        .Permit(Trigger.Ring, State.Ringing, 
              async () => { Console.WriteLine("Attempting to ring"); })
        .Permit(Trigger.Connect, State.Connected, 
              async () => { Console.WriteLine("Connecting"); });

    var connectTriggerWithParameter = 
                config.SetTriggerParameter<string>(Trigger.Connect);

    config.Configure(State.Ringing)
        .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
        .OnExit(() => Console.WriteLine("OnExit of Ringing"))
        .Permit(connectTriggerWithParameter, State.Connected,
                name => { Console.WriteLine("Attempting to connect to {0}", name); })
        .Permit(Trigger.Talk, State.Talking, 
                () => { Console.WriteLine("Attempting to talk"); });

    config.Configure(State.Connected)
        .OnEntry(async () => Console.WriteLine("AOnEntry of Connected"))
        .OnExit(async () => Console.WriteLine("AOnExit of Connected"))
        .PermitReentry(Trigger.Connect)
        .Permit(Trigger.Talk, State.Talking, 
              async () => { Console.WriteLine("Attempting to talk"); })
        .Permit(Trigger.TurnOn, State.Off, 
              async () => { Console.WriteLine("Turning off"); });

    config.Configure(State.Talking)
        .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
        .OnExit(() => Console.WriteLine("OnExit of Talking"))
        .Permit(Trigger.TurnOn, State.Off, 
              () => { Console.WriteLine("Turning off"); })
        .Permit(Trigger.Ring, State.Ringing, 
              () => { Console.WriteLine("Attempting to ring"); });

    var machine = StateMachine.Create(State.Ringing, config);

    await machine.FireAsync(Trigger.Talk);
    await machine.FireAsync(Trigger.Ring);
    await machine.FireAsync(connectTriggerWithParameter, "John Doe");
```

**Release notes:**

######v1.1

- Added removable invalid trigger event handler by default
- Added `Ignore` and `IgnoreIf` to configurations

######v1.2

- Added generic parameterized triggers

######v1.3-beta

- Added QueuedAsyncStateMachine with customizable synchronization context, and queued Fire semantics.

######v.2.0-beta

- Changed AsyncStateMachine to AwaitableStateMachine
- Changed QueuedAsyncStateMachine to AsyncStateMachine
- AwaitableStateMachine are logically synchronous but accepts Task and async methods and can be awaited.
- AsyncStateMachine is fully asynchronous and dispatched onto the instantiated synchronization context, and is thread safe.
- AsyncStateMachines are queued by default.
- All except AsyncStateMachines will throw InvalidOperationException if attempted to Fire while a transition is in progress.

######v.2.1-beta

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



