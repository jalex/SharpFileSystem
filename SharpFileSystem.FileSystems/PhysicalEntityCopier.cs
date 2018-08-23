using System.Threading;
using System.Threading.Tasks;

namespace SharpFileSystem.FileSystems {

    public class PhysicalEntityCopier : IEntityCopier {

        #region properties

        /// <summary>
        /// The size of the buffer that will be used for the move process.
        /// </summary>
        public int BufferSize { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalEntityCopier() {
            this.BufferSize = 81920;
        }

        /// <summary>
        /// Copies an entity from a source file system to a destination file system.
        /// </summary>
        public void Copy(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath) {
            var pSource = (PhysicalFileSystem)source;
            var pDestination = (PhysicalFileSystem)destination;
            var pSourcePath = pSource.GetPhysicalPath(sourcePath);
            var pDestinationPath = pDestination.GetPhysicalPath(destinationPath);
            if(sourcePath.IsFile) {
                System.IO.File.Copy(pSourcePath, pDestinationPath);
            } else {
                destination.CreateDirectory(destinationPath);
                foreach(var e in source.GetEntities(sourcePath)) {
                    source.Copy(e, destination, e.IsFile ? destinationPath.AppendFile(e.EntityName) : destinationPath.AppendDirectory(e.EntityName));
                }
            }
        }

        /// <summary>
        /// Copies an entity from a source file system to a destination file system (async with cancellation token).
        /// </summary>
        public async Task CopyAsync(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            var pSource = (PhysicalFileSystem)source;
            var pDestination = (PhysicalFileSystem)destination;
            var pSourcePath = pSource.GetPhysicalPath(sourcePath);
            var pDestinationPath = pDestination.GetPhysicalPath(destinationPath);
            if(sourcePath.IsFile) {
                await CopyFileAsyncInternal(pSourcePath, pDestinationPath, cancellationToken, BufferSize);
            } else {
                destination.CreateDirectory(destinationPath);
                foreach(var e in source.GetEntities(sourcePath)) {
                    await source.CopyAsync(e, destination, e.IsFile ? destinationPath.AppendFile(e.EntityName) : destinationPath.AppendDirectory(e.EntityName), cancellationToken);
                }
            }
        }

        #region internal

        internal static async Task CopyFileAsyncInternal(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken, int bufferSize) {
            var fileOptions = System.IO.FileOptions.Asynchronous | System.IO.FileOptions.SequentialScan;

            using(var sourceStream = new System.IO.FileStream(sourceFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, bufferSize, fileOptions))
            using(var destinationStream = new System.IO.FileStream(destinationFilePath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write, System.IO.FileShare.None, bufferSize, fileOptions)) {
                await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken);
            }
        }

        #endregion
    }
}
