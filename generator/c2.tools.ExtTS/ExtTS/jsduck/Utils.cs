using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;

namespace ExtTS.jsduck
{
    static class Utils
    {
        public readonly static JsonSerializer Serializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
#if DEBUG
                Formatting = Newtonsoft.Json.Formatting.Indented,
#endif
            }
            );

        public static IEnumerable<Tuple<string, HtmlDocument>> FindHtmls(string folder)
        {
            var files = System.IO.Directory.EnumerateFiles(folder, "*.html").ToArray();
            Console.WriteLine($@": {files.Length} (documents)");
            return files.Select(p => Tuple.Create(p.Substring(folder.Length + 1), LoadHtml(p)));
        }

        private static HtmlDocument LoadHtml(string path)
        {
            var doc = new HtmlDocument();
            doc.Load(path);
            return doc;
        }

        public static IEnumerable<jsduck.Class> FindClasses(string folder, string processName)
        {
            var folderLength = folder.EndsWith(@"\") || folder.EndsWith(@"/") ? folder.Length : (folder.Length + 1);
            var files = Directory.EnumerateFiles(folder, "Ext*.js").ToArray();
            Console.WriteLine($@": {files.Length} (classes)");
            for (var i = 0; i < files.Length; i++)
            {
                Console.Write($@"{processName}-{i + 1}/{files.Length}. {files[i].Substring(folderLength)}");
                var content = File.ReadAllText(files[i]);
                var begin = content.IndexOf("({") + 1;
                using (var reader = new StringReader(content.Substring(begin, content.Length - begin - 2)))
                {
                    jsduck.Class newclass;
                    try
                    {
                        newclass = (jsduck.Class)Serializer.Deserialize(reader, typeof(jsduck.Class));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@": {ex.ToString().Replace(Environment.NewLine, "|")}");
                        newclass = null;
                    }
                    if (IsValid(newclass))
                    {
                        Console.WriteLine();
                        yield return newclass;
                    }
                    else
                    {
                        Console.WriteLine(" -- Invalid data, skipping.");
                    }
                }
            }
        }

        public static bool IsValid(jsduck.Class instance)
        {
            if (instance == null || instance.files == null ||
                instance.files.Count() < 1 || string.IsNullOrWhiteSpace(instance.files[0].filename) || string.IsNullOrWhiteSpace(instance.files[0].href))
            {
                return false;
            }

            return true;
        }

        public static T LoadWithCache<T>(string cachePath, Func<T> func)
        {
            if (File.Exists(cachePath))
            {
                try
                {
                    using (var stream = File.OpenText(cachePath))
                    using (var reader = new JsonTextReader(stream))
                    {
                        return Serializer.Deserialize<T>(reader);
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(cachePath);
                    Console.WriteLine($@"Error load from cache: {ex.ToString().Replace(Environment.NewLine, "|")}");
                }
            }
            var t = func();
            using (var stream = File.CreateText(cachePath))
            using (var writer = new JsonTextWriter(stream))
                Serializer.Serialize(writer, t);
            return t;
        }

        public static IEnumerable<T> Yield<T>() { yield break; }
        public static IEnumerable<T> Yield<T>(T t) { yield return t; }
        public static IEnumerable<T> Yield<T>(T t, T t1) { yield return t; yield return t1; }
        public static IEnumerable<T> Yield<T>(T t, T t1, T t2) { yield return t; yield return t1; yield return t2; }
        public static IEnumerable<T> Yield<T>(T t, T t1, T t2, T t3) { yield return t; yield return t1; yield return t2; yield return t3; }
        public static IEnumerable<T> Yield<T>(T t, T t1, T t2, T t3, T t4) { yield return t; yield return t1; yield return t2; yield return t3; yield return t4; }
        public static IEnumerable<T> Yield<T>(T t, T t1, T t2, T t3, T t4, T t5) { yield return t; yield return t1; yield return t2; yield return t3; yield return t4; yield return t5; }
        public static IEnumerable<T> Yield<T>(T t, T t1, T t2, T t3, T t4, T t5, T t6) { yield return t; yield return t1; yield return t2; yield return t3; yield return t4; yield return t5; yield return t6; }
        public static IEnumerable<T> Yield<T>(T t, T t1, T t2, T t3, T t4, T t5, T t6, T t7) { yield return t; yield return t1; yield return t2; yield return t3; yield return t4; yield return t5; yield return t6; yield return t7; }
    }
}
