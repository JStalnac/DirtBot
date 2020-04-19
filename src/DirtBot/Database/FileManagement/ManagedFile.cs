using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot.Database.FileManagement
{
    public class ManagedFile : IDisposable
    {
        public string FileName { get; } = null;
        public FileInfo FileInfo { get; }

        FileStream LockStream { get; set; } = null;

        public ManagedFile(string filename)
        {
            FileInfo = new FileInfo(filename);
            FileName = FileInfo.FullName;
        }

        #region IDisposable
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                if (!(LockStream is null))
                    ReleaseLock();
                // TODO: Dispose everything else...
            }

            disposed = true;
        }
        #endregion

        /// <summary>
        /// Deletes the file.
        /// </summary>
        public void Delete()
        {
            // Delete the file.
            FileInfo.Delete();
        }

        /// <summary>
        /// Creates the file.
        /// </summary>
        public FileStream Create() => FileInfo.Create();

        /// <summary>
        /// Ensures that the file exists.
        /// </summary>
        public void EnsureExists()
        {
            if (!FileInfo.Exists)
            {
                Create().Close();
            }
        }

        /// <summary>
        /// Moves the file to a new location.
        /// </summary>
        /// <param name="destFileName">New location</param>
        public void MoveTo(string destFileName) => FileInfo.MoveTo(destFileName);

        /// <summary>
        /// Refreshes the file.
        /// </summary>
        public void Refresh() => FileInfo.Refresh();

        /// <summary>
        /// Tries to acquire a lock file for this file.
        /// </summary>
        /// <returns></returns>
        public bool TryAcquireLock()
        {
            if (LockStream is null)
            {
                try
                {
                    CreateLockFile();
                    LockStream = new FileStream($"{FileName}.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }
            else
            {
                // Lock already acquired
                return true;
            }
        }

        /// <summary>
        /// Releases the acquired lock file for this file.
        /// </summary>
        public void ReleaseLock()
        {
            try
            {
                LockStream.Close();
                LockStream.Dispose();
                LockStream = null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CreateLockFile()
        {
            if (!File.Exists($"{FileName}.lock"))
            {
                FileStream stream = File.Create($"{FileName}.lock");
                byte[] text = Encoding.ASCII.GetBytes("This is a lock file. You may remove it.\n");
                stream.Write(text, 0, text.Length);
                stream.Close();
            }
        }

        /// <summary>
        /// Opens an existing file for reading. Tries to do it retries times.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        public FileStream OpenRead(int retries = 50)
        {
            return RepeatTimes(() =>
            {
                return File.OpenRead(FileName);
            }, times: retries);
        }

        /// <summary>
        /// Opens an existing file or creates a new file for writing. Tries to do it retries times.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        public FileStream OpenWrite(int retries = 50)
        {
            return RepeatTimes(() =>
            {
                return File.OpenWrite(FileName);
            }, times: retries);
        }

        /// <summary>
        /// Opens an existing UTF-8 encoded file for reading. Tries to do it retries times.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        public StreamReader OpenText(int retries = 50)
        {
            return RepeatTimes(() =>
            {
                return File.OpenText(FileName);
            }, times: retries);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and closes the file. If the target file already exists, it is overwritten. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public void WriteAllText(string contents, int retries = 70)
        {
            RepeatTimes(() =>
            {
                File.WriteAllText(FileName, contents);
                return 1;
            }, times: retries);
        }
        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding, and closes the file. If the target file already exists, it is overwritten. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public void WriteAllText(string contents, Encoding encoding, int retries = 70)
        {
            RepeatTimes(() =>
            {
                File.WriteAllText(FileName, contents, encoding);
                return 1;
            }, times: retries);
        }

        /// <summary>
        /// Asynchronously creates a new file, writes the specified string to the file, and closes the file. If the target file already exists, it is overwritten. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public async Task WriteAllTextAsync(string contents, int retries = 70)
        {
            await RepeatTimes(async () =>
            {
                await File.WriteAllTextAsync(FileName, contents);
            }, times: retries);
        }
        /// <summary>
        /// Asynchronously creates a new file, writes the specified string to the file using the specified encoding, and closes the file. If the target file already exists, it is overwritten. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public async Task WriteAllTextAsync(string contents, Encoding encoding, int retries = 70)
        {
            await RepeatTimes(async () =>
            {
                await File.WriteAllTextAsync(FileName, contents, encoding);
            }, times: retries);
        }

        /// <summary>
        /// Opens the file, appends the specified string to the file, and then closes it. If the file does not exist, this method creates the file, writes writes the specified string to the file, then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public void AppendAllText(string contents, int retries = 50)
        {
            RepeatTimes(() =>
            {
                File.AppendAllText(FileName, contents);
                return 1;
            }, times: retries);
        }
        /// <summary>
        /// Opens the file, appends the specified string to the file using the specified encoding, and then closes it. If the file does not exist, this method creates the file, writes writes the specified string to the file, then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public void AppendAllText(string contents, Encoding encoding, int retries = 50)
        {
            RepeatTimes(() =>
            {
                File.AppendAllText(FileName, contents, encoding);
                return 1;
            }, times: retries);
        }

        /// <summary>
        /// Asynchronously opens the file, appends the specified string to the file, and then closes it. If the file does not exist, this method creates the file, writes writes the specified string to the file, then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public async Task AppendAllTextAsync(string contents, int retries = 50)
        {
            await RepeatTimes(async () =>
            {
                await File.AppendAllTextAsync(FileName, contents);
            }, times: retries);
        }
        /// <summary>
        /// Asynchronously opens the file, appends the specified string to the file using the specified encoding, and then closes it. If the file does not exist, this method creates the file, writes writes the specified string to the file, then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="retries"></param>
        public async Task AppendAllTextAsync(string contents, Encoding encoding, int retries = 50)
        {
            await RepeatTimes(async () =>
            {
                await File.AppendAllTextAsync(FileName, contents, encoding);
            }, times: retries);
        }

        /// <summary>
        /// Opens the file, reads all the text in the file, and then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        public string ReadAllText(int retries = 30)
        {
            return RepeatTimes(() =>
            {
                return File.ReadAllText(FileName);
            }, times: retries);
        }
        /// <summary>
        /// Opens the file, reads all the text in the file with the specified encoding, and then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        public string ReadAllText(Encoding encoding, int retries = 30)
        {
            return RepeatTimes(() =>
            {
                return File.ReadAllText(FileName, encoding);
            }, times: retries);
        }

        /// <summary>
        /// Asynchronously opens the file, reads all the text in the file, and then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        public async Task<string> ReadAllTextAsync(int retries = 30)
        {
            return await RepeatTimes(async () =>
            {
                return await File.ReadAllTextAsync(FileName);
            }, times: retries);
        }
        /// <summary>
        /// Asynchronously opens the file, reads all the text in the file with the specified encoding, and then closes the file. Tries to do it retries times.
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        public async Task<string> ReadAllTextAsync(Encoding encoding, int retries = 30)
        {
            return await RepeatTimes(async () =>
            {
                return await File.ReadAllTextAsync(FileName, encoding);
            }, times: retries);
        }

        private TResult RepeatTimes<TResult>(Func<TResult> func, int times, int sleep = 50)
        {
            int i = 0;
            Exception ex;

            do
            {
                try
                {
                    return func.Invoke();
                }
                catch (Exception e)
                {
                    // Don't wait to throw the exception
                    if (i < times)
                        Thread.Sleep(sleep);
                    ex = e;
                }

                i++;
            } while (i < times);

            throw ex;
        }
    }
}
