using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NopMobile.Core.NopAPI;
using System.Web.Services.Protocols;
using NopMobile.Core.Exceptions;
using System.Net;

public class NopCore
{
    NopService client;

    // Volatile is used as hint to the compiler that this data 
    // member will be accessed by multiple threads. 
    
    public NopCore()
    {
        client = new NopService();
    }

    public void NewOrderNote(int orderID, string note, string user, string password)
    {
        client.AddOrderNoteAsync(orderID, note, true, user, password);
    }

    public async Task CheckLogin(string user, string password)
    {
        try
        {
            await Task.Factory.StartNew(() =>
            {
                client.CheckLogin(user, password);
            }).ConfigureAwait(false);
            
        }
        catch (SoapException ex)
        {
            Console.WriteLine(ex.Message);
            throw new LoginException("The credentials you entered are incorrect");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            throw new LoginException("The credentials you entered are incorrect");
        }
    }

    public string GetFullName(string user)
    {
        return client.GetUserName(user);
    }

    public async Task<OrderDTO> GetOrderById(int id)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetOrderById(id);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> UserOrders(string user)
    {
        return await Task.Factory.StartNew(() =>
                {
                    return client.GetOrders(0, 0, 0, 0, 0, null, null, null, user, 0, 20);
                }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> ShippingStatusOrders(string SS)
    {

        return await Task.Factory.StartNew(() =>
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
            return client.GetOrders(0, 0, 0, 0, 0, null, null, Status, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> OrderStatusOrders(string OS)
    {

        return await Task.Factory.StartNew(() =>
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
            return client.GetOrders(0, 0, 0, 0, 0, Status, null, null, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> PayStatusOrders(string PS)
    {

        return await Task.Factory.StartNew(() =>
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
            return client.GetOrders(0, 0, 0, 0, 0, null, Status, null, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> StoreIdOrders(int StoreId)
    {

        return await Task.Factory.StartNew(() =>
        {
            return client.GetOrders(StoreId, 0, 0, 0, 0, null, null, null, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> VendorIdOrders(int VendorId)
    {

        return await Task.Factory.StartNew(() =>
        {
            return client.GetOrders(0, VendorId, 0, 0, 0, null, null, null, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> CustomerIdOrders(int CustomerId)
    {

        return await Task.Factory.StartNew(() =>
        {
            return client.GetOrders(0, 0, CustomerId, 0, 0, null, null, null, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> ProductIdOrders(int ProductId)
    {

        return await Task.Factory.StartNew(() =>
        {
            return client.GetOrders(0, 0, 0, ProductId, 0, null, null, null, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> AffiliateIdOrders(int AffiliateId)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetOrders(0, 0, 0, 0, AffiliateId, null, null, null, null, 0, 20);
        }).ConfigureAwait(false);
    }

    public async Task<OrderDTO[]> GetLatestOrders(int OrderCount)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetOrders(0, 0, 0, 0, 0, null, null, null, "", 0, OrderCount);
        }).ConfigureAwait(false);
    }

    public async Task<decimal> GetStats(int OrderStatus)
    {
        switch (OrderStatus)
        {
            case 1:
                return await Task.Factory.StartNew(() =>
                {
                    return client.GetCancelledTotal();
                }).ConfigureAwait(false);
            case 2:
                return await Task.Factory.StartNew(() =>
                {
                    return client.GetCompleteTotal();
                }).ConfigureAwait(false);
            case 3:
                return await Task.Factory.StartNew(() =>
                {
                    return client.GetPendingTotal();
                }).ConfigureAwait(false);
            default:
                return -1;
        }
    }

    public string GetStoreName()
    {
        return client.GetStoreName();
    }

    public async Task<CustomerDTO[]> GetCustomers()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomerList();
        }).ConfigureAwait(false);

    }

    public async Task SetStoreUrl(string Url)
    {
        if (Url.Equals(string.Empty))
        {
            throw new WrongUrlException("The store Url cannot be empty");
        }
        try
        {
            client.Url = "http://" + Url.Replace(" ", string.Empty) + "/Plugins/Misc.WebServices/Remote/NopService.svc?singleWsdl";
            
            await Task.Factory.StartNew(() =>
            {
                HttpWebRequest request =
            WebRequest.Create(client.Url) as HttpWebRequest;
                request.Timeout = 30000;

                using (HttpWebResponse response =
                           request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new WrongUrlException("Error locating web service");
                }
            }).ConfigureAwait(false);

            
        }
        catch (Exception ex)
        {
            if (ex is WebException || ex is UriFormatException)
            {
                throw new WrongUrlException("The store Url appears to be incorrect");
            }
        }
    }

    public void EndSession()
    {
        client.EndSession();
        client = new NopService();
    }

    public async Task<int> GetPendingOrdersCount()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetPendingOrdersCount();
        }).ConfigureAwait(false);
    }

    public async Task<int> GetCartsCount()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCurrentCartsCount();
        }).ConfigureAwait(false);
    }

    public async Task<int> GetWishlistCount()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetWishlistCount();
        }).ConfigureAwait(false);
    }

    public async Task<int> GetRegisteredCustomersCount()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetRegisteredCustomersCount();
        }).ConfigureAwait(false);
    }

    public async Task<int> GetVendorsCount()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetVendorsCount();
        }).ConfigureAwait(false);
    }

    public async Task<int> GetOnlineCount()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetOnlineCount();
        }).ConfigureAwait(false);
    }

    public async Task<String[]> GetPopularKeywords(int n)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetPopularKeywords(n);
        }).ConfigureAwait(false);
    }

    public async Task<String[]> GetBestsellerByAmount()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetBestsellersByAmount();
        }).ConfigureAwait(false);
    }

    public async Task<String[]> GetBestsellerByQuantity()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetBestsellersByQuantity();
        }).ConfigureAwait(false);
    }

    public async Task<CustomerDTO[]> GetBestCustomers()
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetBestCustomers();
        }).ConfigureAwait(false);
    }

    public string GetCurrency()
    {
        return client.GetCurrency();
    }

    public async Task<CustomerDTO[]> GetCustomersByEmail(string Email)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomersByEmail(Email);
        }).ConfigureAwait(false);
    }

    public async Task<CustomerDTO[]> GetCustomersByUsername(string Username)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomersByUsername(Username);
        }).ConfigureAwait(false);

    }

    public async Task<CustomerDTO[]> GetCustomersByFirstname(string Firstname)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomersByFirstname(Firstname);
        }).ConfigureAwait(false);

    }

    public async Task<CustomerDTO[]> GetCustomersByLastname(string Lastname)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomersByLastname(Lastname);
        }).ConfigureAwait(false);

    }

    public async Task<CustomerDTO[]> GetCustomersByFullname(string Fullname)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomersByFullname(Fullname);
        }).ConfigureAwait(false);

    }

    public async Task<CustomerDTO[]> GetCustomersByCompany(string Company)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomersByCompany(Company);
        }).ConfigureAwait(false);

    }

    public async Task<CustomerDTO[]> GetCustomersByPhone(string Phone)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetCustomersByPhone(Phone);
        }).ConfigureAwait(false);

    }

    public async Task<CustomerDTO[]> GetCustomersByPostalCode(string PostalCode)
    {
        return await Task.Factory.StartNew(() =>
        {
            Int32.Parse(PostalCode);
            return client.GetCustomersByPostalCode(PostalCode);
        }).ConfigureAwait(false);

    }

    public async Task<int> GetCustomerCountByTime(int days)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetRegisteredUsersCountByTime(days);
        }).ConfigureAwait(false);
    }

    public async Task<decimal> GetTotalSalesByTime(int days)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetTotalSalesByTime(days);
        }).ConfigureAwait(false);
    }

    public async Task<decimal> GetPendingOrdersByReason(string reason)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.GetTotalPendingByReason(reason);
        }).ConfigureAwait(false);
    }

    public async Task<CustomerDTO[]> GetCurrentCarts(string Filter, string Email, int LowerItems, int HigherItems, decimal LowerTotal, decimal HigherTotal, bool Abandoned)
    {

        return await Task.Factory.StartNew(() =>
        {
            return client.GetCurrentCarts(Filter, Email, LowerItems, HigherItems, LowerTotal, HigherTotal, false);
        }).ConfigureAwait(false); 
    }

    public async Task<OrderError[]> CancelOrders(int [] id)
    {
        return await Task.Factory.StartNew(() =>
        {
            return client.DeleteOrdersWithoutUser(id);
        }).ConfigureAwait(false); 
        
    }

    public async Task ChangeOrderStatus(int id, string status)
    {
        await Task.Factory.StartNew(() =>
        {
            OrderStatus statusE = OrderStatus.Pending;
            switch(status){
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

             client.ChangeOrderStatus(id, statusE);
        }).ConfigureAwait(false); 
    }

    public async Task ChangePaymentStatus(int id, PaymentStatus status )
    {
        await Task.Factory.StartNew(() =>
        {
            client.ChangePaymentStatus(id,status);
        }).ConfigureAwait(false);
    }

    public async Task ChangeShippingStatus(int id, ShippingStatus status)
    {
        await Task.Factory.StartNew(() =>
        {
            client.ChangeShippingStatus(id, status);
        }).ConfigureAwait(false);
    }

    public async Task AddCommentToCustomer(string email, string comment)
    {
        await Task.Factory.StartNew(() =>
        {
            client.AddCommentToCustomer(email,comment);
        }).ConfigureAwait(false);
    }

}
