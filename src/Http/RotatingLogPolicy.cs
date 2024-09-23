// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RotatingLogPolicy.cs
// Repository:  https://github.com/sisk-http/core

using System.IO.Compression;
using System.Timers;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a managed utility for rotating log files by their file size.
    /// </summary>
    public sealed class RotatingLogPolicy : IDisposable
    {
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
        /// Creates an new <see cref="RotatingLogPolicy"/> instance with the given <see cref="LogStream"/> object to watch.
        /// </summary>
        public RotatingLogPolicy(LogStream ls)
        {
            if (ls.rotatingLogPolicy != null)
            {
                throw new InvalidOperationException(SR.LogStream_RotatingLogPolicy_AlreadyBind);
            }
            this._logStream = ls;
            this._logStream.rotatingLogPolicy = this;
        }

        /// <summary>
        /// Defines the time interval and size threshold for starting the task, and then starts the task.
        /// </summary>
        /// <remarks>
        /// The first run is performed immediately after calling this method.
        /// </remarks>
        /// <param name="maximumSize">The non-negative size threshold of the log file size in byte count.</param>
        /// <param name="due">The time interval between checks.</param>
        public void Configure(long maximumSize, TimeSpan due)
        {
            if (this.checkTimer?.Enabled == true)
            {
                return;
            }
            if (string.IsNullOrEmpty(this._logStream?.FilePath))
            {
                throw new NotSupportedException(SR.LogStream_RotatingLogPolicy_NotLocalFile);
            }
            if (due == TimeSpan.Zero)
            {
                throw new ArgumentException(SR.LogStream_RotatingLogPolicy_IntervalZero);
            }

            this.MaximumSize = maximumSize;
            this.Due = due;

            if (this.checkTimer is null)
                this.checkTimer = new System.Timers.Timer()
                {
                    AutoReset = false
                };

            this.checkTimer.Interval = this.Due.TotalMilliseconds;
            this.checkTimer.Elapsed += this.Check;
            this.checkTimer.Start();
        }

        private void Check(object? state, ElapsedEventArgs e)
        {
            if (this.disposedValue) return;
            if (this._logStream is null || this._logStream.Disposed) return;
            if (this.checkTimer is null) return;

            string file = this._logStream.FilePath!;

            if (File.Exists(file))
            {
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Length > this.MaximumSize)
                {
                    DateTime now = DateTime.Now;
                    string ext = fileInfo.Extension;
                    string safeDatetime = $"{now.Day:D2}-{now.Month:D2}-{now.Year}T{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}.{now.Millisecond:D4}";
                    string gzippedFilename = $"{fileInfo.FullName}.{safeDatetime}{ext}.gz";

                    try
                    {
                        this._logStream.rotatingPolicyLocker.Reset();

                        using (FileStream logSs = fileInfo.Open(FileMode.OpenOrCreate))
                        using (FileStream gzFileSs = new FileInfo(gzippedFilename).Create())
                        using (GZipStream gzSs = new GZipStream(gzFileSs, CompressionMode.Compress))
                        {
                            if (logSs.CanRead)
                                logSs.CopyTo(gzSs);

                            if (logSs.CanWrite)
                                logSs.SetLength(0);
                        }
                    }
                    catch
                    {
                        ;
                    }
                    finally
                    {
                        this._logStream.rotatingPolicyLocker.Set();
                    }
                }
            }

            this.checkTimer.Interval = this.Due.TotalMilliseconds;
            this.checkTimer.Start();
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.checkTimer?.Dispose();
                    this._logStream?.Dispose();
                }

                this._logStream = null;
                this.disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
