using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UOX3Atlas
{
    public partial class MainForm : Form
    {
        private Panel panelRegionHeader;

        private void InitializeRegionGroupFilterUI()
        {
            // Create a new panel to hold the combo box
            Panel panelRegionFilterHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(4),
                BackColor = Color.LightSteelBlue
            };

            comboBoxRegionGroups = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            comboBoxRegionGroups.SelectedIndexChanged += comboBoxRegionGroups_SelectedIndexChanged;
            panelRegionFilterHeader.Controls.Add(comboBoxRegionGroups);
            panelRegionSidebar.Controls.Add(panelRegionFilterHeader); // Ensure this line adds the filter panel
            panelRegionSidebar.Controls.SetChildIndex(panelRegionFilterHeader, 0); // Put above other controls
        }

        private void comboBoxRegionGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyRegionGroupFilter();
        }

        private void ApplyRegionGroupFilter()
        {
            string selected = comboBoxRegionGroups.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected) || selected == "All Regions")
            {
                UpdateRegionListUI();
                return;
            }

            checkedListBoxRegions.BeginUpdate();
            checkedListBoxRegions.Items.Clear();

            List<Region> filtered;
            switch (selected)
            {
                case "Towns":
                    filtered = regions.Where(r => r.Tags.TryGetValue("GUARDED", out string g) && g == "1").ToList();
                    break;
                case "Dungeons":
                    filtered = regions.Where(r => r.Tags.TryGetValue("DUNGEON", out string d) && d == "1").ToList();
                    break;
                default:
                    filtered = new List<Region>();
                    break;
            }

            foreach (var region in filtered)
                checkedListBoxRegions.Items.Add(region.Name, region.Visible);

            checkedListBoxRegions.EndUpdate();
        }

        private void PopulateRegionGroupsFromTags()
        {
            comboBoxRegionGroups.Items.Clear();
            comboBoxRegionGroups.Items.Add("All Regions");
            comboBoxRegionGroups.Items.Add("Towns");
            comboBoxRegionGroups.Items.Add("Dungeons");

            comboBoxRegionGroups.SelectedIndex = 0;
        }
    }
}