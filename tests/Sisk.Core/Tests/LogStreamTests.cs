// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogStreamTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using Sisk.Core.Http;

namespace Sisk.Core.Tests {
    [TestClass]
    public class LogStreamTests {
        private string? _tempFilePath;

        [TestInitialize]
        public void Setup () {
            _tempFilePath = Path.Combine ( Path.GetTempPath (), Path.GetRandomFileName () );
        }

        [TestCleanup]
        public void Cleanup () {
            if (File.Exists ( _tempFilePath )) {
                File.Delete ( _tempFilePath );
            }
        }

        [TestMethod]
        public void LogStream_WritesToFile () {
            using (var logStream = new LogStream ( _tempFilePath! )) {
                logStream.WriteLine ( "Test message to file." );
                logStream.Flush ();
            }

            string content = File.ReadAllText ( _tempFilePath! );
            Assert.AreEqual ( "Test message to file." + Environment.NewLine, content );
        }

        [TestMethod]
        public void LogStream_SafeWriteToFile_WritesLinesEndingWithLineBreak () {
            Assert.IsTrue ( LogStream.SafeWriteToFile ( _tempFilePath!, [ "Line 1", "Line 2" ] ) );

            Assert.AreEqual ( $"Line 1{Environment.NewLine}Line 2{Environment.NewLine}", File.ReadAllText ( _tempFilePath! ) );
        }

        [TestMethod]
        public void LogStream_SafeWriteToFile_WritesLinesUsingEncoding () {
            Assert.IsTrue ( LogStream.SafeWriteToFile ( _tempFilePath!, [ "Olá" ], Encoding.Unicode ) );

            Assert.AreEqual ( $"Olá{Environment.NewLine}", File.ReadAllText ( _tempFilePath!, Encoding.Unicode ) );
        }

        [TestMethod]
        public void LogStream_SafeAppendToFile_AppendsText () {
            File.WriteAllText ( _tempFilePath!, "First" );

            Assert.IsTrue ( LogStream.SafeAppendToFile ( _tempFilePath!, " second" ) );
            Assert.IsTrue ( LogStream.SafeAppendToFile ( _tempFilePath!, " third", Encoding.Default ) );

            Assert.AreEqual ( "First second third", File.ReadAllText ( _tempFilePath! ) );
        }

        [TestMethod]
        public void LogStream_SafeAppendToFile_AppendsLinesEndingWithLineBreak () {
            Assert.IsTrue ( LogStream.SafeAppendToFile ( _tempFilePath!, [ "Line 1" ] ) );
            Assert.IsTrue ( LogStream.SafeAppendToFile ( _tempFilePath!, [ "Linha 2" ], Encoding.Default ) );

            Assert.AreEqual ( $"Line 1{Environment.NewLine}Linha 2{Environment.NewLine}", File.ReadAllText ( _tempFilePath! ) );
        }

        [TestMethod]
        public async Task LogStream_SafeWriteToFileAsync_WritesLinesEndingWithLineBreak () {
            Assert.IsTrue ( await LogStream.SafeWriteToFileAsync ( _tempFilePath!, [ "Line 1" ] ) );
            Assert.IsTrue ( await LogStream.SafeWriteToFileAsync ( _tempFilePath!, [ "Linha 2" ], Encoding.Unicode ) );

            Assert.AreEqual ( $"Linha 2{Environment.NewLine}", await File.ReadAllTextAsync ( _tempFilePath!, Encoding.Unicode ) );
        }

        [TestMethod]
        public async Task LogStream_SafeAppendToFileAsync_AppendsText () {
            await File.WriteAllTextAsync ( _tempFilePath!, "First" );

            Assert.IsTrue ( await LogStream.SafeAppendToFileAsync ( _tempFilePath!, " second" ) );
            Assert.IsTrue ( await LogStream.SafeAppendToFileAsync ( _tempFilePath!, " third", Encoding.Default ) );

            Assert.AreEqual ( "First second third", await File.ReadAllTextAsync ( _tempFilePath! ) );
        }

        [TestMethod]
        public async Task LogStream_SafeAppendToFileAsync_AppendsLinesEndingWithLineBreak () {
            Assert.IsTrue ( await LogStream.SafeAppendToFileAsync ( _tempFilePath!, [ "Line 1" ] ) );
            Assert.IsTrue ( await LogStream.SafeAppendToFileAsync ( _tempFilePath!, [ "Linha 2" ], Encoding.Default ) );

            Assert.AreEqual ( $"Line 1{Environment.NewLine}Linha 2{Environment.NewLine}", await File.ReadAllTextAsync ( _tempFilePath! ) );
        }

        [TestMethod]
        public async Task LogStream_SafeFileMethods_SerializeConcurrentAppends () {
            const int operationCount = 40;
            string [] expectedLines = Enumerable.Range ( 0, operationCount ).Select ( i => $"Line {i}" ).ToArray ();
            Task<bool> [] writes = Enumerable.Range ( 0, operationCount ).Select ( i => i switch {
                0 => Task.Run ( () => LogStream.SafeAppendToFile ( _tempFilePath!, $"Line {i}{Environment.NewLine}", Encoding.UTF8 ) ),
                1 => Task.Run ( () => LogStream.SafeAppendToFile ( _tempFilePath!, [ $"Line {i}" ], Encoding.UTF8 ) ),
                _ when (i & 1) == 0 => LogStream.SafeAppendToFileAsync ( _tempFilePath!, $"Line {i}{Environment.NewLine}", Encoding.UTF8 ),
                _ => LogStream.SafeAppendToFileAsync ( _tempFilePath!, [ $"Line {i}" ], Encoding.UTF8 )
            } ).ToArray ();

            bool [] results = await Task.WhenAll ( writes );
            string [] actualLines = await File.ReadAllLinesAsync ( _tempFilePath!, Encoding.UTF8 );

            Assert.IsTrue ( results.All ( result => result ) );
            CollectionAssert.AreEquivalent ( expectedLines, actualLines );
        }

        [TestMethod]
        public void LogStream_WritesToTextWriter () {
            using (var stringWriter = new StringWriter ()) {
                using (var logStream = new LogStream ( stringWriter )) {
                    logStream.WriteLine ( "Test message to TextWriter." );
                    logStream.Flush ();
                }
                Assert.AreEqual ( "Test message to TextWriter." + Environment.NewLine, stringWriter.ToString () );
            }
        }

        [TestMethod]
        public void LogStream_WritesToBothFileAndTextWriter () {
            using (var stringWriter = new StringWriter ()) {
                using (var logStream = new LogStream ( _tempFilePath!, stringWriter )) {
                    logStream.WriteLine ( "Test message to both." );
                    logStream.Flush ();
                }

                string fileContent = File.ReadAllText ( _tempFilePath! );
                Assert.AreEqual ( "Test message to both." + Environment.NewLine, fileContent );
                Assert.AreEqual ( "Test message to both." + Environment.NewLine, stringWriter.ToString () );
            }
        }

        [TestMethod]
        public void LogStream_BufferingAndPeek () {
            using (var logStream = new LogStream ()) {
                logStream.StartBuffering ( 3 );
                logStream.WriteLine ( "Line 1" );
                logStream.WriteLine ( "Line 2" );
                logStream.WriteLine ( "Line 3" );
                logStream.Flush ();

                string nl = Environment.NewLine;
                string expected = $"Line 1{nl}Line 2{nl}Line 3";
                Assert.AreEqual ( expected, logStream.Peek () );

                logStream.WriteLine ( "Line 4" );
                logStream.Flush ();
                expected = $"Line 2{nl}Line 3{nl}Line 4";
                Assert.AreEqual ( expected, logStream.Peek () );

                logStream.StopBuffering ();
                Assert.IsFalse ( logStream.IsBuffering );
                Assert.ThrowsException<InvalidOperationException> ( () => logStream.Peek () );
            }
        }

        [TestMethod]
        public void LogStream_WriteException () {
            using (var stringWriter = new StringWriter ()) {
                using (var logStream = new LogStream ( stringWriter )) {
                    try {
                        throw new InvalidOperationException ( "Test exception message." );
                    }
                    catch (Exception ex) {
                        logStream.WriteException ( ex );
                    }
                    logStream.Flush ();
                }

                string content = stringWriter.ToString ();
                Assert.IsTrue ( content.Contains ( "Test exception message." ) );
                Assert.IsTrue ( content.Contains ( "InvalidOperationException" ) );
            }
        }

        [TestMethod]
        public void LogStream_WriteExceptionWithInnerException () {
            using (var stringWriter = new StringWriter ()) {
                using (var logStream = new LogStream ( stringWriter )) {
                    try {
                        throw new InvalidOperationException ( "Outer exception.", new ArgumentNullException ( "Inner parameter." ) );
                    }
                    catch (Exception ex) {
                        logStream.WriteException ( ex );
                    }
                    logStream.Flush ();
                }

                string content = stringWriter.ToString ();
                Assert.IsTrue ( content.Contains ( "Outer exception." ) );
                Assert.IsTrue ( content.Contains ( "Inner parameter." ) );
                Assert.IsTrue ( content.Contains ( "+++ inner exception +++" ) );
            }
        }

        [TestMethod]
        public async Task LogStream_RaceConditionOnWriting () {
            using (var logStream = new LogStream ( _tempFilePath! )) {
                int numTasks = 10;
                int messagesPerTask = 100;
                Task [] tasks = new Task [ numTasks ];

                for (int i = 0; i < numTasks; i++) {
                    int taskId = i;
                    tasks [ i ] = Task.Run ( () => {
                        for (int j = 0; j < messagesPerTask; j++) {
                            logStream.WriteLine ( $"Task {taskId}, Message {j}" );
                        }
                    } );
                }

                await Task.WhenAll ( tasks );
                await logStream.FlushAsync ();

                string [] lines = File.ReadAllLines ( _tempFilePath! );
                Assert.AreEqual ( numTasks * messagesPerTask, lines.Length );

                // Verify no messages are lost, order is not guaranteed but all should be present
                for (int i = 0; i < numTasks; i++) {
                    for (int j = 0; j < messagesPerTask; j++) {
                        string expectedMessage = $"Task {i}, Message {j}";
                        Assert.IsTrue ( lines.Contains ( expectedMessage ) );
                    }
                }
            }
        }

        [TestMethod]
        public void LogStream_PrefixedLogs () {
            using (var stringWriter = new StringWriter ()) {
                using (var logStream = new PrefixedLogStream ( () => "[PREFIX]", stringWriter )) {
                    logStream.WriteLine ( "Test message." );
                    logStream.Flush ();
                }
                string content = stringWriter.ToString ();
                Assert.IsTrue ( content.Contains ( "[PREFIX]Test message." ) );
            }
        }

        [TestMethod]
        public void LogStream_LogRotation () {
            // This test is tricky and might be flaky due to timing and file system operations.
            // It's designed to be as robust as possible for an automated test.

            // Create a small file to ensure it rotates quickly
            long maxSizeBytes = 100; // Very small size for testing
            TimeSpan dueTime = TimeSpan.FromSeconds ( 1 ); // Check every second

            using (var logStream = new LogStream ( _tempFilePath! )) {
                logStream.ConfigureRotatingPolicy ( maxSizeBytes, dueTime );

                // Write enough data to trigger rotation multiple times
                string message = "This is a test message that will make the file grow. "; // ~50 bytes
                for (int i = 0; i < 5; i++) // Write 5 messages, should be > 100 bytes
                {
                    logStream.WriteLine ( message + i );
                    Thread.Sleep ( 200 ); // Give some time for the background task to process
                }

                // Give some time for the rotation to happen
                Thread.Sleep ( TimeSpan.FromSeconds ( 3 ) );

                // Check if the original file is smaller or a new file was created
                // The exact name of the rotated file is not easily predictable,
                // so we check if the original file is reset or if new files exist.
                // This is a heuristic check.
                long currentFileSize = new FileInfo ( _tempFilePath! ).Length;
                Assert.IsTrue ( currentFileSize <= maxSizeBytes || Directory.GetFiles ( Path.GetDirectoryName ( _tempFilePath! )!, Path.GetFileNameWithoutExtension ( _tempFilePath ) + "*.log" ).Length > 1 );
            }
        }
    }
}
