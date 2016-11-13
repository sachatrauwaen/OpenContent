Import-Module -Name "$PSScriptRoot\Invoke-MsBuild.psm1"

$fileTypes = ".scss",".jpg",".jpeg",".png",".gif",".js",".css",".cshtml",".html",".ascx",".json",".aspx",".resx"

function Deploy()
{
	$projectFile = "OpenContent\OpenContent.csproj"
    $buildSucceeded = Invoke-MsBuild -Path $projectFile -MsBuildParameters "/t:AfterBuild /p:Platform=AnyCPU" -BuildLogDirectoryPath $PSScriptRoot
    if ($buildSucceeded)
    {
        Write-Host " -> Project $project deploy completed successfully." 
    }
    else
    {
        Write-Host " -> Project $project deploy failed."
    }
}

Write-Host (Get-Date -Format "HH:mm") "Auto Deployment is running, press STRG+C to stop ..."
Write-Host 

Write-Host (Get-Date -Format "HH:mm") "Start initial project deploy ..."
Deploy
Write-Host

# Set up watcher
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = "$PSScriptRoot\OpenContent"
$watcher.Filter = "*.*"
$watcher.IncludeSubdirectories = $true
$watcher.EnableRaisingEvents = $true
$watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite -bor [System.IO.NotifyFilters]::FileName

# Watching ...
while($true) {

	# Watch for changes
    $result = $watcher.WaitForChanged([System.IO.WatcherChangeTypes]::Changed -bor [System.IO.WatcherChangeTypes]::Renamed -bor [System.IO.WatcherChangeTypes]::Created, 1000);
	if($result.TimedOut) {
		continue;
	}

    # Check file type
    $fileType = [System.IO.Path]::GetExtension($result.Name)
    if (!$fileTypes.Contains($fileType)) {
        continue;
    }

    # Trigger module deploy
	Write-Host (Get-Date -Format "HH:mm") "Change in"$result.Name"trigger project deploy ..."
	Deploy
	Write-Host

}