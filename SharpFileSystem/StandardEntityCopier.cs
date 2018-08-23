using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SharpFileSystem {

    public class StandardEntityCopier : IEntityCopier {

        #region properties

        /// <summary>
        /// The size of the buffer that will be used for the copy process.
        /// </summary>
        public int BufferSize { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public StandardEntityCopier() {
            this.BufferSize = 81920;
        }

        /// <summary>
        /// Copies an entity from a source file system to a destination file system.
        /// </summary>
        public void Copy(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath) {
            bool isFile = sourcePath.IsFile;
            if(isFile != destinationPath.IsFile) throw new ArgumentException("The specified destination-path is of a different type than the source-path.");

            if(isFile) {
                using(var sourceStream = source.OpenFile(sourcePath, FileAccess.Read))
                using(var destinationStream = destination.CreateFile(destinationPath)) {
                    sourceStream.CopyTo(destinationStream, BufferSize);
                }
            } else {
                if(!destinationPath.IsRoot) destination.CreateDirectory(destinationPath);
                foreach(var ep in source.GetEntities(sourcePath)) {
                    var destinationEntityPath = ep.IsFile ? destinationPath.AppendFile(ep.EntityName) : destinationPath.AppendDirectory(ep.EntityName);
                    Copy(source, ep, destination, destinationEntityPath);
                }
            }
        }

        /// <summary>
        /// Copies an entity from a source file system to a destination file system (async).
        /// </summary>
        public Task CopyAsync(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath) {
            return CopyAsync(source, sourcePath, destination, destinationPath, CancellationToken.None);
        }

        /// <summary>
        /// Copies an entity from a source file system to a destination file system (async with cancellation token).
        /// </summary>
        public async Task CopyAsync(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            bool isFile = sourcePath.IsFile;
            if(isFile != destinationPath.IsFile) throw new ArgumentException("The specified destination-path is of a different type than the source-path.");

            if(isFile) {
                using(var sourceStream = source.OpenFile(sourcePath, FileAccess.Read))
                using(var destinationStream = destination.CreateFile(destinationPath)) {
                    await sourceStream.CopyToAsync(destinationStream, BufferSize, cancellationToken);
                }
            } else {
                if(!destinationPath.IsRoot) destination.CreateDirectory(destinationPath);
                foreach(var ep in source.GetEntities(sourcePath)) {
                    var destinationEntityPath = ep.IsFile ? destinationPath.AppendFile(ep.EntityName) : destinationPath.AppendDirectory(ep.EntityName);
                    await CopyAsync(source, ep, destination, destinationEntityPath, cancellationToken);
                }
            }
        }
    }
}
