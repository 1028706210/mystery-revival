using System;
using System.IO;
using System.Reflection;

// Metadata-only check of the real compiled module. No Unity object is created.
internal static class CheckContract
{
    private static int checks;

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2) throw new ArgumentException("Expected module DLL and Unity managed directory");
            AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
            {
                var path = Path.Combine(args[1], new AssemblyName(request.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
            var module = Assembly.LoadFrom(args[0]);
            var factory = module.GetType("MysteryRevival.Hybrid25D.HybridWorldFactory", true);
            var create = factory.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            Require(create != null, "Missing public static factory Create");
            Require(create.ReturnType.FullName == "UnityEngine.GameObject", "Factory return type mismatch");
            var parameters = create.GetParameters();
            Require(parameters.Length == 1 && parameters[0].ParameterType.FullName == "UnityEngine.Transform",
                "Factory parent parameter mismatch");

            var door = module.GetType("MysteryRevival.Hybrid25D.HybridDoor", true);
            Require(door.BaseType.FullName == "UnityEngine.MonoBehaviour", "Door must be a component");
            var setOpen = door.GetMethod("SetOpen", BindingFlags.Public | BindingFlags.Instance);
            Require(setOpen != null && setOpen.ReturnType == typeof(void), "Missing door SetOpen");
            parameters = setOpen.GetParameters();
            Require(parameters.Length == 1 && parameters[0].ParameterType == typeof(bool),
                "SetOpen must accept one bool");
            var interactor = door.GetField("interactor", BindingFlags.Public | BindingFlags.Instance);
            Require(interactor != null && interactor.FieldType.FullName == "UnityEngine.Transform",
                "Door must expose public Transform interactor");
            var interact = door.GetMethod("TryInteract", BindingFlags.Public | BindingFlags.Instance);
            Require(interact != null && interact.ReturnType == typeof(bool) && interact.GetParameters().Length == 0,
                "Door must expose public bool TryInteract()");

            var occluder = module.GetType("MysteryRevival.Hybrid25D.HybridForegroundOccluder", true);
            Require(occluder.BaseType.FullName == "UnityEngine.MonoBehaviour", "Occluder must be a component");
            var target = occluder.GetField("target", BindingFlags.Public | BindingFlags.Instance);
            Require(target != null && target.FieldType.FullName == "UnityEngine.Transform",
                "Occluder must expose public Transform target");
            Console.WriteLine("CONTRACT_PASS checks=" + checks);
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("CONTRACT_FAIL " + error);
            return 1;
        }
    }
}
