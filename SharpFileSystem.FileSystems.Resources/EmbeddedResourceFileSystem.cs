﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharpFileSystem.FileSystems.Resources {

    public class EmbeddedResourceFileSystem : IFileSystem {

        #region properties

        public Assembly Assembly { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public EmbeddedResourceFileSystem(Assembly assembly) {
            this.Assembly = assembly;
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            if(!path.IsRoot) throw new DirectoryNotFoundException();

            return Assembly.GetManifestResourceNames().Select(name => FileSystemPath.Root.AppendFile(name)).ToArray();
        }

        public bool Exists(FileSystemPath path) {
            return path.IsRoot || !path.IsDirectory && Assembly.GetManifestResourceNames().Contains(path.EntityName);
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            if(access == FileAccess.Write) throw new NotSupportedException();
            if(path.IsDirectory || path.ParentPath != FileSystemPath.Root) throw new FileNotFoundException();

            return Assembly.GetManifestResourceStream(path.EntityName);
        }

        public Stream CreateFile(FileSystemPath path) {
            throw new NotSupportedException();
        }

        public void CreateDirectory(FileSystemPath path) {
            throw new NotSupportedException();
        }

        public void Delete(FileSystemPath path) {
            throw new NotSupportedException();
        }

        public void Dispose() {
        }
    }
}
