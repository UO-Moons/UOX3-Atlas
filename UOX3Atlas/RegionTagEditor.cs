using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UOX3Atlas
{
    public partial class MainForm : Form
    {
        private Dictionary<string, string> tagDescriptions = new Dictionary<string, string>
        {
        {"ABWEATH", "Assign a weather system from weather.dfn"},
        {"APPEARANCE", "Region appearance (0=Spring, 1=Summer, etc)"},
        {"BUYABLE", "Advanced Trade System buyable value"},
        {"CHANCEFORBIGORE", "Chance from 0 to 100 to get 5 ores instead of 1"},
        {"DISABLED", "If 1, disables the region entirely"},
        {"DUNGEON", "If 1, darkens lighting for players"},
        {"ESCORTS", "If 1, enables region as Escort Quest target"},
        {"GATE", "If 1, enables gate travel in and out"},
        {"GOOD", "Advanced Trade System good ID"},
        {"GUARDED", "If 1, region is protected by guards"},
        {"GUARDLIST", "Guard spawn list ID from npclists.dfn"},
        {"GUARDOWNER", "Name of the guards protecting this region"},
        {"INSTANCEID", "Instance ID this region belongs to"},
        {"MAGICDAMAGE", "If 1, allows hostile magic in region"},
        {"MARK", "If 1, allows rune marking in region"},
        {"MUSICLIST", "Music list section for region background music"},
        {"NAME", "Region name (used in [REGION])"},
        {"OREPREF", "Ore type and chance for region mining"},
        {"RACE", "ID of race owning this region"},
        {"RANDOMVALUE", "Advanced Trade System random value"},
        {"RECALL", "If 1, enables Recall spell"},
        {"SAFEZONE", "If 1, disallows all hostile actions"},
        {"SCRIPT", "JS script assigned to region"},
        {"SELLABLE", "Advanced Trade System sellable values"},
        {"SPAWN", "Predefined spawn ID from spawn DFNs"},
        {"TELEPORT", "If 1, enables Teleport spell (default is 1)"},
        {"WORLD", "World number this region is in"}
        };


        private void ShowRegionEditor(Region region)
        {
            Form tagEditor = new Form();
            tagEditor.Text = $"Edit Region: {region.Name}";
            tagEditor.Width = 500;
            tagEditor.Height = 600;

            Panel panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Button saveButton = new Button { Text = "Save", Dock = DockStyle.Bottom };
            tagEditor.Controls.Add(panel);
            tagEditor.Controls.Add(saveButton);

            Dictionary<string, TextBox> tagInputs = new Dictionary<string, TextBox>();
            int y = 10;
            foreach (var tag in tagDescriptions.OrderBy(t => t.Key))
            {
                Label label = new Label
                {
                    Text = $"{tag.Key} - {tag.Value}",
                    Location = new Point(10, y),
                    Width = 460
                };
                y += 18;

                string existingValue = region.Tags.TryGetValue(tag.Key.ToUpper(), out var value) ? value : string.Empty;

                TextBox input = new TextBox
                {
                    Name = tag.Key,
                    Location = new Point(10, y),
                    Width = 460,
                    Text = existingValue
                };

                panel.Controls.Add(label);
                panel.Controls.Add(input);
                tagInputs[tag.Key] = input;
                y += 28;
            }

            // Also show custom tags that aren't in tagDescriptions
            foreach (var kvp in region.Tags)
            {
                if (!tagDescriptions.ContainsKey(kvp.Key.ToUpper()))
                {
                    Label label = new Label
                    {
                        Text = $"{kvp.Key} - (Custom Tag)",
                        Location = new Point(10, y),
                        Width = 460
                    };
                    y += 18;
                    TextBox input = new TextBox
                    {
                        Name = kvp.Key,
                        Location = new Point(10, y),
                        Width = 460,
                        Text = kvp.Value
                    };
                    panel.Controls.Add(label);
                    panel.Controls.Add(input);
                    tagInputs[kvp.Key] = input;
                    y += 28;
                }
            }

            saveButton.Click += (s, e) =>
            {
                foreach (var tag in tagInputs)
                {
                    string val = tag.Value.Text.Trim();
                    if (val.Length > 0)
                        region.Tags[tag.Key.ToUpper()] = val;
                    else if (region.Tags.ContainsKey(tag.Key))
                        region.Tags.Remove(tag.Key);
                }
                tagEditor.Close();
                pictureBox1.Invalidate();
            };

            tagEditor.Show();
        }

        private void editTagsMenuItem_Click(object sender, EventArgs e)
        {
            if (checkedListBoxRegions.SelectedIndex >= 0)
            {
                var selectedRegion = regions[checkedListBoxRegions.SelectedIndex];
                ShowRegionEditor(selectedRegion);
            }
        }

        private void compareTagsMenuItem_Click(object sender, EventArgs e)
        {
            if (checkedListBoxRegions.SelectedIndex >= 0)
            {
                var selectedRegion = regions[checkedListBoxRegions.SelectedIndex];

                // Prompt user to choose another region to compare
                var pickDialog = new Form
                {
                    Text = "Select Region to Compare",
                    Width = 300,
                    Height = 400
                };

                ListBox regionListBox = new ListBox { Dock = DockStyle.Fill };
                regionListBox.Items.AddRange(regions.Where(r => r != selectedRegion).Select(r => r.Name).ToArray());
                pickDialog.Controls.Add(regionListBox);

                regionListBox.DoubleClick += (s2, e2) =>
                {
                    if (regionListBox.SelectedItem != null)
                    {
                        var otherRegion = regions.FirstOrDefault(r => r.Name == regionListBox.SelectedItem.ToString());
                        ShowRegionEditor(selectedRegion);
                        ShowRegionEditor(otherRegion);
                        pickDialog.Close();
                    }
                };

                pickDialog.ShowDialog();
            }
        }
    }
}