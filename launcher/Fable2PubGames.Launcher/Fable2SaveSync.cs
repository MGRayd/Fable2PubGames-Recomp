using System.Security.Cryptography;
using System.Xml.Linq;

namespace Fable2PubGames.Launcher;

internal sealed record SaveSyncResult(int HeroesFound, int BridgeFilesUpdated, int BackupsCreated);

internal static class Fable2SaveSync
{
    private const string Fable2TitleId = "4D5307F1";

    public static bool IsValidSavesRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return false;
        return FindTitleDirectories(path).Any();
    }

    public static SaveSyncResult PrepareForLaunch(string sourceSavesRoot, string pubGamesSavesRoot)
    {
        int heroes = 0;
        int restored = 0;
        int backups = 0;
        string? backupRoot = null;

        foreach (string sourceTitle in FindTitleDirectories(sourceSavesRoot))
        {
            string xuid = Directory.GetParent(sourceTitle)!.Name;
            string targetTitle = Path.Combine(pubGamesSavesRoot, xuid, Fable2TitleId);

            foreach (string sourceBridge in Directory.EnumerateFiles(sourceTitle, "Fable2PubInfo.xml", SearchOption.AllDirectories))
            {
                heroes++;
                string relative = Path.GetRelativePath(sourceTitle, sourceBridge);
                string stagedBridge = Path.Combine(targetTitle, relative);
                if (!File.Exists(stagedBridge) || FilesMatch(sourceBridge, stagedBridge)) continue;
                if (File.GetLastWriteTimeUtc(stagedBridge) <= File.GetLastWriteTimeUtc(sourceBridge)) continue;
                ValidateBridge(stagedBridge);
                backupRoot ??= CreateBackupRoot(sourceSavesRoot);
                BackupFile(sourceBridge, sourceSavesRoot, backupRoot);
                CopyFile(stagedBridge, sourceBridge);
                restored++;
                backups++;
            }

            CopyTree(sourceTitle, targetTitle);
        }

        return new SaveSyncResult(heroes, restored, backups);
    }

    public static SaveSyncResult SyncAfterExit(string sourceSavesRoot, string pubGamesSavesRoot)
    {
        int heroes = 0;
        int updated = 0;
        int backups = 0;
        string? backupRoot = null;

        foreach (string sourceTitle in FindTitleDirectories(sourceSavesRoot))
        {
            string xuid = Directory.GetParent(sourceTitle)!.Name;
            string stagedTitle = Path.Combine(pubGamesSavesRoot, xuid, Fable2TitleId);
            foreach (string sourceBridge in Directory.EnumerateFiles(sourceTitle, "Fable2PubInfo.xml", SearchOption.AllDirectories))
            {
                heroes++;
                string stagedBridge = Path.Combine(stagedTitle, Path.GetRelativePath(sourceTitle, sourceBridge));
                if (!File.Exists(stagedBridge) || FilesMatch(sourceBridge, stagedBridge)) continue;
                ValidateBridge(stagedBridge);
                backupRoot ??= CreateBackupRoot(sourceSavesRoot);
                BackupFile(sourceBridge, sourceSavesRoot, backupRoot);
                CopyFile(stagedBridge, sourceBridge);
                updated++;
                backups++;
            }
        }

        return new SaveSyncResult(heroes, updated, backups);
    }

    private static IEnumerable<string> FindTitleDirectories(string savesRoot) =>
        Directory.EnumerateDirectories(savesRoot)
            .Select(xuid => Path.Combine(xuid, Fable2TitleId))
            .Where(Directory.Exists);

    private static void CopyTree(string sourceRoot, string targetRoot)
    {
        foreach (string sourceFile in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            string targetFile = Path.Combine(targetRoot, Path.GetRelativePath(sourceRoot, sourceFile));
            CopyFile(sourceFile, targetFile);
        }
    }

    private static void CopyFile(string source, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, true);
        File.SetLastWriteTimeUtc(target, File.GetLastWriteTimeUtc(source));
    }

    private static bool FilesMatch(string first, string second)
    {
        if (new FileInfo(first).Length != new FileInfo(second).Length) return false;
        using var left = SHA256.Create();
        using var right = SHA256.Create();
        using var firstStream = File.OpenRead(first);
        using var secondStream = File.OpenRead(second);
        return left.ComputeHash(firstStream).SequenceEqual(right.ComputeHash(secondStream));
    }

    private static void ValidateBridge(string path)
    {
        XDocument document = XDocument.Load(path);
        if (document.Root?.Name.LocalName != "FablePubCharacter")
            throw new InvalidDataException($"The Pub Games bridge file is invalid: {path}");
    }

    private static string CreateBackupRoot(string savesRoot)
    {
        string root = Path.Combine(savesRoot, "pubgames-sync-backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void BackupFile(string source, string savesRoot, string backupRoot)
    {
        string backup = Path.Combine(backupRoot, Path.GetRelativePath(savesRoot, source));
        CopyFile(source, backup);
    }
}
