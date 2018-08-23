using System.Threading;
using System.Threading.Tasks;

namespace SharpFileSystem {

    public interface IEntityCopier {

        /// <summary>
        /// Copies an entity from a source file system to a destination file system.
        /// </summary>
        void Copy(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath);

        /// <summary>
        /// Copies an entity from a source file system to a destination file system (async with cancellation token).
        /// </summary>
        Task CopyAsync(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath, CancellationToken cancellationToken);
    }
}
