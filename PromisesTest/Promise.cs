using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromisesTest
{
    class Promise
    {
        Action<Action, Action<Exception>> _action = null;

        object _lock = new object();

        Promise _nextPromise = null;

        Action<Exception> _errorHandler = null;

        bool _resolved = false;
        bool _rejected = false;

        Exception _ex = null;

        public static Promise Create(Action<Action, Action<Exception>> action)
        {
            return new Promise(action);
        }

        public Promise(Action<Action, Action<Exception>> action) : this(true, action) { }

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
                _action(() => Resolve(), (ex) => OnReject(ex));
            }
            catch (Exception ex)
            {
                OnReject(ex);
            }
        }

        private void OnReject(Exception ex)
        {
            lock (_lock)
            {
                _rejected = true;
                _errorHandler?.Invoke(ex);
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
            return Then((resolve, reject) =>
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
            });
        }

        public void Catch(Action<Exception> action)
        {
            bool alreadyRejected = false;

            lock (_lock)
            {
                if (_resolved)
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
                action.Invoke(_ex);
            }
        }
    }
}
