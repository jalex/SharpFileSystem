using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace SharpFileSystem {

    public struct FileSystemPath: IEquatable<FileSystemPath>, IComparable<FileSystemPath> {
        readonly string _path;

        #region static

        /// <summary>
        /// Provides a character used to separate directory levels in a path string that reflects a hierarchical file system organization.
        /// </summary>
        public const char DirectorySeparatorChar = '/';
        internal const string DirectorySeparatorCharAsString = "/";

        /// <summary>
        /// Root file system path.
        /// </summary>
        public static readonly FileSystemPath Root = new FileSystemPath(DirectorySeparatorCharAsString);

        #endregion

        #region properties

        /// <summary>
        /// This path as string.
        /// </summary>
        public string Path => _path ?? DirectorySeparatorCharAsString;

        /// <summary>
        /// True if this <see cref="FileSystemPath"/> is a directory path.
        /// </summary>
        public bool IsDirectory => Path[Path.Length - 1] == DirectorySeparatorChar;

        /// <summary>
        /// True if this <see cref="FileSystemPath"/> is a file path.
        /// </summary>
        public bool IsFile => !IsDirectory;

        /// <summary>
        /// True if this <see cref="FileSystemPath"/> is the root path.
        /// </summary>
        public bool IsRoot => Path.Length == 1;

        /// <summary>
        /// Entity name (or null if this <see cref="FileSystemPath"/> is the root path).
        /// </summary>
        public string EntityName {
            get {
                if(IsRoot) return null;
                string name = Path;
                int endOfName = name.Length;
                if(IsDirectory) endOfName--;
                int startOfName = name.LastIndexOf(DirectorySeparatorChar, endOfName - 1, endOfName) + 1;
                return name.Substring(startOfName, endOfName - startOfName);
            }
        }

        /// <summary>
        /// Parent path.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this <see cref="FileSystemPath"/> is the root path.</exception>
        public FileSystemPath ParentPath {
            get {
                string parentPath = Path;
                if(IsRoot) throw new InvalidOperationException("There is no parent of root.");
                int lookaheadCount = parentPath.Length;
                if(IsDirectory) lookaheadCount--;
                int index = parentPath.LastIndexOf(DirectorySeparatorChar, lookaheadCount - 1, lookaheadCount);
                Debug.Assert(index >= 0);
                parentPath = parentPath.Remove(index + 1);
                return new FileSystemPath(parentPath);
            }
        }

        #endregion

        /// <summary>
        /// Constructor (private)
        /// </summary>
        FileSystemPath(string path) {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>
        /// Returns <see langword="true"/> if specified string path starts with the directory separator character ('/').
        /// </summary>
        /// <param name="path">A string path.</param>
        /// <returns><see langword="true"/> if specified string path is rooted; owtherwise <see langword="false"/>.</returns>
        public static bool IsRooted(string path) {
            if(path.Length == 0) return false;
            return path[0] == DirectorySeparatorChar;
        }

        /// <summary>
        /// Parses a string path and creates new <see cref="FileSystemPath"/> from it.
        /// </summary>
        /// <param name="path">A string path.</param>
        /// <returns>Created <see cref="FileSystemPath"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> is null.</exception>
        /// <exception cref="ParseException">Parse error.</exception>
        public static FileSystemPath Parse(string path) {
            if(path == null) throw new ArgumentNullException(nameof(path));
            if(!IsRooted(path)) throw new ParseException(path, "Path is not rooted");
            if(path.Contains(DirectorySeparatorCharAsString + DirectorySeparatorCharAsString)) throw new ParseException(path, "Path contains double directory-separators.");
            return new FileSystemPath(path);
        }

        /// <summary>
        /// Append a string path to this <see cref="FileSystemPath"/>.
        /// </summary>
        /// <param name="relativePath">A path to be added.</param>
        /// <returns>A new <see cref="FileSystemPath"/> path as result of concatenation.</returns>
        /// <exception cref="ArgumentException">A relativePath is rooted.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="FileSystemPath"/> is not a directory.</exception>
        public FileSystemPath AppendPath(string relativePath) {
            if(IsRooted(relativePath)) throw new ArgumentException("The specified path should be relative.", "relativePath");
            if(!IsDirectory) throw new InvalidOperationException("This FileSystemPath is not a directory.");
            return new FileSystemPath(Path + relativePath);
        }

        /// <summary>
        /// Append a <see cref="FileSystemPath"/> to this <see cref="FileSystemPath"/>.
        /// </summary>
        /// <param name="path">A path to be added.</param>
        /// <returns>A new <see cref="FileSystemPath"/> path as result of concatenation.</returns>
        /// <exception cref="InvalidOperationException">This <see cref="FileSystemPath"/> is not a directory.</exception>
        [Pure]
        public FileSystemPath AppendPath(FileSystemPath path) {
            if(!IsDirectory) throw new InvalidOperationException("This FileSystemPath is not a directory.");
            return new FileSystemPath(Path + path.Path.Substring(1));
        }

        /// <summary>
        /// Append a directory name to this <see cref="FileSystemPath"/>.
        /// </summary>
        /// <param name="directoryName">A directory name.</param>
        /// <returns>A new <see cref="FileSystemPath"/> path as result of concatenation.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="directoryName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The specified directory name includes directory-separator character(s) ('/').</exception>
        /// <exception cref="InvalidOperationException">This <see cref="FileSystemPath"/> is not a directory.</exception>
        [Pure]
        public FileSystemPath AppendDirectory(string directoryName) {
            if(directoryName == null) throw new ArgumentNullException(nameof(directoryName));
            if(directoryName.Contains(DirectorySeparatorChar.ToString())) throw new ArgumentException("The specified name includes directory-separator(s).", nameof(directoryName));
            if(!IsDirectory) throw new InvalidOperationException("The specified FileSystemPath is not a directory.");
            return new FileSystemPath(Path + directoryName + DirectorySeparatorChar);
        }

        /// <summary>
        /// Append a file name to this <see cref="FileSystemPath"/>.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <returns>A new <see cref="FileSystemPath"/> path as result of concatenation.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The specified file name includes directory-separator character(s) ('/').</exception>
        /// <exception cref="InvalidOperationException">This <see cref="FileSystemPath"/> is not a directory.</exception>
        [Pure]
        public FileSystemPath AppendFile(string fileName) {
            if(fileName.Contains(DirectorySeparatorChar.ToString())) throw new ArgumentException("The specified name includes directory-separator(s).", nameof(fileName));
            if(!IsDirectory) throw new InvalidOperationException("The specified FileSystemPath is not a directory.");
            return new FileSystemPath(Path + fileName);
        }

        /// <summary>
        /// Returns true if this <see cref="FileSystemPath"/> is directory path and it is a parent of the specified path.
        /// </summary>
        /// <param name="path">The specified <see cref="FileSystemPath"/>.</param>
        /// <returns>True if success; otherwise false.</returns>
        [Pure]
        public bool IsParentOf(FileSystemPath path) {
            return IsDirectory && Path.Length != path.Path.Length && path.Path.StartsWith(Path);
        }

        /// <summary>
        /// Returns true if the specified path is directory path and this <see cref="FileSystemPath"/> is a child of it.
        /// </summary>
        /// <param name="path">The specified <see cref="FileSystemPath"/>.</param>
        /// <returns>True if success; otherwise false.</returns>
        [Pure]
        public bool IsChildOf(FileSystemPath path) {
            return path.IsParentOf(this);
        }

        [Pure]
        public FileSystemPath RemoveParent(FileSystemPath parent) {
            if(!parent.IsDirectory) throw new ArgumentException("The specified path can not be the parent of this path: it is not a directory.");
            if(!Path.StartsWith(parent.Path)) throw new ArgumentException("The specified path is not a parent of this path.");
            return new FileSystemPath(Path.Remove(0, parent.Path.Length - 1));
        }

        [Pure]
        public FileSystemPath RemoveChild(FileSystemPath child) {
            if(!Path.EndsWith(child.Path)) throw new ArgumentException("The specified path is not a child of this path.");
            return new FileSystemPath(Path.Substring(0, Path.Length - child.Path.Length + 1));
        }

        [Pure]
        public string GetExtension() {
            if(!IsFile) throw new ArgumentException("The specified FileSystemPath is not a file.");
            string name = EntityName;
            int extensionIndex = name.LastIndexOf('.');
            if(extensionIndex < 0) return "";
            return name.Substring(extensionIndex);
        }

        [Pure]
        public FileSystemPath ChangeExtension(string extension) {
            if(!IsFile) throw new ArgumentException("The specified FileSystemPath is not a file.");
            string name = EntityName;
            int extensionIndex = name.LastIndexOf('.');
            if(extensionIndex >= 0) return ParentPath.AppendFile(name.Substring(0, extensionIndex) + extension);
            return Parse(Path + extension);
        }

        [Pure]
        public string[] GetDirectorySegments() {
            var path = this;
            if(IsFile) path = path.ParentPath;
            var segments = new LinkedList<string>();
            while(!path.IsRoot) {
                segments.AddFirst(path.EntityName);
                path = path.ParentPath;
            }
            return segments.ToArray();
        }

        [Pure]
        public int CompareTo(FileSystemPath other) {
            return Path.CompareTo(other.Path);
        }

        [Pure]
        public override string ToString() {
            return Path;
        }

        [Pure]
        public override bool Equals(object obj) {
            return obj is FileSystemPath path && 
                   Equals(path);
        }

        [Pure]
        public bool Equals(FileSystemPath other) {
            return other.Path.Equals(Path);
        }

        [Pure]
        public override int GetHashCode() {
            return Path.GetHashCode();
        }

        public static bool operator ==(FileSystemPath pathA, FileSystemPath pathB) {
            return pathA.Equals(pathB);
        }

        public static bool operator !=(FileSystemPath pathA, FileSystemPath pathB) {
            return !(pathA == pathB);
        }
    }
}
