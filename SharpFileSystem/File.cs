using System;

namespace SharpFileSystem {

    /// <summary>
    /// File system file class.
    /// </summary>
    public class File : FileSystemEntity, IEquatable<File> {

        /// <summary>
        /// Constructor
        /// </summary>
        public File(IFileSystem fileSystem, FileSystemPath path) : base(fileSystem, path) {
            if(!path.IsFile) throw new ArgumentException("The specified path is no file.", "path");
        }

        #region equals support

        /// <summary>
        /// Return true if the specified object is equal to the current object; otherwise, false.
        /// </summary>
        public bool Equals(File other) {
            return ((IEquatable<FileSystemEntity>)this).Equals(other);
        }

        #endregion
    }
}
