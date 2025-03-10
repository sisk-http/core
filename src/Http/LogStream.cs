// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using System.Threading.Channels;
using Sisk.Core.Entity;

namespace Sisk.Core.Http {
    /// <summary>
    /// Provides a managed, asynchronous log writer which supports writing safe data to log files or text streams.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Naming", "CA1711:Identifiers should not have incorrect suffix",
        Justification = "Breaking change. Not going forward on this one." )]
    public class LogStream : IDisposable {
        private readonly Channel<object?> channel = Channel.CreateUnbounded<object?> ( new UnboundedChannelOptions () { SingleReader = true, SingleWriter = false } );
        private readonly Thread consumerThread;
        internal readonly ManualResetEvent writeEvent = new ManualResetEvent ( false );
        internal readonly ManualResetEvent rotatingPolicyLocker = new ManualResetEvent ( true );
        internal RotatingLogPolicy? rotatingLogPolicy;

        private string? filePath;
        private bool isDisposed;
        private CircularBuffer<string>? _bufferingContent;

        /// <summary>
        /// Gets a <see cref="LogStream"/> that writes its output to the <see cref="Console.Out"/> stream.
        /// </summary>
        public static LogStream ConsoleOutput { get => new LogStream ( Console.Out ); }

        /// <summary>
        /// Gets the defined <see cref="RotatingLogPolicy"/> for this <see cref="LogStream"/>.
        /// </summary>
        public RotatingLogPolicy RotatingPolicy {
            get {
                this.rotatingLogPolicy ??= new RotatingLogPolicy ( this );
                return this.rotatingLogPolicy;
            }
        }

        /// <summary>
        /// Gets an boolean indicating if this <see cref="LogStream"/> is buffering output messages
        /// to their internal message buffer.
        /// </summary>
        public bool IsBuffering { get => this._bufferingContent is not null; }

        /// <summary>
        /// Gets an boolean indicating if this <see cref="LogStream"/> was disposed.
        /// </summary>
        public bool Disposed { get => this.isDisposed; }

        /// <summary>
        /// Gets or sets a boolean that indicates that every input must be trimmed and have their
        /// line endings normalized before being written to the output stream.
        /// </summary>
        public bool NormalizeEntries { get; set; } = true;

        /// <summary>
        /// Gets or sets the absolute path to the file where the log is being written to.
        /// </summary>
        /// <remarks>
        /// When setting this method, if the file directory doens't exists, it is created.
        /// </remarks>
        public string? FilePath {
            get => this.filePath; set {
                if (value is not null) {
                    this.filePath = Path.GetFullPath ( value );

                    string? dirPath = Path.GetDirectoryName ( this.filePath );
                    if (dirPath is not null)
                        Directory.CreateDirectory ( dirPath );
                }
                else {
                    this.filePath = null;
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
        public LogStream () {
            this.consumerThread = new Thread ( new ThreadStart ( this.ProcessQueue ) );
            this.consumerThread.Start ();
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given TextWriter object.
        /// </summary>
        /// <param name="tw">The <see cref="System.IO.TextWriter"/> instance which this instance will write log to.</param>
        public LogStream ( TextWriter tw ) : this () {
            this.TextWriter = tw;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given relative or absolute file path.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        public LogStream ( string filename ) : this () {
            this.FilePath = filename;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance which writes text to an file and an <see cref="System.IO.TextWriter"/>.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        /// <param name="tw">The text writer which this instance will write log to.</param>
        public LogStream ( string? filename, TextWriter? tw ) : this () {
            if (filename is not null)
                this.FilePath = Path.GetFullPath ( filename );
            this.TextWriter = tw;
        }

        /// <summary>
        /// Clears the current log queue and blocks the current thread until all content is written to the underlying streams.
        /// </summary>
        public void Flush () {
            this.writeEvent.WaitOne ();
        }

        /// <summary>
        /// Reads the output buffer. To use this method, it's required to set this
        /// <see cref="LogStream"/> buffering with <see cref="StartBuffering(int)"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this LogStream is not buffering.</exception>
        public string Peek () {
            if (this._bufferingContent is null) {
                throw new InvalidOperationException ( SR.LogStream_NotBuffering );
            }

            lock (this._bufferingContent) {
                string [] lines = this._bufferingContent.ToArray ();
                return string.Join ( Environment.NewLine, lines );
            }
        }

        /// <summary>
        /// Start buffering all output to an alternate stream in memory for readability with <see cref="Peek"/> later.
        /// </summary>
        /// <param name="lines">The amount of lines to store in the buffer.</param>
        public void StartBuffering ( int lines ) {
            if (this._bufferingContent is not null)
                return;
            this._bufferingContent = new CircularBuffer<string> ( lines );
        }

        /// <summary>
        /// Stops buffering output to the alternative stream.
        /// </summary>
        public void StopBuffering () {
            this._bufferingContent = null;
        }

        private async void ProcessQueue () {
            var reader = this.channel.Reader;
            try {
                while (!this.isDisposed && await reader.WaitToReadAsync ()) {
                    this.writeEvent.Reset ();

                    while (reader.TryRead ( out var item )) {
                        this.rotatingPolicyLocker.WaitOne ();

                        bool gotAnyError = false;
                        string? dataStr = item?.ToString ();

                        if (dataStr is null)
                            continue;

                        try {
                            this.TextWriter?.WriteLine ( dataStr );
                        }
                        catch (Exception ex) {
                            Console.WriteLine ( this.GetExceptionEntry ( ex, "Exception raised from the LogStream TextWriter instance" ) );

                            if (!gotAnyError) {
                                await this.channel.Writer.WriteAsync ( item );
                                gotAnyError = true;
                            }
                        }

                        try {
                            if (this.filePath is not null)
                                File.AppendAllText ( this.filePath, dataStr + Environment.NewLine, this.Encoding );
                        }
                        catch (Exception ex) {
                            Console.WriteLine ( this.GetExceptionEntry ( ex, "Exception raised from the LogStream FilePath instance" ) );

                            if (!gotAnyError) {
                                await this.channel.Writer.WriteAsync ( item );
                                gotAnyError = true;
                            }
                        }

                        try {
                            this._bufferingContent?.Add ( dataStr );
                        }
                        catch (Exception ex) {
                            Console.WriteLine ( this.GetExceptionEntry ( ex, "Exception raised from the LogStream BufferingContent instance" ) );

                            if (!gotAnyError) {
                                await this.channel.Writer.WriteAsync ( item );
                            }
                        }
                    }

                    this.writeEvent.Set ();
                }
            }
            finally {
                this.writeEvent.Set ();
            }
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public virtual void Close () => this.Dispose ();

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        public virtual void WriteException ( Exception exp ) => this.WriteException ( exp, null );

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        /// <param name="extraContext">Extra context message to append to the exception message.</param>
        public virtual void WriteException ( Exception exp, string? extraContext = null ) {
            StringBuilder excpStr = new StringBuilder ();
            this.WriteExceptionInternal ( excpStr, exp, extraContext, 0 );
            this.WriteLineInternal ( excpStr.ToString () );
        }

        /// <summary>
        /// Writes an line-break at the end of the output.
        /// </summary>
        public void WriteLine () {
            this.WriteLineInternal ( string.Empty );
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public void WriteLine ( object? message ) {
            this.WriteLineInternal ( message?.ToString () ?? string.Empty );
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public void WriteLine ( string message ) {
            this.WriteLineInternal ( message );
        }

        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public void WriteLine ( string format, params object? [] args ) {
            this.WriteLineInternal ( string.Format ( provider: null, format, args ) );
        }

        /// <summary>
        /// Writes the text format and arguments and appends a line-break at the end into the output, using the specified format provider.
        /// </summary>
        /// <param name="formatProvider">The format provider to use when formatting the string. If null, the current culture is used.</param>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public void WriteLine ( IFormatProvider? formatProvider, string format, params object? [] args ) {
            this.WriteLineInternal ( string.Format ( formatProvider, format, args ) );
        }

        /// <summary>
        /// Represents the method that intercepts the line that will be written to an output log before being queued for writing.
        /// </summary>
        /// <param name="line">The line which will be written to the log stream.</param>
        protected virtual async void WriteLineInternal ( string line ) {
            if (this.NormalizeEntries) {
                await this.EnqueueMessageLine ( line.Normalize ().Trim ().ReplaceLineEndings () );
            }
            else {
                await this.EnqueueMessageLine ( line );
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
        public LogStream ConfigureRotatingPolicy ( long maximumSize, TimeSpan dueTime ) {
            var policy = this.RotatingPolicy;
            policy.Configure ( maximumSize, dueTime );
            return this;
        }

        async ValueTask EnqueueMessageLine ( string message ) {
            ArgumentNullException.ThrowIfNull ( message, nameof ( message ) );
            await this.channel.Writer.WriteAsync ( message );
        }

        void WriteExceptionInternal ( StringBuilder exceptionSbuilder, Exception exp, string? context = null, int currentDepth = 0 ) {
            if (currentDepth == 0)
                exceptionSbuilder.AppendLine ( SR.Format ( SR.LogStream_ExceptionDump_Header,
                    context is null ? DateTime.Now.ToString ( "R" ) : $"{context}, {DateTime.Now:R}" ) );

            exceptionSbuilder.AppendLine ( exp.ToString () );

            if (exp.InnerException != null) {
                if (currentDepth <= 3) {
                    exceptionSbuilder.AppendLine ( "+++ inner exception +++" );
                    this.WriteExceptionInternal ( exceptionSbuilder, exp.InnerException, null, currentDepth + 1 );
                }
                else {
                    exceptionSbuilder.AppendLine ( SR.LogStream_ExceptionDump_TrimmedFooter );
                }
            }
        }

        string GetExceptionEntry ( Exception exception, string? context = null ) {
            StringBuilder exceptionSbuilder = new StringBuilder ();
            this.WriteExceptionInternal ( exceptionSbuilder, exception, context, 0 );
            return exceptionSbuilder.ToString ();
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        protected virtual void Dispose ( bool disposing ) {
            if (!this.isDisposed) {
                if (disposing) {
                    this.channel.Writer.Complete ();
                    this.Flush ();
                    this.TextWriter?.Dispose ();
                    this.rotatingLogPolicy?.Dispose ();
                    this.consumerThread.Join ();
                }

                this._bufferingContent = null;
                this.isDisposed = true;
            }
        }

        /// <inheritdoc/>
        ~LogStream () {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose ( disposing: false );
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public void Dispose () {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose ( disposing: true );
            GC.SuppressFinalize ( this );
        }
    }
}
