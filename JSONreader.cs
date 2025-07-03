using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace discordbot.config
{
    internal class NewBaseType
    {
        public string Token { get; set; }  // removed 'required'
          public string Prefix { get; set; } // removed 'required'
    }

    internal class JSONreader : NewBaseType
    {
      

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONstructure? data = JsonConvert.DeserializeObject<JSONstructure>(json);
                if (data != null)
                {
                    this.Token = data.Token;
                    this.Prefix = data.Prefix;
                }
                else
                {
                    throw new JsonException("Failed to deserialize JSON data.");
                }
            }
        }
    }

    internal sealed class JSONstructure
    {
        public string Token { get; set; }  // removed 'required'
        public string Prefix { get; set; } // removed 'required'
    }
}