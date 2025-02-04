// Copyright © 2018 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NLog;
using System;

namespace Printer
{
    class Program
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            //int val = Convert.ToInt32(Console.ReadLine());
            logger.Info("Print called {a}", args);
            if (args == null || args.Length == 0)
            {
                logger.Info("Order ID is not specified.");
                return;
            }
            logger.Info(args[0]);

            try
            {
                new ReceiptPrint().PrintBill("EPSON TM-T88IV Receipt", args[0].Replace("print://", string.Empty).Replace("/", string.Empty));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}