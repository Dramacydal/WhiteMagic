using DirtyMagic.Processes;

namespace DirtyMagic
{
    public static class ProcessExtensions
    {
        public static string GetVersionInfo(this RemoteProcess Process)
        {
            return string.Format("{0} {1}.{2}.{3} {4}",
                    Process.MainModule.FileVersionInfo.FileDescription,
                    Process.MainModule.FileVersionInfo.FileMajorPart,
                    Process.MainModule.FileVersionInfo.FileMinorPart,
                    Process.MainModule.FileVersionInfo.FileBuildPart,
                    Process.MainModule.FileVersionInfo.FilePrivatePart);
        }
    }
}
