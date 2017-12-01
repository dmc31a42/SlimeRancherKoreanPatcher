#pragma once
#include <string>
#include <vector>
#include <map>

#include "AssetsTools\defines.h"
#include "AssetsTools\AssetsFileFormat.h"
#include "AssetsTools\AssetsFileReader.h"
#include "AssetsTools\AssetsFileTable.h"
#include "AssetsTools\AssetsBundleFileFormat.h"
#include "AssetsTools\ClassDatabaseFile.h"
using namespace std;

class UnmanagedAssetInfo
{
public:
	int pathID;
	string name;
	int offset;
	int size;
	UnmanagedAssetInfo();
	UnmanagedAssetInfo(int pathID, string name, int offset, int size);
};

class unmanagedPatcher
{
private:
	string _gameFolderPath;
	string _currentDirectory;
	string resAssetsFileName;
	string sharedAssetsFileName;
	string classDatabaseFileName;
	AssetsFile *resAssetsFile;
	AssetsFile *sharedAssetsFile;
	ClassDatabaseFile *classDatabaseFile;
	FILE *pResAssetsFile;
	FILE *psharedAssetsFile;
	FILE *pClassDatabaseFile;
	AssetsFileTable *resAssetsFileTable;
	AssetsFileTable *sharedAssetsFileTable;
	map<int, unsigned int> findByClassID;

	ofstream logOfstream;

	vector<UnmanagedAssetInfo> assetInfos;
public:
	unmanagedPatcher(string gameFolderPath, string currentDirectory);
	~unmanagedPatcher();

	void FindInformation();
	vector<UnmanagedAssetInfo> GetAssetInfos();
	void MakeModdedAssets();
};

