using System.Collections.Generic;
using System.IO;

namespace SharpFileSystem.FileSystems {

    public class SealedFileSystem: IFileSystem {
        readonly IFileSystem _parent;

        /// <summary>
        /// Constructor
        /// </summary>
        public SealedFileSystem(IFileSystem parent) {
            _parent = parent;
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            return _parent.GetEntities(path);
        }

        public bool Exists(FileSystemPath path) {
            return _parent.Exists(path);
        }

        public Stream CreateFile(FileSystemPath path) {
            return _parent.CreateFile(path);
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            return _parent.OpenFile(path, access);
        }

        public void CreateDirectory(FileSystemPath path) {
            _parent.CreateDirectory(path);
        }

        public void Delete(FileSystemPath path) {
            _parent.Delete(path);
        }

        public void Dispose() {
            _parent.Dispose();
        }
    }
}
