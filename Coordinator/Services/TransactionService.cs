using Coordinator.Models;
using Coordinator.Models.Contexts;
using Coordinator.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services
{
    public class TransactionService(IHttpClientFactory _httpClientFactory, TwoPhaseCommitContext _context) : ITransactionService
    {
        HttpClient _orderHttpClient = _httpClientFactory.CreateClient("OrderAPI");
        HttpClient _stockttpClient = _httpClientFactory.CreateClient("StockAPI");
        HttpClient _paymentHttpClient = _httpClientFactory.CreateClient("PaymentAPI");
        public async Task<Guid> CreateTransactionAsync()
        {
            Guid transactionId = Guid.NewGuid();

            var nodes = await _context.Nodes.ToListAsync();
            nodes.ForEach(node => node.NodeStates = new List<NodeState>()
            {
                new(transactionId)
                {
                    IsReady = Enums.ReadyType.Pending,
                    TransactionState = Enums.TransactionState.Pending
                }
            });
            await _context.SaveChangesAsync();
            return transactionId;
        }
        public async Task PreapareServicesAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                .Include(ns => ns.Node)
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync();

            foreach (var node in transactionNodes)
            {
                try
                {
                    var response = await (node.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("ready"),
                        "Stock.API" => _stockttpClient.GetAsync("ready"),
                        "Payment.API" => _paymentHttpClient.GetAsync("ready")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    node.IsReady = result ? Enums.ReadyType.Ready : Enums.ReadyType.UnReady;
                }
                catch (Exception)
                {
                    node.IsReady = Enums.ReadyType.UnReady;
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckReadyServicesAsync(Guid TransactionId) => (await _context.NodeStates
                .Where(ns => ns.TransactionId == TransactionId)
                .ToListAsync()).TrueForAll(ns => ns.IsReady == Enums.ReadyType.Ready);
        public async Task CommitAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                                    .Where(ns => ns.TransactionId == transactionId)
                                    .Include(ns => ns.Node)
                                    .ToListAsync();

            foreach (var node in transactionNodes)
            {
                try
                {
                    var response = await (node.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("commit"),
                        "Stock.API" => _stockttpClient.GetAsync("commit"),
                        "Payment.API" => _paymentHttpClient.GetAsync("commit"),
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    node.TransactionState = result ? Enums.TransactionState.Done : Enums.TransactionState.Abort;
                }
                catch (Exception)
                {
                    node.TransactionState = Enums.TransactionState.Abort;
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
            => (await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.TransactionState == Enums.TransactionState.Done);
        public async Task RollBackAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                                    .Where(ns => ns.TransactionId == transactionId)
                                    .Include(ns => ns.Node)
                                    .ToListAsync();

            foreach (var node in transactionNodes)
            {
                try
                {
                    if (node.TransactionState == Enums.TransactionState.Done)
                    {
                        _ = await (node.Node.Name switch
                        {
                            "Order.API" => _orderHttpClient.GetAsync("rollback"),
                            "Stock.API" => _stockttpClient.GetAsync("rollback"),
                            "Payment.API" => _paymentHttpClient.GetAsync("rollback")
                        });

                        node.TransactionState = Enums.TransactionState.Abort;
                    }
                }
                catch (Exception)
                {
                    node.TransactionState = Enums.TransactionState.Abort;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
