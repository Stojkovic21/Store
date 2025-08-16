using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Operation.Buffer;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis; //dotnet add package NRedisStack

[ApiController]
[Route("cart")]
public class CartController : ControllerBase
{
    private readonly ConnectionMultiplexer muxer;
    private readonly IDatabase db;
    // public CartController()
    // {
    //     muxer = ConnectionMultiplexer.Connect(
    //         new ConfigurationOptions
    //         {
    //             EndPoints = { { "redis-17606.c90.us-east-1-3.ec2.redns.redis-cloud.com", 17606 } },
    //             User = "default",
    //             Password = "LgiWDuzQyyezm6euwuHE5moXxgxNMiFv"
    //         }
    //     );
    //     db = muxer.GetDatabase();
    // }
    // [HttpPost]
    // [Route("new")]
    // public ActionResult AddItemInCart([FromBody] CartModel cartItem)
    // {
    //     try
    //     {
    //         //var db = muxer.GetDatabase();
    //         JsonCommands json = db.JSON();
    //         var key = "Item" + cartItem.Id;
    //         string user = "User";
    //         var item = new Dictionary<string, object>
    //         {
    //             {"Id",cartItem.Id},
    //             {"Name", cartItem.Name},
    //             {"Price", cartItem.Price},
    //             {"Quantity", cartItem.Quantity}
    //         };
    //         if (!db.KeyExists(user))
    //         {
    //             json.Set(user,"$","{}");
    //         }
    //         json.Set(user, "$."+cartItem.Id, item);
    //         return Ok("Uspesno dodato u bazu");
    //     }
    //     catch (System.Exception ex)
    //     {
    //         return BadRequest("Doslo je do greske prilikom povezivanja na bazu\n"+ex);
    //     }
    // }
    // [HttpPatch]
    // [Route("inc/{itemId}/{inc}")]
    // public ActionResult IncrementCounter(string itemId, string inc)
    // {
    //     try
    //     {
    //         // var db = muxer.GetDatabase();
    //         JsonCommands json = db.JSON();
    //         json.NumIncrby("User", $"$.item{itemId}.Quantity", double.Parse(inc));

    //         return Ok("Increment successfully");
    //     }
    //     catch (System.Exception)
    //     {
    //         return BadRequest("Something went wrong");
    //     }
    // }
    // [HttpDelete]
    // [Route("Remove/{id}")]
    // public ActionResult RemoveItemFromCart(string id)
    // {
    //     try
    //     {
    //         // var db = muxer.GetDatabase();
    //         JsonCommands json = db.JSON();
    //         json.Del("User", $"$.item{id}");
    //         return Ok("Item is removed successfully");
    //     }
    //     catch (System.Exception)
    //     {
    //         return BadRequest("Something went wrong");
    //     }
    // }
    [HttpGet]
    [Route("get")]
    public ActionResult GetAllItemsFromCurt()
    {
        return Ok(new
        {
            item1=new { id = 1, name= "Pepsi", price= 135,quantity=2},
            item2=new { id = 2, name= "Sinalco", price= 120,quantity=1},
            item3=new { id = 3, name= "Smoki", price= 150,quantity=1},
        });
        // try
        // {
        //     var db = muxer.GetDatabase();
        //     var json = db.JSON();
        //     if (db.KeyExists("User"))
        //     {
        //         var result = db.Execute("JSON.GET", "User", "$").ToString();

        //          var itemWrapper = JsonSerializer.Deserialize<List<Dictionary<string, ItemSerializer>>>(result);
        //          var items = itemWrapper?[0];

        //         // foreach (var entry in items)
        //         // {
        //         //     Console.WriteLine($"{entry.Key}: {entry.Value.Name}, {entry.Value.Price} RSD x {entry.Value.Quantity}");
        //         // }
        //         return Ok(items);
        //     }
        //     return BadRequest("Key's not found");
        // }
        // catch (System.Exception)
        // {
        //     return BadRequest("Something went wrong");
        // }
    }
    // [HttpGet]
    // [Route("get/{id}")]
    // public ActionResult GetItemById(int id)
    // {
    //     try
    //     {
    //         // var db = muxer.GetDatabase();
    //         var json = db.JSON();
    //         if (db.KeyExists("User"))
    //         {
    //             var result = db.Execute("JSON.GET", "User", "$. "+id).ToString();
    //             var itemWrapper = JsonSerializer.Deserialize<List<ItemSerializer>>(result);
    //             return Ok(itemWrapper?[0]);
    //         }
    //         return BadRequest("Item does not exist");
    //     }
    //     catch (System.Exception)
    //     {
    //         throw;
    //     }
    // }
   private class ItemSerializer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
    }
    class Item
    {
        public ItemSerializer GetItem { get; set; }
    }
}

