using System.IO;
using System.Windows;

namespace FancyCards.Helpers
{
    public static class PathHelper
    {

        public static bool FileExists(string path)
        {
            var file_info = new FileInfo(path); 
            return file_info.Exists;
        }
        /// <summary>
        /// Если директория не слуществует, создаём
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public static void CreateDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void ClearFolder(string path)
        {
            var directory = new DirectoryInfo(path);
            if(!directory.Exists) return;

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }
        }


        public static void RemoveFolder(string path)
        {
            var directory = new DirectoryInfo(path);
            if (!directory.Exists) return;

            directory.Delete(true);
        }

        public static void MoveFolder(string source, string destination)
        {
            var directory = new DirectoryInfo(source);
            if (!directory.Exists) return;

            if (string.Equals(source, destination)) return;

            CreateDirectory(destination);

            directory.MoveTo(destination);
        }

        public static void CopyFileFromResources(string source, string destination, bool replace = false)
        {
            var file_info = new FileInfo(destination);
            if (file_info.Exists && !replace) return;

            CreateDirectory(destination);

            var resource_info = Application.GetResourceStream(new Uri(source, UriKind.Relative));
            using (var source_stream = resource_info.Stream)
            using (var file_stream = new FileStream(destination, FileMode.Create))
            {
                source_stream.CopyTo(file_stream);
            }              
        }

        public static void CopyFile(string source, string destination)
        {
            var file_info = new FileInfo(source);
            if (!file_info.Exists) return;

            CreateDirectory(destination);

            file_info.CopyTo(destination);
        }
        
        public static bool DeleteFile(string path)
        {
            try
            {
                var file_info = new FileInfo(path);
                if (!file_info.Exists) return false;

                file_info.Delete();
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
