using System;

namespace ProjectInfinity.Plugins
{
    //I'm not sure whether this class should be declared in the Interface project or not.
    //It is tightly linked to the ReflectionPluginManager inner workings, but all
    //plugins should have this attribute to be recognized by the ReflectionPluginManager
    //as a plugin.  These plugins should only have references to the interface project, so
    //I guess it belongs here....
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class PluginAttribute : Attribute
    {
        public string Name;
        public string Description;

        public PluginAttribute(string name)
        {
            Name = name;
        }

        public PluginAttribute(string name, string description) : this(name)
        {
            Description = description;
        }

        //TODO: Add Icon and other properties
    }
}