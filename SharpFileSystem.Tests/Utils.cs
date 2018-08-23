using System.Collections.Generic;

namespace SharpFileSystem.Tests {

    public static class Utils {

        public static void WriteContents(IFileSystem fileSystem) {
            var rootPath = FileSystemPath.Parse("/_doc.kml");
            var buildingB1Path = FileSystemPath.Parse("/files/B1_Building.kml");
            var buildingB1F1Path = FileSystemPath.Parse("/_BuildingFiles/B1_F1.kml");
            var buildingB1F2Path = FileSystemPath.Parse("/_BuildingFiles/B1_F2.kml");
            var buildingOutdoorPath = FileSystemPath.Parse("/files/OutdoorLayout_Building.kml");
            var b1 = rootPath.ParentPath.AppendPath(buildingB1Path);
            var f1 = b1.ParentPath.AppendPath(buildingB1F1Path);
            var f2 = b1.ParentPath.AppendPath(buildingB1F2Path);
            var b2 = rootPath.ParentPath.AppendPath(buildingOutdoorPath);
            var list = new List<FileSystemPath> { rootPath, b1, f1, f2, b2 };
            foreach(var path in list) {
                using(var stream = fileSystem.CreateFile(path)) {
                    stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
                }
            }
        }
    }
}
