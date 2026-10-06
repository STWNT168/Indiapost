using System.Text;
using System.Text.Json;

namespace IndiaPostAdminSuite;

public class MainForm : Form
{
    readonly string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IndiaPostAdminSuite");
    readonly ComboBox moduleBox = new();
    readonly TextBox searchBox = new();
    readonly DataGridView grid = new();
    readonly Label statusLabel = new();
    readonly Label countLabel = new();
    List<AdminRecord> records = new();
    string currentModule = "Daily Diarisation";

    static readonly string[] Modules = {
        "Daily Diarisation", "Pending Matters Register", "Action Taken Report (ATR)", "Reminder Register",
        "Dak / Correspondence Tracker", "File Movement Register", "Important Dates / Deadline Calendar", "Meeting Register",
        "Minutes & Action Point Register", "Inspection Objection Register", "Complaint / Grievance Tracker", "Court / Legal Case Tracker",
        "RTI Tracker", "Audit Para Register", "Circle Office Reference Register", "CEPT / Technical Complaint Tracker",
        "IT / Network Issue Register", "eKYC / Device Installation Tracker", "Performance Monitoring Sheet", "Office-wise Pendency Dashboard",
        "Tour / Visit Diary", "Visit Observation Register", "Compliance Calendar", "Important Orders & Instructions Register",
        "Confidential / Sensitive Matter Tracker"
    };

    public MainForm()
    {
        Directory.CreateDirectory(dataDir);
        Text = "India Post Administrative Control Suite";
        Width = 1400; Height = 850; StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        BuildUi(); LoadModule();
    }

    void BuildUi()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 86, Padding = new Padding(18, 12, 18, 10) };
        var title = new Label { Text = "INDIA POST  |  ADMINISTRATIVE CONTROL SUITE", Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(18, 10) };
        var subtitle = new Label { Text = "Offline • Editable • Searchable • Local records", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(20, 46) };
        header.Controls.Add(title); header.Controls.Add(subtitle);
        Controls.Add(header);

        var top = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(18, 8, 18, 8) };
        moduleBox.DropDownStyle = ComboBoxStyle.DropDownList; moduleBox.Width = 330; moduleBox.Items.AddRange(Modules); moduleBox.SelectedIndex = 0; moduleBox.SelectedIndexChanged += (_,_) => LoadModule();
        searchBox.PlaceholderText = "Search subject, office, reference, status..."; searchBox.Width = 330; searchBox.TextChanged += (_,_) => RefreshGrid();
        top.Controls.Add(moduleBox); top.Controls.Add(searchBox); moduleBox.Location = new Point(18, 8); searchBox.Location = new Point(365, 8);
        Controls.Add(top);

        var bar = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(18, 4, 18, 4) };
        foreach (var (txt, act) in new (string, Action)[] {
            ("+ Add", AddRecord), ("Edit", EditRecord), ("Delete", DeleteRecord), ("Save", Save), ("Export CSV", ExportCsv), ("Backup", Backup)
        }) { var b = new Button { Text = txt, AutoSize = true, Height = 32, Margin = new Padding(4) }; b.Click += (_,_) => act(); bar.Controls.Add(b); }
        Controls.Add(bar);

        grid.Dock = DockStyle.Fill; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.ReadOnly = true; grid.MultiSelect = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.RowHeadersVisible = false; grid.CellDoubleClick += (_,_) => EditRecord();
        Controls.Add(grid);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 32, Padding = new Padding(18, 5, 18, 5) };
        countLabel.AutoSize = true; statusLabel.AutoSize = true; statusLabel.Dock = DockStyle.Right; footer.Controls.Add(countLabel); footer.Controls.Add(statusLabel); Controls.Add(footer);
    }

    string FilePath => Path.Combine(dataDir, Sanitize(currentModule) + ".json");
    static string Sanitize(string s) => string.Concat(s.Select(c => char.IsLetterOrDigit(c) ? c : '_'));

    void LoadModule()
    {
        currentModule = moduleBox.SelectedItem?.ToString() ?? Modules[0];
        records = LoadRecords(); RefreshGrid(); statusLabel.Text = "  " + currentModule;
    }

    List<AdminRecord> LoadRecords()
    {
        try { if (File.Exists(FilePath)) return JsonSerializer.Deserialize<List<AdminRecord>>(File.ReadAllText(FilePath)) ?? new(); } catch { }
        return new();
    }

    void RefreshGrid()
    {
        var q = searchBox.Text.Trim(); var data = string.IsNullOrWhiteSpace(q) ? records : records.Where(r => string.Join(" ", r.Id,r.Date,r.Reference,r.Subject,r.Office,r.Responsible,r.Status,r.NextAction,r.DueDate,r.Remarks).Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        grid.DataSource = null; grid.DataSource = data.Select(r => new { r.Id, r.Date, r.Reference, r.Subject, r.Office, r.Responsible, r.Status, r.NextAction, r.DueDate, r.Remarks }).ToList();
        countLabel.Text = $"Records: {data.Count} / {records.Count}";
    }

    void AddRecord() { EditDialog(null); }
    void EditRecord()
    {
        if (grid.CurrentRow == null) return; var id = grid.CurrentRow.Cells[0].Value?.ToString(); var r = records.FirstOrDefault(x => x.Id == id); if (r != null) EditDialog(r);
    }
    void EditDialog(AdminRecord? old)
    {
        using var f = new Form { Text = old == null ? "Add Record" : "Edit Record", Width = 700, Height = 620, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var fields = new (string, TextBox)[] { ("Date", new()), ("Reference / File No.", new()), ("Subject / Matter", new()), ("Office", new()), ("Responsible", new()), ("Status", new()), ("Next Action", new()), ("Due Date", new()), ("Remarks", new()) };
        int y=20; foreach(var (label,tb) in fields){ var l=new Label{Text=label,Left=20,Top=y+5,Width=150}; tb.Left=175; tb.Top=y; tb.Width=470; if(label.Contains("Remarks")){tb.Height=100; tb.Multiline=true;} f.Controls.Add(l); f.Controls.Add(tb); y += label.Contains("Remarks") ? 120 : 48; }
        fields[0].Item2.Text = old?.Date ?? DateTime.Today.ToString("dd.MM.yyyy");
        fields[1].Item2.Text = old?.Reference ?? ""; fields[2].Item2.Text = old?.Subject ?? ""; fields[3].Item2.Text=old?.Office??""; fields[4].Item2.Text=old?.Responsible??""; fields[5].Item2.Text=old?.Status??"Pending"; fields[6].Item2.Text=old?.NextAction??""; fields[7].Item2.Text=old?.DueDate??""; fields[8].Item2.Text=old?.Remarks??"";
        var save = new Button{Text="Save",Left=535,Top=y+10,Width=110}; save.Click += (_,_) => { var id=old?.Id ?? Guid.NewGuid().ToString("N")[..8].ToUpper(); var nr=new AdminRecord(id,fields[0].Item2.Text,fields[1].Item2.Text,fields[2].Item2.Text,fields[3].Item2.Text,fields[4].Item2.Text,fields[5].Item2.Text,fields[6].Item2.Text,fields[7].Item2.Text,fields[8].Item2.Text); if(old==null) records.Add(nr); else {var i=records.FindIndex(x=>x.Id==old.Id); records[i]=nr;} Save(); f.DialogResult=DialogResult.OK; f.Close(); }; f.Controls.Add(save); f.AcceptButton=save; f.ShowDialog(this);
    }

    void DeleteRecord(){ if(grid.CurrentRow==null)return; var id=grid.CurrentRow.Cells[0].Value?.ToString(); if(id!=null && MessageBox.Show("Delete selected record?","Confirm",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes){records.RemoveAll(r=>r.Id==id);Save();} }
    void Save(){File.WriteAllText(FilePath,JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));RefreshGrid();statusLabel.Text="  Saved locally: "+DateTime.Now.ToString("dd.MM.yyyy HH:mm");}
    void ExportCsv(){using var s=new SaveFileDialog{Filter="CSV files (*.csv)|*.csv",FileName=Sanitize(currentModule)+".csv"};if(s.ShowDialog()!=DialogResult.OK)return;var sb=new StringBuilder("ID,Date,Reference,Subject,Office,Responsible,Status,Next Action,Due Date,Remarks\n");foreach(var r in records)sb.AppendLine(string.Join(",",new[]{r.Id,r.Date,r.Reference,r.Subject,r.Office,r.Responsible,r.Status,r.NextAction,r.DueDate,r.Remarks}.Select(Csv)));File.WriteAllText(s.FileName,sb.ToString(),Encoding.UTF8);}
    static string Csv(string s)=>"\""+s.Replace("\"","\"\"")+"\"";
    void Backup(){using var d=new FolderBrowserDialog();if(d.ShowDialog()!=DialogResult.OK)return;var dest=Path.Combine(d.SelectedPath,"IndiaPostAdminBackup_"+DateTime.Now.ToString("yyyyMMdd_HHmmss"));Directory.CreateDirectory(dest);foreach(var f in Directory.GetFiles(dataDir,"*.json"))File.Copy(f,Path.Combine(dest,Path.GetFileName(f)));MessageBox.Show("Backup created:\n"+dest,"Backup",MessageBoxButtons.OK,MessageBoxIcon.Information);}
}
