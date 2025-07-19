using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

public static class RegionParser
{
    public static List<Region> LoadRegions(string filePath)
    {
        var regions = new List<Region>();
        var lines = File.ReadAllLines(filePath);

        Region currentRegion = null;
        int? world = null;
        int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.StartsWith("[REGION"))
            {
                // Before creating new region, check if previous should be added
                if (currentRegion != null)
                {
                    // Only include regions where WORLD is 0 or missing
                    if (world == null || world == 0)
                        regions.Add(currentRegion);
                }

                // Start new region block
                currentRegion = new Region();
                world = null; // reset world flag for each new region
            }
            else if (currentRegion != null && line.Contains("="))
            {
                var parts = line.Split('=');
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim().ToUpper();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "NAME":
                        currentRegion.Name = value;
                        break;
                    case "X1":
                        x1 = int.Parse(value);
                        break;
                    case "Y1":
                        y1 = int.Parse(value);
                        break;
                    case "X2":
                        x2 = int.Parse(value);
                        break;
                    case "Y2":
                        y2 = int.Parse(value);
                        currentRegion.Bounds.Add(Rectangle.FromLTRB(x1, y1, x2, y2));
                        break;
                    case "WORLD":
                        if (int.TryParse(value, out int parsedWorld))
                            world = parsedWorld;
                        break;
                    default:
                        // Store all other properties
                        currentRegion.Tags[key] = value;
                        break;
                }
            }
        }

        // Add last region
        if (currentRegion != null && (world == null || world == 0))
            regions.Add(currentRegion);

        return regions;
    }
}