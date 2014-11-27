using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LiquidState.Sample
{
    class Program
    {
        private enum Trigger
        {
            TurnOn,
            Ring,
            Connect,
            Talk,
        }

        private enum State
        {
            Off,
            Ringing,
            Connected,
            Talking,
        }

        static void Main(string[] args)
        {
            SyncMachineExample();
            AsyncMachineExample().Wait();
            
            Console.ReadLine();
        }


        static void SyncMachineExample()
        {
            var config = StateMachine.CreateConfiguration<State, Trigger>();

            config.Configure(State.Off)
                .OnEntry(() => Console.WriteLine("OnEntry of Off"))
                .OnExit(() => Console.WriteLine("OnExit of Off"))
                .PermitReentry(Trigger.TurnOn)
                .Permit(Trigger.Ring, State.Ringing, () => { Console.WriteLine("Attempting to ring"); })
                .Permit(Trigger.Connect, State.Connected, () => { Console.WriteLine("Connecting"); });
            var connectTriggerWithParameter = config.SetTriggerParameter<string>(Trigger.Connect);

            config.Configure(State.Ringing)
                .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
                .OnExit(() => Console.WriteLine("OnExit of Ringing"))
                .Permit(connectTriggerWithParameter, State.Connected,
                        name => { Console.WriteLine("Attempting to connect to {0}", name); })
                .Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); });

            config.Configure(State.Connected)
                .OnEntry(() => Console.WriteLine("AOnEntry of Connected"))
                .OnExit(() => Console.WriteLine("AOnExit of Connected"))
                .PermitReentry(Trigger.Connect)
                .Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); })
                .Permit(Trigger.TurnOn, State.Off, () => { Console.WriteLine("Turning off"); });


            config.Configure(State.Talking)
                .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
                .OnExit(() => Console.WriteLine("OnExit of Talking"))
                .Permit(Trigger.TurnOn, State.Off, () => { Console.WriteLine("Turning off"); })
                .Permit(Trigger.Ring, State.Ringing, () => { Console.WriteLine("Attempting to ring"); });

            var machine = StateMachine.Create(State.Ringing, config);

            machine.Fire(Trigger.Talk);
            machine.Fire(Trigger.Ring);
            machine.Fire(Trigger.Connect, "John Doe");
        }

        static async Task AsyncMachineExample()
        {
            // Note the "CreateAsyncConfiguration"
            var config = StateMachine.CreateAsyncConfiguration<State, Trigger>();

            config.Configure(State.Off)
                .OnEntry(async () => Console.WriteLine("OnEntry of Off"))
                .OnExit(async () => Console.WriteLine("OnExit of Off"))
                .PermitReentry(Trigger.TurnOn)
                .Permit(Trigger.Ring, State.Ringing, async () => { Console.WriteLine("Attempting to ring"); })
                .Permit(Trigger.Connect, State.Connected, async () => { Console.WriteLine("Connecting"); });

            var connectTriggerWithParameter = config.SetTriggerParameter<string>(Trigger.Connect);

            config.Configure(State.Ringing)
                .OnEntry(() => Console.WriteLine("OnEntry of Ringing"))
                .OnExit(() => Console.WriteLine("OnExit of Ringing"))
                .Permit(connectTriggerWithParameter, State.Connected,
                        name => { Console.WriteLine("Attempting to connect to {0}", name); }).Permit(Trigger.Talk, State.Talking, () => { Console.WriteLine("Attempting to talk"); });

            config.Configure(State.Connected)
                .OnEntry(async () => Console.WriteLine("AOnEntry of Connected"))
                .OnExit(async () => Console.WriteLine("AOnExit of Connected"))
                .PermitReentry(Trigger.Connect)
                .Permit(Trigger.Talk, State.Talking, async () => { Console.WriteLine("Attempting to talk"); })
                .Permit(Trigger.TurnOn, State.Off, async () => { Console.WriteLine("Turning off"); });

            config.Configure(State.Talking)
                .OnEntry(() => Console.WriteLine("OnEntry of Talking"))
                .OnExit(() => Console.WriteLine("OnExit of Talking"))
                .Permit(Trigger.TurnOn, State.Off, () => { Console.WriteLine("Turning off"); })
                .Permit(Trigger.Ring, State.Ringing, () => { Console.WriteLine("Attempting to ring"); });

            var machine = StateMachine.Create(State.Ringing, config);

            await machine.Fire(Trigger.Talk);
            await machine.Fire(Trigger.Ring);
            await machine.Fire(Trigger.Connect, "John Doe");

        }
    }
}
