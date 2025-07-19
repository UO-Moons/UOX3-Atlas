using System;
using System.Drawing;
using System.Windows.Forms;

namespace UOX3Atlas
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private PictureBox pictureBox1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem loadRegionsToolStripMenuItem;
        private ToolStripMenuItem toggleRegionsToolStripMenuItem;
        private ToolStripMenuItem saveRegionsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;

        private ToolStrip toolStrip1;
        private ToolStripButton zoomInToolStripButton;
        private ToolStripButton zoomOutToolStripButton;
        private ToolStripLabel zoomLabel;
        private ToolStripComboBox zoomComboBox;

        private CheckedListBox checkedListBoxRegions;
        private TextBox txtRegionSearch;
        private Panel panelRegionSidebar;
        private TableLayoutPanel mainLayout;

        private ContextMenuStrip regionContextMenu;
        private ToolStripMenuItem editTagsMenuItem;
        private ToolStripMenuItem compareTagsMenuItem;

        private ComboBox comboBoxRegionGroups;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pictureBox1 = new PictureBox();
            this.menuStrip1 = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.openToolStripMenuItem = new ToolStripMenuItem();
            this.loadRegionsToolStripMenuItem = new ToolStripMenuItem();
            this.toggleRegionsToolStripMenuItem = new ToolStripMenuItem();
            this.saveRegionsToolStripMenuItem = new ToolStripMenuItem();
            this.helpToolStripMenuItem = new ToolStripMenuItem();

            this.toolStrip1 = new ToolStrip();
            this.zoomInToolStripButton = new ToolStripButton();
            this.zoomOutToolStripButton = new ToolStripButton();
            this.zoomLabel = new ToolStripLabel();
            this.zoomComboBox = new ToolStripComboBox();

            this.checkedListBoxRegions = new CheckedListBox();
            this.txtRegionSearch = new TextBox();
            this.panelRegionSidebar = new Panel();
            this.mainLayout = new TableLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();

            // menuStrip1
            this.menuStrip1.Items.AddRange(new ToolStripItem[] {
                this.fileToolStripMenuItem,
                this.helpToolStripMenuItem
            });
            this.menuStrip1.Location = new Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new Size(800, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.BackColor = Color.LightBlue;

            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.openToolStripMenuItem,
                this.loadRegionsToolStripMenuItem,
                this.toggleRegionsToolStripMenuItem,
                this.saveRegionsToolStripMenuItem
            });
            this.fileToolStripMenuItem.Text = "File";

            this.openToolStripMenuItem.Text = "Open Image";
            this.openToolStripMenuItem.Click += new EventHandler(this.openToolStripMenuItem_Click);

            this.loadRegionsToolStripMenuItem.Text = "Load Regions File";
            this.loadRegionsToolStripMenuItem.Click += new EventHandler(this.loadRegionsToolStripMenuItem_Click);

            this.toggleRegionsToolStripMenuItem.Text = "Toggle All Regions";
            this.toggleRegionsToolStripMenuItem.Click += new EventHandler(this.toggleRegionsToolStripMenuItem_Click);

            this.saveRegionsToolStripMenuItem.Text = "Save Regions File";
            this.saveRegionsToolStripMenuItem.Click += new EventHandler(this.saveRegionsToolStripMenuItem_Click);

            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new EventHandler(this.helpToolStripMenuItem_Click);

            // toolStrip1
            this.toolStrip1.Items.AddRange(new ToolStripItem[] {
                this.zoomInToolStripButton,
                this.zoomOutToolStripButton,
                this.zoomLabel,
                this.zoomComboBox
            });
            this.toolStrip1.Location = new Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new Size(800, 25);
            this.toolStrip1.BackColor = Color.WhiteSmoke;

            this.zoomInToolStripButton.Text = "Zoom In +";
            this.zoomInToolStripButton.Click += new EventHandler(this.zoomInButton_Click);

            this.zoomOutToolStripButton.Text = "Zoom Out -";
            this.zoomOutToolStripButton.Click += new EventHandler(this.zoomOutButton_Click);

            this.zoomLabel.Text = "Zoom:";

            this.zoomComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.zoomComboBox.Items.AddRange(new object[] { "0.5x", "1.0x", "1.5x", "2.0x", "3.0x" });
            this.zoomComboBox.SelectedIndex = 1;
            this.zoomComboBox.SelectedIndexChanged += new EventHandler(this.zoomComboBox_SelectedIndexChanged);

            // Create Region Group dropdown and add it next to zoom dropdown
            this.comboBoxRegionGroups = new ComboBox();
            this.comboBoxRegionGroups.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxRegionGroups.Width = 150;
            this.comboBoxRegionGroups.SelectedIndexChanged += new EventHandler(this.comboBoxRegionGroups_SelectedIndexChanged);

            ToolStripControlHost regionGroupHost = new ToolStripControlHost(this.comboBoxRegionGroups);
            regionGroupHost.Margin = new Padding(10, 0, 0, 0); // Space it nicely

            this.toolStrip1.Items.Add(regionGroupHost);

            // panelRegionSidebar
            this.txtRegionSearch.Dock = DockStyle.Top;
            this.txtRegionSearch.PlaceholderText = "Search regions...";
            this.txtRegionSearch.TextChanged += new EventHandler(this.txtRegionSearch_TextChanged);

            this.checkedListBoxRegions.Dock = DockStyle.Fill;
            this.checkedListBoxRegions.FormattingEnabled = true;
            this.checkedListBoxRegions.Font = new Font("Segoe UI", 7.0f);

            this.panelRegionSidebar.Controls.Add(this.checkedListBoxRegions);
            this.panelRegionSidebar.Controls.Add(this.txtRegionSearch);
            this.panelRegionSidebar.Dock = DockStyle.Fill;
            this.panelRegionSidebar.BackColor = Color.WhiteSmoke;

            regionContextMenu = new ContextMenuStrip();

            editTagsMenuItem = new ToolStripMenuItem("Edit Tags", null, editTagsMenuItem_Click);
            compareTagsMenuItem = new ToolStripMenuItem("Compare With...", null, compareTagsMenuItem_Click);

            regionContextMenu.Items.AddRange(new ToolStripItem[] { editTagsMenuItem, compareTagsMenuItem });

            checkedListBoxRegions.ContextMenuStrip = regionContextMenu;
            checkedListBoxRegions.MouseDown += checkedListBoxRegions_MouseDown;

            // pictureBox1
            this.pictureBox1.Dock = DockStyle.Fill;
            this.pictureBox1.BackColor = Color.DimGray;
            this.pictureBox1.MouseWheel += pictureBox1_MouseWheel;

            // mainLayout
            this.mainLayout.Dock = DockStyle.Fill;
            this.mainLayout.ColumnCount = 2;
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); // menu
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); // toolbar
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            this.mainLayout.Controls.Add(this.menuStrip1, 0, 0);
            this.mainLayout.SetColumnSpan(this.menuStrip1, 2);
            this.mainLayout.Controls.Add(this.toolStrip1, 0, 1);
            this.mainLayout.SetColumnSpan(this.toolStrip1, 2);
            this.mainLayout.Controls.Add(this.panelRegionSidebar, 0, 2);
            this.mainLayout.Controls.Add(this.pictureBox1, 1, 2);

            // MainForm
            this.ClientSize = new Size(1000, 700);
            this.Controls.Add(this.mainLayout);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "UOX3 Atlas v0.1.0.Alpha";

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}