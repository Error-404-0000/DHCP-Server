[AttributeUsage(AttributeTargets.Property)]
public class PropertyAttribute : Attribute
{
    public string PropertyName { get; }
    public string Description { get; } 
    public PropertyAttribute(string cmd,string des)
    {
        PropertyName=cmd;
        Description=des;
    }
}