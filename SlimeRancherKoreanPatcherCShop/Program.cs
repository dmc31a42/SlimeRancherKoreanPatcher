using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SlimeRancherKoreanPatcherCShop
{
    class Program
    {
        static string currentDirectoryPath;
        static string SlimeRancherPath;
        const string TEMP_FOLDER_NAME = @"temp\";
        const string RESOURCE_FOLDER_PATH = @"Resource\";
        const string SHAREDASSETS0_PATCH_NAME = "sharedassets0_patch";
        const string SHAREDASSETS0_PATCH_PATH = TEMP_FOLDER_NAME + SHAREDASSETS0_PATCH_NAME + @"\";
        const string RESOURCE_PATCH_NAME = "resources_patch";
        const string RESOURCE_PATCH_PATH = TEMP_FOLDER_NAME + RESOURCE_PATCH_NAME + @"\";
        const string GAME_NAME = "Slime Rancher";
        const string GAME_DATA_NAME = "SlimeRancher";
        const string UNITY_RESOURCES_ASSETS_NAME = "resources.assets";
        const string UNITY_SHARED0_ASSETS_NAME = "sharedassets0.assets";
        const bool DEBUG = true;

        static void Main(string[] args)
        {
            SlimeRancherPath = FindUnityFolderPath();
            if (SlimeRancherPath == "ERROR")
            {
                Console.WriteLine("Please Write " + GAME_DATA_NAME + "_Data\\ Folder Full Path and Press ENTER : ");
                string readFolderPath = Console.ReadLine();
                if (readFolderPath.Substring(readFolderPath.Length - 1, 1) != "\\")
                {
                    readFolderPath = readFolderPath + "\\";
                }
                if (FolderExist(readFolderPath) && FileExist(readFolderPath + UNITY_RESOURCES_ASSETS_NAME))
                {
                    SlimeRancherPath = readFolderPath;
                }
            }
            currentDirectoryPath = Directory.GetCurrentDirectory() + @"\";

            DownloadCSV();

            SlimeRancherKoreanPatcherManagedCpp.ManagedPatcher managedPatcher
                = new SlimeRancherKoreanPatcherManagedCpp.ManagedPatcher(SlimeRancherPath, currentDirectoryPath);
            SlimeRancherKoreanPatcherManagedCpp.AssetInfo[] assetInfos = managedPatcher.GetAssetInfos();
            //MakeAssetFile(assetInfos);

        }

        static string FindUnityFolderPath()
        {
            string programFilesPath = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            string programFilesX64Path = "";
            if (programFilesPath.Substring(programFilesPath.Length - 5, 5) == "(x86)")
            {
                programFilesX64Path = programFilesPath.Substring(0, programFilesPath.Length - 6) + @"\";
            }
            programFilesPath = programFilesPath + @"\";
            string steamRelativePath = @"Steam\steamapps\common\" + GAME_NAME + @"\" + GAME_DATA_NAME + @"_Data\";
            string steamLibraryRelativePath = @"SteamLibrary\steamapps\common\" + GAME_NAME + @"\" + GAME_DATA_NAME + @"_Data";
            string[] rootDirectoryPath = new string[26];
            char ch = 'A';
            for (int i = 0; i < rootDirectoryPath.Length; i++)
            {
                rootDirectoryPath[i] = ch + @":\";
                ch++;
            }
            if (FolderExist(GAME_DATA_NAME + @"_Data\"))
            {
                return GAME_DATA_NAME + @"_Data\";
            }
            if (FileExist(UNITY_RESOURCES_ASSETS_NAME))
            {
                return @".\";
            }
            if (FileExist(@"..\" + UNITY_RESOURCES_ASSETS_NAME))
            {
                return @"..\";
            }
            if (programFilesPath != "" && FolderExist(programFilesPath + steamRelativePath))
            {
                return programFilesPath + steamRelativePath;
            }
            if (programFilesX64Path != "" && FolderExist(programFilesX64Path + steamRelativePath))
            {
                return programFilesX64Path + steamRelativePath;
            }
            for (int i = 0; i < rootDirectoryPath.Length; i++)
            {
                if (FolderExist(rootDirectoryPath[i] + steamRelativePath))
                {
                    return rootDirectoryPath[i] + steamRelativePath;
                }
            }
            for (int i = 0; i < rootDirectoryPath.Length; i++)
            {
                if (FolderExist(rootDirectoryPath[i] + steamLibraryRelativePath))
                {
                    return rootDirectoryPath[i] + steamLibraryRelativePath;
                }
            }
            return "ERROR";
        }

        static bool FolderExist(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.Exists;

        }

        static bool FileExist(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            return fi.Exists;
        }

        static void FileDeleteIfExist(string filePath)
        {
            FileInfo fileDel = new FileInfo(filePath);
            if (fileDel.Exists) //삭제할 파일이 있는지
            {
                fileDel.Delete(); //없어도 에러안남
            }
        }

        static void CreateFolderOrClean(string folderName)
        {
            DirectoryInfo di = new DirectoryInfo(folderName);
            if (di.Exists == false)
            {
                di.Create();
            }
            else
            {
                Directory.Delete(folderName, true);
                di.Create();
            }
        }

        static void DownloadCSV()
        {

            string CSVURLString = System.IO.File.ReadAllText(RESOURCE_FOLDER_PATH + @"CSVURL.txt");
            CSVURLString = CSVURLString.Replace("\r", "");
            string[] CSVURL = CSVURLString.Split('\n');
            List<string[]> ListCSVURL = new List<string[]>();
            for (int i = 0; i < CSVURL.Length; i++)
            {
                ListCSVURL.Add(CSVURL[i].Split('\\'));
            }
            CreateFolderOrClean(TEMP_FOLDER_NAME);

            for (int i = 0; i < ListCSVURL.Count; i++)
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(ListCSVURL[i][1], TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + ".csv");
            }
        }

        static void MakeAssetFile(SlimeRancherKoreanPatcherManagedCpp.AssetInfo[] AssetInfos)
        {

        }
    }
}
