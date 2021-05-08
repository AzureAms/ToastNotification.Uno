Function TransformNuspec {
    Param (
        [string]$BuildVersion
    )

    $files = Get-ChildItem -Filter *.nuspec
    for ($i=0; $i -lt $files.Count; $i++) {
        $content = Get-Content $files[$i].FullName
        $content = $content.Replace("BUILD_VERSION", $BuildVersion)
        $baseName = $files[$i].BaseName
        Set-Content -Path $baseName"."$BuildVersion".nuspec" -Value $content
        nuget pack $baseName"."$BuildVersion".nuspec"
    }

}