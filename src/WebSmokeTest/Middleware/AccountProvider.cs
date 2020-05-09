using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Middleware;
using Middleware.Accounts;
using Middleware.Accounts.Invoices;
using Middleware.Accounts.Orders;

namespace WebSmokeTest.Middleware
{
    public class AccountProvider : IAccountProvider
    {
        public bool AddDeliveryAddress(in long userId, in DeliveryAddress deliveryAddress)
        {
            throw new InvalidOperationException("Delivery address not supported");
        }

        public bool ChangePassword(in long userId, in string newPassword)
        {
            throw new NotImplementedException();
        }

        public bool ConfirmEmailAddress(in long userId, in string confirmationCode)
        {
            throw new InvalidOperationException("Email confirmation not supported");
        }

        public bool ConfirmTelephoneNumber(in long userId, in string confirmationCode)
        {
            throw new InvalidOperationException("Telephone confirmation not supported");
        }

        public bool CreateAccount(in string email, in string firstName, in string surname, in string password, in string telephone, in string businessName, in string addressLine1, in string addressLine2, in string addressLine3, in string city, in string county, in string postcode, in string countryCode, out long userId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteDeliveryAddress(in long userId, in DeliveryAddress deliveryAddress)
        {
            throw new InvalidOperationException("Delivery address not supported");
        }

        public AddressOptions GetAddressOptions(in AddressOption addressOption)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public Address GetBillingAddress(in long userId)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public DeliveryAddress GetDeliveryAddress(in long userId, in int deliveryAddressId)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public List<DeliveryAddress> GetDeliveryAddresses(in long userId)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public MarketingOptions GetMarketingOptions()
        {
            throw new InvalidOperationException("Marketing not supported");
        }

        public Marketing GetMarketingPreferences(in long userId)
        {
            throw new InvalidOperationException("Marketing not supported");
        }

        public bool GetUserAccountDetails(in long userId, out string firstName, out string lastName, out string email, 
            out bool emailConfirmed, out string telephone, out bool telephoneConfirmed)
        {
            throw new NotImplementedException();
        }

        public List<Invoice> InvoicesGet(in long userId)
        {
            throw new InvalidOperationException("Invoices not supported");
        }

        public void OrderPaid(in Order order, in PaymentStatus paymentStatus, in string message)
        {
            throw new InvalidOperationException("Orders not supported");
        }

        public List<Order> OrdersGet(in long userId)
        {
            throw new InvalidOperationException("Orders not supported");
        }

        public bool SetBillingAddress(in long userId, in Address billingAddress)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public bool SetDeliveryAddress(in long userId, in DeliveryAddress deliveryAddress)
        {
            throw new InvalidOperationException("Address not supported");
        }

        public bool SetMarketingPreferences(in long userId, in Marketing marketing)
        {
            throw new InvalidOperationException("Marketing not supported");
        }

        public bool SetUserAccountDetails(in long userId, in string firstName, in string lastName, in string email, in string telephone)
        {
            throw new NotImplementedException();
        }
    }
}
