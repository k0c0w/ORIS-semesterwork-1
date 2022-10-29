namespace Server.Services
{
    public static class FileProvider
    {

        public static (byte[], string) GetFileAndFileExtension(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                filePath += "/index.html";
                if (File.Exists(filePath))
                {
                    return (File.ReadAllBytes(filePath), GetExtension(filePath));
                }
            }
            //файл
            else if (File.Exists(filePath))
            {
                return (File.ReadAllBytes(filePath), GetExtension(filePath));
            }
            return (null, null);
        }

        private static string GetExtension(string filePath)
        {
            return filePath.Substring(filePath.LastIndexOf('.'));
        }
    }
}
