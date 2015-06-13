msbuild OpenContent.csproj /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:Configuration=Release,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn720" /t:Rebuild

msbuild OpenContent.csproj /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:Configuration=Debug,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn722" /t:Rebuild
msbuild OpenContent.csproj /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:Configuration=Debug,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn732" /t:Rebuild
msbuild OpenContent.csproj /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:Configuration=Debug,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn740" /t:Rebuild

msbuild tests/OpenContentTests.csproj /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:Configuration=Release,Platform=AnyCPU,SolutionDir=Solution,ReferencePath=ref
