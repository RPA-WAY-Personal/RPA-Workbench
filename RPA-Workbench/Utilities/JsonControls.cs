using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace RPA_Workbench.Utilities
{
    public class JsonControls
    {
        private string json;
        private dynamic jsonObj;
        private string InnerJsonFilePath;
        /// <summary>
        /// Step 1 To read a File
        /// </summary>
        /// <param name="JsonFilePath"></param>
        public void ReadJsonFile(String JsonFilePath)
        {
            try
            {
                json = File.ReadAllText(JsonFilePath);
                InnerJsonFilePath = JsonFilePath;
            }
            catch (Exception)
            {

            }
           
        }

        /// <summary>
        /// Step 2 Deserialize The Json Object to ge the "Keys"
        /// </summary>
        public void DeserializeJsonObject()
        {
            try
            {
                jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            }
            catch (Exception)
            {
            }
          
        }

        /// <summary>
        /// Change a key in the json File
        /// </summary>
        public void ChangeKeyString(string Key, string ToValue)
        {
            JToken token = JObject.Parse(File.ReadAllText(@InnerJsonFilePath));
            jsonObj[Key] = ToValue;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(InnerJsonFilePath, output);
        }


        public string GetKeyValue(string Key)
        {
            return jsonObj[Key];
        }

        public JArray GetKeyValues(string Key,string value)
        {
            JArray ja = new JArray();
            JObject jo = new JObject();
           return SetKeyValues(Key, value);
        }

        public JArray SetKeyValues(string Key, string value)
        {
            JArray ja = new JArray();
            JObject jo = new JObject();
            jo.Add(Key, value);
            ja.Add(jo);

            return ja;
        }

        public void CreateNewJsonFile(string FilePath, string FileName, string Key, string[] Value)
        {
            JObject JsonFile = new JObject();

            JsonFile = new JObject
            (
             new JProperty(Key, Value)
            );

          
            File.WriteAllText(FilePath +"\\" + FileName+".json", JsonFile.ToString());
        }

        public void CreateNewJsonFile(string FilePath, string FileName, string Key, string Value)
        {
            JObject JsonFile = new JObject();

            JsonFile = new JObject
            (
             new JProperty(Key, Value)
            );


            File.WriteAllText(FilePath + "\\" + FileName + ".json", JsonFile.ToString());
        }

    }
}
