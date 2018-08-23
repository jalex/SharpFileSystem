using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpFileSystem.Collections;

namespace SharpFileSystem {

    public static class FileSystemExtensions {

        /// <summary>
        /// Opens this <see cref="File"/>.
        /// </summary>
        public static Stream Open(this File file, FileAccess access) {
            return file.FileSystem.OpenFile(file.Path, access);
        }

        /// <summary>
        /// Deletes this <see cref="FileSystemEntity"/>.
        /// </summary>
        public static void Delete(this FileSystemEntity entity) {
            entity.FileSystem.Delete(entity.Path);
        }

        /// <summary>
        /// Gets a collection of entities in this <see cref="Directory"/>.
        /// </summary>
        public static ICollection<FileSystemPath> GetEntityPaths(this Directory directory) {
            return directory.FileSystem.GetEntities(directory.Path);
        }

        /// <summary>
        /// Gets a collection of entities in this <see cref="Directory"/>.
        /// </summary>
        public static ICollection<FileSystemEntity> GetEntities(this Directory directory) {
            var paths = directory.GetEntityPaths();
            return new EnumerableCollection<FileSystemEntity>(paths.Select(p => FileSystemEntity.Create(directory.FileSystem, p)), paths.Count);
        }

        /// <summary>
        /// Gets a collection of entities in the specified <see cref="FileSystemPath"/> (recursive).
        /// </summary>
        public static IEnumerable<FileSystemPath> GetEntitiesRecursive(this IFileSystem fileSystem, FileSystemPath path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is not a directory.");
            foreach(var entity in fileSystem.GetEntities(path)) {
                yield return entity;
                if(entity.IsDirectory) {
                    foreach(var subentity in fileSystem.GetEntitiesRecursive(entity)) {
                        yield return subentity;
                    }
                }
            }
        }

        /// <summary>
        /// Creates or overwrites a directory in the specified <see cref="FileSystemPath"/> (recursive).
        /// </summary>
        public static void CreateDirectoryRecursive(this IFileSystem fileSystem, FileSystemPath path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is not a directory.");
            var currentDirectoryPath = FileSystemPath.Root;
            foreach(var dirName in path.GetDirectorySegments()) {
                currentDirectoryPath = currentDirectoryPath.AppendDirectory(dirName);
                if(!fileSystem.Exists(currentDirectoryPath)) fileSystem.CreateDirectory(currentDirectoryPath);
            }
        }

        #region move extensions

        /// <summary>
        /// Moves an entity from a source file system to a destination file system (async).
        /// </summary>
        public static Task MoveAsync(this IEntityMover mover, IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath) {
            return mover.MoveAsync(source, sourcePath, destination, destinationPath, CancellationToken.None);
        }

        /// <summary>
        /// Moves an entity from this file system to a destination file system.
        /// </summary>
        public static void Move(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            if(!EntityMovers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(), out var mover)) {
                throw new ArgumentException("The specified combination of file-systems is not supported.");
            }
            mover.Move(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath);
        }

        /// <summary>
        /// Moves an entity from this file system to a destination file system (async).
        /// </summary>
        public static Task MoveAsync(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            return sourceFileSystem.MoveAsync(sourcePath, destinationFileSystem, destinationPath, CancellationToken.None);
        }

        /// <summary>
        /// Moves an entity from this file system to a destination file system (async with cancellation token).
        /// </summary>
        public static Task MoveAsync(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            if(!EntityMovers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(), out var mover)) {
                throw new ArgumentException("The specified combination of file-systems is not supported.");
            }
            return mover.MoveAsync(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath, cancellationToken);
        }

        /// <summary>
        /// Moves this entity to a destination file system.
        /// </summary>
        public static void MoveTo(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            entity.FileSystem.Move(entity.Path, destinationFileSystem, destinationPath);
        }

        /// <summary>
        /// Moves this entity to a destination file system (async).
        /// </summary>
        public static Task MoveToAsync(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            return entity.MoveToAsync(destinationFileSystem, destinationPath, CancellationToken.None);
        }

        /// <summary>
        /// Moves this entity to a destination file system (async with cancellation token).
        /// </summary>
        public static Task MoveToAsync(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            return entity.FileSystem.MoveAsync(entity.Path, destinationFileSystem, destinationPath, cancellationToken);
        }

        /// <summary>
        /// Moves this directory to a destination directory.
        /// </summary>
        public static void MoveTo(this Directory source, Directory destination) {
            source.FileSystem.Move(source.Path, destination.FileSystem, destination.Path.AppendDirectory(source.Path.EntityName));
        }

        /// <summary>
        /// Moves this directory to a destination directory (async).
        /// </summary>
        public static Task MoveToAsync(this Directory source, Directory destination) {
            return source.MoveToAsync(destination, CancellationToken.None);
        }

        /// <summary>
        /// Moves this directory to a destination directory (async with cancellation token).
        /// </summary>
        public static Task MoveToAsync(this Directory source, Directory destination, CancellationToken cancellationToken) {
            return source.FileSystem.MoveAsync(source.Path, destination.FileSystem, destination.Path.AppendDirectory(source.Path.EntityName), cancellationToken);
        }

        /// <summary>
        /// Moves this file to a destination directory.
        /// </summary>
        public static void MoveTo(this File source, Directory destination) {
            source.FileSystem.Move(source.Path, destination.FileSystem, destination.Path.AppendFile(source.Path.EntityName));
        }

        /// <summary>
        /// Moves this file to a destination directory (async).
        /// </summary>
        public static Task MoveToAsync(this File source, Directory destination) {
            return source.MoveToAsync(destination, CancellationToken.None);
        }

        /// <summary>
        /// Moves this file to a destination directory (async with cancellation token).
        /// </summary>
        public static Task MoveToAsync(this File source, Directory destination, CancellationToken cancellationToken) {
            return source.FileSystem.MoveAsync(source.Path, destination.FileSystem, destination.Path.AppendFile(source.Path.EntityName), cancellationToken);
        }

        #endregion

        #region copy extensions

        /// <summary>
        /// Copies an entity from a source file system to a destination file system (async).
        /// </summary>
        public static Task CopyAsync(this IEntityCopier copier, IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath) {
            return copier.CopyAsync(source, sourcePath, destination, destinationPath, CancellationToken.None);
        }

        /// <summary>
        /// Copies an entity from this file system to a destination file system.
        /// </summary>
        public static void Copy(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            if(!EntityCopiers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(), out var copier)) {
                throw new ArgumentException("The specified combination of file-systems is not supported.");
            }
            copier.Copy(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath);
        }

        /// <summary>
        /// Copies an entity from this file system to a destination file system (async).
        /// </summary>
        public static Task CopyAsync(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            return sourceFileSystem.CopyAsync(sourcePath, destinationFileSystem, destinationPath, CancellationToken.None);
        }

        /// <summary>
        /// Copies an entity from this file system to a destination file system (async with cancellation token).
        /// </summary>
        public static Task CopyAsync(this IFileSystem sourceFileSystem, FileSystemPath sourcePath, IFileSystem destinationFileSystem, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            if(!EntityCopiers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(), out var copier)) {
                throw new ArgumentException("The specified combination of file-systems is not supported.");
            }
            return copier.CopyAsync(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath, cancellationToken);
        }

        /// <summary>
        /// Copies this entity to a destination file system.
        /// </summary>
        public static void CopyTo(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            entity.FileSystem.Copy(entity.Path, destinationFileSystem, destinationPath);
        }

        /// <summary>
        /// Copies this entity to a destination file system (async).
        /// </summary>
        public static Task CopyToAsync(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath) {
            return entity.CopyToAsync(destinationFileSystem, destinationPath, CancellationToken.None);
        }

        /// <summary>
        /// Copies this entity to a destination file system (async with cancellation token).
        /// </summary>
        public static Task CopyToAsync(this FileSystemEntity entity, IFileSystem destinationFileSystem, FileSystemPath destinationPath, CancellationToken cancellationToken) {
            return entity.FileSystem.CopyAsync(entity.Path, destinationFileSystem, destinationPath, cancellationToken);
        }

        /// <summary>
        /// Copies this directory to a destination directory.
        /// </summary>
        public static void CopyTo(this Directory source, Directory destination) {
            source.FileSystem.Copy(source.Path, destination.FileSystem, destination.Path.AppendDirectory(source.Path.EntityName));
        }

        /// <summary>
        /// Copies this directory to a destination directory (async).
        /// </summary>
        public static Task CopyToAsync(this Directory source, Directory destination) {
            return source.CopyToAsync(destination, CancellationToken.None);
        }

        /// <summary>
        /// Copies this directory to a destination directory (async with cancellation token).
        /// </summary>
        public static Task CopyToAsync(this Directory source, Directory destination, CancellationToken cancellationToken) {
            return source.FileSystem.CopyAsync(source.Path, destination.FileSystem, destination.Path.AppendDirectory(source.Path.EntityName), cancellationToken);
        }

        /// <summary>
        /// Copies this file to a destination directory.
        /// </summary>
        public static void CopyTo(this File source, Directory destination) {
            source.FileSystem.Copy(source.Path, destination.FileSystem, destination.Path.AppendFile(source.Path.EntityName));
        }

        /// <summary>
        /// Copies this file to a destination directory (async).
        /// </summary>
        public static Task CopyToAsync(this File source, Directory destination) {
            return source.CopyToAsync(destination, CancellationToken.None);
        }

        /// <summary>
        /// Copies this file to a destination directory (async with cancellation token).
        /// </summary>
        public static Task CopyToAsync(this File source, Directory destination, CancellationToken cancellationToken) {
            return source.FileSystem.CopyAsync(source.Path, destination.FileSystem, destination.Path.AppendFile(source.Path.EntityName), cancellationToken);
        }

        #endregion
    }
}
