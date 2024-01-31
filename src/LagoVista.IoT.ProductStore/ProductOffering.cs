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
