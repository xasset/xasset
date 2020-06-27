#!/bin/bash

buildPath=$1
ipaName=$2
ipaType=$3
appName=$4
configuration=$5
osascript -e 'tell application "Terminal" to do script "cd '$buildPath' ; ./AutoBuildIPA.sh '$ipaName' '$ipaType' '$appName' '$buildPath' '$configuration'"'
