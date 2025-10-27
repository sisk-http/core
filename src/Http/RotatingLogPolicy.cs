// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RotatingLogPolicy.cs
// Repository:  https://github.com/sisk-http/core

using System.Timers;

namespace Sisk.Core.Http {
    /// <summary>
    /// Provides a managed utility for rotating log files by their file size.
    /// </summary>
    public sealed class RotatingLogPolicy : IDisposable {
        private System.Timers.Timer? checkTimer;
        internal LogStream? _logStream;
        private bool disposedValue;

        /// <summary>
        /// Gets the file size threshold in bytes for when the file will be compressed and then cleared.
        /// </summary>
        public long MaximumSize { get; private set; }

        /// <summary>
        /// Gets the time interval between checks.
        /// </summary>
        public TimeSpan Due { get; private set; }

        /// <summary>
        /// Gets or sets the compressor used to compress the log files.
        /// </summary>
        public RotatingLogPolicyCompressor Compressor { get; set; } = new GZipRotatingLogPolicyCompressor ();

        /// <summary>
        /// Creates an new <see cref="RotatingLogPolicy"/> instance with the given <see cref="LogStream"/> object to watch.
        /// </summary>
        public RotatingLogPolicy ( LogStream ls ) {
            if (ls.rotatingLogPolicy != null) {
                throw new InvalidOperationException ( SR.LogStream_RotatingLogPolicy_AlreadyBind );
            }
            _logStream = ls;
            _logStream.rotatingLogPolicy = this;
        }

        /// <summary>
        /// Defines the time interval and size threshold for starting the task, and then starts the task.
        /// </summary>
        /// <remarks>
        /// The first run is performed immediately after calling this method.
        /// </remarks>
        /// <param name="maximumSize">The non-negative size threshold of the log file size in byte count.</param>
        /// <param name="due">The time interval between checks.</param>
        /// <param name="compressor">The optional compressor to use for log file compression. If <see langword="null"/>, the default compressor (GZip) will be used.</param>
        public void Configure ( long maximumSize, TimeSpan due, RotatingLogPolicyCompressor? compressor = null ) {
            if (checkTimer?.Enabled == true) {
                return;
            }
            if (string.IsNullOrEmpty ( _logStream?.FilePath )) {
                throw new NotSupportedException ( SR.LogStream_RotatingLogPolicy_NotLocalFile );
            }
            if (due == TimeSpan.Zero) {
                throw new ArgumentException ( SR.LogStream_RotatingLogPolicy_IntervalZero );
            }

            MaximumSize = maximumSize;
            Due = due;
            Compressor = compressor ?? new GZipRotatingLogPolicyCompressor ();

            checkTimer ??= new System.Timers.Timer () {
                AutoReset = false
            };

            checkTimer.Interval = Due.TotalMilliseconds;
            checkTimer.Elapsed += Check;
            checkTimer.Start ();
        }

        private void Check ( object? state, ElapsedEventArgs e ) {
            if (disposedValue)
                return;
            if (_logStream is null || _logStream.Disposed)
                return;
            if (checkTimer is null)
                return;

            string file = _logStream.FilePath!;

            if (File.Exists ( file )) {
                FileInfo fileInfo = new FileInfo ( file );

                if (fileInfo.Length > MaximumSize) {

                    DateTime now = DateTime.Now;

                    string ext = fileInfo.Extension;
                    string safeDatetime = $"{now.Day:D2}-{now.Month:D2}-{now.Year}T{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}.{now.Millisecond:D4}";
                    string preFormattedFileName = $"{fileInfo.FullName}.{safeDatetime}{ext}";
                    string finalFileName = Compressor.GetCompressedFileName ( preFormattedFileName );

                    try {
                        _logStream.rotatingPolicyLocker.Wait ();

                        using (FileStream baseLogStream = fileInfo.Open ( FileMode.OpenOrCreate ))
                        using (FileStream compressedFileStream = new FileInfo ( finalFileName ).Create ())
                        using (Stream compressingStream = Compressor.GetCompressingStream ( compressedFileStream )) {
                            if (baseLogStream.CanRead)
                                baseLogStream.CopyTo ( compressingStream );

                            if (baseLogStream.CanWrite)
                                baseLogStream.SetLength ( 0 );
                        }
                    }
                    catch (Exception ex) {
                        _logStream.WriteException ( ex, $"Raised from RotatingLogPolicy while compressing log file '{fileInfo.FullName}'." );
                    }
                    finally {
                        _logStream.rotatingPolicyLocker.Release ();
                    }
                }
            }

            checkTimer.Interval = Due.TotalMilliseconds;
            checkTimer.Start ();
        }

        private void Dispose ( bool disposing ) {
            if (!disposedValue) {
                if (disposing) {
                    checkTimer?.Dispose ();
                    _logStream?.Dispose ();
                }

                _logStream = null;
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose () {
            Dispose ( disposing: true );
            GC.SuppressFinalize ( this );
        }
    }
}
