using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lid
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter)]
    public class BindAttribute:Attribute
    {
        public readonly IBinding Binding;
        public BindAttribute(Type bind)
        {

            if (!typeof(IBinding).IsAssignableFrom(bind))
                throw new ArgumentException("Binding must implement IBinding");
            //how to turn the type to IBinding?
            Binding = (IBinding)Activator.CreateInstance(bind)!;
        }
        public dynamic Invoke(string value)=>Binding.Bind(value);
    }
}
