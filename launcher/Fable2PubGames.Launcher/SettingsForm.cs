using System.Diagnostics;

namespace Fable2PubGames.Launcher;

internal sealed class SettingsForm : Form
{
    private readonly LauncherSettings settings;
    private readonly CheckBox windowed = new() { Text = "Run in a window", AutoSize = true };
    private readonly ComboBox resolution = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
    private readonly CheckBox reliableText = new() { Text = "Reliable text rendering (recommended)", AutoSize = true };
    private readonly CheckBox detailedLogs = new() { Text = "Create detailed logs", AutoSize = true };
    private readonly CheckBox syncFable2 = new() { Text = "Sync progress when Pub Games closes", AutoSize = true };
    private readonly TextBox fable2SavesPath = new() { ReadOnly = true };
    private static string InstallRoot => AppContext.BaseDirectory;

    public SettingsForm(LauncherSettings current)
    {
        settings = current;
        Text = "Launcher Settings";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(540, 585);
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = PubTheme.Background;
        ForeColor = PubTheme.Text;
        Font = new Font("Segoe UI", 10F);

        var title = new Label { Text = "Advanced Options", Font = PubTheme.Heading(19F), ForeColor = PubTheme.Gold, AutoSize = true, Location = new Point(28, 22) };
        var subtitle = new Label { Text = "Choose how the game opens. Recommended settings are already selected.", ForeColor = PubTheme.Muted, AutoSize = true, Location = new Point(30, 64) };
        windowed.Location = new Point(32, 112);
        resolution.Location = new Point(245, 108);
        resolution.Items.AddRange(["1280 x 720", "1600 x 900", "1920 x 1080", "2560 x 1440", "3440 x 1440"]);
        windowed.Checked = settings.Windowed;
        SelectResolution(settings.Width, settings.Height);
        resolution.Enabled = windowed.Checked;
        windowed.CheckedChanged += (_, _) => resolution.Enabled = windowed.Checked;
        reliableText.Location = new Point(32, 158);
        reliableText.Checked = settings.ReliableTextRendering;
        detailedLogs.Location = new Point(32, 196);
        detailedLogs.Checked = settings.DetailedLogs;

        var divider = new Label { BorderStyle = BorderStyle.Fixed3D, AutoSize = false, Height = 2, Width = 475, Location = new Point(32, 238) };
        var linkTitle = new Label { Text = "Fable II character link", Font = new Font("Segoe UI Semibold", 10F), ForeColor = PubTheme.Gold, AutoSize = true, Location = new Point(32, 256) };
        var linkDetail = new Label { Text = "Use your Fable II heroes and return Pub Games winnings safely.", ForeColor = PubTheme.Muted, AutoSize = true, Location = new Point(32, 280) };
        fable2SavesPath.Location = new Point(32, 308);
        fable2SavesPath.Size = new Size(340, 27);
        fable2SavesPath.BackColor = PubTheme.Background;
        fable2SavesPath.ForeColor = PubTheme.Text;
        fable2SavesPath.BorderStyle = BorderStyle.FixedSingle;
        fable2SavesPath.Text = settings.Fable2SavesPath;
        var chooseFable2Saves = new Button { Text = "Choose Folder", Location = new Point(384, 304), Size = new Size(126, 35) };
        PubTheme.StyleButton(chooseFable2Saves);
        chooseFable2Saves.Click += (_, _) => ChooseFable2Saves();
        syncFable2.Location = new Point(32, 348);
        syncFable2.Checked = settings.SyncFable2Saves;

        var toolsDivider = new Label { BorderStyle = BorderStyle.Fixed3D, AutoSize = false, Height = 2, Width = 475, Location = new Point(32, 386) };
        var toolsTitle = new Label { Text = "Folders and troubleshooting", Font = new Font("Segoe UI Semibold", 10F), ForeColor = PubTheme.Gold, AutoSize = true, Location = new Point(32, 404) };
        var saves = new Button { Text = "Open Saves", Location = new Point(32, 438), Size = new Size(130, 36) };
        var logs = new Button { Text = "Open Logs", Location = new Point(174, 438), Size = new Size(130, 36) };
        var clearCache = new Button { Text = "Clear Shader Cache", Location = new Point(316, 438), Size = new Size(162, 36) };
        PubTheme.StyleButton(saves);
        PubTheme.StyleButton(logs);
        PubTheme.StyleButton(clearCache);
        saves.Click += (_, _) => OpenFolder(Path.Combine(InstallRoot, "saves"));
        logs.Click += (_, _) => OpenFolder(Path.Combine(InstallRoot, "logs"));
        clearCache.Click += (_, _) => ClearCache();

        var save = new Button { Text = "Save Settings", Location = new Point(330, 517), Size = new Size(148, 42) };
        var cancel = new Button { Text = "Cancel", Location = new Point(218, 517), Size = new Size(100, 42) };
        PubTheme.StyleButton(save, true);
        PubTheme.StyleButton(cancel);
        save.Click += (_, _) => SaveAndClose();
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange([title, subtitle, windowed, resolution, reliableText, detailedLogs,
            divider, linkTitle, linkDetail, fable2SavesPath, chooseFable2Saves, syncFable2,
            toolsDivider, toolsTitle, saves, logs, clearCache, save, cancel]);
    }

    private void SelectResolution(int width, int height)
    {
        string value = $"{width} x {height}";
        if (!resolution.Items.Contains(value)) resolution.Items.Add(value);
        resolution.SelectedItem = value;
    }

    private void SaveAndClose()
    {
        if (syncFable2.Checked && !Fable2SaveSync.IsValidSavesRoot(fable2SavesPath.Text))
        {
            MessageBox.Show(this, "Choose the Fable II saves folder containing your hero saves.", "Fable II saves", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        string[] parts = resolution.SelectedItem?.ToString()?.Split('x', StringSplitOptions.TrimEntries) ?? ["1280", "720"];
        settings.Windowed = windowed.Checked;
        settings.Width = int.TryParse(parts[0], out int width) ? width : 1280;
        settings.Height = int.TryParse(parts[1], out int height) ? height : 720;
        settings.ReliableTextRendering = reliableText.Checked;
        settings.DetailedLogs = detailedLogs.Checked;
        settings.SyncFable2Saves = syncFable2.Checked;
        settings.Fable2SavesPath = fable2SavesPath.Text;
        LauncherSettingsStore.Save(settings);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ChooseFable2Saves()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose the saves folder from Fable II Recompiled",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(fable2SavesPath.Text) ? fable2SavesPath.Text : string.Empty
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (!Fable2SaveSync.IsValidSavesRoot(dialog.SelectedPath))
        {
            MessageBox.Show(this, "No Fable II hero saves were found in that folder.", "Fable II saves", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        fable2SavesPath.Text = dialog.SelectedPath;
        syncFable2.Checked = true;
    }

    private void ClearCache()
    {
        if (Process.GetProcessesByName("Fable2PubGames").Length > 0)
        {
            MessageBox.Show(this, "Close the game before clearing its shader cache.", "Game is running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        string cache = Path.Combine(InstallRoot, "cache");
        if (!Directory.Exists(cache))
        {
            MessageBox.Show(this, "The shader cache is already empty.", "Clear shader cache", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(this, "Clear the shader cache? It will be rebuilt next time the game starts.",
                "Clear shader cache", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Directory.Delete(cache, true);
        MessageBox.Show(this, "Shader cache cleared.", "Clear shader cache", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    }
}
