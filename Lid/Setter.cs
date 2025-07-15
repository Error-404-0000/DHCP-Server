using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Lid
{
    public class Setter<T> where T : new()
    {
        private readonly T _instance;
        public Setter()
        {
            _instance = new T();
        }
        public Setter(T t)
        {
            _instance = t;
        }
        public T Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return Activator.CreateInstance<T>();
            command = command.TrimStart();
            command = command.TrimEnd() + " ";

            if (command.StartsWith("-"))
            {
                ProcessProperties(command);
            }
            else
            {
                InvokeMethod(command);
            }

            return _instance;
        }

        private void ProcessProperties(string command)
        {
            var properties = _instance.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttributes<PropertyAttribute>().Any())
                .ToDictionary(
                    p => p.GetCustomAttribute<PropertyAttribute>().PropertyName,
                    p => p);

            var matches = Regex.Matches(command, "-([\\w-]+)\\s+([^{}]*?\\s|{.*?}\\s*)");
            if(matches.Count == 0)
            {
                Console.WriteLine($"No {command} properties found.");
                return;
            }
            foreach (Match match in matches)
            {
                string propertyName = match.Groups[1].Value;
                string value = match.Groups[2].Value.Trim();

                if (!properties.TryGetValue(propertyName, out var property))
                {
                    Console.WriteLine("Property '{0}' not found.", propertyName);
                    continue;
                }

                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    value = value.Trim('{', '}').Trim();
                }

                try
                {
                    if(property.GetCustomAttribute<BindAttribute>() is BindAttribute bind)
                    {
                        property.SetValue(_instance, bind.Invoke(value));
                    }
                    else {
                        object convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(_instance, convertedValue);
                    }
                }
                catch(Exception ex)
                {
                 
                        Console.WriteLine("Failed to set property '{0}' with value '{1}'. Error : {2}", propertyName, value, (ex.InnerException ?? ex).Message);
                    
                }
            }
        }

        private void InvokeMethod(string command)
        {
            var parts = SplitCommand(command);
            if (parts.Length == 0)
                throw new ArgumentException("Invalid command format.");

            string methodName = parts[0];
            string[] parameters = parts.Skip(1).ToArray();

            var methods = _instance.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.GetCustomAttribute<MethodDescriptionAttribute>() is MethodDescriptionAttribute MD && MD.Method_Name== methodName)
                .ToList();

            if (!methods.Any())
                throw new InvalidOperationException($"Method '{methodName}' not found.");

            foreach (var method in methods)
            {
                var methodParameters = method.GetParameters();
                var parameterValues = new object[methodParameters.Length];

                if (TryMatchParameters(methodParameters, parameters, parameterValues))
                {
                    method.Invoke(_instance, parameterValues);
                    return;
                }
            }

            throw new InvalidOperationException($"'{methodName}' requires {string.Join(",",methods.First().GetParameters().Select(x=> $"{x.Name}:{x.ParameterType}"))}");
        }

        private string[] SplitCommand(string command)
        {
            return Regex.Matches(command, "{.*?}|\\S+")
                .Cast<Match>()
                .Select(m => m.Value.Trim('{', '}'))
                .ToArray();
        }

        private bool TryMatchParameters(ParameterInfo[] parameters, string[] parameterParts, object[] methodParams)
        {
            if (parameters.Length != parameterParts.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    if (parameters[i].GetCustomAttribute<BindAttribute>(true) is BindAttribute bind)
                    {
                        methodParams[i]= bind.Invoke(parameterParts[i]);
                    }
                    else
                    {
                        methodParams[i] = Convert.ChangeType(parameterParts[i], parameters[i].ParameterType);

                    }
                }
                catch
                {
                        return false;
                 
                }
            }

            return true;
        }

    }

 
}
