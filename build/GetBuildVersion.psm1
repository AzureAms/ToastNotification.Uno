Function GetBuildVersion {
    Param (
        [string]$VersionString,
        [string]$BuildNumber,
        [string]$BuildId
    )

    # Pull request:
    if ($VersionString.StartsWith("refs/pull/") -and $VersionString.EndsWith("/merge")) {
        $VersionString = $VersionString.Substring("refs/pull/".Length)
        $VersionString = $VersionString.Substring(0, $VersionString.IndexOf("/merge"))
        return "0.0.0-pullrequest-"+$VersionString+"-"+$BuildId
    }

    # Process through regex
    $VersionString -match "(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?(\-(?<pre>[0-9A-Za-z\-\.]+))?(\+(?<build>\d+))?" | Out-Null

    # Should be preview
    if ($matches -eq $null) {
        return "0.0.0-preview-"+$BuildNumber+"-"+$BuildId
    }

    # Extract the build metadata
    $BuildRevision = [uint64]$matches['build']
    # Extract the pre-release tag
    $PreReleaseTag = [string]$matches['pre']
    # Extract the patch
    $Patch = [uint64]$matches['patch']
    # Extract the minor
    $Minor = [uint64]$matches['minor']
    # Extract the major
    $Major = [uint64]$matches['major']

    $Version = [string]$Major + '.' + [string]$Minor + '.' + [string]$Patch;
    if ($PreReleaseTag -ne [string]::Empty) {
        $Version = $Version + '-' + $PreReleaseTag
    }

    if ($BuildRevision -ne 0) {
        $Version = $Version + '.' + [string]$BuildRevision
    }

    return $Version
}