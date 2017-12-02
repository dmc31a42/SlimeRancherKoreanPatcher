// 기본 DLL 파일입니다.

#include "stdafx.h"
#include <vector>

#include "SlimeRancherKoreanPatcherManagedCpp.h"

using namespace System::Runtime::InteropServices;

namespace SlimeRancherKoreanPatcherManagedCpp {
	ManagedPatcher::ManagedPatcher(String^ gameFolderPath, String^ currentDirectory)
	{
		IntPtr pDataPtr1 = Marshal::StringToHGlobalAnsi(gameFolderPath);
		char* charPtrGameFolderPath = (char*)pDataPtr1.ToPointer();
		IntPtr pDataPtr2 = Marshal::StringToHGlobalAnsi(currentDirectory);
		char* charPtrCurrentDirectory = (char*)pDataPtr2.ToPointer();
		m_pUnmanagedPatcher = new unmanagedPatcher(charPtrGameFolderPath, charPtrCurrentDirectory);
	}

	ManagedPatcher::~ManagedPatcher()
	{
		if (m_pUnmanagedPatcher)
		{
			delete m_pUnmanagedPatcher;
			m_pUnmanagedPatcher = NULL;
		}
	}

	array<AssetInfo^>^ ManagedPatcher::GetAssetInfos()
	{
		std::vector<UnmanagedAssetInfo> unmanagedAssetInfos = m_pUnmanagedPatcher->GetAssetInfos();

		array<AssetInfo^>^ managedAssetInfos = gcnew array<AssetInfo^>(unmanagedAssetInfos.size());

		for (int i = 0; i < managedAssetInfos->Length; i++)
		{
			managedAssetInfos[i] = gcnew AssetInfo;
			//managedAssetInfos[i]->name = msclr::interop::marshal_as<System::String^>(unmanagedAssetInfos[i].name.c_str());
			managedAssetInfos[i]->name = gcnew String(unmanagedAssetInfos[i].name.c_str());
			managedAssetInfos[i]->offset = unmanagedAssetInfos[i].offset;
			managedAssetInfos[i]->pathID = unmanagedAssetInfos[i].pathID;
			managedAssetInfos[i]->size = unmanagedAssetInfos[i].size;
		}
		return managedAssetInfos;
	}
	void ManagedPatcher::MakeModdedAssets()
	{
		m_pUnmanagedPatcher->MakeModdedAssets();
	}

}
