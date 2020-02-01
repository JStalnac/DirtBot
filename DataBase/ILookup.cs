using System;
using System.Collections.Generic;
using System.Text;

namespace DirtBot.DataBase
{
    public interface ILookup
    {
        /// <summary>
        /// Returns the full name of the file, including the extension.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Returns the name of the data file (excluding the extension).
        /// </summary>
        public string FileName { get; }

        public string FileType { get; }
        private const string TEMPLATE_IDENTIFIER = "template-";

        public void LoadData();
        public void EnsureStorageFile();
    }
}
