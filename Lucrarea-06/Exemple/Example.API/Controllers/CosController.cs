using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using static Exemple.Domain.Models.Carucior;
using Exemple.Domain.Repositories;
using Exemple.Domain;
using Example.Api.Models;
using Exemple.Domain.Models;
using static Exemple.Domain.Models.PaidCaruciorEvent;
using Exemple.Events.ServiceBus;
using Exemple.Events;
using LanguageExt;
using Azure.Messaging;
using System.Net.Mime;

namespace Example.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CosController : Controller
    {
        private ILogger<CosController> logger;

        public CosController(ILogger<CosController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [Route("/Cos/getOrders")]
        public async Task<IActionResult> GetOrders([FromServices] ICosRepository cosRepository) =>
            await cosRepository.TryGetExistingCos().Match(
               Succ: GetAllCosHandleSuccess,
               Fail: GetOrdersHandleError
            );

        private ObjectResult GetOrdersHandleError(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return base.StatusCode(StatusCodes.Status500InternalServerError, "UnexpectedError");
        }

        private OkObjectResult GetAllCosHandleSuccess(List<Exemple.Domain.Models.OrderView> cos) =>
        Ok(cos.Select(cos => new
        {
            cos.OrderID, cos.Price, cos.Address
        }));
        
        [HttpPost]
        [Route("/Cos/postTheOrder")]
        public async Task<IActionResult> PostTheOrder([FromServices] IEventSender serviceBusTopicEventSender,[FromServices] PublishProductWorkflow publishProductWorkflow, [FromBody] InputProducts[] products)
        {
            var unvalidatedGrades = products.Select(MapInputProductsToUnvalidatedGrade)
                                          .ToList()
                                          .AsReadOnly();

            PublishQuantityCommand command = new(unvalidatedGrades);

            var result = await publishProductWorkflow.ExecuteAsync(serviceBusTopicEventSender,command);
            return result.Match<IActionResult>(
                whenPaidCaruciorFaildEvent: failedEvent => StatusCode(StatusCodes.Status500InternalServerError, failedEvent.Reason),
                whenPaidCaruciorScucceededEvent: successEvent => Ok()
            ) ;
        }
       
        private static UnvalidatedProductQuantity MapInputProductsToUnvalidatedGrade(InputProducts products) => new UnvalidatedProductQuantity(
            cod: products.ProductID,
            quantity: products.Quantity.ToString(),
            address: products.Address);

        [HttpPost]
        [Route("/Cos/getFiscalBill")]
        public async Task<IActionResult> GetFiscalBill([FromServices] ICosRepository cosRepository, [FromBody] InputOrderID orderID) =>
            await cosRepository.GetFiscalBill(orderID.OrderID).Match(
               Succ: GetFiscalBillSucces,
               Fail: GetFiscalBillHandleError
            );
        private ObjectResult GetFiscalBillHandleError(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return base.StatusCode(StatusCodes.Status500InternalServerError, "UnexpectedError");
        }

        private OkObjectResult GetFiscalBillSucces(string invoice) =>
        Ok(invoice);
      
    }
}

