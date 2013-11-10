using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HtmlToHamlet
{
    
    public class Program
    {
        private static string outputFolder = "templates";
        private static string tempFolder = "temp";

        public static void Main(string[] args)
        {
            string[] filenameList = args;
            List<string> ignoredFiles = new List<string>();

            if (args.Length == 0)
            {
                Console.WriteLine("No input files.");
                Console.WriteLine("Usage: put the path to the files to convert in argument separated with spaces.");

                char inputChar = ' ';
                while (inputChar != 'y' && inputChar != 'n')
                {
                    Console.WriteLine("\nDo you want to process all the files in the current folder? (y/n)");
                    ConsoleKeyInfo ki = Console.ReadKey();
                    inputChar = Char.ToLower(ki.KeyChar);
                }

                if (inputChar == 'y')
                {
                    filenameList = Directory.GetFiles(Directory.GetCurrentDirectory());
                }
                else if (inputChar == 'n')
                {
                    return;
                }
                
            }


            try
            {
                System.IO.FileInfo tempFolderFile = new System.IO.FileInfo(tempFolder + Path.DirectorySeparatorChar + "whatever");
                tempFolderFile.Directory.Create();
            }
            catch (IOException e)
            {
                Console.WriteLine("## Could not create the temporary folder for conversion. Make sure that you have write access.");
                Console.Write(e.StackTrace + "\n");
                Console.ReadLine(); //Pause
                return;
            }

            try
            {
                System.IO.FileInfo tempFolderFile = new System.IO.FileInfo(outputFolder + Path.DirectorySeparatorChar + "whatever");
                tempFolderFile.Directory.Create();
            }
            catch (IOException e)
            {
                Console.WriteLine("## Error creating the output folder. Make sure that you have write access on your drive.");
                Console.Write(e.StackTrace + "\n");
                Console.ReadLine(); //Pause
                return;
            }

            foreach (string filename in filenameList)
            {
                
                char[] separators = { '.', Path.DirectorySeparatorChar };
                string[] tokens = filename.Split(separators);

                if (tokens.Length<2)
                {
                    ignoredFiles.Add(filename);
                    continue;
                }
                else
                {
                    if (tokens.Last() != "html")
                    {
                        ignoredFiles.Add(filename);
                        continue;
                    }

                    Console.WriteLine("\n\n" + filename + "---------------------------");

                    string newExtension = "hamlet";

                    string filenameWithoutExtention=tokens[tokens.Length-2];
                    string outputFileName = outputFolder + Path.DirectorySeparatorChar + filenameWithoutExtention + "." + newExtension;
                    string tempFilename = tempFolder + Path.DirectorySeparatorChar + filenameWithoutExtention + ".html";
                    

                    StreamReader sr = new StreamReader(filename, Encoding.UTF8);
                    StringBuilder sb = new StringBuilder();
                    
                    while(!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        line = line.Trim();
                        sb.AppendLine(line);
                    }

                    string fullFileContent = sb.ToString();
                    fullFileContent = fullFileContent.Replace("\r\n", "");
                    fullFileContent = fullFileContent.Replace("\n", "");
                    fullFileContent = fullFileContent.Replace("&nbsp;", " #");

                    try
                    {
                        StreamWriter sw = new StreamWriter(new FileStream(tempFilename, FileMode.Create), Encoding.UTF8);
                        sw.Write(fullFileContent);
                        sw.Flush();
                        sw.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("## Error creating the temporary file. Make sure that you have write access on your drive.");
                        Console.Write(e.StackTrace + "\n");
                        continue;
                    }

                    HtmlDocument document = new HtmlDocument();
                    
                    try 
                    {
                        document.Load(tempFilename);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("## Error parsing the HTML file.");
                        Console.Write(e.StackTrace + "\n");
                        continue;
                    }


                    HamletWriter formatter;
                    try
                    {
                        formatter = new HamletWriter(outputFileName, "img", "css", "js");
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine("## Error writing the hamlet file. Make sure you have access on your drive.");
                        Console.Write(e.StackTrace + "\n");
                        continue;
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        Console.WriteLine("## Error writing the hamlet file. Make sure you have access on your drive and that the \"template\" folder exists.");
                        Console.Write(e.StackTrace + "\n");
                        continue;
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("## Error writing the hamlet file. Make sure you have access on your drive.");
                        Console.Write(e.StackTrace + "\n");
                        continue;
                    }
                    catch (System.Security.SecurityException e)
                    {
                        Console.WriteLine("## Error writing the hamlet file. You do not have access to this drive.");
                        Console.Write(e.StackTrace + "\n");
                        continue;
                    }

                    try 
                    {
                        document.DocumentNode.WriteTo(formatter);
                        formatter.Flush();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("## Error creating the the Hamlet file");
                        Console.Write(e.StackTrace + "\n");
                        continue;
                    }
                    Console.WriteLine(filename + " conversion successful.");
                }
            }
            Console.WriteLine("\n\nConversion complete. The converted files are located in the \"templates\" folder.");
            Console.WriteLine("\nThese files were ignored because the extention was not of html type.");
            foreach (string path in ignoredFiles)
            {
                Console.WriteLine("\t " + path);
            }
            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine(); //Pause
        }
    }
}
