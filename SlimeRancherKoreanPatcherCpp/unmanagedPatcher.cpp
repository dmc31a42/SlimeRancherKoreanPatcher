// defined _ITERATOR_DEBUG_LEVEL, _CRT_SECURE_NO_WARNINGS for UABE_API
#define _ITERATOR_DEBUG_LEVEL 0
#define _CRT_SECURE_NO_WARNINGS

#include "stdafx.h"
#include "unmanagedPatcher.h"
#include <iostream>
#include <vector>
#include <map>

#include "AssetsTools\defines.h"
#include "AssetsTools\AssetsFileFormat.h"
#include "AssetsTools\AssetsFileReader.h"
#include "AssetsTools\AssetsFileTable.h"
#include "AssetsTools\AssetTypeClass.h"
#include "AssetsTools\AssetsBundleFileFormat.h"
#include "AssetsTools\ClassDatabaseFile.h"

#define MY_DEBUG
using namespace std;

int filesize(FILE* file)
{
	fseek(file, 0L, SEEK_END);
	int fileLen = ftell(file);
	fseek(file, 0L, SEEK_SET);
	return fileLen;
}

int FindPathID(string str)
{
	std::size_t foundUnderBar = str.find_last_of('_');
	std::size_t foundDot = str.find_last_of('.');
	string ID_string = str.substr(foundUnderBar + 1, foundDot - foundUnderBar - 1);
	return std::stoi(ID_string);
}

unmanagedPatcher::unmanagedPatcher(string gameFolderPath, string currentDirectory)
{
	_gameFolderPath = gameFolderPath;
	_currentDirectory = currentDirectory;
	resAssetsFileName = "resources.assets";
	sharedAssetsFileName = "sharedassets0.assets";
	classDatabaseFileName = "Resource\\U5.6.0f3.dat";
	noExtGlobalAssetFileName = "globalgamemanagers";
	logOfstream.open((_currentDirectory + "output_cpp.log").c_str(), ios::trunc);

#ifdef MY_DEBUG
	logOfstream << "_gameFolderPath : " << _gameFolderPath << endl;
	logOfstream << "_currentDirectory : " << _currentDirectory << endl;
	logOfstream << "(_gameFolderPath + resAssetsFileName).c_str() : " << (_gameFolderPath + resAssetsFileName).c_str() << endl;
	logOfstream << "(_gameFolderPath + sharedAssetsFileName).c_str() : " << (_gameFolderPath + sharedAssetsFileName).c_str() << endl;
	logOfstream << "(_currentDirectory + classDatabaseFileName).c_str() : " << (_currentDirectory + classDatabaseFileName).c_str() << endl;
	logOfstream << "(_gameFolderPath + noExtGlobalAssetFileName).c_str() : " << (_gameFolderPath + noExtGlobalAssetFileName).c_str() << endl;
#endif

#ifdef MY_DEBUG
	logOfstream << endl;
#endif
#ifdef MY_DEBUG
	logOfstream << "fopen_s pFILE" << endl;
#endif
	fopen_s(&pResAssetsFile, (_gameFolderPath + resAssetsFileName).c_str(), "rb");
	fopen_s(&psharedAssetsFile, (_gameFolderPath + sharedAssetsFileName).c_str(), "rb");
	fopen_s(&pClassDatabaseFile, (_currentDirectory + classDatabaseFileName).c_str(), "rb");
	fopen_s(&pNoExtGlobalAssetsFile, (_gameFolderPath + noExtGlobalAssetFileName).c_str(), "rb");
#ifdef MY_DEBUG
	logOfstream << "create AssetsFile, ClassDatabaseFile" << endl;
#endif
	resAssetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)pResAssetsFile);
	sharedAssetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)psharedAssetsFile);
	classDatabaseFile = new ClassDatabaseFile();
	classDatabaseFile->Read(AssetsReaderFromFile, (LPARAM)pClassDatabaseFile);
	noExtGlobalAssetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)pNoExtGlobalAssetsFile);
#ifdef MY_DEBUG
	logOfstream << "create AssetsFileTable" << endl;
#endif
	resAssetsFileTable = new AssetsFileTable(resAssetsFile);
	sharedAssetsFileTable = new AssetsFileTable(sharedAssetsFile);
	noExtGlobalAssetsFileTable = new AssetsFileTable(noExtGlobalAssetsFile);
#ifdef MY_DEBUG
	logOfstream << "make findByClassID" << endl;
#endif
	for (int i = 0; i < (int)classDatabaseFile->classes.size(); i++)
	{
		int classId = classDatabaseFile->classes[i].classId;
		findByClassID.insert(map<int, unsigned int>::value_type(classId, i));
	}

	FindInformation();
}

unmanagedPatcher::~unmanagedPatcher()
{
	if (resAssetsFileTable)
	{
		delete resAssetsFileTable;
		resAssetsFileTable = NULL;
	}
	if (sharedAssetsFileTable)
	{
		delete sharedAssetsFileTable;
		sharedAssetsFileTable = NULL;
	}
	if (noExtGlobalAssetsFileTable)
	{
		delete noExtGlobalAssetsFileTable;
		noExtGlobalAssetsFileTable = NULL;
	}

	if (classDatabaseFile)
	{
		delete classDatabaseFile;
		classDatabaseFile = NULL;
	}
	if (noExtGlobalAssetsFile)
	{
		delete noExtGlobalAssetsFile;
		noExtGlobalAssetsFile = NULL;
	}
	if (sharedAssetsFile)
	{
		delete sharedAssetsFile;
		sharedAssetsFile = NULL;
	}
	if (resAssetsFile)
	{
		delete resAssetsFile;
		resAssetsFile = NULL;
	}

	if (pResAssetsFile)
	{
		fclose(pResAssetsFile);
		pResAssetsFile = NULL;
	}
	if (psharedAssetsFile)
	{
		fclose(psharedAssetsFile);
		psharedAssetsFile = NULL;
	}
	if (pClassDatabaseFile)
	{
		fclose(pClassDatabaseFile);
		pClassDatabaseFile = NULL;
	}
	if (pNoExtGlobalAssetsFile)
	{
		fclose(pNoExtGlobalAssetsFile);
		pNoExtGlobalAssetsFile = NULL;
	}
}

void unmanagedPatcher::FindInformation()
{
#ifdef MY_DEBUG
	logOfstream << "start FindInformation" << endl;
#endif
	map<string, int> materialNames;
	int materialCount;
	map<string, int> monoBehaviourNames;
	int monoBehaviourCount;
	
	materialNames.insert(map<string,int>::value_type("OpenSans-Semibold SDF Material",0));
	materialCount = materialNames.size();

	monoBehaviourNames.insert(map<string, int>::value_type("OpenSans SDF",0));
	monoBehaviourCount = monoBehaviourNames.size();
	
	int languageDataCount = 11;
	unsigned int noExtGlobalPathID = 0;
	for (noExtGlobalPathID = 1; noExtGlobalPathID < noExtGlobalAssetsFileTable->assetFileInfoCount; noExtGlobalPathID++)
	{
		AssetFileInfoEx *tempAssetFileInfoEx = noExtGlobalAssetsFileTable->getAssetInfo(noExtGlobalPathID);
#ifdef MY_DEBUG
		logOfstream << "[" << noExtGlobalPathID << "] : ->curFileType : " << tempAssetFileInfoEx->curFileType << endl;
#endif
		if (tempAssetFileInfoEx->curFileType == 0x00000093)
		{
			AssetTypeTemplateField *tempAssetTypeTemplateField = new AssetTypeTemplateField;
			tempAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[tempAssetFileInfoEx->curFileType]], (DWORD)0);
			AssetTypeInstance tempAssetTypeInstance((DWORD)1, &tempAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)pNoExtGlobalAssetsFile, noExtGlobalAssetsFile->header.endianness ? true : false, tempAssetFileInfoEx->absolutePos);
			AssetTypeValueField *pBase = tempAssetTypeInstance.GetBaseField();
			if (pBase)
			{
				AssetTypeValueField *pm_Container = pBase->Get("m_Container");
				if (pm_Container && pm_Container->IsDummy() == false)
				{
					unsigned int m_ContainerSize = pm_Container->Get("Array")->GetChildrenCount();
					//AssetTypeValueField **m_ContainerArray = pm_Container->GetChildrenList();
#ifdef MY_DEBUG
					logOfstream << "m_ContainerSize : " << m_ContainerSize << endl;
#endif // MY_DEBUG
					for (unsigned int i = 0; i < m_ContainerSize; i++)
					{
						//AssetTypeValueField *tempAssetTypeValueField = (*(m_ContainerArray + i));
						AssetTypeValueField *tempAssetTypeValueField = pm_Container->Get("Array")->Get(i);
						string resourcesPath = tempAssetTypeValueField->Get("first")->GetValue()->AsString();
						int pos = resourcesPath.find("i18n/en/");
#ifdef MY_DEBUG
						logOfstream << "[" << i << "] : " << resourcesPath << ", pos : " << pos << endl;
#endif // MY_DEBUG
						if (pos != -1)
						{
							string textAssetName = resourcesPath.substr(8, resourcesPath.length() - 8);
							int textAssetPathID = tempAssetTypeValueField->Get("second")->Get("m_PathID")->GetValue()->AsInt();
#ifdef MY_DEBUG
							logOfstream << "textAssetName : " << textAssetName << "PathID : " << textAssetPathID << endl;
#endif // MY_DEBUG
							AssetFileInfoEx *textAssetFileInfoEx = resAssetsFileTable->getAssetInfo(textAssetPathID);
							UnmanagedAssetInfo tempAssetInfo;
							tempAssetInfo.pathID = textAssetPathID;
							tempAssetInfo.name = textAssetFileInfoEx->name;
							tempAssetInfo.offset = (int)textAssetFileInfoEx->absolutePos;
							tempAssetInfo.size = textAssetFileInfoEx->curFileSize;
							assetInfos.push_back(tempAssetInfo);

							// extract original txt file
							AssetTypeTemplateField *textAssetTypeTemplateField = new AssetTypeTemplateField;
							textAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[textAssetFileInfoEx->curFileType]], (DWORD)0);
							AssetTypeInstance textAssetTypeInstance((DWORD)1, &textAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)pResAssetsFile, resAssetsFile->header.endianness ? true : false, textAssetFileInfoEx->absolutePos);
							AssetTypeValueField *pTextBase = textAssetTypeInstance.GetBaseField();
							if (pTextBase)
							{
								AssetTypeValueField *pm_Script = pTextBase->Get("m_Script");
								if (pm_Script && pm_Script->IsDummy() == false)
								{
									string m_Script = pm_Script->GetValue()->AsString();
									ofstream ofsTempTxt(_currentDirectory + "temp\\" + tempAssetInfo.name + ".txt", std::ifstream::binary);
									ofsTempTxt.write(m_Script.c_str(),m_Script.length());
									ofsTempTxt.close();
									languageDataCount--;
								}
							}
						}
						//ClearAssetTypeValueField(tempAssetTypeValueField);
						if (languageDataCount == 0)
						{
							break;
						}
					}
				}
			}
		}
		if (languageDataCount == 0)
		{
			break;
		}
	}

	unsigned int currentPathID = 1;
	currentPathID = 1;
	for (; currentPathID <= sharedAssetsFileTable->assetFileInfoCount; currentPathID++)
	{
		if (materialNames.size() == 0)
		{
			break;
		}
		AssetFileInfoEx *tempAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(currentPathID);
#ifdef MY_DEBUG
		logOfstream << "[" << currentPathID << "] : " << "->name : " << tempAssetFileInfoEx->name << endl;
#endif
		map<string, int>::iterator FindIter = materialNames.find(tempAssetFileInfoEx->name);
		if (FindIter != materialNames.end())
		{
			FindIter->second--;
			if (FindIter->second == -1)
			{
				UnmanagedAssetInfo tempAssetInfo;
				tempAssetInfo.pathID = currentPathID;
				tempAssetInfo.name = tempAssetFileInfoEx->name;
				tempAssetInfo.offset = (int)tempAssetFileInfoEx->absolutePos;
				tempAssetInfo.size = tempAssetFileInfoEx->curFileSize;
				assetInfos.push_back(tempAssetInfo);
				AssetTypeTemplateField *tempAssetTypeTemplateField = new AssetTypeTemplateField;
				tempAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[tempAssetFileInfoEx->curFileType]], (DWORD)0);
				AssetTypeInstance tempAssetTypeInstance((DWORD)1, &tempAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)psharedAssetsFile, sharedAssetsFile->header.endianness?true:false, tempAssetFileInfoEx->absolutePos);
				AssetTypeValueField *pBase = tempAssetTypeInstance.GetBaseField();
				if (pBase) {
					AssetTypeValueField *pShader = pBase->Get("m_Shader")->Get("m_PathID");
					AssetTypeValueField *pm_TexEnvs = pBase->Get("m_SavedProperties")->Get("m_TexEnvs")->Get("Array");
					if(pm_TexEnvs && pm_TexEnvs->IsDummy() == false)
					{
						int TexEnvPosition = -1;
						int TexEnvsSize = pm_TexEnvs->GetChildrenCount();
						for (int i = 0; i < TexEnvsSize; i++)
						{
							AssetTypeValueField *pm_TexEnv = pBase->Get("m_SavedProperties")->Get("m_TexEnvs")->Get("Array")->Get((unsigned int)i)->Get("first");
							if (pm_TexEnv && pm_TexEnv->IsDummy() == false)
							{
								string TexEnvName = pm_TexEnv->GetValue()->AsString();
								if (TexEnvName == "_MainTex")
								{
									TexEnvPosition = i;
								}
							}
						}
						AssetTypeValueField *pAtlas = pBase->Get("m_SavedProperties")->Get("m_TexEnvs")->Get("Array")->Get((unsigned int)TexEnvPosition)->Get("second")->Get("m_Texture")->Get("m_PathID");

						//->Get("m_TexEnvs")->Get("Array")->Get((unsigned int)0)->Get("data")->Get("second")->Get("m_Texture")->Get("m_PathID");

						// 만약 한번에 필드를 못얻겠으면 순차적으로 확인해볼것
						//AssetTypeValueField *pAtlas = pBase->Get("m_SavedProperties");
						// //->Get("m_TexEnvs")->Get("Array")->Get((unsigned int)0)->Get("data")->Get("second")->Get("m_Texture")->Get("m_PathID");
#ifdef MY_DEBUG
						bool pShaderExist = pShader ? true : false;
						bool pAtlasExist = pAtlas ? true : false;
						bool pShaderIsDummy = pShader->IsDummy();
						bool pAtlasIsDummy = pAtlas->IsDummy();
						logOfstream << "[" << FindIter->first << "]" << "pShaderExist : " << pShaderExist << endl;
						logOfstream << "[" << FindIter->first << "]" << "pAtlasExist : " << pAtlasExist << endl;
						logOfstream << "[" << FindIter->first << "]" << "pShaderIsDummy : " << pShaderIsDummy << endl;
						logOfstream << "[" << FindIter->first << "]" << "pAtlasIsDummy : " << pAtlasIsDummy << endl;
#endif
						if (pShader && pAtlas && !pShader->IsDummy() && !pAtlas->IsDummy())
						{
							int shaderPathID = pShader->GetValue()->AsInt();
							int atlasPathID = pAtlas->GetValue()->AsInt();

							AssetFileInfoEx *shaderAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(shaderPathID);
							UnmanagedAssetInfo shaderAssetInfo;
							shaderAssetInfo.pathID = shaderPathID;
							shaderAssetInfo.name = FindIter->first + "_Shader";
							shaderAssetInfo.offset = (int)shaderAssetFileInfoEx->absolutePos;
							shaderAssetInfo.size = shaderAssetFileInfoEx->curFileSize;
							assetInfos.push_back(shaderAssetInfo);

							AssetFileInfoEx *atlasAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(atlasPathID);
							UnmanagedAssetInfo atlasAssetInfo;
							atlasAssetInfo.pathID = atlasPathID;
							atlasAssetInfo.name = FindIter->first + "_Atlas";
							shaderAssetInfo.offset = (int)atlasAssetFileInfoEx->absolutePos;
							atlasAssetInfo.size = atlasAssetFileInfoEx->curFileSize;
							assetInfos.push_back(atlasAssetInfo);

							materialCount--;
							if (materialCount == 0)
							{
								break;
							}
						}
					}		
				}
			}
		}
	}

	for (; currentPathID <= sharedAssetsFileTable->assetFileInfoCount; currentPathID++)
	{
		if (monoBehaviourNames.size() == 0)
		{
			break;
		}
		AssetFileInfoEx *tempAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(currentPathID);
		if (tempAssetFileInfoEx->curFileType < 4294901760)
		{
			continue;
		}
		AssetTypeTemplateField *tempAssetTypeTemplateField = new AssetTypeTemplateField;
		tempAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[0x00000072]], (DWORD)0);
		AssetTypeInstance tempAssetTypeInstance((DWORD)1, &tempAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)psharedAssetsFile, sharedAssetsFile->header.endianness ? true : false, tempAssetFileInfoEx->absolutePos);
		AssetTypeValueField *pBase = tempAssetTypeInstance.GetBaseField();
		if (pBase)
		{
			AssetTypeValueField *pm_Name = pBase->Get("m_Name");
			if (pm_Name && pm_Name->IsDummy() == false)
			{
				string m_Name = pm_Name->GetValue()->AsString();
#ifdef MY_DEBUG
				logOfstream << "[PathID : " << currentPathID << "] : " << m_Name << ", ->curFileType : " << tempAssetFileInfoEx->curFileType <<  endl;
#endif
				map<string, int>::iterator FindIter = monoBehaviourNames.find(m_Name);
				if (FindIter != monoBehaviourNames.end())
				{
					FindIter->second--;
					if (FindIter->second == -1)
					{
						UnmanagedAssetInfo tempAssetInfo;
						tempAssetInfo.pathID = currentPathID;
						tempAssetInfo.name = "MonoBehaviour " +  m_Name;
						tempAssetInfo.offset = (int)tempAssetFileInfoEx->absolutePos;
						tempAssetInfo.size = tempAssetFileInfoEx->curFileSize;
						assetInfos.push_back(tempAssetInfo);

						AssetTypeValueField *pScript = pBase->Get("m_Script")->Get("m_PathID");

						if (pScript && pScript->IsDummy() == false)
						{
							UnmanagedAssetInfo scriptAssetInfo;
							scriptAssetInfo.pathID = pScript->GetValue()->AsInt();
							scriptAssetInfo.name = "TMP_FontAsset";
							scriptAssetInfo.offset = -1;
							scriptAssetInfo.size = -1;
							assetInfos.push_back(scriptAssetInfo);

							monoBehaviourCount--;
							if (monoBehaviourCount == 0)
							{
								break;
							}
						}
					}
				}
				/*free(pm_Name);
				pm_Name = 0;*/
			}
			/*free(pBase);
			pBase = 0;*/
		}	
	}
}

vector<UnmanagedAssetInfo> unmanagedPatcher::GetAssetInfos()
{
	return this->assetInfos;
}

void unmanagedPatcher::MakeModdedAssets()
{
	// sharedPatch
	string sharedPatchListFilePath = "temp\\sharedassets0_patch\\sharedassets0_patch_list.txt";

	ifstream ifsSharedPatchListFile(sharedPatchListFilePath);
	std::vector<string> sharedPatchFileName;
	if (!ifsSharedPatchListFile.is_open())
	{
		cout << "cannot open patchFileList text file" << endl;
		cout << "Exit" << endl;
		return;
	}
#ifdef MY_DEBUG
	logOfstream << "Patch File List : " << endl;
#endif
	std::vector<FILE*> pSharedPatchFile;
	while (!ifsSharedPatchListFile.eof())
	{
		string temp;
		FILE *pTempPatchFile = NULL;
		ifsSharedPatchListFile >> temp;
		if (temp == "")
		{
			continue;
		}
		sharedPatchFileName.push_back(temp);
#ifdef MY_DEBUG
		logOfstream << sharedPatchFileName[sharedPatchFileName.size() - 1] << endl;
#endif
		fopen_s(&pTempPatchFile, temp.c_str(), "rb");
		if (pTempPatchFile == NULL)
		{
			cout << "cannot open patch file : " << temp << endl;
			cout << "Exit" << endl;
			fclose(pTempPatchFile);
			for (int i = 0; i < (int)pSharedPatchFile.size(); i++)
			{
				fclose(pSharedPatchFile[i]);
			}
			return;
		}
		pSharedPatchFile.push_back(pTempPatchFile);
	}
	ifsSharedPatchListFile.close();

	string sharedModdedFilePath = _gameFolderPath + sharedAssetsFileName + ".modded";
	std::vector<AssetsReplacer*> sharedReplacors;
	std::vector<AssetFileInfoEx*> sharedAssetsFileInfos;

	for (unsigned int i = 0; i < sharedPatchFileName.size(); i++)
	{
		int tempPathID = FindPathID(sharedPatchFileName[i]);
		sharedAssetsFileInfos.push_back(sharedAssetsFileTable->getAssetInfo(tempPathID)); // I know the ID - no need to search
		sharedReplacors.push_back(MakeAssetModifierFromFile(0, (*sharedAssetsFileInfos[i]).index, (*sharedAssetsFileInfos[i]).curFileType, (*sharedAssetsFileInfos[i]).inheritedUnityClass,
			pSharedPatchFile[i], 0, (QWORD)filesize(pSharedPatchFile[i]))); // I expect that the size parameter refers to the file size but I couldn't check this until now
	}
	FILE *pModdedSharedFile;
	fopen_s(&pModdedSharedFile, (sharedModdedFilePath).c_str(), "wb");
	sharedAssetsFile->Write(AssetsWriterToFile, (LPARAM)pModdedSharedFile, 0, sharedReplacors.data(), sharedReplacors.size(), 0);

	for (unsigned int i = 0; i < pSharedPatchFile.size(); i++)
	{
		if (pSharedPatchFile[i])
		{
			fclose(pSharedPatchFile[i]);
			pSharedPatchFile[i] = NULL;
		}
	}
	if (pModdedSharedFile)
	{
		fclose(pModdedSharedFile);
		pModdedSharedFile = NULL;
	}
	//////////////////////////////////////////////
	// resources
	//////////////////////////////////////////////
	string resPatchListFilePath = "temp\\resources_patch\\resources_patch_list.txt";

	ifstream ifsResPatchListFile(resPatchListFilePath);
	std::vector<string> resPatchFileName;
	if (!ifsResPatchListFile.is_open())
	{
		cout << "cannot open patchFileList text file" << endl;
		cout << "Exit" << endl;
		return;
	}
#ifdef MY_DEBUG
	logOfstream << "Patch File List : " << endl;
#endif
	std::vector<FILE*> pResPatchFile;
	while (!ifsResPatchListFile.eof())
	{
		string temp;
		FILE *pTempPatchFile = NULL;
		ifsResPatchListFile >> temp;
		if (temp == "")
		{
			continue;
		}
		resPatchFileName.push_back(temp);
#ifdef MY_DEBUG
		logOfstream << resPatchFileName[resPatchFileName.size() - 1] << endl;
#endif
		fopen_s(&pTempPatchFile, temp.c_str(), "rb");
		if (pTempPatchFile == NULL)
		{
			cout << "cannot open patch file : " << temp << endl;
			cout << "Exit" << endl;
			fclose(pTempPatchFile);
			for (int i = 0; i < (int)pResPatchFile.size(); i++)
			{
				fclose(pResPatchFile[i]);
			}
			return;
		}
		pResPatchFile.push_back(pTempPatchFile);
	}
	ifsResPatchListFile.close();

	string resModdedFilePath = _gameFolderPath + resAssetsFileName + ".modded";
	std::vector<AssetsReplacer*> resReplacors;
	std::vector<AssetFileInfoEx*> resAssetsFileInfos;

	for (unsigned int i = 0; i < resPatchFileName.size(); i++)
	{
		int tempPathID = FindPathID(resPatchFileName[i]);
		resAssetsFileInfos.push_back(resAssetsFileTable->getAssetInfo(tempPathID)); // I know the ID - no need to search
		resReplacors.push_back(MakeAssetModifierFromFile(0, (*resAssetsFileInfos[i]).index, (*resAssetsFileInfos[i]).curFileType, (*resAssetsFileInfos[i]).inheritedUnityClass,
			pResPatchFile[i], 0, (QWORD)filesize(pResPatchFile[i]))); // I expect that the size parameter refers to the file size but I couldn't check this until now
	}
	FILE *pModdedResFile;
	fopen_s(&pModdedResFile, (resModdedFilePath).c_str(), "wb");
	resAssetsFile->Write(AssetsWriterToFile, (LPARAM)pModdedResFile, 0, resReplacors.data(), resReplacors.size(), 0);

	for (unsigned int i = 0; i < pResPatchFile.size(); i++)
	{
		if (pResPatchFile[i])
		{
			fclose(pResPatchFile[i]);
			pResPatchFile[i] = NULL;
		}
	}
	if (pModdedResFile)
	{
		fclose(pModdedResFile);
		pModdedResFile = NULL;
	}

#ifdef MY_DEBUG
	logOfstream << "Slime Rancher Korean Translation Patch Complete. Exit" << endl;
#endif
}


UnmanagedAssetInfo::UnmanagedAssetInfo()
{

}

UnmanagedAssetInfo::UnmanagedAssetInfo(int pathID, string name, int offset, int size)
{
	this->pathID = pathID;
	this->name = name;
	this->offset = offset;
	this->size = size;
}