
using LanguageExt;
using Exemple.Data.Models;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;
using Exemple.Domain.Repositories;
using Exemple.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using Exemple.Events;

namespace Exemple.Data.Repositories
{
    public class CosRepository : ICosRepository
    {
        private readonly CosContext dbContext;
        public CosRepository(CosContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public TryAsync<List<OrderView>> TryGetExistingCos() => async () =>
        {
            var products = await dbContext.Cos
                           .AsNoTracking()
                           .ToListAsync();

            var qry = products.GroupBy(s => new { s.OrderID, s.Address })
                              .Select(g => new
                              {
                                  OrderID = g.Key.OrderID,
                                  Price = g.Sum(xt => xt.Price),
                                  Address = g.Key.Address
                              });

            List<OrderView> list = new List<OrderView>();

            foreach (var o in qry)
            {
                list.Add(new OrderView(o.OrderID, o.Price, o.Address));
            }
            return list;

        };

        public TryAsync<Unit> TrySaveCos(IEventSender sender, Carucior.PublishedCarucior paidCarucior) => async () =>
        {
            var OrderID = new Random().Next(1000);
            var sendAsync = paidCarucior.ProductList.Select(c => CosDto.ToCosDTO(OrderID, c));
            var productStock = await dbContext.Product.AsNoTracking().ToListAsync();
            foreach (var c in sendAsync)
            {
                var prodToUpdate = productStock.Where(p => p.ProductID.Contains(c.ProductID)).Select(p => p);
                foreach (var product in prodToUpdate)
                {
                    product.Stock -= c.Quantity;
                    dbContext.Update(product);
                }
                dbContext.AddRange(c);
            }
            await dbContext.SaveChangesAsync();
            await sender.SendAsync("cos", sendAsync);
            return unit;
        };

        public TryAsync<string> GetFiscalBill(string OrderID) => async () =>
        {
            var products = await dbContext.Cos
                           .AsNoTracking()
                           .ToListAsync();
            StringBuilder ret = new StringBuilder();

            var prodList = products.Where(q => OrderID.Contains(q.OrderID))
                                   .Select(product => new OrderProducts(
                                        id: product.id,
                                        OrderID: product.OrderID.ToString(),
                                        ProductID: product.ProductID.ToString(),
                                        Quantity: product.Quantity,
                                        Price: product.Price,
                                        Address: product.Address.ToString()
                                    )).ToList();

            var qry = products.GroupBy(s => new { s.OrderID, s.Address })
                              .Select(g => new
                              {
                                  OrderID = g.Key.OrderID,
                                  Price = g.Sum(xt => xt.Price),
                                  Address = g.Key.Address
                              });

            var total = qry.Where(q => OrderID.Contains(q.OrderID))
                           .Select(g => new
                           {
                               OrderID = g.OrderID,
                               Price = g.Price,
                               Address = g.Address
                           }).ToList();

            int i = 0;

            ret.AppendLine($"{"ProductID",-10}{"Quantity",-10}{"Price",-10}");
            foreach (var product in prodList)
            {
                ret.AppendLine($"{product.ProductID,-10} {product.Quantity,-10} {product.Price,-10}");
            }
            ret.AppendLine("---------------YOUR ORDER----------------");
            foreach (var product in prodList)
                foreach (var t in total)
                {
                    ret.AppendLine($"{"AWB:",-30}{RandomString(15)}");
                    ret.AppendLine($"{"OrderID:   ",-30}{t.OrderID,-10}");
                    ret.AppendLine($"{"Adresa:    ",-30}{t.Address,-10}");
                    ret.AppendLine($"{"Pret total:",-30}{t.Price,-10}");
                    return ret.ToString();
                }
            return ret.ToString();
        };

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
