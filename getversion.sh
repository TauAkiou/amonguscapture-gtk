#!/bin/bash

workflow() {
    GITHUB_SHA=3dca4d2b1cbc85787d02204a3a7f503a77b9b66c
	git_hash=$(git rev-parse --short "$GITHUB_SHA" )
	git_branch=${GITHUB_REF##*/}

	echo ${git_hash}

	echo "git-beta-${git_hash}" > AmongUsCapture/version.txt
}

normal() {
	if [[ -f "AmongUsCapture/version.txt" ]]; then
		echo "Already found a version.txt"
		return 0
	elif [[ -z "$GITHUB_SHA" ]]; then
		echo "Already in the github build environment.
		return 0
	fi

	git_line=$(git describe --tag)
	echo ${git_line} > AmongUsCapture/version.txt
}

if [[ $1 == "workflow" ]]; then
	workflow
elif [[ $1 == "normal" ]]; then
	normal
fi

exit 0
