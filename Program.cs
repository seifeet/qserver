using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace qserver
{
    class Program
    {
        static void Main(string[] args)
        {
            int m_NumberOfThreads = 0;
            bool m_TestMode = false;

            try
            {
                m_NumberOfThreads = Convert.ToInt32(Util.NumberOfThreads);
            }
            catch
            {
                Console.WriteLine("Failed to start because NumberOfThreads parameter is not configured. Please check AppSettings.config file.");
            }

            if (m_NumberOfThreads <= 0) // create an array with random number of task runners
            {
                Random random = new Random();
                m_NumberOfThreads = random.Next(4, 50);
                m_TestMode = true;
            }

            List<TaskRunner> trs = new List<TaskRunner>();

            Console.WriteLine(m_NumberOfThreads + " task runners created");

            for (int i = 1; i <= m_NumberOfThreads; ++i)
            {
                trs.Add(new TaskRunner());
            }

            if (m_TestMode)
            {
                try
                {
                    TaskRunner.GenerateRandomtTasks();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            while (true)
            {
                foreach (TaskRunner tr in trs)
                {
                    if (!tr.IsRunning) tr.Start();
                }
                Thread.Sleep(1000); // check for new tasks every second

                // randomly update a random task to check if it will be run
                if (m_TestMode)
                {
                    Random random = new Random();
                    int randomNumber = random.Next(0, 10);

                    if (randomNumber > 7)
                    {
                        TaskRunner.UpdateRandomtTask(randomNumber);
                    }
                }
            }

        }
    }
}
