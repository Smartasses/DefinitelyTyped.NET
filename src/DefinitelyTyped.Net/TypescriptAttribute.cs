using System;

namespace DefinitelyTypedNet
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
    public class TypeScriptAttribute : Attribute
    {
        public TypeScriptAttribute()
        {     
        }
     
        public bool GenerateTypeProperty { get; set; }
    
    }
}