using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Todo;

namespace Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var channel = new Channel("localhost:5050", ChannelCredentials.Insecure);
            await channel.ConnectAsync().ContinueWith(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("Client Channel Started");
            });

            //await Add(channel);
            //await Find(channel);
            //await ListAll(channel);
            //await Delete(channel);
            await Update(channel);


            Console.ReadKey();
        }

        private static async Task Update(ChannelBase channel)
        {
            try
            {
                var client = new TodoItemService.TodoItemServiceClient(channel);

                var response = await client.UpdateAsync(new UpdateItemRequest
                {
                    TodoItem = new TodoItem
                    {
                        Id = "6057b2f48f449c5c71bc5ed2",
                        Name = "grpc updated 2",
                        Deadline = DateTime.UtcNow.ToTimestamp()
                    }
                });

                Console.WriteLine(response.IsUpdated ? "Updated" : "Could not updated");
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private static async Task Delete(ChannelBase channel)
        {
            try
            {
                var client = new TodoItemService.TodoItemServiceClient(channel);

                var response = await client.DeleteAsync(new DeleteItemRequest
                {
                    TodoId = "6057b28e6c70ab6109292079"
                });

                Console.WriteLine(response.IsDeleted ? "Deleted" : "Could not deleted");
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private static async Task ListAll(ChannelBase channel)
        {
            try
            {
                var client = new TodoItemService.TodoItemServiceClient(channel);

                var response = await client.ListAllAsync(new ListAllItemRequest());

                foreach (var item in response.AllTodoItems) Console.WriteLine(item);
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Status.Detail);
            }
        }

        private static async Task Find(ChannelBase channel)
        {
            try
            {
                var client = new TodoItemService.TodoItemServiceClient(channel);

                var response = await client.FindAsync(new FindTodoItemRequest
                {
                    TodoId = "6057b2f48f449c5c71bc5ed2"
                });

                Console.WriteLine("Found Todo Item : " + response);
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }
        }

        private static async Task Add(ChannelBase channel)
        {
            var client = new TodoItemService.TodoItemServiceClient(channel);

            var response = await client.AddAsync(new AddTodoItemRequest
            {
                TodoItem = new TodoItem
                {
                    Name = "gRPC",
                    Deadline = DateTime.UtcNow.ToTimestamp()
                }
            });

            Console.WriteLine("Todo Item Added " + response);
        }
    }
}