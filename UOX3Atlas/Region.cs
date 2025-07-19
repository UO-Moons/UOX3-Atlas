using System.Collections.Generic;
using System.Drawing;

public class Region
{
    public string Name { get; set; } = "Unnamed";
    public bool Visible { get; set; } = true;
    public List<Rectangle> Bounds { get; set; } = new List<Rectangle>();
    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

    public Region Clone()
    {
        return new Region
        {
            Name = this.Name,
            Visible = this.Visible,
            Tags = new Dictionary<string, string>(this.Tags),
            Bounds = new List<Rectangle>(this.Bounds)
        };
    }
}

