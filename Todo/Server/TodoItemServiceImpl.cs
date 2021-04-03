using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using Todo;

namespace Server
{
    public class TodoItemServiceImpl : TodoItemService.TodoItemServiceBase
    {
        private readonly MongoClient _client = new MongoClient("mongodb://localhost:27017");
        private readonly IMongoCollection<BsonDocument> _itemDb;

        public TodoItemServiceImpl()
        {
            var database = _client.GetDatabase("tododb");
            _itemDb = database.GetCollection<BsonDocument>("item");
        }

        public override async Task<AddTodoItemResponse> Add(AddTodoItemRequest request, ServerCallContext context)
        {
            var doc = new BsonDocument
            {
                {"name", request.TodoItem.Name},
                {"deadline", request.TodoItem.Deadline.ToDateTime()}
            };

            await _itemDb.InsertOneAsync(doc);

            var id = doc.GetValue("_id").ToString();
            request.TodoItem.Id = id;

            Console.WriteLine("Request Added. " + request);

            return await Task.FromResult(new AddTodoItemResponse
            {
                TodoItem = request.TodoItem,
                IsSuccess = !string.IsNullOrWhiteSpace(id)
            });
        }

        public override async Task<FindTodoItemResponse> Find(FindTodoItemRequest request, ServerCallContext context)
        {
            var todoId = request.TodoId;

            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(todoId));

            var doc = await _itemDb.FindAsync(filter).Result.FirstOrDefaultAsync();
            if (doc == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Item not found " + request.TodoId));

            var todoItem = new TodoItem
            {
                Id = doc.GetValue("_id").ToString(),
                Name = doc.GetValue("name").ToString(),
                Deadline = doc.GetValue("deadline").ToUniversalTime().ToTimestamp()
            };

            return await Task.FromResult(new FindTodoItemResponse
            {
                TodoItem = todoItem
            });
        }


        public override async Task<ListAllItemResponse> ListAll(ListAllItemRequest request, ServerCallContext context)
        {
            var result = new ListAllItemResponse();

            var items = await _itemDb.FindAsync(Builders<BsonDocument>.Filter.Empty).Result.ToListAsync();

            foreach (var item in items)
                result.AllTodoItems.Add(new TodoItem
                {
                    Id = item.GetValue("_id").ToString(),
                    Name = item.GetValue("name").ToString(),
                    Deadline = item.GetValue("deadline").ToUniversalTime().ToTimestamp()
                });

            return await Task.FromResult(result);
        }

        public override async Task<DeleteItemResponse> Delete(DeleteItemRequest request, ServerCallContext context)
        {
            var todoId = request.TodoId;
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(todoId));
            var deleteResult = await _itemDb.DeleteOneAsync(filter);

            return await Task.FromResult(new DeleteItemResponse
            {
                IsDeleted = deleteResult.DeletedCount > 0
            });
        }

        public override async Task<UpdateItemResponse> Update(UpdateItemRequest request, ServerCallContext context)
        {
            var todoItem = request.TodoItem;

            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(todoItem.Id));

            var update = Builders<BsonDocument>.Update.Set("name", todoItem.Name)
                .Set("deadline", todoItem.Deadline.ToDateTime());

            var updateResult = await _itemDb.UpdateOneAsync(filter, update);

            return await Task.FromResult(new UpdateItemResponse
            {
                IsUpdated = updateResult.ModifiedCount > 0
            });
        }
    }
}