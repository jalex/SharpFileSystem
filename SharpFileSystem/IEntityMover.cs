using System.Threading;
using System.Threading.Tasks;

namespace SharpFileSystem {

    public interface IEntityMover {

        /// <summary>
        /// Moves an entity from a source file system to a destination file system.
        /// </summary>
        void Move(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath);

        /// <summary>
        /// Moves an entity from a source file system to a destination file system (async with cancellation token).
        /// </summary>
        Task MoveAsync(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath, CancellationToken cancellationToken);
    }
}
