//using System;
//using System.Collections.Generic;

using System;
using System.Collections.Generic;
using ATray.Annotations;

namespace ATray.Tools
{
    /// <summary>
    /// Manages subscriptions for messages of type <typeparamref name="T"/>
    /// </summary>
    public interface IPubSubHub<T> : IObservable<T>
    {
        /// <summary>
        /// Publish <paramref name="obj"/> to all subscribers
        /// </summary>
        void Publish(T obj);
    }

    /// <summary>
    /// Base class for observables that keeps track of subscriptions 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableBase<T> : IPubSubHub<T>
    {
        private readonly HashSet<IObserver<T>> _observers = new HashSet<IObserver<T>>();

        /// <summary>
        /// Publish <paramref name="obj"/> to all subscribers (not from IObserable)
        /// </summary>
        public void Publish(T obj)
        {
            foreach (var observer in _observers)
                observer.OnNext(obj);
        }

        /// <summary>
        /// Add a subscriber
        /// </summary>
        public IDisposable Subscribe(IObserver<T> observer) => new UnsubscribeObject(_observers, observer);

        /// <summary>
        /// Object that unsubscribes a subscription when disposed
        /// </summary>
        private class UnsubscribeObject : IDisposable
        {
            private readonly HashSet<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            public UnsubscribeObject([NotNull] HashSet<IObserver<T>> observers,[NotNull] IObserver<T> observer)
            {
                _observers = observers ?? throw new ArgumentNullException(nameof(observers));
                _observer = observer ?? throw new ArgumentNullException(nameof(observer));
                _observers.Add(_observer);
            }

            public void Dispose()
            {
                _observers.Remove(_observer);
            }
        }
    }

    public class PubSubHub<T>
    {
        public void Subscribe(IObserver<T> subscriber)
        {

        }
    }

    //    public class PubSubHub
    //    {
    //        private readonly HashSet<(Type type, ISubscriber sub)> _subscriptions = new HashSet<(Type, ISubscriber)>();

    //        public void Publish<T>(T message)
    //        {
    //            foreach (var subscription in _subscriptions)
    //                if (subscription.type.IsAssignableFrom(typeof(T)))
    //                {
    //                    subscription.type.MakeGenericType()
    //                    subscription.sub.GetType()
    //                }
    //        }

    //        public void Subscribe<T>(ISubscriber<T> subscriber)
    //        {
    //            _subscriptions.Add((typeof(T), subscriber));
    //        }

    //        public void Unsubscribe<T>(ISubscriber<T> subscriber)
    //        {
    //            _subscriptions.RemoveWhere(x => x.sub == subscriber);
    //        }
    //    }

    //    public interface ISubscriber
    //    {
    //    }

    //    public interface ISubscriber<in T> : ISubscriber
    //    {
    //        void Consume(T message);
    //    }
}