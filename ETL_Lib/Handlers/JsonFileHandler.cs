using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using TextTwistEtl.Logging;

namespace TextTwistEtl.Handlers
{
    public static class JsonFileHandler
    {
        //Assumes each line of the file contains a full JSON record
        public static IList<T> ReadJsonFile<T>(string path, int? lines = null)
        {
            List<T> allLines = new List<T>();
            Log.Instance.Info($"Opening JSON {path}");

            var counter = 0;
            using (StreamReader jsonFile = new StreamReader(path))
            {
                bool end = true;
                while (end)
                {
                    Log.Instance.Trace($"Reading line {counter} from JSON {path}");
                    string line = jsonFile.ReadLine() ?? "";
                    var value = JsonConvert.DeserializeObject<T>(line);
                    if (value != null)
                    {
                        allLines.Add(value);
                    }

                    counter++;
                    if (lines.HasValue)
                    {
                        end = counter < lines.Value;
                    }
                    else
                    {
                        end = !jsonFile.EndOfStream;
                    }
                }
            }

            Log.Instance.Info($"Finishing reading JSON {path}");
            return allLines;
        }

        public static void WriteJsonFile<T>(IList<T> objects, string path)
        {
            Log.Instance.Info($"Writing JSON {path}");

            StringBuilder fileContent = new StringBuilder();

            foreach (T obj in objects)
            {
                fileContent.AppendLine(JsonConvert.SerializeObject(obj));
            }

            File.WriteAllText(path, fileContent.ToString(), Encoding.UTF8);

            Log.Instance.Info($"Finishing writing JSON {path}");
        }

        public static void WriteJsonFile<T>(T obj, string path)
        {
            Log.Instance.Info($"Writing JSON {path}");

            StringBuilder fileContent = new StringBuilder();
            
            fileContent.AppendLine(JsonConvert.SerializeObject(obj));

            File.WriteAllText(path, fileContent.ToString(), Encoding.UTF8);

            Log.Instance.Info($"Finishing writing JSON {path}");
        }

    }
}