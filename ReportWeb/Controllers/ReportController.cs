using DevExpress.Web.Mvc;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using ReportWeb.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace ReportWeb.Controllers
{
    public class ReportController : Controller
    {
        CDCNPMEntities db = new CDCNPMEntities();
        XtraReport rep = new XtraReport();
        //string str = "Data Source=DESKTOP-S5NEE3G\\LACNGUYEN;Initial Catalog=CDCNPM;uid=sa;pwd=123456";

        public ActionResult Index()
        {
            Session["str"] = "Data Source=DESKTOP-S5NEE3G\\LACNGUYEN;uid=sa;pwd=123456";
            
            return View();
        }

        public JsonResult getStrConnect(string name)
        {
            db.Configuration.ProxyCreationEnabled = false;
            Session["str"] = "Data Source=DESKTOP-S5NEE3G\\LACNGUYEN;Initial Catalog="+name+";uid=sa;pwd=123456";
            return Json("COMPLETE", JsonRequestBehavior.AllowGet);
        }

        public JsonResult testValid(String testString)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<string> list = new List<string>();
            String mes = "Success!";
            using (SqlConnection con = new SqlConnection((string)Session["str"]))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                
                 using (SqlCommand cmd = new SqlCommand(testString, con))
                   {
                    try
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                list.Add(dr[0].ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mes = e.ToString() ;
                    }
                }
            }
            return Json(mes, JsonRequestBehavior.AllowGet );
        }

        public JsonResult getDatabaseList() 
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<string> list = new List<string>();
            using (SqlConnection con = new SqlConnection((string)Session["str"]))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT name FROM sys.databases WHERE database_id >= 5 AND NOT (name LIKE N'ReportServer%')", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            var selectListDatabase = list.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            selectListDatabase.Insert(0, new SelectListItem() { Value = "", Text = "---DATABASE---" });
            return Json(selectListDatabase, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getTableList()
        {
            db.Configuration.ProxyCreationEnabled = false;
            //List<View_Columns> ColumnList = db.View_Columns.Where(x => x.COLUMN_NAME == TABLE_NAME).ToList();
            List<string> list = new List<string>();
            using (SqlConnection con = new SqlConnection((string)Session["str"]))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME != 'sysdiagrams'", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            var selectListColumn = list.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            selectListColumn.Insert(0, new SelectListItem() { Value = "", Text = "---TABLE---" });
            return Json(selectListColumn, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getColumnList(string TABLE_NAME)
        {
            db.Configuration.ProxyCreationEnabled = false;
            //List<View_Columns> ColumnList = db.View_Columns.Where(x => x.COLUMN_NAME == TABLE_NAME).ToList();
            List<string> list = new List<string>();
            using (SqlConnection con = new SqlConnection((string)Session["str"]))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + TABLE_NAME+"'", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            var selectListColumn = list.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            selectListColumn.Insert(0, new SelectListItem() { Value = "", Text = "---COLUMN---" });
            return Json(selectListColumn, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Index(string Query, string Title, string Type, string Watermark, string Table)
        {
            try
            {
                showReport(Query, Title, Type, Watermark);
                return View("DisplayReport");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult DisplayReport()
        {
            return View();
        }

        public void showReport(String query, String tieude, String chieu, String watermark)
        {
            rep.DataSource = LayDuLieu(query);
            if (rep.DataSource == null)
                 Console.Write("FAIL");

            DataSet r = (DataSet)rep.DataSource;
            rep.DataMember = r.Tables[0].TableName;
            if (chieu == "vertical")//In doc, in ngang
                rep.PaperKind = System.Drawing.Printing.PaperKind.A4;
            else
                rep.PaperKind = System.Drawing.Printing.PaperKind.A4Rotated;
            if (watermark == "text")//WaterMark la chu hay hinh
                SetTextWatermark(rep);
            else
                SetPictureWatermark(rep);

            InitBands(rep);
            InitDetailsBasedonXRTable(rep, tieude);
            if (rep == null) ViewBag.mess = "Report NULL";
            else ViewBag.report = rep;
        }
        public DataSet LayDuLieu(String query)
        {
            DataSet ds = new DataSet();
            using (SqlConnection con = new SqlConnection((string)Session["str"]))
            {
                con.Open();
                SqlDataAdapter adpt = new SqlDataAdapter(query, con);
                adpt.Fill(ds);
                con.Close();
            }
            return ds;
        }
        public void InitBands(XtraReport rep)
        {
            // Create bands
            DetailBand detail = new DetailBand();
            PageHeaderBand pageHeader = new PageHeaderBand();
            ReportHeaderBand reportHeader = new ReportHeaderBand();
            ReportFooterBand reportFooter = new ReportFooterBand();
            reportHeader.Height = 40;
            detail.Height = 20;
            reportFooter.Height = 380;
            pageHeader.Height = 20;
            // Place the bands onto a report
            rep.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] { reportHeader, detail, pageHeader, reportFooter });
        }
        public void InitDetailsBasedonXRTable(XtraReport rep, String txtTieude)
        {
            // String txtTieude = "TIEU DE";

            DataSet ds = (DataSet)rep.DataSource;
            int colCount = ds.Tables[0].Columns.Count;
            int colWidth = System.Convert.ToInt32((rep.PageWidth - (rep.Margins.Left + rep.Margins.Right)) / (double)colCount);
            rep.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);
            XRLabel tieude = new XRLabel();
            tieude.Text = txtTieude;//Tieu De
            tieude.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            tieude.ForeColor = Color.YellowGreen;
            tieude.Font = new Font("Tahoma", 28, FontStyle.Bold, GraphicsUnit.Pixel);
            tieude.Width = Convert.ToInt32(rep.PageWidth - 50);
            // Create a table to represent headers
            XRTable tableHeader = new XRTable();
            tableHeader.Height = 40;
            tableHeader.BackColor = Color.LightSlateGray;
            tableHeader.ForeColor = Color.White;
            tableHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            tableHeader.Font = new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Pixel);
            tableHeader.Width = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right));
            tableHeader.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 5, 5, 100.0F);
            XRTableRow headerRow = new XRTableRow();
            headerRow.Width = tableHeader.Width;
            tableHeader.Rows.Add(headerRow);
            tableHeader.BeginInit();
            // Create a table to display data
            XRTable tableDetail = new XRTable();
            tableDetail.Height = 20;
            tableDetail.Width = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right));
            tableDetail.Font = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Pixel);
            XRTableRow detailRow = new XRTableRow();
            detailRow.Width = tableDetail.Width;
            tableDetail.Rows.Add(detailRow);
            tableDetail.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 5, 5, 100.0F);
            tableDetail.BeginInit();
            // Create table cells, fill the header cells with text, bind the cells to data
            for (int i = 0; i <= colCount - 1; i++)
            {
                XRTableCell headerCell = new XRTableCell();
                headerCell.Text = ds.Tables[0].Columns[i].Caption;
                XRTableCell detailCell = new XRTableCell();
                detailCell.DataBindings.Add("Text", null/* TODO Change to default(_) if this is not a reference type */, ds.Tables[0].Columns[i].Caption);
                if (i == 0)
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                    detailCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                }
                else
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                    detailCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                }
                if (i == 0)
                {
                    headerCell.Width = 50;
                    detailCell.Width = 50;
                    detailCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
                }
                else if (i == 1)
                {
                    headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                    headerCell.Width = 130;
                    detailCell.Width = 130;
                }
                else if (i == 2)
                {
                    headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                    headerCell.Width = 70;
                    detailCell.Width = 70;
                }
                else if (i == 4)
                {
                    headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                    headerCell.Width = 145;
                    detailCell.Width = 145;
                }
                else
                {
                    headerCell.Width = colWidth;
                    detailCell.Width = colWidth;
                }
                detailCell.Borders = DevExpress.XtraPrinting.BorderSide.Bottom | DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right;

                // Place the cells into the corresponding tables
                headerRow.Cells.Add(headerCell);
                detailRow.Cells.Add(detailCell);
            }
            tableHeader.EndInit();
            tableDetail.EndInit();
            // Place the table onto a report's Detail band
            rep.Bands[BandKind.ReportHeader].Controls.Add(tieude);
            rep.Bands[BandKind.PageHeader].Controls.Add(tableHeader);
            rep.Bands[BandKind.Detail].Controls.Add(tableDetail);
        }
        public void SetTextWatermark(XtraReport ps)
        {
            // Create the text watermark.
            Watermark textWatermark = new Watermark();

            // Set watermark options.
            textWatermark.Text = "PTITHCM";
            textWatermark.TextDirection = DirectionMode.ForwardDiagonal;
            textWatermark.Font = new Font(textWatermark.Font.FontFamily, 40);
            textWatermark.ForeColor = Color.DodgerBlue;
            textWatermark.TextTransparency = 150;
            textWatermark.ShowBehind = false;
            textWatermark.PageRange = "1,3-5";

            // Add the watermark to a document.
            ps.Watermark.CopyFrom(textWatermark);
        }

        public void SetPictureWatermark(XtraReport ps)
        {
            // Create the picture watermark.
            Watermark pictureWatermark = new Watermark();

            // Set watermark options.
            pictureWatermark.Image = Image.FromFile("C:\\Users\\nguye\\OneDrive\\Documents\\Visual Studio 2015\\Projects\\ReportWeb-main\\ReportWeb\\logo.png");
            pictureWatermark.ImageAlign = ContentAlignment.MiddleCenter;
            pictureWatermark.ImageTiling = false;
            pictureWatermark.ImageViewMode = ImageViewMode.Zoom;
            pictureWatermark.ImageTransparency = 150;
            pictureWatermark.ShowBehind = true;
            pictureWatermark.PageRange = "1,3-5";

            // Add the watermark to a document.
            ps.Watermark.CopyFrom(pictureWatermark);
        }
    }
}