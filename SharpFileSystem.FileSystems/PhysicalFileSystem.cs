using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpFileSystem.Collections;

namespace SharpFileSystem.FileSystems {

    public class PhysicalFileSystem : IFileSystem {

        #region properties

        public string PhysicalRoot { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalFileSystem(string physicalRoot) {
            if(!Path.IsPathRooted(physicalRoot)) physicalRoot = Path.GetFullPath(physicalRoot);
            if(physicalRoot[physicalRoot.Length - 1] != Path.DirectorySeparatorChar) physicalRoot = physicalRoot + Path.DirectorySeparatorChar;
            this.PhysicalRoot = physicalRoot;
        }

        public string GetPhysicalPath(FileSystemPath path) {
            return Path.Combine(PhysicalRoot, path.ToString().Remove(0, 1).Replace(FileSystemPath.DirectorySeparatorChar, Path.DirectorySeparatorChar));
        }

        public FileSystemPath GetVirtualFilePath(string physicalPath) {
            if(!physicalPath.StartsWith(PhysicalRoot, StringComparison.InvariantCultureIgnoreCase)) {
                throw new ArgumentException("The specified path is not member of the PhysicalRoot.", nameof(physicalPath));
            }
            string virtualPath = FileSystemPath.DirectorySeparatorChar + physicalPath.Remove(0, PhysicalRoot.Length).Replace(Path.DirectorySeparatorChar, FileSystemPath.DirectorySeparatorChar);
            return FileSystemPath.Parse(virtualPath);
        }

        public FileSystemPath GetVirtualDirectoryPath(string physicalPath) {
            if(!physicalPath.StartsWith(PhysicalRoot, StringComparison.InvariantCultureIgnoreCase)) {
                throw new ArgumentException("The specified path is not member of the PhysicalRoot.", nameof(physicalPath));
            }
            string virtualPath = FileSystemPath.DirectorySeparatorChar + physicalPath.Remove(0, PhysicalRoot.Length).Replace(Path.DirectorySeparatorChar, FileSystemPath.DirectorySeparatorChar);
            if(virtualPath[virtualPath.Length - 1] != FileSystemPath.DirectorySeparatorChar) virtualPath += FileSystemPath.DirectorySeparatorChar;
            return FileSystemPath.Parse(virtualPath);
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            string physicalPath = GetPhysicalPath(path);
            string[] directories = System.IO.Directory.GetDirectories(physicalPath);
            string[] files = System.IO.Directory.GetFiles(physicalPath);
            var virtualDirectories = directories.Select(p => GetVirtualDirectoryPath(p));
            var virtualFiles = files.Select(p => GetVirtualFilePath(p));
            return new EnumerableCollection<FileSystemPath>(virtualDirectories.Concat(virtualFiles), directories.Length + files.Length);
        }

        public bool Exists(FileSystemPath path) {
            return path.IsFile ? System.IO.File.Exists(GetPhysicalPath(path)) : System.IO.Directory.Exists(GetPhysicalPath(path));
        }

        public Stream CreateFile(FileSystemPath path) {
            if(!path.IsFile) throw new ArgumentException("The specified path is not a file.", nameof(path));
            var physicalPath = GetPhysicalPath(path);
            EnsureDirectoryExist(physicalPath);
            return System.IO.File.Create(physicalPath);
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            if(!path.IsFile) throw new ArgumentException("The specified path is not a file.", nameof(path));
            return System.IO.File.Open(GetPhysicalPath(path), FileMode.Open, access);
        }

        public void CreateDirectory(FileSystemPath path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is not a directory.", nameof(path));
            System.IO.Directory.CreateDirectory(GetPhysicalPath(path));
        }

        public void Delete(FileSystemPath path) {
            if(path.IsFile) {
                System.IO.File.Delete(GetPhysicalPath(path));
            } else {
                System.IO.Directory.Delete(GetPhysicalPath(path), true);
            }
        }

        public void Dispose() {
        }

        void EnsureDirectoryExist(string path) {
            var dir = Path.GetDirectoryName(path);
            System.IO.Directory.CreateDirectory(dir);
        }
    }
}


