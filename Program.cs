

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace DirectoryAnalyzer
{
    class Program
    {
        //Main method to handle all user input file paths and whether or not to include subdirectories
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a directory to be analyzed");
            string PathToAnalyze = Console.ReadLine();
            Console.WriteLine("Enter your output directory (Where your file will be stored)");
            string OutputPath = Console.ReadLine();
            Console.WriteLine("Include subdirectories? (Y/N)");
            string UserInputSubIsAnalyzed = Console.ReadLine();


            if (UserInputSubIsAnalyzed.ToLower() == "y")
            {

                ProcessDirectory(PathToAnalyze, OutputPath, PathToAnalyze);
                ProcessSubDirectories(PathToAnalyze, OutputPath, PathToAnalyze);
            }
            else if (UserInputSubIsAnalyzed.ToLower() == "n")
            {

                ProcessDirectory(PathToAnalyze, OutputPath, PathToAnalyze);

            }
            //If user enters in the same output path and directory to analyze. this will throw an erorr as it is a security/ file corruption error
            else if (OutputPath == PathToAnalyze)
            {
                Console.WriteLine("Security risk, Do not use the same destination path. ");
            }
            else
            {
                Console.WriteLine("Sorry, the input provided is not recognized");

            }

        }
        //Processdirectory handles converting the directory into a string array to process each file individually. 
        // this could be reworked to improve performance and n (files) gets larger.
        public static void ProcessDirectory(string designatedDirectory, string OutputPath, string inputFilePath)
        {


            string[] fileEntries = Directory.GetFiles(designatedDirectory);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName, OutputPath, inputFilePath);
            }
        }
        //ProcessSubDirectories handles navigating to the sub directories of the provided input path. 
        // It uses indirect recursion to call the method ProcessDirectory in order to process the subdirectory files. 
        public static void ProcessSubDirectories(string designatedDirectory, string OutputPath, string inputFilePath)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(designatedDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, OutputPath, inputFilePath);
        }

        //ProcessFile is the heavy lifter method. It determines the magic number/file signature from the pdf or jpeg files and stores it. 
        // it then handles the Md5 hashing of the file contents. Once those are done, it creates the csv file and writes the required variables to the file/
        public static void ProcessFile(string path, string OutputPath, string inputFilePath)
        {
            BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            byte[] data = reader.ReadBytes(0x10);
            string data_as_hex = BitConverter.ToString(data);
            string data_as_hex_stripped = data_as_hex.Replace("-", "");
            string isJPEG = "";
            string isPDF = "";
            if (data_as_hex_stripped.Contains("FFD8"))
            {
                isJPEG = "JPEG";

            }
            else if (data_as_hex_stripped.Contains("25504446"))
            {
                isPDF = "PDF";

            }
            reader.Close();
            //create CSV to write to 
            var csv = new StringBuilder();
            //create MD5 hash and write to the CSV file
            using (var md5Instance = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    var hashResult = md5Instance.ComputeHash(stream);
                    string MD5Hash = BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
                    var filepath = OutputPath + "\\output.csv";
                    var newLine = string.Format("{0},{1},{2},{3}", inputFilePath, isJPEG, isPDF, MD5Hash, Environment.NewLine);
                    csv.AppendLine(newLine);
                    File.AppendAllText(filepath, csv.ToString());
                }
            }
            Console.WriteLine("Processed File '{0}'.", path);

        }
    }
}