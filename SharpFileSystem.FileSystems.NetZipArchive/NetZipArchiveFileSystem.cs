using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SharpFileSystem.FileSystems.NetZipArchive {

    public class NetZipArchiveFileSystem : IFileSystem {

        #region properties

        public ZipArchive ZipArchive { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        NetZipArchiveFileSystem(ZipArchive archive) {
            this.ZipArchive = archive;
        }

        public static NetZipArchiveFileSystem Open(Stream s) {
            return new NetZipArchiveFileSystem(new ZipArchive(s, ZipArchiveMode.Update, true));
        }

        public static NetZipArchiveFileSystem Create(Stream s) {
            return new NetZipArchiveFileSystem(new ZipArchive(s, ZipArchiveMode.Create, true));
        }
        
        protected IEnumerable<ZipArchiveEntry> GetZipEntries() {
            return ZipArchive.Entries;
        }

        protected FileSystemPath ToPath(ZipArchiveEntry entry) {
            return FileSystemPath.Parse(FileSystemPath.DirectorySeparatorChar + entry.FullName);
        }

        protected string ToEntryPath(FileSystemPath path) {
            // Remove heading '/' from path.
            return path.Path.TrimStart(FileSystemPath.DirectorySeparatorChar);
        }

        protected ZipArchiveEntry ToEntry(FileSystemPath path) {
            return ZipArchive.GetEntry(ToEntryPath(path));
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            return GetZipEntries().Select(ToPath).Where(path.IsParentOf)
                .Select(entryPath => entryPath.ParentPath == path ? entryPath : path.AppendDirectory(entryPath.RemoveParent(path).GetDirectorySegments()[0]))
                .Distinct()
                .ToList();
        }

        public bool Exists(FileSystemPath path) {
            if(path.IsFile) return ToEntry(path) != null;
            return GetZipEntries()
                .Select(ToPath)
                .Any(entryPath => entryPath.IsChildOf(path));
            //foreach(var zipArchiveEntry in GetZipEntries()) {
            //    var p = ToPath(zipArchiveEntry);
            //    if(p.IsChildOf(path)) return true;
            //}
            //return false;
        }

        public Stream CreateFile(FileSystemPath path) {
            var zae = ZipArchive.CreateEntry(ToEntryPath(path));
            return zae.Open();
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            var zae = ZipArchive.GetEntry(ToEntryPath(path));
            return zae.Open();
        }

        public void CreateDirectory(FileSystemPath path) {
            ZipArchive.CreateEntry(ToEntryPath(path));
        }

        public void Delete(FileSystemPath path) {
            var zae = ZipArchive.GetEntry(ToEntryPath(path));
            zae.Delete();
        }

        public void Dispose() {
            ZipArchive.Dispose();
        }
    }
}
