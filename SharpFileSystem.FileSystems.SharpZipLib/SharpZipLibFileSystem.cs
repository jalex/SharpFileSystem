using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using SharpFileSystem.IO;

namespace SharpFileSystem.FileSystems.SharpZipLib {

    public class SharpZipLibFileSystem : IFileSystem {

        #region properties

        public ZipFile ZipFile { get; }

        #endregion

        private SharpZipLibFileSystem(ZipFile zipFile) {
            this.ZipFile = zipFile;
        }

        public static SharpZipLibFileSystem Open(Stream s) {
            var seeking = s.CanSeek ? s : new SeekStream(s);
            return new SharpZipLibFileSystem(new ZipFile(seeking));
        }

        public static SharpZipLibFileSystem Create(Stream s) {
            var seeking = s.CanSeek ? s : new SeekStream(s);
            return new SharpZipLibFileSystem(ZipFile.Create(seeking));
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
            BeginUpdate();
            ZipFile.Add(entry, ToEntryPath(path));
            EndUpdate();

            return entry.GetSource();
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            if(access != FileAccess.Read) throw new NotSupportedException();
            return ZipFile.GetInputStream(ToEntry(path));
        }

        public void CreateDirectory(FileSystemPath path) {
            BeginUpdate();
            ZipFile.AddDirectory(ToEntryPath(path));
            EndUpdate();
        }

        public void Delete(FileSystemPath path) {
            ZipFile.Delete(ToEntryPath(path));
        }

        public void Dispose() {
            if(ZipFile.IsUpdating) ZipFile.CommitUpdate();
            ZipFile.Close();
        }

        void BeginUpdate() {
            if(!ZipFile.IsUpdating) ZipFile.BeginUpdate();
        }

        void EndUpdate() {
            if(ZipFile.IsUpdating) ZipFile.CommitUpdate();
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
