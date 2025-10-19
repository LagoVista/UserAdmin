// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f40e423c8311a7a82d16f3ba99097f11982a05580765c778d5d9ea5023bd97b1
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.ProductStore
{
    public interface IProductStore
    {
        Task<IEnumerable<ProductOffering>> GetProductsAsync(string orgns, string categoryKey);
    }
}
