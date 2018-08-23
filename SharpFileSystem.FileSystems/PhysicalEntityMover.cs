using System.Threading;
using System.Threading.Tasks;

namespace SharpFileSystem.FileSystems {

    public class PhysicalEntityMover : IEntityMover {

        #region properties

        /// <summary>
        /// The size of the buffer that will be used for the move process.
        /// </summary>
        public int BufferSize { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalEntityMover() {
            this.BufferSize = 81920;
        }

        /// <summary>
        /// Moves an entity from a source file system to a destination file system.
        /// </summary>
        public void Move(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath) {
            var pSource = (PhysicalFileSystem)source;
            var pDestination = (PhysicalFileSystem)destination;
            var pSourcePath = pSource.GetPhysicalPath(sourcePath);
            var pDestinationPath = pDestination.GetPhysicalPath(destinationPath);
            if(sourcePath.IsFile) {
                System.IO.File.Move(pSourcePath, pDestinationPath);
            } else {
                System.IO.Directory.Move(pSourcePath, pDestinationPath);
            }
        }

        /// <summary>
        /// Moves an entity from a source file system to a destination file system (async with cancellation token).
        /// </summary>
        public Task MoveAsync(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            Move(source, sourcePath, destination, destinationPath);
            return Task.CompletedTask;
        }
    }

   
}
