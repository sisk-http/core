// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogStream.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
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
        /// Gets the absolute path to the file where the log is being written to.
        /// </summary>
        /// <definition>
        /// public string? FilePath { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? FilePath { get; private set; } = null;

        /// <summary>
        /// Gets the <see cref="TextWriter"/> object where the log is being written to.
        /// </summary>
        /// <definition>
        /// public TextWriter? TextWriter { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public TextWriter? TextWriter { get; private set; } = null;

        /// <summary>
        /// Gets or sets the function that formats input when used with <see cref="WriteFormat(object?)"/>.
        /// </summary>
        /// <definition>
        /// public Func{{object?, string}}? Format { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
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
        /// <param name="tw">Represents the writer which this instance will write log to.</param>
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
        /// Reads the last few lines of the linked log file.
        /// </summary>
        /// <param name="lines">The amount of lines to be read from the file.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown when used with a log file for a stream, textwriter, or other non-file structure.</exception>
        /// <definition>
        /// public string[] Peek(int lines)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public string[] Peek(int lines)
        {
            if (this.FilePath == null)
            {
                throw new NotSupportedException(SR.LogStream_Peek_NotFilePath);
            }

            string[] output = Array.Empty<string>();
            if (File.Exists(this.FilePath))
            {
                output = File.ReadLines(this.FilePath, this.Encoding).TakeLast(lines).ToArray();
            }

            return output;
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

                //Console.WriteLine("{0,20}{1,20} {2}", "", "queue ++", logQueue.Count);

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
                    exitBuffer.Append(line?.ToString());
                }

                if (FilePath != null)
                {
                    File.AppendAllText(FilePath!, exitBuffer.ToString(), Encoding);
                }
                else if (TextWriter != null)
                {
                    TextWriter?.Write(exitBuffer.ToString());
                    TextWriter?.Flush();
                }
            }
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        /// <definition>
        /// public void Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Close() => Dispose();

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        /// <definition>
        /// public void WriteException(Exception exp)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteException(Exception exp)
        {
            StringBuilder sexc = new StringBuilder();
            WriteExceptionInternal(sexc, exp, 0);
            WriteLine(sexc.ToString());
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
            lock (logQueue)
            {
                logQueue.Enqueue("\n");
                setWatcher();
            }
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
        public void WriteFormat(object? input)
        {
            if (Format == null)
            {
                throw new InvalidOperationException(SR.LogStream_NoFormat);
            }
            string format = Format(input);
            Write(format);
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
            lock (logQueue)
            {
                logQueue.Enqueue(message?.ToString() + "\n");
                setWatcher();
            }
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
            lock (logQueue)
            {
                logQueue.Enqueue(message + "\n");
                setWatcher();
            }
        }

        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        /// <definition>
        /// public void WriteLine(string format, params object?[] args)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine(string format, params object?[] args)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(string.Format(format, args) + "\n");
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
        public void Write(object? value)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(value);
                setWatcher();
            }
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

        void WriteExceptionInternal(StringBuilder exceptionStr, Exception exp, int depth = 0)
        {
            if (depth == 0)
                exceptionStr.AppendLine(string.Format(SR.LogStream_ExceptionDump_Header, DateTime.Now.ToString("R")));
            exceptionStr.AppendLine(exp.ToString());

            if (exp.InnerException != null)
            {
                if (depth <= 10)
                {
                    WriteExceptionInternal(exceptionStr, exp.InnerException, depth + 1);
                }
                else
                {
                    exceptionStr.AppendLine(SR.LogStream_ExceptionDump_TrimmedFooter);
                }
            }
        }
    }
}
