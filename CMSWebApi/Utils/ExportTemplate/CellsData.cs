using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.Utils.ExportTemplate
{
    public class CellsData
    {
        public int ID;
        public string Name;
        public string Cell;
        public int Merge;
        public int StyleIndex;
        public int StartCol;
    }
    public class DashboardExportData
    {
        public List<List<CellsData>> Cells;
        public string[][] Chart1;
        public string[] Chart2;
        public float GoalValueMax;
        public float GoalValueMin;
        public string Chart1Title;
        public string Chart2Title;
    }
    public class ExampleData
    {
        public List<List<CellsData>> DataCells()
        {
            int ID = 0;

            List<CellsData> row = new List<CellsData>();
            List<List<CellsData>> res = new List<List<CellsData>>();

            CellsData data = new CellsData() { StartCol = 3, ID = ID++, Merge = 4, Name = "National Weekly Report.", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORTNAME };
            row.Add(data);
            res.Add(row);

            row = new List<CellsData>();
            data = new CellsData() { StartCol = 3, ID = ID++, Name = "Week:26", Merge = 3, StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORTWEEK };
            row.Add(data);
            res.Add(row);

            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = 0, Merge = 0, Name = "", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_DEFAULT };
            row.Add(data);
            res.Add(row);

            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = 0, Merge = 0, Name = "", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_DEFAULT };
            row.Add(data);
            res.Add(row);


            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 4, Name = "Metric", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_METRIC_HEADER };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Forcast for", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Actual for", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Week to date", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Period to Date", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Store Goals", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_STOREGOAL_HEADER };
            row.Add(data);
            res.Add(row);


            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 4, Name = "", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_METRIC_HEADER };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Dec 24th 2015", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Dec 24th 2015", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_STOREGOAL_HEADER };
            row.Add(data);
            res.Add(row);
            /*End Row*/



            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 4, Name = "Conversion %", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "88.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);
            res.Add(row);
            /*End Row*/


            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 4, Name = "Dollars Per Opportunity", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);
            res.Add(row);
            /*End Row*/



            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 4, Name = "Opportunities(traffic)", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);
            res.Add(row);
            /*End Row*/

            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = 0, Merge = 0, Name = "", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_DEFAULT };
            row.Add(data);
            res.Add(row);



            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Metric", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_METRIC_HEADER };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "12/01", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "12/02", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "12/03", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "12/04", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "12/05", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "12/06", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "12/07", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_HEADER };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "Week", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_STOREGOAL_HEADER };
            row.Add(data);
            res.Add(row);







            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "DPO", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);

            res.Add(row);
            /*End Row*/


            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Opportunities(traffic)", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);

            res.Add(row);
            /*End Row*/



            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Conversion %", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);

            res.Add(row);
            /*End Row*/



            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Sales", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);

            res.Add(row);
            /*End Row*/



            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Transactions", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);

            res.Add(row);
            /*End Row*/


            /*New Row*/
            row = new List<CellsData>();
            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "AVT", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);


            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            row.Add(data);

            data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            row.Add(data);

            res.Add(row);
            /*End Row*/




            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 3, Name = "Unconverted Pontential Dollar", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID= ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);

            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "DPO", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Opportunities(traffic)", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Conversion %", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Sales", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Transactions", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "AVT", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/




            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Unconverted Pontential Dollar", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "DPO", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Opportunities(traffic)", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Conversion %", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Sales", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Transactions", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "AVT", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/




            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Unconverted Pontential Dollar", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);

            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "DPO", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Opportunities(traffic)", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Conversion %", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Sales", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Transactions", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "AVT", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/




            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Unconverted Pontential Dollar", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/

            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "DPO", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Opportunities(traffic)", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Conversion %", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Sales", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Transactions", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "AVT", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/




            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Unconverted Pontential Dollar", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);

            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "DPO", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Opportunities(traffic)", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Conversion %", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Sales", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/



            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Transactions", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/


            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "AVT", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            ///*End Row*/




            ///*New Row*/
            //row = new List<CellsData>();
            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 3, Name = "Unconverted Pontential Dollar", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);


            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_DATA };
            //row.Add(data);

            //data = new CellsData() { StartCol = 0, ID = ID++, Merge = 2, Name = "0.00%", StyleIndex = (int)DashboardTemplate.STYLE_INDEX.STYLE_INDEX_REPORT_CONTENT_METRIC_LABEL };
            //row.Add(data);

            //res.Add(row);
            /*End Row*/

            return res;

        }
    }


}
