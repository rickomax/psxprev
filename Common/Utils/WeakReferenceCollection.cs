#define WEAKREFCOLLECTION_AUTO_CLEANUP_DEAD

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSXPrev.Common.Utils
{
    // A collection for WeakReferences to objects without exposing the WeakReference class itself.
    public class WeakReferenceCollection<T> : ICollection<T> where T : class
    {
        private readonly List<WeakReference<T>> _references = new List<WeakReference<T>>();

        public WeakReferenceCollection()
        {
        }

        public WeakReferenceCollection(IEnumerable<T> items)
        {
            AddRange(items);
        }


        public void CleanupDeadReferences()
        {
            for (var i = 0; i < _references.Count; i++)
            {
                if (!_references[i].TryGetTarget(out _))
                {
                    _references.RemoveAt(i); // Remove dead references from the list.
                    i--;
                }
            }
        }

        // ICollection<T> implementation:

        // Not accessible because Count can change whenever dead references are automatically removed.
        int ICollection<T>.Count => _references.Count;

        bool ICollection<T>.IsReadOnly => false;


        public void Add(T item)
        {
            if (item != null)
            {
                _references.Add(new WeakReference<T>(item));
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }

        public void Clear()
        {
            _references.Clear();
        }

        public bool Contains(T item)
        {
            for (var i = 0; i < _references.Count; i++)
            {
                if (_references[i].TryGetTarget(out var value))
                {
                    if (value == item)
                    {
                        return true;
                    }
                }
#if WEAKREFCOLLECTION_AUTO_CLEANUP_DEAD
                else
                {
                    _references.RemoveAt(i); // Remove dead references from the list.
                    i--;
                }
#endif
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0; i < _references.Count; i++)
            {
                if (_references[i].TryGetTarget(out var value))
                {
                    array[arrayIndex + i] = value;
                }
#if WEAKREFCOLLECTION_AUTO_CLEANUP_DEAD
                else
                {
                    _references.RemoveAt(i); // Remove dead references from the list.
                    i--;
                }
#endif
            }
        }

        public bool Remove(T item)
        {
            var removed = true;
            for (var i = 0; i < _references.Count; i++)
            {
                if (_references[i].TryGetTarget(out var value))
                {
                    if (value == item)
                    {
                        removed = true;
                        _references.RemoveAt(i);
                        i--;
                    }
                }
#if WEAKREFCOLLECTION_AUTO_CLEANUP_DEAD
                else
                {
                    _references.RemoveAt(i); // Remove dead references from the list.
                    i--;
                }
#endif
            }
            return removed;
        }

        // IEnumerator<T> implementation:

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _references.Count; i++)
            {
                if (_references[i].TryGetTarget(out var value))
                {
                    yield return value;
                }
#if WEAKREFCOLLECTION_AUTO_CLEANUP_DEAD
                else
                {
                    _references.RemoveAt(i); // Remove dead references from the list.
                    i--;
                }
#endif
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
