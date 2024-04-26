// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogStream.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using System.Text;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a managed, asynchronous log writer which supports writing safe data to log files or streams.
    /// </summary>
    public class LogStream : IDisposable
    {
        private readonly Queue<object?> logQueue = new Queue<object?>();
        private readonly ManualResetEvent watcher = new ManualResetEvent(false);
        private readonly ManualResetEvent waiter = new ManualResetEvent(false);
        private readonly ManualResetEvent terminate = new ManualResetEvent(false);
        private readonly Thread loggingThread;
        private bool isBlocking = false;
        internal RotatingLogPolicy? rotatingLogPolicy = null;
        internal CircularBuffer<string>? _bufferingContent = null;

        /// <summary>
        /// Represents a LogStream that writes its output to the <see cref="Console.Out"/> stream.
        /// </summary>
        public static readonly LogStream ConsoleOutput = new LogStream(Console.Out);

        /// <summary>
        /// Gets the defined <see cref="RotatingLogPolicy"/> for this <see cref="LogStream"/>.
        /// </summary>
        /// <remarks>
        /// Internally, this property creates a new <see cref="RotatingLogPolicy"/> for this log stream if it is not defined before.
        /// </remarks>
        public RotatingLogPolicy RotatingPolicy
        {
            get
            {
                if (rotatingLogPolicy == null)
                {
                    rotatingLogPolicy = new RotatingLogPolicy(this);
                }
                return rotatingLogPolicy;
            }
        }

        /// <summary>
        /// Gets an boolean indicating if this <see cref="LogStream"/> is buffering output messages
        /// to their internal message buffer.
        /// </summary>
        public bool IsBuffering { get => _bufferingContent is not null; }

        /// <summary>
        /// Gets or sets the absolute path to the file where the log is being written to.
        /// </summary>
        /// <remarks>
        /// When setting this method, if the file directory doens't exists, it is created.
        /// </remarks>
        public string? FilePath
        {
            get => filePath; set
            {
                if (value is not null)
                {
                    filePath = Path.GetFullPath(value);

                    string? dirPath = Path.GetDirectoryName(filePath);
                    if (dirPath is not null)
                        Directory.CreateDirectory(dirPath);
                }
                else
                {
                    filePath = null;
                }
            }
        }
        string? filePath;

        /// <summary>
        /// Gets the <see cref="System.IO.TextWriter"/> object where the log is being written to.
        /// </summary>
        public TextWriter? TextWriter { get; set; }

        /// <summary>
        /// Gets or sets the encoding used for writting data to the output file. This property is only appliable if
        /// this instance is using an file-based output.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with no predefined outputs.
        /// </summary>
        public LogStream()
        {
            loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            loggingThread.Start();
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given TextWriter object.
        /// </summary>
        /// <param name="tw">The <see cref="System.IO.TextWriter"/> instance which this instance will write log to.</param>
        public LogStream(TextWriter tw) : this()
        {
            TextWriter = tw;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given relative or absolute file path.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        public LogStream(string filename) : this()
        {
            FilePath = filename;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance which writes text to an file and an <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        /// <param name="tw">Represents the text writer which this instance will write log to.</param>
        public LogStream(string? filename, TextWriter? tw) : this()
        {
            if (filename is not null) FilePath = Path.GetFullPath(filename);
            TextWriter = tw;
        }

        /// <summary>
        /// Reads the output buffer. To use this method, it's required to set this
        /// <see cref="LogStream"/> buffering with <see cref="StartBuffering(int)"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this LogStream is not buffering.</exception>
        public string Peek()
        {
            if (_bufferingContent is null)
            {
                throw new InvalidOperationException(SR.LogStream_NotBuffering);
            }

            lock (_bufferingContent)
            {
                string[] lines = _bufferingContent.ToArray();
                return string.Join("", lines);
            }
        }

        /// <summary>
        /// Waits for the log to finish writing the current queue state.
        /// </summary>
        /// <param name="blocking">Block next writings until that instance is released by the <see cref="Set"/> method.</param>
        public void Wait(bool blocking = false)
        {
            if (blocking)
            {
                watcher.Reset();
                isBlocking = true;
            }
            waiter.WaitOne();
        }

        /// <summary>
        /// Releases the execution of the queue.
        /// </summary>
        public void Set()
        {
            watcher.Set();
            isBlocking = false;
        }

        /// <summary>
        /// Start buffering all output to an alternate stream in memory for readability with <see cref="Peek"/> later.
        /// </summary>
        /// <param name="lines">The amount of lines to store in the buffer.</param>
        public void StartBuffering(int lines)
        {
            if (_bufferingContent is not null) return;
            _bufferingContent = new CircularBuffer<string>(lines);
        }

        /// <summary>
        /// Stops buffering output to the alternative stream.
        /// </summary>
        public void StopBuffering()
        {
            _bufferingContent = null;
        }

        private void setWatcher()
        {
            if (!isBlocking)
                watcher.Set();
        }

        private void ProcessQueue()
        {
            while (true)
            {
                waiter.Set();
                int i = WaitHandle.WaitAny(new WaitHandle[] { watcher, terminate });
                if (i == 1) return; // terminate

                watcher.Reset();
                waiter.Reset();

                object?[] copy;
                lock (logQueue)
                {
                    copy = logQueue.ToArray();
                    logQueue.Clear();
                }

                StringBuilder exitBuffer = new StringBuilder();
                foreach (object? line in copy)
                {
                    exitBuffer.AppendLine(line?.ToString());
                }

                if (FilePath is null && TextWriter is null)
                {
                    throw new InvalidOperationException(SR.LogStream_NoOutput);
                }

                if (_bufferingContent is not null)
                {
                    _bufferingContent.Add(exitBuffer.ToString());
                }

                // writes log to outputs
                if (FilePath is not null)
                {
                    File.AppendAllText(FilePath, exitBuffer.ToString(), Encoding);
                }
                if (TextWriter is not null)
                {
                    TextWriter?.Write(exitBuffer.ToString());
                    TextWriter?.Flush();
                }
            }
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public virtual void Close() => Dispose();

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        public virtual void WriteException(Exception exp)
        {
            StringBuilder excpStr = new StringBuilder();
            WriteExceptionInternal(excpStr, exp, 0);
            WriteLineInternal(excpStr.ToString());
        }

        /// <summary>
        /// Writes an line-break at the end of the output.
        /// </summary>
        public void WriteLine()
        {
            WriteLineInternal("");
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public void WriteLine(object? message)
        {
            WriteLineInternal(message?.ToString() ?? "");
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public void WriteLine(string message)
        {
            WriteLineInternal(message);
        }

        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public void WriteLine(string format, params object?[] args)
        {
            WriteLineInternal(string.Format(format, args));
        }

        /// <summary>
        /// Represents the method that intercepts the line that will be written to an output log before being queued for writing.
        /// </summary>
        /// <param name="line">The line which will be written to the log stream.</param>
        protected virtual void WriteLineInternal(string line)
        {
            EnqueueMessageLine(line.Normalize());
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            terminate.Set();
            loggingThread.Join();
            logQueue.Clear();
            TextWriter?.Flush();
            TextWriter?.Close();
        }

        /// <summary>
        /// Defines the time interval and size threshold for starting the task, and then starts the task. This method is an
        /// shortcut for calling <see cref="RotatingLogPolicy.Configure(long, TimeSpan)"/> of this defined <see cref="RotatingPolicy"/> method.
        /// </summary>
        /// <remarks>
        /// The first run is performed immediately after calling this method.
        /// </remarks>
        /// <param name="maximumSize">The non-negative size threshold of the log file size in byte count.</param>
        /// <param name="dueTime">The time interval between checks.</param>
        public LogStream ConfigureRotatingPolicy(long maximumSize, TimeSpan dueTime)
        {
            var policy = RotatingPolicy;
            policy.Configure(maximumSize, dueTime);
            return this;
        }

        void EnqueueMessageLine(string message)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));
            lock (logQueue)
            {
                logQueue.Enqueue(message);
                setWatcher();
            }
        }

        void WriteExceptionInternal(StringBuilder exceptionSbuilder, Exception exp, int currentDepth = 0)
        {
            if (currentDepth == 0)
                exceptionSbuilder.AppendLine(string.Format(SR.LogStream_ExceptionDump_Header, DateTime.Now.ToString("R")));
            exceptionSbuilder.AppendLine(exp.ToString());

            if (exp.InnerException != null)
            {
                if (currentDepth <= 3)
                {
                    WriteExceptionInternal(exceptionSbuilder, exp.InnerException, currentDepth + 1);
                }
                else
                {
                    exceptionSbuilder.AppendLine(SR.LogStream_ExceptionDump_TrimmedFooter);
                }
            }
        }
    }
}
