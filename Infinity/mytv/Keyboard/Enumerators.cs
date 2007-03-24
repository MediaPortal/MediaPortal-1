using System;
using System.Collections;

namespace MCEControls
{
    /// <summary>
    /// Returns an Enumerator that enumerates over nothing.
    /// </summary>
    internal class EmptyEnumerator : IEnumerator
    {
        // singleton class, private ctor
        private EmptyEnumerator()
        {
        }

        /// <summary>
        /// Read-Only instance of an Empty Enumerator.
        /// </summary>
        public static IEnumerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EmptyEnumerator();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Reset() { }

        /// <summary>
        /// Returns false.
        /// </summary>
        /// <returns>false</returns>
        public bool MoveNext() { return false; }


#pragma warning disable 1634, 1691  // about to use PreSharp message numbers - unknown to C#

        /// <summary>
        /// Returns null.
        /// </summary>
        public object Current
        {
            get
            {
#pragma warning disable 6503 // "Property get methods should not throw exceptions."

                throw new InvalidOperationException();

#pragma warning restore 6503
            }
        }
#pragma warning restore 1634, 1691

        private static IEnumerator _instance;
    }


    internal class SingleChildEnumerator : IEnumerator
    {
        internal SingleChildEnumerator(object Child)
        {
            _child = Child;
            _count = Child == null ? 0 : 1;
        }

        object IEnumerator.Current
        {
            get { return (_index == 0) ? _child : null; }
        }

        bool IEnumerator.MoveNext()
        {
            _index++;
            return _index < _count;
        }

        void IEnumerator.Reset()
        {
            _index = -1;
        }

        private int _index = -1;
        private int _count = 0;
        private object _child;
    }

}
