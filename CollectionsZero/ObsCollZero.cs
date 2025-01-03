using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Net;

namespace CollectionsZero
{
    public class ObsCollZero<T> : ObservableCollection<T>
    {
        private readonly Action<T> _attachAction;
        private readonly Action<T> _detachAction;

        public ObsCollZero(Action<T> attachAction, Action<T> detachAction) : base()
        {
            _attachAction = attachAction ?? ((T t) => { });
            _detachAction = detachAction ?? ((T t) => { });
        }
        public ObsCollZero(Action<T> attachAction, Action<T> detachAction, IEnumerable<T> values) : base(values)
        {
            _attachAction = attachAction ?? ((T t) => { });
            _detachAction = detachAction ?? ((T t) => { });
        }
        public ObsCollZero(Action<T> attachAction, Action<T> detachAction, List<T> values) : base(values)
        {
            _attachAction = attachAction ?? ((T t) => { });
            _detachAction = detachAction ?? ((T t) => { });
        }


        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            _attachAction(item);
        }

        protected override void RemoveItem(int index)
        {
            _detachAction(this[index]);
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            while (this.Count != 0)
                RemoveItem(this.Count - 1);
        }
    }
}
