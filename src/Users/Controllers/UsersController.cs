using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Pitstop.CustomerManagementAPI.DataAccess;
using Pitstop.CustomerManagementAPI.Model;
using AutoMapper;
using Pitstop.Infrastructure.Messaging;
using Pitstop.CustomerManagementAPI.Events;
using Pitstop.CustomerManagementAPI.Commands;

namespace WhileBlue.Application.Users.Controllers
{
    [Route("/api/[controller]")]
    public class UsersController : Controller
    {
        IMessagePublisher _messagePublisher;
        CustomerManagementDBContext _dbContext;

        public UsersController(CustomerManagementDBContext dbContext, IMessagePublisher messagePublisher)
        {
            _dbContext = dbContext;
            _messagePublisher = messagePublisher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _dbContext.Customers.ToListAsync());
        }

        [HttpGet]
        [Route("{userId}", Name = "GetUserById")]
        public async Task<IActionResult> GetUserById(string customerId)
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUser command)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // insert user
                    Customer customer = Mapper.Map<User>(command);
                    _dbContext.Users.Add(customer);
                    await _dbContext.SaveChangesAsync();

                    CustomerRegistered e = Mapper.Map<CustomerRegistered>(command);
                    await _messagePublisher.PublishMessageAsync(e.MessageType, e , "");

                    // return result
                    return CreatedAtRoute("GetByCustomerId", new { customerId = customer.CustomerId }, customer);
                }
                return BadRequest();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}