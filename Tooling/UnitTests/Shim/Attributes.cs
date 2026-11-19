using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeFieldAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAssetMenuAttribute : Attribute
    {
        public string menuName;
        public string fileName;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RequireComponentAttribute : Attribute
    {
        public RequireComponentAttribute(Type type) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultExecutionOrderAttribute : Attribute
    {
        public DefaultExecutionOrderAttribute(int order) { }
    }
}
