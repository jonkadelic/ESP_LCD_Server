using LCDWidget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ESP_LCD_Server
{
    public static class WidgetLoader
    {
        public static List<Type> WidgetTypes { get; } = new List<Type>();

        public static void LoadWidgets()
        {
            string[] dlls = Directory.GetFiles(".", "*.dll");

            foreach (string dll in dlls)
            {
                Assembly assembly = Assembly.LoadFrom(dll);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(BaseWidget)) && type.IsAbstract == false)
                    {
                        Logger.Log($"Loaded widget {type.Name} from assembly {assembly.GetName().Name}.", typeof(WidgetLoader));
                        WidgetTypes.Add(type);
                    }
                }
            }
        }
    }
}
