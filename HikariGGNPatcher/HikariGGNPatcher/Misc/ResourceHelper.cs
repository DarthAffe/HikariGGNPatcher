using System.IO;
using System.Reflection;

namespace HikariGGNPatcher.Misc
{
    public static class ResourceHelper
    {
        public static void WriteResourceToFile(string fileName, string resourceName)
        {
            using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                resource.CopyTo(file);
        }
    }
}
