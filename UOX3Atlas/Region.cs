using System.Collections.Generic;
using System.Drawing;

public class Region
{
    public string Name { get; set; } = "Unnamed";
    public List<Rectangle> Bounds { get; set; } = new List<Rectangle>();
    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    public bool Visible = true;

    public Region()
    {
        Bounds = new List<Rectangle>();
    }
}

