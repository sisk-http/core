// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RotatingLogPolicy.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.IO.Compression;

namespace Sisk.ServiceProvider
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
        private readonly Thread checkThread;
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
        /// public RotatingLogPolicy(LogStream? ls)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type> 
        public RotatingLogPolicy(LogStream? ls)
        {
            this._logStream = ls;
            this.checkThread = new Thread(new ThreadStart(this.Check));
            this.checkThread.IsBackground = true;
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
            if (string.IsNullOrEmpty(this._logStream?.FilePath))
            {
                throw new NotSupportedException("Cannot link an rotaging log policy to an log stream which ins't pointing to an local file.");
            }
            this.MaximumSize = maximumSize;
            this.Due = due;
            this.checkThread.Start();
        }

        private void Check()
        {
            while (!this.isTerminating)
            {
                if (this._logStream == null) continue;
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
                            //  Console.WriteLine("{0,20}{1,20}", "wait queue", "");
                            this._logStream.Wait(true);
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
                            this._logStream.Set();
                        }
                    }
                }

                Thread.Sleep(this.Due);
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
            this.isTerminating = true;
            this.checkThread.Join();
        }
    }
}
