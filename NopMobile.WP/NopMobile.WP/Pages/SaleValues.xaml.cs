using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using NopMobile.AppCore.NopAPI;

namespace NopMobile.WP
{
    public partial class SaleValues : PhoneApplicationPage
    {
        NopCore api = new NopCore();
        Settings settings = new Settings();
        private const string SaleVQuantity = "SaleValuesQuantity";
        private const string SaleVAmount = "SaleValuesAmount";
        public SaleValues()
        {
            InitializeComponent();
            var Task = InitializePage();
        }

        private async Task InitializePage()
        {
            var UnpaidVal = await api.GetPendingOrdersByReason("unpaid");
            var NotShippedVal = await api.GetPendingOrdersByReason("not shipped");
            var PendingCountVal = await api.GetPendingOrdersCount();
            var Currency = await api.GetCurrency();

            Unpaid.Text = UnpaidVal.ToString("0.0#") + " " + Currency;
            NotShipped.Text = NotShippedVal.ToString("0.0#") + " " + Currency;
            PendingCount.Text = PendingCountVal.ToString();
  
            var BestsellersQuantityArray = await api.GetBestsellerByQuantity();
            var BestsellersAmountArray = await api.GetBestsellerByAmount();
            var QuantityList = new List <BestsellerData>();
            var BestQuantity = settings.GetValueOrDefault(SaleVQuantity,5);
            var Counter = 0;
            foreach (BestsellerDTO prod in BestsellersQuantityArray)
            {
                if (Counter == BestQuantity) break;
                QuantityList.Add(new BestsellerData { Image = ConvertToBitmapImage(prod.Product.Image.First()), QuantityOrAmount = prod.Quantity.ToString(), ProductName = prod.Product.Name });
                Counter++;
            }
            BestsellersQuantity.ItemsSource = QuantityList;
            LoadingQuantityHolder.Visibility = System.Windows.Visibility.Collapsed;
            var AmountList = new List<BestsellerData>();
            var BestAmount = settings.GetValueOrDefault(SaleVQuantity, 5);
            Counter = 0;
            foreach (BestsellerDTO prod in BestsellersAmountArray)
            {
                if (Counter == BestAmount) break;
                AmountList.Add(new BestsellerData { Image = ConvertToBitmapImage(prod.Product.Image.First()), QuantityOrAmount = prod.Amount.ToString("0.#") + " " + await api.GetCurrency(), ProductName = prod.Product.Name });
                Counter++;
            }
            BestsellersAmount.ItemsSource = AmountList;
            LoadingAmountHolder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private static BitmapImage ConvertToBitmapImage(byte[] image)
        {
            var bitmapImage = new BitmapImage();
            var memoryStream = new MemoryStream(image);
            bitmapImage.SetSource(memoryStream);
            return bitmapImage;
        }

        
    }

    class BestsellerData
    {
        public BitmapImage Image { get; set; }
        public string QuantityOrAmount { get; set; }
        public string ProductName { get; set; }
    }
    
}
