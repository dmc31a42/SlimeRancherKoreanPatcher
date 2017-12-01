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
	logOfstream.open((_currentDirectory + "output_cpp.log").c_str(), ios::trunc);

#ifdef MY_DEBUG
	logOfstream << "_gameFolderPath : " << _gameFolderPath << endl;
	logOfstream << "_currentDirectory : " << _currentDirectory << endl;
	logOfstream << "(_gameFolderPath + resAssetsFileName).c_str() : " << (_gameFolderPath + resAssetsFileName).c_str() << endl;
	logOfstream << "(_gameFolderPath + sharedAssetsFileName).c_str() : " << (_gameFolderPath + sharedAssetsFileName).c_str() << endl;
	logOfstream << "(_currentDirectory + classDatabaseFileName).c_str() : " << (_currentDirectory + classDatabaseFileName).c_str() << endl;
#endif

	fopen_s(&pResAssetsFile, (_gameFolderPath + resAssetsFileName).c_str(), "rb");
	fopen_s(&psharedAssetsFile, (_gameFolderPath + sharedAssetsFileName).c_str(), "rb");
	fopen_s(&pClassDatabaseFile, (_currentDirectory + classDatabaseFileName).c_str(), "rb");

#ifdef MY_DEBUG
	//cout << pResAssetsFile->
#endif

	resAssetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)pResAssetsFile);
	sharedAssetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)psharedAssetsFile);
	classDatabaseFile = new ClassDatabaseFile();
	classDatabaseFile->Read(AssetsReaderFromFile, (LPARAM)pClassDatabaseFile);

	resAssetsFileTable = new AssetsFileTable(resAssetsFile);
	sharedAssetsFileTable = new AssetsFileTable(sharedAssetsFile);

	//findByClassID = new map<int, unsigned int>();
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

	if (classDatabaseFile)
	{
		delete classDatabaseFile;
		classDatabaseFile = NULL;
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
}

void unmanagedPatcher::FindInformation()
{
	map<string, int> materialNames;
	int materialCount;
	map<string,int> languageDataNames;
	int languageDataCount;
	map<string, int> monoBehaviourNames;
	int monoBehaviourCount;
	
	materialNames.insert(map<string,int>::value_type("OpenSans-Semibold SDF Material",0));
	materialCount = materialNames.size();

	languageDataNames.insert(map<string, int>::value_type("achieve",3));
	languageDataNames.insert(map<string, int>::value_type("actor", 1));
	languageDataNames.insert(map<string, int>::value_type("exchange", 3));
	languageDataNames.insert(map<string, int>::value_type("global", 0));
	languageDataNames.insert(map<string, int>::value_type("keys", 1));
	languageDataNames.insert(map<string, int>::value_type("mail", 0));
	languageDataNames.insert(map<string, int>::value_type("pedia", 2));
	languageDataNames.insert(map<string, int>::value_type("range", 4));
	languageDataNames.insert(map<string, int>::value_type("tutorial", 1));
	languageDataNames.insert(map<string, int>::value_type("ui", 1));

	languageDataCount = languageDataNames.size();

	monoBehaviourNames.insert(map<string, int>::value_type("OpenSans SDF",0));
	monoBehaviourCount = monoBehaviourNames.size();

	unsigned int currentPathID = 1;
	for (; currentPathID <= resAssetsFileTable->assetFileInfoCount; currentPathID++)
	{
		AssetFileInfoEx *tempAssetFileInfoEx = resAssetsFileTable->getAssetInfo(currentPathID);
#ifdef MY_DEBUG
		logOfstream << "[" << currentPathID << "] : " <<  tempAssetFileInfoEx->name << endl;
#endif
		map<string, int>::iterator FindIter = languageDataNames.find(tempAssetFileInfoEx->name);
		if (FindIter != languageDataNames.end())
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

				// extract original txt file
				AssetTypeTemplateField *tempAssetTypeTemplateField = new AssetTypeTemplateField;
				tempAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[tempAssetFileInfoEx->curFileType]], (DWORD)0);
				AssetTypeInstance tempAssetTypeInstance((DWORD)1, &tempAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)pResAssetsFile, resAssetsFile->header.endianness ? true : false, tempAssetFileInfoEx->absolutePos);
				AssetTypeValueField *pBase = tempAssetTypeInstance.GetBaseField();
				if (pBase)
				{
					AssetTypeValueField *pm_Script = pBase->Get("m_Script");
					if (pm_Script && pm_Script->IsDummy() == false)
					{
						string m_Script = pm_Script->GetValue()->AsString();
						ofstream ofsTempTxt(_currentDirectory + "temp\\" + tempAssetInfo.name + ".txt");
						ofsTempTxt << m_Script;
						ofsTempTxt.close();
						languageDataCount--;
						if (languageDataCount == 0)
						{
							break;
						}
					}
				}
				
			}
		}
	}

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