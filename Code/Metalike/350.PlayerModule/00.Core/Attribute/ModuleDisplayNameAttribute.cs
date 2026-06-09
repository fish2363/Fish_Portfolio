using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleDisplayNameAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public ModuleDisplayNameAttribute(string name, string description = "")
    {
        Name = name;
        Description = description;
    }
}
