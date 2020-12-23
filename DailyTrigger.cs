using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartScheduler
{
    public class DailyTrigger
    {
        readonly TimeSpan triggerHour;
        private CancellationTokenSource source = new CancellationTokenSource();

        public DailyTrigger(int hour, int minute = 0, int second = 0)
        {
            triggerHour = new TimeSpan(hour, minute, second);
            InitiateAsync();
        }

        public DailyTrigger(TimeSpan ts)
        {
            triggerHour = ts;
            InitiateAsync();
        }

        public void Repeat()
        {
            InitiateAsync();
        }

        public void CancelTask()
        {
            source.Cancel();
        }

        async void InitiateAsync()
        {
            while (!source.IsCancellationRequested)
            {
                try
                {
                    var triggerTime = DateTime.Today + triggerHour - DateTime.Now;
                    if (triggerTime < TimeSpan.Zero)
                        triggerTime = triggerTime.Add(new TimeSpan(24, 0, 0));
                    await Task.Delay(triggerTime, source.Token);
                    OnTimeTriggered?.Invoke();
                }
                catch (TaskCanceledException) when (source.Token.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
            }
        }

        public event Action OnTimeTriggered;
    }
}
