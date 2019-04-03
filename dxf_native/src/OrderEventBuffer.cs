using System.Collections;
using System.Collections.Generic;
using com.dxfeed.api.data;
using com.dxfeed.api.events;

namespace com.dxfeed.native {
    /// <summary>
    /// Simple buffer to collect order events. Not thread safe.
    /// </summary>
    public class OrderEventBuffer : IDxEventBuf<IDxOrder> {
        private readonly IDictionary<long, IDxOrder> events = new Dictionary<long, IDxOrder>();

        /// <summary>
        /// Creates the new buffer.
        /// </summary>
        /// <param name="type">The type of the new buffer's events</param>
        /// <param name="symbol">The symbol of the new buffer's events</param>
        /// <param name="eventParams">The event parameters</param>
        internal OrderEventBuffer(EventType type, string symbol, EventParams eventParams) {
            EventType = type;
            Symbol = symbol;
            EventParams = eventParams;
        }

        /// <summary>
        /// Adds a new order event to the buffer or replaces the old event with the same index.
        /// </summary>
        /// <param name="order">A new order event</param>
        internal void Add(IDxOrder order) {
            events[order.Index] = order;

            if (Size == 1 || First.Index > order.Index) {
                First = order;
            }
        }

        /// <summary>
        /// Clears the buffer
        /// </summary>
        internal void Clear() {
            events.Clear();
            First = null;
        }

        /// <summary>
        /// Removes an order event from the buffer by index
        /// </summary>
        /// <param name="order"></param>
        internal void Remove(IDxOrder order) {
            if (!events.Remove(order.Index)) return;
            
            if (First.Index == order.Index) {
                First = null;
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<IDxOrder> GetEnumerator() {
            return new SortedDictionary<long, IDxOrder>(events).Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new SortedDictionary<long, IDxOrder>(events).Values.GetEnumerator();
        }

        #endregion

        #region Implementation of IDxEventBuf<out T>

        public EventType EventType { get; }

        public string Symbol { get; }

        public int Size => events.Count;
        public EventParams EventParams { get; internal set; }

        #endregion

        /// <summary>
        /// The first order event (the order with min Index) 
        /// </summary>
        public IDxOrder First { get; private set; }
    }
}