using System;

namespace Crm.Repository
{
    public class AutoMappersAttribute : Attribute
    {
        public Type[] ToSource { get; private set; }

        public AutoMappersAttribute(params Type[] toSource)
        {
            this.ToSource = toSource;
        }
    }
}
