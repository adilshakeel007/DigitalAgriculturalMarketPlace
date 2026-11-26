using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketPlace.Controllers
{
    [SessionAuthorize(UserRoles.MarketplaceMember)]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(
            ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Inbox));
        }
        [HttpGet]
        public async Task<IActionResult> Send(
    int receiverId,
    int? productId)
        {
            int? currentUserId =
                HttpContext.Session.GetInt32("UserId");

            if (currentUserId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            if (receiverId == currentUserId.Value)
            {
                return BadRequest();
            }

            User? receiver =
                await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == receiverId &&
                        u.IsActive
                    );

            if (receiver == null)
            {
                return NotFound();
            }

            Product? product = null;

            if (productId.HasValue)
            {
                product = await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == productId.Value
                    );

                if (product == null)
                {
                    return NotFound();
                }
            }
            string? currentUserType =
    HttpContext.Session.GetString("UserType");

            if (product != null &&
                currentUserType == UserRoles.Consumer &&
                product.FarmerId != receiver.Id)
            {
                return Forbid();
            }

            SendMessageViewModel model =
                new SendMessageViewModel
                {
                    ReceiverId = receiver.Id,
                    ReceiverName = receiver.Name,
                    ProductId = product?.Id,
                    ProductName = product?.Name
                };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(
    SendMessageViewModel model)
        {
            int? senderId =
                HttpContext.Session.GetInt32("UserId");

            if (senderId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            if (model.ReceiverId == senderId.Value)
            {
                ModelState.AddModelError(
                    "",
                    "You cannot send a message to yourself."
                );

                return View(model);
            }

            User? receiver =
                await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == model.ReceiverId &&
                        u.IsActive
                    );

            if (receiver == null)
            {
                return NotFound();
            }

            Product? product = null;

            if (model.ProductId.HasValue)
            {
                product = await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == model.ProductId.Value
                    );

                if (product == null)
                {
                    return NotFound();
                }
            }
            string? currentUserType =
    HttpContext.Session.GetString("UserType");

            if (product != null &&
                currentUserType == UserRoles.Consumer &&
                product.FarmerId != receiver.Id)
            {
                return Forbid();
            }

            // Refill display values
            model.ReceiverName = receiver.Name;
            model.ProductName = product?.Name;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Message message = new Message
            {
                SenderId = senderId.Value,
                ReceiverId = receiver.Id,
                ProductId = product?.Id,
                Content = model.Content.Trim(),
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);

            await _context.SaveChangesAsync();
            string senderName =
    HttpContext.Session.GetString("UserName")
    ?? "User";

            await CreateNotification(
                receiver.Id,
                "New Message",
                $"You received a new message from {senderName}.",
                "Message",
                message.Id
            );

            TempData["SuccessMessage"] =
                "Message sent successfully.";

            return RedirectToAction(
                "Conversation",
                new
                {
                    userId = receiver.Id,
                    productId = product?.Id
                }
            );
        }
        [HttpGet]
        public async Task<IActionResult> Conversation(
    int userId,
    int? productId)
        {
            int? currentUserId =
                HttpContext.Session.GetInt32("UserId");

            if (currentUserId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            if (userId == currentUserId.Value)
            {
                return BadRequest();
            }

            User? otherUser =
                await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == userId
                    );

            if (otherUser == null)
            {
                return NotFound();
            }

            var query = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Product)
                .Where(m =>
                    (
                        m.SenderId == currentUserId.Value &&
                        m.ReceiverId == userId
                    )
                    ||
                    (
                        m.SenderId == userId &&
                        m.ReceiverId == currentUserId.Value
                    )
                );

            if (productId.HasValue)
            {
                query = query.Where(m =>
                    m.ProductId == productId.Value
                );
            }

            var messages = await query
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Mark received messages as read
            foreach (var message in messages)
            {
                if (message.ReceiverId == currentUserId.Value &&
                    !message.IsRead)
                {
                    message.IsRead = true;
                }
            }

            await _context.SaveChangesAsync();

            ViewBag.OtherUserId = otherUser.Id;
            ViewBag.OtherUserName = otherUser.Name;
            ViewBag.ProductId = productId;

            if (productId.HasValue)
            {
                Product? product =
                    await _context.Products
                        .FirstOrDefaultAsync(p =>
                            p.Id == productId.Value
                        );

                ViewBag.ProductName = product?.Name;
            }

            return View(messages);
        }
        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            int? currentUserId =
                HttpContext.Session.GetInt32("UserId");

            if (currentUserId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Product)
                .Where(m =>
                    m.ReceiverId == currentUserId.Value ||
                    m.SenderId == currentUserId.Value
                )
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var conversations = messages
                .GroupBy(m => new
                {
                    OtherUserId = m.SenderId == currentUserId.Value
                        ? m.ReceiverId
                        : m.SenderId,
                    m.ProductId
                })
                .Select(group =>
                {
                    Message latest = group.First();
                    User? otherUser = latest.SenderId == currentUserId.Value
                        ? latest.Receiver
                        : latest.Sender;

                    return new InboxConversationViewModel
                    {
                        OtherUserId = group.Key.OtherUserId,
                        OtherUserName = otherUser?.Name ?? "User",
                        ProductId = group.Key.ProductId,
                        ProductName = latest.Product?.Name,
                        LastMessage = latest.Content,
                        LastMessageAt = latest.SentAt,
                        UnreadCount = group.Count(m =>
                            m.ReceiverId == currentUserId.Value &&
                            !m.IsRead)
                    };
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return View(conversations);
        }
        private async Task CreateNotification(
    int userId,
    string title,
    string message,
    string type,
    int? relatedId = null)
        {
            Notification notification =
                new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedId = relatedId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
        }
    }
}
