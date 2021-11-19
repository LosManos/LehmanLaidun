# Readme for LehmanLaidun.Console

## Run console

If you are in the bin folder: 
`dotnet LehmanLaidun.Console.dll ..\..\..\Data\MyDrive ..\..\..\Data\TheirDrive`

If you are in the project folder: 
`dotnet .\bin\Debug\net6.0\LehmanLaidun.Console.dll --mypath .\Data\CompareTwoFolders\MyDrive\ --theirpath .\Data\CompareTwoFolders\TheirDrive\`

To output a tree structure as XML:
`dotnet .\bin\Debug\net6.0\LehmanLaidun.Console.dll --mypath .\Data\MyDrive --ox`

T output a tree strcure as XML with a plugin file:
`dotnet .\Source\LehmanLaidun.Console\bin\Debug\net6.0\LehmanLaidun.Console.dll --mypath .\Source\LehmanLaidun.Console\Data\MyDrive\ --pluginfiles .\Source\Plugins\ImagePlugin\ImagePlugin.dll --verbose`

## Images
The randomised images are from Lorem picsum https://picsum.photos/
Get new by https://picsum.photos/200/200/?random
