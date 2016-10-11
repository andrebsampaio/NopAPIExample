using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NopMobile.AppCore.NopAPI;
using System.Net;
using System.ComponentModel;
using Windows.Data.Xml.Dom;
using NopMobile.AppCore.Exceptions;
using Windows.Web.Http;

public class NopCore
{
    NopServiceClient client;
    public NopCore()
    {
        client = new NopServiceClient();
    }

    public void NewOrderNote(int orderID, string note, bool showUser)
    {
        client.AddOrderNoteAsync(orderID, note, showUser );
    }

    public Task<bool> CheckLogin(string user, string password)
    {
        var tcs = new TaskCompletionSource<bool>();
        client.CheckLoginCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.CheckLoginAsync(user, password); 
        return tcs.Task;   
    }

    public Task<bool> CheckLoginClient(string user, string password)
    {
        var tcs = new TaskCompletionSource<bool>();
        client.CheckLoginClientCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.CheckLoginClientAsync(user, password);
        return tcs.Task;
    }

    public Task<bool> RegisterClient(bool Male, string Firstname, string Lastname, DateTime Birthday, string Email, string Company, string Password)
    {
        var tcs = new TaskCompletionSource<bool>();
        client.RegisterClientCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.RegisterClientAsync(Male,Firstname, Lastname, Birthday, Email, Company, Password);
        return tcs.Task;
    }

    public Task<ProductDTO[]> RecentlyViewedProducts(int number)
    {
        var tcs = new TaskCompletionSource<ProductDTO[]>();
        client.RecentlyViewedProductsCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.RecentlyViewedProductsAsync(number);
        return tcs.Task;
    }

    public Task<ProductDTO[]> FeaturedProducts()
    {
        var tcs = new TaskCompletionSource<ProductDTO[]>();
        client.FeaturedProductsCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.FeaturedProductsAsync();
        return tcs.Task;
    }

    public Task<CategoryDTO[]> GetSubCategoriesFromParent(int Id)
    {
        var tcs = new TaskCompletionSource<CategoryDTO[]>();
        client.GetSubCategoriesFromParentCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetSubCategoriesFromParentAsync(Id);
        return tcs.Task;
    }

    public Task<CategoryDTO[]> GetMainCategories()
    {
        var tcs = new TaskCompletionSource<CategoryDTO[]>();
        client.GetMainCategoriesCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetMainCategoriesAsync();
        return tcs.Task;
    }

    public Task<bool> AddToCart(string Username, int Id, int Quantity, string [] Attributes, ShoppingCartType Type)
    {
        var tcs = new TaskCompletionSource<bool>();
        client.AddToCartCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.AddToCartAsync(Username, Id, Quantity, new List<string>(Attributes), Type);
        return tcs.Task;
    }

    public Task<bool> RemoveFromCart(string Username, int Id)
    {
        var tcs = new TaskCompletionSource<bool>();
        client.RemoveFromCartCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.RemoveFromCartAsync(Username, Id);
        return tcs.Task;
    }

    public Task<ProductDTO> GetProductById(int id)
    {
        var tcs = new TaskCompletionSource<ProductDTO>();
        client.GetProductByIdCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetProductByIdAsync(id);
        return tcs.Task;
    }

    public Task<ProductDTO[]> GetAllProductsFromCategory(int id)
    {
        var tcs = new TaskCompletionSource<ProductDTO[]>();
        client.GetAllProductsFromCategoryCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetAllProductsFromCategoryAsync(id);
        return tcs.Task;
    }

    public Task<CategoryDTO> GetCategoryById(int id)
    {
        var tcs = new TaskCompletionSource<CategoryDTO>();
        client.GetCategoryByIdCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetCategoryByIdAsync(id);
        return tcs.Task;
    }

    public Task<ProductDTO[]> SearchProducts(string Words)
    {
        var tcs = new TaskCompletionSource<ProductDTO[]>();
        client.GetProductsByKeywordsCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetProductsByKeywordsAsync(Words);
        return tcs.Task;
    }

    public Task<string> GetFullName(string user)
    {
        var tcs = new TaskCompletionSource<string>();
        client.GetUserNameCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetUserNameAsync(user);
        return tcs.Task;
    }

    public Task<OrderDTO> GetOrderById(int id)
    {
        var tcs = new TaskCompletionSource<OrderDTO>();
        client.GetOrderByIdCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetOrderByIdAsync(id);
        return tcs.Task;
    }

    public Task<OrderDTO[]> UserOrders(string user)
    {
        TaskCompletionSource<OrderDTO[]> tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, 0, 0, 0, 0, null, null, null, user, 0, 20);
        return tcs.Task;
    }

    public Task<CountryDTO[]> GetAllCountries()
    {
        var tcs = new TaskCompletionSource<CountryDTO[]>();
        client.GetAllCountriesCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetAllCountriesAsync();
        return tcs.Task;   
    }

    public Task<OrderDTO[]> ShippingStatusOrders(string SS)
    {
        ShippingStatus Status = ShippingStatus.Delivered;
        switch (SS)
        {
            case "Delivered":
                Status = ShippingStatus.Delivered;
                break;
            case "Not Yet Shipped":
                Status = ShippingStatus.NotYetShipped;
                break;
            case "Partially Shipped":
                Status = ShippingStatus.PartiallyShipped;
                break;
            case "Shipped":
                Status = ShippingStatus.Shipped;
                break;
            case "Shipping Not Required":
                Status = ShippingStatus.ShippingNotRequired;
                break;
        }
        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, 0, 0, 0, 0, null, null, Status, null, 0, 20);
        return tcs.Task;
    }

    public  Task<OrderDTO[]> OrderStatusOrders(string OS)
    {

        OrderStatus Status = OrderStatus.Complete;
        switch (OS)
        {
            case "Complete":
                Status = OrderStatus.Complete;
                break;
            case "Pending":
                Status = OrderStatus.Pending;
                break;
            case "Processing":
                Status = OrderStatus.Processing;
                break;
            case "Cancelled":
                Status = OrderStatus.Cancelled;
                break;
        }

        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, 0, 0, 0, 0, Status, null, null, null, 0, 20);
        return tcs.Task;
    }

    public Task<OrderDTO[]> PayStatusOrders(string PS)
    {
            PaymentStatus Status = PaymentStatus.Paid;
            switch (PS)
            {
                case "Paid":
                    Status = PaymentStatus.Paid;
                    break;
                case "Authorized":
                    Status = PaymentStatus.Authorized;
                    break;
                case "Partially Refunded":
                    Status = PaymentStatus.PartiallyRefunded;
                    break;
                case "Refunded":
                    Status = PaymentStatus.Refunded;
                    break;
                case "Voided":
                    Status = PaymentStatus.Voided;
                    break;
            }

            var tcs = new TaskCompletionSource<OrderDTO[]>();
            client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
            client.GetOrdersAsync(0, 0, 0, 0, 0, null, Status, null, null, 0, 20);
            return tcs.Task;
    }

    public Task<OrderDTO[]> StoreIdOrders(int StoreId)
    {

        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(StoreId, 0, 0, 0, 0, null, null, null, null, 0, 20);
        return tcs.Task;
    }

    public Task<OrderDTO[]> VendorIdOrders(int VendorId)
    {
        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, VendorId, 0, 0, 0, null, null, null, null, 0, 20);
        return tcs.Task;
    }

    public Task<OrderDTO[]> CustomerIdOrders(int CustomerId)
    {
        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, 0, CustomerId, 0, 0, null, null, null, null, 0, 20);
        return tcs.Task;
    }

    public Task<OrderDTO[]> ProductIdOrders(int ProductId)
    {

        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, 0, 0, ProductId, 0, null, null, null, null, 0, 20);
        return tcs.Task;
    }

    public Task<OrderDTO[]> AffiliateIdOrders(int AffiliateId)
    {
        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, 0, 0, 0, AffiliateId, null, null, null, null, 0, 20);
        return tcs.Task;
    }

    public Task<OrderDTO[]> GetLatestOrders(int OrderCount)
    {
        var tcs = new TaskCompletionSource<OrderDTO[]>();
        client.GetOrdersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetOrdersAsync(0, 0, 0, 0, 0, null, null, null, "", 0, OrderCount);
        return tcs.Task;
    }

    public Task<decimal> GetStats(int OrderStatus)
    {
        var tcs = new TaskCompletionSource<decimal>();
        switch (OrderStatus)
        {
            case 1:
                 client.GetCancelledTotalCompleted += (sender, result) => tcs.TrySetResult(result.Result);
                 client.GetCancelledTotalAsync();
                 break;
            case 2:
                client.GetCompleteTotalCompleted += (sender, result) => tcs.TrySetResult(result.Result);
                client.GetCompleteTotalAsync();
                break;
            case 3:
                client.GetPendingTotalCompleted += (sender, result) => tcs.TrySetResult(result.Result);
                client.GetPendingTotalAsync();
                break;
        }
        return tcs.Task;
    }

    public Task<string> GetStoreUrl()
    {
        var tcs = new TaskCompletionSource<string>();
        client.GetStoreUrlCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetStoreUrlAsync();
        return tcs.Task;
    }

    public Task<string> GetStoreName()
    {
        var tcs = new TaskCompletionSource<string>();
        client.GetStoreNameCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetStoreNameAsync();
        return tcs.Task;
    }

    public Task<CustomerDTO[]> GetCustomers()
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomerListCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomerListAsync();
        return tcs.Task;

    }

    public void SetStoreUrl(string Url)
    {
        if (Url.Equals(string.Empty))
        {
            throw new WrongUrlException("The store Url cannot be empty");
        }
        client.Endpoint.Address = new System.ServiceModel.EndpointAddress("http://" + Url.Replace(" ", string.Empty) + "/Plugins/Misc.WebServices/Remote/NopService.svc?singleWsdl");
    }

    public void EndSession()
    {
        //client.EndSession();
        //client = new NopService();
    }



    public Task<int> GetPendingOrdersCount()
    {
        var tcs = new TaskCompletionSource<int>();
        client.GetPendingOrdersCountCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetPendingOrdersCountAsync();
        return tcs.Task;
    }

    public Task<int> GetCartsCount()
    {
        var tcs = new TaskCompletionSource<int>();
        client.GetCurrentCartsCountCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetCurrentCartsCountAsync();
        return tcs.Task;
    }

    public Task<int> GetWishlistCount()
    {
        var tcs = new TaskCompletionSource<int>();
        client.GetWishlistCountCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetWishlistCountAsync();
        return tcs.Task;
    }

    public Task<int> GetRegisteredCustomersCount()
    {
        var tcs = new TaskCompletionSource<int>();
        client.GetRegisteredCustomersCountCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetRegisteredCustomersCountAsync();
        return tcs.Task;
    }

    public Task<int> GetVendorsCount()
    {
        var tcs = new TaskCompletionSource<int>();
        client.GetVendorsCountCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetVendorsCountAsync();
        return tcs.Task;
    }

    public Task<int> GetOnlineCount()
    {
        var tcs = new TaskCompletionSource<int>();
        client.GetOnlineCountCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetOnlineCountAsync();
        return tcs.Task;
    }

    public Task<KeywordDTO[]> GetPopularKeywords(int n)
    {
        var tcs = new TaskCompletionSource<KeywordDTO[]>();
        client.GetPopularKeywordsCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetPopularKeywordsAsync(n);
        return tcs.Task;
    }

    public Task<BestsellerDTO[]> GetBestsellerByAmount()
    {
        var tcs = new TaskCompletionSource<BestsellerDTO[]>();
        client.GetBestsellersByAmountCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetBestsellersByAmountAsync();
        return tcs.Task;
    }

    public Task<BestsellerDTO[]> GetBestsellerByQuantity()
    {
        var tcs = new TaskCompletionSource<BestsellerDTO[]>();
        client.GetBestsellersByQuantityCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetBestsellersByQuantityAsync();
        return tcs.Task;
    }

    public Task<bool> AddAddress(string Email, string Firstname, string Lastname, string CountryName, string City, string Street, string PostalCode, string PhoneNumber, string Province)
    {
        var tcs = new TaskCompletionSource<bool>();
        client.AddAddressCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.AddAddressAsync(Email,Firstname,Lastname,CountryName,City,Street,PostalCode,PhoneNumber, Province);
        return tcs.Task;
    }

    public Task<CustomerDTO[]> GetBestCustomers()
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetBestCustomersCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetBestCustomersAsync();
        return tcs.Task;
    }

    public Task<ProductDTO[]> CategoryProductsSortedFiltered(int CatId, bool Sorted, bool Filtered, ProductSortingEnum Sorting, decimal Max, decimal Min)
    {
        var tcs = new TaskCompletionSource<ProductDTO[]>();
        client.CategoryProductsSortedFilteredCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.CategoryProductsSortedFilteredAsync(CatId,Sorted,Filtered,Sorting,Max,Min);
        return tcs.Task;
    }

    public Task<string[]> GetShippingMethods()
    {
        var tcs = new TaskCompletionSource<string[]>();
        client.ShippingMethodsCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.ShippingMethodsAsync();
        return tcs.Task;
    }

    public Task<decimal> GetShippingFees(int CustomerId)
    {
        var tcs = new TaskCompletionSource<decimal>();
        client.GetShippingFeesCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetShippingFeesAsync(CustomerId);
        return tcs.Task;
    }

    public Task<decimal> GetTaxFees(int CustomerId)
    {
        var tcs = new TaskCompletionSource<decimal>();
        client.GetTaxFeesCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetTaxFeesAsync(CustomerId);
        return tcs.Task;
    }

    public Task<string[]> GetPaymentMethods()
    {
        var tcs = new TaskCompletionSource<string[]>();
        client.PaymentMethodsCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.PaymentMethodsAsync();
        return tcs.Task;
    }

    public Task<string[]> AddNewOrder(int CustomerId, int BillingAddressId, int ShippingAddressId, string ShippingMethod, string PaymentMethod, bool GiftWrap )
    {
        var tcs = new TaskCompletionSource<string[]>();
        client.AddNewOrderCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.AddNewOrderAsync(CustomerId,BillingAddressId,ShippingAddressId,ShippingMethod,PaymentMethod, GiftWrap);
        return tcs.Task;
    }

    public Task<string> GetCurrency()
    {
        var tcs = new TaskCompletionSource<string>();
        client.GetCurrencyCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetCurrencyAsync();
        return tcs.Task;
    }

    public Task<string> GetPdfInvoice(int OrderId)
    {
        var tcs = new TaskCompletionSource<string>();
        client.GetPdfInvoiceCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetPdfInvoiceAsync(OrderId);
        return tcs.Task;
    }

    public Task<bool> ReOrder(int OrderId)
    {
        var tcs = new TaskCompletionSource<bool>();
        client.ReOrderCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.ReOrderAsync(OrderId);
        return tcs.Task;
    }

    public Task<CustomerDTO[]> GetCustomersByEmail(string Email)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomersByEmailCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomersByEmailAsync(Email);
        return tcs.Task;
    }

    public Task<CustomerDTO[]> GetCustomersByUsername(string Username)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomersByUsernameCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomersByUsernameAsync(Username);
        return tcs.Task;

    }

    public Task<CustomerDTO[]> GetCustomersByFirstname(string Firstname)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomersByFirstnameCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomersByFirstnameAsync(Firstname);
        return tcs.Task;

    }

    public Task<CustomerDTO[]> GetCustomersByLastname(string Lastname)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomersByLastnameCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomersByLastnameAsync(Lastname);
        return tcs.Task;

    }

    public Task<CustomerDTO[]> GetCustomersByFullname(string Fullname)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomersByFullnameCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomersByFullnameAsync(Fullname);
        return tcs.Task;

    }

    public Task<CustomerDTO[]> GetCustomersByCompany(string Company)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomersByCompanyCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomersByCompanyAsync(Company);
        return tcs.Task;

    }

    public Task<CustomerDTO[]> GetCustomersByPhone(string Phone)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCustomersByPhoneCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCustomersByPhoneAsync(Phone);
        return tcs.Task;

    }

    public Task<CustomerDTO[]> GetCustomersByPostalCode(string PostalCode)
    {
            
            var tcs = new TaskCompletionSource<CustomerDTO[]>();
            client.GetCustomersByPostalCodeCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
            client.GetCustomersByPostalCodeAsync(PostalCode);
            return tcs.Task;

    }

    public Task<int> GetCustomerCountByTime(int days)
    {
        var tcs = new TaskCompletionSource<int>();
        client.GetRegisteredUsersCountByTimeCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetRegisteredUsersCountByTimeAsync(days);
        return tcs.Task;
    }

    public Task<decimal> GetTotalSalesByTime(int days)
    {
        var tcs = new TaskCompletionSource<decimal>();
        client.GetTotalSalesByTimeCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetTotalSalesByTimeAsync(days);
        return tcs.Task;
    }

    public Task<decimal> GetPendingOrdersByReason(string reason)
    {
        var tcs = new TaskCompletionSource<decimal>();
        client.GetTotalPendingByReasonCompleted += (sender, result) => tcs.TrySetResult(result.Result);
        client.GetTotalPendingByReasonAsync(reason);
        return tcs.Task;
    }

    public Task<CustomerDTO[]> GetCurrentCarts(string Filter, string Email, int LowerItems, int HigherItems, decimal LowerTotal, decimal HigherTotal, bool Abandoned)
    {
        var tcs = new TaskCompletionSource<CustomerDTO[]>();
        client.GetCurrentCartsCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.GetCurrentCartsAsync(Filter, Email, LowerItems, HigherItems, LowerTotal, HigherTotal, false);
        return tcs.Task;
    }

    public Task<OrderError[]> CancelOrders(int[] id)
    {
        var tcs = new TaskCompletionSource<OrderError[]>();
        client.DeleteOrdersWithoutUserCompleted += (sender, result) => tcs.TrySetResult(result.Result.ToArray());
        client.DeleteOrdersWithoutUserAsync(new List<int>(id));
        return tcs.Task;

    }

    public async Task ChangeOrderStatus(int id, string status)
    {

        OrderStatus statusE = OrderStatus.Pending;
        switch (status)
        {
            case "Pending":
                statusE = OrderStatus.Pending;
                break;
            case "Complete":
                statusE = OrderStatus.Complete;
                break;
            case "Cancelled":
                statusE = OrderStatus.Cancelled;
                break;
            case "Processing":
                statusE = OrderStatus.Processing;
                break;
        }

        await Task.Factory.StartNew(() =>
        {
            client.ChangeOrderStatusAsync(id, statusE);
        }).ConfigureAwait(false);
    }

    public async Task ChangePaymentStatus(int id, PaymentStatus status)
    {
        await Task.Factory.StartNew(() =>
        {
            client.ChangePaymentStatusAsync(id, status);
        }).ConfigureAwait(false);
       
    }

    public async Task ChangeShippingStatus(int id, ShippingStatus status)
    {
        await Task.Factory.StartNew(() =>
        {
            client.ChangeShippingStatusAsync(id, status);
        }).ConfigureAwait(false);
        
    }

    public async Task AddCommentToCustomer(string email, string comment)
    {
        await Task.Factory.StartNew(() =>
        {
            client.AddCommentToCustomerAsync(email, comment);
        }).ConfigureAwait(false);
        
    }

}
