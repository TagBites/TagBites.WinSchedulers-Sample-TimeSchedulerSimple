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
        private readonly ResourceModel[] _resources;
        private readonly IDictionary<DateTime, IList<TaskModel>> _tasks = new Dictionary<DateTime, IList<TaskModel>>();

        public SchedulerDataSource()
        {
            _resources = new []
            {
                new ResourceModel("Chapman Glenn"),
                new ResourceModel("Reyes Sally"),
                new ResourceModel("Torres Xavier"),
                new ResourceModel("Daniels Ava"),
                new ResourceModel("Reese Millie"),
                new ResourceModel("Adams Musa"),
                new ResourceModel("Morgan Roxanne"),
                new ResourceModel("Wolf Darren"),
                new ResourceModel("Walters Anna"),
                new ResourceModel("Bush Ryan"),
                new ResourceModel("Owens Maryam"),
                new ResourceModel("Whitehouse Evangeline"),
                new ResourceModel("Hammond Paige"),
                new ResourceModel("Solis Chris"),
                new ResourceModel("Rodgers Jacqueline"),
                new ResourceModel("Molina Otis"),
                new ResourceModel("Mendez Constance"),
                new ResourceModel("Lloyd Sienna"),
                new ResourceModel("Murray Abby"),
                new ResourceModel("Lawrence Tyler")
            }; 
        }


        protected override TimeSchedulerResourceDescriptor CreateResourceDescriptor()
        {
            return new TimeSchedulerResourceDescriptor(typeof(object));
        }
        protected override TimeSchedulerTaskDescriptor CreateTaskDescriptor()
        {
            return new TimeSchedulerTaskDescriptor(typeof(TaskModel), nameof(TaskModel.Resource), nameof(TaskModel.Interval));
        }

        public override IList<object> LoadResources() => _resources.Cast<object>().ToList();
        public override void LoadContent(TimeSchedulerDataSourceView view)
        {
            var interval = view.Interval;
            var resources = view.Resources.Cast<ResourceModel>().ToList();

            IList<TaskModel> GetTaskForDate(DateTime date)
            {
                if (!_tasks.ContainsKey(date))
                    _tasks.Add(date.Date, GenerateTasks(date).ToList());

                return _tasks[date];
            };

            foreach (var item in resources)
                view.AddWorkTime(item, interval, Colors.White);

            for (var i = interval.Start.Date; i <= interval.End.Date; i = i.AddDays(1))
            {
                var date = i.Date;
                var tasks = GetTaskForDate(date);
                var loadedTasks = tasks.Where(x => interval.IntersectsWith(x.Interval) && view.Resources.Contains(x.Resource))
                    .OrderBy(x => x.Interval.Start).ThenBy(x => x.Interval.End).ToList();

                for (var j = 0; j < loadedTasks.Count; j++)
                {
                    var task = loadedTasks[j];
                    view.AddTask(task);
                    // Connections
                    var nextTask = tasks.FirstOrDefault(x => x.Resource.ID == task.Resource.ID + 2 && x.Interval.Start > task.Interval.End)
                                   ?? GetTaskForDate(i.AddDays(1).Date).FirstOrDefault(x => x.Resource.ID == task.Resource.ID + 2 && x.Interval.Start > task.Interval.End);
                    if (nextTask != null)
                        view.AddConnection(task, nextTask, true, Colors.DarkOrange);
                }
            } 
        }

        #region Data generation

        private readonly Random m_random = new Random();
        public IEnumerable<TaskModel> GenerateTasks(DateTime dateTime)
        {
            for (var k = 0; k < _resources.Length; k++)
            {
                var resource = _resources[k];
                var count = m_random.Next(2);
                for (var j = 0; j < count; j++)
                {
                    var minutes = m_random.Next(12 * 60);
                    var length = m_random.Next(2 * 60,  11 * 60);

                    yield return new TaskModel()
                    {
                        Resource = resource,
                        Interval = new DateTimeInterval(dateTime.AddMinutes(minutes), new TimeSpan(0, length, 0))
                    };
                }
            }
        }

        #endregion

        #region Classes

        public class ResourceModel
        {
            private static int m_id = 0;
            public int ID { get; }
            public string Name { get; }

            public ResourceModel(string name)
            {
                ID = m_id++;
                Name = name;
            }

            public override string ToString() => Name;
        }
        public class TaskModel
        {
            public ResourceModel Resource { get; set; }
            public DateTimeInterval Interval { get; set; }

            public override string ToString()
            {
                return $"Task interval {Interval}";
            }
        }

        #endregion
    }
}
