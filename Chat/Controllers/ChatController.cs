using System.Net.Sockets;
using Chat.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;

        public ChatController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index(string receiver)
        {
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser))
                return RedirectToAction("Login", "Account");

            var users = _db.Users.ToList();


            List<ChatMessage> messages = new List<ChatMessage>();
            if (!string.IsNullOrEmpty(receiver))
            {
                messages = _db.ChatMessages
                    .Where(m => (m.Sender == currentUser && m.Receiver == receiver) ||
                                (m.Sender == receiver && m.Receiver == currentUser))
                    .OrderBy(m => m.Timestamp)
                    .ToList();
            }

            ViewBag.CurrentUser = currentUser;
            ViewBag.Users = users;
            ViewBag.Receiver = receiver;

            return View(messages);
        }

        [HttpPost]
        public IActionResult DeleteUserMessages([FromBody] string username)
        {
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser)) return Unauthorized();

            var messages = _db.ChatMessages
                .Where(m => (m.Sender == currentUser && m.Receiver == username) ||
                            (m.Sender == username && m.Receiver == currentUser))
                .ToList();

            if (messages.Any())
            {
                _db.ChatMessages.RemoveRange(messages);
                _db.SaveChanges();
            }

            return Ok();
        }

        //[HttpPost("SendMessage")]
        //public IActionResult SendMessage(string receiver, string text)
        //{
        //    var sender = HttpContext.Session.GetString("Username");
        //    if (string.IsNullOrEmpty(sender))
        //        return RedirectToAction("Index");
        //    var message = new ChatMessage
        //    {
        //        Sender = sender,
        //        Receiver = receiver,
        //        Text = text,
        //        Timestamp = DateTime.Now
        //    };
        //    _db.ChatMessages.Add(message);
        //    _db.SaveChanges();

        //    return RedirectToAction("Index", new { receiver = receiver });
        //}

        //public IActionResult GetMessage(string receiver)
        //{
        //    var currentuser = HttpContext.Session.GetString("Username");    
        //    if (string.IsNullOrEmpty(currentuser))
        //        return Unauthorized();

        //    var messages = _db.ChatMessages
        //        .Where(m =>
        //              (m.Sender == currentuser && m.Receiver == receiver) ||
        //              (m.Sender == receiver && m.Receiver == currentuser))
        //        .OrderBy(m => m.Timestamp)
        //        .ToList();

        //    return PartialView("_MessagesPartial", messages);
        //}
    }
}

