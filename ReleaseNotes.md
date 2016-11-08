#v8.2

- Refactor project structure to move to NETCore: NetStandard 1.0
- Revamp the nuget build process
- Update coding conventions and cleanup

#v8.0

- Major cleanup
- Add proper runtime null checks on required public API surface.
- Enable usage of `null` as a value for a reference based `TState`.
- All internal APIs that take both `TState` and `TTrigger` as generic parameters are now consistent, and always take them in that order. Generally shouldn't be any breaking change unless you ended up using internal or obscure APIs.

#v7.2.0

- Introduce `Transition<TState, TTrigger>` object
- All state machine configurations now take the transition object. (Extensions methods act as overloads without the transition objects for convenience)
- New factory methods to be able to create custom state machines through the factory.

#v7.0.0

- New `Diagnostics` API, for examination of current state, and possible trigger that could be handled.
- `PermitDynamic` now takes a func that returns `DynamicState<TState>` struct, instead of `TState` - `DynamicState.Create` and `DynamicState.NoTransition` helpers can be used to easily create them. 
- Numerous internal changes, and refactoring.
- Bugfix: Machine continues with execution on InvalidState.

Breaking changes:

- `CurrentPermittedStates` moved into `Diagnostics`.

#v6.0.1

- Complete rewrite, removed a lot of code duplication, and restructured.

Breaking changes: 

- Everthing other than the MoveToState*, Fire* and configuration methods. 

#v5.0.0

- Add `Task<bool> CanHandleTriggerAsync(TTrigger trigger);` to IAwaitableStateMachine
- Make CanHandleTrigger check for predicates, if present.

Breaking changes:

- Removed `bool CanHandleTrigger(TTrigger trigger)` from it IAwaitableStateMachine. Has been replaced by the async version.

#v4.1.1

- Fix CanHandleTrigger()

#v4.1

- Added new AwaitableStateMachineWithScheduler which is just a simple AwaitableStateMachine that accepts a TaskScheduler to directly execute them without the queuing, or thread-safety overheads.

#v4.0

- Rename StateMachine to StateMachineFactory to be more approriate.
- Add a new BlockingStateMachine for a synchronous machine that processes sequentially by blocking, enabled with a blocking parameter on creation.
- StateMachineFactory now returns IStateMachine instead of the concrete class for synchronous state machines also.

#v3.2.1

- Minor perf improvements
- Enforce strict semantic versioning

#v3.2.0

- Breaking change: The configuration API has been renamed from
  **x.Configure(TState)** to **x.ForState(TState)**
  (Since, its only small literal change more in line with the semantics involved, the major version number has been retained)

#v3.1.0

- Version bump, with removal of all dependencies.
- Fix: Code contracts rewrite was missing on Release builds.

#v3.0.6

- Drop dependency on System.Collections.Immutable

#v3.0.5

- v3.0.5, is now transitioning to its own Immutable Collections, and as a side effect, it is also out of beta, and is  now fully stable.

#v3.0.4-beta

- Internal implementation changes.
- Remove Stop method from all machine, as the complexity it bring about is simply unnecessary since the functionality can easily be implemented with an additional state.

#v3.0.3-beta

- Cleanup release

#v3.0.2-beta

- Minor changes and fixes

#v3.0.1-beta

- Fix: MoveToState on AsyncStateMachine was broken due to wrong internal method being called.

#v3.0.0-beta

- Complete re-write of all the machines, with Interlocked routines. All three machines are lock-free and thread-safe.
- Add MoveToState(TState, StateTransitionOption) to all three state machines to move freely between states.

#v2.1.6-beta

- Remove dispatcher on AsyncStateMachine. Dispatching and synchronization be easily handled within the delegates itself, and not by the machine.

#v2.1.5-beta

- Fix: Async state machine queues attempts to execute concurrently (throwing an exception preventing it) when it enters a queue, and not awaited

#v2.1.4-beta

- Remove FluidStateMachine in favor of SimpleStateMachine (not available yet)

#v2.1.2-beta

- Allow null states in FluidStateMachine
- Allow zero configuration start for FluidStateMachine

#v2.1.1-beta

- Improve fluid flow dynamics for FluidStateMachine

#v2.1.0-beta

- Critical Fix: All the state machines never reset the IsRunning value on error or unhandled state, leading to the machine being dormant
- More robust error handling
- Removed StateMachine.ReConfigure. Retain the configuration, and reconfigure it any time to modify a live state machine.
- FluidStateMachine added

#v2.0.0-beta

- Changed AsyncStateMachine to AwaitableStateMachine
- Changed QueuedAsyncStateMachine to AsyncStateMachine
- AwaitableStateMachine are logically synchronous but accepts Task and async methods and can be awaited.
- AsyncStateMachine is fully asynchronous and dispatched onto the instantiated synchronization context, and is thread safe.
- AsyncStateMachines are queued by default.
- All except AsyncStateMachines will throw InvalidOperationException if attempted to Fire while a transition is in progress.

#v1.3.0-beta

- Added QueuedAsyncStateMachine with customizable synchronization context, and queued Fire semantics.

#v1.2.0

- Added generic parameterized triggers

#v1.1.0

- Added removable invalid trigger event handler by default
- Added `Ignore` and `IgnoreIf` to configurations
