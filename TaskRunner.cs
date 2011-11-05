using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Threading;

namespace qserver
{
    class TaskRunner
    {
        private delegate void TaskRunnerThreadDelegate();

        public string PID { get; private set; }

        public bool IsRunning {get;private set;}

        public TaskRunner()
        {
            PID = Guid.NewGuid().ToString();
        }

        public void Start()
        {
            IsRunning = true;
            TaskRunnerFunction();
        }

        public static void GenerateRandomtTasks()
        {
            MongoServer server = MongoServer.Create(Util.MainConnectionString);
            MongoDatabase tasksdb = server.GetDatabase("tasks");
            MongoCollection<BsonDocument> tasks = tasksdb.GetCollection<BsonDocument>("tasks");

            tasks.Remove(Query.Null);

            for (int i = 0; i < 24; ++i)
            {
                BsonDocument task = new BsonDocument {
                        { "time_to_run", new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month,DateTime.MinValue.Day, i, i, i) },
                        { "pid", "0" }
                    };
                tasks.Insert(task);
            }
        }

        public static void UpdateRandomtTask(int rnum)
        {
            try
            {
                MongoServer server = MongoServer.Create(Util.MainConnectionString);
                MongoDatabase tasksdb = server.GetDatabase("tasks");

                MongoCollection<BsonDocument> tasks = tasksdb.GetCollection<BsonDocument>("tasks");
                var query = Query.And(
                    Query.EQ("pid", "0")
                );

                var sortBy = SortBy.Descending("time_to_run");

                var updatReserve = Update
                    .Set("time_to_run", new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, rnum, rnum, rnum));

                var result = tasks.FindAndModify(
                    query,
                    sortBy,
                    updatReserve,
                    true
                );
                var task = result.ModifiedDocument;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void RunTheTask(DateTime timeToRun)
        {
            Console.Write("Time to Run: " + timeToRun.ToString() + " Processed by PID: " + PID + "\n");
        }

        private void TaskRunnerThreadFunction()
        {
            try
            {
                MongoServer server = MongoServer.Create(Util.MainConnectionString);
                MongoDatabase tasksdb = server.GetDatabase("tasks");

                MongoCollection<BsonDocument> tasks = tasksdb.GetCollection<BsonDocument>("tasks");
                var queryReserve = Query.And(
                    Query.EQ("pid", "0"),
                    Query.LT("time_to_run", DateTime.UtcNow)
                );

                var sortBy = SortBy.Descending("time_to_run");

                var updatReserve = Update
                    .Set("pid", PID);

                var result = tasks.FindAndModify(
                    queryReserve,
                    sortBy,
                    updatReserve,
                    true
                );
                var task = result.ModifiedDocument;

                if (task != null)
                {
                    BsonValue timeToRun;
                    task.TryGetValue("time_to_run", out timeToRun);

                    RunTheTask(timeToRun.AsDateTime);

                    DateTime newTimeToRunUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timeToRun.AsDateTime.Hour, timeToRun.AsDateTime.Minute, timeToRun.AsDateTime.Second);

                    newTimeToRunUtc.AddDays(1);

                    var queryRelease = Query.And(
                        Query.EQ("pid", PID)
                    );

                    var updatRelease = Update
                        .Set("pid", "0")
                        .Set("time_to_run", newTimeToRunUtc);

                    result = tasks.FindAndModify(
                        queryRelease,
                        sortBy,
                        updatRelease,
                        true
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void TaskRunnerFunction()
        {
            TaskRunnerThreadDelegate myd = new TaskRunnerThreadDelegate(TaskRunnerThreadFunction);

            AsyncCallback completedCallback = new AsyncCallback(TaskCompletedCallback);

            AsyncOperation async = AsyncOperationManager.CreateOperation(null);
            myd.BeginInvoke(completedCallback, async);
        }

        protected void TaskCompletedCallback(IAsyncResult ar)
        {
            TaskRunnerThreadDelegate myd = (TaskRunnerThreadDelegate)((AsyncResult)ar).AsyncDelegate;

            AsyncOperation async = (AsyncOperation)ar.AsyncState;

            // finish the asynchronous operation
            myd.EndInvoke(ar);

            // raise the completed event
            AsyncCompletedEventArgs ace = new AsyncCompletedEventArgs(null, false, null);
            async.PostOperationCompleted(delegate(object e) { OnTaskCompleted((AsyncCompletedEventArgs)e); }, ace);

        }

        private void OnTaskCompleted(AsyncCompletedEventArgs e)
        {
            IsRunning = false;
        }

    }
}
