using System;

namespace SharpFileSystem {

    /// <summary>
    /// File system entity class.
    /// </summary>
    public class FileSystemEntity: IEquatable<FileSystemEntity> {

        #region properties

        /// <summary>
        /// A file system of this entity.
        /// </summary>
        public IFileSystem FileSystem { get; }

        /// <summary>
        /// A path of this entity.
        /// </summary>
        public FileSystemPath Path { get; }

        /// <summary>
        /// A name of this entity.
        /// </summary>
        public string Name => Path.EntityName;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public FileSystemEntity(IFileSystem fileSystem, FileSystemPath path) {
            this.FileSystem = fileSystem;
            this.Path = path;
        }

        /// <summary>
        /// Create a new file system entity.
        /// </summary>
        public static FileSystemEntity Create(IFileSystem fileSystem, FileSystemPath path) {
            if(path.IsFile) {
                return new File(fileSystem, path);
            } else {
                return new Directory(fileSystem, path);
            }
        }

        #region equals support

        /// <summary>
        /// Return true if the specified object is equal to the current object; otherwise, false.
        /// </summary>
        public override bool Equals(object obj) {
            return obj is FileSystemEntity other && 
                   this.Equals(other);
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return FileSystem.GetHashCode() ^ Path.GetHashCode();
        }

        /// <summary>
        /// Return true if the specified object is equal to the current object; otherwise, false.
        /// </summary>
        public bool Equals(FileSystemEntity other) {
            return FileSystem.Equals(other.FileSystem) && 
                   Path.Equals(other.Path);
        }

        #endregion
    }
}
