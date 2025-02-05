using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelProcessing
{
    internal class Program
    {
        // 不寫await就是背景處理，且是主程式還必須執行的情況下(主程式處理的時間>背景處理時間)
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            #region Task非同步練習
            //請撰寫三個Task 這三個Task 個分別需要花費 9秒 6秒 3秒 的時間 請思考如何壓縮時間 降低等待




            //Stopwatch stopwatch = new Stopwatch();  
            //stopwatch.Start();

            //// await Task.WhenAll(Task1(), Task2(), Task3());


            //Console.WriteLine("任務三啟動");
            //var task1 = Task3();


            //Console.WriteLine("任務二啟動");
            //var task2 = Task2();


            //Console.WriteLine("任務一啟動");
            //var task3 = Task1();


            //string res3 = await task3;
            //Console.WriteLine(res3);

            //string res2 = await task2;
            //Console.WriteLine(res2);

            //string res1 = await task1;

            //Console.WriteLine(res1);

            ////Thread.Sleep(9000);

            //stopwatch.Stop();
            //Console.WriteLine($"任務完成，總共耗時:{stopwatch.ElapsedMilliseconds/1000}秒");
            #endregion

            //CPU的核心數 跟 執行緒的數量之間的關係是甚麼?
            // 看是什麼樣的核心，效能核心（Performance-cores）支援超執行緒技術（Hyper-Threading），可以處理2 條執行緒，效率核心（Efficient-cores）每個核心處理 1 條執行緒


            //何謂執行緒阻塞? => 執行緒不是開越多越好
            // 
            //最好的效能是: 執行緒的總數量+1

            //把讀取+寫入同時完成四個檔案:500,000  1,000,000   5,000,000  10,000,000
            //執行緒 開50條執行緒(自己去除看每一條要多少批次)  100, 200, 3, 5 , 10,15,16,17,18 ...etc  反覆驗證 在你的電腦上 執行緒具體開多少條效能會是最佳



            string readFilePath = @"C:\CSharp練習\data read\MOCK_DATA (10).csv";
            string baseFolderPath = @"C:\Users\icewi\OneDrive\桌面\DataTest";

            int totalRecords = 10_000_000; // 總資料筆數
            int batchSize = totalRecords / 16; // 每個執行緒負責的筆數
            int threadCount = 16; // 啟動 16 個執行緒

            List<Task> tasks = new List<Task>();
            Stopwatch totalStopwatch = Stopwatch.StartNew();

            double maxReadTime = 0;  // 記錄最大讀取耗時
            double maxWriteTime = 0; // 記錄最大寫入耗時

            object lockObject = new object(); // 用於同步更新最大耗時

            for (int i = 0; i < threadCount; i++)
            {
                int count = i;
                tasks.Add(Task.Run(async () =>
                {
                    Stopwatch readStopwatch = new Stopwatch();
                    Stopwatch writeStopwatch = new Stopwatch();

                    // 計算讀取範圍
                    int startLine = count * batchSize;
                    int linesToRead = Math.Min(batchSize, totalRecords - startLine);

                    Console.WriteLine($"執行緒 {count + 1}/{threadCount} 開始讀取 {startLine} ~ {startLine + linesToRead}");

                    // 計時讀取過程
                    readStopwatch.Start();
                    List<CsvRow> result = CSVHelper.CSV.ReadCSV<CsvRow>(readFilePath, startLine, linesToRead);
                    readStopwatch.Stop();

                    double batchReadTime = readStopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"執行緒 {count + 1}/{threadCount} 結束讀取，資料筆數: {result.Count}，讀取耗時: {batchReadTime:F4} 秒");

                    // 更新最大讀取時間
                    lock (lockObject)
                    {
                        if (batchReadTime > maxReadTime)
                        {
                            maxReadTime = batchReadTime;
                        }
                    }

                    // 模擬寫入過程
                    string outputPath = Path.Combine(baseFolderPath, $"output_{count + 1}.csv");

                    // 計時寫入過程
                    writeStopwatch.Start();
                    CSVHelper.CSV.WriteCSV<CsvRow>(outputPath, result);
                    writeStopwatch.Stop();

                    double batchWriteTime = writeStopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"執行緒 {count + 1}/{threadCount} 完成寫入，寫入耗時: {batchWriteTime:F4} 秒");

                    // 更新最大寫入時間
                    lock (lockObject)
                    {
                        if (batchWriteTime > maxWriteTime)
                        {
                            maxWriteTime = batchWriteTime;
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
            totalStopwatch.Stop();

            Console.WriteLine("程式執行結束");
            Console.WriteLine($"最大讀取耗時: {maxReadTime:F2} 秒");
            Console.WriteLine($"最大寫入耗時: {maxWriteTime:F2} 秒");
            Console.WriteLine($"總共耗時: {totalStopwatch.Elapsed.TotalSeconds:F2} 秒");
            Console.ReadKey();
        }

        // 模擬寫入 CSV 的函式
      
    }


            //double totalReadTime = 0;
            //List<Task> tasks = new List<Task>();    
            //// 逐批讀取資料，拿到每個task的區間
            //for (int i = 0; i < batchCount; i++)
            //{
            //    int count = i;
            //    Task task = Task.Run(() =>
            //    {

            //        Stopwatch sw = new Stopwatch();
            //        sw.Start();  
            //        // 起始行計算
            //        int startLine = count * batchSize;

            //        // 當前批次讀取的行數（確保不超出總行數）
            //        int linesToRead = Math.Min(batchSize, totalRecords - startLine);
            //        Console.WriteLine($"批次 {count + 1}/{batchCount} 開始讀取 {startLine} ~ {startLine + linesToRead}");
            //        // 正確讀取範圍
            //        List<CsvRow> result = CSVHelper.CSV.ReadCSV<CsvRow>(readFilePath, startLine, linesToRead);
            //        Console.WriteLine($"批次 {count + 1}/{batchCount} 結束 資料筆數: {result.Count}");
            //        sw.Stop();
            //        double batchReadTime = sw.Elapsed.TotalSeconds;
            //        Console.WriteLine($"批次 {count + 1}/{batchCount} 讀取花費時間: {batchReadTime:F4} 秒");
            //        GC.Collect();
            //    });

            //    tasks.Add( task );

            //}

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //await Task.WhenAll( tasks );
            //stopwatch.Stop();
            //Console.WriteLine("程式執行結束");
            //Console.WriteLine("總共耗時:"+ stopwatch.Elapsed.TotalSeconds + "秒");
            //Console.ReadKey();

        }


        //private static async Task Task1()
        //{
        //    //await Task.Run(async () =>
        //    //{
        //    //    await Task.Delay(3000);
        //    //});
        //}

        //private static async Task<string> Task2()
        //{
        //    await Task.Run(async () =>
        //    {
        //        await Task.Delay(6000);
        //    });

        //    return "任務二完成";
        //}

        //private static async Task<string> Task3()
        //{
        //    await Task.Run(async () =>
        //    {
        //        await Task.Delay(9000);
        //    });

        //    return "任務三完成";
        //}

        //private static Task<string> Task1()
        //{
        //    Task task = new Task(async () => await Task.Delay(new Random(Guid.NewGuid().GetHashCode()).Next(1, 3000)));
        //    task.Start();
        //    return Task.FromResult("任務一結束");

        //}

        //private static Task<string> Task2()
        //{
        //    Task task = new Task(async () => await Task.Delay(new Random(Guid.NewGuid().GetHashCode()).Next(1, 3000)));
        //    task.Start();
        //    return Task.FromResult("任務二結束");

        //}
        

