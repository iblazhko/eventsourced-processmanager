#!/usr/bin/env pwsh

Param(
    [ValidateNotNullOrEmpty()]
    [string]$Target = "FullBuild",

    [ValidateNotNullOrEmpty()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$ScaleOutApplication,

    [string]$DotnetVerbosity = "minimal"
)

#######################################################################
# SHARED VARIABLES

$repositoryDir = (Get-Item $PSScriptRoot).FullName
$srcDir = Join-Path $repositoryDir "src"
$dotnetSolutionFile = Get-ChildItem -Path $srcDir -Filter "*.sln" | Select-Object -First 1
$buildCoreVersionFile = Join-Path $repositoryDir "version.yaml"
$buildCoreVersion = $(Get-Content "$buildCoreVersionFile").Substring("version:".Length).Trim()
$buildVersion = "$buildCoreVersion$VersionSuffix"

$dockerComposeProject = "espm"

$normalizedTarget = $Target.Replace(".", "_")

# This build system expects following solution layout:
# solution_root/
#   build.ps1                  -- PowerShell build CLI
#   docker-compose.yaml
#   src/
#     Project1/
#       Project1.csproj        -- project base filename matches directory name
#     Project1.Tests/
#       Project1.Tests.csproj  -- tests projects are xUnit-based; project name must have suffix '.Tests'
#     Project2/
#       Project2.fsproj
#       imagename.Dockerfile   -- if the `*.Dockerfile` is present, we'll build Docker image `imagename`
#     Project2.Tests/
#       Project2.Tests.fsproj
#     SolutionName.sln         -- only one '.sln' file in 'src'
#   version.yaml               -- core part of solution SemVer

#######################################################################
# LOGGING

Function LogInfo {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Green $Message
}

Function LogWarning {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "*** $Message"
}

Function LogError {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Red "*** $Message"
}

Function LogStep {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "--- STEP: $Message"
}

Function LogTarget {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Green "--- TARGET: $Message"
}

Function LogCmd {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "--- $Message"
}

#######################################################################
# STEPS

Function PreludeStep_ValidateDotNetCli {
    LogStep "Prelude: .NET CLI"
    $dotnetCmd = Get-Command "dotnet" -ErrorAction Ignore
    if (-Not $dotnetCmd) {
        LogError ".NET CLI 'dotnet' command not found."
        Exit 1
    }
}

Function PreludeStep_ValidateDockerCli {
    LogStep "Prelude: Docker CLI"
    $dockerCmd = Get-Command "docker" -ErrorAction Ignore
    if (-Not $dockerCmd) {
        LogError "Docker CLI 'docker' command not found."
        Exit 1
    }
}

Function Step_PruneBuild {
    LogStep "PruneBuild"

    $pruneDir = $repositoryDir
    LogWarning "Pruning $pruneDir build artifacts"

    # Prune nested directories
    'bin', 'obj', 'publish' | ForEach-Object {
        Get-ChildItem -Path $pruneDir -Filter $_ -Directory -Recurse | ForEach-Object { $_.Delete($true) }
    }

    # Prune nested files
    '*.trx', '*.fsx.lock', '*.Tests_*.xml' | ForEach-Object {
        Get-ChildItem -Path $pruneDir -Filter $_ -File -Recurse | ForEach-Object { $_.Delete() }
    }

    # Prune top-level items
    '.fable', '.ionide', 'build', 'project', 'target', 'node_modules' | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item -Path $(Join-Path $pruneDir $_) -Recurse -Force
        }
    }
}

Function Step_PruneDocker {
    LogStep "PruneDocker"

    $pruneDir = $repositoryDir
    LogWarning "Pruning $pruneDir Docker artifacts"

    LogCmd "docker container prune -f"
    & docker container prune -f | Out-Null

    # Note: Docker image names below are hardcoded. Consider taking it from build.yaml
    "${dockerComposeProject}_client:latest", "${dockerComposeProject}_server:latest" | ForEach-Object {
        if ($(docker image ls $_ -q | Out-String) -ne "") {
            LogCmd "docker rmi $_ -f"
            & docker rmi $_ -f | Out-Null
        }
    }

    'image', 'volume', 'network' | ForEach-Object {
        LogCmd "docker $_ prune -f"
        & docker $_ prune -f | Out-Null
    }
}

Function Step_DotnetClean {
    LogStep "dotnet clean $dotnetSolutionFile --verbosity $DotnetVerbosity"
    & dotnet clean "$dotnetSolutionFile" --verbosity $DotnetVerbosity
}

Function Step_DotnetRestore {
    LogStep "dotnet restore $dotnetSolutionFile --verbosity $DotnetVerbosity"
    & dotnet restore "$dotnetSolutionFile" --verbosity $DotnetVerbosity
}

Function Step_DotnetBuild {
    LogStep "dotnet build $dotnetSolutionFile --no-restore --configuration $Configuration --verbosity $DotnetVerbosity /p:Version=$buildVersion"
    & dotnet build "$dotnetSolutionFile" --no-restore --configuration $Configuration --verbosity $DotnetVerbosity /p:Version=$buildVersion
}

Function Step_DotnetPublish {
    Param([ValidateNotNullOrEmpty()] [string]$ProjectFile, [ValidateNotNullOrEmpty()] [string]$PublishOutput)
    LogStep "dotnet publish $ProjectFile --output $PublishOutput --configuration $Configuration --verbosity $DotnetVerbosity /p:Version=$buildVersion"
    & dotnet publish "$ProjectFile" --output "$PublishOutput" --configuration $Configuration --verbosity $DotnetVerbosity /p:Version=$buildVersion
}

Function Step_DotnetTest {
    Param([ValidateNotNullOrEmpty()] [string]$ProjectFile)
    LogStep "dotnet test $ProjectFile --no-build --configuration $Configuration --test-adapter-path:. --logger:xunit"
    & dotnet test "$ProjectFile" --no-build --configuration $Configuration --test-adapter-path:. --logger:xunit
}

Function Get_DockerComposeAppFile {
    if ($ScaleOutApplication) {
        "docker-compose.app-scaled.yaml"
    }
    else {
        "docker-compose.app.yaml"
    }
}

Function Step_DockerComposeStart {
    $dockerComposeInfraFile = "docker-compose.infra.yaml"
    $dockerComposeAppFile = Get_DockerComposeAppFile
    LogStep "docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeAppFile up --build --abort-on-container-exit"
    & docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeAppFile up --build --abort-on-container-exit
}

Function Step_DockerComposeStartDetached {
    $dockerComposeInfraFile = "docker-compose.infra.yaml"
    $dockerComposeAppFile = Get_DockerComposeAppFile
    LogStep "docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeAppFile up --build --abort-on-container-exit -d"
    & docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeAppFile up --build --abort-on-container-exit -d
}

Function Step_DockerComposeStop {
    $dockerComposeInfraFile = "docker-compose.infra.yaml"
    $dockerComposeAppFile = Get_DockerComposeAppFile
    LogStep "docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeAppFile down"
    & docker compose -p $dockerComposeProject -f $dockerComposeInfraFile -f $dockerComposeAppFile down
}


#######################################################################
# DEPENDENCIES TRACKING

$targetCalls = @{ }
Function DependsOn {
    Param([ValidateNotNullOrEmpty()] [string]$Target)
    $normalizedTarget = $Target.Replace(".", "_")

    if (-Not $targetCalls.ContainsKey($Target)) {
        Invoke-Expression "Target_$normalizedTarget"
        $targetCalls.Add($Target, $(Get-Date))
    }
}

#######################################################################
# PRELUDE TARGET
# Special target that is called automatically

Function Target_Prelude {
    LogTarget "Prelude"

    PreludeStep_ValidateDotNetCli
    PreludeStep_ValidateDockerCli
}

#######################################################################
# TARGETS

Function Target_Dotnet_Clean {
    LogTarget "DotNet.Clean"
    Step_DotnetClean
}

Function Target_Dotnet_Restore {
    LogTarget "DotNet.Restore"
    Step_DotnetRestore
}

Function Target_Dotnet_Build {
    DependsOn "Dotnet_Restore"

    LogTarget "DotNet.Build"
    Step_DotnetBuild
}

Function Target_Dotnet_Test {
    DependsOn "Dotnet.Build"

    LogTarget "DotNet.Test"
    $projects = Get-ChildItem -Path $srcDir -Filter "*.Tests.csproj" -Recurse -File
    Foreach ($projectFile in $projects) {
        Step_DotnetTest $projectFile
    }
}

Function Target_Dotnet_Publish {
    DependsOn "Dotnet.Build"

    LogTarget "DotNet.Publish"
    $dockerfiles = Get-ChildItem -Path $srcDir -Filter "*.Dockerfile" -Recurse -File
    Foreach ($dockerFile in $dockerfiles) {
        LogInfo "Dockerfile found: $dockerFile"
        $projectDirectory = $dockerFile.Directory
        $projectFile = Get-ChildItem -Path $projectDirectory -Filter "*.?sproj" | Select-Object -First 1
        $publishOutput = [System.IO.Path]::Combine($projectDirectory, "bin", "publish")
        Step_DotnetPublish $projectFile $publishOutput
    }
}

Function Target_DockerCompose_Start {
    DependsOn "Dotnet.Publish"

    LogTarget "DockerCompose.Start"
    try {
        Step_DockerComposeStart
    }
    finally {
        # ensure proper cleanup
        Step_DockerComposeStop
    }
}

Function Target_DockerCompose_StartDetached {
    DependsOn "Dotnet.Publish"

    LogTarget "DockerCompose.StartDetached"
    Step_DockerComposeStartDetached
}

Function Target_DockerCompose_Stop {
    LogTarget "DockerCompose.Stop"
    Step_DockerComposeStop
}

Function Target_FullBuild {
    DependsOn "Dotnet.Build"
    DependsOn "Dotnet.Test"
    DependsOn "Dotnet.Publish"

    LogTarget "FullBuild"
    LogInfo "DONE"
}

#######################################################################
# PRUNE TARGETS

if ($Target -eq "Prune") {
    Step_PruneBuild
    Exit 0
}

if ($Target -eq "Prune.Docker") {
    Step_PruneDocker
    Exit 0
}

#######################################################################
# MAIN ENTRY POINT

LogInfo "*** BUILD: $Target ($Configuration) in $repositoryDir"

DependsOn "Prelude"
Invoke-Expression "Target_$normalizedTarget"
