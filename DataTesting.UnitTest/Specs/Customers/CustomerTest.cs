using Microsoft.Extensions.DependencyInjection;
using DataTesting.UnitTest.Helpers;
using DataTesting.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Xunit;

namespace DataTesting.UnitTest.Specs.Customers
{
    [Trait("Procedure", "customer_procedure")]
    public class CustomerTest : TestBase<Customer>
    {

        private readonly ICsvHelper<Customer> csvHelper;
        private readonly IDatabaseHelper<Customer> databaseHelper;

        public CustomerTest()
        {
            this.csvHelper = ServiceProvider.GetService<ICsvHelper<Customer>>();
            this.databaseHelper = ServiceProvider.GetService<IDatabaseHelper<Customer>>();
        }

        [Fact(DisplayName = "Add_Customer_ReturnsSameCustomersAdded")]
        public void Add_Customer_ReturnsSameCustomersAdded()
        {
            csvHelper.LoadCSVToDB(@"Data\Customers\Customer.csv", "Customer");

            var loadedResult = databaseHelper.SelectManually("select * from Customer where CustomerID = @CustomerID", new
            {
                CustomerID = 2
            });

            var expectedResult = csvHelper.LoadCSVToModel(@"Data\Customers\CustomerResult.csv");

            loadedResult.Should().BeEquivalentTo(expectedResult);
        }

        [Fact(DisplayName = "Add_Customer_ReturnsCustomerID2")]
        public void Add_Customer_ReturnsCustomerID2()
        {
            csvHelper.LoadCSVToDB(@"Data\Customers\Customer.csv", "Customer");

            var result = databaseHelper.ExecuteReader("customer_procedure", new
            {
                veExecutor = 2,
                veParameter = 2,
                veCustomerID = 2
            });

            var expectedResult = csvHelper.LoadCSVToModel(@"Data\Customers\CustomerResult.csv");

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact(DisplayName = "Add_NewCustomer_CompareNewID")]
        public void Add_NewCustomer_CompareNewID()
        {
            csvHelper.LoadCSVToDB(@"Data\Customers\Customer.csv", "Customer");

            int lastId = databaseHelper.SelectManuallyScalar("select max(customerid) from Customer");

            databaseHelper.Execute("customer_procedure", new
            {
                veExecutor = 1,
                veParameter = 1,
                veName = "PRIMEIRO TESTE",
                veAge = 50,
                veEmail = "teste@teste.com.br"
            });

            int newId = databaseHelper.SelectManuallyScalar("select max(customerid) from Customer");

            newId.Should().BeGreaterThan(lastId);
        }

        [Fact(DisplayName = "Get_AgeBiggerThan35_ReturnsMarie")]
        public void Get_AgeBiggerThan35_ReturnsMarie()
        {          

            csvHelper.LoadCSVToDB(@"Data\Customers\Customer.csv", "Customer");

            var result = databaseHelper.ExecuteReader("customer_procedure", new
            {
                veExecutor = 2,
                veParameter = 3,
                veAge = 35,
            });

            var expectedResult = csvHelper.LoadCSVToModel(@"Data\Customers\CustomerResult.csv");

            expectedResult.Should().BeEquivalentTo(result);
        }

    }
}
