using System.Reflection;

namespace TMPlaywrightInstaller;

public static class PlaywrightLauncher
{
    public static int? Execute(string[] args)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var dllPath = Path.Combine(baseDirectory, "Microsoft.Playwright.dll");

        Environment.SetEnvironmentVariable("PLAYWRIGHT_DRIVER_SEARCH_PATH", baseDirectory);

        var assemblyBytes = File.ReadAllBytes(dllPath);
        Assembly playwrightAssembly = Assembly.Load(assemblyBytes);

        Type programType = playwrightAssembly.GetType("Microsoft.Playwright.Program");
        MethodInfo mainMethod = programType?.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

        if (mainMethod == null)
            throw new Exception("The method Main not found in Microsoft.Playwright.dll");

        return (int?)mainMethod.Invoke(null, new object[] { args });
    }
}