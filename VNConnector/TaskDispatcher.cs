using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace VNConnector
{
    /// <summary>
    /// Позволяет управлять запущенными задачами:
    /// -- Закрывает задачи по запросу.
    /// -- Препядствует повторному запуску задачи.
    /// </summary>
    internal class TaskDispatcher
    {
        private Dictionary<string, List<Thread>> threads;

        private Dictionary<string, List<Task>> tasks;

        public TaskDispatcher() 
        {
            threads = new Dictionary<string,List<Thread>>();
            tasks = new Dictionary<string,List<Task>>();
        }

        private void Clear(List<Thread> thread_list)
        {
            thread_list.RemoveAll(thread => thread.ThreadState == ThreadState.Stopped);
        }

        private bool ThreadsStoped(List<Thread> threads)
        {
            return threads.Where(thread => thread.ThreadState != ThreadState.Stopped).Count() == 0;
        }

        public Thread GetThread(string ThreadName)
        {
            List<Thread> threads_list;
            if (threads.TryGetValue(ThreadName, out threads_list))
            {
                return threads_list.Last();
            }
            else return null;
        }

        public List<Thread> GetThreadList(string ThreadName)
        {
            List<Thread> threads_list;
            if (threads.TryGetValue(ThreadName, out threads_list)) return threads_list;
            else return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thread">поток</param>
        /// <param name="ThreadName">имя потока</param>
        /// <param name="CollosionAction">действие при коллизии имен</param>
        public void AddThread(Thread thread, string ThreadName, ThreadCollosionActions CollosionAction = ThreadCollosionActions.RESTRICT)
        {
            List<Thread> thread_list;
            bool threadExists = threads.TryGetValue(ThreadName, out thread_list);
            if (threadExists)
                if (threads[ThreadName]?.Count == 0 || ThreadsStoped(thread_list))
                {
                    threads[ThreadName] = new List<Thread>() { thread };
                }
                else
                {
                    switch (CollosionAction)
                    {
                        case ThreadCollosionActions.APPEND:
                            threads[ThreadName].Add(thread);
                            break;
                        case ThreadCollosionActions.REPLASE:
                            threads[ThreadName] = new List<Thread>() { thread };
                            break;
                        case ThreadCollosionActions.RESTRICT:
                            throw new ThreadExistsException();
                    }
                }
            else threads[ThreadName] = new List<Thread>() { thread };
        }

        public void StartThread(string ThreadName)
        {
            foreach (Thread thread in threads[ThreadName])
            {
                thread.Start();
            }
        }

        /// <summary>
        /// Завершает работу потока. Если в списке несколько потоков проверяется параметр CloseAll.
        ///     CloseAll == false - (по умолчанию) закрывает первый поток скиска
        ///     CloseAll == true - закрывает все потоки списка
        /// </summary>
        /// <param name="ThreadName">Имя потока</param>
        /// <param name="CloseAll">Закрывать все потоки, если Count > 1</param>
        public void CloseThread(string ThreadName, bool CloseAll = false)
        {
            if (threads[ThreadName] == null) { throw new ThreadDoesNotExistsException(); }
            switch (threads[ThreadName].Count)
            {
                case 0:
                    threads[ThreadName] = null;
                    break;
                case 1:
                    threads[ThreadName][0].Abort();
                    threads[ThreadName] = null;
                    break;
                default:
                    if (CloseAll)
                    {
                        foreach (Thread thread in threads[ThreadName])
                        {
                            thread.Abort();
                            threads[ThreadName] = null;
                        }
                    }
                    else
                    {
                        threads[ThreadName].First().Abort();
                    }
                    break;
            }
        }

        //to Tusk

        private bool TasksCompleted(List<Task> tasks_list)
        {
            return tasks_list.Where(task => task.IsCompleted).Count() == tasks_list.Count();
        }

        public bool TasksCompleted(string TaskName)
        {
            return tasks[TaskName].Where(task => task.IsCompleted).Count() == tasks[TaskName].Count();
        }

        public bool TasksExists(string TaskName)
        {
            return tasks.Keys.Contains(TaskName);
        }

        public void DeleteCompleted(string TaskName)
        {
            tasks[TaskName].RemoveAll(task => task.IsCompleted);
        }

        public void AddTask(Task task, string TaskName, ThreadCollosionActions CollosionAction = ThreadCollosionActions.RESTRICT)
        {
            List<Task> tasks_list;
            bool taskExists = tasks.TryGetValue(TaskName, out tasks_list);
            if (taskExists)
                if (tasks[TaskName]?.Count == 0 || TasksCompleted(tasks_list))
                {
                    tasks[TaskName] = new List<Task>() { task };
                }
                else
                {
                    switch (CollosionAction)
                    {
                        case ThreadCollosionActions.APPEND:
                            tasks[TaskName].Add(task);
                            break;
                        case ThreadCollosionActions.REPLASE:
                            tasks[TaskName] = new List<Task>() { task };
                            break;
                        case ThreadCollosionActions.RESTRICT:
                            throw new ThreadExistsException();
                    }
                }
            else tasks[TaskName] = new List<Task>() { task };
        }

        public void AddTask(Action action, string TaskName, ThreadCollosionActions CollosionAction = ThreadCollosionActions.RESTRICT)
        {
            AddTask(new Task(action), TaskName, CollosionAction);
        }

        public void Run(Task task, string TaskName)
        {
            AddTask(task, TaskName);
            task.Start();
        }

        public void Run(Action action, string TaskName)
        {
            AddTask(Task.Run(action), TaskName);
        }

        public void Start(string TaskName)
        {
            tasks[TaskName].Last().Start();
        }

        public void StartAll(string TaskName)
        {
            foreach (Task task in tasks[TaskName]) { task.Start(); }
        }

        public void Wait(string TaskName)
        {
            foreach (Task task in tasks[TaskName])
            {
                if (!task.IsCompleted) { task.Wait(); }
            }
        }

        public void CloseAll()
        {
            foreach (List<Thread> thread_list in threads.Values)
            {
                foreach (Thread thread in thread_list)
                {
                    thread.Abort();
                }
            }
            foreach (List<Task> task_list in tasks.Values)
            {
                foreach (Task task in task_list)
                {
                    task.Dispose();
                }
            }
            threads = new Dictionary<string, List<Thread>>();
        }
    }
}
