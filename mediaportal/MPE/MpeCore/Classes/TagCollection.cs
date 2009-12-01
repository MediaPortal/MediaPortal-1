using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class TagCollection
    {
        public TagCollection()
        {
            Tags = new List<string>();
        }

        public TagCollection(string tags)
        {
            Tags = new List<string>();
            Pharse(tags);
        }

        public List<string> Tags { get; set; }

        public void Add(TagCollection collection)
        {

            foreach (string s in collection.Tags)
            {
                Add(s);
            }
        }

        public void Add(string tag)
        {
            tag = tag.ToLower().Trim();
            if (string.IsNullOrEmpty(tag))
                return;
            if (!Tags.Contains(tag))
                Tags.Add(tag);
        }

        public void Pharse(string tags)
        {
            string[] list = tags.Split(',');
            foreach (string s in list)
            {
                Add(s);
            }
            
        }
    }
}
