using System;

namespace TypeScriptGeneration
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
    public class TypeScriptAttribute : Attribute
    {
        public TypeScriptAttribute()
        {
            GenerateInterface = true;
        }
        public bool GenerateInterface { get; set; }
        public bool GenerateClass { get; set; }
        public bool GenerateTypeProperty { get; set; }
        public bool GenerateInterfaceMethods { get; set; }
    }
}