using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using SharpFileSystem.FileSystems.NetZipArchive;
using SharpFileSystem.IO;

namespace SharpFileSystem.Tests.NetZipArchive {

    [TestFixture]
    public class NetZipArchiveFileSystemTest {
        Stream _zipStream;
        NetZipArchiveFileSystem _fileSystem;
        const string _FileContentString = "this is a file";

        readonly FileSystemPath _directoryPath = FileSystemPath.Parse("/directory/");
        readonly FileSystemPath _textfileAPath = FileSystemPath.Parse("/textfileA.txt");
        readonly FileSystemPath _fileInDirectoryPath = FileSystemPath.Parse("/directory/fileInDirectory.txt");
        readonly FileSystemPath _scratchDirectoryPath = FileSystemPath.Parse("/scratchdirectory/");

        [OneTimeSetUp]
        public void Initialize() {
            //var memoryStream = new MemoryStream();
            //_zipStream = memoryStream;

            _zipStream = new FileStream(@"c:\_atest\aaa.zip", FileMode.Create);

            var zipOutput = new ZipOutputStream(_zipStream);

            var fileContentBytes = Encoding.ASCII.GetBytes(_FileContentString);
            zipOutput.PutNextEntry(new ZipEntry("textfileA.txt") {
                Size = fileContentBytes.Length
            });
            zipOutput.Write(fileContentBytes);
            zipOutput.PutNextEntry(new ZipEntry("directory/fileInDirectory.txt"));
            zipOutput.PutNextEntry(new ZipEntry("scratchdirectory/scratch"));
            zipOutput.Finish();
            _zipStream.Position = 0;
            _fileSystem = NetZipArchiveFileSystem.Open(_zipStream);
        }

        [OneTimeTearDown]
        public void Cleanup() {
            _fileSystem.Dispose();
            _zipStream.Dispose();
        }

        [Test]
        public void GetEntitiesOfRootTest() {
            CollectionAssert.AreEquivalent(
                new[] { _textfileAPath, _directoryPath, _scratchDirectoryPath }, 
                _fileSystem.GetEntities(FileSystemPath.Root).ToArray()
            );
        }

        [Test]
        public void GetEntitiesOfDirectoryTest() {
            CollectionAssert.AreEquivalent(
                new[] { _fileInDirectoryPath }, 
                _fileSystem.GetEntities(_directoryPath).ToArray()
            );
        }

        [Test]
        public void ExistsTest() {
            Assert.IsTrue(_fileSystem.Exists(FileSystemPath.Root));
            Assert.IsTrue(_fileSystem.Exists(_textfileAPath));
            Assert.IsTrue(_fileSystem.Exists(_directoryPath));
            Assert.IsTrue(_fileSystem.Exists(_fileInDirectoryPath));
            Assert.IsFalse(_fileSystem.Exists(FileSystemPath.Parse("/nonExistingFile")));
            Assert.IsFalse(_fileSystem.Exists(FileSystemPath.Parse("/nonExistingDirectory/")));
            Assert.IsFalse(_fileSystem.Exists(FileSystemPath.Parse("/directory/nonExistingFileInDirectory")));
        }

        [Test]
        public void CanReadFile() {
            var file = _fileSystem.OpenFile(_textfileAPath, FileAccess.ReadWrite);
            var text = file.ReadAllText();
            Assert.IsTrue(string.Equals(text, _FileContentString));
        }

        [Test]
        public void CanWriteFile() {
            var file = _fileSystem.OpenFile(_textfileAPath, FileAccess.ReadWrite);
            var textBytes = Encoding.ASCII.GetBytes(_FileContentString + " and a new string");
            file.Write(textBytes);
            file.Close();
            file = _fileSystem.OpenFile(_textfileAPath, FileAccess.ReadWrite);
            var text = file.ReadAllText();
            Assert.IsTrue(string.Equals(text, _FileContentString + " and a new string"));
        }

        [Test]
        public void CanAddFile() {
            var fsp = FileSystemPath.Parse("/scratchdirectory/recentlyadded.txt");
            var file = _fileSystem.CreateFile(fsp);
            var textBytes = Encoding.ASCII.GetBytes("recently added");
            file.Write(textBytes);
            file.Close();
            Assert.IsTrue(_fileSystem.Exists(fsp));
            file = _fileSystem.OpenFile(fsp, FileAccess.ReadWrite);
            var text = file.ReadAllText();
            Assert.IsTrue(string.Equals(text, "recently added"));
        }

        [Test]
        public void CanAddDirectory() {
            var fsp = FileSystemPath.Parse("/scratchdirectory/newdir/");
            _fileSystem.CreateDirectory(fsp);
            //Assert.IsTrue(_fileSystem.Exists(fsp));
        }
    }
}
