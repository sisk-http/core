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
using System.Threading.Channels;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a managed, asynchronous log writer which supports writing safe data to log files or text streams.
    /// </summary>
    public class LogStream : IDisposable
    {
        private readonly Channel<object?> channel = Channel.CreateUnbounded<object?>(new UnboundedChannelOptions() { SingleReader = true });
        private readonly Thread consumerThread;
        internal readonly ManualResetEvent writeEvent = new ManualResetEvent(false);
        internal readonly ManualResetEvent rotatingPolicyLocker = new ManualResetEvent(true);
        internal RotatingLogPolicy? rotatingLogPolicy = null;

        private string? filePath;
        private bool isDisposed;
        private CircularBuffer<string>? _bufferingContent = null;

        /// <summary>
        /// Gets a <see cref="LogStream"/> that writes its output to the <see cref="Console.Out"/> stream.
        /// </summary>
        public static LogStream ConsoleOutput { get => new LogStream(Console.Out); }

        /// <summary>
        /// Gets the defined <see cref="RotatingLogPolicy"/> for this <see cref="LogStream"/>.
        /// </summary>
        public RotatingLogPolicy RotatingPolicy
        {
            get
            {
                if (rotatingLogPolicy is null)
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
        /// Gets an boolean indicating if this <see cref="LogStream"/> was disposed.
        /// </summary>
        public bool Disposed { get => isDisposed; }

        /// <summary>
        /// Gets or sets a boolean that indicates that every input must be trimmed and normalized before
        /// being written to some output stream.
        /// </summary>
        public bool NormalizeEntries { get; set; } = true;

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
            consumerThread = new Thread(new ThreadStart(ProcessQueue));
            consumerThread.Start();
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
        /// Creates an new <see cref="LogStream"/> instance which writes text to an file and an <see cref="System.IO.TextWriter"/>.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        /// <param name="tw">The text writer which this instance will write log to.</param>
        public LogStream(string? filename, TextWriter? tw) : this()
        {
            if (filename is not null) FilePath = Path.GetFullPath(filename);
            TextWriter = tw;
        }

        /// <summary>
        /// Clears the current log queue and blocks the current thread until all content is written to the underlying streams.
        /// </summary>
        public void Flush()
        {
            writeEvent.WaitOne();
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
                return string.Join(Environment.NewLine, lines);
            }
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

        private async void ProcessQueue()
        {
            var reader = channel.Reader;
            try
            {
                while (!isDisposed && await reader.WaitToReadAsync())
                {
                    writeEvent.Reset();

                    while (reader.TryRead(out var item))
                    {
                        rotatingPolicyLocker.WaitOne();

                        bool gotAnyError = false;
                        string? dataStr = item?.ToString();

                        if (dataStr is null)
                            continue;

                        try
                        {
                            TextWriter?.WriteLine(dataStr);
                        }
                        catch
                        {
                            if (!gotAnyError)
                            {
                                await channel.Writer.WriteAsync(item);
                                gotAnyError = true;
                            }
                        }

                        try
                        {
                            if (filePath is not null)
                                File.AppendAllText(filePath, dataStr + Environment.NewLine, Encoding);
                        }
                        catch
                        {
                            if (!gotAnyError)
                            {
                                await channel.Writer.WriteAsync(item);
                                gotAnyError = true;
                            }
                        }

                        try
                        {
                            _bufferingContent?.Add(dataStr);
                        }
                        catch
                        {
                            if (!gotAnyError)
                            {
                                await channel.Writer.WriteAsync(item);
                                gotAnyError = true;
                            }
                        }
                    }

                    writeEvent.Set();
                }
            }
            finally
            {
                writeEvent.Set();
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
            WriteLineInternal(string.Empty);
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public void WriteLine(object? message)
        {
            WriteLineInternal(message?.ToString() ?? string.Empty);
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
            if (NormalizeEntries)
            {
                EnqueueMessageLine(line.Normalize().Trim().ReplaceLineEndings());
            }
            else
            {
                EnqueueMessageLine(line);
            }
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
            _ = channel.Writer.WriteAsync(message);
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

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    channel.Writer.Complete();
                    Flush();
                    TextWriter?.Dispose();
                    rotatingLogPolicy?.Dispose();
                    consumerThread.Join();
                }

                _bufferingContent = null;
                isDisposed = true;
            }
        }

        /// <inheritdoc/>
        ~LogStream()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
