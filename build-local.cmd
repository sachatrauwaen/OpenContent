REM  %WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild OpenContent\OpenContent.csproj /p:Configuration=Release,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn720" /t:Rebuild >build-local-dnn720.log
REM  %WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild OpenContent.csproj /p:Configuration=Debug,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn722" /t:Rebuild >build-local-dnn722.log
"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild" OpenContent\OpenContent.csproj /p:Configuration=Debug,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn732" /t:Rebuild >build-local-dnn732.log
"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild" OpenContent\OpenContent.csproj /p:Configuration=Debug,Platform=AnyCPU,SolutionDir=Solution,ReferencePath="ref;ref\dnn740" /t:Rebuild >build-local-dnn740.log

"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild" OpenContentTests\OpenContentTests.csproj /p:Configuration=Release,Platform=AnyCPU,SolutionDir=Solution,ReferencePath=ref /t:Rebuild >build-local-test.log
pause