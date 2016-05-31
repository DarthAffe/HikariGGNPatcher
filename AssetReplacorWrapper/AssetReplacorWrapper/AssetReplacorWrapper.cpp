#include "stdafx.h"
#include <fstream>
#include "AssetsTools\AssetsReplacer.h"
#include "AssetsTools\AssetsFileReader.h"
#include "AssetsTools\AssetsFileTable.h"

// C++ the fking furuncle on the ass of programming -.- It works, I don't care about anything else ...

std::vector<AssetsReplacer*> replacors;
std::vector<FILE*> files;
FILE *pInputFile;
FILE *pOutputFile;
AssetsFile* assetsFile;
AssetsFileTable* assetsFileTable;

std::ifstream::pos_type filesize(const char* filename)
{
	std::ifstream in(filename, std::ifstream::ate | std::ifstream::binary);
	return in.tellg();
}

extern "C" __declspec(dllexport) void openAssetFile(const char* inputAssetPath)
{
	pInputFile = fopen(inputAssetPath, "rb");
	assetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)pInputFile);
	assetsFileTable = new AssetsFileTable(assetsFile);
}

extern "C" __declspec(dllexport) void replaceAsset(int pathId, const char* file)
{
	AssetFileInfoEx* assetsFileInfo = (*assetsFileTable).getAssetInfo(pathId);
	
	FILE *pReplaceFile = fopen(file, "rb");
	AssetsReplacer* assetReplacer = MakeAssetModifierFromFile(0, (*assetsFileInfo).index, (*assetsFileInfo).curFileType, (*assetsFileInfo).inheritedUnityClass,
		pReplaceFile, 0, (QWORD)filesize(file));
	
	files.push_back(pReplaceFile);
	replacors.push_back(assetReplacer);
}

extern "C" __declspec(dllexport) void saveAssetFile(const char* outputAssetPath)
{
	pOutputFile = fopen(outputAssetPath, "wb");
	(*assetsFile).Write(AssetsWriterToFile, (LPARAM)pOutputFile, 0, replacors, 0);
}

extern "C" __declspec(dllexport) void cleanup()
{
	fclose(pInputFile);
	fclose(pOutputFile);

	for (auto file = files.begin(); file != files.end(); ++file) { 
		fclose(*file);
    }

	replacors.clear();
	files.clear();
	delete assetsFile;
	delete assetsFileTable;
}
