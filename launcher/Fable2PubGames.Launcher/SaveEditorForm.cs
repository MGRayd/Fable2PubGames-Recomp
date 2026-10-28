using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Fable2PubGames.Launcher;

internal sealed class SaveEditorForm : Form
{
    private readonly string savesRoot;
    private readonly ComboBox patron = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown gold = NumberBox(0, 2_000_000_000);
    private readonly NumericUpDown debt = NumberBox(0, 2_000_000_000);
    private readonly NumericUpDown rating = NumberBox(1, 5);
    private readonly NumericUpDown towerPoints = NumberBox(0, 2_000_000_000);
    private readonly NumericUpDown keystonePoints = NumberBox(0, 2_000_000_000);
    private readonly NumericUpDown spinnerboxPoints = NumberBox(0, 2_000_000_000);
    private readonly Button saveButton = new();
    private readonly Label status = new();
    private readonly List<CharacterSave> characters = [];

    public SaveEditorForm(string savesRoot)
    {
        this.savesRoot = savesRoot;
        Text = "Fable II - Pub Games Save Editor";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(620, 570);
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = PubTheme.Background;
        ForeColor = PubTheme.Text;
        Font = new Font("Segoe UI", 10F);

        var title = new Label { Text = "Patron Save Editor", Font = PubTheme.Heading(20F), ForeColor = PubTheme.Gold, AutoSize = true, Location = new Point(30, 24) };
        var subtitle = new Label { Text = "Edit a local patron while the game is closed. Every save creates a backup.", ForeColor = PubTheme.Muted, AutoSize = true, Location = new Point(32, 67) };
        var card = new LauncherCard { Location = new Point(30, 105), Size = new Size(560, 380) };
        AddLabel(card, "PATRON", 24, 22);
        patron.Location = new Point(24, 47);
        patron.Size = new Size(512, 28);
        patron.BackColor = PubTheme.SurfaceRaised;
        patron.ForeColor = PubTheme.Text;
        patron.SelectedIndexChanged += (_, _) => LoadSelectedCharacter();
        card.Controls.Add(patron);

        AddField(card, "Gold", gold, 24, 100);
        AddField(card, "Debt", debt, 290, 100);
        AddField(card, "Rating", rating, 24, 176);
        AddField(card, "Fortune's Tower points", towerPoints, 290, 176);
        AddField(card, "Keystone points", keystonePoints, 24, 252);
        AddField(card, "Spinnerbox points", spinnerboxPoints, 290, 252);
        status.ForeColor = PubTheme.Muted;
        status.AutoSize = false;
        status.Size = new Size(510, 38);
        status.Location = new Point(24, 324);
        card.Controls.Add(status);

        var close = new Button { Text = "Close", Location = new Point(348, 510), Size = new Size(105, 40) };
        saveButton.Text = "Save Changes";
        saveButton.Location = new Point(465, 510);
        saveButton.Size = new Size(125, 40);
        PubTheme.StyleButton(close);
        PubTheme.StyleButton(saveButton, true);
        close.Click += (_, _) => Close();
        saveButton.Click += (_, _) => SaveChanges();
        Controls.AddRange([title, subtitle, card, close, saveButton]);

        LoadCharacters();
    }

    private void LoadCharacters()
    {
        characters.Clear();
        patron.Items.Clear();
        if (Directory.Exists(savesRoot))
        {
            foreach (string path in Directory.EnumerateFiles(savesRoot, "Fable2PubInfo.xml", SearchOption.AllDirectories).OrderBy(p => p))
            {
                try
                {
                    XElement root = XElement.Parse(File.ReadAllText(path));
                    if (root.Name.LocalName != "FablePubCharacter") continue;
                    var character = new CharacterSave(
                        path,
                        Attribute(root, "Name", "Unknown patron"),
                        Number(root, "GoldBalance"),
                        Number(root, "Debt"),
                        Math.Clamp(Number(root, "Rating", 1), 1, 5),
                        Number(root, "PointsEarned0"),
                        Number(root, "PointsEarned1"),
                        Number(root, "PointsEarned2"));
                    characters.Add(character);
                    patron.Items.Add(character.Name);
                }
                catch { }
            }
        }

        bool found = characters.Count > 0;
        saveButton.Enabled = found;
        patron.Enabled = found;
        status.Text = found ? $"{characters.Count} patron save{(characters.Count == 1 ? "" : "s")} found." : "No patron saves were found. Create a patron in the game first.";
        if (found) patron.SelectedIndex = 0;
    }

    private void LoadSelectedCharacter()
    {
        if (patron.SelectedIndex < 0 || patron.SelectedIndex >= characters.Count) return;
        CharacterSave selected = characters[patron.SelectedIndex];
        gold.Value = Clamp(selected.Gold, gold);
        debt.Value = Clamp(selected.Debt, debt);
        rating.Value = Clamp(selected.Rating, rating);
        towerPoints.Value = Clamp(selected.TowerPoints, towerPoints);
        keystonePoints.Value = Clamp(selected.KeystonePoints, keystonePoints);
        spinnerboxPoints.Value = Clamp(selected.SpinnerboxPoints, spinnerboxPoints);
        status.Text = $"Editing {selected.Name}. The original file will be backed up before saving.";
    }

    private void SaveChanges()
    {
        if (patron.SelectedIndex < 0 || patron.SelectedIndex >= characters.Count) return;
        if (Process.GetProcessesByName("Fable2PubGames").Length > 0)
        {
            MessageBox.Show(this, "Close Fable II - Pub Games before editing a patron save.", "Game is running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        CharacterSave selected = characters[patron.SelectedIndex];
        try
        {
            string original = File.ReadAllText(selected.Path);
            string updated = ReplaceNumber(original, "GoldBalance", gold.Value);
            updated = ReplaceNumber(updated, "Debt", debt.Value);
            updated = ReplaceNumber(updated, "Rating", rating.Value);
            updated = ReplaceNumber(updated, "PointsEarned0", towerPoints.Value);
            updated = ReplaceNumber(updated, "PointsEarned1", keystonePoints.Value);
            updated = ReplaceNumber(updated, "PointsEarned2", spinnerboxPoints.Value);
            _ = XElement.Parse(updated);

            string slotName = Directory.GetParent(selected.Path)?.Name ?? "unknown-slot";
            string backupFolder = Path.Combine(savesRoot, "save-editor-backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"), slotName);
            Directory.CreateDirectory(backupFolder);
            File.Copy(selected.Path, Path.Combine(backupFolder, "Fable2PubInfo.xml"), true);
            string temporary = selected.Path + ".editing";
            File.WriteAllText(temporary, updated, new System.Text.UTF8Encoding(false));
            File.Move(temporary, selected.Path, true);
            status.Text = $"Saved {selected.Name}. Backup: {Path.GetRelativePath(savesRoot, backupFolder)}";
            MessageBox.Show(this, $"{selected.Name} was updated successfully.\n\nA backup was created before the change.", "Save updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadCharacters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Could not update save", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string ReplaceNumber(string source, string attribute, decimal value)
    {
        string pattern = $"({Regex.Escape(attribute)}\\s*=\\s*['\"])[^'\"]*(['\"])";
        int replacements = 0;
        string updated = Regex.Replace(source, pattern, match =>
        {
            replacements++;
            return match.Groups[1].Value + decimal.Truncate(value).ToString(System.Globalization.CultureInfo.InvariantCulture) + match.Groups[2].Value;
        }, RegexOptions.CultureInvariant);
        if (replacements != 1) throw new InvalidDataException($"The {attribute} field was not found exactly once.");
        return updated;
    }

    private static NumericUpDown NumberBox(decimal minimum, decimal maximum) => new()
    {
        Minimum = minimum,
        Maximum = maximum,
        ThousandsSeparator = true,
        Width = 220,
        BackColor = PubTheme.SurfaceRaised,
        ForeColor = PubTheme.Text,
        BorderStyle = BorderStyle.FixedSingle
    };

    private static void AddLabel(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label { Text = text.ToUpperInvariant(), Font = new Font("Segoe UI Semibold", 8F), ForeColor = PubTheme.Muted, AutoSize = true, Location = new Point(x, y) });
    }

    private static void AddField(Control parent, string label, Control field, int x, int y)
    {
        AddLabel(parent, label, x, y);
        field.Location = new Point(x, y + 24);
        parent.Controls.Add(field);
    }

    private static string Attribute(XElement root, string name, string fallback) => root.Attribute(name)?.Value ?? fallback;
    private static decimal Number(XElement root, string name, decimal fallback = 0) => decimal.TryParse(root.Attribute(name)?.Value, out decimal value) ? value : fallback;
    private static decimal Clamp(decimal value, NumericUpDown control) => Math.Min(control.Maximum, Math.Max(control.Minimum, value));

    private sealed record CharacterSave(string Path, string Name, decimal Gold, decimal Debt, decimal Rating, decimal TowerPoints, decimal KeystonePoints, decimal SpinnerboxPoints);
}
