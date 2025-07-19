using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UOX3Atlas
{
    public partial class MainForm : Form
    {
        private Bitmap originalImage;
        private float zoomFactor = 1.0f;
        private List<Region> regions = new List<Region>();
        private bool showRegions = true;
        private Point imageDrawLocation = Point.Empty;
        private float imageDrawScale = 1.0f;
        private Point panOffset = new Point(0, 0);
        private Point mouseDownPos;
        private bool isDragging = false;
        private Region selectedRegion = null;
        private Rectangle selectedRect;
        private Point regionDragStart;
        private bool isMovingRegion = false;
        private bool isCreatingRegion = false;
        private Rectangle newRegionRect;
        private enum ResizeHandle { None, TopLeft, TopRight, BottomLeft, BottomRight }
        private ResizeHandle activeHandle = ResizeHandle.None;
        private bool isResizing = false;
        private string lastMapPath;
        private string lastRegionPath;

        private List<Region> undoStack = new List<Region>();

        private class EditorSettings
        {
            public string MapPath { get; set; }
            public string RegionPath { get; set; }
            public List<string> HiddenRegions { get; set; } = new List<string>();
        }

        private readonly string settingsPath = Path.Combine(Application.StartupPath, "settings.json");

        private void SaveSettings()
        {
            var settings = new EditorSettings
            {
                MapPath = lastMapPath,
                RegionPath = lastRegionPath,
                HiddenRegions = regions.Where(r => !r.Visible).Select(r => r.Name).ToList()
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(settingsPath, json);
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsPath))
                    return;

                string json = File.ReadAllText(settingsPath);
                var settings = JsonConvert.DeserializeObject<EditorSettings>(json);

                if (!string.IsNullOrEmpty(settings.MapPath) && File.Exists(settings.MapPath))
                {
                    lastMapPath = settings.MapPath; // <-- Important!
                    LoadImage(lastMapPath);
                }

                if (!string.IsNullOrEmpty(settings.RegionPath) && File.Exists(settings.RegionPath))
                {
                    lastRegionPath = settings.RegionPath; // <-- Important!
                    LoadRegionsFile(lastRegionPath);
                }

                if (settings.HiddenRegions != null)
                {
                    foreach (var region in regions)
                        region.Visible = !settings.HiddenRegions.Contains(region.Name);
                    UpdateRegionListUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load settings:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PushUndo()
        {
            var copy = new List<Region>();
            foreach (var r in regions)
            {
                var newRegion = new Region
                {
                    Name = r.Name,
                    Visible = r.Visible,
                    Tags = new Dictionary<string, string>(r.Tags) //Copy tags too!
                };

                foreach (var b in r.Bounds)
                    newRegion.Bounds.Add(b);

                copy.Add(newRegion);
            }

            undoStack = copy;
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z))
            {
                UndoLastAction();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UndoLastAction()
        {
            if (undoStack != null && undoStack.Count > 0)
            {
                regions = undoStack;
                selectedRegion = null;
                pictureBox1.Invalidate();
            }
        }

        public MainForm()
        {
            InitializeComponent();
            using (var stream = new MemoryStream(Properties.Resources.uox3atlas))
            {
                this.Icon = new Icon(stream);
            }
            // Hook up event handlers after components are initialized
            checkedListBoxRegions.ItemCheck += checkedListBoxRegions_ItemCheck;

            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;
            pictureBox1.MouseWheel += pictureBox1_MouseWheel;
            pictureBox1.Paint += pictureBox1_Paint;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // Load config or settings on form shown
            this.Shown += (s, e) => LoadSettings();

            PopulateRegionGroupsFromTags();
        }

        private void checkedListBoxRegions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((Action)(() =>
            {
                if (e.Index >= 0 && e.Index < regions.Count)
                {
                    regions[e.Index].Visible = checkedListBoxRegions.GetItemChecked(e.Index);
                    pictureBox1.Invalidate();
                    SaveSettings();
                }
            }));
        }

        private void UpdateRegionListUI()
        {
            checkedListBoxRegions.BeginUpdate();
            checkedListBoxRegions.Items.Clear();

            string filter = txtRegionSearch?.Text?.Trim().ToLower();

            for (int i = 0; i < regions.Count; i++)
            {
                string displayName = $"[{i + 1}] {regions[i].Name}";
                if (string.IsNullOrEmpty(filter) || displayName.ToLower().Contains(filter))
                {
                    checkedListBoxRegions.Items.Add(displayName, regions[i].Visible);
                }
            }

            for (int i = 0; i < regions.Count; i++)
            {
                var region = regions[i];
                string displayName = $"[{i}] {region.Name}";
                checkedListBoxRegions.Items.Add(displayName, region.Visible);
            }

            // Measure the longest item
            int maxWidth = 0;
            using (Graphics g = checkedListBoxRegions.CreateGraphics())
            {
                foreach (var item in checkedListBoxRegions.Items)
                {
                    SizeF size = g.MeasureString(item.ToString(), checkedListBoxRegions.Font);
                    if (size.Width > maxWidth)
                        maxWidth = (int)size.Width;
                }
            }

            // Set scrollbar width (add padding)
            checkedListBoxRegions.HorizontalExtent = maxWidth + 20;

            checkedListBoxRegions.EndUpdate();
        }


        private void LoadImage(string filePath)
        {
            originalImage?.Dispose();
            using (var tempImage = new Bitmap(filePath))
            {
                int maxDimension = 3000;
                float scale = Math.Min((float)maxDimension / tempImage.Width, (float)maxDimension / tempImage.Height);
                int newWidth = (int)(tempImage.Width * scale);
                int newHeight = (int)(tempImage.Height * scale);
                originalImage = new Bitmap(tempImage, newWidth, newHeight);
            }

            zoomFactor = 1.0f;
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (originalImage == null)
                return;

            e.Graphics.Clear(pictureBox1.BackColor);

            // Drawing dimensions
            int drawWidth = (int)(originalImage.Width * zoomFactor);
            int drawHeight = (int)(originalImage.Height * zoomFactor);
            int offsetX = (pictureBox1.Width - drawWidth) / 2;
            int offsetY = (pictureBox1.Height - drawHeight) / 2;

            var originalTransform = e.Graphics.Transform;

            // Setup transform for zoom and pan
            var transform = new System.Drawing.Drawing2D.Matrix();
            //transform.Translate(offsetX, offsetY);
            transform.Translate(panOffset.X, panOffset.Y);
            transform.Scale(zoomFactor, zoomFactor);
            e.Graphics.Transform = transform;

            // Draw the image
            e.Graphics.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);

            // Calculate UO -> pixel scaling
            float scaleX = (float)originalImage.Width / 7168f;
            float scaleY = (float)originalImage.Height / 4096f;

            // Draw UO regions
            if (showRegions && regions != null)
            {
                using (var pen = new Pen(Color.Red, 1f / zoomFactor))
                using (var font = new Font("Arial", 10f / zoomFactor, FontStyle.Bold))
                using (var selectedPen = new Pen(Color.Lime, 2f / zoomFactor))
                using (var dashedPen = new Pen(Color.Orange, 1f / zoomFactor) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    string selectedGroup = comboBoxRegionGroups?.SelectedItem?.ToString() ?? "All Regions";
                    bool filterActive = selectedGroup != "All Regions";
                    HashSet<string>? filteredRegionNames = filterActive
                        ? checkedListBoxRegions.Items.Cast<string>().ToHashSet()
                        : null;
                    foreach (var region in regions)
                    {
                        if (!region.Visible || (filterActive && !filteredRegionNames.Contains(region.Name)))
                            continue;

                        foreach (var rect in region.Bounds)
                        {
                            RectangleF scaled = new RectangleF(
                                rect.X * scaleX,
                                rect.Y * scaleY,
                                rect.Width * scaleX,
                                rect.Height * scaleY
                            );

                            // Highlight selected region
                            if (region == selectedRegion && rect == selectedRect)
                            {
                                e.Graphics.DrawRectangle(selectedPen, scaled.X, scaled.Y, scaled.Width, scaled.Height);
                            }
                            else
                            {
                                e.Graphics.DrawRectangle(pen, scaled.X, scaled.Y, scaled.Width, scaled.Height);
                            }

                            // Draw resize handles if selected
                            if (region == selectedRegion && rect == selectedRect)
                            {
                                float handleSize = 6f / zoomFactor;

                                e.Graphics.FillRectangle(Brushes.White, scaled.X - handleSize / 2, scaled.Y - handleSize / 2, handleSize, handleSize);
                                e.Graphics.FillRectangle(Brushes.White, scaled.Right - handleSize / 2, scaled.Y - handleSize / 2, handleSize, handleSize);
                                e.Graphics.FillRectangle(Brushes.White, scaled.X - handleSize / 2, scaled.Bottom - handleSize / 2, handleSize, handleSize);
                                e.Graphics.FillRectangle(Brushes.White, scaled.Right - handleSize / 2, scaled.Bottom - handleSize / 2, handleSize, handleSize);
                            }

                            var textSize = e.Graphics.MeasureString(region.Name, font);
                            e.Graphics.DrawString(region.Name, font, Brushes.Yellow, scaled.X, scaled.Y - textSize.Height);
                        }
                    }

                    // Draw preview for new region being created
                    if (isCreatingRegion)
                    {
                        RectangleF scaledPreview = new RectangleF(
                            newRegionRect.X * scaleX,
                            newRegionRect.Y * scaleY,
                            newRegionRect.Width * scaleX,
                            newRegionRect.Height * scaleY
                        );

                        e.Graphics.DrawRectangle(dashedPen, scaledPreview.X, scaledPreview.Y, scaledPreview.Width, scaledPreview.Height);
                    }
                }
            }


            e.Graphics.Transform = originalTransform;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            })
            {
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    lastMapPath = openDialog.FileName;
                    LoadImage(lastMapPath);
                    SaveSettings();
                }
            }
        }

        private void zoomInButton_Click(object sender, EventArgs e)
        {
            zoomFactor += 0.1f;
            ClampZoom();
            UpdateZoomUI();
            pictureBox1.Invalidate();
        }

        private void zoomOutButton_Click(object sender, EventArgs e)
        {
            if (zoomFactor > 0.2f) // Prevent going below 0.1
                zoomFactor -= 0.1f;

            ClampZoom();
            UpdateZoomUI();
            pictureBox1.Invalidate();
        }

        private void zoomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = zoomComboBox.SelectedItem.ToString().Replace("x", "");
            if (float.TryParse(selected, out float zoom))
            {
                zoomFactor = zoom;
                UpdateZoomUI(); // Sync combo box state if zoom level was typed in or added manually
                pictureBox1.Invalidate();
            }
        }

        private void ClampZoom()
        {
            if (zoomFactor < 0.1f)
                zoomFactor = 0.1f;
            if (zoomFactor > 5.0f)
                zoomFactor = 5.0f;
        }

        private void UpdateZoomUI()
        {
            string zoomString = $"{Math.Round(zoomFactor, 1):0.0}x";
            if (zoomComboBox.Items.Contains(zoomString))
                zoomComboBox.SelectedItem = zoomString;
            else
            {
                zoomComboBox.Items.Add(zoomString);
                zoomComboBox.SelectedItem = zoomString;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettings();
            originalImage?.Dispose();
            base.OnFormClosing(e);
        }

        private void LoadRegionsFile(string regionFilePath)
        {
            regions = RegionParser.LoadRegions(regionFilePath);
            pictureBox1.Invalidate();
            UpdateRegionListUI();
        }

        private void loadRegionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Regions File|*.dfn"
            })
            {
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    lastRegionPath = openDialog.FileName;
                    LoadRegionsFile(lastRegionPath);
                    SaveSettings();
                }
            }

            // Then add:
            PopulateRegionGroupsFromTags();
        }

        private void toggleRegionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Determine desired state by checking if all visible items are already checked
            bool anyUnchecked = false;
            for (int i = 0; i < checkedListBoxRegions.Items.Count; i++)
            {
                if (!checkedListBoxRegions.GetItemChecked(i))
                {
                    anyUnchecked = true;
                    break;
                }
            }

            // Apply new state to visible items in the list
            for (int i = 0; i < checkedListBoxRegions.Items.Count; i++)
            {
                checkedListBoxRegions.SetItemChecked(i, anyUnchecked);

                string regionName = checkedListBoxRegions.Items[i].ToString();
                var region = regions.FirstOrDefault(r => r.Name == regionName);
                if (region != null)
                    region.Visible = anyUnchecked;
            }

            SaveSettings();
            pictureBox1.Invalidate();
        }


        private bool isPanning = false;

        private void checkedListBoxRegions_MouseDown(object sender, MouseEventArgs e)
        {
            int index = checkedListBoxRegions.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
                checkedListBoxRegions.SelectedIndex = index;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalImage == null)
                return;

            // --- BEGIN: New Region Creation ---
            if (e.Button == MouseButtons.Left && ModifierKeys.HasFlag(Keys.Shift))
            {
                isCreatingRegion = true;
                PointF start = ScreenToMapCoords(e.Location);
                newRegionRect = new Rectangle((int)start.X, (int)start.Y, 0, 0);
                regionDragStart = e.Location;
                pictureBox1.Cursor = Cursors.Cross;
                return;
            }
            // --- END: New Region Creation ---

            if (e.Button == MouseButtons.Left)
            {
                PointF pt = ScreenToMapCoords(e.Location);

                // Check if clicked on a handle
                if (selectedRegion != null && selectedRect != Rectangle.Empty)
                {
                    Rectangle screenRect = MapToScreenRect(selectedRect);
                    activeHandle = GetHandleUnderMouse(screenRect, e.Location);
                    if (activeHandle != ResizeHandle.None)
                    {
                        PushUndo();
                        isResizing = true;
                        regionDragStart = e.Location;
                        pictureBox1.Cursor = Cursors.Cross;
                        return;
                    }
                }

                // Check for region under cursor
                selectedRegion = null;
                foreach (var region in regions)
                {
                    foreach (var rect in region.Bounds)
                    {
                        if (rect.Contains((int)pt.X, (int)pt.Y))
                        {
                            PushUndo(); // SAVE STATE before modifying the region
                            selectedRegion = region;
                            selectedRect = rect;
                            regionDragStart = e.Location;
                            isMovingRegion = true;
                            pictureBox1.Cursor = Cursors.SizeAll;
                            return;
                        }
                    }
                }

                // No region clicked start panning
                isPanning = true;
                mouseDownPos = e.Location;
                pictureBox1.Cursor = Cursors.Hand;
            }
            else if (e.Button == MouseButtons.Right && selectedRegion != null)
            {
                var menu = new ContextMenuStrip();
                menu.Items.Add("Rename", null, (s, a) => RenameSelectedRegion());
                menu.Items.Add("Delete", null, (s, a) => DeleteSelectedRegion());
                menu.Items.Add("Edit Tags", null, (s, a) => ShowRegionEditor(selectedRegion)); // <-- add this line
                menu.Show(pictureBox1, e.Location);
            }
        }


        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (originalImage == null)
                return;

            if (isCreatingRegion)
            {
                PointF start = ScreenToMapCoords(regionDragStart);
                PointF end = ScreenToMapCoords(e.Location);
                newRegionRect = new Rectangle(
                    (int)Math.Min(start.X, end.X),
                    (int)Math.Min(start.Y, end.Y),
                    (int)Math.Abs(end.X - start.X),
                    (int)Math.Abs(end.Y - start.Y)
                );
                pictureBox1.Invalidate();
                return;
            }

            if (isResizing && selectedRegion != null)
            {
                PointF startMap = ScreenToMapCoords(regionDragStart);
                PointF currentMap = ScreenToMapCoords(e.Location);

                int x1 = selectedRect.X;
                int y1 = selectedRect.Y;
                int x2 = x1 + selectedRect.Width;
                int y2 = y1 + selectedRect.Height;

                if (activeHandle == ResizeHandle.TopLeft)
                {
                    x1 = (int)currentMap.X;
                    y1 = (int)currentMap.Y;
                }
                else if (activeHandle == ResizeHandle.TopRight)
                {
                    x2 = (int)currentMap.X;
                    y1 = (int)currentMap.Y;
                }
                else if (activeHandle == ResizeHandle.BottomLeft)
                {
                    x1 = (int)currentMap.X;
                    y2 = (int)currentMap.Y;
                }
                else if (activeHandle == ResizeHandle.BottomRight)
                {
                    x2 = (int)currentMap.X;
                    y2 = (int)currentMap.Y;
                }

                Rectangle resized = new Rectangle(
                    Math.Min(x1, x2),
                    Math.Min(y1, y2),
                    Math.Abs(x2 - x1),
                    Math.Abs(y2 - y1)
                );

                selectedRegion.Bounds.Remove(selectedRect);
                selectedRegion.Bounds.Add(resized);
                selectedRect = resized;
                regionDragStart = e.Location;
                pictureBox1.Invalidate();
                return;
            }

            if (isMovingRegion && selectedRegion != null)
            {
                PointF pt1 = ScreenToMapCoords(regionDragStart);
                PointF pt2 = ScreenToMapCoords(e.Location);

                int dx = (int)(pt2.X - pt1.X);
                int dy = (int)(pt2.Y - pt1.Y);

                selectedRegion.Bounds.Remove(selectedRect);
                Rectangle moved = new Rectangle(
                    selectedRect.X + dx,
                    selectedRect.Y + dy,
                    selectedRect.Width,
                    selectedRect.Height
                );
                selectedRegion.Bounds.Add(moved);
                selectedRect = moved;
                regionDragStart = e.Location;
                pictureBox1.Invalidate();
            }
            else if (isPanning)
            {
                int dx = e.X - mouseDownPos.X;
                int dy = e.Y - mouseDownPos.Y;
                panOffset.X += dx;
                panOffset.Y += dy;
                mouseDownPos = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (originalImage == null)
                return;

            if (isResizing)
            {
                isResizing = false;
                pictureBox1.Cursor = Cursors.Default;
                return;
            }
            else if (isMovingRegion)
            {
                isMovingRegion = false;
                pictureBox1.Cursor = Cursors.Default;
            }
            else if (isPanning)
            {
                isPanning = false;
                pictureBox1.Cursor = Cursors.Default;
            }
            else if (isCreatingRegion)
            {
                isCreatingRegion = false;
                pictureBox1.Cursor = Cursors.Default;

                if (newRegionRect.Width < 4 || newRegionRect.Height < 4)
                    return; // too small, ignore

                // Prompt for name
                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter name for new region:", "New Region", "New Region");
                if (string.IsNullOrWhiteSpace(input))
                    return;

                PushUndo();

                Region newRegion = new Region
                {
                    Name = input.Trim(),
                    Visible = true
                };
                newRegion.Bounds.Add(newRegionRect);
                regions.Add(newRegion);

                UpdateRegionListUI();
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            const float zoomStep = 0.1f;

            float oldZoom = zoomFactor;

            if (e.Delta > 0)
                zoomFactor = Math.Min(5.0f, zoomFactor + zoomStep);
            else
                zoomFactor = Math.Max(0.1f, zoomFactor - zoomStep);

            // Adjust pan to center zoom on cursor
            PointF mouseBeforeZoom = ScreenToMapCoords(e.Location);
            PointF mouseAfterZoom = ScreenToMapCoords(e.Location);

            panOffset.X += (int)(mouseAfterZoom.X - mouseBeforeZoom.X);
            panOffset.Y += (int)(mouseAfterZoom.Y - mouseBeforeZoom.Y);

            UpdateZoomUI(); // This updates the combo box label
            pictureBox1.Invalidate();
        }

        private void RenameSelectedRegion()
        {
            if (selectedRegion == null)
                return;

            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter new region name:", "Rename Region", selectedRegion.Name);
            if (!string.IsNullOrWhiteSpace(input))
            {
                PushUndo(); // Save state before renaming
                selectedRegion.Name = input.Trim();
                pictureBox1.Invalidate();
            }
        }

        private void DeleteSelectedRegion()
        {
            if (selectedRegion == null)
                return;

            var result = MessageBox.Show($"Delete region '{selectedRegion.Name}'?", "Confirm", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                PushUndo(); // Save state before removal
                regions.Remove(selectedRegion);
                selectedRegion = null;
                pictureBox1.Invalidate();
            }
        }


        private PointF ScreenToMapCoords(Point screenPt)
        {
            if (originalImage == null)
                return PointF.Empty;

            float scale = zoomFactor;

            return new PointF(
                (screenPt.X - panOffset.X) / scale * 7168f / originalImage.Width,
                (screenPt.Y - panOffset.Y) / scale * 4096f / originalImage.Height
            );
        }

        private void saveRegionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "UOX3 Region File|*.dfn",
                Title = "Save Regions As"
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    SaveRegionsToFile(saveDialog.FileName);
                }
            }
        }

        private void SaveRegionsToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                int regionIndex = 1;

                foreach (var region in regions)
                {
                    writer.WriteLine($"[REGION {regionIndex}]");
                    writer.WriteLine("{");

                    // Only write NAME if it originally existed
                    if (region.Tags.ContainsKey("NAME"))
                        writer.WriteLine($"NAME={region.Name}");

                    // Write preserved tags (except X1/Y1/X2/Y2)
                    foreach (var tag in region.Tags)
                    {
                        string tagKey = tag.Key.ToUpper();
                        if (tagKey != "X1" && tagKey != "Y1" && tagKey != "X2" && tagKey != "Y2" && tagKey != "NAME")
                        {
                            writer.WriteLine($"{tag.Key}={tag.Value}");
                        }
                    }

                    // Write updated bounds
                    foreach (var rect in region.Bounds)
                    {
                        writer.WriteLine($"X1={rect.Left}");
                        writer.WriteLine($"Y1={rect.Top}");
                        writer.WriteLine($"X2={rect.Right}");
                        writer.WriteLine($"Y2={rect.Bottom}");
                    }

                    writer.WriteLine("}");
                    writer.WriteLine();
                    regionIndex++;
                }
            }

            MessageBox.Show("Regions saved successfully!", "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private ResizeHandle GetHandleUnderMouse(Rectangle rect, Point mouse)
        {
            const int handleSize = 6;

            if (new Rectangle(rect.Left - handleSize / 2, rect.Top - handleSize / 2, handleSize, handleSize).Contains(mouse))
                return ResizeHandle.TopLeft;
            if (new Rectangle(rect.Right - handleSize / 2, rect.Top - handleSize / 2, handleSize, handleSize).Contains(mouse))
                return ResizeHandle.TopRight;
            if (new Rectangle(rect.Left - handleSize / 2, rect.Bottom - handleSize / 2, handleSize, handleSize).Contains(mouse))
                return ResizeHandle.BottomLeft;
            if (new Rectangle(rect.Right - handleSize / 2, rect.Bottom - handleSize / 2, handleSize, handleSize).Contains(mouse))
                return ResizeHandle.BottomRight;

            return ResizeHandle.None;
        }

        private Rectangle MapToScreenRect(Rectangle rect)
        {
            if (originalImage == null)
                return Rectangle.Empty;

            float scaleX = (float)originalImage.Width / 7168f * zoomFactor;
            float scaleY = (float)originalImage.Height / 4096f * zoomFactor;

            int x = (int)(rect.X * scaleX + panOffset.X);
            int y = (int)(rect.Y * scaleY + panOffset.Y);
            int w = (int)(rect.Width * scaleX);
            int h = (int)(rect.Height * scaleY);
            return new Rectangle(x, y, w, h);
        }

        private void txtRegionSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateRegionListUI();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string helpText = @"
== Image Zoom Tool Help ==

• SHIFT + Left Click + Drag:
  Create a new region on the map.

• Left Click and drag inside a region:
  Move the selected region.

• Left Click on a corner handle:
  Resize the selected region.

• Mouse Wheel:
  Zoom in/out.

• Middle or Left Drag (no region selected):
  Pan the map.

• Right Click on a region:
  Rename or delete it.

• [File] Menu:
  - Open Image: Load the map image (e.g., facet0.png)
  - Load Regions File: Load UOX3 .dfn region file
  - Save Regions File: Save changes to regions
  - Toggle Regions: Show/hide all regions

• Region Sidebar:
  Shows all loaded region names with checkboxes to toggle visibility.

• CTRL + Z:
  Undo the last edit (move, resize, add, delete)
";
            MessageBox.Show(helpText, "How to Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}