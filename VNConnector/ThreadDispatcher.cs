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
    internal class ThreadDispatcher
    {
        private Dictionary<string, List<Thread>> threads;

        public ThreadDispatcher() 
        {
            threads = new Dictionary<string,List<Thread>>();
        }

        private void ClearStopped(List<Thread> thread_list)
        {
            thread_list.RemoveAll(thread => thread.ThreadState == ThreadState.Stopped);
        }

        private bool ThreadsStoped(List<Thread> threads)
        {
            if (threads == null) return false;
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

        public static void StartUIAction(UIElement uiElement, Action action)
        {
            uiElement.Dispatcher.Invoke(action);
        }

        public void Start(string ThreadName)
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

        public void CloseAll()
        {
            foreach (List<Thread> thread_list in threads.Values)
            {
                foreach (Thread thread in thread_list)
                {
                    thread.Abort();
                }
            }
            threads = new Dictionary<string, List<Thread>>();
        }
    }
}
