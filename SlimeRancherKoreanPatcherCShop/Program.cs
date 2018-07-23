using SlimeRancherKoreanPatcherManagedCpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        const string currentVersion = "20180101";
        const string currentVersionURL = @"https://github.com/dmc31a42/SlimeRancherKoreanPatcher/raw/ModifyParserAndCSV2Zanata/currentVersion.txt";

        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    {
                        Console.WriteLine(GAME_NAME + " 게임 폴더 위치 찾는 중...");
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
                        Console.WriteLine("임시폴더 비우는 중...");
                        if (CreateFolderOrClean(TEMP_FOLDER_NAME) == false)
                        {
                            Console.WriteLine(TEMP_FOLDER_NAME + " 폴더를 삭제할 수 없습니다.");
                            Console.WriteLine("직접 삭제하신 후 다시 실행하여주세요");
                            Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                            Console.Read();
                        }
                        Console.WriteLine("온라인상에서 최신버전이 있는지 확인하는 중...");
                        if (CheckLastVersion() == false)
                        {
                            Console.WriteLine("온라인상에 업데이트된 최신버전이 존재합니다.");
                            Console.Write("그래도 계속 진행하겠습니까?(Y/n) : ");
                            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                            if (consoleKeyInfo.KeyChar == 'Y')
                            {
                                Console.WriteLine("");
                            }
                            else
                            {
                                Console.WriteLine("\n인터넷에서 새로운 패치를 다운받아주세요. 패치를 중단합니다.");
                                Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                                Console.Read();
                                return;
                            }
                        }
                        Console.WriteLine("에셋 정보 추출 중...");
                        SlimeRancherKoreanPatcherManagedCpp.ManagedPatcher managedPatcher
                            = new SlimeRancherKoreanPatcherManagedCpp.ManagedPatcher(SlimeRancherPath, currentDirectoryPath);
                        SlimeRancherKoreanPatcherManagedCpp.AssetInfo[] assetInfos = managedPatcher.GetAssetInfos();
                        Console.WriteLine("번역된 문장 다운로드 및 적용 중...");
                        DownloadCSVandPatch2();
                        Console.WriteLine("패치 파일 생성 중...");
                        MakeAssetFile2(assetInfos);
                        Console.WriteLine("패치된 유니티 에셋 생성 중...");
                        managedPatcher.MakeModdedAssets();
                        managedPatcher.Dispose();
                        Console.WriteLine("유니티 에셋 원본과 패치된 파일 교체 중...");
                        SwitchFile();
                        Console.WriteLine("한글 폰트 이미지 파일 복사 중...");
                        File.Copy(RESOURCE_FOLDER_PATH + @"koreanAtla2.assets.resS", SlimeRancherPath + @"koreanAtla2.assets.resS", true);
                        Console.WriteLine("임시로 생성된 파일 삭제 중...");
                        DeleteFolder(currentDirectoryPath + TEMP_FOLDER_NAME);
                        Console.WriteLine(GAME_NAME + " 한글패치 완료!");
                        Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                        Console.Read();
                    }
                    break;
                case 1:
                    {
                        switch (args[0])
                        {
                            case "-Update":
                                Console.WriteLine(GAME_NAME + " 게임 폴더 위치 찾는 중...");
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
                                Console.WriteLine("임시폴더 비우는중...");
                                if (CreateFolderOrClean(TEMP_FOLDER_NAME) == false)
                                {
                                    Console.WriteLine(TEMP_FOLDER_NAME + " 폴더를 삭제할 수 없습니다.");
                                    Console.WriteLine("직접 삭제하신 후 다시 실행하여주세요");
                                    Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                                    Console.Read();
                                }
                                Console.WriteLine("온라인상에서 최신버전이 있는지 확인하는 중...");
                                if (CheckLastVersion() == false)
                                {
                                    Console.WriteLine("온라인상에 업데이트된 최신버전이 존재합니다.");
                                    Console.Write("그래도 계속 진행하겠습니까?(Y/n) : ");
                                    ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                                    if (consoleKeyInfo.KeyChar == 'Y')
                                    {
                                        Console.WriteLine("");
                                    }
                                    else
                                    {
                                        Console.WriteLine("\n인터넷에서 새로운 패치를 다운받아주세요. 패치를 중단합니다.");
                                        Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                                        Console.Read();
                                        return;
                                    }
                                }
                                Console.WriteLine("에셋 정보 추출중...");
                                SlimeRancherKoreanPatcherManagedCpp.ManagedPatcher managedPatcher
                                    = new SlimeRancherKoreanPatcherManagedCpp.ManagedPatcher(SlimeRancherPath, currentDirectoryPath);
                                SlimeRancherKoreanPatcherManagedCpp.AssetInfo[] assetInfos = managedPatcher.GetAssetInfos();
                                Console.WriteLine("번역된 문장 다운로드 및 언어 파일 업데이트 중...");
                                DownloadCSVandUpdate2();
                                Console.WriteLine("프로그램이 종료되면 임시로 생성된 파일들이 지워집니다.");
                                Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                                Console.Read();
                                Console.WriteLine("임시로 생성된 파일 삭제 중...");
                                DeleteFolder(currentDirectoryPath + TEMP_FOLDER_NAME);
                                break;
                        }
                    }
                    break;
            }
        }

        static void SwitchFile()
        {
            FileDeleteIfExist(SlimeRancherPath + UNITY_RESOURCES_ASSETS_NAME);
            FileDeleteIfExist(SlimeRancherPath + UNITY_SHARED0_ASSETS_NAME);
            System.IO.File.Move(SlimeRancherPath + UNITY_RESOURCES_ASSETS_NAME + ".modded", SlimeRancherPath + UNITY_RESOURCES_ASSETS_NAME);
            System.IO.File.Move(SlimeRancherPath + UNITY_SHARED0_ASSETS_NAME + ".modded", SlimeRancherPath + UNITY_SHARED0_ASSETS_NAME);
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
            string steamLibraryRelativePath = @"SteamLibrary\steamapps\common\" + GAME_NAME + @"\" + GAME_DATA_NAME + @"_Data\";
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
            if (FolderExist(@"..\" + GAME_DATA_NAME + @"_Data\"))
            {
                return @"..\" + GAME_DATA_NAME + @"_Data\";
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

        static bool CreateFolderOrClean(string folderName, int count=10)
        {
            DirectoryInfo di = new DirectoryInfo(folderName);
            if(count==0)
            {
                return false;
            }
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }
            else
            {
                try
                {
                    Directory.Delete(folderName, true);
                }
                catch
                {
                    Console.WriteLine(folderName + " 폴더 삭제 시도 " + count + "번 남음");
                }
                return CreateFolderOrClean(folderName, count-1);
            }
        }

        static void DeleteFolder(string folderName)
        {
            DirectoryInfo di = new DirectoryInfo(folderName);
            if (di.Exists == true)
            {
                Directory.Delete(folderName, true);
            }
        }

        static bool CheckLastVersion()
        {
            string currentVersionFilePath = TEMP_FOLDER_NAME + "currentVersion.txt";
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile(currentVersionURL, currentVersionFilePath);
                string onlineVersion = System.IO.File.ReadAllText(currentVersionFilePath);
                if(onlineVersion == currentVersion)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("최신버전을 확인하는 도중 오류가 발생하였습니다. 현재 버전으로 패치를 계속합니다.");
                return true;
            }
        }

        static void DownloadCSVandPatch2()
        {
            List<string> ListName = DownloadWebFileOrCopyOfflineFile(RESOURCE_FOLDER_PATH + @"CSVURL2.txt");
            foreach (string name in ListName)
            {
                PatchText2(TEMP_FOLDER_NAME + "\\" + name + ".txt", TEMP_FOLDER_NAME + "\\" + name + ".po");
            }
        }

        static void DownloadCSVandUpdate2()
        {
            List<string> ListName = DownloadWebFileOrCopyOfflineFile(RESOURCE_FOLDER_PATH + @"CSVURL2.txt");
            foreach (string name in ListName)
            {
                UpdateText2(TEMP_FOLDER_NAME + "\\" + name + ".txt", TEMP_FOLDER_NAME + "\\" + name + ".po", TEMP_FOLDER_NAME + "\\" + name + ".pot");
            }
        }

        static List<string> DownloadWebFileOrCopyOfflineFile(string URLListTXTPath)
        {
            string CSVURLString = System.IO.File.ReadAllText(URLListTXTPath);
            CSVURLString = CSVURLString.Replace("\r", "");
            string[] CSVURL = CSVURLString.Split('\n');
            List<string[]> ListFileNameAndURL = new List<string[]>();
            List<string> ListFileName = new List<string>();
            List<string> ListFileURL = new List<string>();
            for (int i = 0; i < CSVURL.Length; i++)
            {
                string[] tempNAMEURL = CSVURL[i].Split('\\');
                ListFileNameAndURL.Add(tempNAMEURL);
                ListFileName.Add(tempNAMEURL[0]);
                ListFileURL.Add(tempNAMEURL[1]);  
            }
            foreach (string[] NameAndURL in ListFileNameAndURL)
            {
                DownloadWebFileOrCopyOfflineFile(NameAndURL[0], NameAndURL[1], ".po");
            }
            return ListFileName;
        }
        static void DownloadWebFileOrCopyOfflineFile(string name, string url, string extension)
        {
            if(extension != string.Empty & extension[0] != '.')
            {
                extension = "." + extension;
            }
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile(url, TEMP_FOLDER_NAME + "\\" + name + extension);
            }
            catch
            {
                Console.WriteLine(name + "      \t번역 파일을 받는 중 오류가 발생하였습니다. 오프라인 파일을 사용합니다.");
                File.Copy(RESOURCE_FOLDER_PATH + name + ".po", TEMP_FOLDER_NAME + name + extension, true);
            }
        }

        static void UpdateText2(string originalTXTPath, string translatedPoPath, string newTranslatedPotPath)
        {
            Dictionary<string, string> translatedPoDictionary = LoadFromPo(translatedPoPath);
            Dictionary<string, string> originalDictionary = LoadFromTextFile(originalTXTPath);
            WriteToPo(newTranslatedPotPath, originalDictionary, translatedPoDictionary);
        }

        static void PatchText2(string originalTXTPath, string translatedPoPath)
        {
            Dictionary<string, string> translatedPoDictionary = LoadFromPo(translatedPoPath);
            Dictionary<string, string> originalDictionary = LoadFromTextFile(originalTXTPath);
            WriteToText(originalTXTPath, originalDictionary, translatedPoDictionary);
        }

        static void WriteToPo(string newTranslatedPotPath, Dictionary<string, string> originalDictionary)
        {
            WriteToPo(newTranslatedPotPath, originalDictionary, new Dictionary<string, string>());
        }
        static void WriteToPo(string newTranslatedPotPath, Dictionary<string, string> originalDictionary, Dictionary<string, string> translatedDictionary)
        {
            Dictionary<string, string[]> jointDictionary = JointDictionary(originalDictionary, translatedDictionary);
            WriteToPo(newTranslatedPotPath, jointDictionary);
        }
        static void WriteToPo(string newTranslatedPotPath, Dictionary<string, string[]> jointDictionary)
        {
            int i = 1;
            string potString = "";
            foreach(KeyValuePair<string, string[]> keyValue in jointDictionary)
            {
                if (keyValue.Value[0] != string.Empty)
                {
                    potString += "#: " + i++ + "\n" +
                        "msgctxt \"" + keyValue.Key +
                        "\"\nmsgid \"" +
                        keyValue.Value[0].Replace("=", @"\=").Replace("\n", @"\n").Replace("\x00ad", @"\u00AD").Trim() +
                        "\"\n" +
                        "msgstr \"" +
                        keyValue.Value[1].Replace("=", @"\=").Replace("\n", @"\n").Replace("\x00ad", @"\u00AD").Trim() +
                        "\"\n\n";
                }
            }
            System.IO.File.WriteAllText(newTranslatedPotPath, potString);
            System.IO.File.WriteAllText(newTranslatedPotPath.Replace(".pot", "_updated.po"), potString);
        }

        static void WriteToText(string originalTXTPath, Dictionary<string, string> originalDictionary, Dictionary<string, string> translatedDictionary)
        {
            Dictionary<string, string> patchedDictionary = MergeDictionary(originalDictionary, translatedDictionary);
            WriteToText(originalTXTPath, patchedDictionary);
        }
        static void WriteToText(string originalTXTPath, Dictionary<string, string> patchedDictionary)
        {
            string outputTextString = "";
            foreach (KeyValuePair<string, string> keyValue in patchedDictionary)
            {
                outputTextString += keyValue.Key + " = " + keyValue.Value.Replace("\n", "\\n") + "\n";
            }
            System.IO.File.WriteAllText(originalTXTPath, outputTextString);
        }

        static Dictionary<string, string> LoadFromTextFile(string TextPath)
        {
            string originalText = System.IO.File.ReadAllText(TextPath);
            List<string> list = new List<string>();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            char[] separator = new char[] { '\n' };
            foreach (string str in Regex.Replace(originalText, @"\\(\r\n|\n|\r)[ \t]*", string.Empty).Split(separator))
            {
                list.Add(str);
            }
            if (list == null)
            {
                Console.WriteLine("Resource is empty. '" + TextPath + "'");
                return new Dictionary<string, string>();
            }
            foreach (string str2 in list)
            {
                if ((str2.Length > 1) && (str2[0] != '#'))
                {
                    string[] strArray3 = Regex.Split(str2, @"(?<!(?<!\\)*\\)\=");
                    if (strArray3.Length != 2)
                    {
                        object[] args = new object[] { "path", TextPath, "line", str2 };
                        //Console.WriteLine("Illegal resource bundle line", args);
                    }
                    else
                    {
                        dictionary[strArray3[0].Replace(@"\=", "=").Trim()] = strArray3[1].Replace(@"\=", "=").Replace(@"\n", "\n").Replace(@"\u00AD", "\x00ad").Trim();
                    }
                }
            }
            return dictionary;
        }

        static Dictionary<string, string> LoadFromPo(string translatedPoPath)
        {
            Dictionary<string, string> translatedPoDictionary = new Dictionary<string, string>();
            string translatedPoString = System.IO.File.ReadAllText(translatedPoPath);
            // https://regexr.com/3hnbt
            MatchCollection translatedPoMatchCollection = Regex.Matches(translatedPoString, "msgctxt \"(.*)\"\\nmsgid ([\\s\\S]*?)\\nmsgstr ([\\s\\S]+?)\"\\n\\n");
            foreach (Match match in translatedPoMatchCollection)
            {
                string key = match.Groups[1].Value.Replace("\"", string.Empty);
                string value = match.Groups[3].Value;
                value = Regex.Replace(value, "^\"|\"\n\"|\"$", string.Empty);
                value = value.Trim().Replace("\\n", "\n").Trim();
                translatedPoDictionary.Add(key, value);
            }
            return translatedPoDictionary;
        }

        static Dictionary<string, string> MergeDictionary(Dictionary<string, string> originalDictionary, Dictionary<string, string> translatedDictionary)
        {
            Dictionary<string, string> patchedDictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> keyValue in originalDictionary)
            {
                string translatedText = "";
                if (translatedDictionary.TryGetValue(keyValue.Key, out translatedText) && translatedText != string.Empty)
                {
                    patchedDictionary.Add(keyValue.Key, translatedText);
                }
                else
                {
                    patchedDictionary.Add(keyValue.Key, keyValue.Value);
                }
            }
            return patchedDictionary;
        }

        static Dictionary<string, string[]> JointDictionary(Dictionary<string, string> originalDictionary, Dictionary<string, string> translatedDictionary)
        {
            Dictionary<string, string[]> patchedDictionary = new Dictionary<string, string[]>();
            foreach (KeyValuePair<string, string> keyValue in originalDictionary)
            {
                string translatedText = "";
                if (translatedDictionary.TryGetValue(keyValue.Key, out translatedText) && translatedText != string.Empty)
                {
                    patchedDictionary.Add(keyValue.Key, new string[] { keyValue.Value, translatedText });
                }
                else
                {
                    patchedDictionary.Add(keyValue.Key, new string[] { keyValue.Value, "" });
                }
            }
            return patchedDictionary;
        }

        static void MakeAssetFile2(SlimeRancherKoreanPatcherManagedCpp.AssetInfo[] AssetInfos)
        {
            string shaderSuffix = "_Shader";
            string atlasSuffix = "_Atlas";

            string[] languageDataNames =
            {
                "achieve",
                "actor",
                "exchange",
                "global",
                "keys",
                "mail",
                "pedia",
                "range",
                "tutorial",
                "ui"
            };

            string[] materialNames =
            {
                "OpenSans-Semibold SDF Material"
            };

            string[] monoBehaviourNames =
            {
                "MonoBehaviour OpenSans SDF"
            };

            CreateFolderOrClean(SHAREDASSETS0_PATCH_PATH);
            List<string> shared0PatchList = new List<string>();
            // Material
            for (int i = 0; i < materialNames.Length; i++)
            {
                AssetInfo materialInfo = Array.Find(AssetInfos, x => x.name == materialNames[i]);
                File.Copy(RESOURCE_FOLDER_PATH + materialNames[i] + @"2.dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat", true);
                shared0PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat");
                using (FileStream fsMaterial = new FileStream(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat", FileMode.Open, FileAccess.ReadWrite))
                {
                    // Shader
                    fsMaterial.Seek(0x00000028, SeekOrigin.Begin);
                    AssetInfo shaderInfo = Array.Find(AssetInfos, x => x.name == materialNames[i] + shaderSuffix);
                    byte[] byteShaderPathID = BitConverter.GetBytes(shaderInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMaterial.WriteByte(byteShaderPathID[j]);
                    }

                    // Atlas
                    //fsMaterial.Seek(0x000000D4, SeekOrigin.Begin);
                    fsMaterial.Seek(0x0000005C, SeekOrigin.Begin);
                    AssetInfo atlasInfo = Array.Find(AssetInfos, x => x.name == materialNames[i] + atlasSuffix);
                    byte[] byteAtlasPathID = BitConverter.GetBytes(atlasInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMaterial.WriteByte(byteAtlasPathID[j]);
                    }
                    File.Copy(RESOURCE_FOLDER_PATH + materialNames[i] + atlasSuffix + @"2.dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + atlasInfo.pathID + ".dat", true);
                    shared0PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + atlasInfo.pathID + ".dat");
                }
            }

            //MonoBehaviour
            for (int i = 0; i < monoBehaviourNames.Length; i++)
            {
                AssetInfo mono2Info = Array.Find(AssetInfos, x => x.name == monoBehaviourNames[i]);
                File.Copy(RESOURCE_FOLDER_PATH + monoBehaviourNames[i] + @"2.dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat", true);
                shared0PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat");
                using (FileStream fsMono2 = new FileStream(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat", FileMode.Open, FileAccess.ReadWrite))
                {

                    // TMP_FontAsset
                    fsMono2.Seek(0x00000014, SeekOrigin.Begin);
                    AssetInfo TMP_FontAssetInfo = Array.Find(AssetInfos, x => x.name == "TMP_FontAsset");
                    byte[] byteTMP_FontAssetPathID = BitConverter.GetBytes(TMP_FontAssetInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteTMP_FontAssetPathID[j]);
                    }

                    // Material
                    fsMono2.Seek(0x00000034, SeekOrigin.Begin);
                    string temp = materialNames[0];
                    AssetInfo materialInfo = Array.Find(AssetInfos, x => x.name == temp);
                    byte[] byteMaterialPathID = BitConverter.GetBytes(materialInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteMaterialPathID[j]);
                    }

                    // Atlas
                    fsMono2.Seek(0x000000A8, SeekOrigin.Begin);
                    AssetInfo atlasInfo = Array.Find(AssetInfos, x => x.name == temp + atlasSuffix);
                    byte[] byteAtlasPathID = BitConverter.GetBytes(atlasInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteAtlasPathID[j]);
                    }

                    // MonoBehaviour OpenSans SDF Cyrillic
                    fsMono2.Seek(0x00019434, SeekOrigin.Begin);
                    AssetInfo cyrillicInfo = Array.Find(AssetInfos, x => x.name == "MonoBehaviour OpenSans SDF Cyrillic");
                    byte[] byteCyrillicPathID = BitConverter.GetBytes(cyrillicInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteCyrillicPathID[j]);
                    }

                    // MonoBehaviour FLOPDesignFont SDF
                    fsMono2.Seek(0x00019440, SeekOrigin.Begin);
                    AssetInfo FLOPDesignFontInfo = Array.Find(AssetInfos, x => x.name == "MonoBehaviour FLOPDesignFont SDF");
                    byte[] byteFLOPDesignFontPathID = BitConverter.GetBytes(FLOPDesignFontInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteFLOPDesignFontPathID[j]);
                    }

                    // NotoSansCJKsc-Regular SDF Chinese
                    fsMono2.Seek(0x0001944C, SeekOrigin.Begin);
                    AssetInfo notoSansCJKscInfo = Array.Find(AssetInfos, x => x.name == "MonoBehaviour NotoSansCJKsc-Regular SDF Chinese");
                    byte[] byteNotoSansCJKscPathID = BitConverter.GetBytes(notoSansCJKscInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteNotoSansCJKscPathID[j]);
                    }

                    // MonoBehaviour OpenSans SDF Small
                    fsMono2.Seek(0x00019460, SeekOrigin.Begin);
                    AssetInfo smallInfo = Array.Find(AssetInfos, x => x.name == "MonoBehaviour OpenSans SDF Small");
                    byte[] byteSmallPathID = BitConverter.GetBytes(smallInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteSmallPathID[j]);
                    }

                    // MonoBehaviour OpenSans SDF Small
                    fsMono2.Seek(0x00019470, SeekOrigin.Begin);
                    AssetInfo small2Info = Array.Find(AssetInfos, x => x.name == "MonoBehaviour OpenSans SDF Small");
                    byte[] byteSmall2PathID = BitConverter.GetBytes(small2Info.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteSmall2PathID[j]);
                    }
                }
            }

            using (StreamWriter swPatchList = new StreamWriter(SHAREDASSETS0_PATCH_PATH + SHAREDASSETS0_PATCH_NAME + "_list.txt"))
            {
                for (int i = 0; i < shared0PatchList.Count; i++)
                {
                    if (i != shared0PatchList.Count - 1)
                    {
                        swPatchList.WriteLine(shared0PatchList[i]);
                    }
                    else
                    {
                        swPatchList.Write(shared0PatchList[i]);
                    }
                }
            }

            //res
            List<string> resPatchList = new List<string>();
            CreateFolderOrClean(RESOURCE_PATCH_PATH);
            for (int i = 0; i < languageDataNames.Length; i++)
            {
                AssetInfo tempLanguageDataInfo = Array.Find(AssetInfos, x => x.name == languageDataNames[i]);
                //File.Copy(RESOURCE_FOLDER_PATH + languageDataNames[i] + @".txt", RESOURCE_PATCH_PATH + "Raw_0_" + tempLanguageDataInfo.pathID + ".dat", true);
                resPatchList.Add(RESOURCE_PATCH_PATH + "Raw_0_" + tempLanguageDataInfo.pathID + ".dat");
                using (FileStream fsTempLang = new FileStream(RESOURCE_PATCH_PATH + "Raw_0_" + tempLanguageDataInfo.pathID + ".dat", FileMode.Create, FileAccess.ReadWrite))
                {
                    byte[] languageDataNameSize = BitConverter.GetBytes(languageDataNames[i].Length);
                    for (int j = 0; j < 4; j++)
                    {
                        fsTempLang.WriteByte(languageDataNameSize[j]);
                    }
                    for (int j = 0; j < languageDataNames[i].Length; j++)
                    {
                        fsTempLang.WriteByte((byte)languageDataNames[i][j]);
                    }
                    while (fsTempLang.Length % 4 != 0)
                    {
                        fsTempLang.WriteByte((byte)0);
                    }
                    using (FileStream fsTxt = new FileStream(TEMP_FOLDER_NAME + languageDataNames[i] + @".txt", FileMode.Open, FileAccess.Read))
                    {
                        byte[] fsTxtSize = BitConverter.GetBytes(fsTxt.Length);
                        for (int j = 0; j < 4; j++)
                        {
                            fsTempLang.WriteByte(fsTxtSize[j]);
                        }
                        for (int j = 0; j < fsTxt.Length; j++)
                        {
                            fsTempLang.WriteByte((byte)fsTxt.ReadByte());
                        }
                    }
                    while (fsTempLang.Length % 4 != 0)
                    {
                        fsTempLang.WriteByte((byte)0);
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        fsTempLang.WriteByte((byte)0);
                    }
                }
            }
            using (StreamWriter swResPatchList = new StreamWriter(RESOURCE_PATCH_PATH + RESOURCE_PATCH_NAME + "_list.txt"))
            {
                for (int j = 0; j < resPatchList.Count; j++)
                {
                    if (j != resPatchList.Count - 1)
                    {
                        swResPatchList.WriteLine(resPatchList[j]);
                    }
                    else
                    {
                        swResPatchList.Write(resPatchList[j]);
                    }
                }
            }
        }

        // Will be deprecated after v3.1
        //static void DownloadCSVandPatch() 
        //{

        //    string CSVURLString = System.IO.File.ReadAllText(RESOURCE_FOLDER_PATH + @"CSVURL.txt");
        //    CSVURLString = CSVURLString.Replace("\r", "");
        //    string[] CSVURL = CSVURLString.Split('\n');
        //    List<string[]> ListCSVURL = new List<string[]>();
        //    for (int i = 0; i < CSVURL.Length; i++)
        //    {
        //        ListCSVURL.Add(CSVURL[i].Split('\\'));
        //    }
        //    for (int i = 0; i < ListCSVURL.Count; i++)
        //    {
        //        WebClient webClient = new WebClient();
        //        try
        //        {
        //            webClient.DownloadFile(ListCSVURL[i][1], TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + ".csv");
        //        }
        //        catch
        //        {
        //            Console.WriteLine(ListCSVURL[i][0] + "      \t번역 파일을 받는 중 오류가 발생하였습니다. 오프라인 파일을 사용합니다.");
        //            File.Copy(RESOURCE_FOLDER_PATH + ListCSVURL[i][0] + ".csv", TEMP_FOLDER_NAME + ListCSVURL[i][0] + ".csv", true);
        //        }
        //    }
        //    for (int i = 0; i < ListCSVURL.Count; i++)
        //    {
        //        PatchText(TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + ".txt", TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + ".csv");
        //    }
        //}

        //static void DownloadCSVandUpdate() // Will be deprecated after v3.1
        //{

        //    string CSVURLString = System.IO.File.ReadAllText(RESOURCE_FOLDER_PATH + @"CSVURL.txt");
        //    CSVURLString = CSVURLString.Replace("\r", "");
        //    string[] CSVURL = CSVURLString.Split('\n');
        //    List<string[]> ListCSVURL = new List<string[]>();
        //    for (int i = 0; i < CSVURL.Length; i++)
        //    {
        //        ListCSVURL.Add(CSVURL[i].Split('\\'));
        //    }
        //    for (int i = 0; i < ListCSVURL.Count; i++)
        //    {
        //        WebClient webClient = new WebClient();
        //        try
        //        {
        //            webClient.DownloadFile(ListCSVURL[i][1], TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + ".csv");
        //        }
        //        catch
        //        {
        //            Console.WriteLine(ListCSVURL[i][0] + "      \t번역 파일을 받는 중 오류가 발생하였습니다. 오프라인 파일을 사용합니다.");
        //            File.Copy(RESOURCE_FOLDER_PATH + ListCSVURL[i][0] + ".csv", TEMP_FOLDER_NAME + ListCSVURL[i][0] + ".csv", true);
        //        }
        //    }
        //    for (int i = 0; i < ListCSVURL.Count; i++)
        //    {
        //        UpdateText(TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + ".txt", TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + ".csv", TEMP_FOLDER_NAME + "\\" + ListCSVURL[i][0] + "_updated.csv");
        //    }
        //}

        //static void UpdateText(string originalTXTPath, string translatedCSVPath, string newTranslatedCSVFileName) // Will be deprecated after v3.1
        //{
        //    string OrigianlTextString = System.IO.File.ReadAllText(originalTXTPath);
        //    TXTParser txtParser = new TXTParser();
        //    txtParser.Append(OrigianlTextString);
        //    List<string[]> originalListContext = txtParser.ListContext;
        //    string TranslatedCSVString = System.IO.File.ReadAllText(translatedCSVPath);
        //    CSVParser csvParser = new CSVParser();
        //    csvParser.Append(TranslatedCSVString);
        //    List<string[]> translatedListContext = csvParser.ListContext;
        //    ListMerge listMerge = new ListMerge(originalListContext, translatedListContext.GetRange(1, translatedListContext.Count - 1), ListMerge.Option.Update);
        //    listMerge.RemoveSharp();
        //    List<string[]> tempMergedList = listMerge.MergendList;
        //    tempMergedList.Insert(0, new string[] { "KEY", "ENGLISH", "KOREAN", "CHECKED" });
        //    CSVParser csvParser2 = new CSVParser(tempMergedList);
        //    string NewTranslatedCSVString = csvParser2.ReadAll();
        //    System.IO.File.WriteAllText(newTranslatedCSVFileName, NewTranslatedCSVString);
        //}

        //static void UpdateText2(string originalTXTPath, string translatedPoPath, string newTranslatedPotPath) //Will be deprecated after v3.1
        //{

        //    Dictionary<string, string> translatedPoDictionary = new Dictionary<string, string>();
        //    try
        //    {
        //        string translatedPoString = System.IO.File.ReadAllText(translatedPoPath);
        //        MatchCollection translatedPoMatchCollection = Regex.Matches(translatedPoString, "msgctxt \"(.*)\"\\nmsgid ([\\s\\S]*?)\\nmsgstr ([\\s\\S]+?)\"\\n\\n");
        //        foreach (Match match in translatedPoMatchCollection)
        //        {
        //            string key = match.Groups[1].Value.Replace("\"", string.Empty);
        //            string value = match.Groups[3].Value;
        //            value = value.Replace("\"", string.Empty);
        //            value = value.Replace("\\n\n", "\n");
        //            value = value.Replace("\\\\", "\\");
        //            value = Regex.Replace(value, "^\\n", string.Empty);
        //            translatedPoDictionary.Add(key, value);
        //        }
        //    }
        //    catch
        //    {
        //        Console.WriteLine("Warning : " + translatedPoPath + " 을 불러올 수 없습니다. 임시로 csv를 사용합니다.");
        //        string OrigianlTextString = System.IO.File.ReadAllText(originalTXTPath);
        //        TXTParser txtParser = new TXTParser();
        //        txtParser.Append(OrigianlTextString);
        //        List<string[]> originalListContext = txtParser.ListContext;
        //        string TranslatedCSVString = System.IO.File.ReadAllText(translatedPoPath.Replace(".po", ".csv"));
        //        CSVParser csvParser = new CSVParser();
        //        csvParser.Append(TranslatedCSVString);
        //        List<string[]> translatedListContext = csvParser.ListContext;
        //        ListMerge listMerge = new ListMerge(originalListContext, translatedListContext.GetRange(1, translatedListContext.Count - 1), ListMerge.Option.Update);
        //        listMerge.RemoveSharp();
        //        List<string[]> tempMergedList = listMerge.MergendList;
        //        //tempMergedList.Insert(0, new string[] { "KEY", "ENGLISH", "KOREAN", "CHECKED" });
        //        foreach (string[] strs in tempMergedList)
        //        {
        //            translatedPoDictionary.Add(strs[0], strs[2].Replace("\r", string.Empty).Replace("\\\n", string.Empty).Replace("\\n", "\n"));
        //        }
        //    }
        //    Dictionary<string, string> originalDictionary = LoadFromTextFile(originalTXTPath);
        //    WriteToPo(newTranslatedPotPath, originalDictionary, translatedPoDictionary);
        //}

        //static void PatchText(string originalTXTPath, string translatedCSVPath)
        //{
        //    string OrigianlTextString = System.IO.File.ReadAllText(originalTXTPath);
        //    OrigianlTextString = OrigianlTextString.Replace("\r\n", "\r");
        //    OrigianlTextString = OrigianlTextString.Replace("\r", "\n");
        //    TXTParser txtParser = new TXTParser();
        //    txtParser.Append(OrigianlTextString);
        //    List<string[]> originalListContext = txtParser.ListContext;
        //    string TranslatedCSVString = System.IO.File.ReadAllText(translatedCSVPath);
        //    CSVParser csvParser = new CSVParser();
        //    csvParser.Append(TranslatedCSVString);
        //    List<string[]> translatedListContext = csvParser.ListContext;
        //    ListMerge listMerge = new ListMerge(originalListContext, translatedListContext.GetRange(1, translatedListContext.Count - 1), ListMerge.Option.Update);
        //    List<string[]> tempMergedList = listMerge.MergendList;
        //    List<string[]> tempList = new List<string[]>();
        //    for (int i = 0; i < tempMergedList.Count; i++)
        //    {
        //        if (tempMergedList[i][2] != "")
        //        {
        //            tempList.Add(new string[] { tempMergedList[i][0], tempMergedList[i][2] });
        //        }
        //        else
        //        {
        //            tempList.Add(new string[] { tempMergedList[i][0], tempMergedList[i][1] });
        //        }
        //    }
        //    TXTParser txtParser2 = new TXTParser(tempList);
        //    string PatchedString = txtParser2.ReadAll();
        //    System.IO.File.WriteAllText(originalTXTPath, PatchedString);
        //}

        //static void MakeAssetFile(SlimeRancherKoreanPatcherManagedCpp.AssetInfo[] AssetInfos)
        //{
        //    string shaderSuffix = "_Shader";
        //    string atlasSuffix = "_Atlas";

        //    string[] languageDataNames =
        //    {
        //        "achieve",
        //        "actor",
        //        "exchange",
        //        "global",
        //        "keys",
        //        "mail",
        //        "pedia",
        //        "range",
        //        "tutorial",
        //        "ui"
        //    };

        //    string[] materialNames =
        //    {
        //        "OpenSans-Semibold SDF Material"
        //    };

        //    string[] monoBehaviourNames =
        //    {
        //        "MonoBehaviour OpenSans SDF"
        //    };

        //    CreateFolderOrClean(SHAREDASSETS0_PATCH_PATH);
        //    List<string> shared0PatchList = new List<string>();
        //    // Material
        //    for (int i = 0; i < materialNames.Length; i++)
        //    {
        //        AssetInfo materialInfo = Array.Find(AssetInfos, x => x.name == materialNames[i]);
        //        File.Copy(RESOURCE_FOLDER_PATH + materialNames[i] + @".dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat", true);
        //        shared0PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat");
        //        using (FileStream fsMaterial = new FileStream(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat", FileMode.Open, FileAccess.ReadWrite))
        //        {
        //            // Shader
        //            fsMaterial.Seek(0x00000028, SeekOrigin.Begin);
        //            AssetInfo shaderInfo = Array.Find(AssetInfos, x => x.name == materialNames[i] + shaderSuffix);
        //            byte[] byteShaderPathID = BitConverter.GetBytes(shaderInfo.pathID);
        //            for (int j = 0; j < 4; j++)
        //            {
        //                fsMaterial.WriteByte(byteShaderPathID[j]);
        //            }

        //            // Atlas
        //            fsMaterial.Seek(0x000000D4, SeekOrigin.Begin);
        //            AssetInfo atlasInfo = Array.Find(AssetInfos, x => x.name == materialNames[i] + atlasSuffix);
        //            byte[] byteAtlasPathID = BitConverter.GetBytes(atlasInfo.pathID);
        //            for (int j = 0; j < 4; j++)
        //            {
        //                fsMaterial.WriteByte(byteAtlasPathID[j]);
        //            }
        //            File.Copy(RESOURCE_FOLDER_PATH + materialNames[i] + atlasSuffix + @".dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + atlasInfo.pathID + ".dat", true);
        //            shared0PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + atlasInfo.pathID + ".dat");
        //        }
        //    }

        //    //MonoBehaviour
        //    for (int i = 0; i < monoBehaviourNames.Length; i++)
        //    {
        //        AssetInfo mono2Info = Array.Find(AssetInfos, x => x.name == monoBehaviourNames[i]);
        //        File.Copy(RESOURCE_FOLDER_PATH + monoBehaviourNames[i] + @".dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat", true);
        //        shared0PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat");
        //        using (FileStream fsMono2 = new FileStream(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat", FileMode.Open, FileAccess.ReadWrite))
        //        {

        //            // TMP_FontAsset
        //            fsMono2.Seek(0x00000014, SeekOrigin.Begin);
        //            AssetInfo TMP_FontAssetInfo = Array.Find(AssetInfos, x => x.name == "TMP_FontAsset");
        //            byte[] byteTMP_FontAssetPathID = BitConverter.GetBytes(TMP_FontAssetInfo.pathID);
        //            for (int j = 0; j < 4; j++)
        //            {
        //                fsMono2.WriteByte(byteTMP_FontAssetPathID[j]);
        //            }

        //            // Material
        //            fsMono2.Seek(0x00000034, SeekOrigin.Begin);
        //            string temp = materialNames[0];
        //            AssetInfo materialInfo = Array.Find(AssetInfos, x => x.name == temp);
        //            byte[] byteMaterialPathID = BitConverter.GetBytes(materialInfo.pathID);
        //            for (int j = 0; j < 4; j++)
        //            {
        //                fsMono2.WriteByte(byteMaterialPathID[j]);
        //            }

        //            // Atlas
        //            fsMono2.Seek(0x000000A8, SeekOrigin.Begin);
        //            AssetInfo atlasInfo = Array.Find(AssetInfos, x => x.name == temp + atlasSuffix);
        //            byte[] byteAtlasPathID = BitConverter.GetBytes(atlasInfo.pathID);
        //            for (int j = 0; j < 4; j++)
        //            {
        //                fsMono2.WriteByte(byteAtlasPathID[j]);
        //            }
        //        }
        //    }

        //    using (StreamWriter swPatchList = new StreamWriter(SHAREDASSETS0_PATCH_PATH + SHAREDASSETS0_PATCH_NAME + "_list.txt"))
        //    {
        //        for (int i = 0; i < shared0PatchList.Count; i++)
        //        {
        //            if (i != shared0PatchList.Count - 1)
        //            {
        //                swPatchList.WriteLine(shared0PatchList[i]);
        //            }
        //            else
        //            {
        //                swPatchList.Write(shared0PatchList[i]);
        //            }
        //        }
        //    }

        //    //res
        //    List<string> resPatchList = new List<string>();
        //    CreateFolderOrClean(RESOURCE_PATCH_PATH);
        //    for (int i = 0; i < languageDataNames.Length; i++)
        //    {
        //        AssetInfo tempLanguageDataInfo = Array.Find(AssetInfos, x => x.name == languageDataNames[i]);
        //        //File.Copy(RESOURCE_FOLDER_PATH + languageDataNames[i] + @".txt", RESOURCE_PATCH_PATH + "Raw_0_" + tempLanguageDataInfo.pathID + ".dat", true);
        //        resPatchList.Add(RESOURCE_PATCH_PATH + "Raw_0_" + tempLanguageDataInfo.pathID + ".dat");
        //        using (FileStream fsTempLang = new FileStream(RESOURCE_PATCH_PATH + "Raw_0_" + tempLanguageDataInfo.pathID + ".dat", FileMode.Create, FileAccess.ReadWrite))
        //        {
        //            byte[] languageDataNameSize = BitConverter.GetBytes(languageDataNames[i].Length);
        //            for (int j = 0; j < 4; j++)
        //            {
        //                fsTempLang.WriteByte(languageDataNameSize[j]);
        //            }
        //            for (int j = 0; j < languageDataNames[i].Length; j++)
        //            {
        //                fsTempLang.WriteByte((byte)languageDataNames[i][j]);
        //            }
        //            while (fsTempLang.Length % 4 != 0)
        //            {
        //                fsTempLang.WriteByte((byte)0);
        //            }
        //            using (FileStream fsTxt = new FileStream(TEMP_FOLDER_NAME + languageDataNames[i] + @".txt", FileMode.Open, FileAccess.Read))
        //            {
        //                byte[] fsTxtSize = BitConverter.GetBytes(fsTxt.Length);
        //                for (int j = 0; j < 4; j++)
        //                {
        //                    fsTempLang.WriteByte(fsTxtSize[j]);
        //                }
        //                for (int j = 0; j < fsTxt.Length; j++)
        //                {
        //                    fsTempLang.WriteByte((byte)fsTxt.ReadByte());
        //                }
        //            }
        //            while (fsTempLang.Length % 4 != 0)
        //            {
        //                fsTempLang.WriteByte((byte)0);
        //            }
        //            for (int j = 0; j < 4; j++)
        //            {
        //                fsTempLang.WriteByte((byte)0);
        //            }
        //        }
        //    }
        //    using (StreamWriter swResPatchList = new StreamWriter(RESOURCE_PATCH_PATH + RESOURCE_PATCH_NAME + "_list.txt"))
        //    {
        //        for (int j = 0; j < resPatchList.Count; j++)
        //        {
        //            if (j != resPatchList.Count - 1)
        //            {
        //                swResPatchList.WriteLine(resPatchList[j]);
        //            }
        //            else
        //            {
        //                swResPatchList.Write(resPatchList[j]);
        //            }
        //        }
        //    }
        //}

        //static void TestRegex()
        //{
        //    // 슬라임랜처에 적합한 Regex => (.+[^ ]) *?= *([^\n]*.*(\n[^#\n]([^=\n]*))*)?(\n|$)
        //    // po pot 파일 파서 => #: (.*)\nmsgctxt "(.*)"\nmsgid "(.*)"\nmsgstr "(.*)"
        //    string text = System.IO.File.ReadAllText(TEMP_FOLDER_NAME + @"pedia.txt");
        //    Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
        //    text = text.Replace("\r", string.Empty);
        //    MatchCollection matches = Regex.Matches(text, @"(.+[^ ]) *?= *([^\n]*.*(\n[^#\n]([^=\n]*))*)?(\n|$)");
        //    foreach (Match match in matches)
        //    {
        //        string valueString = match.Groups[2].Value;
        //        valueString = Regex.Replace(valueString, @"^[\r\n]+|\.|[\r\n]+$", string.Empty);
        //        dictionary2.Add(match.Groups[1].Value, valueString);
        //    }
        //    //return dictionary;
        //    string poString = "";
        //    foreach(KeyValuePair<string,string> keyValue in dictionary2)
        //    {
        //        poString += "msgctxt \"" + keyValue.Key + "\"\nmsgid ";
        //        string[] valueStrings = keyValue.Value.Split('\n');
        //        foreach(string str in valueStrings)
        //        {
        //            string tempStr = str.Replace("\\", "\\\\");
        //            if (valueStrings.Last() == str)
        //            {
        //                poString += "\"" + tempStr + "\"\n";
        //            }
        //            else
        //            {
        //                poString += "\"" + tempStr + "\\n\"\n";
        //            }
        //        }
        //        poString += "msgstr \"\"\n\n";
        //    }
        //    // \n \r 같은거 바꿔줘야함
        //    System.IO.File.WriteAllText(TEMP_FOLDER_NAME + @"pedia.pot", poString);

        //    // msgctxt "(.*)"\nmsgid ([\s\S]*?)\nmsgstr ([\s\S]+?)"\n\n
        //    Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
        //    string translatedPoString = System.IO.File.ReadAllText(TEMP_FOLDER_NAME + @"pedia.po");
        //    MatchCollection translatedMatchCollection = Regex.Matches(translatedPoString, "msgctxt \"(.*)\"\\nmsgid ([\\s\\S]*?)\\nmsgstr ([\\s\\S]+?)\"\\n\\n");
        //    foreach(Match match in translatedMatchCollection)
        //    {
        //        string key = match.Groups[1].Value.Replace("\"", string.Empty);
        //        string value = match.Groups[3].Value;
        //        value = value.Replace("\"", string.Empty);
        //        value = value.Replace("\\n\n", "\n");
        //        value = value.Replace("\\\\", "\\");
        //        value = Regex.Replace(value, "^\\n", string.Empty);
        //        dictionary3.Add(key, value);
        //    }
        //}
    }
}
