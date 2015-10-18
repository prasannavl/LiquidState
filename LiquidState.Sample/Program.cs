// Author: Prasanna V. Loganathar
// Created: 2:16 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LiquidState.Sample
{
    internal class Program
    {
        public static void LiquidStateSyncTest()
        {
            var config = StateMachineFactory.CreateConfiguration<State, Trigger>();

            config.ForState(State.Off)
                .OnEntry(() => Console.WriteLine("OnEntry of Off"))
                .OnExit(() => Console.WriteLine("OnExit of Off"))
                .PermitReentry(Trigger.TurnOff)
                .Permit(Trigger.Ring, State.Ringing, () => { Console.WriteLine("Attempting to ring"); })
                .Permit(Trigger.Connect, State.Connected, () => { Console.WriteLine("Connecting"); });

            var connectTriggerWithParameter = config.SetTriggerParameter<string>(Trigger.Connect);

            config.ForState(State.Ringing)
                .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
                .OnExit(() => Console.WriteLine("OnExit of Ringing"))
                .Permit(connectTriggerWithParameter, State.Connected,
                    name => { Console.WriteLine("Attempting to connect to " + name); })
                .Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); });

            config.ForState(State.Connected)
                .OnEntry(() => Console.WriteLine("AOnEntry of Connected"))
                .OnExit(() => Console.WriteLine("AOnExit of Connected"))
                .PermitReentry(Trigger.Connect)
                .Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); })
                .Permit(Trigger.TurnOff, State.Off, () => { Console.WriteLine("Turning off"); });


            config.ForState(State.Talking)
                .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
                .OnExit(() => Console.WriteLine("OnExit of Talking"))
                .Permit(Trigger.TurnOff, State.Off, () => { Console.WriteLine("Turning off"); })
                .PermitDynamic(Trigger.Ring, () => DynamicState.Create(State.Ringing),
                    () => { Console.WriteLine("Attempting to ring"); });

            var machine = StateMachineFactory.Create(State.Ringing, config);

            machine.Fire(Trigger.Talk);
            machine.Fire(Trigger.Ring);
        }

        public static void LiquidStateAwaitableSyncTest()
        {
            var config = StateMachineFactory.CreateAwaitableConfiguration<State, Trigger>();

            config.ForState(State.Off)
                .OnEntry(async () => Console.WriteLine("OnEntry of Off"))
                .OnExit(async () => Console.WriteLine("OnExit of Off"))
                .PermitReentry(Trigger.TurnOff)
                .Permit(Trigger.Ring, State.Ringing, async () => { Console.WriteLine("Attempting to ring"); })
                .Permit(Trigger.Connect, State.Connected, async () => { Console.WriteLine("Connecting"); });

            var connectTriggerWithParameter = config.SetTriggerParameter<string>(Trigger.Connect);

            config.ForState(State.Ringing)
                .OnEntry(async () => Console.WriteLine("OnEntry of Ringing"))
                .OnExit(async () => Console.WriteLine("OnExit of Ringing"))
                .Permit(connectTriggerWithParameter, State.Connected,
                    async name => { Console.WriteLine("Attempting to connect to " + name); })
                .Permit(Trigger.Talk, State.Talking, async () => { Console.WriteLine("Attempting to talk"); });

            config.ForState(State.Connected)
                .OnEntry(async () => Console.WriteLine("AOnEntry of Connected"))
                .OnExit(async () => Console.WriteLine("AOnExit of Connected"))
                .PermitReentry(Trigger.Connect)
                .Permit(Trigger.Talk, State.Talking, async () => { Console.WriteLine("Attempting to talk"); })
                .Permit(Trigger.TurnOff, State.Off, async () => { Console.WriteLine("Turning off"); });


            config.ForState(State.Talking)
                .OnEntry(async () => Console.WriteLine("OnEntry of Talking"))
                .OnExit(async () => Console.WriteLine("OnExit of Talking"))
                .Permit(Trigger.TurnOff, State.Off, async () => { Console.WriteLine("Turning off"); })
                .PermitDynamic(Trigger.Ring, () => DynamicState.Create(State.Ringing),
                    async () => { Console.WriteLine("Attempting to ring"); });

            var machine = StateMachineFactory.Create(State.Ringing, config, queued: false);

            machine.FireAsync(Trigger.Talk).Wait();
            machine.FireAsync(Trigger.Ring).Wait();
            machine.FireAsync(Trigger.Talk).Wait();
        }

        public static async Task LiquidStateAsyncTest()
        {
            var config = StateMachineFactory.CreateAwaitableConfiguration<State, Trigger>();

            config.ForState(State.Off)
                .OnEntry(async () => Console.WriteLine("OnEntry of Off"))
                .OnExit(async () => Console.WriteLine("OnExit of Off"))
                .PermitReentry(Trigger.TurnOff)
                .Permit(Trigger.Ring, State.Ringing, async () => { Console.WriteLine("Attempting to ring"); })
                .Permit(Trigger.Connect, State.Connected, async () => { Console.WriteLine("Connecting"); });

            var connectTriggerWithParameter = config.SetTriggerParameter<string>(Trigger.Connect);

            config.ForState(State.Ringing)
                .OnEntry(async () => Console.WriteLine("OnEntry of Ringing"))
                .OnExit(async () => Console.WriteLine("OnExit of Ringing"))
                .Permit(connectTriggerWithParameter, State.Connected,
                    async name => { Console.WriteLine("Attempting to connect to " + name); })
                .Permit(Trigger.Talk, State.Talking, async () => { Console.WriteLine("Attempting to talk"); })
                .PermitReentry(Trigger.Ring);

            config.ForState(State.Connected)
                .OnEntry(async () => Console.WriteLine("AOnEntry of Connected"))
                .OnExit(async () => Console.WriteLine("AOnExit of Connected"))
                .PermitReentry(Trigger.Connect)
                .Permit(Trigger.Talk, State.Talking, async () => { Console.WriteLine("Attempting to talk"); })
                .Permit(Trigger.TurnOff, State.Off, async () => { Console.WriteLine("Turning off"); });


            config.ForState(State.Talking)
                .OnEntry(async () => Console.WriteLine("OnEntry of Talking"))
                .OnExit(async () => Console.WriteLine("OnExit of Talking"))
                .Permit(Trigger.TurnOff, State.Off, async () => { Console.WriteLine("Turning off"); })
                .Permit(Trigger.Ring, State.Ringing, async () => { Console.WriteLine("Attempting to ring"); })
                .PermitReentry(Trigger.Talk);


            var machine = StateMachineFactory.Create(State.Ringing, config, queued: true);

            var sw = Stopwatch.StartNew();

            var i = 0;
            for (i = 0; i < 1; i++)
            {
                try
                {
                    var t1 = Task.Run(async () =>
                    {
                        await machine.FireAsync(Trigger.Talk);
                        await machine.FireAsync(Trigger.Ring);
                    });

                    var t2 = Task.Run(async () =>
                    {
                        var i1 = machine.FireAsync(Trigger.Talk);
                        var i2 = machine.FireAsync(Trigger.Ring);
                        await Task.WhenAll(i1, i2);
                    });

                    var t3 = Task.Run(async () =>
                    {
                        var i1 = machine.FireAsync(Trigger.Talk);
                        var i2 = machine.FireAsync(Trigger.Ring);
                        await Task.WhenAll(i1, i2);
                    });

                    await Task.WhenAll(t1, t2, t3);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void Main(string[] args)
        {
            SyncMachineExample();
            AsyncMachineExample().Wait();
            LiquidStateSyncTest();
            LiquidStateAwaitableSyncTest();
            Task.Run(async () =>
            {
                try
                {
                    await LiquidStateAsyncTest();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }).Wait();
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void SyncMachineExample()
        {
            var config = StateMachineFactory.CreateConfiguration<State, Trigger>();

            config.ForState(State.Off)
                .OnEntry(() => Console.WriteLine("OnEntry of Off"))
                .OnExit(() => Console.WriteLine("OnExit of Off"))
                .PermitReentry(Trigger.TurnOff)
                .Permit(Trigger.Ring, State.Ringing, () => { Console.WriteLine("Attempting to ring"); })
                .Permit(Trigger.Connect, State.Connected, () => { Console.WriteLine("Connecting"); });
            var connectTriggerWithParameter = config.SetTriggerParameter<string>(Trigger.Connect);

            config.ForState(State.Ringing)
                .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
                .OnExit(() => Console.WriteLine("OnExit of Ringing"))
                .Permit(connectTriggerWithParameter, State.Connected,
                    name => { Console.WriteLine("Attempting to connect to {0}", name); })
                .Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); });

            config.ForState(State.Connected)
                .OnEntry(() => Console.WriteLine("AOnEntry of Connected"))
                .OnExit(() => Console.WriteLine("AOnExit of Connected"))
                .PermitReentry(Trigger.Connect)
                .Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); })
                .Permit(Trigger.TurnOff, State.Off, () => { Console.WriteLine("Turning off"); });


            config.ForState(State.Talking)
                .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
                .OnExit(() => Console.WriteLine("OnExit of Talking"))
                .Permit(Trigger.TurnOff, State.Off, () => { Console.WriteLine("Turning off"); })
                .Permit(Trigger.Ring, State.Ringing, () => { Console.WriteLine("Attempting to ring"); });

            var machine = StateMachineFactory.Create(State.Ringing, config);

            machine.Fire(Trigger.Talk);
            machine.Fire(Trigger.Ring);
            machine.Fire(connectTriggerWithParameter, "John Doe");
        }

        private static async Task AsyncMachineExample()
        {
            // Note the "CreateAsyncConfiguration"
            var config = StateMachineFactory.CreateAwaitableConfiguration<State, Trigger>();

            config.ForState(State.Off)
                .OnEntry(async () => Console.WriteLine("OnEntry of Off"))
                .OnExit(async () => Console.WriteLine("OnExit of Off"))
                .PermitReentry(Trigger.TurnOff)
                .Permit(Trigger.Ring, State.Ringing, async () => { Console.WriteLine("Attempting to ring"); })
                .Permit(Trigger.Connect, State.Connected, async () => { Console.WriteLine("Connecting"); });

            var connectTriggerWithParameter = config.SetTriggerParameter<string>(Trigger.Connect);

            config.ForState(State.Ringing)
                .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
                .OnExit(() => Console.WriteLine("OnExit of Ringing"))
                .Permit(connectTriggerWithParameter, State.Connected,
                    name => { Console.WriteLine("Attempting to connect to {0}", name); })
                .Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); });

            config.ForState(State.Connected)
                .OnEntry(async () => Console.WriteLine("AOnEntry of Connected"))
                .OnExit(async () => Console.WriteLine("AOnExit of Connected"))
                .PermitReentry(Trigger.Connect)
                .Permit(Trigger.Talk, State.Talking, async () => { Console.WriteLine("Attempting to talk"); })
                .Permit(Trigger.TurnOff, State.Off, async () => { Console.WriteLine("Turning off"); });

            config.ForState(State.Talking)
                .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
                .OnExit(() => Console.WriteLine("OnExit of Talking"))
                .Permit(Trigger.TurnOff, State.Off, () => { Console.WriteLine("Turning off"); })
                .Permit(Trigger.Ring, State.Ringing, () => { Console.WriteLine("Attempting to ring"); });

            var machine = StateMachineFactory.Create(State.Ringing, config);

            await machine.FireAsync(Trigger.Talk);
            await machine.FireAsync(Trigger.Ring);
            await machine.FireAsync(connectTriggerWithParameter, "John Doe");
        }

        private enum State
        {
            Off,
            Ringing,
            Connected,
            Talking,
        }

        private enum Trigger
        {
            TurnOff,
            Ring,
            Connect,
            Talk,
        }
    }
}
