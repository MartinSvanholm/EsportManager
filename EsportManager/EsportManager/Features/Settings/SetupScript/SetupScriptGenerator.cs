using System.Text;
using CliWrap;
using EsportManager.Features.Devices.UpdateFortnite;

namespace EsportManager.Features.Settings.SetupScript;

public static class SetupScriptGenerator
{
    private const string FileName = "SetupCopyShares.ps1";

    public static async Task<string> SaveToDownloadsAsync(string userName)
    {
        string script = await GenerateAsync(userName);

        string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(downloadsFolder);

        string path = Path.Combine(downloadsFolder, FileName);
        await File.WriteAllTextAsync(path, script, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        return path;
    }

    public static async Task ShowInExplorerAsync(string path)
    {
        await Cli.Wrap("explorer.exe")
            .WithArguments($"/select,\"{path}\"")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
    }

    private static async Task<string> GenerateAsync(string userName)
    {
        await using Stream stream = await FileSystem.OpenAppPackageFileAsync(FileName);
        using StreamReader reader = new(stream);
        string template = await reader.ReadToEndAsync();

        return template
            .Replace("{{UserName}}", EscapeSingleQuoted(userName))
            .Replace("{{GameFilesShare}}", EscapeSingleQuoted(FortniteShares.GameFiles))
            .Replace("{{ManifestsShare}}", EscapeSingleQuoted(FortniteShares.Manifests));
    }

    private static string EscapeSingleQuoted(string value) => value.Replace("'", "''");
}
