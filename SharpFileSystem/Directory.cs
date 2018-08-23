using System;

namespace SharpFileSystem {

    /// <summary>
    /// File system directory class.
    /// </summary>
    public class Directory : FileSystemEntity, IEquatable<Directory> {

        /// <summary>
        /// Constructor
        /// </summary>
        public Directory(IFileSystem fileSystem, FileSystemPath path) : base(fileSystem, path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is no directory.", nameof(path));
        }

        #region equals support

        /// <summary>
        /// Return true if the specified object is equal to the current object; otherwise, false.
        /// </summary>
        public bool Equals(Directory other) {
            return ((IEquatable<FileSystemEntity>)this).Equals(other);
        }

        #endregion
    }
}
