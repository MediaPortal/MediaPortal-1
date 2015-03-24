using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents base abstract class that executes operation on separate thread.
    /// </summary>
    /// <threadsafety static="true" instance="false" />
    public class BackgroundWorker : IDisposable
    {
        #region Private fields

        private Thread worker;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Ignored. Volatile instance field.")]
        protected volatile Boolean cancelRequested;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Ignored. Volatile instance field.")]
        protected volatile Boolean running;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Ignored. Volatile instance field.")]
        protected volatile Boolean finished;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BackgroundWorker"/> class.
        /// </summary>
        public BackgroundWorker()
        {
            this.cancelRequested = false;
            this.running = false;
            this.finished = false;
            this.Exception = null;

            this.worker = new Thread(new ParameterizedThreadStart(BackgroundWorker.ThreadProc));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tests if cancel of execution is requested.
        /// </summary>
        /// <value><see langword="true"/> if cancel of execution is requested, <see langword="false"/> otherwise.</value>
        public Boolean IsCancelRequested { get { return this.cancelRequested; } }

        /// <summary>
        /// Tests if operation is running.
        /// </summary>
        /// <value><see langword="true"/> if operation is running, <see langword="false"/> otherwise.</value>
        public Boolean IsRunning { get { return this.running; } }

        /// <summary>
        /// Tests is operation is finished.
        /// </summary>
        /// <value><see langword="true"/> if operation is finished, <see langword="false"/> otherwise.</value>
        public Boolean IsFinished { get { return this.finished; } }

        /// <summary>
        /// Tests if while executing operation occured error.
        /// </summary>
        /// <value><see langword="true"/> if while executing operation occurred error, <see langword="false"/> otherwise.</value>
        public Boolean IsError { get { return (this.Exception != null); } }

        /// <summary>
        /// Gets the error occurred while executing operation.
        /// </summary>
        public Exception Exception { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Cancels execution of operation.
        /// </summary>
        public virtual void CancelOperation()
        {
            this.cancelRequested = true;

            while (this.running)
            {
                // sleep some time to get chance of other threads to do something
                Thread.Sleep(1);
            }

            this.cancelRequested = false;
            this.running = false;
        }

        /// <summary>
        /// Starts operation execution.
        /// </summary>
        public virtual void StartOperation()
        {
            this.worker.Start(this);
        }

        /// <summary>
        /// Occurs when operation should do some work.
        /// </summary>
        public event EventHandler<DoWorkEventArgs> DoWork;

        /// <summary>
        /// Occurs when executing thread finishes its work.
        /// </summary>
        public event EventHandler<FinishedWorkEventArgs> FinishedWork;

        /// <summary>
        /// Disposes this <see cref="BackgroundWorker"/> object.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release managed resources; if <see langword="false"/>, <see cref="Dispose(bool)"/> has no effect.</param>
        /// <overloads>
        /// Disposes this <see cref="BackgroundWorker"/> object.
        /// </overloads>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release managed resources

                this.CancelOperation();
            }

            // free unmanaged resources
        }

        /// <summary>
        /// Releases all resources used by the <see cref="BackgroundWorker"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignored. Method must be safe.")]
        private static void ThreadProc(object param)
        {
            BackgroundWorker caller = (BackgroundWorker)param;

            caller.running = true;

            try
            {
                bool finished = false;

                while ((!caller.cancelRequested) && (!finished))
                {
                    if (caller.DoWork != null)
                    {
                        DoWorkEventArgs eventArgs = new DoWorkEventArgs(finished, null);
                        caller.DoWork(caller, eventArgs);

                        finished = eventArgs.IsFinished;
                        caller.Exception = eventArgs.Exception;
                    }

                    // sleep some time to get chance of other threads to do something
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                // catch all exceptions, avoid problem with unhandled exception

                caller.Exception = ex;
            }

            if (caller.FinishedWork != null)
            {
                try
                {
                    caller.FinishedWork(caller, new FinishedWorkEventArgs(caller.IsCancelRequested, caller.Exception));
                }
                catch
                {
                    // catch all exceptions, avoid problem with unhandled exception
                }
            }

            caller.running = false;
            caller.finished = true;
        }

        #endregion
    }
}
