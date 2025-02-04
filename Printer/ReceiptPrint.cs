// Copyright © 2018 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using NLog;
using Printer.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace Printer
{
    public class ReceiptPrint : PrintBase
    {
        private List<OrderData> _lstOrderData = null;
        Logger logger = LogManager.GetCurrentClassLogger();
        // TODO: we don't need the orderId paramter, it is here just as an illustration
        public void PrintBill(string printerName, string orderId)
        {
            try
            {
                logger.Info(orderId);
                PrintDocument printDocument = new PrintDocument();
                // Set the printer name
                printDocument.PrinterSettings.PrinterName = printerName;

                // Check if the printer exists
                if (!printDocument.PrinterSettings.IsValid)
                {
                    logger.Error($"The specified printer \"{printerName}\" is not valid.");
                    return;
                }

                string data = getPrintData(orderId);
                if (!string.IsNullOrEmpty(data))
                {
                    ApiResponse response = JsonConvert.DeserializeObject<ApiResponse>(data);
                    if (response.StatusCode == 1)
                    {
                        _lstOrderData = response.Data;
                        printDocument.PrintPage += new PrintPageEventHandler(PrintPageHandler);
                        printDocument.Print();
                    }
                    else
                    {
                        _lstOrderData = null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            try
            {
                if (_lstOrderData != null)
                {
                    Graphics g = e.Graphics;
                    float y = 0;

                    // Load and draw the logo
                    using (Image logoImage = Image.FromFile(@"C:\Users\Sumit Jadhav\Downloads\loremIpsum.png")) // Update to your logo's path
                    {
                        g.DrawImage(logoImage, 0, y, 100, 50);
                        y += 60; // Space for the logo
                    }

                    // Draw Header
                    y += DrawCenteredText(g, y, "SAMCO", new Font("Arial", 16, FontStyle.Bold));
                    y += DrawCenteredText(g, y, "#21, Velacherry Main Road (Opp Phoenix Mall),", new Font("Arial", 10));
                    y += DrawCenteredText(g, y, "Chennai-600042 #044-48565656", new Font("Arial", 10));

                    y += 20; // Add some space
                    logger.Info($"order Id {_lstOrderData[0].OrderId}");
                    // Draw Bill Details
                    y += DrawCenteredText(g, y, "** Bill No : 19 **", new Font("Arial", 14, FontStyle.Bold));
                    y += DrawColumns(g, y, new[] { $"Order ID :{_lstOrderData[0].OrderId}" }, new Font("Arial", 12), "");
                    y += DrawColumns(g, y, new[] { $"Hall :{_lstOrderData[0].Hall} ", $"Table: {_lstOrderData[0].Table}" }, new Font("Arial", 12), "5x5");
                    y += DrawColumns(g, y, new[] { $"Date:{DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss")}" }, new Font("Arial", 12), "");
                    y += 10; // Add some space
                    DrawDashedLine(g, y);
                    y += 5;
                    // Draw Item Header
                    y += DrawColumns(g, y, new[] { "ITEM NAME", "PRICE", "QTY", "TOTAL" }, new Font("Arial", 8, FontStyle.Bold), "5x2x1x2");
                    DrawDashedLine(g, y);
                    y += 5;
                    // Draw Items

                    foreach (var item in _lstOrderData[0].Menus)
                    {
                        string size = item.Size.ToUpper() == "F" ? "" : " Half";
                        y += DrawColumns(g, y, new[] { item.MenuName + size, item.Price.ToString(), item.Quantity, item.TotalPrice.ToString() }, new Font("Arial", 8), "5x2x1x2");
                    }

                    DrawDashedLine(g, y);
                    y += 10; // Add some space

                    // Draw Totals
                    y += DrawColumns(g, y, new[] { "Subtotal", _lstOrderData[0].Subtotal.ToString() }, new Font("Arial", 11), "6x4");
                    y += DrawColumns(g, y, new[] { "CGST (9.00%)", _lstOrderData[0].Tax.ToString() }, new Font("Arial", 11), "6x4");
                    y += DrawColumns(g, y, new[] { "SGST (9.00%)", _lstOrderData[0].Tax.ToString() }, new Font("Arial", 11), "6x4");
                    y += DrawColumns(g, y, new[] { "Total", _lstOrderData[0].Total.ToString() }, new Font("Arial", 14, FontStyle.Bold), "6x4");
                    DrawDashedLine(g, y);
                    y += 10; // Add some space

                    // Draw Footer
                    y += DrawCenteredText(g, y, "Thank you visit again", new Font("Arial", 12));
                    //y += DrawCenteredText(g, y, "We look forward to your next indulgence", new Font("Arial", 10));

                    _lstOrderData = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private float DrawCenteredText(Graphics g, float y, string text, Font font)
        {
            SizeF textSize = g.MeasureString(text, font);
            float x = (g.VisibleClipBounds.Width - textSize.Width) / 2;
            g.DrawString(text, font, Brushes.Black, x, y);
            return textSize.Height + 5; // Add a small margin
        }

        private float DrawColumns(Graphics g, float y, string[] texts, Font font, string col)
        {
            float totalWidth = g.VisibleClipBounds.Width;
            float[] columnWidths = { 1f }; // Proportions for columns
            if (col == "5x5")
            {
                columnWidths = new[] { 0.5f, 0.5f };
            }
            else if (col == "6x4")
            {
                columnWidths = new[] { 0.6f, 0.4f };
            }
            else if (col == "7x3")
            {
                columnWidths = new[] { 0.7f, 0.3f };
            }
            else if (col == "5x2x1x2")
            {
                columnWidths = new[] { 0.5f, 0.2f, 0.1f, 0.2f };
            }
            float x = 0;

            for (int i = 0; i < texts.Length; i++)
            {
                float columnWidth = totalWidth * columnWidths[i];
                logger.Info(columnWidth);
                StringFormat format = new StringFormat();

                if (i == 0)
                {
                    format.Alignment = StringAlignment.Near; // Left-aligned
                    g.DrawString(texts[i], new Font("Arial", 8, FontStyle.Bold), Brushes.Black, new RectangleF(x, y, columnWidth, font.GetHeight(g)), format);
                }
                else if (i == texts.Length - 1)
                {
                    format.Alignment = StringAlignment.Far; // Right-aligned
                    g.DrawString(texts[i], font, Brushes.Black, new RectangleF(x, y, columnWidth, font.GetHeight(g)), format);
                }
                else
                {
                    format.Alignment = StringAlignment.Center; // Center-aligned
                    g.DrawString(texts[i], font, Brushes.Black, new RectangleF(x, y, columnWidth, font.GetHeight(g)), format);
                }
                x += columnWidth;
            }

            return font.GetHeight(g) + 5; // Add a small margin
        }
        private void DrawDashedLine(Graphics g, float y)
        {
            // Create a pen with dashed style
            using (Pen dashedPen = new Pen(Color.Black, 2))
            {
                dashedPen.DashStyle = DashStyle.Dash; // Set the dash style
                float startX = 0; // Starting X position
                float endX = g.VisibleClipBounds.Width; // Ending X position
                g.DrawLine(dashedPen, startX, y, endX, y); // Draw the dashed line
            }
        }

        private string getPrintData(string OrderId)
        {
            try
            {
                var client = new RestClient($"https://localhost:7253/api/AdminAPI/GetPrintBillData/{OrderId}");
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Postman-Token", "1a379443-4b17-4150-b6c4-4e129854b3b5");
                request.AddHeader("Cache-Control", "no-cache");
                var response = client.Execute(request);
                return response.Content;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return "";
            }
        }
    }
}