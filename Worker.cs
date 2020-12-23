using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartScheduler
{
    public class Worker
    {
        public Worker(string _name)
        {
            WName = _name;
        }
        public string WName { get; set; }
        public ObservableCollection<string> DayHoursC { get; set; } = new ObservableCollection<string> { "", "", "", "", "", "", "" };

        public DateTime[,] DayHours = new DateTime[7,2];
        public DateTime NextWorkingDay { get; set; }
        
        public void ResetHours()
        {
            DayHours = new DateTime[7, 2];
            DayHoursC = new ObservableCollection<string> { "", "", "", "", "", "", "" };
        }

        public void SetCHours()
        {
            for (var i = 0; i<7; i++)
            {
                if (CheckIfBlank(DayHours[i,0]) || CheckIfBlank(DayHours[i, 1]))
                {
                    DayHoursC[i] = "";
                }
                else
                {
                    DayHoursC[i] = DayHours[i, 0].ToString("h:mm tt") + " - \n" + DayHours[i, 1].ToString("h:mm tt");
                }
            }
        }

        public bool CheckIfBlank(DateTime dt)
        {
            if (dt == new DateTime())
            {
                return true;
            }
            return false;
        }

    }
}
