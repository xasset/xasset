#! /bin/bash

target_name=Unity-iPhone
configuration_name=$5
ipaName=$1
ipaType=$2
appName=$3
buildPath=$4
project_name=${buildPath}/Unity-iPhone.xcodeproj
#build
xcodebuild -project ${project_name} -target ${target_name} -configuration ${configuration_name} build

buildDayTime=`Date +%Y-%m-%d-%H-%M`

buildIPARootFolder=~/Desktop/tankii-ipa-build
buildIPATypeFolder=${buildIPARootFolder}/${ipaType}
buildIPATimeFolder=${buildIPATypeFolder}/${buildDayTime}
buildIPAPayloadFolder=${buildIPATimeFolder}/Payload

buildAppFile=./build/${configuration_name}-iphoneos/${appName}.app


if [ ! -d "$buildIPARootFolder" ];then
    mkdir "$buildIPARootFolder"
fi

if [ ! -d "$buildIPATypeFolder" ];then
    mkdir "$buildIPATypeFolder"
fi

if [ -d "$buildIPATimeFolder" ];then
    rm -rf "$buildIPATimeFolder"
fi

mkdir "$buildIPATimeFolder"

if [ ! -d "$buildIPAPayloadFolder" ];then
    mkdir "$buildIPAPayloadFolder"
fi

cp -r ${buildAppFile} ${buildIPAPayloadFolder}/${appName}.app
cd ${buildIPATimeFolder}
zip -r ${ipaName}.ipa Payload

if [ -d "$buildIPAPayloadFolder" ];then
    rm -rf "$buildIPAPayloadFolder"
fi
