using System.Collections.Generic;

namespace AsinoPuzzles.Functions.Models
{
    public sealed class Element
    {
        public string Text { get; set; }
    }

    public sealed class Section
    {
        public string Type { get; set; }
        public List<Element> Elements { get; set; }
        public Element Element { get; set; }
    }

    public sealed class Document
    {
        public List<Section> Sections { get; set; }
    }
}
