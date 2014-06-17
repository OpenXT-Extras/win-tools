//
// Copyright (c) 2012 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel; // AsyncCompletedEventArgs
using System.Threading;
using System.Collections.Specialized;

/// <summary>
/// Invokes functions via an AsyncOperation and then raises completion event in appropriate thread.
/// External code provides input data wrapped in single argument,
/// and functions to call operation with that data and convert result to EventArgs sub-class instance.
/// Typically will also expose "CompletedEvent" as an event property with a meaningful name.
/// Hopefully this will remove a world of repetitive suck.
/// </summary>
/// <typeparam name="TEventArgs">EventArgs sub-class which will be constructed and passed to event.</typeparam>
public class AsyncOperationEventRaiser<TEventArgs>
    where TEventArgs : EventArgs
{
    // Async tracking implementation.
    private HybridDictionary dictTasks = new HybridDictionary(1);
    private int nCurrentTaskId = 0;

    // Asynchronous implementation.
    /// <summary>
    /// Handles internal task state and triggering completion activity on appropriate thread via "AsyncOperation".
    /// </summary>
    /// <param name="a">Arguments passed to completion code.</param>
    /// <param name="asyncOp">Responsible for co-ordinating operation threads.</param>
    protected void AsyncWorkCompletionResult(TEventArgs a, AsyncOperation asyncOp)
    {
        UserStateHolder userstate = (UserStateHolder)asyncOp.UserSuppliedState;
        lock (this.dictTasks.SyncRoot)
        {
            this.dictTasks.Remove(userstate.TaskId);
        }

        // Call into our completion generic delegate on the appropriate thread.
        asyncOp.PostOperationCompleted(this.onAsyncWorkCompletedDelegate, a);
        //asyncOp.PostOperationCompleted(this.CallbackAsyncWorkCompleted, a);
    }

    // AsyncOperation interaction.
    /// <summary>
    /// Delegate instance called by AsyncOperation.PostOperationCompleted() in correct thread once task has completed.
    /// </summary>
    protected SendOrPostCallback onAsyncWorkCompletedDelegate;
    /// <summary>
    /// Delegate implementation for <see cref="onAsyncWorkCompletedDelegate">onAsyncWorkCompletedDelegate</see>.
    /// </summary>
    /// <param name="state">Actually this is the event args.</param>
    private void CallbackAsyncWorkCompleted(object state)
    {
        TEventArgs e = state as TEventArgs;
        this.OnAsyncWorkCompleted(e);
    }

    // Overridable behavior.
    /// <summary>
    /// Called in correct thread to raise appropriate event.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAsyncWorkCompleted(TEventArgs e)
    {
        if (this.CompletedEvent != null)
        {
            this.CompletedEvent(this, e);
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="proxy">Proxy to call through.</param>
    public AsyncOperationEventRaiser()
    {
        this.InitialiseDelegates();
    }

    /// <summary>
    /// Initialise the delegates which will be used by AsyncOperation.
    /// </summary>
    protected virtual void InitialiseDelegates()
    {
        this.onAsyncWorkCompletedDelegate = CallbackAsyncWorkCompleted;

    }

    // Async interface.
    /// <summary>
    /// Used to combine internal required user state and user state provided by calling code.
    /// </summary>
    private struct UserStateHolder
    {
        int nTaskId;
        object userState;

        internal int TaskId { get { return nTaskId; } }
        public object UserState { get { return userState; } }

        internal UserStateHolder(int nTaskId, object userState)
        {
            this.nTaskId = nTaskId;
            this.userState = userState;
        }

    } // Ends struct UserStateHolder

    /// <summary>
    /// Create an operation to carry out task asynchronously.
    /// </summary>
    /// <param name="userState">User provided state object.</param>
    /// <returns>AsyncOperation ready to pass to delegate "BeginInvoke()".</returns>
    protected AsyncOperation CreateOperation(object userState)
    {
        AsyncOperation asyncOp;

        lock (this.dictTasks.SyncRoot)
        {
            asyncOp = AsyncOperationManager.CreateOperation(new UserStateHolder(++this.nCurrentTaskId, userState));
            this.dictTasks[this.nCurrentTaskId] = asyncOp;
        }

        return asyncOp;
    }

    /// <summary>
    /// Raised when operation completes.
    /// Typically external code will present this as an event property with a more meaningful name.
    /// </summary>
    public event System.EventHandler<TEventArgs> CompletedEvent;

    // Function takes a single data argument, and returns a value.
    #region Func
    #region Func Implementation
    // Implementation for Func.
    /// <summary>
    /// Holds outer function to perform, and its corresponding function parameters.
    /// </summary>
    /// <typeparam name="TData">Type of input data.</typeparam>
    /// <typeparam name="TResult">Type of action result.</typeparam>
    private class DataAndFunc<TData, TResult>
    {
        public AsyncOperationFuncDefaultDelegate<TData, TResult> operation;
        public Func<TData, TResult> func;
        public MakeFuncEventDelegate<TData, TResult> funcEvent;
        public TData data;

    }

    /// <summary>
    /// Delegate for operation triggered by AsyncOperation in another thread..
    /// </summary>
    /// <typeparam name="TData">Input data type to pass to operation.</typeparam>
    /// <typeparam name="TResult">Result data type for operation.</typeparam>
    /// <param name="datafunction">Collection of operation to be called by AsyncOperation, and its input functions.</param>
    /// <param name="asyncOp">Operation making call.</param>
    private delegate void AsyncOperationFuncDelegate<TData, TResult>(DataAndFunc<TData, TResult> datafunction, AsyncOperation asyncOp);

    /// <summary>
    /// Call the operation, and then handle completion.
    /// </summary>
    /// <typeparam name="TData">Input data type to pass to operation.</typeparam>
    /// <typeparam name="TResult">Result data type for operation.</typeparam>
    /// <param name="datafunction">Collection of operation to be called by AsyncOperation, and its input functions.</param>
    /// <param name="asyncOp">Operation making call.</param>
    private void AsyncOperationFunc<TData, TResult>(DataAndFunc<TData, TResult> datafunction, AsyncOperation asyncOp)
    {
        // Will have to do exception handling itself.
        TEventArgs args = datafunction.operation(datafunction.data, datafunction.func, false, asyncOp.UserSuppliedState, datafunction.funcEvent);

        this.AsyncWorkCompletionResult(args, asyncOp);
    }
    #endregion // Ends Func Implementation

    #region Func public interface
    /// <summary>
    /// Default implementation called by AsyncOperation in background thread to perform actual asynchronous action.
    /// </summary>
    /// <typeparam name="TData">Input data type for operation.</typeparam>
    /// <typeparam name="TResult">Output data type for operation.</typeparam>
    /// <param name="data">Input data for operation.</param>
    /// <param name="func">Function to perform on data.</param>
    /// <param name="cancelled">Cancelled state.</param>
    /// <param name="userState">User state object.</param>
    /// <param name="funcEvent">Function which builds event args from results.</param>
    /// <returns>Event args for operation which will end up raised in event on correct thread.</returns>
    public delegate TEventArgs AsyncOperationFuncDefaultDelegate<TData, TResult>(TData data, Func<TData, TResult> func, bool cancelled, object userState,
        MakeFuncEventDelegate<TData, TResult> funcEvent);

    public static TEventArgs AsyncOperationFuncDefaultImpl<TData, TResult>(TData data, Func<TData, TResult> func, bool cancelled, object userState,
        MakeFuncEventDelegate<TData, TResult> funcEvent)
    {
        Exception e = null;
        TResult result = default(TResult);
        try
        {
            result = func(data);
        }
        catch (Exception ex)
        {
            e = ex;
        }
        object userStateOriginal = userState;
        if (userState is UserStateHolder)
        {
            // Get the original user-provided object out of the holder.
            userState = ((UserStateHolder)userState).UserState;
        }
        return funcEvent(data, result, e, cancelled, userState);
    }
    /// <summary>
    /// Create a TEventArgs for a function call.
    /// </summary>
    /// <typeparam name="TData">Function input data type.</typeparam>
    /// <typeparam name="TResult">Function result data type.</typeparam>
    /// <param name="data">Function input data.</param>
    /// <param name="result">Function result data/</param>
    /// <param name="e">Exception thrown as result of function call, otherwise null.</param>
    /// <param name="cancelled">True if function cancelled.</param>
    /// <param name="userState">User state created when function invoked asynchronously.</param>
    /// <returns>TEventArgs containing function results.</returns>
    public delegate TEventArgs MakeFuncEventDelegate<TData, TResult>(TData data, TResult result, Exception e, bool cancelled, object userState);
    // Implementation defined in sub-class.

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <typeparam name="TResult">Result data type of operation.</typeparam>
    /// <param name="userState">User state object provided by calling code.</param>
    /// <param name="data">Operation input data.</param>
    /// <param name="operation">Operation being called in another thread by AsyncOperation.</param>
    /// <param name="funcProxy">Function being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncFuncImpl<TData, TResult>(object userState
        , TData data
        , AsyncOperationFuncDefaultDelegate<TData, TResult> operation
        , Func<TData, TResult> funcProxy
        , MakeFuncEventDelegate<TData, TResult> funcEvent
        )
    {
        AsyncOperation asyncOp = this.CreateOperation(userState);
        DataAndFunc<TData, TResult> datafunction = new DataAndFunc<TData, TResult>
        {
            operation = operation,
            data = data,
            func = funcProxy,
            funcEvent = funcEvent
        };
        AsyncOperationFuncDelegate<TData, TResult> asyncDelegate = this.AsyncOperationFunc<TData, TResult>;
        asyncDelegate.BeginInvoke(datafunction, asyncOp, null, null);

    }

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// This overload requires no operation delegate specified for AsyncOperation.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <typeparam name="TResult">Result data type of operation.</typeparam>
    /// <param name="data">Operation input data.</param>
    /// <param name="funcProxy">Function being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncFuncImpl<TData, TResult>(object userState
        , TData data
        , Func<TData, TResult> funcProxy
        , MakeFuncEventDelegate<TData, TResult> funcEvent
        )
    {
        this.AsyncFuncImpl(userState, data, AsyncOperationFuncDefaultImpl, funcProxy, funcEvent);
    }

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// This override just defaults the userState object to null.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <typeparam name="TResult">Result data type of operation.</typeparam>
    /// <param name="data">Operation input data.</param>
    /// <param name="operation">Operation being called in another thread by AsyncOperation.</param>
    /// <param name="funcProxy">Function being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncFuncImpl<TData, TResult>(TData data
        , AsyncOperationFuncDefaultDelegate<TData, TResult> operation
        , Func<TData, TResult> funcProxy
        , MakeFuncEventDelegate<TData, TResult> funcEvent)
    {
        this.AsyncFuncImpl(null, data, operation, funcProxy, funcEvent);
    }

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// This overload requires no operation delegate specified for AsyncOperation.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <typeparam name="TResult">Result data type of operation.</typeparam>
    /// <param name="data">Operation input data.</param>
    /// <param name="funcProxy">Function being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncFuncImpl<TData, TResult>(TData data
        , Func<TData, TResult> funcProxy
        , MakeFuncEventDelegate<TData, TResult> funcEvent)
    {
        this.AsyncFuncImpl(null, data, AsyncOperationFuncDefaultImpl, funcProxy, funcEvent);
    }

    #endregion // Ends Func protected interface
    #endregion // Ends Func

    // Action takes a single data argument, and has no return value.
    #region Action
    #region Action Implementation
    // Implementation for Action.
    /// <summary>
    /// Holds outer function to perform, and its corresponding action and function parameters.
    /// </summary>
    /// <typeparam name="TData">Type of input data.</typeparam>
    private class DataAndAction<TData>
    {
        public AsyncOperationActionDefaultDelegate<TData> operation;
        public Action<TData> action;
        public MakeActionEventDelegate<TData> funcEvent;
        public TData data;

    } // Ends class DataAndAction

    /// <summary>
    /// Delegate for operation triggered by AsyncOperation in another thread..
    /// </summary>
    /// <typeparam name="TData">Input data type to pass to operation.</typeparam>
    /// <param name="datafunction">Collection of operation to be called by AsyncOperation, and its input functions.</param>
    /// <param name="asyncOp">Operation making call.</param>
    private delegate void AsyncOperationActionDelegate<TData>(DataAndAction<TData> datafunction, AsyncOperation asyncOp);

    /// <summary>
    /// Call the operation, and then handle completion.
    /// </summary>
    /// <typeparam name="TData">Input data type to pass to operation.</typeparam>
    /// <param name="datafunction">Collection of operation to be called by AsyncOperation, and its input functions.</param>
    /// <param name="asyncOp">Operation making call.</param>
    private void AsyncOperationAction<TData>(DataAndAction<TData> datafunction, AsyncOperation asyncOp)
    {
        // Will have to do exception handling itself.
        TEventArgs args = datafunction.operation(datafunction.data, datafunction.action, false, asyncOp.UserSuppliedState, datafunction.funcEvent);

        this.AsyncWorkCompletionResult(args, asyncOp);
    }
    #endregion // Ends Action Implementation

    #region Action public interface
    /// <summary>
    /// Default implementation called by AsyncOperation in background thread to perform actual asynchronous action.
    /// </summary>
    /// <typeparam name="TData">Input data type for operation.</typeparam>
    /// <param name="data">Input data for operation.</param>
    /// <param name="action">Action to perform on data.</param>
    /// <param name="cancelled">Cancelled state.</param>
    /// <param name="userState">User state object.</param>
    /// <param name="funcEvent">Action which builds event args from results.</param>
    /// <returns>Event args for operation which will end up raised in event on correct thread.</returns>
    public delegate TEventArgs AsyncOperationActionDefaultDelegate<TData>(TData data, Action<TData> action, bool cancelled, object userState,
        MakeActionEventDelegate<TData> funcEvent);
    public static TEventArgs AsyncOperationActionDefaultImpl<TData>(TData data, Action<TData> action, bool cancelled, object userState,
        MakeActionEventDelegate<TData> funcEvent)
    {
        Exception e = null;
        try
        {
            action(data);
        }
        catch (Exception ex)
        {
            e = ex;
        }

        object userStateOriginal = userState;
        if (userState is UserStateHolder)
        {
            // Get the original user-provided object out of the holder.
            userState = ((UserStateHolder)userState).UserState;
        }

        return funcEvent(data, e, cancelled, userState);
    }

    /// <summary>
    /// Create a TEventArgs for an action call.
    /// </summary>
    /// <typeparam name="TData">Action input data type.</typeparam>
    /// <param name="data">Action input data.</param>
    /// <param name="e">Exception thrown as result of action call, otherwise null.</param>
    /// <param name="cancelled">True if action cancelled.</param>
    /// <param name="userState">User state created when action invoked asynchronously.</param>
    /// <returns>TEventArgs containing action results.</returns>
    public delegate TEventArgs MakeActionEventDelegate<TData>(TData data, Exception e, bool cancelled, object userState);
    // Implementation defined in sub-class.

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate action and function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <param name="userState">User state object provided by calling code.</param>
    /// <param name="data">Operation input data.</param>
    /// <param name="operation">Operation being called in another thread by AsyncOperation.</param>
    /// <param name="actionProxy">Action being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncActionImpl<TData>(object userState
        , TData data
        , AsyncOperationActionDefaultDelegate<TData> operation
        , Action<TData> actionProxy
        , MakeActionEventDelegate<TData> funcEvent
        )
    {
        AsyncOperation asyncOp = this.CreateOperation(userState);
        DataAndAction<TData> dataaction = new DataAndAction<TData>
        {
            operation = operation,
            data = data,
            action = actionProxy,
            funcEvent = funcEvent
        };
        AsyncOperationActionDelegate<TData> asyncDelegate = this.AsyncOperationAction<TData>;
        asyncDelegate.BeginInvoke(dataaction, asyncOp, null, null);
    }

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate action and function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// This override just defaults the userState object to null.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <param name="data">Operation input data.</param>
    /// <param name="operation">Operation being called in another thread by AsyncOperation.</param>
    /// <param name="actionProxy">Action being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncActionImpl<TData>(TData data
        , AsyncOperationActionDefaultDelegate<TData> operation
        , Action<TData> actionProxy
        , MakeActionEventDelegate<TData> funcEvent)
    {
        this.AsyncActionImpl(null, data, operation, actionProxy, funcEvent);
    }

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate action and function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// This overload requires no operation delegate specified for AsyncOperation.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <param name="data">Operation input data.</param>
    /// <param name="operation">Operation being called in another thread by AsyncOperation.</param>
    /// <param name="actionProxy">Action being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncActionImpl<TData>(object userState
        , TData data
        , Action<TData> actionProxy
        , MakeActionEventDelegate<TData> funcEvent
        )
    {
        this.AsyncActionImpl(userState, data, AsyncOperationActionDefaultImpl, actionProxy, funcEvent);
    }

    /// <summary>
    /// Implementation of asynchronous function called by sub-classes with appropriate action and function parameters
    /// to perform the asynchronous operation, and turn results into TEventArgs.
    /// This overload requires no operation delegate specified for AsyncOperation, and defaults userState to null.
    /// </summary>
    /// <typeparam name="TData">Input data type of operation.</typeparam>
    /// <param name="data">Operation input data.</param>
    /// <param name="operation">Operation being called in another thread by AsyncOperation.</param>
    /// <param name="actionProxy">Action being called.</param>
    /// <param name="funcEvent">Function to create TEventArgs from results.</param>
    public void AsyncActionImpl<TData>(TData data
        , Action<TData> actionProxy
        , MakeActionEventDelegate<TData> funcEvent)
    {
        this.AsyncActionImpl(null, data, AsyncOperationActionDefaultImpl, actionProxy, funcEvent);
    }

    #endregion // Ends Action protected interface
    #endregion // Ends Action

} // Ends class AsyncOperationEventRaiser
