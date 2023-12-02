﻿using Microsoft.AspNetCore.Mvc;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Interfaces;
using ShopAPI.Mappers;
using ShopAPI.Models;

namespace ShopAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ItemsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/Item/GetAllItems
        [HttpGet("Item/GetAllItems")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.SearchProductsAsync();

            return Ok(products.ToDTO());
        }

        // GET: api/Item/Filter
        [HttpGet("Item/Filter/{category}")]
        public async Task<IActionResult> FilterProducts(string category, [FromQuery] string keyword = "")
        {
            // Try to get category enum from category string
            bool parseSuccess = Enum.TryParse(category, ignoreCase: true, out Category productCategory);

            if (!parseSuccess)
            {
                return NotFound("Invalid category");
            }

            var products = await _productService.SearchProductsAsync(productCategory, keyword);

            return Ok(products.ToDTO());
        }

        // GET: api/Item/{id}
        [HttpGet("Item/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product.ToDTO());
        }

        // POST: api/Item
        [HttpPost]
        [Route("Item")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO dto)
        {
            // Try to get category enum from category string
            bool parseSuccess = Enum.TryParse(dto.Category, ignoreCase: true, out Category productCategory);

            if (!parseSuccess)
            {
                return BadRequest("Invalid category");
            }

            // Create a new base product entity
            var baseProduct = dto.ToBaseProduct(productCategory);

            // Make sure the product details are valid
            var validationError = ProductHelper.ValidateProductDetails(productCategory, dto.Details);

            // If the product details are invalid, return a bad request
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            // Add the product to the database
            var product = await _productService.CreateProductAsync(baseProduct, dto.Details);

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        // DELETE: api/products/{id}
        [HttpDelete("Item/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Find the product
            var product = await _productService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productService.RemoveProductAsync(id);

            return NoContent();
        }

        [HttpPatch("Inventory/UpdateStock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int stock)
        {
            var product = await _productService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productService.UpdateProductStock(id, stock);

            return NoContent();
        }
    }
}