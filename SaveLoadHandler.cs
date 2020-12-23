using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using Windows.Storage;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace SmartScheduler
{
    class SaveLoadHandler
    {

        public async void SaveHandler(string saveString, dynamic target)
        {
            StorageFile sF = await ApplicationData.Current.LocalFolder.CreateFileAsync(saveString + ".json", CreationCollisionOption.ReplaceExisting);
            string json = JsonConvert.SerializeObject(target);
            Debug.WriteLine(json);
            await FileIO.WriteTextAsync(sF, json);
        }

        public dynamic LoadHandler(string loadString, char typeOfLoad)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            string fp = folder.Path + @"\" + loadString + ".json";
            if (File.Exists(fp))
            {
                var sf = File.ReadAllText(fp);
                switch (typeOfLoad)
                {
                    case 'w':
                        return new ObservableCollection<Worker>(JsonConvert.DeserializeObject<List<Worker>>(sf));
                    case 'p':
                        return new ObservableCollection<string>(JsonConvert.DeserializeObject<List<string>>(sf));
                    case 't':
                        return JsonConvert.DeserializeObject<TimeSpan>(sf);
                    case 'b':
                        return JsonConvert.DeserializeObject<bool>(sf);
                    case 'i':
                        return JsonConvert.DeserializeObject<int>(sf);
                    case 'd':
                        return JsonConvert.DeserializeObject<Dictionary<string, int>>(sf);
                }
            }
            else
            {
                Debug.WriteLine("File does not exist " + fp);
            }
            return null;
        }


    }
}
