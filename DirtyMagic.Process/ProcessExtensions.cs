using DirtyMagic.Processes;

namespace DirtyMagic
{
    public static class ProcessExtensions
    {
        public static string GetVersionInfo(this RemoteProcess process)
        {
            return string.Format("{0} {1}.{2}.{3} {4}",
                    process.MainModule.FileVersionInfo.FileDescription,
                    process.MainModule.FileVersionInfo.FileMajorPart,
                    process.MainModule.FileVersionInfo.FileMinorPart,
                    process.MainModule.FileVersionInfo.FileBuildPart,
                    process.MainModule.FileVersionInfo.FilePrivatePart);
        }
    }
}
