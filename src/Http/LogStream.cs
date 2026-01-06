// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Globalization;
using System.Text;
using System.Threading.Channels;
using Sisk.Core.Entity;
using Sisk.Core.Helpers;

namespace Sisk.Core.Http {
    /// <summary>
    /// Provides a managed, asynchronous log writer which supports writing safe data to log files or text streams.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Naming", "CA1711:Identifiers should not have incorrect suffix",
        Justification = "Breaking change. Not going forward on this one." )]
    public class LogStream : IDisposable, IAsyncDisposable {

        private readonly Channel<object> _channel = Channel.CreateBounded<object> ( new BoundedChannelOptions ( 10_000 ) {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        } );

        private readonly Task _consumerTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource ();
        internal readonly SemaphoreSlim rotatingPolicyLocker = new SemaphoreSlim ( 1, 1 );
        internal RotatingLogPolicy? rotatingLogPolicy;

        private string? _filePath;
        private bool _isDisposed;
        private CircularBuffer<string>? _bufferingContent;

        private static readonly Lazy<LogStream> _consoleOutputLazy = new Lazy<LogStream> ( () => new LogStream ( Console.Out ) );
        private static readonly Lazy<LogStream> _emptyLazy = new Lazy<LogStream> ( () => new LogStream () );

        /// <summary>
        /// Converts the specified <see cref="DateTime"/> to a file-name-safe string representation
        /// formatted as "yyyy-MM-dd_HH-mm-ss".
        /// </summary>
        /// <param name="dt">The date and time to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information. Can be <see langword="null"/>.</param>
        /// <returns>A string that is safe for use in file names.</returns>
        public static string EscapeSafeDateTime ( DateTime dt, IFormatProvider? provider = null ) {
            return dt.ToString ( "yyyy-MM-dd_HH-mm-ss", provider );
        }

        /// <summary>
        /// Gets the current local date and time as a file-name-safe string formatted as "yyyy-MM-dd_HH-mm-ss".
        /// </summary>
        public static string SafeTimestamp => EscapeSafeDateTime ( DateTime.Now, CultureInfo.InvariantCulture );

        /// <summary>
        /// Gets a shared <see cref="LogStream"/> that writes its output to the <see cref="Console.Out"/> stream.
        /// </summary>
        public static LogStream ConsoleOutput => _consoleOutputLazy.Value;

        /// <summary>
        /// Gets a shared <see cref="LogStream"/> without any output stream.
        /// </summary>
        public static LogStream Empty => _emptyLazy.Value;

        /// <summary>
        /// Gets the defined <see cref="RotatingLogPolicy"/> for this <see cref="LogStream"/>.
        /// </summary>
        public RotatingLogPolicy RotatingPolicy {
            get {
                rotatingLogPolicy ??= new RotatingLogPolicy ( this );
                return rotatingLogPolicy;
            }
        }

        /// <summary>
        /// Safely writes the specified string to the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the file where the string will be written.</param>
        /// <param name="contents">The string to write to the file. Can be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the operation was successful, otherwise <see langword="false"/>.</returns>
        public static bool SafeWriteToFile ( string filePath, string? contents ) {
            return SafeWriteToFile ( filePath, contents, Encoding.UTF8 );
        }

        /// <summary>
        /// Safely writes the specified string to the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the file where the string will be written.</param>
        /// <param name="contents">The string to write to the file. Can be <see langword="null"/>.</param>
        /// <param name="encoding">The encoding to use when writing the string. Defaults to <see cref="Encoding.UTF8"/>.</param>
        /// <returns><see langword="true"/> if the operation was successful, otherwise <see langword="false"/>.</returns>
        public static bool SafeWriteToFile ( string filePath, string? contents, Encoding encoding ) {
            string fullPath = Path.GetFullPath ( filePath );
            string? directoryPath = Path.GetDirectoryName ( fullPath );

            if (directoryPath is null)
                return false; // user is trying to write to a root directory?

            try {
                if (!Directory.Exists ( directoryPath ))
                    Directory.CreateDirectory ( directoryPath );

                File.WriteAllText ( fullPath, contents, encoding );

                return true;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Safely writes the specified string to the specified file path asynchronously.
        /// </summary>
        /// <param name="filePath">The path to the file where the string will be written.</param>
        /// <param name="contents">The string to write to the file. Can be <see langword="null"/>.</param>
        /// <param name="cancellation">The cancellation token to use for the operation. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation. The task returns <see langword="true"/> if the operation was successful, otherwise <see langword="false"/>.</returns>
        public static Task<bool> SafeWriteToFileAsync ( string filePath, string? contents, CancellationToken cancellation = default ) {
            return SafeWriteToFileAsync ( filePath, contents, Encoding.UTF8, cancellation );
        }

        /// <summary>
        /// Safely writes the specified string to the specified file path asynchronously.
        /// </summary>
        /// <param name="filePath">The path to the file where the string will be written.</param>
        /// <param name="contents">The string to write to the file. Can be <see langword="null"/>.</param>
        /// <param name="encoding">The encoding to use when writing the string. Defaults to <see cref="Encoding.UTF8"/>.</param>
        /// <param name="cancellation">The cancellation token to use for the operation. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation. The task returns <see langword="true"/> if the operation was successful, otherwise <see langword="false"/>.</returns>
        public static async Task<bool> SafeWriteToFileAsync ( string filePath, string? contents, Encoding encoding, CancellationToken cancellation = default ) {
            string fullPath = Path.GetFullPath ( filePath );
            string? directoryPath = Path.GetDirectoryName ( fullPath );

            if (directoryPath is null)
                return false; // user is trying to write to a root directory?

            try {
                if (!Directory.Exists ( directoryPath ))
                    Directory.CreateDirectory ( directoryPath );

                await File.WriteAllTextAsync ( fullPath, contents, encoding, cancellation );

                return true;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Gets an boolean indicating if this <see cref="LogStream"/> is buffering output messages
        /// to their internal message buffer.
        /// </summary>
        public bool IsBuffering => _bufferingContent is not null;

        /// <summary>
        /// Gets an boolean indicating if this <see cref="LogStream"/> was disposed.
        /// </summary>
        public bool Disposed => _isDisposed;

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
            get => _filePath;
            set {
                if (value is not null) {
                    _filePath = Path.GetFullPath ( value );

                    if (!PathHelper.IsPathAllowed ( _filePath )) {
                        throw new ArgumentException ( SR.Format ( SR.LogStream_InvalidFilePath, value ), nameof ( value ) );
                    }

                    string? dirPath = Path.GetDirectoryName ( _filePath );
                    if (dirPath is not null)
                        Directory.CreateDirectory ( dirPath );
                }
                else {
                    _filePath = null;
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
            // Start a long-running background task to process the log queue.
            _consumerTask = Task.Factory.StartNew (
                ProcessQueueAsync,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default );
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given TextWriter object.
        /// </summary>
        /// <param name="tw">The <see cref="System.IO.TextWriter"/> instance which this instance will write log to.</param>
        public LogStream ( TextWriter tw ) : this () {
            TextWriter = tw;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given relative or absolute file path.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        public LogStream ( string filename ) : this () {
            FilePath = filename;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance which writes text to an file and an <see cref="System.IO.TextWriter"/>.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        /// <param name="tw">The text writer which this instance will write log to.</param>
        public LogStream ( string? filename, TextWriter? tw ) : this () {
            if (filename is not null)
                FilePath = Path.GetFullPath ( filename );
            TextWriter = tw;
        }

        /// <summary>
        /// Blocks the current thread until all currently enqueued content is written to the underlying streams.
        /// </summary>
        public void Flush () {
            FlushAsync ().ConfigureAwait ( false ).GetAwaiter ().GetResult ();
        }

        /// <summary>
        /// Asynchronously waits until all currently enqueued content is written to the underlying streams.
        /// </summary>
        public async Task FlushAsync () {
            var flushSignal = new FlushSignal ();
            await _channel.Writer.WriteAsync ( flushSignal );
            await flushSignal.Completion.Task;
        }

        /// <summary>
        /// Reads the output buffer. To use this method, it's required to set this
        /// <see cref="LogStream"/> buffering with <see cref="StartBuffering(int)"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this LogStream is not buffering.</exception>
        public string Peek () {
            if (_bufferingContent is null) {
                throw new InvalidOperationException ( SR.LogStream_NotBuffering );
            }

            lock (_bufferingContent) {
                string [] lines = _bufferingContent.ToArray ();
                Array.Reverse ( lines );
                return string.Join ( Environment.NewLine, lines );
            }
        }

        /// <summary>
        /// Start buffering all output to an alternate stream in memory for readability with <see cref="Peek"/> later.
        /// </summary>
        /// <param name="lines">The amount of lines to store in the buffer.</param>
        public void StartBuffering ( int lines ) {
            if (_bufferingContent is not null) {
                _bufferingContent.Resize ( lines );
                return;
            }

            _bufferingContent = new CircularBuffer<string> ( lines );
        }

        /// <summary>
        /// Stops buffering output.
        /// </summary>
        public void StopBuffering () {
            _bufferingContent = null;
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public void Close () => Dispose ();

        /// <summary>
        /// Defines the time interval and size threshold for starting the task, and then starts the task. This method is an
        /// shortcut for calling <see cref="RotatingLogPolicy.Configure(long, TimeSpan, RotatingLogPolicyCompressor)"/> of this defined <see cref="RotatingPolicy"/> method.
        /// </summary>
        /// <remarks>
        /// The first run is performed immediately after calling this method.
        /// </remarks>
        /// <param name="maximumSize">The non-negative size threshold of the log file size in byte count.</param>
        /// <param name="dueTime">The time interval between checks.</param>
        public LogStream ConfigureRotatingPolicy ( long maximumSize, TimeSpan dueTime ) {
            var policy = RotatingPolicy;
            policy.Configure ( maximumSize, dueTime );
            return this;
        }

        #region Sync write methods
        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        public void WriteException ( Exception exp ) => WriteException ( exp, null );

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        /// <param name="extraContext">Extra context message to append to the exception message.</param>
        public void WriteException ( Exception exp, string? extraContext = null ) {
            StringBuilder excpStr = new StringBuilder ();
            WriteExceptionInternal ( excpStr, exp, extraContext, 0 );
            WriteLineInternal ( excpStr.ToString () );
        }

        /// <summary>
        /// Writes an line-break at the end of the output.
        /// </summary>
        public void WriteLine () {
            WriteLineInternal ( string.Empty );
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public void WriteLine ( object? message ) {
            WriteLineInternal ( message?.ToString () ?? string.Empty );
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public void WriteLine ( string message ) {
            WriteLineInternal ( message );
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public void WriteLine ( string format, params ReadOnlySpan<object?> args ) {
            WriteLineInternal ( string.Format ( provider: null, format, args ) );
        }

        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public void WriteLine ( string format, params IEnumerable<object?> args ) {
            WriteLineInternal ( string.Format ( provider: null, format, args ) );
        }
#else
        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public void WriteLine ( string format, params object? [] args ) {
            WriteLineInternal ( string.Format ( provider: null, format, args ) );
        }
#endif

        /// <summary>
        /// Writes the text format and arguments and appends a line-break at the end into the output, using the specified format provider.
        /// </summary>
        /// <param name="formatProvider">The format provider to use when formatting the string. If null, the current culture is used.</param>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public void WriteLine ( IFormatProvider? formatProvider, string format, params object? [] args ) {
            WriteLineInternal ( string.Format ( formatProvider, format, args ) );
        }
        #endregion

        #region Async write methods
        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        public Task WriteExceptionAsync ( Exception exp ) => WriteExceptionAsync ( exp, null );

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        /// <param name="extraContext">Extra context message to append to the exception message.</param>
        public async Task WriteExceptionAsync ( Exception exp, string? extraContext = null ) {
            StringBuilder excpStr = new StringBuilder ();
            WriteExceptionInternal ( excpStr, exp, extraContext, 0 );
            await WriteLineInternalAsync ( excpStr.ToString () ).ConfigureAwait ( false );
        }

        /// <summary>
        /// Writes an line-break at the end of the output.
        /// </summary>
        public async Task WriteLineAsync () {
            await WriteLineInternalAsync ( string.Empty ).ConfigureAwait ( false );
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public async Task WriteLineAsync ( object? message ) {
            await WriteLineInternalAsync ( message?.ToString () ?? string.Empty ).ConfigureAwait ( false );
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        public async Task WriteLineAsync ( string message ) {
            await WriteLineInternalAsync ( message ).ConfigureAwait ( false );
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public async Task WriteLineAsync ( string format, params IEnumerable<object?> args ) {
            await WriteLineInternalAsync ( string.Format ( provider: null, format, args ) ).ConfigureAwait ( false );
        }
#else
        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public async Task WriteLineAsync ( string format, params object? [] args ) {
            await WriteLineInternalAsync ( string.Format ( provider: null, format, args ) ).ConfigureAwait ( false );
        }
#endif

        /// <summary>
        /// Writes the text format and arguments and appends a line-break at the end into the output, using the specified format provider.
        /// </summary>
        /// <param name="formatProvider">The format provider to use when formatting the string. If null, the current culture is used.</param>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        public async Task WriteLineAsync ( IFormatProvider? formatProvider, string format, params object? [] args ) {
            await WriteLineInternalAsync ( string.Format ( formatProvider, format, args ) ).ConfigureAwait ( false );
        }


        #endregion

        #region Virtual methods
        /// <summary>
        /// Represents the method that intercepts the line that will be written to an output log before being queued for writing.
        /// This method will block if the log queue is full.
        /// </summary>
        /// <param name="line">The line which will be written to the log stream.</param>
        protected virtual void WriteLineInternal ( string line ) {
            if (_isDisposed)
                return;

            string lineText = NormalizeEntries ?
                 line.Normalize ().Trim ().ReplaceLineEndings () : line;

            if (!_channel.Writer.TryWrite ( lineText )) {
                throw new InvalidOperationException ( SR.LogStream_FailedWrite );
            }
        }

        /// <summary>
        /// Represents the asynchronous method that intercepts the line that will be written to an output log before being queued for writing.
        /// </summary>
        /// <param name="line">The line which will be written to the log stream.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        protected virtual async ValueTask WriteLineInternalAsync ( string line ) {
            if (_isDisposed)
                return;

            string lineText = NormalizeEntries ?
                line.Normalize ().Trim ().ReplaceLineEndings () : line;

            try {
                await _channel.Writer.WriteAsync ( lineText, _cancellationTokenSource.Token ).ConfigureAwait ( false );
            }
            catch (ChannelClosedException) {
                // Channel was closed, which is expected during shutdown. Ignore.
            }
        }
        #endregion

        void WriteExceptionInternal ( StringBuilder exceptionSbuilder, Exception exp, string? context = null, int currentDepth = 0 ) {
            if (currentDepth == 0)
                exceptionSbuilder.AppendLine ( SR.Format ( SR.LogStream_ExceptionDump_Header,
                    context is null ? DateTime.Now.ToString ( "R" ) : $"{context}, {DateTime.Now:R}" ) );

            exceptionSbuilder.AppendLine ( exp.ToString () );

            if (exp.InnerException != null) {
                if (currentDepth <= 3) {
                    exceptionSbuilder.AppendLine ( "+++ inner exception +++" );
                    WriteExceptionInternal ( exceptionSbuilder, exp.InnerException, null, currentDepth + 1 );
                }
                else {
                    exceptionSbuilder.AppendLine ( SR.LogStream_ExceptionDump_TrimmedFooter );
                }
            }
        }

        private async Task ProcessQueueAsync () {
            try {
                await foreach (var item in _channel.Reader.ReadAllAsync ( _cancellationTokenSource.Token )) {
                    if (item is FlushSignal flushSignal) {
                        flushSignal.Completion.TrySetResult ( true );
                        continue;
                    }

                    string? dataStr = item?.ToString ();
                    if (dataStr is null)
                        continue;

                    await rotatingPolicyLocker.WaitAsync ( _cancellationTokenSource.Token );
                    try {
                        WriteToOutputs ( dataStr );
                    }
                    finally {
                        rotatingPolicyLocker.Release ();
                    }
                }
            }
            catch (OperationCanceledException) {
            }
            catch (Exception ex) {
                Console.Error.WriteLine ( GetExceptionEntry ( ex, "Unhandled exception in LogStream consumer task." ) );
            }
        }

        private void WriteToOutputs ( string dataStr ) {
            try {
                TextWriter?.WriteLine ( dataStr );
            }
            catch (Exception ex) {
                Console.Error.WriteLine ( GetExceptionEntry ( ex, "Exception from LogStream TextWriter" ) );
            }

            try {
                if (_filePath is not null)
                    File.AppendAllText ( _filePath, dataStr + Environment.NewLine, Encoding );
            }
            catch (Exception ex) {
                Console.Error.WriteLine ( GetExceptionEntry ( ex, $"Exception writing to LogStream file: {_filePath}" ) );
            }

            try {
                _bufferingContent?.Add ( dataStr );
            }
            catch (Exception ex) {
                Console.Error.WriteLine ( GetExceptionEntry ( ex, "Exception from LogStream internal buffer" ) );
            }
        }

        string GetExceptionEntry ( Exception exception, string? context = null ) {
            StringBuilder exceptionSbuilder = new StringBuilder ();
            WriteExceptionInternal ( exceptionSbuilder, exception, context, 0 );
            return exceptionSbuilder.ToString ();
        }

        /// <summary>
        /// Asynchronously writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public async ValueTask DisposeAsync () {
            if (_isDisposed) {
                return;
            }
            _isDisposed = true;

            _channel.Writer.TryComplete ();
            if (!_cancellationTokenSource.IsCancellationRequested) {
                _cancellationTokenSource.Cancel ();
            }

            await _consumerTask;

            TextWriter?.Dispose ();
            rotatingLogPolicy?.Dispose ();
            _cancellationTokenSource.Dispose ();
            rotatingPolicyLocker.Dispose ();
            _bufferingContent = null;

            GC.SuppressFinalize ( this );
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        public void Dispose () {
            if (_isDisposed) {
                return;
            }
            _isDisposed = true;

            _channel.Writer.TryComplete ();
            if (!_cancellationTokenSource.IsCancellationRequested) {
                _cancellationTokenSource.Cancel ();
            }

            _consumerTask.Wait ();

            TextWriter?.Dispose ();
            rotatingLogPolicy?.Dispose ();
            _cancellationTokenSource.Dispose ();
            rotatingPolicyLocker.Dispose ();
            _bufferingContent = null;

            GC.SuppressFinalize ( this );
        }

        /// <inheritdoc/>
        ~LogStream () {
            Dispose ();
        }

        private sealed class FlushSignal {
            public TaskCompletionSource<bool> Completion { get; } = new TaskCompletionSource<bool> ( TaskCreationOptions.RunContinuationsAsynchronously );
        }
    }
}