using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SharpFileSystem.FileSystems.NetZipArchive {

    public class NetZipArchiveFileSystem : IFileSystem {
        readonly bool _leaveArhiveOpen;

        #region properties

        public ZipArchive ZipArchive { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="archive">A <see cref="ZipArchive"/> object.</param>
        /// <param name="leaveArhiveOpen">
        /// <see langword="true"/> to leave the archive open after the <see cref="NetZipArchiveFileSystem"/> object is disposed; otherwise, <see langword="false"/>.
        /// </param>
        public NetZipArchiveFileSystem(ZipArchive archive, bool leaveArhiveOpen = false) {
            _leaveArhiveOpen = leaveArhiveOpen;
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
            if(!_leaveArhiveOpen) ZipArchive.Dispose();
        }
    }
}
