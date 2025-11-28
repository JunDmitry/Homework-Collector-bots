using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Common.Utilities
{
    public sealed class PriorityQueue<TValue, TPriority> : IEnumerable<TValue>
        where TPriority : IComparable
    {
        private List<TValue> _values;
        private List<TPriority> _priorities;

        private IComparer<TPriority> _comparer;

        public PriorityQueue() : this(1)
        { }

        public PriorityQueue(int capacity) : this(capacity, Comparer<TPriority>.Default)
        {
        }

        public PriorityQueue(IComparer<TPriority> comparer) : this(1, comparer)
        {
        }

        public PriorityQueue(Comparison<TPriority> comparison)
            : this(1, comparison)
        {
        }

        public PriorityQueue(int capacity, Comparison<TPriority> comparison)
            : this(capacity, Comparer<TPriority>.Create(comparison))
        {
        }

        public PriorityQueue(int capacity, IComparer<TPriority> comparer)
        {
            _values = new(capacity);
            _priorities = new(capacity);
            _comparer = comparer;
        }

        public int Count { get; private set; }
        public int Capacity => _values.Capacity;

        public void Enqueue(TValue value, TPriority priority)
        {
            _values.Add(value);
            _priorities.Add(priority);

            ShiftUp(Count);
            Count++;
        }

        public TValue DequeueFirst()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

            TValue result = _values[0];

            Count--;
            _values[0] = _values[Count];
            _priorities[0] = _priorities[Count];
            ShiftDown(0);

            _values.RemoveAt(Count);
            _priorities.RemoveAt(Count);

            return result;
        }

        public TValue DequeueLast()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

            Count--;
            _values.RemoveAt(Count);
            _priorities.RemoveAt(Count);

            return _values[Count];
        }

        public TValue PeekFirst()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

            return _values[0];
        }

        public TValue PeekLast()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

            return _values[Count - 1];
        }

        public void Clear()
        {
            _values.Clear();
            _priorities.Clear();
            _comparer = default;
            Count = 0;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ShiftUp(int i)
        {
            int parentIndex;

            while (i > 0 && _comparer.Compare(_priorities[Parent(i)], _priorities[i]) < 0)
            {
                parentIndex = Parent(i);
                Swap(i, parentIndex);

                i = parentIndex;
            }
        }

        private void ShiftDown(int i)
        {
            int maxIndex = i;

            int left = LeftChild(i);
            int right = RightChild(i);

            if (left <= Count && _comparer.Compare(_priorities[left], _priorities[maxIndex]) > 0)
                maxIndex = left;

            if (right <= Count && _comparer.Compare(_priorities[right], _priorities[maxIndex]) > 0)
                maxIndex = right;

            if (i != maxIndex)
            {
                Swap(maxIndex, i);
                ShiftDown(maxIndex);
            }
        }

        private void Swap(int i, int j)
        {
            TValue tempItem = _values[j];
            TPriority tempPriority = _priorities[j];

            _values[j] = _values[i];
            _priorities[j] = _priorities[i];

            _values[i] = tempItem;
            _priorities[i] = tempPriority;
        }

        private static int Parent(int i)
        {
            return (i - 1) / 2;
        }

        private static int LeftChild(int i)
        {
            return ((2 * i) + 1);
        }

        private static int RightChild(int i)
        {
            return ((2 * i) + 2);
        }
    }
}