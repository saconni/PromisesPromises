using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Promises
{
    class MultipleExceptions : Exception
    {
        public IEnumerable<Exception> InnerExceptions { get; private set; }

        public MultipleExceptions(IEnumerable<Exception> exceptions)
        {
            InnerExceptions = exceptions;
        }
    };

    public class Promise
    {
        // the action that will resolve (or reject) the promise
        Action<Action, Action<Exception>> _action = null;

        // lock to syncronize access to _resolved and _rejected vars
        object _lock = new object();

        // state of the promise
        bool _resolved = false;
        bool _rejected = false;

        // next promise in chain
        Promise _nextPromise = null;

        // this promise error handler
        Action<Exception> _errorHandler = null;

        Exception _ex = null;

        public static Promise Create(Action<Action, Action<Exception>> action)
        {
            return new Promise((resolve, reject) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        action(resolve, reject);
                    }
                    catch(Exception ex)
                    {
                        reject(ex);
                    }
                });
            });
        }

        public static Promise Create(Action simpleAction)
        {
            return Create(CreatePromiseAction(simpleAction));
        }

        private Promise(Action<Action, Action<Exception>> action) : this(true, action) { }

        private Promise(bool immediate, Action<Action, Action<Exception>> action)
        {
            _action = action;
            if(immediate)
            {
                Perform();
            }
        }

        private void Perform()
        {
            try
            {
                _action(() => Resolve(), (ex) => Reject(ex));
            }
            catch (Exception ex)
            {
                Reject(ex);
            }
        }

        private void Reject(Exception ex)
        {
            lock (_lock)
            {
                _ex = ex; // store the exception
                _rejected = true;
                if(_nextPromise != null)
                {
                    _nextPromise.Reject(ex);
                }
                else
                {
                    _errorHandler?.Invoke(ex);
                }
            }
        }

        private void Resolve()
        {
            lock (_lock)
            {
                _resolved = true;
                _nextPromise?.Perform();
            }
        }

        public Promise Then(Action<Action, Action<Exception>> action)
        {
            _nextPromise = new Promise(false, action);

            bool alreadyResolved = false;

            lock (_lock)
            {
                if (_resolved)
                {
                    alreadyResolved = true;
                }
            }

            if(alreadyResolved)
            {
                _nextPromise.Perform();
            }

            return _nextPromise;
        }

        public Promise Then(Action simpleAction)
        {
            return Then(CreatePromiseAction(simpleAction));
        }

        private static Action<Action, Action<Exception>> CreatePromiseAction(Action simpleAction)
        {
            return (resolve, reject) =>
            {
                try
                {
                    simpleAction.Invoke();
                    resolve();
                }
                catch (Exception ex)
                {
                    reject(ex);
                }
            };
        }

        public void Catch(Action<Exception> action)
        {
            bool alreadyRejected = false;

            lock (_lock)
            {
                if (_rejected)
                {
                    alreadyRejected = true;
                }
                else
                {
                    _errorHandler = action;
                }
            }

            if (alreadyRejected)
            {
                _errorHandler.Invoke(_ex);
            }
        }

        public static Promise All(IEnumerable<Promise> promises)
        {
            return Promise.Create((resolve, reject) =>
            {
                var waiters = new List<ManualResetEvent>();
                var exceptions = new List<Exception>();

                foreach (var p in promises)
                {
                    var waiter = new ManualResetEvent(false);
                    waiters.Add(waiter);

                    Task.Run(() =>
                    {
                        p.Then(() =>
                        {
                            waiter.Set();
                        })
                        .Catch((ex) =>
                        {
                            exceptions.Add(ex);
                            waiter.Set();
                        });
                    });
                }

                WaitHandle.WaitAll(waiters.ToArray());

                if(exceptions.Count == 0)
                {
                    resolve();
                }
                else if(exceptions.Count == 1)
                {
                    reject(exceptions.First());
                }
                else
                {
                    reject(new MultipleExceptions(exceptions.ToArray()));
                }
            });
        }
    }
}
