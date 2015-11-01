using Netool.Network.DataFormats;
using System;

namespace Netool.Logging
{
    /// <summary>
    /// This class is only capable of building one file,
    /// once you build the file (call the Close method) don't use it anymore, but create a new builder.
    /// </summary>
    public class LoggedFileBuilder
    {
        private FileLog log;
        private long id;
        private long hint;

        /// <summary>
        /// Creates new file inside the passed FileLog
        /// </summary>
        /// <param name="log"></param>
        /// <exception cref="ArgumentNullException">log</exception>
        public LoggedFileBuilder(FileLog log)
        {
            if (log == null) throw new ArgumentNullException("log");
            this.log = log;
            var file = log.CreateFile();
            id = file.ID;
            hint = file.Hint;
        }

        /// <summary>
        /// Append data to the logged file
        /// </summary>
        /// <param name="data">data to be appended</param>
        public void Append(IDataStream data)
        {
            log.AppendDataToFile(hint, data);
        }

        /// <summary>
        /// Finish building the file. Don't use this builder after that!
        /// </summary>
        /// <returns></returns>
        public LoggedFile Close()
        {
            var ret = new LoggedFile(id, log);
            id = -1;
            log = null;
            return ret;
        }
    }
}
