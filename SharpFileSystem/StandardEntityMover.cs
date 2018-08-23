using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpFileSystem {

    public class StandardEntityMover : IEntityMover {

        #region properties

        /// <summary>
        /// The size of the buffer that will be used for the move process.
        /// </summary>
        public int BufferSize { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public StandardEntityMover() {
            this.BufferSize = 81920;
        }

        /// <summary>
        /// Moves an entity from a source file system to a destination file system.
        /// </summary>
        public void Move(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath) {
            bool isFile = sourcePath.IsFile;
            if(isFile != destinationPath.IsFile) throw new ArgumentException("The specified destination-path is of a different type than the source-path.");

            if(isFile) {
                using(var sourceStream = source.OpenFile(sourcePath, FileAccess.Read))
                using(var destinationStream = destination.CreateFile(destinationPath)) {
                    sourceStream.CopyTo(destinationStream, BufferSize);
                }
                source.Delete(sourcePath);
            } else {
                destination.CreateDirectory(destinationPath);
                foreach(var ep in source.GetEntities(sourcePath).ToArray()) {
                    var destinationEntityPath = ep.IsFile ? destinationPath.AppendFile(ep.EntityName) : destinationPath.AppendDirectory(ep.EntityName);
                    Move(source, ep, destination, destinationEntityPath);
                }
                if(!sourcePath.IsRoot) source.Delete(sourcePath);
            }
        }

        /// <summary>
        /// Moves an entity from a source file system to a destination file system (async with cancellation token).
        /// </summary>
        public async Task MoveAsync(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            bool isFile = sourcePath.IsFile;
            if(isFile != destinationPath.IsFile) throw new ArgumentException("The specified destination-path is of a different type than the source-path.");

            if(isFile) {
                using(var sourceStream = source.OpenFile(sourcePath, FileAccess.Read))
                using(var destinationStream = destination.CreateFile(destinationPath)) {
                    await sourceStream.CopyToAsync(destinationStream, BufferSize, cancellationToken);
                }
                source.Delete(sourcePath);
            } else {
                destination.CreateDirectory(destinationPath);
                foreach(var ep in source.GetEntities(sourcePath).ToArray()) {
                    var destinationEntityPath = ep.IsFile ? destinationPath.AppendFile(ep.EntityName) : destinationPath.AppendDirectory(ep.EntityName);
                    await MoveAsync(source, ep, destination, destinationEntityPath, cancellationToken);
                }
                if(!sourcePath.IsRoot) source.Delete(sourcePath);
            }
        }
    }
}
