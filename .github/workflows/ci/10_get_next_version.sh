#!/bin/bash
buildNumber=$1
branch=$2

isDefaultBranch=false
if [ "$branch" == "master" ]; then
	isDefaultBranch=true
fi

latestVersion=$(git tag -l 'v*' --merged $branch | sort -V | tail -1)
if [ -z "$latestVersion" ]; then
	latestVersion="v0.0.0"
fi

latestVersion=${latestVersion:1}
latestVersionSplitted=(${latestVersion//./ })
latestMajor=${latestVersionSplitted[0]}
latestMinor=${latestVersionSplitted[1]}
# Remove suffix
latestPatch=${latestVersionSplitted[2]/-*/}
echo "Latest version: $latestVersion (major: $latestMajor, minor: $latestMinor, patch: $latestPatch)"

version=($(cat version))
versionSplitted=(${version//./ })
major=${versionSplitted[0]}
minor=${versionSplitted[1]}
echo "Proposed version: $version"
echo "Branch: $branch"
echo "Default branch: $isDefaultBranch"
echo "Build: $buildNumber"

echo "*** Version Calculation ***"

# Skip bumping patch if latest version was a pre-release and we're on the default branch
if [[ "${latestVersion}" =~ "-" && $isDefaultBranch == true ]]; then 
	patch=$latestPatch
else
	patch=$(($latestPatch + 1))
fi

if [ "$latestMajor" != "$major" ]; then
	if (( $major != $((latestMajor+1)) )); then
		echo "Major version identity $major can only be incremented by one in regards to previous major $latestMajor"
		exit 1	
	fi
	if (( $minor != "0" )); then
		echo "When major is bumped minor needs to be reset to 0"
		exit 1
	fi
	echo "Major version has been changed, setting patch to 0"
	patch=0    
elif (( $latestMinor != $minor )); then
	echo "Minor has been bumped."
	if (( $minor != $((latestMinor+1)) )); then
		echo "Minor version identity $minor can only be incremented by one in regards to previous minor $latestMinor"
		exit 1	
	fi
	echo "Minor version has been changed, setting patch to 0"
	patch=0
fi

versionSuffix=""
if [ "$isDefaultBranch" != true ]; then
	echo "This is a prerelease"
	versionSuffix="-prerelease$buildNumber"
fi

nextVersion="$version.$patch$versionSuffix"
echo "Next version: $nextVersion"
echo "::set-output name=version::$nextVersion"