#!/bin/bash

workflow() {
	git_hash=$(git rev-parse --short "${GITHUB_SHA}")
	git_branch=${GITHUB_REF##*/}

	echo ${GITHUB_SHA}


	echo "git-beta-${GIT_HASH}" > AmongUsCapture/version.txt
}

normal() {
	if [[ -f "AmongUsCapture/version.txt" ]]; then
		echo "Already found a version.txt"
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
