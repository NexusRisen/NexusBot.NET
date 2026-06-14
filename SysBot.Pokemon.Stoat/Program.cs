using System;
using System.Reflection;
using System.Linq;

class Program {
    static void Main() {
        var asm = Assembly.Load("StoatSharp");
        var clientType = asm.GetType("StoatSharp.StoatClient");
        foreach(var ctor in clientType.GetConstructors()) {
            Console.WriteLine("StoatClient constructor: " + string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)));
        }
        var restType = asm.GetType("StoatSharp.Rest.StoatRestClient") ?? asm.GetType("StoatSharp.StoatRestClient");
        if (restType != null) {
            foreach(var method in restType.GetMethods().Where(m => m.Name.Contains("DM") || m.Name.Contains("Message"))) {
                Console.WriteLine("StoatRestClient method: " + method.Name);
            }
        }
    }
}
