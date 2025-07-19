# UOX3Atlas 🗺️  
*A Visual Region Editor and Map Viewer for UOX3*

UOX3Atlas is a powerful WinForms-based tool for viewing, editing, and managing `regions.dfn` files for [UOX3](https://www.uox3.org/), the Ultima Online server emulator. It provides a graphical interface to interact with your shard's regions visually—allowing zooming, panning, tag editing, region grouping, and more.

![UOX3Atlas Screenshot](docs/screenshot.png)

---

## ✨ Features

- ✅ **Interactive Map Viewer**  
  View and navigate your shard’s map with zoom and pan controls.

- ✅ **Region Editing Tools**  
  Create, drag, resize, and rename regions directly on the map.

- ✅ **Tag Editor UI**  
  View and edit UOX3-compatible region tags with descriptions.

- ✅ **Region Group Filters**  
  Filter regions by tags like `dungeon`, `guarded`, or `safezone`.

- ✅ **Persistent Settings**  
  Remembers last opened image and region files, zoom state, and more.

- ✅ **Undo Support (Ctrl+Z)**  
  Revert region changes quickly.

- ✅ **Context Menu Shortcuts**  
  Right-click any region in the list to edit tags instantly.

---

## 🖼 Icon & Theme

UOX3Atlas uses a custom symbolic icon (included as a resource) that reflects the parchment-and-ink aesthetic of classic Ultima lore.

---

## 🛠 Requirements

- .NET 6.0+  
- Visual Studio 2022 or newer (WinForms support)

---

## 🚀 Getting Started

1. **Clone the repository**  
   ```bash
   git clone https://github.com/yourname/UOX3Atlas.git
   ```

2. **Open the solution in Visual Studio**

3. **Build & Run**

4. **Load your UO map image**  
   Use **File → Open Image** to load your shard's custom map (typically a stitched BMP or PNG)
   Comes with Default UO map Image to use.

5. **Load Regions File**  
   Use **File → Load Regions File** to import your `regions.dfn`.

---

## 📁 File Structure

```
UOX3Atlas/
├── UOX3Atlas.sln
UOX3Atlas
├── /Properties          # .ico icon, embedded resources
├── /docs               # Screenshots or documentation assets
├── MainForm.cs          # Main UI logic
├── Region.cs           # Region data structure
├── regionGroups.cs       # Loading/saving DFN files
├── RegionParser.cs
├── UOX3Atlas.cs
└── README.md
```

---

## ✏️ Notes

- **UOX3 Tag Compatibility**: All region tags are handled in lowercase for editing, but saved as UOX3-compatible uppercase in the final DFN.
- **Zoom Presets**: Use the dropdown for fast zoom levels (`0.5x`, `1.0x`, `2.0x`, etc).
- **Group Filters**: Select a group from the dropdown to highlight only specific types of regions.

---

## 💡 Future Ideas

- Export selected regions to a new file  
- Region-to-script assignment  
- Highlight overlapping regions  
- Snap-to-grid for region boxes  
- Drag-and-drop map image overlay calibration

---

## 🤝 Contributing

Pull requests and suggestions are welcome! Please create an issue for discussion before submitting large changes.

---

## 📜 License

MIT License. See [LICENSE](LICENSE) for more information.

---

## 🧙 Credits

Created by [Dragon Slayer] for Ultima Online server admins and developers using UOX3.
