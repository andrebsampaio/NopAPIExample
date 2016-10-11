using System;
using System.Collections.Generic;
using System.Text;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Globalization;
using NopMobile.Core.NopAPI;
using System.Threading.Tasks;

namespace NopMobile.Core
{
    public class RegisteredUsersGraph
    {
        public PlotModel MyModel { get; set; }

        private NopCore api = new NopCore();
        int [] registered = new int [4];
        private ColumnSeries ColumnS = new ColumnSeries();
        public RegisteredUsersGraph(int [] registered)
        {
            var tmp = new PlotModel();
            tmp.LegendTitle = "Registered Users by Time";
            tmp.LegendPlacement = LegendPlacement.Outside;
            tmp.LegendMargin = 1;
            tmp.LegendPadding = 1;
            tmp.LegendPosition = LegendPosition.TopCenter;
            this.registered = registered;
            ColumnS.FillColor = OxyColor.Parse("#FF4444");
            var categoryAxisForDays = new CategoryAxis()
            {
                
                Position = AxisPosition.Bottom,
                Title = "Days",
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                MaximumPadding = 0.00
            };

            var valueAxis = new LinearAxis(AxisPosition.Left)
            {
                MinimumPadding = 0,
                MaximumPadding = 0.00,
                AbsoluteMinimum = 0
            };


            categoryAxisForDays.Labels.Add("7");
            categoryAxisForDays.Labels.Add("14");
            categoryAxisForDays.Labels.Add("30");
            categoryAxisForDays.Labels.Add("365");

            for (int i = 0; i < registered.Length; i++)
            {
                ColumnS.Items.Add(new ColumnItem(registered[i], i));
            }

            tmp.PlotAreaBorderThickness = 0;

            tmp.Axes.Add(categoryAxisForDays);
            tmp.Axes.Add(valueAxis);

            tmp.Series.Add(ColumnS);
            MyModel = tmp;// this is raising the INotifyPropertyChanged event
        }
    }
}
