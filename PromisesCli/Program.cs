using Promises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PromisesCli
{
    class Program
    {
        static void Main(string[] args)
        {
            Promises.Promise.Create(() =>
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] starting");
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
            })
            .Catch((ex)=>
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] error: {ex.Message}");
            });

            Promise.All(new Promise[]
            {
                Promise.Create(() =>
                {
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] starting task #1");
                })
                .Then((resolve, reject)=>
                {
                    Thread.Sleep(5000);
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] done task #1");
                    resolve();
                }),
                Promise.Create(() =>
                {
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] starting task #2");
                })
                .Then((resolve, reject)=>
                {
                    Thread.Sleep(5000);
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] done task #2");
                    resolve();
                })
            })
            .Then(() =>
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] done with all tasks");
            });

            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] what are you waiting for?");
            Console.ReadLine();
        }
    }
}
