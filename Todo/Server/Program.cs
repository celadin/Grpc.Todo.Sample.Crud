using System;
using Grpc.Core;
using Todo;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var server = new Grpc.Core.Server
            {
                Services =
                {
                    TodoItemService.BindService(new TodoItemServiceImpl())
                },
                Ports =
                {
                    new ServerPort("localhost", 5050, ServerCredentials.Insecure)
                }
            };

            server.Start();

            Console.WriteLine("Server Started!");
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
        }
    }
}