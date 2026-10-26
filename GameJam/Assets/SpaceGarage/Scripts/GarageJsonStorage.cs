using System;
using System.IO;

namespace SpaceGarage
{
    public static class GarageJsonStorage
    {
        public static void Write(string path, string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllText(path + ".tmp", json);
            if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
            else File.Move(path + ".tmp", path);
        }

        public static bool TryRead(string path, Func<string, GarageSaveData> deserialize,
            Action<string> warn, out GarageSaveData data, out bool unreadable)
        {
            data = null;
            unreadable = false;
            foreach (string candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    var loaded = deserialize(File.ReadAllText(candidate));
                    if (loaded == null || !loaded.IsValid()) throw new InvalidDataException("Invalid save data or unsupported version");
                    data = loaded; return true;
                }
                catch (Exception error) { warn?.Invoke("Unable to load " + candidate + ": " + error.Message); }
            }
            unreadable = File.Exists(path) || File.Exists(path + ".bak");
            return false;
        }
    }
}
