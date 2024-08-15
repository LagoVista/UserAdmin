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
