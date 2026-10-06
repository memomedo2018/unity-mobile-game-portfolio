using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PortfolioSamples
{
    public sealed class Progress
    {
        public int UnlockedLevel { get; private set; }
        public int Coins { get; private set; }
        public Progress(int unlockedLevel = 1, int coins = 0)
        {
            if (unlockedLevel < 1 || unlockedLevel > 10000)
                throw new ArgumentOutOfRangeException(nameof(unlockedLevel));
            if (coins < 0 || coins > 1000000000)
                throw new ArgumentOutOfRangeException(nameof(coins));
            UnlockedLevel = unlockedLevel;
            Coins = coins;
        }
    }

    public static class ProgressCodec
    {
        // A tiny versioned text envelope; hash detects accidental corruption, not cheating.
        public static string Encode(Progress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            string body = "1|" + progress.UnlockedLevel.ToString(CultureInfo.InvariantCulture)
                + "|" + progress.Coins.ToString(CultureInfo.InvariantCulture);
            return body + "|" + Hash(body);
        }

        public static bool TryDecode(string text, out Progress progress)
        {
            progress = null;
            if (string.IsNullOrEmpty(text) || text.Length > 128) return false;
            string[] fields = text.Split('|');
            int level, coins;
            if (fields.Length != 4 || fields[0] != "1"
                || !int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out level)
                || !int.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out coins)
                || level < 1 || level > 10000 || coins < 0 || coins > 1000000000) return false;
            if (fields[3] != Hash(fields[0] + "|" + fields[1] + "|" + fields[2])) return false;
            progress = new Progress(level, coins);
            return true;
        }

        private static string Hash(string body)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(body))).Replace("-", "");
        }
    }

    public enum SaveOrigin { Primary, Backup, Default }

    public sealed class LoadResult
    {
        public Progress Progress { get; private set; }
        public SaveOrigin Origin { get; private set; }
        public LoadResult(Progress progress, SaveOrigin origin)
        {
            Progress = progress;
            Origin = origin;
        }
    }

    /// <summary>Single-writer desktop/mobile sample. Does not silently hide permission or I/O failures.</summary>
    public sealed class FileProgressStore
    {
        private readonly string primaryPath;
        private readonly string backupPath;

        public FileProgressStore(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("A save directory is required.");
            directory = Path.GetFullPath(directory);
            primaryPath = Path.Combine(directory, "progress.save");
            backupPath = primaryPath + ".bak";
        }

        public LoadResult Load()
        {
            Progress progress;
            string text;
            if (TryRead(primaryPath, out text, out progress)) return new LoadResult(progress, SaveOrigin.Primary);
            if (TryRead(backupPath, out text, out progress)) return new LoadResult(progress, SaveOrigin.Backup);
            return new LoadResult(new Progress(), SaveOrigin.Default);
        }

        public void Save(Progress progress)
        {
            string next = ProgressCodec.Encode(progress);
            Directory.CreateDirectory(Path.GetDirectoryName(primaryPath));
            string previous;
            Progress decoded;
            // Never replace a known-good backup with corrupt primary data.
            if (TryRead(primaryPath, out previous, out decoded)) WriteAtomically(backupPath, previous);
            WriteAtomically(primaryPath, next);
        }

        private static bool TryRead(string path, out string text, out Progress progress)
        {
            text = null;
            progress = null;
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length > 256) return false;
                    using (var reader = new StreamReader(stream)) text = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
            return ProgressCodec.TryDecode(text, out progress);
        }

        private static void WriteAtomically(string path, string text)
        {
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }
    }
}
