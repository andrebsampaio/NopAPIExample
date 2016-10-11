using System;
using System.Collections.Generic;
using System.Text;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Globalization;
using System.Threading.Tasks;

namespace NopMobile.AppCore
{
    public class RegisteredUsersGraph
    {
        public PlotModel MyModel { get; set; }

        private NopCore api = new NopCore();
        int [] registered = new int [4];
        private ColumnSeries ColumnS = new ColumnSeries();
        private bool WindowsPhone;
        public RegisteredUsersGraph(int [] registered, bool WindowsPhone)
        {
            this.WindowsPhone = WindowsPhone;
            var tmp = new PlotModel();
            tmp.Title = "Registered customers by time";
            if (WindowsPhone){
                tmp.TitleColor = OxyColor.Parse("#FFFFFF");
                tmp.TitleFont = "Segoe WP";
                tmp.PlotAreaBackground = OxyColor.Parse("#33000000");
                tmp.PlotAreaBorderColor = OxyColor.Parse("#33000000");
                tmp.LegendTitleColor = OxyColor.Parse("#FFFFFF");
                ColumnS.FillColor = OxyColor.Parse("#FFFFFF");
                tmp.TitleFontSize = 22;
                tmp.TitleFontWeight = 0.5;
            }
            else
            {
                tmp.TitleColor = OxyColor.Parse("#000000");
                tmp.TitleFont = "Roboto";
                tmp.PlotAreaBackground = OxyColor.Parse("#FFFFFF");
                tmp.PlotAreaBorderColor = OxyColor.Parse("#FFFFFF");
                tmp.LegendTitleColor = OxyColor.Parse("#000000");
                ColumnS.FillColor = OxyColor.Parse("#FF4444");
                tmp.TitleFontSize = 16;
                tmp.TitleFontWeight = 0.3;
            }

            
            this.registered = registered;
            
           
            var categoryAxisForDaysAnd = new CategoryAxis()
            {
                
                Position = AxisPosition.Bottom,
                Title = "Days",
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                MaximumPadding = 0.00,
                IsZoomEnabled = false,

                
                TicklineColor = OxyColor.Parse("#000000"),
                TitleFont = "Roboto",
                TitleColor = OxyColor.Parse("#000000"),
                TextColor = OxyColor.Parse("#000000"),
                Font = "Roboto",

                TickStyle = OxyPlot.Axes.TickStyle.None,
                TitleFontSize = 14
            };

            var categoryAxisForDaysWP = new CategoryAxis()
            {
                
                Position = AxisPosition.Bottom,
                Title = "Days",
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                MaximumPadding = 0.00,
                IsZoomEnabled = false,

                TicklineColor = OxyColor.Parse("#FFFFFF"),
                TitleFont = "Segoe WP",
                TitleColor = OxyColor.Parse("#FFFFFF"),
                TextColor = OxyColor.Parse("#FFFFFF"),
                Font = "Segoe WP",

                TickStyle = OxyPlot.Axes.TickStyle.None,
                TitleFontSize = 20
            };

            var valueAxisAnd = new LinearAxis(AxisPosition.Left)
            {
                MinimumPadding = 0,
                MaximumPadding = 0.00,
                AbsoluteMinimum = 0,
               
                Font = "Roboto",
                TicklineColor = OxyColor.Parse("#000000"),
                TextColor = OxyColor.Parse("#000000"),
                

                IsZoomEnabled = false
            };

            var valueAxisWP = new LinearAxis(AxisPosition.Left)
            {
                MinimumPadding = 0,
                MaximumPadding = 0.00,
                AbsoluteMinimum = 0,
                
                Font = "Segoe WP",
                TicklineColor = OxyColor.Parse("#FFFFFF"),
                TextColor = OxyColor.Parse("#FFFFFF"),

                IsZoomEnabled = false
            };

            var SelectedCategory = categoryAxisForDaysWP;
            var SelectedAxis = valueAxisWP;
            
            if (WindowsPhone)
            {
                SelectedCategory = categoryAxisForDaysWP;
                SelectedAxis = valueAxisWP;
            }
            else
            {
                SelectedCategory = categoryAxisForDaysAnd;
                SelectedAxis = valueAxisAnd;
            }
            

            
            

            SelectedCategory.Labels.Add("7");
            SelectedCategory.Labels.Add("14");
            SelectedCategory.Labels.Add("30");
            SelectedCategory.Labels.Add("365");

            for (int i = 0; i < registered.Length; i++)
            {
                ColumnS.Items.Add(new ColumnItem(registered[i], i));
            }

            tmp.Axes.Add(SelectedCategory);
            tmp.Axes.Add(SelectedAxis);

            tmp.Series.Add(ColumnS);
            MyModel = tmp;// this is raising the INotifyPropertyChanged event
        }
    }
}
