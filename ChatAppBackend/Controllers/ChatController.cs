using Api.DataAccess.Data;
using Api.Models;
using Api.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Newtonsoft.Json;

namespace ChatAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ChatServices _services;
        public ChatController(ChatServices services,AppDbContext context)
        {
            _services = services;
            _context = context;
        }

        [HttpPost("register-user")]
        public IActionResult RegisterUser(User user)
         {
            if (_services.AddUserToList(user.Email))
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpPost("add-text")]
        public async Task<IActionResult> AddText(Messages msg)
        {
            await _context.messages.AddAsync(msg);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "Msg Saved!"
            });

        }

        [HttpGet("get-data")]
        public async Task<IActionResult> GetData()
        {

            //table.Rows.Find

            //foreach (var row in table.Rows)
            //{

            //    row["Sent"] = _context.messages.Count(m => m.Sender == row.Email);
            //    row["Received"] = _context.messages.Count(m => m.Receiver == user.Email);
            //}


            DataTable table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Email");
            table.Columns.Add("Sent");
            table.Columns.Add("Received");

            foreach(var user in _context.users)
            {
                var row = table.NewRow();
                row["Name"] = user.Name;
                row["Email"] = user.Email;

                table.Rows.Add(row);
            }


            foreach (DataRow row in table.Rows)
            {
                row["Sent"] = _context.messages.Count(m => m.Sender == row["Email"]);
                row["Received"] = _context.messages.Count(m => m.Receiver == row["Email"]);
            }

            var jsonString = JsonConvert.SerializeObject(table);

            return Ok(jsonString);
        }

    }
}
