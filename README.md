LiquidState
===========

Efficient state machines for .NET with both synchronous and asynchronous support.  
Heavily inspired by the excellent state machine library [**Stateless**](https://github.com/nblumhardt/stateless) by 
**Nicholas Blumhardt.**

**NuGet:** 

> Install-Package LiquidState -Pre

Note: The latest version is beta (pre-release) due to the dependency on System.Collections.Immutable, which is still under beta.), and is otherwise stable. The release notes are at the end.
  
Supported Platforms:
> PCL profile 259: Supports all platforms including Xamarin.iOS and Xamarin.Android. 


**Available State Machines:**

1. **StateMachine** - Fully synchronous.
2. **AwaitableStateMachine** - Logically synchronous, but accepts Task and async methods and can be awaited.
3. **AsyncStateMachine** - Fully asynchronous, and is queued by default.
  
######Why LiquidState:

- Fully supports async/await methods everywhere => `OnEntry`, `OnExit`, during trigger, and even trigger conditions.
- Builds a linked object graph internally during configuration making it a much faster and more efficient implementation.

######Why Stateless:

- Has hierarchal states.
- Supports dynamic states.
  
**Note:** 
I personally think switching states dynamically should never be the job of the machine. It should be a part of your domain logic, or better yet write a facade for the statemachine, making the intent very clear. 

**Banchmarks**

Comparing with Sync Machine of Stateless for 10 million state changes:

```
Benchmarks.StateMachines.StatelessTest .. Passed. Time taken: 21.923s

Count: 10000000
Synchronous StateMachines - Stateless => Time taken: 00:00:21.5919263

Benchmarks.StateMachines.LiquidStateSyncTest .. Passed. Time taken: 2.483s

Count: 10000000
Synchronous StateMachines - LiquidState => Time taken: 00:00:02.2374253

Benchmarks.StateMachines.LiquidStateAwaitableSyncTest .. Passed. Time taken: 14.334s

Count: 10000000
Synchronous StateMachines - LiquidState (Task/Async Awaitable) => Time taken: 00:00:13.9579566

Benchmarks.StateMachines.LiquidStateAsyncTest .. Passed. Time taken: 19.714s

Count: 10000000
Asynchronous StateMachines - LiquidState => Time taken: 00:00:19.4116743

```

Benchmarking code, and libraries at: [https://github.com/prasannavl/Benchmarks](https://github.com/prasannavl/Benchmarks)

**Example:** 

A terrible example: 

```
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

Now, let's take the same terrible example, but now do it **asynchronously**!  
(Mix and match synchronous code when you don't need asynchrony to avoid the costs.)

```
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

######v.2.0-beta

- Changed AsyncStateMachine to AwaitableStateMachine
- Changed QueuedAsyncStateMachine to AsyncStateMachine
- AwaitableStateMachine are logically synchronous but accepts Task and async methods and can be awaited.
- AsyncStateMachine is fully asynchronous and dispatched onto the instantiated synchronization context, and is thread safe.
- AsyncStateMachines are queued by default.
- All except AsyncStateMachines will throw InvalidOperationException if attempted to Fire while a transition is in progress.

######v1.3-beta

- Added QueuedAsyncStateMachine with customizable synchronization context, and queued Fire semantics.

######v1.2

- Added generic parameterized triggers

*Breaking changes:*

- Non-parameterized argument (object) has been removed in favour of parameterized triggers

######v1.1

- Added removable invalid trigger event handler by default
- Added `Ignore` and `IgnoreIf` to configurations

*Breaking changes:*  

- Invalid trigger handler takes `<TTrigger, TState>`


