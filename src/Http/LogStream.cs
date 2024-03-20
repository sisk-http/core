// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a managed, asynchronous log writer which supports writing safe data to log files or streams.
    /// </summary>
    /// <definition>
    /// public class LogStream : IDisposable
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class LogStream : IDisposable
    {
        private Queue<object?> logQueue = new Queue<object?>();
        private ManualResetEvent watcher = new ManualResetEvent(false);
        private ManualResetEvent waiter = new ManualResetEvent(false);
        private ManualResetEvent terminate = new ManualResetEvent(false);
        private Thread loggingThread;
        private bool isBlocking = false;
        internal RotatingLogPolicy? rotatingLogPolicy = null;
        internal CircularBuffer<string>? _bufferingContent = null;

        /// <summary>
        /// Represents a LogStream that writes its output to the <see cref="Console.Out"/> stream.
        /// </summary>
        /// <definition>
        /// public static LogStream ConsoleOutput;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public static LogStream ConsoleOutput = new LogStream(Console.Out);

        /// <summary>
        /// Gets the defined <see cref="RotatingLogPolicy"/> for this <see cref="LogStream"/>.
        /// </summary>
        /// <remarks>
        /// Internally, this property creates a new <see cref="RotatingLogPolicy"/> for this log stream if it is not defined before.
        /// </remarks>
        /// <definition>
        /// public RotatingLogPolicy RotatingPolicy
        /// </definition>
        /// <type>
        /// Property
        /// </type>
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
        /// <definition>
        /// public bool IsBuffering { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsBuffering { get => _bufferingContent is not null; }

        /// <summary>
        /// Gets the absolute path to the file where the log is being written to.
        /// </summary>
        /// <definition>
        /// public string? FilePath { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? FilePath { get; } = null;

        /// <summary>
        /// Gets the <see cref="System.IO.TextWriter"/> object where the log is being written to.
        /// </summary>
        /// <definition>
        /// public TextWriter? TextWriter { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public TextWriter? TextWriter { get; set; } = null;

        /// <summary>
        /// Gets the <see cref="System.IO.StreamWriter"/> object where the log is being written to.
        /// </summary>
        /// <definition>
        /// public StreamWriter? StreamWriter { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public StreamWriter? StreamWriter { get; set; } = null;

        /// <summary>
        /// Gets or sets the function that formats input when used with <see cref="WriteFormat(object?)"/>.
        /// </summary>
        /// <definition>
        /// public Func{{object?, string}}? Format { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        [Obsolete("This property no longer works and will be removed in future versions of Sisk. To write messages with custom formats, extend " +
            "this class and override WriteLine.")]
        public Func<object?, string>? Format { get; set; }

        /// <summary>
        /// Gets or sets the encoding used for writting data to the output file. This property is only appliable if
        /// this instance is using an file-based output.
        /// </summary>
        /// <definition>
        /// public Encoding Encoding { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        private LogStream()
        {
            loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            loggingThread.Start();
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given TextWriter object.
        /// </summary>
        /// <param name="tw">The <see cref="System.IO.TextWriter"/> instance which this instance will write log to.</param>
        /// <definition>
        /// public LogStream(TextWriter tw)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public LogStream(TextWriter tw) : this()
        {
            TextWriter = tw;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given relative or absolute file path.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        /// <definition>
        /// public LogStream(string filename)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public LogStream(string filename) : this()
        {
            FilePath = Path.GetFullPath(filename);
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given <see cref="System.IO.StreamWriter"/> resource.
        /// </summary>
        /// <param name="sw">The stream which the log stream will write log to.</param>
        /// <definition>
        /// public LogStream(StreamWriter sw)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public LogStream(StreamWriter sw) : this()
        {
            StreamWriter = sw;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance which writes text to an file and an <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        /// <param name="tw">Represents the writer which this instance will write log to.</param>
        /// <param name="sw">The <see cref="System.IO.TextWriter"/> instance which this instance will write log to.</param>
        /// <definition>
        /// public LogStream(string? filename, TextWriter? tw, StreamWriter? sw)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public LogStream(string? filename, TextWriter? tw, StreamWriter? sw) : this()
        {
            if (filename is not null) FilePath = Path.GetFullPath(filename);
            TextWriter = tw;
            StreamWriter = sw;
        }

        /// <summary>
        /// Reads the output buffer. To use this method, it's required to set this
        /// <see cref="LogStream"/> buffering with <see cref="StartBuffering(int)"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when this LogStream is not buffering.</exception>
        /// <definition>
        /// public string Peek()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
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
        /// <definition>
        /// public void Wait(bool blocking = false)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
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
        /// <definition>
        /// public void Set()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Set()
        {
            watcher.Set();
            isBlocking = false;
        }

        /// <summary>
        /// Start buffering all output to an alternate stream in memory for readability with <see cref="Peek(int)"/> later.
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

                if (FilePath is null && TextWriter is null && StreamWriter is null)
                {
                    throw new InvalidOperationException(SR.LogStream_NoOutput);
                }

                if (_bufferingContent is not null)
                {
                    _bufferingContent.PushBack(exitBuffer.ToString());
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
                if (StreamWriter is not null)
                {
                    StreamWriter.Write(exitBuffer.ToString());
                    StreamWriter.Flush();
                }
            }
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        /// <definition>
        /// public virtual void Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public virtual void Close() => Dispose();

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        /// <definition>
        /// public virtual void WriteException(Exception exp)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public virtual void WriteException(Exception exp)
        {
            StringBuilder sexc = new StringBuilder();
            WriteExceptionInternal(sexc, exp, 0);
            WriteLine(sexc.ToString(), Array.Empty<object?>());
        }

        /// <summary>
        /// Writes an line-break at the end of the output.
        /// </summary>
        /// <definition>
        /// public void WriteLine()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine()
        {
            WriteLine("", Array.Empty<object?>());
        }

        /// <summary>
        /// Writes the input, formatting with <see cref="Format"/> handler, at the end of the output.
        /// </summary>
        /// <param name="input">The input object which will be formatted and written to the output.</param>
        /// <definition>
        /// public void WriteFormat(object? input)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        [Obsolete("This method no longer works and will be removed in future versions of Sisk. To write messages with custom formats, extend " +
            "this class and override WriteLine.")]
        public void WriteFormat(object? input)
        {
            return;
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        /// <definition>
        /// public void WriteLine(object? message)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine(object? message)
        {
            WriteLine(message?.ToString() ?? "", Array.Empty<object?>());
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        /// <definition>
        /// public void WriteLine(string message)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine(string message)
        {
            WriteLine(message, Array.Empty<object?>());
        }

        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        /// <definition>
        /// public virtual void WriteLine(string format, params object?[] args)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public virtual void WriteLine(string format, params object?[] args)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(string.Format(format, args));
                setWatcher();
            }
        }

        /// <summary>
        /// Writes the text into the output.
        /// </summary>
        /// <param name="value">The text which will be inserted at the output.</param>
        /// <definition>
        /// public void Write(object? value)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        [Obsolete("This method no longer works and will be removed in future versions of Sisk. Use WriteLine instead.")]
        public void Write(object? value)
        {
            return;
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Dispose()
        {
            terminate.Set();
            loggingThread.Join();
            logQueue.Clear();
            TextWriter?.Flush();
            TextWriter?.Close();
            StreamWriter?.Flush();
            StreamWriter?.Close();
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
        /// <definition>
        /// public void Configure(long maximumSize, TimeSpan due)
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public void ConfigureRotatingPolicy(long maximumSize, TimeSpan dueTime)
        {
            var policy = RotatingPolicy;
            policy.Configure(maximumSize, dueTime);
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
