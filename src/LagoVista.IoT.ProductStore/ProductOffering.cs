// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 852e82425f820167a3a8eba2a20d8de921fdd51b25a7d49553286ccd575c1c7c
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;

namespace LagoVista.IoT.ProductStore
{
    public class ProductOffering
    {
        public Guid Id { get; set; }

        public string Key { get; set; }
        public string ProductCategoryKey { get; set; }
        public string Name { get; set; }

        public string Sku { get; set; }

        public string DetailsHTML { get; set; }

        public string Description { get; set; }

        public string RemoteResourceId { get; set; }

        public decimal UnitPrice { get; set; }

        public string UnitType { get; set; }
    }
}
