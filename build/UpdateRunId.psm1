Function UpdateRunId {
    Param (
	$AccessToken
    )
    $link = "https://github-actions[bot]:"+$AccessToken+"@github.com/AzureAms/ToastNotification.Uno"
    git init | Out-Host
    git remote add origin $link | Out-Host
    git fetch origin version-number | Out-Host
    git checkout -b origin/version-number | Out-Host
    git switch version-number | Out-Host
    $Content = Get-Content -Path "BUILD_RUN.txt"
    $id = $Content -as [int]
    $id = $id + 1;
    $Content = $id -as [string]
    Set-Content -Path "BUILD_RUN.txt" -Value $Content
    git config --local user.email "41898282+github-actions[bot]@users.noreply.github.com" | Out-Host
    git config --local user.name "github-actions[bot]" | Out-Host
    git add "BUILD_RUN.txt"
    git commit --amend --no-edit --author="github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>" | Out-Host
    git push -f origin version-number | Out-Host
    return $Content
}