using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using TagBites.WinSchedulers;
using TagBites.WinSchedulers.Descriptors;

namespace TimeSchedulerSimple
{
    public class SchedulerDataSource : TimeSchedulerDataSource
    {
        private readonly object[] _resources;
        private readonly IDictionary<DateTime, IList<SchedulerTask>> _taskDictionary = new Dictionary<DateTime, IList<SchedulerTask>>();

        public SchedulerDataSource()
        {
            _resources = new object[] { "Chapman Glenn", "Reyes Sally", "Torres Xavier", "Daniels Ava", "Reese Millie", "Adams Musa", "Morgan Roxanne", "Wolf Darren", "Walters Anna", "Bush Ryan", "Owens Maryam", "Whitehouse Evangeline", "Hammond Paige", "Solis Chris", "Rodgers Jacqueline", "Molina Otis", "Mendez Constance", "Lloyd Sienna", "Murray Abby", "Lawrence Tyler" }; 
        }


        protected override TimeSchedulerResourceDescriptor CreateResourceDescriptor()
        {
            return new TimeSchedulerResourceDescriptor(typeof(object));
        }
        protected override TimeSchedulerTaskDescriptor CreateTaskDescriptor()
        {
            return new TimeSchedulerTaskDescriptor(typeof(SchedulerTask), nameof(SchedulerTask.Resource), nameof(SchedulerTask.Interval));
        }

        public override IList<object> LoadResources() => _resources;
        public override void LoadContent(TimeSchedulerDataSourceView view)
        {
            var interval = view.Interval;
            var resources = view.Resources;

            foreach (var item in resources)
                view.AddWorkTime(item, interval, Colors.White);

            for (var i = interval.Start.Date; i <= interval.End.Date; i = i.AddDays(1))
            {
                if(!_taskDictionary.ContainsKey(i))
                    _taskDictionary.Add(i, GenerateTasks(i).ToList());

                var tasks = _taskDictionary[i];
                var loadedTasks = tasks.Where(x => interval.IntersectsWith(x.Interval) && view.Resources.Contains(x.Resource))
                    .OrderBy(x => x.Interval.Start).ThenBy(x => x.Interval.End).ToList();

                for (var j = 0; j < loadedTasks.Count; j++)
                {
                    var task = loadedTasks[j];
                    view.AddTask(task);
                    if (j > 0)
                        view.AddConnection(loadedTasks[j - 1], task, false, Colors.DarkGray);
                }
            } 
        }

        #region Data generation

        private readonly Random m_random = new Random();
        public IEnumerable<SchedulerTask> GenerateTasks(DateTime dateTime)
        {
            for (var k = 0; k < _resources.Length; k++)
            {
                var element = _resources[k];
                var count = m_random.Next(2);
                for (var j = 0; j < count; j++)
                {
                    var minutes = m_random.Next(12 * 60);
                    var length = m_random.Next(2 * 60,  11 * 60);

                    yield return new SchedulerTask()
                    {
                        Resource = element,
                        Interval = new DateTimeInterval(dateTime.AddMinutes(minutes), new TimeSpan(0, length, 0))
                    };
                }
            }
        }

        #endregion

        #region Classes

        public class SchedulerTask
        {
            public object Resource { get; set; }
            public DateTimeInterval Interval { get; set; }

            public override string ToString()
            {
                return $"Task interval {Interval}";
            }
        }

        #endregion
    }
}
