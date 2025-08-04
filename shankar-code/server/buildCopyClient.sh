#! /bin/bash 

cd ../client || exit
ng build 
cd ../server || exit
cp -R ../client/dist/client/*  ./wwwroot
