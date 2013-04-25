﻿using System.Collections.Generic;
using System.Linq;

namespace System.IO.Abstractions.TestingHelpers
{
    [Serializable]
    public class MockFileSystem : IFileSystem, IMockFileDataAccessor
    {
        readonly IDictionary<string, MockFileData> files;
        readonly FileBase file;
        readonly DirectoryBase directory;
        readonly IFileInfoFactory fileInfoFactory;
        readonly PathBase pathField;
        readonly IDirectoryInfoFactory directoryInfoFactory;

        public MockFileSystem() : this(new Dictionary<string, MockFileData>()) { }

        public MockFileSystem(IDictionary<string, MockFileData> files, string currentDirectory = @"C:\Foo\Bar")
        {
            this.files = new Dictionary<string, MockFileData>(StringComparer.InvariantCultureIgnoreCase);
            file = new MockFile(this);
            directory = new MockDirectory(this, file, currentDirectory);
            fileInfoFactory = new MockFileInfoFactory(this);
            pathField = new MockPath();
            directoryInfoFactory = new MockDirectoryInfoFactory(this);

            foreach (var entry in files)
                AddFile(entry.Key, entry.Value);
        }

        public FileBase File
        {
            get { return file; }
        }

        public DirectoryBase Directory
        {
            get { return directory; }
        }

        public IFileInfoFactory FileInfo
        {
            get { return fileInfoFactory; }
        }

        public PathBase Path
        {
            get { return pathField; }
        }

        public IDirectoryInfoFactory DirectoryInfo
        {
            get { return directoryInfoFactory; }
        }

        private string FixPath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public MockFileData GetFile(string path, bool returnNullObject = false) 
        {
            path = FixPath(path);
            return FileExists(path) ? files[path] : returnNullObject ? MockFileData.NullObject : null;
        }
  
        public void AddFile(string path, MockFileData mockFile)
        {
            var fixedPath = FixPath(path);
            if (FileExists(path) && (files[fixedPath].Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied.", path));

            var directoryPath = Path.GetDirectoryName(path);
            if (!directory.Exists(directoryPath))
                directory.CreateDirectory(directoryPath);

            files[fixedPath] = mockFile;
        }

        public void AddDirectory(string path)
        {
            var fixedPath = FixPath(path);
            if (FileExists(path) && (files[fixedPath].Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied.", path));

            files[fixedPath] = new MockDirectoryData();
        }

        public void RemoveFile(string path)
        {
            path = FixPath(path);
            files.Remove(path);
        }

        public bool FileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            path = FixPath(path);
            return files.ContainsKey(path);
        }

        public IEnumerable<string> AllPaths
        {
            get { return files.Keys; }
        }

        public IEnumerable<string> AllFiles {
            get { return files.Where(f => !f.Value.IsDirectory).Select(f => f.Key); }
        }

        public IEnumerable<string> AllDirectories {
            get { return files.Where(f => f.Value.IsDirectory).Select(f => f.Key); }
        }
    }
}