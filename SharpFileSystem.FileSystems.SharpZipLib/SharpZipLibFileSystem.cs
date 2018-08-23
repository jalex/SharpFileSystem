using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace SharpFileSystem.FileSystems.SharpZipLib {

    public class SharpZipLibFileSystem : IFileSystem {

        #region properties

        public ZipFile ZipFile { get; }

        #endregion

        private SharpZipLibFileSystem(ZipFile zipFile) {
            this.ZipFile = zipFile;
        }

        public static SharpZipLibFileSystem Open(Stream s) {
            return new SharpZipLibFileSystem(new ZipFile(s));
        }

        public static SharpZipLibFileSystem Create(Stream s) {
            return new SharpZipLibFileSystem(ZipFile.Create(s));
        }

        protected FileSystemPath ToPath(ZipEntry entry) {
            return FileSystemPath.Parse(FileSystemPath.DirectorySeparatorChar + entry.Name);
        }

        protected string ToEntryPath(FileSystemPath path) {
            // Remove heading '/' from path.
            return path.Path.TrimStart(FileSystemPath.DirectorySeparatorChar);
        }

        protected ZipEntry ToEntry(FileSystemPath path) {
            return ZipFile.GetEntry(ToEntryPath(path));
        }

        protected IEnumerable<ZipEntry> GetZipEntries() {
            return ZipFile.Cast<ZipEntry>();
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            return GetZipEntries()
                .Select(ToPath)
                .Where(entryPath => path.IsParentOf(entryPath))
                .Select(entryPath => entryPath.ParentPath == path ? entryPath : path.AppendDirectory(entryPath.RemoveParent(path).GetDirectorySegments()[0]))
                .Distinct()
                .ToList();
        }

        public bool Exists(FileSystemPath path) {
            if(path.IsFile) return ToEntry(path) != null;
            return GetZipEntries()
                .Select(ToPath)
                .Any(entryPath => entryPath.IsChildOf(path));
        }

        public Stream CreateFile(FileSystemPath path) {
            var entry = new MemoryZipEntry();
            ZipFile.Add(entry, ToEntryPath(path));
            return entry.GetSource();
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            if(access != FileAccess.Read) throw new NotSupportedException();
            return ZipFile.GetInputStream(ToEntry(path));
        }

        public void CreateDirectory(FileSystemPath path) {
            ZipFile.AddDirectory(ToEntryPath(path));
        }

        public void Delete(FileSystemPath path) {
            ZipFile.Delete(ToEntryPath(path));
        }

        public void Dispose() {
            if(ZipFile.IsUpdating) ZipFile.CommitUpdate();
            ZipFile.Close();
        }

        #region sub classes

        public class MemoryZipEntry : MemoryFileSystem.MemoryFile, IStaticDataSource {

            public Stream GetSource() {
                return new MemoryFileSystem.MemoryFileStream(this);
            }
        }

        #endregion
    }
}
