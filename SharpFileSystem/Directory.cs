using System;

namespace SharpFileSystem {

    /// <summary>
    /// File system directory class.
    /// </summary>
    public class FileSystemDirectory : FileSystemEntity, IEquatable<FileSystemDirectory> {

        /// <summary>
        /// Constructor
        /// </summary>
        public FileSystemDirectory(IFileSystem fileSystem, FileSystemPath path) : base(fileSystem, path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is no directory.", nameof(path));
        }

        #region equals support

        /// <summary>
        /// Return true if the specified object is equal to the current object; otherwise, false.
        /// </summary>
        public bool Equals(FileSystemDirectory other) {
            return ((IEquatable<FileSystemEntity>)this).Equals(other);
        }

        #endregion
    }
}
