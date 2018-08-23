using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpFileSystem.FileSystems {

    public class MemoryFileSystem : IFileSystem {
        readonly IDictionary<FileSystemPath, ISet<FileSystemPath>> _directories = new Dictionary<FileSystemPath, ISet<FileSystemPath>>();
        readonly IDictionary<FileSystemPath, MemoryFile> _files = new Dictionary<FileSystemPath, MemoryFile>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryFileSystem() {
            _directories.Add(FileSystemPath.Root, new HashSet<FileSystemPath>());
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is no directory.", "path");
            if(!_directories.TryGetValue(path, out var subentities)) throw new DirectoryNotFoundException();
            return subentities;
        }

        public bool Exists(FileSystemPath path) {
            return path.IsDirectory ? _directories.ContainsKey(path) : _files.ContainsKey(path);
        }

        public Stream CreateFile(FileSystemPath path) {
            if(!path.IsFile) throw new ArgumentException("The specified path is no file.", "path");
            if(!_directories.ContainsKey(path.ParentPath)) throw new DirectoryNotFoundException();
            _directories[path.ParentPath].Add(path);
            return new MemoryFileStream(_files[path] = new MemoryFile());
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            if(!path.IsFile) throw new ArgumentException("The specified path is no file.", "path");
            if(!_files.TryGetValue(path, out var file)) throw new FileNotFoundException();
            return new MemoryFileStream(file);
        }

        public void CreateDirectory(FileSystemPath path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is no directory.", "path");
            if(_directories.ContainsKey(path)) throw new ArgumentException("The specified directory-path already exists.", "path");
            if(!_directories.TryGetValue(path.ParentPath, out var subentities)) throw new DirectoryNotFoundException();
            subentities.Add(path);
            _directories[path] = new HashSet<FileSystemPath>();
        }

        public void Delete(FileSystemPath path) {
            if(path.IsRoot) throw new ArgumentException("The root cannot be deleted.");
            bool removed;
            if(path.IsDirectory) {
                removed = _directories.Remove(path);
            } else {
                removed = _files.Remove(path);
            }
            if(!removed) throw new ArgumentException("The specified path does not exist.");
            var parent = _directories[path.ParentPath];
            parent.Remove(path);
        }

        public void Dispose() {
        }

        #region sub classes

        public class MemoryFile {
            public byte[] Content { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public MemoryFile() : this(new byte[0]) {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public MemoryFile(byte[] content) {
                this.Content = content;
            }
        }

        public class MemoryFileStream : Stream {
            readonly MemoryFile _file;

            public byte[] Content {
                get => _file.Content;
                set => _file.Content = value;
            }

            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite => true;

            public override long Length => _file.Content.Length;

            public override long Position { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public MemoryFileStream(MemoryFile file) {
                _file = file;
            }

            public override void Flush() {
            }

            public override long Seek(long offset, SeekOrigin origin) {
                if(origin == SeekOrigin.Begin) return Position = offset;
                if(origin == SeekOrigin.Current) return Position += offset;
                return this.Position = Length - offset;
            }

            public override void SetLength(long value) {
                int newLength = (int)value;
                byte[] newContent = new byte[newLength];
                Buffer.BlockCopy(Content, 0, newContent, 0, Math.Min(newLength, (int)Length));
                this.Content = newContent;
            }

            public override int Read(byte[] buffer, int offset, int count) {
                int mincount = Math.Min(count, Math.Abs((int)(Length - Position)));
                Buffer.BlockCopy(Content, (int)Position, buffer, offset, mincount);
                this.Position += mincount;
                return mincount;
            }

            public override void Write(byte[] buffer, int offset, int count) {
                if(Length - Position < count) SetLength(Position + count);
                Buffer.BlockCopy(buffer, offset, Content, (int)Position, count);
                this.Position += count;
            }
        }

        #endregion
    }
}
