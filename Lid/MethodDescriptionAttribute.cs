
namespace Lid
{
    public class MethodDescriptionAttribute(string method_name,string des) : Attribute
    {
        public string Method_Name { get; } = method_name;
        public string Description { get; set; } = des;
    }
}