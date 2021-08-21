# Welcome to C# Database Unit Test

Hi! I've created this repository to illustrate how to use C# UnitTest to Test Database data and stored procedures.

## Dependencies

 - XUnit Test Framework
 - Dapper
 - FluentAssertions
 - Scrutor
 - Microsoft Dependency Injection
 - Newtonsoft
 
Thank all you guys to build the dependencies.

## How to use

 - Download this source an open in visual studio (2019)
 - Download all nuget dependencies
 - On project we have the folder structure
```
+-- Data
|   +-- Customers (Table in teste. It's an exemple that will use to test)
    |	+-- Customer.csv (file that will loaded in database)
    |	+-- CustomerResult.csv (file with data that we expect from database)
+-- Specs
|   +-- Customers (Organization of UnitTests)
	|   +-- CustomerTest (class that implements the UnitTest)
|   +-- NewFolder(Create a new folder to create a new UnitTest)
	|   +-- NameTest (class that implements the new UnitTest)	
+-- Model
|   +-- Customer (Just to type the results from database for build examples)
+-- app.config (change the connection string here)
```
 - Create a new csv file on Data folder (creating a folder that represent your table) and on the first line of csv put the header. Than the below lines you can fill with data respecting your header.
 - Create a new UnitTest class on Specs folder (creating a folder that represent your table) and implement your UnitTest. This new class needs to inherit from TestBase<T>

## Example of UnitTest class

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

## Helper Methods

### Load CSV (CsvHelper Class)

- LoadCSVToModel
- LoadCSVToDB

### Database
- LoadData
- ExecuteReader
- Execute
- ExecuteSql
- SelectManually
- SelectManuallyScalar
- GetInfoSqlConnection
- GetInfoSqlTransaction
- Rollback

All the methods have a summary to help you to use.
