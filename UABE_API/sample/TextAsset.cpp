#include "stdafx.h"

#include <stdlib.h>
#include <stdio.h>
#include <malloc.h>

#include <AssetBundleExtractor\IPluginInterface.h>
#include <AssetsTools\TextureFileFormat.h>
#include <AssetsTools\AssetsReplacer.h>


void TextAsset_FreeMemoryResource(void *pResource)
{
	free(pResource);
}

void ImportFromTXT(HWND hParentWnd, IPluginInterface *pInterface, IAssetInterface **ppAssets, size_t assetCount)
{
	IAssetInterface *pAsset = ppAssets[0];
	WCHAR *sourceFileName;
	if (SUCCEEDED(pInterface->ShowFileOpenDialog(hParentWnd, &sourceFileName, L"*.txt|Text file:")))
	{
		FILE *pSourceFile = NULL;
		_wfopen_s(&pSourceFile, sourceFileName, L"rb");
		if (pSourceFile)
		{
			fseek(pSourceFile, 0, SEEK_END);
			long inFileSize = ftell(pSourceFile);
			fseek(pSourceFile, 0, SEEK_SET);
			void *pFileBuffer = malloc(inFileSize);
			__checkoutofmemory(pFileBuffer==NULL);

			fread(pFileBuffer, inFileSize, 1, pSourceFile);

			AssetTypeTemplateField templateBase;
			char *name = "";
			char *pathName = "";
			if (pInterface->MakeTemplateField(pAsset->GetAssetsFile(), 0x31, &templateBase))
			{
				//make m_Script a TypelessData array because sometimes binary data (including 00s) is stored there
				for (DWORD i = 0; i < templateBase.childrenCount; i++)
				{
					if (!strcmp(templateBase.children[i].name, "m_Script"))
					{
						templateBase.children[i].type = "_string";
						templateBase.children[i].valueType = ValueType_None;
						for (DWORD k = 0; k < templateBase.children[i].childrenCount; k++)
						{
							if (!strcmp(templateBase.children[i].children[k].name, "Array"))
							{
								templateBase.children[i].children[k].type = "TypelessData";
								break;
							}
						}
						break;
					}
				}
				AssetTypeTemplateField *pTemplateBase = &templateBase;
				AssetsFileReader reader;
				LPARAM readerPar;
				unsigned __int64 assetFilePos;
				unsigned __int64 assetFileSize = pAsset->GetFileReader(reader, readerPar, assetFilePos);
				AssetTypeInstance instance(1, &pTemplateBase, reader, readerPar, pAsset->GetAssetsFile()->header.endianness, assetFilePos);
				pAsset->FreeFileReader(reader, readerPar);
				AssetTypeValueField *pBase = instance.GetBaseField();
				if (pBase)
				{
					AssetTypeValueField *nameField = pBase->Get("m_Name");
					AssetTypeValueField *scriptField = pBase->Get("m_Script");
					AssetTypeValueField *dataArrayField = scriptField->Get("Array");
					AssetTypeValueField *pathNameField = pBase->Get("m_PathName");
					if (!nameField->IsDummy() && !scriptField->IsDummy() && !pathNameField->IsDummy() && !dataArrayField->IsDummy())
					{
						name = nameField->GetValue()->AsString();
						pathName = pathNameField->GetValue()->AsString();
						AssetTypeByteArray byteArrayData;
						byteArrayData.size = (DWORD)inFileSize;
						byteArrayData.data = (BYTE*)pFileBuffer;
						dataArrayField->GetValue()->Set(&byteArrayData);

						QWORD newByteSize = pBase->GetByteSize(0);
						void *newAssetBuffer = malloc((size_t)newByteSize);
						__checkoutofmemory(newAssetBuffer==NULL);
						LPARAM writerPar = Create_AssetsWriterToMemory(newAssetBuffer, (size_t)newByteSize);
						__checkoutofmemory(writerPar==NULL);

						newByteSize = pBase->Write(AssetsWriterToMemory, writerPar, 0);
						AssetsReplacer *pReplacer = MakeAssetModifierFromMemory(pAsset->GetFileID(), pAsset->GetPathID(),
							pAsset->GetClassID(), pAsset->GetMonoClassID(),
							newAssetBuffer, newByteSize, TextAsset_FreeMemoryResource);
						__checkoutofmemory(pReplacer==NULL);

						pInterface->AddReplacer(pReplacer);

						Free_AssetsWriterToMemory(writerPar);
					}
					else
						MessageBox(NULL, TEXT("Unknown TextAsset format!"), TEXT("Error"), 16);
				}
				else
					MessageBox(NULL, TEXT("Unable to load the TextAsset!"), TEXT("Error"), 16);
			}
			else
				MessageBox(NULL, TEXT("TextAsset format not found!"), TEXT("Error"), 16);
			free(pFileBuffer);
			fclose(pSourceFile);
		}
		pInterface->FreeCOMFilePathBuf(&sourceFileName);
	}
}

void ExportToTXT(HWND hParentWnd, IPluginInterface *pInterface, IAssetInterface **ppAssets, size_t assetCount)
{
	char *outFolderPath = NULL; size_t outFolderPathLen = 0;
	char **prevAssetNames = NULL;
	if (assetCount > 1)
	{
		prevAssetNames = (char**)malloc(sizeof(char*) * (assetCount - 1));
		if (prevAssetNames == NULL)
		{
			MessageBox(NULL, TEXT("Out of memory!"), TEXT("ERROR"), 16);
			return;
		}

		WCHAR *outFolderPathW;
		if (!pInterface->ShowFolderSelectDialog(hParentWnd, &outFolderPathW, L"Select an output path"))
			return;
		size_t outFolderPathWLen = wcslen(outFolderPathW);
		outFolderPathLen = (size_t)WideCharToMultiByte(CP_UTF8, 0, outFolderPathW, (int)outFolderPathWLen, NULL, 0, NULL, NULL);
		outFolderPath = (char*)malloc((outFolderPathLen + 1) * sizeof(char));
		if (outFolderPath == NULL)
		{
			pInterface->FreeCOMFilePathBuf(&outFolderPathW);
			MessageBox(NULL, TEXT("Out of memory!"), TEXT("ERROR"), 16);
			return;
		}
		WideCharToMultiByte(CP_UTF8, 0, outFolderPathW, (int)outFolderPathWLen, outFolderPath, (int)outFolderPathLen, NULL, NULL);
		outFolderPath[outFolderPathLen] = 0;
		pInterface->FreeCOMFilePathBuf(&outFolderPathW);
	}
	for (size_t i = 0; i < assetCount; i++)
	{
		IAssetInterface *pAsset = ppAssets[i];
		AssetTypeTemplateField templateBase;
		if (pInterface->MakeTemplateField(pAsset->GetAssetsFile(), 0x31, &templateBase))
		{
			//make m_Script a TypelessData array because it might contain binary data (even though it's a string)
			for (DWORD i = 0; i < templateBase.childrenCount; i++)
			{
				if (!strcmp(templateBase.children[i].name, "m_Script"))
				{
					templateBase.children[i].type = "_string";
					templateBase.children[i].valueType = ValueType_None;
					for (DWORD k = 0; k < templateBase.children[i].childrenCount; k++)
					{
						if (!strcmp(templateBase.children[i].children[k].name, "Array"))
						{
							templateBase.children[i].children[k].type = "TypelessData";
							break;
						}
					}
					break;
				}
			}
			AssetsFileReader reader;
			LPARAM readerPar;
			unsigned __int64 filePos;
			unsigned __int64 fileSize = pAsset->GetFileReader(reader, readerPar, filePos);
			AssetTypeTemplateField *pTemplateBase = &templateBase;
			AssetTypeInstance instance(1, &pTemplateBase, reader, readerPar, pAsset->GetAssetsFile()->header.endianness, filePos);
			pAsset->FreeFileReader(reader, readerPar);
			AssetTypeValueField *pBase = instance.GetBaseField();
			//templateBase.MakeValue(reader, readerPar, (QWORD)filePos, &pBase);
		
			if (pBase)
			{
				AssetTypeValueField *nameField = pBase->Get("m_Name");
				AssetTypeValueField *scriptField = pBase->Get("m_Script");
				AssetTypeValueField *dataArrayField = scriptField->Get("Array");
				AssetTypeValueField *pathNameField = pBase->Get("m_PathName");
				if (!nameField->IsDummy() && !scriptField->IsDummy() && !pathNameField->IsDummy() && !dataArrayField->IsDummy())
				{
					char *name = nameField->GetValue()->AsString();
					
					WCHAR *dlgTargetFileName = NULL;
					char *fileName = name;
					if (outFolderPath == NULL)
					{
#ifdef _UNICODE
						WCHAR *exportSuggestion = pInterface->MakeExportFileNameW(
#else
						char *exportSuggestion = pInterface->MakeExportFileName(
#endif
							outFolderPath, fileName,
							pAsset->GetFileID(), pAsset->GetPathID(),
							prevAssetNames,
							i, assetCount,
							".txt");
						WCHAR *filePath;
						if (SUCCEEDED(pInterface->ShowFileSaveDialog(hParentWnd, &filePath, L"*.txt|Text file:", exportSuggestion)))
						{
							size_t outFilePathLen = wcslen(filePath);
							if ((outFilePathLen < 4) || (_wcsicmp(&filePath[outFilePathLen-4], L".txt")))
							{
								dlgTargetFileName = (WCHAR*)malloc((outFilePathLen + 5) * sizeof(WCHAR));
								__checkoutofmemory(dlgTargetFileName==NULL);
								memcpy(dlgTargetFileName, filePath, outFilePathLen * sizeof(WCHAR));
								memcpy(&dlgTargetFileName[outFilePathLen], L".txt", 5 * sizeof(WCHAR));
							}
							else
							{
								dlgTargetFileName = (WCHAR*)malloc((outFilePathLen + 1) * sizeof(WCHAR));
								__checkoutofmemory(dlgTargetFileName==NULL);
								memcpy(dlgTargetFileName, filePath, (outFilePathLen+1) * sizeof(WCHAR));
							}
							pInterface->FreeCOMFilePathBuf(&filePath);
						}
						pInterface->MemFree(exportSuggestion);
					}
					else
					{
						char *cOutFilePath = pInterface->MakeExportFileName(
							outFolderPath, fileName,
							pAsset->GetFileID(), pAsset->GetPathID(),
							prevAssetNames,
							i, assetCount,
							".txt");
						if ((i+1) < assetCount)
						{
							size_t fileNameLen = strlen(fileName);
							prevAssetNames[i] = (char*)malloc((fileNameLen + 1) * sizeof(char));
							if (prevAssetNames[i] == NULL)
								MessageBox(NULL, TEXT("Out of memory!"), TEXT("ERROR"), 16);
							else
							{
								memcpy(prevAssetNames[i], fileName, (fileNameLen + 1) * sizeof(char));
							}
						}
						if (cOutFilePath == NULL)
							MessageBox(NULL, TEXT("Out of memory!"), TEXT("ERROR"), 16);
						else
						{
							int filePathALen = (int)strlen(cOutFilePath);
							int filePathWLen = MultiByteToWideChar(CP_UTF8, 0, cOutFilePath, (int)filePathALen, NULL, 0);
							dlgTargetFileName = (WCHAR*)malloc((filePathWLen + 1) * sizeof(wchar_t));
							if (dlgTargetFileName != NULL)
							{
								MultiByteToWideChar(CP_UTF8, 0, cOutFilePath, (int)filePathALen, dlgTargetFileName, filePathWLen);
								dlgTargetFileName[filePathWLen] = 0;
							}
							else
								MessageBox(NULL, TEXT("Out of memory!"), TEXT("ERROR"), 16);
							pInterface->MemFree(cOutFilePath);
							//free(cOutFilePath);
						}
					}

					if (dlgTargetFileName != NULL)
					{
						FILE *pOutFile = NULL;
						_wfopen_s(&pOutFile, dlgTargetFileName, L"wb");
						if (pOutFile != NULL)
						{
							AssetTypeByteArray *pByteArray = dataArrayField->GetValue()->AsByteArray();
							fwrite(pByteArray->data, pByteArray->size, 1, pOutFile);
							fclose(pOutFile);
						}

						free(dlgTargetFileName);
					}
				}
				else
				{
					MessageBox(NULL, TEXT("Unknown TextAsset format!"), TEXT("Error"), 16);
					break;
				}
			}
			else
				MessageBox(NULL, TEXT("Unable to load the TextAsset!"), TEXT("Error"), 16);
		}
		else
		{
			MessageBox(NULL, TEXT("TextAsset format not found!"), TEXT("Error"), 16);
			break;
		}
	}
	if (outFolderPath)
		free(outFolderPath);
}

BYTE PluginInfoBuffer[sizeof(PluginInfo)+2*sizeof(PluginAssetOption)];
PluginInfo *GetAssetBundlePluginInfo()
{
	PluginInfo *pPluginInfo = (PluginInfo*)PluginInfoBuffer;
	strcpy(pPluginInfo->name, "TextAsset");
	pPluginInfo->optionCount = 2;

	pPluginInfo->options[0].action = PluginAction_EXPORT_Batch;
	strcpy(pPluginInfo->options[0].desc, "Export to .txt");
	pPluginInfo->options[0].unityClassID = 0x31;
	pPluginInfo->options[0].callback = ExportToTXT;

	pPluginInfo->options[1].action = PluginAction_IMPORT;
	strcpy(pPluginInfo->options[1].desc, "Import from .txt");
	pPluginInfo->options[1].unityClassID = 0x31;
	pPluginInfo->options[1].callback = ImportFromTXT;
	return pPluginInfo;
}