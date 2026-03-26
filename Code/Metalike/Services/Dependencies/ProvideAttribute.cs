using System;

namespace GondrLib.Dependencies
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ProvideAttribute : Attribute
    {
    }
}