using System;
using System.IO;
using System.Linq;

namespace ExtTS
{
    public class TypeScript
    {
        public static void Generate(string docPath, string tsPath, string version, string toolkit)
        {
            var outputPath = Path.Combine(docPath, "output");
            if (!Directory.Exists(outputPath))
                throw new DirectoryNotFoundException("Directory not found: " + outputPath);
            if (!Directory.EnumerateFiles(outputPath, "*.js", SearchOption.TopDirectoryOnly).Any())
                throw new FileNotFoundException("File not found: " + Path.Combine(outputPath, "*.js"));

            var sourcePath = Path.Combine(docPath, "source");
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException("Directory not found: " + sourcePath);
            if (!Directory.EnumerateFiles(sourcePath, "*.html", SearchOption.TopDirectoryOnly).Any())
                throw new FileNotFoundException("File not found: " + Path.Combine(sourcePath, "*.html"));

            try
            {
                jsduck.JsDoc.UnknownExtTypes.Clear();
                Console.WriteLine($"GENERATING:\n    - From OUTPUT: {outputPath}\\*.js\n    - From SOURCE: {sourcePath}\\*.html");
                var tsFile = new model.Builder(outputPath, sourcePath, version).Build();
                if (tsFile == null)
                    Console.WriteLine($@"Modules: not found");
                else
                {
                    Console.WriteLine($@"Modules: {tsFile.Classes.Count}");
                    var tmp = tsPath + ".~tmp";
                    try
                    {
                        tsFile.Write(tmp);
                        if (!File.Exists(tsPath))
                            File.Move(tmp, tsPath);
                        else if (!model.Builder.IsSameTS(tmp, tsPath))
                        {
                            File.Delete(tsPath);
                            File.Move(tmp, tsPath);
                        }
                        else
                            Console.WriteLine($@"WARNING: {tsPath} has not changed");
                    }
                    finally
                    {
                        if (File.Exists(tmp))
                            File.Delete(tmp);
                    }
                    Console.WriteLine($@"GENERATED: {tsPath}");
                }
                if (jsduck.JsDoc.UnknownExtTypes.Count > 0)
                    Console.WriteLine($@"UNKNOWN TYPES[{jsduck.JsDoc.UnknownExtTypes.Count}]: {String.Join(", ", jsduck.JsDoc.UnknownExtTypes)}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"GENERATE ERROR: {tsPath}. {ex}");
            }
            Console.WriteLine();
        }
    }
}
