// Author: Prasanna V. Loganathar
// Created: 5:07 PM 22-02-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using LiquidState.Common;

namespace LiquidState.Scaffold
{
    internal interface IImmutableStack<T> : IEnumerable<T>, IEnumerable
    {
        bool IsEmpty { get; }
        IImmutableStack<T> Clear();
        T Peek();
        IImmutableStack<T> Pop();
        IImmutableStack<T> Push(T value);
    }

    internal interface IImmutableQueue<T> : IEnumerable<T>, IEnumerable
    {
        bool IsEmpty { get; }
        IImmutableQueue<T> Clear();
        IImmutableQueue<T> Dequeue();
        IImmutableQueue<T> Enqueue(T value);
        T Peek();
    }


    internal static class ImmutableQueue
    {
        public static ImmutableQueue<T> Create<T>()
        {
            return ImmutableQueue<T>.Empty;
        }

        public static ImmutableQueue<T> Create<T>(T item)
        {
            return ImmutableQueue<T>.Empty.Enqueue(item);
        }

        public static ImmutableQueue<T> CreateRange<T>(IEnumerable<T> items)
        {
            Requires.NotNull(items, "items");

            var immutableQueue = ImmutableQueue<T>.Empty;
            foreach (var obj in items)
                immutableQueue = immutableQueue.Enqueue(obj);
            return immutableQueue;
        }

        public static ImmutableQueue<T> Create<T>(params T[] items)
        {
            Requires.NotNull(items, "items");
            var immutableQueue = ImmutableQueue<T>.Empty;
            foreach (var obj in items)
                immutableQueue = immutableQueue.Enqueue(obj);
            return immutableQueue;
        }

        public static IImmutableQueue<T> Dequeue<T>(this IImmutableQueue<T> queue, out T value)
        {
            Requires.NotNull(queue, "queue");
            value = queue.Peek();
            return queue.Dequeue();
        }
    }

    [DebuggerDisplay("IsEmpty = {IsEmpty}")]
    internal sealed class ImmutableQueue<T> : IImmutableQueue<T>
    {
        private static readonly ImmutableQueue<T> EmptyField = new ImmutableQueue<T>(ImmutableStack<T>.Empty,
            ImmutableStack<T>.Empty);

        private readonly ImmutableStack<T> backwards;
        private readonly ImmutableStack<T> forwards;
        private ImmutableStack<T> backwardsReversed;

        private ImmutableQueue(ImmutableStack<T> forward, ImmutableStack<T> backward)
        {
            Requires.NotNull(forward, "forward");
            Requires.NotNull(backward, "backward");

            forwards = forward;
            backwards = backward;
            backwardsReversed = null;
        }

        public static ImmutableQueue<T> Empty
        {
            get { return EmptyField; }
        }

        private ImmutableStack<T> BackwardsReversed
        {
            get
            {
                if (backwardsReversed == null)
                    backwardsReversed = backwards.Reverse();
                return backwardsReversed;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("InvalidEmptyOperation");
            return forwards.Peek();
        }

        IImmutableQueue<T> IImmutableQueue<T>.Clear()
        {
            return Clear();
        }

        IImmutableQueue<T> IImmutableQueue<T>.Enqueue(T value)
        {
            return Enqueue(value);
        }

        IImmutableQueue<T> IImmutableQueue<T>.Dequeue()
        {
            return Dequeue();
        }

        public bool IsEmpty
        {
            get
            {
                if (forwards.IsEmpty)
                    return backwards.IsEmpty;
                return false;
            }
        }

        public ImmutableQueue<T> Clear()
        {
            return Empty;
        }

        public ImmutableQueue<T> Enqueue(T value)
        {
            if (IsEmpty)
                return new ImmutableQueue<T>(ImmutableStack<T>.Empty.Push(value), ImmutableStack<T>.Empty);
            return new ImmutableQueue<T>(forwards, backwards.Push(value));
        }

        public ImmutableQueue<T> Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("InvalidEmptyOperation");
            var forward = forwards.Pop();
            if (!forward.IsEmpty)
                return new ImmutableQueue<T>(forward, backwards);
            if (backwards.IsEmpty)
                return Empty;
            return new ImmutableQueue<T>(BackwardsReversed, ImmutableStack<T>.Empty);
        }

        public ImmutableQueue<T> Dequeue(out T value)
        {
            value = Peek();
            return Dequeue();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private class EnumeratorObject : IEnumerator<T>, IEnumerator, IDisposable
        {
            private readonly ImmutableQueue<T> originalQueue;
            private ImmutableStack<T> remainingForwardsStack;
            private ImmutableStack<T> remainingBackwardsStack;
            private bool disposed;

            internal EnumeratorObject(ImmutableQueue<T> queue)
            {
                originalQueue = queue;
            }

            public void Dispose()
            {
                disposed = true;
            }

            public bool MoveNext()
            {
                ThrowIfDisposed();
                if (remainingForwardsStack == null)
                {
                    remainingForwardsStack = originalQueue.forwards;
                    remainingBackwardsStack = originalQueue.BackwardsReversed;
                }
                else if (!remainingForwardsStack.IsEmpty)
                    remainingForwardsStack = remainingForwardsStack.Pop();
                else if (!remainingBackwardsStack.IsEmpty)
                    remainingBackwardsStack = remainingBackwardsStack.Pop();
                if (remainingForwardsStack.IsEmpty)
                    return !remainingBackwardsStack.IsEmpty;
                return true;
            }

            public void Reset()
            {
                ThrowIfDisposed();
                remainingBackwardsStack = null;
                remainingForwardsStack = null;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public T Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (remainingForwardsStack == null)
                        throw new InvalidOperationException();
                    if (!remainingForwardsStack.IsEmpty)
                        return remainingForwardsStack.Peek();
                    if (!remainingBackwardsStack.IsEmpty)
                        return remainingBackwardsStack.Peek();
                    throw new InvalidOperationException();
                }
            }

            private void ThrowIfDisposed()
            {
                if (!disposed)
                    return;
                Requires.FailObjectDisposed(this);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator
        {
            private readonly ImmutableQueue<T> originalQueue;
            private ImmutableStack<T> remainingForwardsStack;
            private ImmutableStack<T> remainingBackwardsStack;

            internal Enumerator(ImmutableQueue<T> queue)
            {
                originalQueue = queue;
                remainingForwardsStack = null;
                remainingBackwardsStack = null;
            }

            public T Current
            {
                get
                {
                    if (remainingForwardsStack == null)
                        throw new InvalidOperationException();
                    if (!remainingForwardsStack.IsEmpty)
                        return remainingForwardsStack.Peek();
                    if (!remainingBackwardsStack.IsEmpty)
                        return remainingBackwardsStack.Peek();
                    throw new InvalidOperationException();
                }
            }

            public bool MoveNext()
            {
                if (remainingForwardsStack == null)
                {
                    remainingForwardsStack = originalQueue.forwards;
                    remainingBackwardsStack = originalQueue.BackwardsReversed;
                }
                else if (!remainingForwardsStack.IsEmpty)
                    remainingForwardsStack = remainingForwardsStack.Pop();
                else if (!remainingBackwardsStack.IsEmpty)
                    remainingBackwardsStack = remainingBackwardsStack.Pop();
                if (remainingForwardsStack.IsEmpty)
                    return !remainingBackwardsStack.IsEmpty;
                return true;
            }
        }
    }

    [DebuggerDisplay("IsEmpty = {IsEmpty}; Top = {head}")]
    internal sealed class ImmutableStack<T> : IImmutableStack<T>
    {
        private static readonly ImmutableStack<T> EmptyField = new ImmutableStack<T>();
        private readonly T head;
        private readonly ImmutableStack<T> tail;
        private ImmutableStack() { }

        private ImmutableStack(T head, ImmutableStack<T> tail)
        {
            Requires.NotNull(tail, "tail");
            this.head = head;
            this.tail = tail;
        }

        public static ImmutableStack<T> Empty
        {
            get { return EmptyField; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("InvalidEmptyOperation");
            return head;
        }

        IImmutableStack<T> IImmutableStack<T>.Clear()
        {
            return Clear();
        }

        IImmutableStack<T> IImmutableStack<T>.Push(T value)
        {
            return Push(value);
        }

        IImmutableStack<T> IImmutableStack<T>.Pop()
        {
            return Pop();
        }

        public bool IsEmpty
        {
            get { return tail == null; }
        }

        public ImmutableStack<T> Clear()
        {
            return Empty;
        }

        public ImmutableStack<T> Push(T value)
        {
            return new ImmutableStack<T>(value, this);
        }

        public ImmutableStack<T> Pop()
        {
            if (IsEmpty)
                throw new InvalidOperationException("InvalidEmptyOperation");
            return tail;
        }

        public ImmutableStack<T> Pop(out T value)
        {
            value = Peek();
            return Pop();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        internal ImmutableStack<T> Reverse()
        {
            var immutableStack1 = Clear();
            for (var immutableStack2 = this; !immutableStack2.IsEmpty; immutableStack2 = immutableStack2.Pop())
                immutableStack1 = immutableStack1.Push(immutableStack2.Peek());
            return immutableStack1;
        }

        private class EnumeratorObject : IEnumerator<T>
        {
            private readonly ImmutableStack<T> originalStack;
            private ImmutableStack<T> remainingStack;
            private bool disposed;

            internal EnumeratorObject(ImmutableStack<T> stack)
            {
                Requires.NotNull(stack, "stack");
                originalStack = stack;
            }

            public void Dispose()
            {
                disposed = true;
            }

            public bool MoveNext()
            {
                ThrowIfDisposed();
                if (remainingStack == null)
                    remainingStack = originalStack;
                else if (!remainingStack.IsEmpty)
                    remainingStack = remainingStack.Pop();
                return !remainingStack.IsEmpty;
            }

            public void Reset()
            {
                ThrowIfDisposed();
                remainingStack = null;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public T Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (remainingStack == null || remainingStack.IsEmpty)
                        throw new InvalidOperationException();
                    return remainingStack.Peek();
                }
            }

            private void ThrowIfDisposed()
            {
                if (!disposed)
                    return;
                Requires.FailObjectDisposed(this);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator
        {
            private readonly ImmutableStack<T> originalStack;
            private ImmutableStack<T> remainingStack;

            internal Enumerator(ImmutableStack<T> stack)
            {
                Requires.NotNull(stack, "stack");
                originalStack = stack;
                remainingStack = null;
            }

            public T Current
            {
                get
                {
                    if (remainingStack == null || remainingStack.IsEmpty)
                        throw new InvalidOperationException();
                    return remainingStack.Peek();
                }
            }

            public bool MoveNext()
            {
                if (remainingStack == null)
                    remainingStack = originalStack;
                else if (!remainingStack.IsEmpty)
                    remainingStack = remainingStack.Pop();
                return !remainingStack.IsEmpty;
            }
        }
    }
}
