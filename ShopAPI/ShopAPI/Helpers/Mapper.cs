using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;

namespace ShopAPI.Mappers
{
    public static class Mapper
    {
        /// <summary>
        /// Converts a ProductDTO to the base Product type based on the specified category.
        /// </summary>
        /// <param name="dto">The ProductDTO to convert.</param>
        /// <param name="category">The category of the product.</param>
        /// <returns>The base Product object with properties populated from the DTO.</returns>
        /// <exception cref="NotImplementedException">Thrown when the provided category is not supported.</exception>
        public static Product ToBaseProduct(this ProductDTO dto, Category category)
        {
            Product product = category switch
            {
                Category.Cpu => new Cpu(),
                Category.Case => new Case(),
                Category.CpuCooler => new CpuCooler(),
                Category.Memory => new Memory(),
                Category.Motherboard => new Motherboard(),
                Category.Storage => new Storage(),
                Category.VideoCard => new VideoCard(),
                Category.PowerSupply => new PowerSupply(),
                _ => throw new NotImplementedException()
            };

            product.Name = dto.Name;
            product.Category = category;
            product.Price = dto.Price;
            product.Manufacturer = dto.Manufacturer;
            product.NormalizedName = dto.Name.ToLower();
            product.Stock = dto.Stock;

            return product;
        }

        /// <summary>
        /// Converts a Product object to a ProductDTO, including additional details based on the product type.
        /// </summary>
        /// <param name="product">The Product object to convert.</param>
        /// <returns>A ProductDTO representing the converted product.</returns>
        /// <exception cref="NotImplementedException">Thrown when the product type is not supported.</exception>
        public static ProductDTO ToDTO(this Product product, bool includeDetails = true)
        {
            ProductDTO dto = new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category.ToString(),
                Price = product.Price,
                Manufacturer = product.Manufacturer,
                Stock = product.Stock
            };

            if (includeDetails)
            {
                dto.Details = GetDetails(product);
            }

            return dto;
        }

        public static TotalsDTO GetTotalsDTO(this Bill bill)
        {
            return new TotalsDTO()
            {
                BaseTotal = bill.GetTotalWithoutSurcharges(),
                BundleTotal = bill.GetTotalWithoutTaxes(),
                TaxTotal = bill.GetTotalWithTaxes(),
            };
        }

        /// <summary>
        /// Converts a collection of Product objects to a collection of ProductDTO objects.
        /// </summary>
        /// <param name="products">The collection of Product objects to convert.</param>
        /// <returns>A collection of ProductDTO representing the converted products.</returns>
        public static IEnumerable<ProductDTO> ToDTO(this IEnumerable<Product> products, bool includeDetails = true) =>
            products.Select(p => p.ToDTO(includeDetails));

        private static Dictionary<string, string> GetDetails(Product product)
        {
            Dictionary<string, string> details = new();

            var properties = product.GetType().GetProperties()
                .Where(prop => prop.DeclaringType == product.GetType());

            foreach (var property in properties)
            {
                details.Add(property.Name.ToLowerInvariant(), property.GetValue(product)?.ToString() ?? "");
            }

            return details;
        }

        public static List<CartItemDTO> ToDTO(this List<CartItem> cartItems)
        {
            List<CartItemDTO> cartItemDTOs = new(cartItems.Count);
            foreach (var cartItem in cartItems)
            {
                cartItemDTOs.Add(
                    new CartItemDTO(cartItem.Product.ToDTO(), cartItem.Quantity)
                );
            }

            return cartItemDTOs;
        }

        public static CartItemDTO ToDTO(this CartItem cartItem)
        {
            return new CartItemDTO(cartItem.Product.ToDTO(), cartItem.Quantity);
        }

        public static CartDTO ToDTO(this Cart cart)
        {
            CartDTO dto = new CartDTO
            {
                Id = cart.Id,
                Name = cart.Name,
                Items = cart.Items.ToDTO(),
                Totals = Calculate.DefaultBill(cart.Items).GetTotalsDTO()
            };

            return dto;
        }

        /// <summary>
        /// Ensures bill has 2 digit rounding
        /// </summary>
        /// <returns>Returns a bill with charges rounded to 2 decimal places</returns>
        public static BillDTO ToDTO(this Bill bill)
        {
            BillDTO billDTO = new()
            {
                BaseCharges = new(),
                BundleSurcharges = bill.BundleSurcharges,
                TaxSurcharges = bill.TaxSurcharges,
            };

            // Round tax surcharges to 2 decimal places
            foreach (var surcharge in billDTO.TaxSurcharges)
            {
                surcharge.Cost = Math.Round(surcharge.Cost, 2);
            }

            // Round bundle surcharges to 2 decimal places
            foreach (var surcharge in billDTO.BundleSurcharges)
            {
                surcharge.Cost = Math.Round(surcharge.Cost, 2);
            }

            // Add each item to the DTO as a surcharge
            foreach (var item in bill.Items)
            {
                billDTO.BaseCharges.Add(new Surcharge()
                {
                    // Round cost to 2 decimal places
                    Cost = Math.Round(item.Product.Price, 2),
                    // Ex 5X AMD Ryzen...
                    Description = item.Quantity + "X " + item.Product.Name,
                });
            }

            return billDTO;
        }
    }
}
