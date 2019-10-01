#version#
$ScriptVersion = 1
#version#

$githubUri = "https://raw.githubusercontent.com/leechdraw/ytm/dev/scripts/install.ps1"
# get cur dir
# check if newer version of this script exists
# find if install info exists
# if not - download last version of archive
# if exists - check date of install version and download latest if need

$workDir = Resolve-Path $PSScriptRoot

# functions
function ExtractVersionFromScript {
    param (
        [string]$content
    )
    $mtch = $content -match '\$ScriptVersion\ \=\ (\d+)'
    if($mtch)
    {
        return [int]$Matches[1]
    } 
    return 0
}

function UpdateScriptVersionIfNeed {
    $repoScript = (Invoke-WebRequest -Uri $githubUri).Content
    $repoVersionOfScript = ExtractVersionFromScript -content $repoScript
    if($repoVersionOfScript -gt $ScriptVersion)
    {
        $newVersionFile = Join-Path $workDir "new_install.ps1"
        if(Test-Path $newVersionFile)
        {
            $tmpFileContent = Get-Content $newVersionFile
            $tmpVersion = ExtractVersionFromScript $tmpFileContent
            if($tmpVersion -eq $repoVersionOfScript)
            {
                Write-Host "Please, rename file [new_install.ps1] into [install.ps1]"
                exit 0
            }
            Remove-Item $newVersionFile -Force -ErrorAction Stop            
        }
        $repoScript | Set-Content -Path $newVersionFile -ErrorAction Stop -Force -Encoding utf8
        Write-Host "Install script was updated from Version [$ScriptVersion`] to [$repoVersionOfScript]."
        Write-Host "Please, rename file [new_install.ps1] into [install.ps1]"
        exit 0
    }
    Write-Host "Install script has latest version!"
}

function CheckFolderStructure 
{
    $folders = "mp3", "video", "ffmpeg"
    $folders = $folders | ForEach-Object{Join-Path $workDir $_}
    $folders | ForEach-Object {
        if(!(Test-Path $_))
        {
            New-Item -ItemType Directory -Path $_ -Force
        }
    }
}
# functions

# main
UpdateScriptVersionIfNeed
CheckFolderStructure

