using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRDynamicMultipleColumn
{
    public partial class Form1 : Form
    {
        const int PAGE_MARGIN = 400;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            //process data
            DataSet data = new DataSet();
            DataTable table = new DataTable("test");
            string[] rowData = new string[] { "line1", "line1\nline2", "line1\nline2\nline3", "line1\nline2\nline3\nline4" };
            if (cbl_col.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please ensure that you have chosen at least 1 item.");
                return;
            }
            foreach (string name in cbl_col.CheckedItems)
            {
                table.Columns.Add(name);
            }
            for (int i = 0; i < rowData.Length; i++)
            {
                table.Rows.Add(rowData[i], rowData[(i + 1) % 4], rowData[(i + 2) % 4], rowData[(i + 3) % 4]);
            }
            data.Tables.Add(table);


            //load rpt
            ReportDocument rpt = new ReportDocument();
            rpt.Load(".\\test.rpt");

            //set data source
            rpt.SetDataSource(data);

            //do setting columns dynamically

            //get page width
            int width = rpt.PrintOptions.PageContentWidth - PAGE_MARGIN;
            //we set each columns' weight to 1 here
            //there are 4 columns
            //so total weight is 4
            int totalWeight = 4;

            //now, we want to remove columns that user isn't select
            //so we get the Enumerator of Section which the Section name is 'Section2' & 'Section3'
            IEnumerator section2Enumerator = rpt.ReportDefinition.Sections["Section2"].ReportObjects.GetEnumerator();
            IEnumerator section3Enumerator = rpt.ReportDefinition.Sections["Section3"].ReportObjects.GetEnumerator();

            //walk through & remove unused columns
            while (section2Enumerator.MoveNext())
            {
                if (section2Enumerator.Current == null) continue;
                //remove ReportObjects if the name equals database(or data definition) column name
                if (!table.Columns.Contains((section2Enumerator.Current as ReportObject).Name.Replace("txt_", "")))
                {
                    CrystalDecisions.ReportAppServer.ReportDefModel.ISCRReportObject obj = rpt.ReportClientDocument.ReportDefController.ReportObjectController
                        .GetReportObjectsByKind(CrystalDecisions.ReportAppServer.ReportDefModel.CrReportObjectKindEnum.crReportObjectKindText)[(section2Enumerator.Current as ReportObject).Name];
                    rpt.ReportClientDocument.ReportDefController.ReportObjectController.Remove(obj);
                }
            }

            //do the same thing with Section3
            while (section3Enumerator.MoveNext())
            {
                if (section3Enumerator.Current == null) continue;
                //remove ReportObjects if the name equals database(or data definition) column name
                if (!table.Columns.Contains((section3Enumerator.Current as ReportObject).Name.Replace("fd_", "")))
                {
                    CrystalDecisions.ReportAppServer.ReportDefModel.ISCRReportObject obj = rpt.ReportClientDocument.ReportDefController.ReportObjectController
                        .GetReportObjectsByKind(CrystalDecisions.ReportAppServer.ReportDefModel.CrReportObjectKindEnum.crReportObjectKindField)[(section3Enumerator.Current as ReportObject).Name];
                    rpt.ReportClientDocument.ReportDefController.ReportObjectController.Remove(obj);
                }
            }

            //if we set border in design tool, assume there is data that has different lines between each other
            //you were found that vertical(left & right) border height is different between each columns
            //to solve the problem, we need to use line object and set 'ExtendToBottomOfSection' to true
            //and because our columns are dynamic, we also need to add line object dynamically

            //get Section3 as ReportDefModel.Section, we're able to add LineObject later
            CrystalDecisions.ReportAppServer.ReportDefModel.Section section3 = rpt.ReportClientDocument.ReportDefController.ReportDefinition.FindSectionByName("Section3");

            //
            int last = 200;
            CrystalDecisions.ReportAppServer.ReportDefModel.LineObject startLine = new CrystalDecisions.ReportAppServer.ReportDefModel.LineObject();
            startLine.LineThickness = 20;
            startLine.LineColor = 0x0;
            startLine.LineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleSingle;
            startLine.EnableExtendToBottomOfSection = true;
            startLine.Left = last;
            startLine.Right = last;
            startLine.Bottom = 254;
            startLine.Top = 0;
            startLine.SectionName = section3.Name;
            startLine.EndSectionName = section3.Name;
            startLine.SectionCode = section3.SectionCode;
            rpt.ReportClientDocument.ReportDefController.ReportObjectController.Add(startLine, section3);

            for (int i = 0; i < table.Columns.Count; i++) {
                int mWidth = width/totalWeight;
                TextObject mTxt = rpt.ReportDefinition.Sections["Section2"].ReportObjects["txt_" + table.Columns[i].ColumnName] as TextObject;
                FieldObject mDt = rpt.ReportDefinition.Sections["Section3"].ReportObjects["fd_" + table.Columns[i].ColumnName] as FieldObject;
                CrystalDecisions.ReportAppServer.ReportDefModel.LineObject mLine = new CrystalDecisions.ReportAppServer.ReportDefModel.LineObject();
                mLine.LineThickness = 20;
                mLine.LineColor = 0x0;
                mLine.LineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleSingle;
                mLine.EnableExtendToBottomOfSection = true;
                mTxt.Left = last;
                mDt.Left = last;
                mTxt.Width = mWidth;
                mDt.Width = mWidth;
                mLine.Left = last + mWidth;
                mLine.Right = last + mWidth;
                mLine.Bottom = 211;
                mLine.Top = 0;
                last = last + mTxt.Width + 45;
                mLine.SectionName = section3.Name;
                mLine.EndSectionName = section3.Name;
                mLine.SectionCode = section3.SectionCode;
                rpt.ReportClientDocument.ReportDefController.ReportObjectController.Add(mLine, section3);
            }
            rpt.Refresh();
            crystalReportViewer1.ReportSource = rpt;
            crystalReportViewer1.Refresh();
        }
    }
}
