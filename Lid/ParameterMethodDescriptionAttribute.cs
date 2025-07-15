
namespace Lid
{
    public class ParameterMethodDescriptionAttribute(string Description) : Attribute
    {
        public string Description { get; set; } = Description;
    }
}