using Microsoft.Data.SqlClient;
using Cwiczenia_9.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Cwiczenia_9.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly string _connectionString;

        public WarehouseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddProductToWarehouseAsync(ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Sprawdzenie, czy produkt istnieje oraz pobranie ceny.
                        decimal productPrice;
                        using (var cmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @IdProduct", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                            var priceObj = await cmd.ExecuteScalarAsync();
                            if (priceObj == null)
                                throw new Exception("Invalid parameter: Provided IdProduct does not exist.");
                            productPrice = (decimal)priceObj;
                        }

                        // 2. Sprawdzenie istnienia magazynu.
                        using (var cmd = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                            var exists = await cmd.ExecuteScalarAsync();
                            if (exists == null)
                                throw new Exception("Invalid parameter: Provided IdWarehouse does not exist.");
                        }

                        // 3. Wyszukanie nieprzydzielonego zamówienia z tabeli [Order].
                        int idOrder;
                        string selectOrderQuery = @"
                            SELECT TOP 1 o.IdOrder 
                            FROM [Order] o
                            LEFT JOIN Product_Warehouse pw ON o.IdOrder = pw.IdOrder
                            WHERE o.IdProduct = @IdProduct 
                              AND o.Amount = @Amount 
                              AND o.CreatedAt < @CreatedAt 
                              AND pw.IdProductWarehouse IS NULL";
                        using (var cmd = new SqlCommand(selectOrderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                            cmd.Parameters.AddWithValue("@Amount", request.Amount);
                            cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                            var orderObj = await cmd.ExecuteScalarAsync();
                            if (orderObj == null)
                                throw new Exception("Invalid parameter: There is no order to fulfill.");
                            idOrder = Convert.ToInt32(orderObj);
                        }

                        // Ustalenie aktualnego czasu operacji
                        DateTime currentTime = DateTime.Now;

                        // 4. Aktualizacja kolumny FulfilledAt w tabeli [Order].
                        using (var cmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@FulfilledAt", currentTime);
                            cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0)
                                throw new Exception("Error updating the order fulfillment status.");
                        }

                        // 5. Wstawienie rekordu do tabeli Product_Warehouse.
                        int newId;
                        string insertQuery = @"
                            INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                            SELECT CAST(SCOPE_IDENTITY() AS int);";
                        using (var cmd = new SqlCommand(insertQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                            cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                            cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                            cmd.Parameters.AddWithValue("@Amount", request.Amount);
                            // Cena całkowita = ilość * cena jednostkowa.
                            decimal totalPrice = request.Amount * productPrice;
                            cmd.Parameters.AddWithValue("@Price", totalPrice);
                            cmd.Parameters.AddWithValue("@CreatedAt", currentTime);

                            var newIdObj = await cmd.ExecuteScalarAsync();
                            if (newIdObj == null)
                                throw new Exception("Error inserting into Product_Warehouse.");
                            newId = Convert.ToInt32(newIdObj);
                        }

                        transaction.Commit();
                        return newId;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<int> AddProductToWarehouseUsingSPAsync(ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("AddProductToWarehouse", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    cmd.Parameters.AddWithValue("@Amount", request.Amount);
                    cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                    // Procedura składowana zwraca nowy identyfikator poprzez SELECT @@IDENTITY AS NewId.
                    var newIdObj = await cmd.ExecuteScalarAsync();
                    if (newIdObj == null)
                        throw new Exception("Error inserting product via stored procedure.");
                    return Convert.ToInt32(newIdObj);
                }
            }
        }
    }
}
