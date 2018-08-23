using System;
using System.Collections.Generic;
using System.IO;

namespace SharpFileSystem {

    public interface IFileSystem: IDisposable {

        /// <summary>
        /// Gets a collection of entities in the specified <see cref="FileSystemPath"/>.
        /// </summary>
        ICollection<FileSystemPath> GetEntities(FileSystemPath path);

        /// <summary>
        /// Returns true if the specified <see cref="FileSystemPath"/> exists; otherwise, false.
        /// </summary>
        bool Exists(FileSystemPath path);

        /// <summary>
        /// Creates or overwrites a file in the specified <see cref="FileSystemPath"/>.
        /// </summary>
        Stream CreateFile(FileSystemPath path);

        /// <summary>
        /// Opens a file on the specified <see cref="FileSystemPath"/>.
        /// </summary>
        Stream OpenFile(FileSystemPath path, FileAccess access);

        /// <summary>
        /// Creates or overwrites a directory in the specified <see cref="FileSystemPath"/>.
        /// </summary>
        void CreateDirectory(FileSystemPath path);

        /// <summary>
        /// Deletes an entity on the specified <see cref="FileSystemPath"/>.
        /// </summary>
        void Delete(FileSystemPath path);
    }
}
