using Lid;
using System.Reflection;
#pragma warning disable CS8602
public  class CLIHelp
{
    [MethodDescription("help", "Displays detailed information about all method and property")]

    public void Help()
    {
        Console.WriteLine("\rProperties: ");
        foreach (var item in this.GetType().GetProperties().Where(x => x.GetCustomAttributes<PropertyAttribute>(true).Any()).Select(x => new { Property = x.GetCustomAttribute<PropertyAttribute>(), Type = x.PropertyType }))
        {
            Console.WriteLine(item.Property.PropertyName);
            Console.WriteLine("\tType : " + item!.Type.Name);
            Console.WriteLine("\tDescription : " + item!.Property.Description);

        }
        Console.WriteLine("\rMethod(s): ");
        foreach (var item in this.GetType().GetMethods().Where(x => x.GetCustomAttributes<MethodDescriptionAttribute>(true).Any()))
        {
            var matt = item.GetCustomAttribute<MethodDescriptionAttribute>();
            Console.WriteLine(matt.Method_Name);
            Console.WriteLine("\t" + matt!.Description);
            foreach (var parm in item.GetParameters())
            {
                if (parm.GetCustomAttribute<ParameterMethodDescriptionAttribute>() is ParameterMethodDescriptionAttribute pt)
                {
                    Console.WriteLine($"\t {parm.Name} : {pt.Description}");
                }
                else
                {
                    Console.WriteLine($"\t {parm.Name}");
                }
            }
        }

    }
    [MethodDescription("help", "Displays detailed information about a specific method or property")]
    public void Help([ParameterMethodDescription("Method or Property Name")] string name)
    {
        var item = this.GetType().GetProperties().FirstOrDefault(x => x.GetCustomAttribute<PropertyAttribute>(true) is PropertyAttribute pt && pt.PropertyName == name);
        if (item is not null)
        {
            var Property = item.GetCustomAttribute<PropertyAttribute>();
            Console.WriteLine(Property.PropertyName);
            Console.WriteLine("\tType : " + item!.PropertyType.Name);
            Console.WriteLine("\tDescription : " + Property!.Description);
            return;
        }
        var firstMethod = this.GetType().GetMethods()
    .FirstOrDefault(x => x.GetCustomAttribute<MethodDescriptionAttribute>(true) is MethodDescriptionAttribute MD && MD.Method_Name == name);

        if (firstMethod != null)
        {
            var matt = firstMethod.GetCustomAttribute<MethodDescriptionAttribute>();
            Console.WriteLine($"mthname: {matt.Method_Name}");
            Console.WriteLine($"\t{matt!.Description}");

            var firstParam = firstMethod.GetParameters().FirstOrDefault();
            if (firstParam != null)
            {
                if (firstParam.GetCustomAttribute<ParameterMethodDescriptionAttribute>() is ParameterMethodDescriptionAttribute pm)
                {
                    Console.WriteLine($"\t {firstParam.Name} : {pm.Description}");
                }
                else
                {
                    Console.WriteLine($"\t {firstParam.Name}");
                }
            }
            return;
        }
        Console.WriteLine("No method or property found with the given name.");



    }

}
