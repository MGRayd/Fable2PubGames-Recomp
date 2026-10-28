using System.Diagnostics;
using System.Text;

namespace Fable2PubGames.Launcher;

internal sealed class MainForm : Form
{
    private readonly TextBox packagePath = new();
    private readonly Button browseButton = new();
    private readonly Button extractButton = new();
    private readonly Button saveEditorButton = new();
    private readonly Button playButton = new();
    private readonly Button settingsButton = new();
    private readonly Label statusTitle = new();
    private readonly Label statusDetail = new();
    private readonly Label settingsSummary = new();
    private readonly Label installBadge = new();
    private readonly ProgressBar progress = new();
    private readonly GameArtworkPanel artwork = new();
    private LauncherSettings settings = LauncherSettingsStore.Load();

    private static string InstallRoot => AppContext.BaseDirectory;
    private static string DataRoot => Path.Combine(InstallRoot, "data");
    private static string MediaRoot => Path.Combine(DataRoot, "media");
    private static string GameExecutable => Path.Combine(InstallRoot, "Fable2PubGames.exe");
    private static string ExtractorPath => Path.Combine(InstallRoot, "tools", "Extract-STFS.ps1");

    public MainForm()
    {
        PubTheme.Initialise(MediaRoot);
        Text = "Fable II - Pub Games";
        Icon = DdsIconLoader.TryLoadIcon(Path.Combine(MediaRoot, "LogoTiny.dds")) ?? Icon;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        ClientSize = new Size(900, 610);
        MaximizeBox = false;
        BackColor = PubTheme.Background;
        ForeColor = PubTheme.Text;
        Font = new Font("Segoe UI", 10F);

        var title = new Label { Text = "FABLE II - PUB GAMES", Font = PubTheme.Heading(25F), ForeColor = PubTheme.Gold, BackColor = Color.Transparent, AutoSize = true, Location = new Point(34, 26) };
        var subtitle = new Label { Text = "RECOMP LAUNCHER", Font = new Font("Segoe UI Semibold", 8.5F), ForeColor = PubTheme.Muted, BackColor = Color.Transparent, AutoSize = true, Location = new Point(37, 69) };
        installBadge.AutoSize = false;
        installBadge.Size = new Size(175, 38);
        installBadge.Location = new Point(565, 34);
        installBadge.TextAlign = ContentAlignment.MiddleCenter;
        installBadge.BorderStyle = BorderStyle.FixedSingle;
        settingsButton.Text = "Advanced Options";
        settingsButton.Size = new Size(130, 38);
        settingsButton.Location = new Point(751, 34);
        PubTheme.StyleButton(settingsButton);

        var setupCard = new LauncherCard { Location = new Point(34, 108), Size = new Size(522, 255) };
        var sectionTitle = new Label { Text = "Game files", Font = PubTheme.Heading(17F), ForeColor = PubTheme.Gold, BackColor = Color.Transparent, AutoSize = true, Location = new Point(22, 20) };
        statusTitle.Font = new Font("Segoe UI Semibold", 11F);
        statusTitle.ForeColor = PubTheme.Text;
        statusTitle.BackColor = Color.Transparent;
        statusTitle.AutoSize = true;
        statusTitle.Location = new Point(24, 57);
        statusDetail.ForeColor = PubTheme.Muted;
        statusDetail.BackColor = Color.Transparent;
        statusDetail.AutoSize = true;
        statusDetail.MaximumSize = new Size(470, 0);
        statusDetail.Location = new Point(24, 81);
        var fileLabel = new Label { Text = "ORIGINAL XBOX 360 GAME FILE", Font = new Font("Segoe UI Semibold", 8F), ForeColor = PubTheme.Muted, BackColor = Color.Transparent, AutoSize = true, Location = new Point(24, 126) };
        packagePath.ReadOnly = true;
        packagePath.PlaceholderText = "Choose the LIVE container from your own copy";
        packagePath.Location = new Point(24, 148);
        packagePath.Size = new Size(330, 27);
        packagePath.BackColor = PubTheme.Background;
        packagePath.ForeColor = PubTheme.Text;
        packagePath.BorderStyle = BorderStyle.FixedSingle;
        browseButton.Text = "Choose File";
        browseButton.Location = new Point(365, 144);
        browseButton.Size = new Size(129, 35);
        PubTheme.StyleButton(browseButton);
        extractButton.Location = new Point(24, 195);
        extractButton.Size = new Size(190, 42);
        PubTheme.StyleButton(extractButton);
        progress.Location = new Point(24, 244);
        progress.Size = new Size(470, 6);
        progress.Style = ProgressBarStyle.Marquee;
        progress.Visible = false;
        var legal = new Label { Text = "No game content is included.", ForeColor = PubTheme.Muted, BackColor = Color.Transparent, AutoSize = true, Location = new Point(232, 207) };
        setupCard.Controls.AddRange([sectionTitle, statusTitle, statusDetail, fileLabel, packagePath, browseButton, extractButton, progress, legal]);

        var patronCard = new LauncherCard { Location = new Point(34, 381), Size = new Size(522, 120) };
        var patronTitle = new Label { Text = "Patron tools", Font = PubTheme.Heading(16F), ForeColor = PubTheme.Gold, BackColor = Color.Transparent, AutoSize = true, Location = new Point(22, 14) };
        var patronDetail = new Label { Text = "Adjust gold, debt, rating and game statistics.", ForeColor = PubTheme.Muted, BackColor = Color.Transparent, AutoSize = true, Location = new Point(24, 56) };
        saveEditorButton.Text = "Open Save Editor";
        saveEditorButton.Location = new Point(352, 39);
        saveEditorButton.Size = new Size(142, 42);
        PubTheme.StyleButton(saveEditorButton);
        patronCard.Controls.AddRange([patronTitle, patronDetail, saveEditorButton]);

        artwork.Location = new Point(574, 108);
        artwork.Size = new Size(292, 393);
        artwork.LoadFrom(Path.Combine(MediaRoot, "Splash_02.jpg"));
        var footer = new Panel { Location = new Point(0, 530), Size = new Size(900, 80), BackColor = Color.FromArgb(12, 23, 21) };
        var footerLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = PubTheme.Border };
        playButton.Text = "Play Pub Games";
        playButton.Location = new Point(696, 18);
        playButton.Size = new Size(170, 44);
        PubTheme.StyleButton(playButton, true);
        settingsSummary.ForeColor = PubTheme.Muted;
        settingsSummary.BackColor = Color.Transparent;
        settingsSummary.AutoSize = true;
        settingsSummary.Location = new Point(34, 31);
        footer.Controls.AddRange([footerLine, settingsSummary, playButton]);
        Controls.AddRange([title, subtitle, installBadge, settingsButton, setupCard, patronCard, artwork, footer]);

        browseButton.Click += BrowseForPackage;
        extractButton.Click += async (_, _) => await ExtractPackageAsync();
        saveEditorButton.Click += (_, _) => OpenSaveEditor();
        playButton.Click += async (_, _) => await LaunchGameAsync();
        settingsButton.Click += (_, _) => OpenSettings();
        Shown += (_, _) => RefreshState();
    }

    private void OpenSettings()
    {
        using var dialog = new SettingsForm(settings);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            settings = LauncherSettingsStore.Load();
            UpdateSettingsSummary();
        }
    }

    private void OpenSaveEditor()
    {
        using var dialog = new SaveEditorForm(Path.Combine(InstallRoot, "saves"));
        dialog.ShowDialog(this);
    }

    private void BrowseForPackage(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Title = "Select the Fable II Pub Games Xbox 360 container", Filter = "Xbox 360 container|*|All files|*.*", CheckFileExists = true, Multiselect = false };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            packagePath.Text = dialog.FileName;
            statusDetail.Text = IsStfsContainer(dialog.FileName)
                ? "Xbox 360 container detected. Select Install Game Files."
                : "That file is not a recognised LIVE, PIRS or CON container.";
        }
    }

    private async Task ExtractPackageAsync()
    {
        string source = packagePath.Text;
        if (!IsStfsContainer(source))
        {
            MessageBox.Show(this, "Choose a valid Xbox 360 LIVE, PIRS or CON container first.", "Choose game file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!File.Exists(ExtractorPath))
        {
            MessageBox.Show(this, "The bundled STFS extractor is missing.", "Launcher error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (File.Exists(Path.Combine(DataRoot, "default.xex")) && MessageBox.Show(this, "Replace the installed game data? The current data will be kept as a backup.", "Replace game data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        string staging = Path.Combine(InstallRoot, "data.extracting");
        SetBusy(true, "Unpacking your game file. This may take a few minutes...");
        try
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            Directory.CreateDirectory(staging);
            var startInfo = new ProcessStartInfo { FileName = "powershell.exe", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (string argument in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", ExtractorPath, "-Path", source, "-OutputDir", staging }) startInfo.ArgumentList.Add(argument);
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start PowerShell.");
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            string output = await stdout;
            string error = await stderr;
            if (process.ExitCode != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? output : error);
            if (!File.Exists(Path.Combine(staging, "default.xex"))) throw new InvalidOperationException("default.xex was not found. Confirm this is the Pub Games container.");
            if (Directory.Exists(DataRoot)) Directory.Move(DataRoot, Path.Combine(InstallRoot, $"data.backup-{DateTime.Now:yyyyMMdd-HHmmss}"));
            Directory.Move(staging, DataRoot);
            packagePath.Clear();
            PubTheme.Initialise(MediaRoot);
            artwork.LoadFrom(Path.Combine(MediaRoot, "Splash_02.jpg"));
            Icon = DdsIconLoader.TryLoadIcon(Path.Combine(MediaRoot, "LogoTiny.dds")) ?? Icon;
            RefreshState("Game files installed. The table is ready.");
        }
        catch (Exception ex)
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            RefreshState("Game file installation did not finish.");
            MessageBox.Show(this, ex.Message, "Could not install game files", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { SetBusy(false); }
    }

    private async Task LaunchGameAsync()
    {
        if (!File.Exists(GameExecutable) || !File.Exists(Path.Combine(DataRoot, "default.xex")))
        {
            RefreshState("Install the game files before playing.");
            return;
        }
        if (Process.GetProcessesByName("Fable2PubGames").Length > 0)
        {
            statusDetail.Text = "The game is already running.";
            return;
        }

        string pubGamesSaves = Path.Combine(InstallRoot, "saves");
        if (settings.SyncFable2Saves)
        {
            try
            {
                SaveSyncResult prepared = Fable2SaveSync.PrepareForLaunch(settings.Fable2SavesPath, pubGamesSaves);
                if (prepared.HeroesFound == 0) throw new InvalidOperationException("No Fable II hero saves were found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Could not link Fable II saves", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        var startInfo = new ProcessStartInfo { FileName = GameExecutable, WorkingDirectory = InstallRoot, UseShellExecute = true };
        startInfo.ArgumentList.Add($"--game_data_root={DataRoot}");
        startInfo.ArgumentList.Add("--license_mask=1");
        startInfo.ArgumentList.Add($"--async_shader_compilation={!settings.ReliableTextRendering}".ToLowerInvariant());
        if (settings.DetailedLogs) startInfo.ArgumentList.Add("--log_level=debug");
        if (settings.Windowed)
        {
            startInfo.ArgumentList.Add("--no-fullscreen");
            startInfo.ArgumentList.Add($"--window_width={settings.Width}");
            startInfo.ArgumentList.Add($"--window_height={settings.Height}");
        }
        using Process? process = Process.Start(startInfo);
        if (process is null) return;
        playButton.Enabled = false;
        statusDetail.Text = settings.SyncFable2Saves ? "The game is running. Fable II progress will sync when it closes." : "The game is running.";
        try
        {
            await process.WaitForExitAsync();
            if (settings.SyncFable2Saves)
            {
                SaveSyncResult synced = Fable2SaveSync.SyncAfterExit(settings.Fable2SavesPath, pubGamesSaves);
                statusDetail.Text = synced.BridgeFilesUpdated > 0
                    ? $"Fable II progress synced for {synced.BridgeFilesUpdated} hero save(s)."
                    : "Game closed. Fable II saves are already up to date.";
            }
            else statusDetail.Text = "The game has closed.";
        }
        catch (Exception ex)
        {
            statusDetail.Text = "The game closed, but Fable II progress could not be synced.";
            MessageBox.Show(this, ex.Message, "Fable II sync", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { playButton.Enabled = true; }
    }

    private void RefreshState(string? message = null)
    {
        bool installed = File.Exists(Path.Combine(DataRoot, "default.xex"));
        bool executablePresent = File.Exists(GameExecutable);
        bool ready = installed && executablePresent;
        installBadge.Text = ready ? "GAME FILES READY" : "SETUP REQUIRED";
        installBadge.ForeColor = ready ? PubTheme.Success : PubTheme.Gold;
        installBadge.BackColor = PubTheme.Surface;
        statusTitle.Text = installed ? "Original game files detected" : "Install your original game files";
        statusDetail.Text = message ?? (installed
            ? executablePresent ? "Your extracted Pub Games files are installed locally." : "Fable2PubGames.exe is missing."
            : "Choose your Xbox 360 container and the launcher will unpack it into the data folder.");
        playButton.Enabled = ready;
        saveEditorButton.Enabled = installed;
        extractButton.Text = installed ? "Replace Game Files" : "Install Game Files";
        UpdateSettingsSummary();
    }

    private void UpdateSettingsSummary()
    {
        settingsSummary.Text = settings.Windowed
            ? $"Display: {settings.Width} x {settings.Height} windowed   •   Fable II link {(settings.SyncFable2Saves ? "on" : "off")}"
            : $"Display: Full screen   •   Fable II link {(settings.SyncFable2Saves ? "on" : "off")}";
    }

    private void SetBusy(bool busy, string? message = null)
    {
        browseButton.Enabled = !busy;
        extractButton.Enabled = !busy;
        settingsButton.Enabled = !busy;
        saveEditorButton.Enabled = !busy && Directory.Exists(Path.Combine(InstallRoot, "saves"));
        playButton.Enabled = !busy && File.Exists(Path.Combine(DataRoot, "default.xex")) && File.Exists(GameExecutable);
        progress.Visible = busy;
        if (message is not null) statusDetail.Text = message;
    }

    private static bool IsStfsContainer(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            using var stream = File.OpenRead(path);
            if (stream.Length < 4) return false;
            Span<byte> signature = stackalloc byte[4];
            if (stream.Read(signature) != 4) return false;
            return Encoding.ASCII.GetString(signature) is "LIVE" or "PIRS" or "CON ";
        }
        catch { return false; }
    }
}
