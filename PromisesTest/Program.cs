using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PromisesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Promise.Create((resolve, reject) =>
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] starting");
                resolve();
            })
            .Then((resolve, reject) =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] done1");
                    resolve();
                });
            })
            .Then(() =>
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] done2");
            })
            .Then((resolve, reject) =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] done3");
                    resolve();
                });
            })
            .Then(() =>
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] done4");
            });

            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] what are you waiting for?");
            Console.ReadLine();
        }
    }
}
