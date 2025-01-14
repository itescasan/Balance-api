using DevExpress.Charts.Native;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Balance_api.Reporte.Contabilidad
{
    public partial class xrpAsientoContable : DevExpress.XtraReports.UI.XtraReport
    {
        int MaxRow = -1;
        public xrpAsientoContable()
        {
            InitializeComponent();
        }

        private void xrTable3_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
             XRTable table = sender as XRTable;  
    
            if (table.Report.CurrentRowIndex == 16)  
                xrTable1.Borders = DevExpress.XtraPrinting.BorderSide.All;  
       
        }

        private void xrpAsientoContable_DataSourceRowChanged(object sender, DataSourceRowEventArgs e)
        {
            
        }

        private void xrLabel18_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            var a = xrLabel18.Text;
        }

        private void xrLabel18_TextChanged(object sender, EventArgs e)
        {
            var a = xrLabel18.Text;
           
        }

        private void xrLabel18_SummaryRowChanged(object sender, EventArgs e)
        {
            var a = xrLabel18.Text;
           
        }

        private void xrLabel18_SummaryGetResult(object sender, SummaryGetResultEventArgs e)
        {
            //MaxRow = (int) e.CalculatedValues.Count;
        }

        private void xrLabel18_SummaryCalculated(object sender, TextFormatEventArgs e)
        {
            MaxRow = (int)e.Value;
            this.parameter1.Value = MaxRow;
        }
    }
}
