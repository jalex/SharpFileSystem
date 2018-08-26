using System;

namespace SharpFileSystem {

    /// <summary>
    /// File system file class.
    /// </summary>
    public class FileSystemFile : FileSystemEntity, IEquatable<FileSystemFile> {

        /// <summary>
        /// Constructor
        /// </summary>
        public FileSystemFile(IFileSystem fileSystem, FileSystemPath path) : base(fileSystem, path) {
            if(!path.IsFile) throw new ArgumentException("The specified path is no file.", nameof(path));
        }

        #region equals support

        /// <summary>
        /// Return true if the specified object is equal to the current object; otherwise, false.
        /// </summary>
        public bool Equals(FileSystemFile other) {
            return ((IEquatable<FileSystemEntity>)this).Equals(other);
        }

        #endregion
    }
}
