using TMPlaywrightInstaller;

Console.WriteLine("Hello, World!");
try
{
    var code = PlaywrightLauncher.Execute(args);
    Console.WriteLine($"Done, result: {code}");
} catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}