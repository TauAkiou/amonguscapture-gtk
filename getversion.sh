#!/bin/bash

workflow() {
	git_hash=$(git rev-parse --short "$GITHUB_SHA" )
	git_branch=${GITHUB_REF##*/}

	echo ${git_hash}

	echo "git-beta-${git_hash}" > AmongUsCapture/version.txt
}

normal() {
	if [[ -n "$GITHUB_SHA" ]]; then
		echo "Already in the github build environment."
		return 0
	fi

	git_line=$(git describe --tag)
	
	echo "$git_line" > "version.txt"
}

pwd

if [[ $1 == "workflow" ]]; then
	workflow
elif [[ $1 == "normal" ]]; then
	normal
fi

exit 0
