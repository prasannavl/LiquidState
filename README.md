LiquidState
===========

Efficient state machines for .NET with both synchronous and asynchronous support.  
Heavily inspired by the excellent state machine library [**Stateless**](https://github.com/nblumhardt/stateless) by 
**Nicholas Blumhardt.**

**NuGet:** 

> Install-Package LiquidState
  
Supported Platforms:
> PCL profile 259: Supports all platform including Xamarin.iOS and Xamarin.Android. 
  
######Why LiquidState:

- Fully supports async/await methods everywhere => `OnEntry`, `OnExit`, during trigger, and even trigger conditions.
- Builds a linked object graph internally during configuration making it a slightly faster and more efficient implementation.

######Why Stateless:

- Has hierarchal states.
- Supports dynamic states.
  
**Note:** 
I personally think switching states dynamically should never be the job of the machine. It should be a part of your domain logic, or better yet write a facade for the statemachine, making the intent very clear. 

**Banchmarks**

Comparing with Sync Machine of Stateless:

```
Benchmarks.StateMachines.StatelessTest .. Passed. Time taken: 23.293s

Synchronous StateMachine - Stateless => Time taken: 00:00:22.9425084

Benchmarks.StateMachines.LiquidStateSyncTest .. Passed. Time taken: 4.09s

Synchronous StateMachines - LiquidState => Time taken: 00:00:03.8172853
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
    machine.Fire(Trigger.Connect, "John Doe");
```

Now, let's take the same terrible example, but now do it **asynchronously**!  
(Mix and match synchronous code when you don't need asynchrony to avoid the costs.)

```
    // Note the "CreateAsyncConfiguration"
    var config = StateMachine.CreateAsyncConfiguration<State, Trigger>();

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

    await machine.Fire(Trigger.Talk);
    await machine.Fire(Trigger.Ring);
    await machine.Fire(Trigger.Connect, "John Doe");

```
