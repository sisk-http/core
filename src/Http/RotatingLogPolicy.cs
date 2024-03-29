﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RotatingLogPolicy.cs
// Repository:  https://github.com/sisk-http/core

using System.IO.Compression;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a managed utility for rotating log files by their file size.
    /// </summary>
    /// <definition>
    /// public class RotatingLogPolicy : IDisposable
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class RotatingLogPolicy : IDisposable
    {
        private Thread checkThread;
        private bool isTerminating = false;
        internal LogStream? _logStream;

        /// <summary>
        /// Gets the file size threshold in bytes for when the file will be compressed and then cleared.
        /// </summary>
        /// <definition>
        /// public long MaximumSize { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public long MaximumSize { get; private set; }

        /// <summary>
        /// Gets the time interval between checks.
        /// </summary>
        /// <definition>
        /// public TimeSpan Due { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type> 
        public TimeSpan Due { get; private set; }

        /// <summary>
        /// Creates an new <see cref="RotatingLogPolicy"/> instance with the given <see cref="LogStream"/> object to watch.
        /// </summary>
        /// <definition>
        /// public RotatingLogPolicy(LogStream ls)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type> 
        public RotatingLogPolicy(LogStream ls)
        {
            if (ls.rotatingLogPolicy != null)
            {
                throw new InvalidOperationException(SR.LogStream_RotatingLogPolicy_AlreadyBind);
            }
            this._logStream = ls;
            this._logStream.rotatingLogPolicy = this;
            checkThread = new Thread(new ThreadStart(Check));
            checkThread.IsBackground = true;
        }

        /// <summary>
        /// Defines the time interval and size threshold for starting the task, and then starts the task.
        /// </summary>
        /// <remarks>
        /// The first run is performed immediately after calling this method.
        /// </remarks>
        /// <param name="maximumSize">The non-negative size threshold of the log file size in byte count.</param>
        /// <param name="due">The time interval between checks.</param>
        /// <definition>
        /// public void Configure(long maximumSize, TimeSpan due)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Configure(long maximumSize, TimeSpan due)
        {
            if (string.IsNullOrEmpty(_logStream?.FilePath))
            {
                throw new NotSupportedException(SR.LogStream_RotatingLogPolicy_NotLocalFile);
            }
            if (checkThread.IsAlive)
            {
                throw new NotSupportedException(SR.LogStream_RotatingLogPolicy_AlreadyRunning);
            }
            MaximumSize = maximumSize;
            Due = due;
            checkThread.Start();
        }

        private void Check()
        {
            while (!isTerminating)
            {
                if (_logStream == null) continue;
                string file = _logStream.FilePath!;

                if (File.Exists(file))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Length > MaximumSize)
                    {
                        DateTime now = DateTime.Now;
                        string ext = fileInfo.Extension;
                        string safeDatetime = $"{now.Day:D2}-{now.Month:D2}-{now.Year}T{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}.{now.Millisecond:D4}";
                        string gzippedFilename = $"{fileInfo.FullName}.{safeDatetime}{ext}.gz";

                        try
                        {
                            //  Console.WriteLine("{0,20}{1,20}", "wait queue", "");
                            _logStream.Wait(true);
                            //  Console.WriteLine("{0,20}{1,20}", "gz++", "");
                            using (FileStream logSs = fileInfo.Open(FileMode.OpenOrCreate))
                            using (FileStream gzFileSs = new FileInfo(gzippedFilename).Create())
                            using (GZipStream gzSs = new GZipStream(gzFileSs, CompressionMode.Compress))
                            {
                                logSs.CopyTo(gzSs);
                                logSs.SetLength(0);
                            }
                        }
                        finally
                        {
                            //Console.WriteLine("{0,20}{1,20}", "gz--", "");
                            _logStream.Set();
                        }
                    }
                }

                Thread.Sleep(Due);
            }
        }

        /// <summary>
        /// Waits for the last scheduled run and terminates this class and its resources.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public void Dispose()
        {
            isTerminating = true;
            checkThread.Join();
        }
    }
}
